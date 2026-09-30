using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Features.Voucher;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Features.Payment;

public record GetPaymentsQuery(PaginationRequest Request, Guid? CustomerId = null, Guid? VendorId = null) : IQuery<PagedResult<PaymentResponseDto>>;
public record GetPaymentByIdQuery(Guid Id) : IQuery<PaymentResponseDto>;
public record CreatePaymentCommand(CreatePaymentDto Dto, string CreatedBy = "system") : ICommand<PaymentResponseDto>;
public record VoidPaymentCommand(Guid Id) : ICommand<PaymentResponseDto>;
public record GetAllPaymentsForExportQuery : IQuery<List<PaymentResponseDto>>;

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
            .Include(x => x.Voucher)
            .Where(x => !x.IsDeleted);

        if (request.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);

        if (request.VendorId.HasValue)
            query = query.Where(x => x.VendorId == request.VendorId.Value);

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.PaymentNumber, x => x.ReferenceNumber);

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
            .Include(x => x.Voucher)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Payment), request.Id);

        return PaymentMappingHelper.ToDto(e);
    }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IDocumentNumberingService _numbering;
    private readonly IJournalEntryService _jeService;
    private readonly IJournalEntryRepository _jeRepository;

    public CreatePaymentCommandHandler(AppDbContext context, IDocumentNumberingService numbering, IJournalEntryService jeService, IJournalEntryRepository jeRepository)
    {
        _context = context;
        _numbering = numbering;
        _jeService = jeService;
        _jeRepository = jeRepository;
    }

    public async Task<PaymentResponseDto> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        var paymentNumber = request.Dto.PaymentNumber;
        if (string.IsNullOrWhiteSpace(paymentNumber))
            paymentNumber = await _numbering.GenerateNextAsync("Payment", "PMT");

        if (await _context.Set<Models.Entities.Payment>().AnyAsync(x => x.PaymentNumber == paymentNumber && !x.IsDeleted, ct))
            throw new AppException($"Payment number '{paymentNumber}' already exists.");

        var totalAllocated = request.Dto.Allocations.Sum(a => a.AllocatedAmount);
        if (totalAllocated > request.Dto.Amount)
            throw new AppException("Total allocated amount cannot exceed payment amount.");

        // Check the voucher's prerequisites before anything is written, so ticking "create voucher"
        // either produces payment + voucher together or fails cleanly with nothing saved.
        Models.Entities.BankAccount? bankAccount = null;
        var arApAccountId = Guid.Empty;
        if (request.Dto.BankAccountId.HasValue)
        {
            bankAccount = await _context.Set<Models.Entities.BankAccount>()
                .FirstOrDefaultAsync(b => b.Id == request.Dto.BankAccountId.Value && !b.IsDeleted, ct);
            arApAccountId = await ResolveArApAccountIdAsync(request.Dto, ct);
        }
        if (request.Dto.CreateVoucher)
            EnsureVoucherPossible(request.Dto, bankAccount, arApAccountId);

        var entity = new Models.Entities.Payment
        {
            PaymentNumber = paymentNumber,
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
                var invoice = await _context.Set<Models.Entities.Invoice>().FindAsync(new object[] { alloc.InvoiceId.Value }, ct)
                    ?? throw new NotFoundException(nameof(Models.Entities.Invoice), alloc.InvoiceId.Value);

                if (invoice.PaidAmount + alloc.AllocatedAmount > invoice.TotalAmount)
                    throw new AppException($"Allocation of {alloc.AllocatedAmount} exceeds remaining balance of {invoice.TotalAmount - invoice.PaidAmount} on invoice {invoice.InvoiceNumber}.");

                invoice.PaidAmount += alloc.AllocatedAmount;
                invoice.Status = invoice.PaidAmount >= invoice.TotalAmount
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.PartiallyPaid;
                invoice.UpdatedAt = DateTime.UtcNow;
            }

            if (alloc.BillId.HasValue)
            {
                var bill = await _context.Set<Models.Entities.Bill>().FindAsync(new object[] { alloc.BillId.Value }, ct)
                    ?? throw new NotFoundException(nameof(Models.Entities.Bill), alloc.BillId.Value);

                if (bill.PaidAmount + alloc.AllocatedAmount > bill.TotalAmount)
                    throw new AppException($"Allocation of {alloc.AllocatedAmount} exceeds remaining balance of {bill.TotalAmount - bill.PaidAmount} on bill {bill.BillNumber}.");

                bill.PaidAmount += alloc.AllocatedAmount;
                bill.Status = bill.PaidAmount >= bill.TotalAmount
                    ? BillStatus.Paid
                    : BillStatus.PartiallyPaid;
                bill.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.Set<Models.Entities.Payment>().Add(entity);
        await _context.SaveChangesAsync(ct);

        if (bankAccount != null && arApAccountId != Guid.Empty)
        {
            var je = await BuildAndSavePaymentJeAsync(entity, bankAccount.ChartOfAccountId, arApAccountId, ct);
            await _jeService.PostAsync(je.Id, "System");
            entity.JournalEntryId = je.Id;

            if (request.Dto.CreateVoucher)
                entity.Voucher = await BuildPostedVoucherAsync(entity, je, request.CreatedBy, ct);

            await _context.SaveChangesAsync(ct);
        }

        await _context.Entry(entity).Reference(x => x.Customer).LoadAsync(ct);
        await _context.Entry(entity).Reference(x => x.Vendor).LoadAsync(ct);
        return PaymentMappingHelper.ToDto(entity);
    }

    private static void EnsureVoucherPossible(CreatePaymentDto dto, Models.Entities.BankAccount? bankAccount, Guid arApAccountId)
    {
        if (!dto.CustomerId.HasValue && !dto.VendorId.HasValue)
            throw new AppException("A voucher can only be created for a payment to a vendor or from a customer.");

        if (bankAccount is null)
            throw new AppException("Select the bank/cash account the money moved through to create a voucher.");

        if (arApAccountId == Guid.Empty)
            throw new AppException(dto.CustomerId.HasValue
                ? "The customer has no receivable account set, so no voucher can be created. Set one on the customer first."
                : "The vendor has no payable account set, so no voucher can be created. Set one on the vendor first.");
    }

    /// <summary>
    /// A voucher documenting the payment's already-posted journal entry. It skips the
    /// submit/approve/post workflow on purpose: running it would generate a second entry and book
    /// the payment twice. Money out becomes a Debit voucher, money in a Credit voucher — the same
    /// split <see cref="BuildAndSavePaymentJeAsync"/> uses.
    /// </summary>
    private async Task<Models.Entities.Voucher> BuildPostedVoucherAsync(
        Models.Entities.Payment payment, Models.Entities.JournalEntry je, string user, CancellationToken ct)
    {
        var debitLine = je.Lines.First(l => l.DebitAmount > 0);
        var creditLine = je.Lines.First(l => l.CreditAmount > 0);
        var voucherType = IsInflow(payment) ? VoucherType.Credit : VoucherType.Debit;

        var voucherNumber = await _numbering.GenerateNextAsync("Voucher", VoucherAccountRules.NumberPrefix(voucherType));

        var partyName = payment.CustomerId.HasValue
            ? (await _context.Set<Models.Entities.Customer>().FindAsync(new object[] { payment.CustomerId.Value }, ct))?.Name
            : (await _context.Set<Vendor>().FindAsync(new object[] { payment.VendorId!.Value }, ct))?.Name;

        var now = DateTime.UtcNow;
        var voucher = new Models.Entities.Voucher
        {
            VoucherNumber = voucherNumber,
            VoucherType = voucherType,
            VoucherDate = payment.PaymentDate,
            ReferenceNumber = payment.PaymentNumber,
            FiscalYearId = payment.FiscalYearId,
            Name = partyName,
            Description = voucherType == VoucherType.Credit
                ? $"Received from {partyName} against payment {payment.PaymentNumber}"
                : $"Paid to {partyName} against payment {payment.PaymentNumber}",
            DebitAccountId = debitLine.AccountId,
            CreditAccountId = creditLine.AccountId,
            Amount = payment.Amount,
            Status = VoucherStatus.Posted,
            JournalEntryId = je.Id,
            PreparedBy = user,
            SubmittedBy = user,
            SubmittedAt = now,
            ApprovedBy = user,
            ApprovedAt = now,
            PostedBy = user,
            PostedAt = now,
            CreatedBy = user,
            CreatedAt = now
        };
        _context.Set<Models.Entities.Voucher>().Add(voucher);
        return voucher;
    }

    // Inflow: CustomerPayment, AdvanceFromCustomer, or a vendor-side Refund (vendor refunds us)
    private static bool IsInflow(Models.Entities.Payment payment)
        => payment.PaymentType is PaymentType.CustomerPayment or PaymentType.AdvanceFromCustomer
            || (payment.PaymentType == PaymentType.Refund && payment.VendorId.HasValue);

    private async Task<Guid> ResolveArApAccountIdAsync(CreatePaymentDto dto, CancellationToken ct)
    {
        if (dto.CustomerId.HasValue)
        {
            var customer = await _context.Set<Models.Entities.Customer>().FindAsync(new object[] { dto.CustomerId.Value }, ct);
            return customer?.ReceivableAccountId ?? Guid.Empty;
        }
        if (dto.VendorId.HasValue)
        {
            var vendor = await _context.Set<Vendor>().FindAsync(new object[] { dto.VendorId.Value }, ct);
            return vendor?.PayableAccountId ?? Guid.Empty;
        }
        return Guid.Empty;
    }

    private async Task<Models.Entities.JournalEntry> BuildAndSavePaymentJeAsync(Models.Entities.Payment payment, Guid bankCoaId, Guid arApAccountId, CancellationToken ct)
    {
        var entryNumber = await _jeRepository.GenerateEntryNumberAsync();

        var isInflow = IsInflow(payment);

        var je = new Models.Entities.JournalEntry
        {
            EntryNumber = entryNumber,
            EntryDate = payment.PaymentDate,
            Description = $"Payment {payment.PaymentNumber}",
            ReferenceNumber = payment.ReferenceNumber ?? payment.PaymentNumber,
            Status = JournalEntryStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        if (isInflow)
        {
            je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = bankCoaId, DebitAmount = payment.Amount, Description = $"Payment received - {payment.PaymentNumber}", LineOrder = 1, CreatedAt = DateTime.UtcNow });
            je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = arApAccountId, CreditAmount = payment.Amount, Description = $"AR settlement - {payment.PaymentNumber}", LineOrder = 2, CreatedAt = DateTime.UtcNow });
        }
        else
        {
            je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = arApAccountId, DebitAmount = payment.Amount, Description = $"AP settlement - {payment.PaymentNumber}", LineOrder = 1, CreatedAt = DateTime.UtcNow });
            je.Lines.Add(new Models.Entities.JournalEntryLine { AccountId = bankCoaId, CreditAmount = payment.Amount, Description = $"Payment made - {payment.PaymentNumber}", LineOrder = 2, CreatedAt = DateTime.UtcNow });
        }

        je.TotalDebit = je.Lines.Sum(l => l.DebitAmount);
        je.TotalCredit = je.Lines.Sum(l => l.CreditAmount);

        _context.Set<Models.Entities.JournalEntry>().Add(je);
        await _context.SaveChangesAsync(ct);
        return je;
    }
}

public class VoidPaymentCommandHandler : IRequestHandler<VoidPaymentCommand, PaymentResponseDto>
{
    private readonly AppDbContext _context;
    private readonly IJournalEntryService _jeService;

    public VoidPaymentCommandHandler(AppDbContext context, IJournalEntryService jeService)
    {
        _context = context;
        _jeService = jeService;
    }

    public async Task<PaymentResponseDto> Handle(VoidPaymentCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Models.Entities.Payment>()
            .Include(x => x.Customer)
            .Include(x => x.Vendor)
            .Include(x => x.Allocations)
            .Include(x => x.Voucher)
            .Where(x => !x.IsDeleted && x.Id == request.Id)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Payment), request.Id);

        foreach (var alloc in entity.Allocations.Where(a => !a.IsDeleted))
        {
            if (alloc.InvoiceId.HasValue)
            {
                var invoice = await _context.Set<Models.Entities.Invoice>().FindAsync(new object[] { alloc.InvoiceId.Value }, ct);
                if (invoice != null)
                {
                    invoice.PaidAmount = Math.Max(0, invoice.PaidAmount - alloc.AllocatedAmount);
                    invoice.Status = invoice.PaidAmount <= 0 ? InvoiceStatus.Sent : InvoiceStatus.PartiallyPaid;
                    invoice.UpdatedAt = DateTime.UtcNow;
                }
            }
            if (alloc.BillId.HasValue)
            {
                var bill = await _context.Set<Models.Entities.Bill>().FindAsync(new object[] { alloc.BillId.Value }, ct);
                if (bill != null)
                {
                    bill.PaidAmount = Math.Max(0, bill.PaidAmount - alloc.AllocatedAmount);
                    bill.Status = bill.PaidAmount <= 0 ? BillStatus.Received : BillStatus.PartiallyPaid;
                    bill.UpdatedAt = DateTime.UtcNow;
                }
            }
            alloc.IsDeleted = true;
            alloc.DeletedAt = DateTime.UtcNow;
        }

        if (entity.JournalEntryId.HasValue)
            await _jeService.CancelAsync(entity.JournalEntryId.Value);

        // The generated voucher documents the entry just cancelled, so it goes with it.
        if (entity.Voucher is { Status: not VoucherStatus.Cancelled } voucher)
        {
            voucher.Status = VoucherStatus.Cancelled;
            voucher.UpdatedAt = DateTime.UtcNow;
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return PaymentMappingHelper.ToDto(entity);
    }
}

public class GetAllPaymentsForExportQueryHandler : IRequestHandler<GetAllPaymentsForExportQuery, List<PaymentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllPaymentsForExportQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<PaymentResponseDto>> Handle(GetAllPaymentsForExportQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Models.Entities.Payment>()
            .Include(x => x.Customer)
            .Include(x => x.Vendor)
            .Include(x => x.Allocations)
            .Include(x => x.Voucher)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync(ct);
        return items.Select(PaymentMappingHelper.ToDto).ToList();
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
        VoucherId = e.VoucherId,
        VoucherNumber = e.Voucher?.VoucherNumber,
        Allocations = e.Allocations.Where(a => !a.IsDeleted).Select(a => new PaymentAllocationResponseDto
        {
            Id = a.Id,
            InvoiceId = a.InvoiceId,
            BillId = a.BillId,
            AllocatedAmount = a.AllocatedAmount
        }).ToList()
    };
}
