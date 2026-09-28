using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.FixedAssets;

public record GetDepreciationRunsQuery(PaginationRequest Request, int? Year = null) : IQuery<PagedResult<DepreciationRunResponseDto>>;
public record GetDepreciationRunByIdQuery(Guid Id) : IQuery<DepreciationRunResponseDto>;
public record GetAllDepreciationRunsForExportQuery : IQuery<List<DepreciationRunResponseDto>>;
public record PreviewDepreciationQuery(int Year, int Month) : IQuery<DepreciationPreviewDto>;
public record PostDepreciationRunCommand(PostDepreciationRunDto Dto) : ICommand<DepreciationRunResponseDto>;
public record ReverseDepreciationRunCommand(Guid Id) : ICommand<DepreciationRunResponseDto>;

public class GetDepreciationRunsQueryHandler : IRequestHandler<GetDepreciationRunsQuery, PagedResult<DepreciationRunResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetDepreciationRunsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<DepreciationRunResponseDto>> Handle(GetDepreciationRunsQuery request, CancellationToken ct)
    {
        var query = DepreciationRunQueries.WithDetails(_context).Where(x => !x.IsDeleted);

        if (request.Year.HasValue)
            query = query.Where(x => x.PeriodYear == request.Year.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.RunNumber, x => x.Notes!);

        query = (request.Request.SortBy?.ToLowerInvariant(), request.Request.SortDescending) switch
        {
            ("runnumber", false) => query.OrderBy(x => x.RunNumber),
            ("runnumber", true) => query.OrderByDescending(x => x.RunNumber),
            ("totalamount", false) => query.OrderBy(x => x.TotalAmount),
            ("totalamount", true) => query.OrderByDescending(x => x.TotalAmount),
            ("periodenddate", false) => query.OrderBy(x => x.PeriodEndDate).ThenBy(x => x.CreatedAt),
            // Newest period first is what an accountant scanning the history wants by default.
            _ => query.OrderByDescending(x => x.PeriodEndDate).ThenByDescending(x => x.CreatedAt),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DepreciationRunResponseDto>
        {
            Items = items.Select(x => _mapper.ToDto(x)).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetDepreciationRunByIdQueryHandler : IRequestHandler<GetDepreciationRunByIdQuery, DepreciationRunResponseDto>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetDepreciationRunByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<DepreciationRunResponseDto> Handle(GetDepreciationRunByIdQuery request, CancellationToken ct)
    {
        var entity = await DepreciationRunQueries.WithDetails(_context)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.DepreciationRun), request.Id);
        return _mapper.ToDto(entity);
    }
}

public class GetAllDepreciationRunsForExportQueryHandler : IRequestHandler<GetAllDepreciationRunsForExportQuery, List<DepreciationRunResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetAllDepreciationRunsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<DepreciationRunResponseDto>> Handle(GetAllDepreciationRunsForExportQuery request, CancellationToken ct)
    {
        var items = await DepreciationRunQueries.WithDetails(_context)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.PeriodEndDate)
            .ToListAsync(ct);
        return items.Select(x => _mapper.ToDto(x)).ToList();
    }
}

public class PreviewDepreciationQueryHandler : IRequestHandler<PreviewDepreciationQuery, DepreciationPreviewDto>
{
    private readonly AppDbContext _context;
    public PreviewDepreciationQueryHandler(AppDbContext context) => _context = context;

    public async Task<DepreciationPreviewDto> Handle(PreviewDepreciationQuery request, CancellationToken ct)
    {
        await DepreciationRunQueries.EnsurePeriodCanBePostedAsync(_context, request.Year, request.Month, ct);
        var charges = await DepreciationRunQueries.ComputeChargesAsync(_context, request.Year, request.Month, ct);

        return new DepreciationPreviewDto
        {
            Year = request.Year,
            Month = request.Month,
            PeriodEndDate = DepreciationCalculator.MonthEnd(request.Year, request.Month),
            TotalAmount = charges.Sum(c => c.Charge.Amount),
            Lines = charges.Select(c => new DepreciationLineDto
            {
                FixedAssetId = c.Asset.Id,
                AssetCode = c.Asset.AssetCode,
                AssetName = c.Asset.Name,
                CategoryName = c.Asset.Category.Name,
                Months = c.Charge.Months,
                Amount = c.Charge.Amount,
                PurchaseCost = c.Asset.PurchaseCost,
                AccumulatedBefore = c.Asset.AccumulatedDepreciation,
                AccumulatedAfter = c.Asset.AccumulatedDepreciation + c.Charge.Amount,
            }).ToList()
        };
    }
}

public class PostDepreciationRunCommandHandler : IRequestHandler<PostDepreciationRunCommand, DepreciationRunResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IDocumentNumberingService _numbering;
    private readonly IJournalEntryService _jeService;
    private readonly IJournalEntryRepository _jeRepository;
    private readonly FixedAssetMapper _mapper = new();

    public PostDepreciationRunCommandHandler(AppDbContext context, IDocumentNumberingService numbering,
        IJournalEntryService jeService, IJournalEntryRepository jeRepository)
    {
        _context = context;
        _numbering = numbering;
        _jeService = jeService;
        _jeRepository = jeRepository;
    }

    public async Task<DepreciationRunResponseDto> Handle(PostDepreciationRunCommand request, CancellationToken ct)
    {
        var (year, month) = (request.Dto.Year, request.Dto.Month);
        await DepreciationRunQueries.EnsurePeriodCanBePostedAsync(_context, year, month, ct);

        var charges = await DepreciationRunQueries.ComputeChargesAsync(_context, year, month, ct);
        var label = DepreciationRunQueries.PeriodLabel(year, month);
        if (charges.Count == 0)
            throw new AppException($"No depreciation is due for {label}. Every asset is either fully depreciated, not yet in service, or already charged for this month.");

        var periodEnd = DepreciationCalculator.MonthEnd(year, month);
        var runNumber = await _numbering.GenerateNextAsync("DepreciationRun", "DEP");

        var je = await BuildAndSaveJournalEntryAsync(runNumber, label, periodEnd, charges, ct);
        await _jeService.PostAsync(je.Id, "System");

        var run = new Models.Entities.DepreciationRun
        {
            RunNumber = runNumber,
            PeriodYear = year,
            PeriodMonth = month,
            PeriodEndDate = periodEnd,
            TotalAmount = charges.Sum(c => c.Charge.Amount),
            Status = DepreciationRunStatus.Posted,
            Notes = request.Dto.Notes,
            JournalEntryId = je.Id,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var (asset, charge) in charges)
        {
            run.Lines.Add(new Models.Entities.DepreciationRunLine
            {
                FixedAssetId = asset.Id,
                Months = charge.Months,
                Amount = charge.Amount,
                AccumulatedBefore = asset.AccumulatedDepreciation,
                AccumulatedAfter = asset.AccumulatedDepreciation + charge.Amount,
                PreviousLastDepreciationDate = asset.LastDepreciationDate,
                CreatedAt = DateTime.UtcNow,
            });

            asset.AccumulatedDepreciation += charge.Amount;
            asset.LastDepreciationDate = periodEnd;
            asset.UpdatedAt = DateTime.UtcNow;
        }

        _context.Set<Models.Entities.DepreciationRun>().Add(run);
        await _context.SaveChangesAsync(ct);

        var saved = await DepreciationRunQueries.WithDetails(_context).FirstAsync(x => x.Id == run.Id, ct);
        return _mapper.ToDto(saved);
    }

    /// <summary>
    /// One debit and one credit per asset rather than one pair per category: the expense line carries
    /// the asset's own cost center, and the ledger narration names the machine it was charged for.
    /// </summary>
    private async Task<Models.Entities.JournalEntry> BuildAndSaveJournalEntryAsync(
        string runNumber, string label, DateTime periodEnd, List<AssetCharge> charges, CancellationToken ct)
    {
        var je = new Models.Entities.JournalEntry
        {
            EntryNumber = await _jeRepository.GenerateEntryNumberAsync(),
            EntryDate = periodEnd,
            Description = $"Depreciation for {label} ({runNumber})",
            ReferenceNumber = runNumber,
            Status = JournalEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        var order = 1;
        foreach (var (asset, charge) in charges)
        {
            var narration = $"Depreciation {label} - {asset.AssetCode} {asset.Name}";
            je.Lines.Add(new Models.Entities.JournalEntryLine
            {
                AccountId = asset.Category.DepreciationExpenseAccountId,
                DebitAmount = charge.Amount,
                Description = narration,
                LineOrder = order++,
                CostCenterId = asset.CostCenterId,
                CreatedAt = DateTime.UtcNow
            });
            je.Lines.Add(new Models.Entities.JournalEntryLine
            {
                AccountId = asset.Category.AccumulatedDepreciationAccountId,
                CreditAmount = charge.Amount,
                Description = narration,
                LineOrder = order++,
                CreatedAt = DateTime.UtcNow
            });
        }

        je.TotalDebit = je.Lines.Sum(l => l.DebitAmount);
        je.TotalCredit = je.Lines.Sum(l => l.CreditAmount);

        _context.Set<Models.Entities.JournalEntry>().Add(je);
        await _context.SaveChangesAsync(ct);
        return je;
    }
}

public class ReverseDepreciationRunCommandHandler : IRequestHandler<ReverseDepreciationRunCommand, DepreciationRunResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;
    private readonly FixedAssetMapper _mapper = new();

    public ReverseDepreciationRunCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<DepreciationRunResponseDto> Handle(ReverseDepreciationRunCommand request, CancellationToken ct)
    {
        var run = await _context.Set<Models.Entities.DepreciationRun>()
            .Include(x => x.Lines).ThenInclude(l => l.FixedAsset)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.DepreciationRun), request.Id);

        if (run.Status != DepreciationRunStatus.Posted)
            throw new AppException($"Depreciation run {run.RunNumber} is already {run.Status}.");

        // Runs unwind newest-first. Each asset's "depreciated up to" date is restored from the run
        // being reversed, which is only correct if no later run has moved it on since.
        var latest = await _context.Set<Models.Entities.DepreciationRun>()
            .Where(x => !x.IsDeleted && x.Status == DepreciationRunStatus.Posted)
            .OrderByDescending(x => x.PeriodEndDate).ThenByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.RunNumber })
            .FirstAsync(ct);
        if (latest.Id != run.Id)
            throw new AppException($"Only the most recent depreciation run ({latest.RunNumber}) can be reversed. Reverse later runs first.");

        if (run.JournalEntryId.HasValue)
            await _jeService.CancelAsync(run.JournalEntryId.Value);

        foreach (var line in run.Lines.Where(l => !l.IsDeleted))
        {
            line.FixedAsset.AccumulatedDepreciation -= line.Amount;
            line.FixedAsset.LastDepreciationDate = line.PreviousLastDepreciationDate;
            line.FixedAsset.UpdatedAt = DateTime.UtcNow;
        }

        run.Status = DepreciationRunStatus.Reversed;
        run.ReversedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var saved = await DepreciationRunQueries.WithDetails(_context).FirstAsync(x => x.Id == run.Id, ct);
        return _mapper.ToDto(saved);
    }
}

internal record AssetCharge(Models.Entities.FixedAsset Asset, DepreciationCharge Charge);

internal static class DepreciationRunQueries
{
    public static IQueryable<Models.Entities.DepreciationRun> WithDetails(AppDbContext context) =>
        context.Set<Models.Entities.DepreciationRun>()
            .Include(x => x.JournalEntry)
            .Include(x => x.Lines).ThenInclude(l => l.FixedAsset).ThenInclude(a => a.Category);

    public static string PeriodLabel(int year, int month) =>
        new DateTime(year, month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    public static async Task EnsurePeriodCanBePostedAsync(AppDbContext context, int year, int month, CancellationToken ct)
    {
        if (year < 2000 || year > 2100 || month < 1 || month > 12)
            throw new AppException("Choose a valid month to run depreciation for.");

        var periodEnd = DepreciationCalculator.MonthEnd(year, month);
        var now = DateTime.UtcNow;
        if (periodEnd > DepreciationCalculator.MonthEnd(now.Year, now.Month))
            throw new AppException("Depreciation cannot be posted for a future month.");

        var label = PeriodLabel(year, month);
        var posted = context.Set<Models.Entities.DepreciationRun>()
            .Where(x => !x.IsDeleted && x.Status == DepreciationRunStatus.Posted);

        var samePeriod = await posted
            .Where(x => x.PeriodYear == year && x.PeriodMonth == month)
            .Select(x => x.RunNumber)
            .FirstOrDefaultAsync(ct);
        if (samePeriod is not null)
            throw new AppException($"Depreciation for {label} has already been posted ({samePeriod}). Reverse it first to run the month again.");

        var later = await posted
            .Where(x => x.PeriodEndDate > periodEnd)
            .OrderByDescending(x => x.PeriodEndDate)
            .Select(x => new { x.RunNumber, x.PeriodYear, x.PeriodMonth })
            .FirstOrDefaultAsync(ct);
        if (later is not null)
            throw new AppException($"{PeriodLabel(later.PeriodYear, later.PeriodMonth)} has already been posted ({later.RunNumber}). Depreciation must be run month by month in order.");
    }

    public static async Task<List<AssetCharge>> ComputeChargesAsync(AppDbContext context, int year, int month, CancellationToken ct)
    {
        var periodEnd = DepreciationCalculator.MonthEnd(year, month);
        var assets = await context.Set<Models.Entities.FixedAsset>()
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted
                && x.DepreciationStartDate <= periodEnd
                && x.AccumulatedDepreciation < x.PurchaseCost - x.SalvageValue)
            .OrderBy(x => x.AssetCode)
            .ToListAsync(ct);

        return assets
            .Select(a => new AssetCharge(a, DepreciationCalculator.Calculate(a, periodEnd)))
            .Where(c => c.Charge.Amount > 0)
            .ToList();
    }
}
