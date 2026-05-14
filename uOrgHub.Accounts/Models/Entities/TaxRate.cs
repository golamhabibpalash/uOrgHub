using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_tax_rates")]
public class TaxRate : BaseEntity
{
    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    public TaxType TaxType { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal Rate { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? TaxAccountId { get; set; }
    public ChartOfAccount? TaxAccount { get; set; }
}
