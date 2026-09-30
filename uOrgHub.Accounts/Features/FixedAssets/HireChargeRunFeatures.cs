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
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Features.FixedAssets;

public record GetHireChargeRunsQuery(PaginationRequest Request) : IQuery<PagedResult<HireChargeRunResponseDto>>;
public record GetHireChargeRunByIdQuery(Guid Id) : IQuery<HireChargeRunResponseDto>;
public record GetAllHireChargeRunsForExportQuery : IQuery<List<HireChargeRunResponseDto>>;
public record PreviewHireChargesQuery(DateTime FromDate, DateTime ToDate) : IQuery<HireChargePreviewDto>;
public record PostHireChargeRunCommand(PostHireChargeRunDto Dto) : ICommand<HireChargeRunResponseDto>;
public record ReverseHireChargeRunCommand(Guid Id) : ICommand<HireChargeRunResponseDto>;

public class GetHireChargeRunsQueryHandler : IRequestHandler<GetHireChargeRunsQuery, PagedResult<HireChargeRunResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetHireChargeRunsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<HireChargeRunResponseDto>> Handle(GetHireChargeRunsQuery request, CancellationToken ct)
    {
        var query = HireRunQueries.WithDetails(_context).Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.RunNumber, x => x.Notes!);

        query = (request.Request.SortBy?.ToLowerInvariant(), request.Request.SortDescending) switch
        {
            ("runnumber", false) => query.OrderBy(x => x.RunNumber),
            ("runnumber", true) => query.OrderByDescending(x => x.RunNumber),
            ("totalamount", false) => query.OrderBy(x => x.TotalAmount),
            ("totalamount", true) => query.OrderByDescending(x => x.TotalAmount),
            ("fromdate", false) => query.OrderBy(x => x.FromDate).ThenBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.FromDate).ThenByDescending(x => x.CreatedAt),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<HireChargeRunResponseDto>
        {
            Items = items.Select(x => _mapper.ToDto(x)).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetHireChargeRunByIdQueryHandler : IRequestHandler<GetHireChargeRunByIdQuery, HireChargeRunResponseDto>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetHireChargeRunByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<HireChargeRunResponseDto> Handle(GetHireChargeRunByIdQuery request, CancellationToken ct)
    {
        var entity = await HireRunQueries.WithDetails(_context)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.HireChargeRun), request.Id);
        return _mapper.ToDto(entity);
    }
}

public class GetAllHireChargeRunsForExportQueryHandler : IRequestHandler<GetAllHireChargeRunsForExportQuery, List<HireChargeRunResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetAllHireChargeRunsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<HireChargeRunResponseDto>> Handle(GetAllHireChargeRunsForExportQuery request, CancellationToken ct)
    {
        var items = await HireRunQueries.WithDetails(_context)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.FromDate)
            .ToListAsync(ct);
        return items.Select(x => _mapper.ToDto(x)).ToList();
    }
}

public class PreviewHireChargesQueryHandler : IRequestHandler<PreviewHireChargesQuery, HireChargePreviewDto>
{
    private readonly AppDbContext _context;
    private readonly IProjectCostLimitChecker _costLimits;

    public PreviewHireChargesQueryHandler(AppDbContext context, IProjectCostLimitChecker costLimits)
    {
        _context = context;
        _costLimits = costLimits;
    }

    public async Task<HireChargePreviewDto> Handle(PreviewHireChargesQuery request, CancellationToken ct)
    {
        var (from, to) = (HireChargeCalculator.AsUtcDay(request.FromDate), HireChargeCalculator.AsUtcDay(request.ToDate));
        await HireRunQueries.EnsureRangeCanBePostedAsync(_context, from, to, ct);
        var charges = await HireRunQueries.ComputeChargesAsync(_context, from, to, ct);

        return new HireChargePreviewDto
        {
            FromDate = from,
            ToDate = to,
            TotalAmount = charges.Sum(c => c.Charge.Amount),
            Lines = charges.Select(HireRunQueries.ToLineDto).ToList(),
            Warning = await _costLimits.CheckAsync(HireRunQueries.Allocations(charges), ct),
        };
    }
}

public class PostHireChargeRunCommandHandler : IRequestHandler<PostHireChargeRunCommand, HireChargeRunResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IDocumentNumberingService _numbering;
    private readonly IJournalEntryService _jeService;
    private readonly IJournalEntryRepository _jeRepository;
    private readonly IProjectCostLimitChecker _costLimits;
    private readonly FixedAssetMapper _mapper = new();

    public PostHireChargeRunCommandHandler(AppDbContext context, IDocumentNumberingService numbering,
        IJournalEntryService jeService, IJournalEntryRepository jeRepository, IProjectCostLimitChecker costLimits)
    {
        _context = context;
        _numbering = numbering;
        _jeService = jeService;
        _jeRepository = jeRepository;
        _costLimits = costLimits;
    }

    public async Task<HireChargeRunResponseDto> Handle(PostHireChargeRunCommand request, CancellationToken ct)
    {
        var (from, to) = (HireChargeCalculator.AsUtcDay(request.Dto.FromDate), HireChargeCalculator.AsUtcDay(request.Dto.ToDate));
        await HireRunQueries.EnsureRangeCanBePostedAsync(_context, from, to, ct);

        var charges = await HireRunQueries.ComputeChargesAsync(_context, from, to, ct);
        if (charges.Count == 0)
            throw new AppException($"No hire is due for {from:yyyy-MM-dd} to {to:yyyy-MM-dd}: no machine was on a project on a hire rate in that range.");

        // Checked before posting, so the projection is "already on the books plus this run".
        var warning = await _costLimits.CheckAsync(HireRunQueries.Allocations(charges), ct);

        var runNumber = await _numbering.GenerateNextAsync("HireChargeRun", "HCR");
        var je = await BuildAndSaveJournalEntryAsync(runNumber, from, to, charges, ct);
        await _jeService.PostAsync(je.Id, "System");

        var run = new Models.Entities.HireChargeRun
        {
            RunNumber = runNumber,
            FromDate = from,
            ToDate = to,
            TotalAmount = charges.Sum(c => c.Charge.Amount),
            Status = HireChargeRunStatus.Posted,
            Notes = request.Dto.Notes,
            JournalEntryId = je.Id,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var (deployment, charge) in charges)
        {
            run.Lines.Add(new Models.Entities.HireChargeRunLine
            {
                AssetDeploymentId = deployment.Id,
                FromDate = charge.FromDate,
                ToDate = charge.ToDate,
                Days = charge.Days,
                RateUnit = deployment.RateUnit!.Value,
                Rate = deployment.Rate!.Value,
                Amount = charge.Amount,
                CreatedAt = DateTime.UtcNow,
            });
        }

        _context.Set<Models.Entities.HireChargeRun>().Add(run);
        await _context.SaveChangesAsync(ct);

        var saved = await HireRunQueries.WithDetails(_context).FirstAsync(x => x.Id == run.Id, ct);
        var result = _mapper.ToDto(saved);
        result.Warning = warning;
        return result;
    }

    private async Task<Models.Entities.JournalEntry> BuildAndSaveJournalEntryAsync(
        string runNumber, DateTime from, DateTime to, List<DeploymentCharge> charges, CancellationToken ct)
    {
        var je = new Models.Entities.JournalEntry
        {
            EntryNumber = await _jeRepository.GenerateEntryNumberAsync(),
            EntryDate = to,
            Description = $"Equipment hire {from:yyyy-MM-dd} to {to:yyyy-MM-dd} ({runNumber})",
            ReferenceNumber = runNumber,
            Status = JournalEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        var order = 1;
        foreach (var (deployment, charge) in charges)
        {
            var asset = deployment.FixedAsset;
            var narration = $"Hire {asset.AssetCode} {asset.Name} on {deployment.CostCenter.Name}, {charge.Days} day(s)";
            je.Lines.Add(new Models.Entities.JournalEntryLine
            {
                AccountId = asset.Category.HireExpenseAccountId!.Value,
                DebitAmount = charge.Amount,
                Description = narration,
                LineOrder = order++,
                CostCenterId = deployment.CostCenterId,
                CreatedAt = DateTime.UtcNow
            });
            je.Lines.Add(new Models.Entities.JournalEntryLine
            {
                AccountId = asset.Category.HireRecoveryAccountId!.Value,
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

public class ReverseHireChargeRunCommandHandler : IRequestHandler<ReverseHireChargeRunCommand, HireChargeRunResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;
    private readonly FixedAssetMapper _mapper = new();

    public ReverseHireChargeRunCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<HireChargeRunResponseDto> Handle(ReverseHireChargeRunCommand request, CancellationToken ct)
    {
        var run = await _context.Set<Models.Entities.HireChargeRun>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.HireChargeRun), request.Id);

        if (run.Status != HireChargeRunStatus.Posted)
            throw new AppException($"Hire charge run {run.RunNumber} is already {run.Status}.");

        // Unlike depreciation, hire runs carry no per-asset running state — what has been charged is
        // read from the posted runs themselves — so any run can be reversed, in any order. Its range
        // simply becomes free to be charged again.
        if (run.JournalEntryId.HasValue)
            await _jeService.CancelAsync(run.JournalEntryId.Value);

        run.Status = HireChargeRunStatus.Reversed;
        run.ReversedAt = DateTime.UtcNow;
        run.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var saved = await HireRunQueries.WithDetails(_context).FirstAsync(x => x.Id == run.Id, ct);
        return _mapper.ToDto(saved);
    }
}

internal record DeploymentCharge(Models.Entities.AssetDeployment Deployment, HireCharge Charge);

internal static class HireRunQueries
{
    public static IQueryable<Models.Entities.HireChargeRun> WithDetails(AppDbContext context) =>
        context.Set<Models.Entities.HireChargeRun>()
            .Include(x => x.JournalEntry)
            .Include(x => x.Lines).ThenInclude(l => l.AssetDeployment).ThenInclude(d => d.FixedAsset)
            .Include(x => x.Lines).ThenInclude(l => l.AssetDeployment).ThenInclude(d => d.CostCenter);

    public static async Task EnsureRangeCanBePostedAsync(AppDbContext context, DateTime from, DateTime to, CancellationToken ct)
    {
        if (to < from)
            throw new AppException("The To date must be on or after the From date.");
        if (to > DateTime.UtcNow.Date)
            throw new AppException("Hire cannot be charged for days that have not happened yet.");

        // Posted ranges never overlap, which is what guarantees no day on site is charged twice.
        var clash = await context.Set<Models.Entities.HireChargeRun>()
            .Where(x => !x.IsDeleted && x.Status == HireChargeRunStatus.Posted && x.FromDate <= to && x.ToDate >= from)
            .OrderBy(x => x.FromDate)
            .Select(x => new { x.RunNumber, x.FromDate, x.ToDate })
            .FirstOrDefaultAsync(ct);
        if (clash is not null)
            throw new AppException($"{clash.RunNumber} already charged {clash.FromDate:yyyy-MM-dd} to {clash.ToDate:yyyy-MM-dd}, which overlaps this range. Pick dates outside it, or reverse that run first.");
    }

    public static async Task<List<DeploymentCharge>> ComputeChargesAsync(AppDbContext context, DateTime from, DateTime to, CancellationToken ct)
    {
        var deployments = await context.Set<Models.Entities.AssetDeployment>()
            .Include(x => x.FixedAsset).ThenInclude(a => a.Category)
            .Include(x => x.CostCenter)
            .Where(x => !x.IsDeleted
                && x.ChargeMode == DeploymentChargeMode.HireRate
                && x.StartDate <= to
                && (x.EndDate == null || x.EndDate >= from))
            .OrderBy(x => x.FixedAsset.AssetCode)
            .ToListAsync(ct);

        var missing = deployments
            .Where(d => !d.FixedAsset.Category.HireExpenseAccountId.HasValue || !d.FixedAsset.Category.HireRecoveryAccountId.HasValue)
            .Select(d => d.FixedAsset.Category.Name)
            .Distinct()
            .ToList();
        if (missing.Count > 0)
            throw new AppException($"Set the equipment hire accounts on asset categor{(missing.Count == 1 ? "y" : "ies")} {string.Join(", ", missing.Select(m => $"'{m}'"))} before charging hire.");

        return deployments
            .Select(d => (Deployment: d, Charge: HireChargeCalculator.Calculate(d.RateUnit!.Value, d.Rate!.Value, d.StartDate, d.EndDate, from, to)))
            .Where(x => x.Charge is not null && x.Charge.Amount > 0)
            .Select(x => new DeploymentCharge(x.Deployment, x.Charge!))
            .ToList();
    }

    public static IEnumerable<ProjectCostAllocation> Allocations(List<DeploymentCharge> charges) =>
        charges.Select(c => new ProjectCostAllocation(c.Deployment.CostCenterId, c.Charge.Amount));

    public static HireChargeLineDto ToLineDto(DeploymentCharge c) => new()
    {
        AssetDeploymentId = c.Deployment.Id,
        AssetCode = c.Deployment.FixedAsset.AssetCode,
        AssetName = c.Deployment.FixedAsset.Name,
        ProjectName = c.Deployment.CostCenter.Name,
        FromDate = c.Charge.FromDate,
        ToDate = c.Charge.ToDate,
        Days = c.Charge.Days,
        RateUnit = c.Deployment.RateUnit!.Value,
        Rate = c.Deployment.Rate!.Value,
        Amount = c.Charge.Amount,
    };
}
