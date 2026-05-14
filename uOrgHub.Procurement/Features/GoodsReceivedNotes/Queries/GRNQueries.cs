using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Features.PurchaseOrders.Queries;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Procurement.Features.GoodsReceivedNotes.Queries;

public record GetGRNsQuery(PaginationRequest Request, GRNStatus? Status = null) : IQuery<PagedResult<GRNResponseDto>>;
public record GetGRNByIdQuery(Guid Id) : IQuery<GRNResponseDto>;

public class GetGRNsQueryHandler : IRequestHandler<GetGRNsQuery, PagedResult<GRNResponseDto>>
{
    private readonly AppDbContext _context;
    public GetGRNsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<GRNResponseDto>> Handle(GetGRNsQuery request, CancellationToken ct)
    {
        var query = _context.Set<GoodsReceivedNote>()
            .Include(x => x.Items)
            .Include(x => x.PurchaseOrder)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.GRNNumber.Contains(request.Request.Search) ||
                                     (x.InvoiceNumber != null && x.InvoiceNumber.Contains(request.Request.Search)));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.GRNDate) : query.OrderBy(x => x.GRNDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        var warehouseIds = items.Select(x => x.WarehouseId).Distinct().ToList();
        var receiverIds = items.Select(x => x.ReceivedById).Distinct().ToList();
        var variantIds = items.SelectMany(x => x.Items.Select(i => i.ItemVariantId)).Distinct().ToList();

        var warehouses = await _context.Set<Warehouse>().Where(x => warehouseIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var receivers = await _context.Set<Employee>().Where(x => receiverIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return new PagedResult<GRNResponseDto>
        {
            Items = items.Select(grn => GRNQueryHelper.BuildDto(grn, warehouses, receivers, variants)).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetGRNByIdQueryHandler : IRequestHandler<GetGRNByIdQuery, GRNResponseDto>
{
    private readonly AppDbContext _context;
    public GetGRNByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<GRNResponseDto> Handle(GetGRNByIdQuery request, CancellationToken ct)
    {
        var grn = await _context.Set<GoodsReceivedNote>()
            .Include(x => x.Items)
            .Include(x => x.PurchaseOrder)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(GoodsReceivedNote), request.Id);

        var variantIds = grn.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var warehouses = await _context.Set<Warehouse>().Where(x => x.Id == grn.WarehouseId).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var receivers = await _context.Set<Employee>().Where(x => x.Id == grn.ReceivedById).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return GRNQueryHelper.BuildDto(grn, warehouses, receivers, variants);
    }
}
