using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_items")]
public class Item : BaseEntity
{
    [Required] [MaxLength(200)] public string BaseName { get; set; } = string.Empty;
    [MaxLength(30)] public string? ItemCode { get; set; }
    public Guid TypeId { get; set; }
    public InventoryType Type { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public InventoryCategory Category { get; set; } = null!;
    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    [MaxLength(100)] public string? Brand { get; set; }
    [MaxLength(100)] public string? Manufacturer { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public decimal ReorderLevel { get; set; } = 0;
    public decimal StandardCost { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<ItemVariant> Variants { get; set; } = new List<ItemVariant>();
}
