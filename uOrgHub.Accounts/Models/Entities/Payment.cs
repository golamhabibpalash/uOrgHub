using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_payments")]
public class Payment : BaseEntity
{
    [Required][MaxLength(30)] public string PaymentNumber { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [MaxLength(50)]  public string? ReferenceNumber { get; set; }
    [MaxLength(50)]  public string? ChequeNumber { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public Guid? BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    public Guid FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
