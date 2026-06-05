using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Procurement.Features.VendorQuotations.Queries;

public record GetQuotationsQuery(PaginationRequest Request, QuotationStatus? Status = null) : IQuery<PagedResult<VendorQuotationResponseDto>>;
public record GetAllQuotationsForExportQuery(QuotationStatus? Status = null) : IQuery<List<VendorQuotationResponseDto>>;
public record GetQuotationByIdQuery(Guid Id) : IQuery<VendorQuotationResponseDto>;

public class GetQuotationsQueryHandler : IRequestHandler<GetQuotationsQuery, PagedResult<VendorQuotationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetQuotationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorQuotationResponseDto>> Handle(GetQuotationsQuery request, CancellationToken ct)
    {
        var query = _context.Set<VendorQuotation>()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .Include(x => x.RequestForQuotation)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.QuotationNumber);

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.QuotationDate) : query.OrderBy(x => x.QuotationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        var variantIds = items.SelectMany(x => x.Items.Select(i => i.ItemVariantId)).Distinct().ToList();
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return new PagedResult<VendorQuotationResponseDto>
        {
            Items = items.Select(q => BuildDto(q, variants)).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    internal static VendorQuotationResponseDto BuildDto(VendorQuotation q, Dictionary<Guid, dynamic> variants) => new()
    {
        Id = q.Id, QuotationNumber = q.QuotationNumber,
        RFQId = q.RFQId, RFQNumber = q.RequestForQuotation?.RFQNumber ?? string.Empty,
        VendorId = q.VendorId, VendorName = q.Vendor?.CompanyName ?? string.Empty,
        QuotationDate = q.QuotationDate, ValidUntil = q.ValidUntil,
        Status = q.Status, TotalAmount = q.TotalAmount,
        DeliveryDays = q.DeliveryDays, PaymentTerms = q.PaymentTerms,
        Notes = q.Notes, CreatedAt = q.CreatedAt,
        Items = q.Items.Where(i => !i.IsDeleted).Select(item => new QuotationItemResponseDto
        {
            Id = item.Id, RFQItemId = item.RFQItemId, ItemVariantId = item.ItemVariantId,
            VariantSKU = variants.TryGetValue(item.ItemVariantId, out var v) ? (string)v.SKU : string.Empty,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v2) ? (string)v2.VariantName : string.Empty,
            QuotedQuantity = item.QuotedQuantity, UnitPrice = item.UnitPrice, TotalPrice = item.TotalPrice, Notes = item.Notes
        }).ToList()
    };
}

public class GetAllQuotationsForExportQueryHandler : IRequestHandler<GetAllQuotationsForExportQuery, List<VendorQuotationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllQuotationsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<VendorQuotationResponseDto>> Handle(GetAllQuotationsForExportQuery request, CancellationToken ct)
    {
        var query = _context.Set<VendorQuotation>()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .Include(x => x.RequestForQuotation)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        query = query.OrderBy(x => x.QuotationDate);

        var items = await query.ToListAsync(ct);

        var variantIds = items.SelectMany(x => x.Items.Select(i => i.ItemVariantId)).Distinct().ToList();
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return items.Select(q => GetQuotationsQueryHandler.BuildDto(q, variants)).ToList();
    }
}

public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, VendorQuotationResponseDto>
{
    private readonly AppDbContext _context;
    public GetQuotationByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<VendorQuotationResponseDto> Handle(GetQuotationByIdQuery request, CancellationToken ct)
    {
        var q = await _context.Set<VendorQuotation>()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .Include(x => x.RequestForQuotation)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(VendorQuotation), request.Id);

        var variantIds = q.Items.Select(x => x.ItemVariantId).Distinct().ToList();
        var variantsList = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToListAsync(ct);
        var variants = variantsList.ToDictionary(x => x.Id, x => (dynamic)new VariantInfo(x.SKU, x.VariantName));

        return GetQuotationsQueryHandler.BuildDto(q, variants);
    }
}
