using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Stock.Queries;

public record GetStockBalancesQuery(PaginationRequest Request, Guid? WarehouseId = null, Guid? ItemVariantId = null) : IQuery<PagedResult<StockBalanceResponseDto>>;
public record GetStockBalanceByIdQuery(Guid Id) : IQuery<StockBalanceResponseDto>;

public class GetStockBalancesQueryHandler : IRequestHandler<GetStockBalancesQuery, PagedResult<StockBalanceResponseDto>>
{
    private readonly AppDbContext _context;
    public GetStockBalancesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<StockBalanceResponseDto>> Handle(GetStockBalancesQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.StockBalance>()
            .Include(x => x.ItemVariant).ThenInclude(x => x.Item)
            .Include(x => x.Warehouse)
            .Where(x => !x.IsDeleted);

        if (request.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        if (request.ItemVariantId.HasValue) query = query.Where(x => x.ItemVariantId == request.ItemVariantId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.ItemVariant.SKU.Contains(request.Request.Search) || x.Warehouse.Name.Contains(request.Request.Search));

        query = query.OrderBy(x => x.ItemVariant.SKU);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<StockBalanceResponseDto>
        {
            Items = items.Select(e => new StockBalanceResponseDto
            {
                Id = e.Id, ItemVariantId = e.ItemVariantId,
                VariantSKU = e.ItemVariant?.SKU ?? string.Empty, VariantName = e.ItemVariant?.VariantName ?? string.Empty,
                ItemBaseName = e.ItemVariant?.Item?.BaseName ?? string.Empty,
                WarehouseId = e.WarehouseId, WarehouseName = e.Warehouse?.Name ?? string.Empty,
                WarehouseCode = e.Warehouse?.Code ?? string.Empty,
                QuantityOnHand = e.QuantityOnHand, QuantityReserved = e.QuantityReserved, LastUpdated = e.LastUpdated
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetStockBalanceByIdQueryHandler : IRequestHandler<GetStockBalanceByIdQuery, StockBalanceResponseDto>
{
    private readonly AppDbContext _context;
    public GetStockBalanceByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<StockBalanceResponseDto> Handle(GetStockBalanceByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.StockBalance>()
            .Include(x => x.ItemVariant).ThenInclude(x => x.Item)
            .Include(x => x.Warehouse)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.StockBalance), request.Id);

        return new StockBalanceResponseDto
        {
            Id = e.Id, ItemVariantId = e.ItemVariantId,
            VariantSKU = e.ItemVariant?.SKU ?? string.Empty, VariantName = e.ItemVariant?.VariantName ?? string.Empty,
            ItemBaseName = e.ItemVariant?.Item?.BaseName ?? string.Empty,
            WarehouseId = e.WarehouseId, WarehouseName = e.Warehouse?.Name ?? string.Empty,
            WarehouseCode = e.Warehouse?.Code ?? string.Empty,
            QuantityOnHand = e.QuantityOnHand, QuantityReserved = e.QuantityReserved, LastUpdated = e.LastUpdated
        };
    }
}
