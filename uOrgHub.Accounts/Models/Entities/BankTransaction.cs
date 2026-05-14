using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_bank_transactions")]
public class BankTransaction : BaseEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;

    public BankTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [MaxLength(50)]  public string? ReferenceNumber { get; set; }
    [MaxLength(500)] public string? ChequeNumber { get; set; }
    [Required][MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(200)] public string? Payee { get; set; }
    public bool IsReconciled { get; set; } = false;

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}
