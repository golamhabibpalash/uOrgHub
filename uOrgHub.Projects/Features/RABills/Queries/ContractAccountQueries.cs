using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features._Common;
using uOrgHub.Projects.Features.RABills.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Projects.Features.RABills.Queries;

public record GetContractAccountQuery(Guid ProjectId) : IQuery<ContractAccountDto>;

/// <summary>
/// For AR invoices raised from Projects, a short label of where each came from ("RA Bill #2 (RAB-002)",
/// "Retention release"). Accounts cannot see Projects, so documents like the money receipt ask here.
/// </summary>
public record GetInvoiceSourcesQuery(IReadOnlyCollection<Guid> InvoiceIds) : IQuery<Dictionary<Guid, string>>;

public class GetInvoiceSourcesQueryHandler : IRequestHandler<GetInvoiceSourcesQuery, Dictionary<Guid, string>>
{
    private readonly AppDbContext _context;
    public GetInvoiceSourcesQueryHandler(AppDbContext context) => _context = context;

    public async Task<Dictionary<Guid, string>> Handle(GetInvoiceSourcesQuery request, CancellationToken ct)
    {
        var ids = request.InvoiceIds.Distinct().ToList();
        var result = new Dictionary<Guid, string>();
        if (ids.Count == 0)
            return result;

        var bills = await _context.Set<RABill>()
            .Where(b => !b.IsDeleted && b.InvoiceId != null && ids.Contains(b.InvoiceId.Value))
            .Select(b => new { InvoiceId = b.InvoiceId!.Value, b.BillSequence, b.BillNumber })
            .ToListAsync(ct);
        foreach (var b in bills)
            result.TryAdd(b.InvoiceId, $"RA Bill #{b.BillSequence} ({b.BillNumber})");

        var releases = await _context.Set<RetentionRelease>()
            .Where(r => !r.IsDeleted && ids.Contains(r.InvoiceId))
            .Select(r => r.InvoiceId)
            .ToListAsync(ct);
        foreach (var id in releases)
            result.TryAdd(id, "Retention release");

        return result;
    }
}

/// <summary>
/// Contract → certified → invoiced → received → outstanding for one project, with retention held
/// back. Read from the RA bills and their linked invoices, so it can never drift from Accounts.
/// </summary>
public class GetContractAccountQueryHandler : IRequestHandler<GetContractAccountQuery, ContractAccountDto>
{
    private readonly AppDbContext _context;
    public GetContractAccountQueryHandler(AppDbContext context) => _context = context;

    public async Task<ContractAccountDto> Handle(GetContractAccountQuery request, CancellationToken ct)
    {
        var project = await _context.Set<Project>()
            .Include(p => p.Client).ThenInclude(c => c.Customer)
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var bills = await _context.Set<RABill>()
            .Include(b => b.Items)
            .Where(b => !b.IsDeleted && b.ProjectId == project.Id)
            .OrderBy(b => b.BillSequence)
            .ToListAsync(ct);
        var billDtos = bills.Select(RABillMapper.ToDto).ToList();
        await RABillInvoiceInfo.ApplyAsync(_context, billDtos, ct);
        var certified = billDtos
            .Where(b => b.Status is RABillStatus.Certified or RABillStatus.Paid)
            .ToList();

        var releases = await _context.Set<RetentionRelease>()
            .Include(r => r.Invoice)
            .Where(r => !r.IsDeleted && r.ProjectId == project.Id)
            .OrderBy(r => r.ReleaseDate)
            .ToListAsync(ct);
        var liveReleases = releases.Where(r => !ProjectInvoicing.IsVoid(r.Invoice.Status)).ToList();

        var liveBillInvoices = certified.Where(b => b.InvoiceId.HasValue && b.PaymentState != RABillPaymentStates.InvoiceVoid).ToList();
        var invoiced = liveBillInvoices.Sum(b => b.InvoiceTotal) + liveReleases.Sum(r => r.Invoice.TotalAmount);
        var received = liveBillInvoices.Sum(b => b.InvoicePaid) + liveReleases.Sum(r => r.Invoice.PaidAmount);

        var retentionHeld = certified.Sum(b => b.RetentionAmount);
        var retentionReleased = liveReleases.Sum(r => r.Amount);
        var certifiedGross = certified.Sum(b => b.GrossAmount);

        return new ContractAccountDto
        {
            ProjectId = project.Id,
            ProjectCode = project.ProjectCode,
            ProjectName = project.ProjectName,
            ClientName = project.Client?.CompanyName ?? string.Empty,
            CustomerId = project.Client?.CustomerId,
            CustomerName = project.Client?.Customer?.Name,
            DefaultRevenueAccountId = project.DefaultRevenueAccountId,

            ContractValue = project.ContractValue,
            CertifiedGross = certifiedGross,
            Deductions = certified.Sum(b => b.DeductionAmount),
            NetCertified = certified.Sum(b => b.NetAmount),
            RemainingToBill = project.ContractValue - certifiedGross,

            RetentionHeld = retentionHeld,
            RetentionReleased = retentionReleased,
            RetentionOutstanding = retentionHeld - retentionReleased,

            // Legacy bills paid by hand (no invoice) are settled outside AR, so they're not "to invoice".
            NotYetInvoiced = certified
                .Where(b => b.PaymentState is RABillPaymentStates.NotInvoiced or RABillPaymentStates.InvoiceVoid
                    && b.Status == RABillStatus.Certified)
                .Sum(b => b.NetAmount),
            Invoiced = invoiced,
            Received = received,
            Outstanding = invoiced - received,

            Bills = billDtos,
            RetentionReleases = releases.Select(r => new RetentionReleaseDto
            {
                Id = r.Id,
                ReleaseDate = r.ReleaseDate,
                Amount = r.Amount,
                Notes = r.Notes,
                InvoiceId = r.InvoiceId,
                InvoiceNumber = r.Invoice.InvoiceNumber,
                InvoiceStatus = r.Invoice.Status.ToString(),
                InvoicePaid = r.Invoice.PaidAmount,
            }).ToList(),
        };
    }
}
