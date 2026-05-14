using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Procurement.Features.RequestForQuotations.Queries;

public record GetRFQsQuery(PaginationRequest Request, RFQStatus? Status = null) : IQuery<PagedResult<RFQResponseDto>>;
public record GetRFQByIdQuery(Guid Id) : IQuery<RFQResponseDto>;
public record GetRFQQuotationsQuery(Guid RFQId, PaginationRequest Request) : IQuery<PagedResult<VendorQuotationResponseDto>>;

public class GetRFQsQueryHandler : IRequestHandler<GetRFQsQuery, PagedResult<RFQResponseDto>>
{
    private readonly AppDbContext _context;
    public GetRFQsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<RFQResponseDto>> Handle(GetRFQsQuery request, CancellationToken ct)
    {
        var query = _context.Set<RequestForQuotation>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.RFQNumber.Contains(request.Request.Search) || x.Title.Contains(request.Request.Search));

        query = request.Request.SortDescending ? query.OrderByDescending(x => x.RFQDate) : query.OrderBy(x => x.RFQDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        var prIds = items.Where(x => x.PRId.HasValue).Select(x => x.PRId!.Value).Distinct().ToList();
        var variantIds = items.SelectMany(x => x.Items.Select(i => i.ItemVariantId)).Distinct().ToList();

        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);

        return new PagedResult<RFQResponseDto>
        {
            Items = items.Select(rfq => BuildDto(rfq, prs, variants)).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    internal static RFQResponseDto BuildDto(RequestForQuotation rfq, Dictionary<Guid, string> prs, Dictionary<Guid, VariantInfo> variants) => new()
    {
        Id = rfq.Id, RFQNumber = rfq.RFQNumber, RFQDate = rfq.RFQDate, ClosingDate = rfq.ClosingDate,
        PRId = rfq.PRId, PRNumber = rfq.PRId.HasValue ? prs.GetValueOrDefault(rfq.PRId.Value) : null,
        Title = rfq.Title, Description = rfq.Description, Status = rfq.Status, Notes = rfq.Notes, CreatedAt = rfq.CreatedAt,
        Items = rfq.Items.Where(i => !i.IsDeleted).Select(item => new RFQItemResponseDto
        {
            Id = item.Id, ItemVariantId = item.ItemVariantId,
            VariantSKU = variants.TryGetValue(item.ItemVariantId, out var v) ? v.SKU : string.Empty,
            VariantName = variants.TryGetValue(item.ItemVariantId, out var v2) ? v2.VariantName : string.Empty,
            RequestedQuantity = item.RequestedQuantity, Notes = item.Notes
        }).ToList()
    };
}

public class GetRFQByIdQueryHandler : IRequestHandler<GetRFQByIdQuery, RFQResponseDto>
{
    private readonly AppDbContext _context;
    public GetRFQByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<RFQResponseDto> Handle(GetRFQByIdQuery request, CancellationToken ct)
    {
        var rfq = await _context.Set<RequestForQuotation>()
            .Include(x => x.Items)
            .Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(RequestForQuotation), request.Id);

        var prIds = rfq.PRId.HasValue ? new List<Guid> { rfq.PRId.Value } : new List<Guid>();
        var variantIds = rfq.Items.Select(x => x.ItemVariantId).Distinct().ToList();

        var prs = await _context.Set<PurchaseRequisition>().Where(x => prIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PRNumber, ct);
        var variants = await _context.Set<ItemVariant>().Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => new VariantInfo(x.SKU, x.VariantName), ct);

        return GetRFQsQueryHandler.BuildDto(rfq, prs, variants);
    }
}

public class GetRFQQuotationsQueryHandler : IRequestHandler<GetRFQQuotationsQuery, PagedResult<VendorQuotationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetRFQQuotationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorQuotationResponseDto>> Handle(GetRFQQuotationsQuery request, CancellationToken ct)
    {
        var query = _context.Set<VendorQuotation>()
            .Include(x => x.Vendor)
            .Include(x => x.RequestForQuotation)
            .Where(x => !x.IsDeleted && x.RFQId == request.RFQId)
            .OrderByDescending(x => x.QuotationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<VendorQuotationResponseDto>
        {
            Items = items.Select(q => new VendorQuotationResponseDto
            {
                Id = q.Id, QuotationNumber = q.QuotationNumber,
                RFQId = q.RFQId, RFQNumber = q.RequestForQuotation?.RFQNumber ?? string.Empty,
                VendorId = q.VendorId, VendorName = q.Vendor?.CompanyName ?? string.Empty,
                QuotationDate = q.QuotationDate, ValidUntil = q.ValidUntil,
                Status = q.Status, TotalAmount = q.TotalAmount,
                DeliveryDays = q.DeliveryDays, PaymentTerms = q.PaymentTerms,
                Notes = q.Notes, CreatedAt = q.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}
