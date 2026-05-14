using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_bill_lines")]
public class BillLine : BaseEntity
{
    public Guid BillId { get; set; }
    public Bill Bill { get; set; } = null!;

    [Required][MaxLength(500)] public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,4)")] public decimal Quantity { get; set; } = 1;
    [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(5,2)")]  public decimal DiscountPercent { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal TaxAmount { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal LineTotal { get; set; }
    public int LineOrder { get; set; }

    public Guid? TaxRateId { get; set; }
    public TaxRate? TaxRate { get; set; }

    public Guid ExpenseAccountId { get; set; }
    public ChartOfAccount ExpenseAccount { get; set; } = null!;

    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
}
