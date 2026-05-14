using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_payment_allocations")]
public class PaymentAllocation : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public Guid? BillId { get; set; }
    public Bill? Bill { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal AllocatedAmount { get; set; }
}
