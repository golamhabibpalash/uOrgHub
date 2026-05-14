using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_attribute_definitions")]
public class AttributeDefinition : BaseEntity
{
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    public AttributeDataType DataType { get; set; }
    public Guid? CategoryId { get; set; }
    public InventoryCategory? Category { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? PredefinedValues { get; set; }
    public bool IsActive { get; set; } = true;
}
