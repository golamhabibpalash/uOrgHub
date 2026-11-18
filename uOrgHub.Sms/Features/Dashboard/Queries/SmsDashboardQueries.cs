using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Models.Enums;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Dashboard.Queries;

/// <param name="TzOffsetMinutes">The viewer's UTC offset, so "today" is their today (e.g. 360 for Bangladesh).</param>
public record GetSmsDashboardQuery(int TzOffsetMinutes) : IQuery<SmsDashboardDto>;

/// <param name="From">First local calendar day included.</param>
/// <param name="To">Last local calendar day included.</param>
public record GetSmsAnalyticsQuery(DateTime From, DateTime To, SmsAnalyticsGranularity Granularity, int TzOffsetMinutes, Guid? GatewayId)
    : IQuery<SmsAnalyticsDto>;

internal static class SmsStats
{
    public record Row(SmsMessageStatus Status, int Count, int Segments, decimal Cost);

    public static async Task<SmsTotalsDto> TotalsAsync(IQueryable<SmsMessage> query, CancellationToken ct)
    {
        var rows = await query
            .GroupBy(m => m.Status)
            .Select(g => new Row(g.Key, g.Count(), g.Sum(m => m.Segments), g.Sum(m => m.Cost ?? 0)))
            .ToListAsync(ct);
        return Totals(rows);
    }

    public static SmsTotalsDto Totals(IEnumerable<Row> rows)
    {
        var list = rows.ToList();
        int Count(params SmsMessageStatus[] s) => list.Where(r => s.Contains(r.Status)).Sum(r => r.Count);
        var pending = Count(SmsMessageStatus.Queued, SmsMessageStatus.Sending);
        return new SmsTotalsDto
        {
            Sent = list.Sum(r => r.Count) - pending,
            Successful = Count(SmsMessageStatus.Submitted, SmsMessageStatus.Delivered),
            Delivered = Count(SmsMessageStatus.Delivered),
            Failed = Count(SmsMessageStatus.Failed),
            Undetermined = Count(SmsMessageStatus.Undetermined),
            Pending = pending,
            // Segments and cost count what went out — not queued work, not failures.
            Segments = list.Where(r => r.Status is not (SmsMessageStatus.Queued or SmsMessageStatus.Sending or SmsMessageStatus.Failed)).Sum(r => r.Segments),
            Cost = list.Where(r => r.Status is not (SmsMessageStatus.Queued or SmsMessageStatus.Sending or SmsMessageStatus.Failed)).Sum(r => r.Cost),
        };
    }
}

public class GetSmsDashboardQueryHandler(AppDbContext db, ISmsGatewayRuntime runtime) : IRequestHandler<GetSmsDashboardQuery, SmsDashboardDto>
{
    public async Task<SmsDashboardDto> Handle(GetSmsDashboardQuery request, CancellationToken ct)
    {
        var offset = TimeSpan.FromMinutes(Math.Clamp(request.TzOffsetMinutes, -14 * 60, 14 * 60));
        var todayStartUtc = (DateTime.UtcNow + offset).Date - offset;
        var last30StartUtc = todayStartUtc.AddDays(-29);

        var gateways = await db.Set<SmsGateway>().AsNoTracking().Where(g => !g.IsDeleted).ToListAsync(ct);
        var active = gateways.FirstOrDefault(g => g.IsDefault);
        var messages = db.Set<SmsMessage>().AsNoTracking().Where(m => !m.IsDeleted);

        return new SmsDashboardDto
        {
            ActiveGateway = active is null ? null : runtime.ToDto(active),
            GatewayCount = gateways.Count,
            AllTime = await SmsStats.TotalsAsync(messages, ct),
            Today = await SmsStats.TotalsAsync(messages.Where(m => m.CreatedAt >= todayStartUtc), ct),
            Last30Days = await SmsStats.TotalsAsync(messages.Where(m => m.CreatedAt >= last30StartUtc), ct),
        };
    }
}

/// <summary>
/// Trend series from the message records themselves. Grouped by local day in the database,
/// then rolled up to weeks (starting Monday) or months here; empty periods are filled in so the
/// chart's axis has no gaps.
/// </summary>
public class GetSmsAnalyticsQueryHandler(AppDbContext db) : IRequestHandler<GetSmsAnalyticsQuery, SmsAnalyticsDto>
{
    private const int MaxDays = 3 * 366;
    private const int MaxDailyBuckets = 400;

    public async Task<SmsAnalyticsDto> Handle(GetSmsAnalyticsQuery request, CancellationToken ct)
    {
        var fromDay = request.From.Date;
        var toDay = request.To.Date;
        if (toDay < fromDay) throw new AppException("Start date cannot be after end date.");
        var days = (toDay - fromDay).Days + 1;
        if (days > MaxDays) throw new AppException("Choose a range of at most three years.");
        if (request.Granularity == SmsAnalyticsGranularity.Day && days > MaxDailyBuckets)
            throw new AppException("That range is too long for a daily view — switch to weekly or monthly.");

        var offsetMinutes = Math.Clamp(request.TzOffsetMinutes, -14 * 60, 14 * 60);
        var offset = TimeSpan.FromMinutes(offsetMinutes);
        var fromUtc = fromDay - offset;
        var toUtc = toDay.AddDays(1) - offset;

        var query = db.Set<SmsMessage>().AsNoTracking()
            .Where(m => !m.IsDeleted && m.CreatedAt >= fromUtc && m.CreatedAt < toUtc);
        if (request.GatewayId is { } gatewayId) query = query.Where(m => m.GatewayId == gatewayId);

        var daily = await query
            .GroupBy(m => new { Day = m.CreatedAt.AddMinutes(offsetMinutes).Date, m.Status })
            .Select(g => new { g.Key.Day, g.Key.Status, Count = g.Count(), Segments = g.Sum(m => m.Segments), Cost = g.Sum(m => m.Cost ?? 0) })
            .ToListAsync(ct);

        var byBucket = daily
            .GroupBy(d => BucketStart(d.Day, request.Granularity))
            .ToDictionary(g => g.Key, g => SmsStats.Totals(g.Select(d => new SmsStats.Row(d.Status, d.Count, d.Segments, d.Cost))));

        var points = new List<SmsAnalyticsPointDto>();
        for (var bucket = BucketStart(fromDay, request.Granularity); bucket <= toDay; bucket = Next(bucket, request.Granularity))
        {
            var t = byBucket.GetValueOrDefault(bucket) ?? new SmsTotalsDto();
            points.Add(new SmsAnalyticsPointDto
            {
                Period = bucket.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Sent = t.Sent,
                Successful = t.Successful,
                Failed = t.Failed,
                Pending = t.Pending,
                Segments = t.Segments,
                Cost = t.Cost,
            });
        }

        var currency = await db.Set<SmsGateway>().AsNoTracking()
            .Where(g => !g.IsDeleted && (request.GatewayId == null ? g.IsDefault : g.Id == request.GatewayId))
            .Select(g => g.Currency).FirstOrDefaultAsync(ct);

        return new SmsAnalyticsDto
        {
            From = fromDay,
            To = toDay,
            Granularity = request.Granularity.ToString(),
            Totals = SmsStats.Totals(daily.Select(d => new SmsStats.Row(d.Status, d.Count, d.Segments, d.Cost))),
            Points = points,
            Currency = currency ?? "BDT",
        };
    }

    public static DateTime BucketStart(DateTime day, SmsAnalyticsGranularity g) => g switch
    {
        SmsAnalyticsGranularity.Week => day.Date.AddDays(-(((int)day.DayOfWeek + 6) % 7)),
        SmsAnalyticsGranularity.Month => new DateTime(day.Year, day.Month, 1),
        _ => day.Date,
    };

    private static DateTime Next(DateTime bucket, SmsAnalyticsGranularity g) => g switch
    {
        SmsAnalyticsGranularity.Week => bucket.AddDays(7),
        SmsAnalyticsGranularity.Month => bucket.AddMonths(1),
        _ => bucket.AddDays(1),
    };
}
