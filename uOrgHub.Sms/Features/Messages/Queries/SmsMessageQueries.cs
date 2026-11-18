using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.DTOs.Validators;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Mappings;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Models.Enums;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Messages.Queries;

/// <param name="From">Inclusive, UTC.</param>
/// <param name="To">Exclusive, UTC.</param>
public record GetSmsMessagesQuery(
    PaginationRequest Request, DateTime? From, DateTime? To, Guid? GatewayId, SmsMessageStatus? Status)
    : IQuery<PagedResult<SmsMessageListItemDto>>;

public record GetSmsMessageByIdQuery(Guid Id) : IQuery<SmsMessageDetailDto>;
public record GetSmsSendRequestStatusQuery(Guid RequestId) : IQuery<SmsSendRequestStatusDto>;
public record GetSmsComposeInfoQuery(bool CanSendBulk) : IQuery<SmsComposeInfoDto>;

public class GetSmsMessagesQueryHandler(AppDbContext db) : IRequestHandler<GetSmsMessagesQuery, PagedResult<SmsMessageListItemDto>>
{
    private static readonly SmsMapper Mapper = new();

    public async Task<PagedResult<SmsMessageListItemDto>> Handle(GetSmsMessagesQuery request, CancellationToken ct)
    {
        var query = db.Set<SmsMessage>().AsNoTracking().Where(m => !m.IsDeleted);

        if (request.From is { } from) query = query.Where(m => m.CreatedAt >= from);
        if (request.To is { } to) query = query.Where(m => m.CreatedAt < to);
        if (request.GatewayId is { } gatewayId) query = query.Where(m => m.GatewayId == gatewayId);
        if (request.Status is { } status) query = query.Where(m => m.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            // A whole number in any written form (01711-…, +88 01711…) is searched in its stored
            // form; a partial one ("01711") already matches inside "8801711…" as a substring.
            var search = request.Request.Search.Trim();
            var term = SmsPhoneNumber.Normalize(search) ?? search;
            query = query.WhereSearch(term, m => m.Recipient, m => m.MessageText, m => m.ProviderMessageId!);
        }

        // Newest first by default; Id breaks ties so paging is stable.
        query = query.ApplySorting(request.Request.SortBy ?? nameof(SmsMessage.CreatedAt),
            request.Request.SortBy is null || request.Request.SortDescending, m => m.Id,
            propertyMappings: new() { ["messagePreview"] = nameof(SmsMessage.MessageText) });

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<SmsMessageListItemDto>
        {
            Items = items.Select(m =>
            {
                var dto = Mapper.ToListItem(m);
                dto.MessagePreview = SmsMapper.Preview(m.MessageText);
                return dto;
            }).ToList(),
            TotalCount = total,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize,
        };
    }
}

public class GetSmsMessageByIdQueryHandler(AppDbContext db) : IRequestHandler<GetSmsMessageByIdQuery, SmsMessageDetailDto>
{
    private static readonly SmsMapper Mapper = new();

    public async Task<SmsMessageDetailDto> Handle(GetSmsMessageByIdQuery request, CancellationToken ct)
    {
        var message = await db.Set<SmsMessage>().AsNoTracking().Include(m => m.SendRequest)
            .FirstOrDefaultAsync(m => !m.IsDeleted && m.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SmsMessage), request.Id);
        var dto = Mapper.ToDetail(message);
        dto.MessagePreview = SmsMapper.Preview(message.MessageText);
        return dto;
    }
}

public class GetSmsSendRequestStatusQueryHandler(AppDbContext db) : IRequestHandler<GetSmsSendRequestStatusQuery, SmsSendRequestStatusDto>
{
    public async Task<SmsSendRequestStatusDto> Handle(GetSmsSendRequestStatusQuery request, CancellationToken ct)
    {
        var send = await db.Set<SmsSendRequest>().AsNoTracking()
            .FirstOrDefaultAsync(r => !r.IsDeleted && r.Id == request.RequestId, ct)
            ?? throw new NotFoundException(nameof(SmsSendRequest), request.RequestId);

        var counts = await db.Set<SmsMessage>().AsNoTracking()
            .Where(m => m.SendRequestId == send.Id)
            .GroupBy(m => m.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Cost = g.Sum(m => m.Cost) })
            .ToListAsync(ct);
        int Count(params SmsMessageStatus[] s) => counts.Where(c => s.Contains(c.Status)).Sum(c => c.Count);

        var failures = await db.Set<SmsMessage>().AsNoTracking()
            .Where(m => m.SendRequestId == send.Id && m.Status == SmsMessageStatus.Failed)
            .OrderBy(m => m.CreatedAt).Take(5)
            .Select(m => new SmsFailureDto(m.Recipient, m.ErrorMessage))
            .ToListAsync(ct);

        var queued = Count(SmsMessageStatus.Queued, SmsMessageStatus.Sending);
        return new SmsSendRequestStatusDto
        {
            RequestId = send.Id,
            RecipientCount = send.RecipientCount,
            Queued = queued,
            Submitted = Count(SmsMessageStatus.Submitted),
            Delivered = Count(SmsMessageStatus.Delivered),
            Failed = Count(SmsMessageStatus.Failed),
            Undetermined = Count(SmsMessageStatus.Undetermined),
            Completed = queued == 0,
            Cost = counts.Sum(c => c.Cost ?? 0),
            Failures = failures,
        };
    }
}

public class GetSmsComposeInfoQueryHandler(AppDbContext db, ISmsGatewayRuntime runtime) : IRequestHandler<GetSmsComposeInfoQuery, SmsComposeInfoDto>
{
    public async Task<SmsComposeInfoDto> Handle(GetSmsComposeInfoQuery request, CancellationToken ct)
    {
        var gateway = await db.Set<SmsGateway>().AsNoTracking()
            .FirstOrDefaultAsync(g => !g.IsDeleted && g.IsDefault && g.IsEnabled, ct);
        var dto = new SmsComposeInfoDto { CanSendBulk = request.CanSendBulk, MaxRecipients = SmsLimits.MaxRecipients };
        if (gateway is null) return dto;

        var view = runtime.ToDto(gateway);
        dto.HasActiveGateway = true;
        dto.GatewayId = gateway.Id;
        dto.GatewayName = gateway.Name;
        dto.ProviderName = view.ProviderName;
        dto.RatePerSegment = gateway.EffectiveRate;
        dto.Currency = gateway.Currency;
        return dto;
    }
}
