using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_inventory_categories")]
public class InventoryCategory : BaseEntity
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(20)] public string Code { get;set; } = string.Empty;
    public Guid TypeId { get; set; }
    public InventoryType Type { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
    public InventoryCategory? ParentCategory { get; set; }
    public ICollection<InventoryCategory> Children { get; set; } = new List<InventoryCategory>();
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
