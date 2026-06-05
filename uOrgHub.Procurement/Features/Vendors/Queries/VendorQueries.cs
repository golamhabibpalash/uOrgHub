using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features._Common;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Procurement.Features.Vendors.Queries;

public record GetVendorsQuery(PaginationRequest Request, VendorStatus? Status = null, VendorType? VendorType = null) : IQuery<PagedResult<VendorResponseDto>>;
public record GetAllVendorsQuery(VendorStatus? Status = null, VendorType? VendorType = null, string? Search = null) : IQuery<List<VendorResponseDto>>;
public record GetVendorByIdQuery(Guid Id) : IQuery<VendorResponseDto>;
public record GetVendorQuotationsQuery(Guid VendorId, PaginationRequest Request) : IQuery<PagedResult<VendorQuotationResponseDto>>;
public record GetVendorOrdersQuery(Guid VendorId, PaginationRequest Request) : IQuery<PagedResult<POResponseDto>>;

public class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, PagedResult<VendorResponseDto>>
{
    private readonly AppDbContext _context;
    public GetVendorsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorResponseDto>> Handle(GetVendorsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Vendor>().Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.VendorType.HasValue) query = query.Where(x => x.VendorType == request.VendorType.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.CompanyName, x => x.VendorCode, x => x.Email);

        query = query.ApplySorting(request.Request.SortBy ?? "CompanyName", request.Request.SortDescending);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<VendorResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }

    internal static VendorResponseDto MapToDto(Vendor v) => new()
    {
        Id = v.Id, VendorCode = v.VendorCode, CompanyName = v.CompanyName,
        ContactPerson = v.ContactPerson, Email = v.Email, Phone = v.Phone,
        Address = v.Address, TradeLicense = v.TradeLicense, TIN = v.TIN, BIN = v.BIN,
        VendorType = v.VendorType, Status = v.Status,
        CreditLimit = v.CreditLimit, PaymentTermDays = v.PaymentTermDays,
        Notes = v.Notes, CreatedAt = v.CreatedAt
    };
}

public class GetAllVendorsQueryHandler : IRequestHandler<GetAllVendorsQuery, List<VendorResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllVendorsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<VendorResponseDto>> Handle(GetAllVendorsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Vendor>().Where(x => !x.IsDeleted);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.VendorType.HasValue) query = query.Where(x => x.VendorType == request.VendorType.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.WhereSearch(request.Search, x => x.CompanyName, x => x.VendorCode, x => x.Email);

        query = query.OrderBy(x => x.CompanyName);
        var items = await query.ToListAsync(ct);
        return items.Select(GetVendorsQueryHandler.MapToDto).ToList();
    }
}

public class GetVendorByIdQueryHandler : IRequestHandler<GetVendorByIdQuery, VendorResponseDto>
{
    private readonly AppDbContext _context;
    public GetVendorByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<VendorResponseDto> Handle(GetVendorByIdQuery request, CancellationToken ct)
    {
        var v = await _context.Set<Vendor>().Where(x => !x.IsDeleted && x.Id == request.Id).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Vendor), request.Id);

        return new VendorResponseDto
        {
            Id = v.Id, VendorCode = v.VendorCode, CompanyName = v.CompanyName,
            ContactPerson = v.ContactPerson, Email = v.Email, Phone = v.Phone,
            Address = v.Address, TradeLicense = v.TradeLicense, TIN = v.TIN, BIN = v.BIN,
            VendorType = v.VendorType, Status = v.Status,
            CreditLimit = v.CreditLimit, PaymentTermDays = v.PaymentTermDays,
            Notes = v.Notes, CreatedAt = v.CreatedAt
        };
    }
}

public class GetVendorQuotationsQueryHandler : IRequestHandler<GetVendorQuotationsQuery, PagedResult<VendorQuotationResponseDto>>
{
    private readonly AppDbContext _context;
    public GetVendorQuotationsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorQuotationResponseDto>> Handle(GetVendorQuotationsQuery request, CancellationToken ct)
    {
        var query = _context.Set<VendorQuotation>()
            .Include(x => x.RequestForQuotation)
            .Include(x => x.Vendor)
            .Where(x => !x.IsDeleted && x.VendorId == request.VendorId);

        query = query.ApplySorting(request.Request.SortBy ?? "QuotationDate", request.Request.SortDescending);

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

public class GetVendorOrdersQueryHandler : IRequestHandler<GetVendorOrdersQuery, PagedResult<POResponseDto>>
{
    private readonly AppDbContext _context;
    public GetVendorOrdersQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<POResponseDto>> Handle(GetVendorOrdersQuery request, CancellationToken ct)
    {
        var query = _context.Set<PurchaseOrder>()
            .Include(x => x.Vendor)
            .Where(x => !x.IsDeleted && x.VendorId == request.VendorId);

        query = query.ApplySorting(request.Request.SortBy ?? "PODate", request.Request.SortDescending);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((request.Request.Page - 1) * request.Request.PageSize).Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<POResponseDto>
        {
            Items = items.Select(p => new POResponseDto
            {
                Id = p.Id, PONumber = p.PONumber, PODate = p.PODate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                VendorId = p.VendorId, VendorName = p.Vendor?.CompanyName ?? string.Empty,
                QuotationId = p.QuotationId, PRId = p.PRId,
                Status = p.Status, SubTotal = p.SubTotal,
                TaxAmount = p.TaxAmount, DiscountAmount = p.DiscountAmount, TotalAmount = p.TotalAmount,
                PaymentTerms = p.PaymentTerms, Notes = p.Notes, CreatedAt = p.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}
