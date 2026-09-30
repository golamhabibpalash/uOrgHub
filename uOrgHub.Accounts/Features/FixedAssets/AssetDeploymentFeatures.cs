using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.FixedAssets;

public record GetAssetDeploymentsQuery(PaginationRequest Request, Guid? FixedAssetId = null, Guid? ProjectId = null, bool OpenOnly = false) : IQuery<PagedResult<AssetDeploymentResponseDto>>;
public record GetAssetDeploymentByIdQuery(Guid Id) : IQuery<AssetDeploymentResponseDto>;
public record DeployAssetCommand(DeployAssetDto Dto) : ICommand<AssetDeploymentResponseDto>;
public record ReturnAssetCommand(Guid DeploymentId, ReturnAssetDto Dto) : ICommand<AssetDeploymentResponseDto>;
public record CancelAssetDeploymentCommand(Guid Id) : ICommand<Unit>;

public class GetAssetDeploymentsQueryHandler : IRequestHandler<GetAssetDeploymentsQuery, PagedResult<AssetDeploymentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAssetDeploymentsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<AssetDeploymentResponseDto>> Handle(GetAssetDeploymentsQuery request, CancellationToken ct)
    {
        var query = DeploymentQueries.WithDetails(_context).Where(x => !x.IsDeleted);

        if (request.FixedAssetId.HasValue)
            query = query.Where(x => x.FixedAssetId == request.FixedAssetId.Value);
        if (request.ProjectId.HasValue)
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        if (request.OpenOnly)
            query = query.Where(x => x.EndDate == null);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search,
                x => x.FixedAsset.AssetCode, x => x.FixedAsset.Name, x => x.CostCenter.Name);

        query = (request.Request.SortBy?.ToLowerInvariant(), request.Request.SortDescending) switch
        {
            ("fixedassetname", false) => query.OrderBy(x => x.FixedAsset.Name),
            ("fixedassetname", true) => query.OrderByDescending(x => x.FixedAsset.Name),
            ("costcentername", false) => query.OrderBy(x => x.CostCenter.Name),
            ("costcentername", true) => query.OrderByDescending(x => x.CostCenter.Name),
            ("enddate", false) => query.OrderBy(x => x.EndDate),
            ("enddate", true) => query.OrderByDescending(x => x.EndDate),
            ("startdate", false) => query.OrderBy(x => x.StartDate),
            // Newest first by default: what is on site now matters more than history.
            _ => query.OrderByDescending(x => x.StartDate).ThenBy(x => x.FixedAsset.AssetCode),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<AssetDeploymentResponseDto>
        {
            Items = await DeploymentQueries.ToDtosAsync(_context, items, ct),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetAssetDeploymentByIdQueryHandler : IRequestHandler<GetAssetDeploymentByIdQuery, AssetDeploymentResponseDto>
{
    private readonly AppDbContext _context;
    public GetAssetDeploymentByIdQueryHandler(AppDbContext context) => _context = context;

    public Task<AssetDeploymentResponseDto> Handle(GetAssetDeploymentByIdQuery request, CancellationToken ct) =>
        DeploymentQueries.LoadDtoAsync(_context, request.Id, ct);
}

public class DeployAssetCommandHandler : IRequestHandler<DeployAssetCommand, AssetDeploymentResponseDto>
{
    private readonly AppDbContext _context;
    public DeployAssetCommandHandler(AppDbContext context) => _context = context;

    public async Task<AssetDeploymentResponseDto> Handle(DeployAssetCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var startDate = dto.StartDate.Date;

        var asset = await _context.Set<Models.Entities.FixedAsset>()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.FixedAssetId, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.FixedAsset), dto.FixedAssetId);

        if (asset.Status == FixedAssetStatus.Deployed)
            throw new AppException($"{asset.AssetCode} {asset.Name} is already deployed. Return it before sending it to another project.");
        if (asset.Status == FixedAssetStatus.UnderMaintenance)
            throw new AppException($"{asset.AssetCode} {asset.Name} is under maintenance and cannot be deployed.");
        if (startDate < asset.PurchaseDate.Date)
            throw new AppException("An asset cannot be deployed before its purchase date.");
        if (startDate > DateTime.UtcNow.Date)
            throw new AppException("Deployment start date cannot be in the future. Record it on the day the machine reaches site.");

        // Stays on site cannot overlap: the machine was in one place at a time, and hire is charged per day.
        var lastEnd = await _context.Set<Models.Entities.AssetDeployment>()
            .Where(x => !x.IsDeleted && x.FixedAssetId == asset.Id && x.EndDate != null)
            .MaxAsync(x => (DateTime?)x.EndDate, ct);
        if (lastEnd.HasValue && startDate <= lastEnd.Value.Date)
            throw new AppException($"This asset was on another project until {lastEnd.Value:yyyy-MM-dd}. The new deployment must start after that.");

        var costCenter = await _context.Set<Models.Entities.CostCenter>()
            .Where(x => !x.IsDeleted && x.ProjectId == dto.ProjectId)
            .OrderByDescending(x => x.IsActive)
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("This project has no cost center, so equipment costs have nowhere to land. Create one linked to the project first.");

        var isHire = dto.ChargeMode == DeploymentChargeMode.HireRate;
        if (isHire && (!asset.Category.HireExpenseAccountId.HasValue || !asset.Category.HireRecoveryAccountId.HasValue))
            throw new AppException($"Asset category '{asset.Category.Name}' has no equipment hire accounts. Set its hire expense and recovery accounts before deploying on a hire rate.");

        var deployment = new Models.Entities.AssetDeployment
        {
            FixedAssetId = asset.Id,
            ProjectId = dto.ProjectId,
            CostCenterId = costCenter.Id,
            StartDate = startDate,
            ChargeMode = dto.ChargeMode,
            RateUnit = isHire ? dto.RateUnit : null,
            Rate = isHire ? dto.Rate : null,
            PreviousLocation = asset.Location,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
        };

        asset.Status = FixedAssetStatus.Deployed;
        asset.Location = costCenter.Name;
        asset.UpdatedAt = DateTime.UtcNow;

        _context.Set<Models.Entities.AssetDeployment>().Add(deployment);
        await _context.SaveChangesAsync(ct);
        return await DeploymentQueries.LoadDtoAsync(_context, deployment.Id, ct);
    }
}

public class ReturnAssetCommandHandler : IRequestHandler<ReturnAssetCommand, AssetDeploymentResponseDto>
{
    private readonly AppDbContext _context;
    public ReturnAssetCommandHandler(AppDbContext context) => _context = context;

    public async Task<AssetDeploymentResponseDto> Handle(ReturnAssetCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var endDate = dto.EndDate.Date;

        var deployment = await _context.Set<Models.Entities.AssetDeployment>()
            .Include(x => x.FixedAsset)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.DeploymentId, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetDeployment), request.DeploymentId);

        if (deployment.EndDate.HasValue)
            throw new AppException($"This deployment already ended on {deployment.EndDate.Value:yyyy-MM-dd}.");
        if (endDate < deployment.StartDate.Date)
            throw new AppException("The return date cannot be before the deployment started.");
        if (endDate > DateTime.UtcNow.Date)
            throw new AppException("The return date cannot be in the future.");

        // Hire already posted past the return date would charge the project for days the machine was gone.
        var chargedUpTo = await DeploymentQueries.ChargedUpToAsync(_context, deployment.Id, ct);
        if (chargedUpTo.HasValue && endDate < chargedUpTo.Value.Date)
            throw new AppException($"Hire has already been charged for this deployment up to {chargedUpTo.Value:yyyy-MM-dd}. Reverse that hire charge run first, or return on or after that date.");

        deployment.EndDate = endDate;
        deployment.ReturnNotes = dto.ReturnNotes;
        deployment.UpdatedAt = DateTime.UtcNow;

        var asset = deployment.FixedAsset;
        asset.Status = dto.ReturnStatus;
        asset.Location = string.IsNullOrWhiteSpace(dto.ReturnLocation) ? deployment.PreviousLocation : dto.ReturnLocation.Trim();
        asset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return await DeploymentQueries.LoadDtoAsync(_context, deployment.Id, ct);
    }
}

public class CancelAssetDeploymentCommandHandler : IRequestHandler<CancelAssetDeploymentCommand, Unit>
{
    private readonly AppDbContext _context;
    public CancelAssetDeploymentCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(CancelAssetDeploymentCommand request, CancellationToken ct)
    {
        var deployment = await _context.Set<Models.Entities.AssetDeployment>()
            .Include(x => x.FixedAsset)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetDeployment), request.Id);

        if (await DeploymentQueries.ChargedUpToAsync(_context, deployment.Id, ct) is not null)
            throw new AppException("Hire has been charged for this deployment, so it cannot be cancelled. Reverse the hire charge runs first.");

        // Cancelling an open deployment puts the asset back where it was, as if it never left.
        if (deployment.EndDate is null)
        {
            deployment.FixedAsset.Status = FixedAssetStatus.Active;
            deployment.FixedAsset.Location = deployment.PreviousLocation;
            deployment.FixedAsset.UpdatedAt = DateTime.UtcNow;
        }

        deployment.IsDeleted = true;
        deployment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

internal static class DeploymentQueries
{
    private static readonly FixedAssetMapper Mapper = new();

    public static IQueryable<Models.Entities.AssetDeployment> WithDetails(AppDbContext context) =>
        context.Set<Models.Entities.AssetDeployment>()
            .Include(x => x.FixedAsset)
            .Include(x => x.CostCenter);

    public static async Task<AssetDeploymentResponseDto> LoadDtoAsync(AppDbContext context, Guid id, CancellationToken ct)
    {
        var entity = await WithDetails(context).FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetDeployment), id);
        return (await ToDtosAsync(context, new() { entity }, ct))[0];
    }

    /// <summary>Last day covered by a posted (not reversed) hire run for this deployment.</summary>
    public static Task<DateTime?> ChargedUpToAsync(AppDbContext context, Guid deploymentId, CancellationToken ct) =>
        PostedLines(context)
            .Where(l => l.AssetDeploymentId == deploymentId)
            .MaxAsync(l => (DateTime?)l.ToDate, ct);

    public static IQueryable<Models.Entities.HireChargeRunLine> PostedLines(AppDbContext context) =>
        context.Set<Models.Entities.HireChargeRunLine>()
            .Where(l => !l.IsDeleted && !l.HireChargeRun.IsDeleted && l.HireChargeRun.Status == HireChargeRunStatus.Posted);

    /// <summary>Maps a page of deployments with each one's charged-to date and hire total, in one query.</summary>
    public static async Task<List<AssetDeploymentResponseDto>> ToDtosAsync(AppDbContext context, List<Models.Entities.AssetDeployment> items, CancellationToken ct)
    {
        var ids = items.Select(x => x.Id).ToList();
        var charged = await PostedLines(context)
            .Where(l => ids.Contains(l.AssetDeploymentId))
            .GroupBy(l => l.AssetDeploymentId)
            .Select(g => new { Id = g.Key, UpTo = g.Max(l => l.ToDate), Total = g.Sum(l => l.Amount) })
            .ToDictionaryAsync(x => x.Id, ct);

        return items.Select(x =>
        {
            var dto = Mapper.ToDto(x);
            if (charged.TryGetValue(x.Id, out var c))
            {
                dto.ChargedUpTo = c.UpTo;
                dto.TotalHireCharged = c.Total;
            }
            return dto;
        }).ToList();
    }
}
