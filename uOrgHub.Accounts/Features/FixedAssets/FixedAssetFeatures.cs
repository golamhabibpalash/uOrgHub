using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Mappings;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.FixedAssets;

public record GetFixedAssetsQuery(PaginationRequest Request, Guid? CategoryId = null, FixedAssetStatus? Status = null) : IQuery<PagedResult<FixedAssetResponseDto>>;
public record GetFixedAssetByIdQuery(Guid Id) : IQuery<FixedAssetResponseDto>;
public record GetFixedAssetSummaryQuery : IQuery<FixedAssetSummaryDto>;
public record GetFixedAssetDepreciationHistoryQuery(Guid Id) : IQuery<List<FixedAssetDepreciationHistoryDto>>;
public record GetAllFixedAssetsForExportQuery : IQuery<List<FixedAssetResponseDto>>;
public record CreateFixedAssetCommand(CreateFixedAssetDto Dto) : ICommand<FixedAssetResponseDto>;
public record UpdateFixedAssetCommand(Guid Id, UpdateFixedAssetDto Dto) : ICommand<FixedAssetResponseDto>;
public record DeleteFixedAssetCommand(Guid Id) : ICommand<Unit>;

public class FixedAssetSummaryDto
{
    public int AssetCount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalAccumulatedDepreciation { get; set; }
    public decimal TotalBookValue => TotalCost - TotalAccumulatedDepreciation;
}

public class GetFixedAssetsQueryHandler : IRequestHandler<GetFixedAssetsQuery, PagedResult<FixedAssetResponseDto>>
{
    private readonly AppDbContext _context;
    public GetFixedAssetsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<FixedAssetResponseDto>> Handle(GetFixedAssetsQuery request, CancellationToken ct)
    {
        var query = FixedAssetQueries.WithDetails(_context).Where(x => !x.IsDeleted);

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search,
                x => x.AssetCode, x => x.Name, x => x.SerialNumber!, x => x.ChassisNumber!,
                x => x.EngineNumber!, x => x.RegistrationNumber!, x => x.Location!);

        query = (request.Request.SortBy?.ToLowerInvariant(), request.Request.SortDescending) switch
        {
            ("name", false) => query.OrderBy(x => x.Name),
            ("name", true) => query.OrderByDescending(x => x.Name),
            ("categoryname", false) => query.OrderBy(x => x.Category.Name),
            ("categoryname", true) => query.OrderByDescending(x => x.Category.Name),
            ("purchasedate", false) => query.OrderBy(x => x.PurchaseDate),
            ("purchasedate", true) => query.OrderByDescending(x => x.PurchaseDate),
            ("purchasecost", false) => query.OrderBy(x => x.PurchaseCost),
            ("purchasecost", true) => query.OrderByDescending(x => x.PurchaseCost),
            ("bookvalue", false) => query.OrderBy(x => x.PurchaseCost - x.AccumulatedDepreciation),
            ("bookvalue", true) => query.OrderByDescending(x => x.PurchaseCost - x.AccumulatedDepreciation),
            ("status", false) => query.OrderBy(x => x.Status),
            ("status", true) => query.OrderByDescending(x => x.Status),
            (_, true) => query.OrderByDescending(x => x.AssetCode),
            _ => query.OrderBy(x => x.AssetCode),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<FixedAssetResponseDto>
        {
            Items = await FixedAssetQueries.ToDtosAsync(_context, items, ct),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetFixedAssetByIdQueryHandler : IRequestHandler<GetFixedAssetByIdQuery, FixedAssetResponseDto>
{
    private readonly AppDbContext _context;
    public GetFixedAssetByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<FixedAssetResponseDto> Handle(GetFixedAssetByIdQuery request, CancellationToken ct)
    {
        var entity = await FixedAssetQueries.WithDetails(_context)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.FixedAsset), request.Id);
        return (await FixedAssetQueries.ToDtosAsync(_context, new() { entity }, ct))[0];
    }
}

public class GetFixedAssetSummaryQueryHandler : IRequestHandler<GetFixedAssetSummaryQuery, FixedAssetSummaryDto>
{
    private readonly AppDbContext _context;
    public GetFixedAssetSummaryQueryHandler(AppDbContext context) => _context = context;

    public async Task<FixedAssetSummaryDto> Handle(GetFixedAssetSummaryQuery request, CancellationToken ct)
    {
        var assets = _context.Set<Models.Entities.FixedAsset>().Where(x => !x.IsDeleted);
        return new FixedAssetSummaryDto
        {
            AssetCount = await assets.CountAsync(ct),
            TotalCost = await assets.SumAsync(x => x.PurchaseCost, ct),
            TotalAccumulatedDepreciation = await assets.SumAsync(x => x.AccumulatedDepreciation, ct),
        };
    }
}

public class GetFixedAssetDepreciationHistoryQueryHandler : IRequestHandler<GetFixedAssetDepreciationHistoryQuery, List<FixedAssetDepreciationHistoryDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetFixedAssetDepreciationHistoryQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<FixedAssetDepreciationHistoryDto>> Handle(GetFixedAssetDepreciationHistoryQuery request, CancellationToken ct)
    {
        var lines = await _context.Set<Models.Entities.DepreciationRunLine>()
            .Include(l => l.DepreciationRun)
            .Where(l => !l.IsDeleted && !l.DepreciationRun.IsDeleted && l.FixedAssetId == request.Id)
            .OrderByDescending(l => l.DepreciationRun.PeriodEndDate)
            .ThenByDescending(l => l.DepreciationRun.CreatedAt)
            .ToListAsync(ct);
        return lines.Select(l => _mapper.ToHistoryDto(l)).ToList();
    }
}

public class GetAllFixedAssetsForExportQueryHandler : IRequestHandler<GetAllFixedAssetsForExportQuery, List<FixedAssetResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllFixedAssetsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<FixedAssetResponseDto>> Handle(GetAllFixedAssetsForExportQuery request, CancellationToken ct)
    {
        var items = await FixedAssetQueries.WithDetails(_context)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.AssetCode)
            .ToListAsync(ct);
        return await FixedAssetQueries.ToDtosAsync(_context, items, ct);
    }
}

public class CreateFixedAssetCommandHandler : IRequestHandler<CreateFixedAssetCommand, FixedAssetResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IDocumentNumberingService _numbering;
    private readonly FixedAssetMapper _mapper = new();

    public CreateFixedAssetCommandHandler(AppDbContext context, IDocumentNumberingService numbering)
    {
        _context = context;
        _numbering = numbering;
    }

    public async Task<FixedAssetResponseDto> Handle(CreateFixedAssetCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var category = await FixedAssetQueries.GetActiveCategoryAsync(_context, dto.CategoryId, ct);

        var entity = _mapper.ToEntity(dto);
        entity.DepreciationStartDate = dto.DepreciationStartDate ?? dto.PurchaseDate;
        entity.UsefulLifeMonths = dto.UsefulLifeMonths ?? category.UsefulLifeMonths;
        entity.DepreciationMethod = dto.DepreciationMethod ?? category.DepreciationMethod;
        entity.SalvageValue = dto.SalvageValue
            ?? Math.Round(dto.PurchaseCost * category.SalvageValuePercent / 100m, 2, MidpointRounding.AwayFromZero);

        // An asset bought before go-live arrives part-depreciated: record how far, so the first run
        // charges only the months after that point instead of re-charging the whole history.
        entity.AccumulatedDepreciation = dto.OpeningAccumulatedDepreciation;
        entity.LastDepreciationDate = dto.OpeningAccumulatedDepreciation > 0 && dto.OpeningDepreciatedUpTo.HasValue
            ? DepreciationCalculator.MonthEnd(dto.OpeningDepreciatedUpTo.Value.Year, dto.OpeningDepreciatedUpTo.Value.Month)
            : null;
        entity.CreatedAt = DateTime.UtcNow;

        // Everything is validated before a code is drawn: the numbering sequence never gives a
        // number back, so a rejected request that had already taken one would leave a gap.
        FixedAssetQueries.EnsureAmounts(entity);
        entity.VendorId = await FixedAssetQueries.ResolveReferencesAsync(_context, entity.VendorId, entity.BillId, entity.CostCenterId, ct);

        var set = _context.Set<Models.Entities.FixedAsset>();
        var assetCode = dto.AssetCode?.Trim() ?? string.Empty;
        if (assetCode.Length == 0)
            assetCode = await _numbering.GenerateNextAsync("FixedAsset", "FA");
        else if (await set.AnyAsync(x => x.AssetCode == assetCode, ct))
            throw new AppException($"Asset code '{assetCode}' already exists.");
        entity.AssetCode = assetCode;

        set.Add(entity);
        await _context.SaveChangesAsync(ct);

        return await FixedAssetQueries.LoadDtoAsync(_context, entity.Id, ct);
    }
}

public class UpdateFixedAssetCommandHandler : IRequestHandler<UpdateFixedAssetCommand, FixedAssetResponseDto>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public UpdateFixedAssetCommandHandler(AppDbContext context) => _context = context;

    public async Task<FixedAssetResponseDto> Handle(UpdateFixedAssetCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var entity = await _context.Set<Models.Entities.FixedAsset>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.FixedAsset), request.Id);

        if (dto.CategoryId != entity.CategoryId)
            await FixedAssetQueries.GetActiveCategoryAsync(_context, dto.CategoryId, ct);

        // Deployed is owned by the deployment record: set by deploying, cleared by returning.
        if (entity.Status == FixedAssetStatus.Deployed && dto.Status != FixedAssetStatus.Deployed)
            throw new AppException("This asset is on a project. Return it from its deployment to change its status.");
        if (entity.Status != FixedAssetStatus.Deployed && dto.Status == FixedAssetStatus.Deployed)
            throw new AppException("Deploy the asset to a project instead of setting its status to Deployed.");

        // Once depreciation has posted, the figures it was computed from are history. Changing them
        // would leave the ledger disagreeing with the register with no entry explaining why.
        var financialsChanged = dto.CategoryId != entity.CategoryId
            || dto.PurchaseCost != entity.PurchaseCost
            || dto.SalvageValue != entity.SalvageValue
            || dto.UsefulLifeMonths != entity.UsefulLifeMonths
            || dto.DepreciationMethod != entity.DepreciationMethod
            || dto.DepreciationStartDate.Date != entity.DepreciationStartDate.Date;
        if (financialsChanged && await FixedAssetQueries.HasPostedDepreciationAsync(_context, entity.Id, ct))
            throw new AppException(
                "Depreciation has already been posted for this asset, so its category, cost, salvage value, " +
                "useful life, method and start date are locked. Reverse the depreciation runs first to change them.");

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        FixedAssetQueries.EnsureAmounts(entity);
        entity.VendorId = await FixedAssetQueries.ResolveReferencesAsync(_context, entity.VendorId, entity.BillId, entity.CostCenterId, ct);

        await _context.SaveChangesAsync(ct);
        return await FixedAssetQueries.LoadDtoAsync(_context, entity.Id, ct);
    }
}

public class DeleteFixedAssetCommandHandler : IRequestHandler<DeleteFixedAssetCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteFixedAssetCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteFixedAssetCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.FixedAsset>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.FixedAsset), request.Id);

        if (await FixedAssetQueries.HasPostedDepreciationAsync(_context, entity.Id, ct))
            throw new AppException("This asset has posted depreciation and cannot be deleted. Reverse its depreciation runs first.");
        if (await _context.Set<Models.Entities.AssetDeployment>().AnyAsync(x => !x.IsDeleted && x.FixedAssetId == entity.Id, ct))
            throw new AppException("This asset has project deployments on record and cannot be deleted. Cancel them first.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

internal static class FixedAssetQueries
{
    private static readonly FixedAssetMapper Mapper = new();

    public static IQueryable<Models.Entities.FixedAsset> WithDetails(AppDbContext context) =>
        context.Set<Models.Entities.FixedAsset>()
            .Include(x => x.Category)
            .Include(x => x.Vendor)
            .Include(x => x.Bill)
            .Include(x => x.CostCenter);

    public static async Task<FixedAssetResponseDto> LoadDtoAsync(AppDbContext context, Guid id, CancellationToken ct)
    {
        var entity = await WithDetails(context).FirstAsync(x => x.Id == id, ct);
        return (await ToDtosAsync(context, new() { entity }, ct))[0];
    }

    /// <summary>Maps a page of assets, flagging in one query which of them have posted depreciation.</summary>
    public static async Task<List<FixedAssetResponseDto>> ToDtosAsync(AppDbContext context, List<Models.Entities.FixedAsset> assets, CancellationToken ct)
    {
        var ids = assets.Select(a => a.Id).ToList();
        var depreciated = (await context.Set<Models.Entities.DepreciationRunLine>()
            .Where(l => !l.IsDeleted && ids.Contains(l.FixedAssetId)
                && l.DepreciationRun.Status == DepreciationRunStatus.Posted && !l.DepreciationRun.IsDeleted)
            .Select(l => l.FixedAssetId)
            .Distinct()
            .ToListAsync(ct)).ToHashSet();

        return assets.Select(a =>
        {
            var dto = Mapper.ToDto(a);
            dto.HasPostedDepreciation = depreciated.Contains(a.Id);
            return dto;
        }).ToList();
    }

    public static Task<bool> HasPostedDepreciationAsync(AppDbContext context, Guid assetId, CancellationToken ct) =>
        context.Set<Models.Entities.DepreciationRunLine>().AnyAsync(l =>
            !l.IsDeleted && l.FixedAssetId == assetId
            && l.DepreciationRun.Status == DepreciationRunStatus.Posted && !l.DepreciationRun.IsDeleted, ct);

    public static async Task<Models.Entities.AssetCategory> GetActiveCategoryAsync(AppDbContext context, Guid categoryId, CancellationToken ct)
    {
        var category = await context.Set<Models.Entities.AssetCategory>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == categoryId, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetCategory), categoryId);
        if (!category.IsActive)
            throw new AppException($"Asset category '{category.Name}' is inactive.");
        return category;
    }

    public static void EnsureAmounts(Models.Entities.FixedAsset asset)
    {
        if (asset.SalvageValue < 0 || asset.SalvageValue >= asset.PurchaseCost)
            throw new AppException("Salvage value must be at least zero and less than the purchase cost.");
        if (asset.AccumulatedDepreciation > asset.PurchaseCost - asset.SalvageValue)
            throw new AppException("Accumulated depreciation cannot exceed the depreciable amount (cost minus salvage value).");
    }

    /// <summary>
    /// Checks the optional links and returns the vendor to store: a linked bill supplies its vendor
    /// when none was given, and a vendor that contradicts the bill is rejected.
    /// </summary>
    public static async Task<Guid?> ResolveReferencesAsync(AppDbContext context, Guid? vendorId, Guid? billId, Guid? costCenterId, CancellationToken ct)
    {
        if (billId.HasValue)
        {
            var bill = await context.Set<Models.Entities.Bill>()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == billId.Value, ct)
                ?? throw new NotFoundException(nameof(Models.Entities.Bill), billId.Value);
            // A draft bill has not reached the ledger yet, and a void/cancelled one never will — in
            // neither case does the bill actually carry this asset's cost onto the books.
            if (bill.Status == BillStatus.Draft || bill.Status == BillStatus.Void || bill.Status == BillStatus.Cancelled)
                throw new AppException($"Bill {bill.BillNumber} is {bill.Status}. Only an approved bill can be linked to an asset.");
            if (vendorId.HasValue && vendorId.Value != bill.VendorId)
                throw new AppException($"The selected vendor does not match the vendor on bill {bill.BillNumber}.");
            vendorId ??= bill.VendorId;
        }
        else if (vendorId.HasValue
            && !await context.Set<Shared.Entities.Vendor>().AnyAsync(x => !x.IsDeleted && x.Id == vendorId.Value, ct))
        {
            throw new NotFoundException("Vendor", vendorId.Value);
        }

        if (costCenterId.HasValue
            && !await context.Set<Models.Entities.CostCenter>().AnyAsync(x => !x.IsDeleted && x.Id == costCenterId.Value, ct))
            throw new NotFoundException(nameof(Models.Entities.CostCenter), costCenterId.Value);

        return vendorId;
    }
}
