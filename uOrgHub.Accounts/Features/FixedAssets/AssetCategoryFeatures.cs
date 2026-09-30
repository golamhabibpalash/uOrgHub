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

public record GetAssetCategoriesQuery(PaginationRequest Request) : IQuery<PagedResult<AssetCategoryResponseDto>>;
public record GetAllAssetCategoriesQuery : IQuery<List<AssetCategoryResponseDto>>;
public record GetAssetCategoryByIdQuery(Guid Id) : IQuery<AssetCategoryResponseDto>;
public record CreateAssetCategoryCommand(CreateAssetCategoryDto Dto) : ICommand<AssetCategoryResponseDto>;
public record UpdateAssetCategoryCommand(Guid Id, UpdateAssetCategoryDto Dto) : ICommand<AssetCategoryResponseDto>;
public record DeleteAssetCategoryCommand(Guid Id) : ICommand<Unit>;

public class GetAssetCategoriesQueryHandler : IRequestHandler<GetAssetCategoriesQuery, PagedResult<AssetCategoryResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetAssetCategoriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<AssetCategoryResponseDto>> Handle(GetAssetCategoriesQuery request, CancellationToken ct)
    {
        var query = AssetCategoryQueries.WithAccounts(_context).Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name, x => x.Code);

        query = (request.Request.SortBy?.ToLowerInvariant(), request.Request.SortDescending) switch
        {
            ("code", false) => query.OrderBy(x => x.Code),
            ("code", true) => query.OrderByDescending(x => x.Code),
            ("usefullifemonths", false) => query.OrderBy(x => x.UsefulLifeMonths),
            ("usefullifemonths", true) => query.OrderByDescending(x => x.UsefulLifeMonths),
            (_, true) => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name),
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<AssetCategoryResponseDto>
        {
            Items = items.Select(x => _mapper.ToDto(x)).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetAllAssetCategoriesQueryHandler : IRequestHandler<GetAllAssetCategoriesQuery, List<AssetCategoryResponseDto>>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetAllAssetCategoriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<AssetCategoryResponseDto>> Handle(GetAllAssetCategoriesQuery request, CancellationToken ct)
    {
        var items = await AssetCategoryQueries.WithAccounts(_context)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        return items.Select(x => _mapper.ToDto(x)).ToList();
    }
}

public class GetAssetCategoryByIdQueryHandler : IRequestHandler<GetAssetCategoryByIdQuery, AssetCategoryResponseDto>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public GetAssetCategoryByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<AssetCategoryResponseDto> Handle(GetAssetCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await AssetCategoryQueries.WithAccounts(_context)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetCategory), request.Id);
        return _mapper.ToDto(entity);
    }
}

public class CreateAssetCategoryCommandHandler : IRequestHandler<CreateAssetCategoryCommand, AssetCategoryResponseDto>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public CreateAssetCategoryCommandHandler(AppDbContext context) => _context = context;

    public async Task<AssetCategoryResponseDto> Handle(CreateAssetCategoryCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        await AssetCategoryQueries.EnsureAccountsAsync(_context, dto.AssetAccountId, dto.AccumulatedDepreciationAccountId, dto.DepreciationExpenseAccountId, dto.HireExpenseAccountId, dto.HireRecoveryAccountId, ct);

        var set = _context.Set<Models.Entities.AssetCategory>();
        var code = dto.Code?.Trim() ?? string.Empty;
        if (code.Length == 0)
        {
            // Deleted rows still hold their code under the unique index, so they count too.
            var next = await set.CountAsync(ct) + 1;
            code = $"AC-{next:D3}";
            while (await set.AnyAsync(x => x.Code == code, ct))
                code = $"AC-{++next:D3}";
        }
        else if (await set.AnyAsync(x => x.Code == code, ct))
            throw new AppException($"Asset category code '{code}' already exists.");

        var entity = _mapper.ToEntity(dto);
        entity.Code = code;
        entity.IsActive = true;
        entity.CreatedAt = DateTime.UtcNow;

        set.Add(entity);
        await _context.SaveChangesAsync(ct);

        return _mapper.ToDto(await AssetCategoryQueries.WithAccounts(_context).FirstAsync(x => x.Id == entity.Id, ct));
    }
}

public class UpdateAssetCategoryCommandHandler : IRequestHandler<UpdateAssetCategoryCommand, AssetCategoryResponseDto>
{
    private readonly AppDbContext _context;
    private readonly FixedAssetMapper _mapper = new();
    public UpdateAssetCategoryCommandHandler(AppDbContext context) => _context = context;

    public async Task<AssetCategoryResponseDto> Handle(UpdateAssetCategoryCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var entity = await _context.Set<Models.Entities.AssetCategory>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetCategory), request.Id);

        await AssetCategoryQueries.EnsureAccountsAsync(_context, dto.AssetAccountId, dto.AccumulatedDepreciationAccountId, dto.DepreciationExpenseAccountId, dto.HireExpenseAccountId, dto.HireRecoveryAccountId, ct);

        // Re-pointing the GL accounts once depreciation has posted would split one asset's history
        // across two sets of accounts: past runs credited the old ones, future runs the new ones.
        var accountsChanged = entity.AssetAccountId != dto.AssetAccountId
            || entity.AccumulatedDepreciationAccountId != dto.AccumulatedDepreciationAccountId
            || entity.DepreciationExpenseAccountId != dto.DepreciationExpenseAccountId;
        if (accountsChanged && await AssetCategoryQueries.HasPostedDepreciationAsync(_context, entity.Id, ct))
            throw new AppException("This category's assets already have posted depreciation, so its GL accounts can no longer be changed.");

        var hireAccountsChanged = entity.HireExpenseAccountId != dto.HireExpenseAccountId
            || entity.HireRecoveryAccountId != dto.HireRecoveryAccountId;
        if (hireAccountsChanged && await AssetCategoryQueries.HasPostedHireChargesAsync(_context, entity.Id, ct))
            throw new AppException("Hire charges have already been posted for this category's assets, so its hire accounts can no longer be changed.");

        _mapper.UpdateEntity(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return _mapper.ToDto(await AssetCategoryQueries.WithAccounts(_context).FirstAsync(x => x.Id == entity.Id, ct));
    }
}

public class DeleteAssetCategoryCommandHandler : IRequestHandler<DeleteAssetCategoryCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteAssetCategoryCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteAssetCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.AssetCategory>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.AssetCategory), request.Id);

        if (await _context.Set<Models.Entities.FixedAsset>().AnyAsync(x => !x.IsDeleted && x.CategoryId == entity.Id, ct))
            throw new AppException("This category still has fixed assets. Move or delete them first, or mark the category inactive.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

internal static class AssetCategoryQueries
{
    public static IQueryable<Models.Entities.AssetCategory> WithAccounts(AppDbContext context) =>
        context.Set<Models.Entities.AssetCategory>()
            .Include(x => x.AssetAccount)
            .Include(x => x.AccumulatedDepreciationAccount)
            .Include(x => x.DepreciationExpenseAccount)
            .Include(x => x.HireExpenseAccount)
            .Include(x => x.HireRecoveryAccount);

    public static Task<bool> HasPostedHireChargesAsync(AppDbContext context, Guid categoryId, CancellationToken ct) =>
        context.Set<Models.Entities.HireChargeRunLine>().AnyAsync(l =>
            !l.IsDeleted
            && l.AssetDeployment.FixedAsset.CategoryId == categoryId
            && l.HireChargeRun.Status == HireChargeRunStatus.Posted
            && !l.HireChargeRun.IsDeleted, ct);

    public static Task<bool> HasPostedDepreciationAsync(AppDbContext context, Guid categoryId, CancellationToken ct) =>
        context.Set<Models.Entities.DepreciationRunLine>().AnyAsync(l =>
            !l.IsDeleted
            && l.FixedAsset.CategoryId == categoryId
            && l.DepreciationRun.Status == DepreciationRunStatus.Posted
            && !l.DepreciationRun.IsDeleted, ct);

    /// <summary>
    /// Accumulated depreciation is a contra-asset: it lives with the assets on the balance sheet and
    /// carries a credit balance, so it must be an Asset-type account — not a liability.
    /// </summary>
    public static async Task EnsureAccountsAsync(AppDbContext context, Guid assetAccountId, Guid accumulatedAccountId, Guid expenseAccountId,
        Guid? hireExpenseAccountId, Guid? hireRecoveryAccountId, CancellationToken ct)
    {
        if (assetAccountId == accumulatedAccountId)
            throw new AppException("The asset account and the accumulated depreciation account must be different accounts.");

        var ids = new List<Guid> { assetAccountId, accumulatedAccountId, expenseAccountId };
        if (hireExpenseAccountId.HasValue) ids.Add(hireExpenseAccountId.Value);
        if (hireRecoveryAccountId.HasValue) ids.Add(hireRecoveryAccountId.Value);
        var accounts = await context.Set<Models.Entities.ChartOfAccount>()
            .Where(a => ids.Contains(a.Id) && !a.IsDeleted)
            .ToDictionaryAsync(a => a.Id, ct);

        Check(accounts, assetAccountId, AccountGroupType.Asset, "Asset account");
        Check(accounts, accumulatedAccountId, AccountGroupType.Asset, "Accumulated depreciation account");
        Check(accounts, expenseAccountId, AccountGroupType.Expense, "Depreciation expense account");
        // Hire is charged to the project as an expense and recovered by the company as income, so
        // the pair nets to zero company-wide while the project carries the cost.
        if (hireExpenseAccountId.HasValue)
            Check(accounts, hireExpenseAccountId.Value, AccountGroupType.Expense, "Equipment hire expense account");
        if (hireRecoveryAccountId.HasValue)
            Check(accounts, hireRecoveryAccountId.Value, AccountGroupType.Income, "Internal equipment recovery account");
    }

    private static void Check(Dictionary<Guid, Models.Entities.ChartOfAccount> accounts, Guid id, AccountGroupType expected, string label)
    {
        if (!accounts.TryGetValue(id, out var account))
            throw new AppException($"{label} was not found in the chart of accounts.");
        if (!account.IsActive)
            throw new AppException($"{label} '{account.AccountName}' is inactive.");
        if (account.AccountType != expected)
            throw new AppException($"{label} must be an {expected} account; '{account.AccountName}' is {account.AccountType}.");
    }
}
