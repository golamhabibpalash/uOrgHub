using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_ra_bill_items")]
public class RABillItem : BaseEntity
{
    public Guid RABillId { get; set; }
    public RABill RABill { get; set; } = null!;

    public Guid? BOQItemId { get; set; }
    public BOQItem? BOQItem { get; set; }

    [Required][MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(50)]            public string? UnitOfMeasure { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal PreviousQuantity { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal CurrentQuantity { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal TotalQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Rate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }

    public int Sequence { get; set; }
}
