using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_inventory_types")]
public class InventoryType : BaseEntity
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(20)] public string Code { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
