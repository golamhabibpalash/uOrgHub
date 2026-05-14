using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_item_variants")]
public class ItemVariant : BaseEntity
{
    [Required] public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    [Required] [MaxLength(50)] public string SKU { get; set; } = string.Empty;
    [Required] [MaxLength(500)] public string VariantName { get; set; } = string.Empty;
    [MaxLength(100)] public string? Barcode { get; set; }
    public decimal CostPrice { get; set; } = 0;
    public decimal SellingPrice { get; set; } = 0;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    [MaxLength(64)] public string? AttributeHash { get; set; }

    public ICollection<VariantAttribute> Attributes { get; set; } = new List<VariantAttribute>();
}
