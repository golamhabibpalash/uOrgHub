using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.Payment;

public record GetPaymentsQuery(PaginationRequest Request, Guid? CustomerId = null, Guid? VendorId = null) : IQuery<PagedResult<PaymentResponseDto>>;
public record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentResponseDto>;
public record CreatePaymentCommand(CreatePaymentDto Dto) : ICommand<PaymentResponseDto>;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPaymentsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<PaymentResponseDto>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Models.Entities.Payment>()
            .Include(x => x.Customer)
            .Include(x => x.Vendor)
            .Include(x => x.Allocations)
            .Where(x => !x.IsDeleted);

        if (request.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);

        if (request.VendorId.HasValue)
            query = query.Where(x => x.VendorId == request.VendorId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.PaymentNumber.Contains(request.Request.Search)
                || (x.ReferenceNumber != null && x.ReferenceNumber.Contains(request.Request.Search)));

        query = request.Request.SortDescending
            ? query.OrderByDescending(x => x.PaymentDate)
            : query.OrderBy(x => x.PaymentDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<PaymentResponseDto>
        {
            Items = items.Select(PaymentMappingHelper.ToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize
        };
    }
}

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentResponseDto>
{
    private readonly AppDbContext _context;
    public GetPaymentByIdQueryHandler(AppDbContext context) => _context = context;

    public async Task<PaymentResponseDto> Handle(GetPaymentByIdQuery request, CancellationToken ct)
    {
        var e = await _context.Set<Models.Entities.Payment>()
            .Include(x => x.Customer)
            .Include(x => x.Vendor)
            .Include(x => x.Allocations)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Payment), request.Id);

        return PaymentMappingHelper.ToDto(e);
    }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResponseDto>
{
    private readonly AppDbContext _context;
    public CreatePaymentCommandHandler(AppDbContext context) => _context = context;

    public async Task<PaymentResponseDto> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        if (await _context.Set<Models.Entities.Payment>().AnyAsync(x => x.PaymentNumber == request.Dto.PaymentNumber && !x.IsDeleted, ct))
            throw new AppException($"Payment number '{request.Dto.PaymentNumber}' already exists.");

        var totalAllocated = request.Dto.Allocations.Sum(a => a.AllocatedAmount);
        if (totalAllocated > request.Dto.Amount)
            throw new AppException("Total allocated amount cannot exceed payment amount.");

        var entity = new Models.Entities.Payment
        {
            PaymentNumber = request.Dto.PaymentNumber,
            PaymentType = request.Dto.PaymentType,
            PaymentMethod = request.Dto.PaymentMethod,
            PaymentDate = request.Dto.PaymentDate,
            Amount = request.Dto.Amount,
            ReferenceNumber = request.Dto.ReferenceNumber,
            ChequeNumber = request.Dto.ChequeNumber,
            Notes = request.Dto.Notes,
            CustomerId = request.Dto.CustomerId,
            VendorId = request.Dto.VendorId,
            BankAccountId = request.Dto.BankAccountId,
            FiscalYearId = request.Dto.FiscalYearId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var alloc in request.Dto.Allocations)
        {
            entity.Allocations.Add(new Models.Entities.PaymentAllocation
            {
                InvoiceId = alloc.InvoiceId,
                BillId = alloc.BillId,
                AllocatedAmount = alloc.AllocatedAmount,
                CreatedAt = DateTime.UtcNow
            });

            if (alloc.InvoiceId.HasValue)
            {
                var invoice = await _context.Set<Models.Entities.Invoice>().FindAsync(new object[] { alloc.InvoiceId.Value }, ct);
                if (invoice != null)
                {
                    invoice.PaidAmount += alloc.AllocatedAmount;
                    invoice.Status = invoice.PaidAmount >= invoice.TotalAmount
                        ? InvoiceStatus.Paid
                        : InvoiceStatus.PartiallyPaid;
                    invoice.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (alloc.BillId.HasValue)
            {
                var bill = await _context.Set<Models.Entities.Bill>().FindAsync(new object[] { alloc.BillId.Value }, ct);
                if (bill != null)
                {
                    bill.PaidAmount += alloc.AllocatedAmount;
                    bill.Status = bill.PaidAmount >= bill.TotalAmount
                        ? BillStatus.Paid
                        : BillStatus.PartiallyPaid;
                    bill.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        _context.Set<Models.Entities.Payment>().Add(entity);
        await _context.SaveChangesAsync(ct);

        await _context.Entry(entity).Reference(x => x.Customer).LoadAsync(ct);
        await _context.Entry(entity).Reference(x => x.Vendor).LoadAsync(ct);
        return PaymentMappingHelper.ToDto(entity);
    }
}

file static class PaymentMappingHelper
{
    public static PaymentResponseDto ToDto(Models.Entities.Payment e) => new()
    {
        Id = e.Id,
        PaymentNumber = e.PaymentNumber,
        PaymentType = e.PaymentType,
        PaymentMethod = e.PaymentMethod,
        PaymentDate = e.PaymentDate,
        Amount = e.Amount,
        ReferenceNumber = e.ReferenceNumber,
        ChequeNumber = e.ChequeNumber,
        Notes = e.Notes,
        CustomerId = e.CustomerId,
        CustomerName = e.Customer?.Name,
        VendorId = e.VendorId,
        VendorName = e.Vendor?.Name,
        BankAccountId = e.BankAccountId,
        FiscalYearId = e.FiscalYearId,
        JournalEntryId = e.JournalEntryId,
        Allocations = e.Allocations.Select(a => new PaymentAllocationResponseDto
        {
            Id = a.Id,
            InvoiceId = a.InvoiceId,
            BillId = a.BillId,
            AllocatedAmount = a.AllocatedAmount
        }).ToList()
    };
}
