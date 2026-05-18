using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_journalentries")]
public class JournalEntry : BaseEntity
{
    [Required]
    [MaxLength(30)]
    public string EntryNumber { get; set; } = string.Empty;

    public DateTime EntryDate { get; set; }

    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredit { get; set; } = 0;

    [MaxLength(100)]
    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}