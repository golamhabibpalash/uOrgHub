using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class UpdateJournalEntryDto
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<UpdateJournalEntryLineDto> Lines { get; set; } = new();
}

public class UpdateJournalEntryLineDto
{
    public Guid? Id { get; set; }
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; } = 0;
    public decimal CreditAmount { get; set; } = 0;
    public int LineOrder { get; set; }
    public Guid? CostCenterId { get; set; }
}