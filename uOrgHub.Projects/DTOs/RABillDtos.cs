using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateRABillDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public Guid SubmittedById { get; set; }
    public decimal RetentionPercent { get; set; }
    public string? Notes { get; set; }
    public List<CreateRABillItemDto> Items { get; set; } = new();
}

public class UpdateRABillDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal RetentionPercent { get; set; }
    public string? Notes { get; set; }
}

public class CertifyRABillDto
{
    public Guid CertifiedById { get; set; }
    public DateTime CertifiedDate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
}

public class CreateRABillItemDto
{
    public Guid? BOQItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal Rate { get; set; }
    public int Sequence { get; set; }
}

public class RABillResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public int BillSequence { get; set; }
    public Guid SubmittedById { get; set; }
    public Guid? CertifiedById { get; set; }
    public DateTime? CertifiedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal RetentionPercent { get; set; }
    public decimal RetentionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PreviousBilledAmount { get; set; }
    public decimal CumulativeBilledAmount { get; set; }
    public RABillStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<RABillItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    /// <summary>Set when certifying takes cumulative billing past the project's contract value.</summary>
    public string? Warning { get; set; }

    // ── AR invoice raised for this bill (filled from the invoice at read time) ──
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? InvoiceStatus { get; set; }
    public decimal InvoiceTotal { get; set; }
    public decimal InvoicePaid { get; set; }
    public decimal InvoiceBalance { get; set; }

    /// <summary>NotInvoiced · Unpaid · PartiallyPaid · Paid · InvoiceVoid — driven by real receipts.</summary>
    public string PaymentState { get; set; } = RABillPaymentStates.NotInvoiced;
}

public static class RABillPaymentStates
{
    public const string NotInvoiced = "NotInvoiced";
    public const string Unpaid = "Unpaid";
    public const string PartiallyPaid = "PartiallyPaid";
    public const string Paid = "Paid";
    public const string InvoiceVoid = "InvoiceVoid";
}

/// <summary>Raise (and post) the AR invoice for a certified RA bill's net amount.</summary>
public class RaiseRABillInvoiceDto
{
    public Guid RevenueAccountId { get; set; }
    /// <summary>Defaults to the bill date plus the customer's payment terms.</summary>
    public DateTime? DueDate { get; set; }
}

public class RaiseRetentionInvoiceDto
{
    public Guid ProjectId { get; set; }
    public decimal Amount { get; set; }
    public Guid RevenueAccountId { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// The client-facing money picture of one contract: what was agreed, certified, invoiced, received
/// and is still owed, plus retention held back. Everything is derived at read time.
/// </summary>
public class ContractAccountDto
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? DefaultRevenueAccountId { get; set; }

    public decimal ContractValue { get; set; }
    public decimal CertifiedGross { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetCertified { get; set; }
    /// <summary>Contract value not yet certified as work done.</summary>
    public decimal RemainingToBill { get; set; }

    public decimal RetentionHeld { get; set; }
    public decimal RetentionReleased { get; set; }
    public decimal RetentionOutstanding { get; set; }

    /// <summary>Certified net not yet on an invoice (bills certified before invoicing was wired, or awaiting it).</summary>
    public decimal NotYetInvoiced { get; set; }
    public decimal Invoiced { get; set; }
    public decimal Received { get; set; }
    public decimal Outstanding { get; set; }

    public List<RABillResponseDto> Bills { get; set; } = new();
    public List<RetentionReleaseDto> RetentionReleases { get; set; } = new();
}

public class RetentionReleaseDto
{
    public Guid Id { get; set; }
    public DateTime ReleaseDate { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public decimal InvoicePaid { get; set; }
}

public class RABillItemResponseDto
{
    public Guid Id { get; set; }
    public Guid RABillId { get; set; }
    public Guid? BOQItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public int Sequence { get; set; }
}
