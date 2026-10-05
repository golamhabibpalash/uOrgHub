using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.RABills.Commands;

public record RaiseRABillInvoiceCommand(Guid RABillId, RaiseRABillInvoiceDto Dto) : ICommand<RABillResponseDto>;
public record RaiseRetentionInvoiceCommand(RaiseRetentionInvoiceDto Dto) : ICommand<RetentionReleaseDto>;

/// <summary>
/// Turns a certified RA bill into a posted AR invoice for its net amount (gross − deductions −
/// retention), billed to the client's linked customer on the project's cost center. Uses Accounts'
/// own create + post commands, so the ledger posting (Dr Receivable / Cr Revenue) lives in one place.
/// </summary>
public class RaiseRABillInvoiceCommandHandler : IRequestHandler<RaiseRABillInvoiceCommand, RABillResponseDto>
{
    private readonly AppDbContext _context;
    private readonly ISender _sender;

    public RaiseRABillInvoiceCommandHandler(AppDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<RABillResponseDto> Handle(RaiseRABillInvoiceCommand request, CancellationToken ct)
    {
        var bill = await _context.Set<RABill>()
            .Include(x => x.Items)
            .Include(x => x.Invoice)
            .Include(x => x.Project).ThenInclude(p => p.Client)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.RABillId, ct)
            ?? throw new NotFoundException(nameof(RABill), request.RABillId);

        if (bill.Status != RABillStatus.Certified)
            throw new AppException($"Only certified RA bills can be invoiced. {bill.BillNumber} is {bill.Status}.");

        if (bill.Invoice is { IsDeleted: false } existing && !ProjectInvoicing.IsVoid(existing.Status))
            throw new AppException($"{bill.BillNumber} is already invoiced as {existing.InvoiceNumber}.");

        if (bill.NetAmount <= 0)
            throw new AppException($"{bill.BillNumber} has no net amount to invoice.");

        var description = $"RA Bill #{bill.BillSequence} ({bill.BillNumber}) – {bill.Title}, " +
                          $"{bill.PeriodFrom:dd MMM yyyy} to {bill.PeriodTo:dd MMM yyyy}";

        var invoice = await ProjectInvoicing.RaiseAsync(_context, _sender, bill.Project, bill.NetAmount, description,
            request.Dto.RevenueAccountId, bill.BillDate, request.Dto.DueDate,
            $"Net of retention {bill.RetentionAmount:N2} and deductions {bill.DeductionAmount:N2}.", ct);

        bill.InvoiceId = invoice.Id;
        bill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var dto = RABillMapper.ToDto(bill);
        await RABillInvoiceInfo.ApplyAsync(_context, [dto], ct);
        return dto;
    }
}

/// <summary>Invoices part of the retention held on a project's RA bills, capped at what is still held.</summary>
public class RaiseRetentionInvoiceCommandHandler : IRequestHandler<RaiseRetentionInvoiceCommand, RetentionReleaseDto>
{
    private readonly AppDbContext _context;
    private readonly ISender _sender;

    public RaiseRetentionInvoiceCommandHandler(AppDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<RetentionReleaseDto> Handle(RaiseRetentionInvoiceCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var project = await _context.Set<Project>()
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == dto.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), dto.ProjectId);

        if (dto.Amount <= 0)
            throw new AppException("Enter the retention amount to release.");

        var held = await ProjectInvoicing.RetentionHeldAsync(_context, project.Id, ct);
        var released = await ProjectInvoicing.RetentionReleasedAsync(_context, project.Id, ct);
        var available = held - released;
        if (dto.Amount > available)
            throw new AppException($"Only {available:N2} of retention is still held on {project.ProjectCode} " +
                                   $"({held:N2} held, {released:N2} already released).");

        var date = (dto.ReleaseDate ?? DateTime.UtcNow).Date;
        var invoice = await ProjectInvoicing.RaiseAsync(_context, _sender, project, dto.Amount,
            $"Retention release – {project.ProjectName}", dto.RevenueAccountId, date, dto.DueDate, dto.Notes, ct);

        var release = new RetentionRelease
        {
            ProjectId = project.Id,
            InvoiceId = invoice.Id,
            Amount = dto.Amount,
            ReleaseDate = date,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<RetentionRelease>().Add(release);
        await _context.SaveChangesAsync(ct);

        return new RetentionReleaseDto
        {
            Id = release.Id,
            ReleaseDate = release.ReleaseDate,
            Amount = release.Amount,
            Notes = release.Notes,
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceStatus = invoice.Status.ToString(),
            InvoicePaid = invoice.PaidAmount,
        };
    }
}

internal static class ProjectInvoicing
{
    public static bool IsVoid(InvoiceStatus status) => status is InvoiceStatus.Void or InvoiceStatus.Cancelled;

    /// <summary>
    /// Creates and posts one single-line invoice for a project, inside a transaction when the
    /// provider supports one, so a failed post never leaves a stray draft invoice behind.
    /// </summary>
    public static async Task<Invoice> RaiseAsync(
        AppDbContext context, ISender sender, Project project, decimal amount, string description,
        Guid revenueAccountId, DateTime invoiceDate, DateTime? dueDate, string? notes, CancellationToken ct)
    {
        if (revenueAccountId == Guid.Empty)
            throw new AppException("Choose the revenue account to credit.");

        var customerId = project.Client?.CustomerId
            ?? throw new AppException(
                $"Client '{project.Client?.CompanyName}' is not linked to an Accounts customer. " +
                "Open Projects → Clients and use \"Create AR customer\" (or link an existing one) first.");

        var customer = await context.Set<Customer>().FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, ct)
            ?? throw new AppException($"The customer linked to '{project.Client!.CompanyName}' no longer exists. Re-link the client.");

        var date = invoiceDate.Date;
        var fiscalYear = await context.Set<FiscalYear>()
            .Where(f => !f.IsDeleted && f.StartDate <= date && f.EndDate >= date)
            .OrderByDescending(f => f.StartDate)
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException($"No fiscal year covers {date:dd MMM yyyy}.");
        if (fiscalYear.Status == FiscalYearStatus.Closed)
            throw new AppException($"Fiscal year '{fiscalYear.Name}' is closed.");

        var costCenterId = await context.Set<CostCenter>()
            .Where(c => !c.IsDeleted && c.ProjectId == project.Id)
            .OrderBy(c => c.CreatedAt)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        var transaction = context.Database.IsRelational() ? await context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var created = await sender.Send(new CreateInvoiceCommand(new CreateInvoiceDto
            {
                CustomerId = customer.Id,
                FiscalYearId = fiscalYear.Id,
                InvoiceDate = date,
                DueDate = (dueDate ?? date.AddDays(customer.PaymentTermsDays)).Date,
                CostCenterId = costCenterId,
                Notes = notes,
                Lines =
                [
                    new CreateInvoiceLineDto
                    {
                        Description = description.Length > 500 ? description[..500] : description,
                        Quantity = 1,
                        UnitPrice = amount,
                        LineOrder = 1,
                        RevenueAccountId = revenueAccountId,
                        CostCenterId = costCenterId,
                    }
                ]
            }), ct);
            await sender.Send(new PostInvoiceCommand(created.Id), ct);

            project.DefaultRevenueAccountId = revenueAccountId;
            await context.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);

            return await context.Set<Invoice>().FirstAsync(i => i.Id == created.Id, ct);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public static Task<decimal> RetentionHeldAsync(AppDbContext context, Guid projectId, CancellationToken ct)
        => context.Set<RABill>()
            .Where(b => !b.IsDeleted && b.ProjectId == projectId
                && (b.Status == RABillStatus.Certified || b.Status == RABillStatus.Paid))
            .SumAsync(b => b.RetentionAmount, ct);

    /// <summary>Releases whose invoice was voided no longer count — that retention is held again.</summary>
    public static Task<decimal> RetentionReleasedAsync(AppDbContext context, Guid projectId, CancellationToken ct)
        => context.Set<RetentionRelease>()
            .Where(r => !r.IsDeleted && r.ProjectId == projectId
                && r.Invoice.Status != InvoiceStatus.Void && r.Invoice.Status != InvoiceStatus.Cancelled)
            .SumAsync(r => r.Amount, ct);
}

/// <summary>Fills each RA bill's invoice figures and payment state from its linked invoice.</summary>
public static class RABillInvoiceInfo
{
    public static async Task ApplyAsync(AppDbContext context, IReadOnlyCollection<RABillResponseDto> bills, CancellationToken ct)
    {
        var ids = bills.Where(b => b.InvoiceId.HasValue).Select(b => b.InvoiceId!.Value).Distinct().ToList();
        var invoices = ids.Count == 0
            ? new Dictionary<Guid, Invoice>()
            : await context.Set<Invoice>().Where(i => ids.Contains(i.Id)).ToDictionaryAsync(i => i.Id, ct);

        foreach (var b in bills)
        {
            if (b.InvoiceId is not { } id || !invoices.TryGetValue(id, out var inv) || inv.IsDeleted)
            {
                // Bills marked paid by hand before invoicing was wired keep showing as paid.
                b.PaymentState = b.Status == RABillStatus.Paid ? RABillPaymentStates.Paid : RABillPaymentStates.NotInvoiced;
                continue;
            }

            b.InvoiceNumber = inv.InvoiceNumber;
            b.InvoiceStatus = inv.Status.ToString();
            b.InvoiceTotal = inv.TotalAmount;
            b.InvoicePaid = inv.PaidAmount;
            b.InvoiceBalance = inv.TotalAmount - inv.PaidAmount;
            b.PaymentState = ProjectInvoicing.IsVoid(inv.Status) ? RABillPaymentStates.InvoiceVoid
                : inv.PaidAmount >= inv.TotalAmount ? RABillPaymentStates.Paid
                : inv.PaidAmount > 0 ? RABillPaymentStates.PartiallyPaid
                : RABillPaymentStates.Unpaid;
        }
    }
}
