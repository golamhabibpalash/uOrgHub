using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features._Common;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Inventory.Features.Stock.Queries;

public record GetStockTransactionsQuery(PaginationRequest Request, Guid? WarehouseId = null, Guid? ItemVariantId = null, StockTransactionStatus? Status = null) : IQuery<PagedResult<StockTransactionResponseDto>>;
public record GetStockTransactionByIdQuery(Guid Id) : IQuery<StockTransactionResponseDto>;

public class GetStockTransactionsQueryHandler : IRequestHandler<GetStockTransactionsQuery, PagedResult<StockTransactionResponseDto>>
{
    private readonly AppDbContext _context;
    public GetStockTransactionsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<StockTransactionResponseDto>> Handle(GetStockTransactionsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.StockTransaction>()
            .Include(x => x.ItemVariant)
            .Include(x => x.Warehouse)
            .Include(x => x.FromWarehouse)
            .Where(x => !x.IsDeleted);

        if (request.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        if (request.ItemVariantId.HasValue) query = query.Where(x => x.ItemVariantId == request.ItemVariantId.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.TransactionNumber.Contains(request.Request.Search) || (x.ReferenceNumber != null && x.ReferenceNumber.Contains(request.Request.Search)));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.TransactionDate) : query.OrderBy(x => x.TransactionDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<StockTransactionResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    private static StockTransactionResponseDto MapToDto(Models.Entities.StockTransaction e) => new()
    {
        Id = e.Id, TransactionNumber = e.TransactionNumber, TransactionDate = e.TransactionDate,
        TransactionType = e.TransactionType, Status = e.Status,
        ItemVariantId = e.ItemVariantId, VariantSKU = e.ItemVariant?.SKU ?? string.Empty, VariantName = e.ItemVariant?.VariantName ?? string.Empty,
        WarehouseId = e.WarehouseId, WarehouseName = e.Warehouse?.Name ?? string.Empty,
        FromWarehouseId = e.FromWarehouseId, FromWarehouseName = e.FromWarehouse?.Name,
        Quantity = e.Quantity, UnitCost = e.UnitCost,
        ReferenceNumber = e.ReferenceNumber, Notes = e.Notes, CreatedAt = e.CreatedAt
    };
}

public class GetStockTransactionByIdQueryHandler : IRequestHandler<GetStockTransactionByIdQuery, StockTransactionResponseDto>
{
    private readonly AppDbContext _context;
    public GetStockTransactionByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<StockTransactionResponseDto> Handle(GetStockTransactionByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.StockTransaction>()
            .Include(x => x.ItemVariant)
            .Include(x => x.Warehouse)
            .Include(x => x.FromWarehouse)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.StockTransaction), request.Id);

        return new StockTransactionResponseDto
        {
            Id = e.Id, TransactionNumber = e.TransactionNumber, TransactionDate = e.TransactionDate,
            TransactionType = e.TransactionType, Status = e.Status,
            ItemVariantId = e.ItemVariantId, VariantSKU = e.ItemVariant?.SKU ?? string.Empty, VariantName = e.ItemVariant?.VariantName ?? string.Empty,
            WarehouseId = e.WarehouseId, WarehouseName = e.Warehouse?.Name ?? string.Empty,
            FromWarehouseId = e.FromWarehouseId, FromWarehouseName = e.FromWarehouse?.Name,
            Quantity = e.Quantity, UnitCost = e.UnitCost,
            ReferenceNumber = e.ReferenceNumber, Notes = e.Notes, CreatedAt = e.CreatedAt
        };
    }
}
