using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Procurement.Features.PurchaseOrders.Queries;

public record GetPOsQuery(PaginationRequest Request, POStatus? Status = null) : IQuery<PagedResult<POResponseDto>>;
public record GetPOByIdQuery(Guid Id) : IQuery<POResponseDto>;
public record GetPOGRNsQuery(Guid POId, PaginationRequest Request) : IQuery<PagedResult<GRNResponseDto>>;

public class GetPOsQueryHandler : IRequestHandler<GetPOsQuery, PagedResult<POResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPOsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<POResponseDto>> Handle(GetPOsQuery request, CancellationToken ct)
    {
        var query = _context.Set<PurchaseOrder>()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.PONumber.Contains(request.Request.Search));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.PODate) : query.OrderBy(x => x.PODate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        var quotationIds = items.Where(x => x.QuotationId.HasValue).Select(x => x.QuotationId!.Value).Distinct().ToList();
        var prIds = items.Where(x => x.PRId.HasValue).Select(x => x.PRId!.Value).Distinct().ToList();
        var approverIds = items.Where(x => x.ApprovedById.HasValue).Select(x => x.ApprovedById!.Value).Distinct().ToList();
        var variantIds = items.SelectMany(x => x.Items.Select(i => i.ItemVariantId)).Distinct().ToList();

        var quotations = await _context.Set<VendorQuotation>().Where(x => quotationIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.QuotationNumber, ct);
        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var approvers = await _context.Set<Employee>().Where(x => approverIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return new PagedResult<POResponseDto>
        {
            Items = items.Select(po => BuildDto(po, quotations, prs, approvers, variants)).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    internal static POResponseDto BuildDto(PurchaseOrder po,
        Dictionary<Guid, string> quotations,
        Dictionary<Guid, string> prs,
        Dictionary<Guid, string> approvers,
        Dictionary<Guid, dynamic> variants) => new()
    {
        Id = po.Id, PONumber = po.PONumber, PODate = po.PODate, ExpectedDeliveryDate = po.ExpectedDeliveryDate,
        VendorId = po.VendorId, VendorName = po.Vendor?.CompanyName ?? string.Empty,
        QuotationId = po.QuotationId, QuotationNumber = po.QuotationId.HasValue ? quotations.GetValueOrDefault(po.QuotationId.Value) : null,
        PRId = po.PRId, PRNumber = po.PRId.HasValue ? prs.GetValueOrDefault(po.PRId.Value) : null,
        Status = po.Status, SubTotal = po.SubTotal, TaxAmount = po.TaxAmount, DiscountAmount = po.DiscountAmount, TotalAmount = po.TotalAmount,
        PaymentTerms = po.PaymentTerms, DeliveryAddress = po.DeliveryAddress, Notes = po.Notes,
        ApprovedById = po.ApprovedById, ApprovedByName = po.ApprovedById.HasValue ? approvers.GetValueOrDefault(po.ApprovedById.Value) : null,
        ApprovedAt = po.ApprovedAt, CreatedAt = po.CreatedAt,
        Items = po.Items.Where(i => !i.IsDeleted).Select(item => new POItemResponseDto
        {
            Id = item.Id, ItemVariantId = item.ItemVariantId,
            VariantSKU = variants.TryGetValue(item.ItemVariantId, out var v) ? (string)v.SKU : string.Empty,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v2) ? (string)v2.VariantName : string.Empty,
            OrderedQuantity = item.OrderedQuantity, ReceivedQuantity = item.ReceivedQuantity,
            UnitPrice = item.UnitPrice, TaxPercent = item.TaxPercent, DiscountPercent = item.DiscountPercent,
            TotalPrice = item.TotalPrice, Notes = item.Notes
        }).ToList()
    };
}

public class GetPOByIdQueryHandler : IRequestHandler<GetPOByIdQuery, POResponseDto>
{
    private readonly AppDbContext _context;
    public GetPOByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<POResponseDto> Handle(GetPOByIdQuery request, CancellationToken ct)
    {
        var po = await _context.Set<PurchaseOrder>()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        var quotationIds = po.QuotationId.HasValue ? new List<Guid> { po.QuotationId.Value } : new List<Guid>();
        var prIds = po.PRId.HasValue ? new List<Guid> { po.PRId.Value } : new List<Guid>();
        var approverIds = po.ApprovedById.HasValue ? new List<Guid> { po.ApprovedById.Value } : new List<Guid>();
        var variantIds = po.Items.Select(x => x.ItemVariantId).Distinct().ToList();

        var quotations = await _context.Set<VendorQuotation>().Where(x => quotationIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.QuotationNumber, ct);
        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var approvers = await _context.Set<Employee>().Where(x => approverIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}", ct);
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return GetPOsQueryHandler.BuildDto(po, quotations, prs, approvers, variants);
    }
}

public class GetPOGRNsQueryHandler : IRequestHandler<GetPOGRNsQuery, PagedResult<GRNResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPOGRNsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<GRNResponseDto>> Handle(GetPOGRNsQuery request, CancellationToken ct)
    {
        var query = _context.Set<GoodsReceivedNote>()
            .Include(x => x.Items)
            .Include(x => x.PurchaseOrder)
            .Where(x => !x.IsDeleted && x.POId == request.POId)
            .OrderByDescending(x => x.GRNDate);

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

internal static class GRNQueryHelper
{
    internal static GRNResponseDto BuildDto(GoodsReceivedNote grn,
        Dictionary<Guid, string> warehouses,
        Dictionary<Guid, string> receivers,
        Dictionary<Guid, dynamic> variants) => new()
    {
        Id = grn.Id, GRNNumber = grn.GRNNumber, GRNDate = grn.GRNDate,
        POId = grn.POId, PONumber = grn.PurchaseOrder?.PONumber ?? string.Empty,
        WarehouseId = grn.WarehouseId, WarehouseName = warehouses.GetValueOrDefault(grn.WarehouseId, string.Empty),
        ReceivedById = grn.ReceivedById, ReceivedByName = receivers.GetValueOrDefault(grn.ReceivedById, string.Empty),
        Status = grn.Status, Notes = grn.Notes, InvoiceNumber = grn.InvoiceNumber, InvoiceDate = grn.InvoiceDate, CreatedAt = grn.CreatedAt,
        Items = grn.Items.Where(i => !i.IsDeleted).Select(item => new GRNItemResponseDto
        {
            Id = item.Id, POItemId = item.POItemId, ItemVariantId = item.ItemVariantId,
            VariantSKU = variants.TryGetValue(item.ItemVariantId, out var v) ? (string)v.SKU : string.Empty,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v2) ? (string)v2.VariantName : string.Empty,
            OrderedQuantity = item.OrderedQuantity, ReceivedQuantity = item.ReceivedQuantity,
            RejectedQuantity = item.RejectedQuantity, AcceptedQuantity = item.AcceptedQuantity,
            UnitCost = item.UnitCost, Notes = item.Notes
        }).ToList()
    };
}
