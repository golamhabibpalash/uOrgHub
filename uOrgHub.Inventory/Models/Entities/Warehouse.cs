using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_warehouses")]
public class Warehouse : BaseEntity
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(20)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string? Location { get; set; }
    [MaxLength(100)] public string? ContactPerson { get; set; }
    [MaxLength(20)] public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
}
