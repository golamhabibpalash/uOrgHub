using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class JournalEntryResponseDto
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public JournalEntryStatus Status { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The document that generated this entry — "Voucher", "Bill", "Invoice", "Payment" — or null
    /// when it was written by hand. A generated entry belongs to that document's workflow and
    /// cannot be posted, edited, deleted or cancelled from the Journal Entries screen, so the UI
    /// uses this to disable those actions rather than letting the click fail server-side.
    /// </summary>
    public string? SourceDocumentType { get; set; }
    public string? SourceDocumentNumber { get; set; }
    public string? SourceDocumentStatus { get; set; }

    /// <summary>True when a source document owns this entry. Convenience for the UI.</summary>
    public bool IsSystemGenerated => SourceDocumentType is not null;

    public List<JournalEntryLineResponseDto> Lines { get; set; } = new();
}

public class JournalEntryLineResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int LineOrder { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
}