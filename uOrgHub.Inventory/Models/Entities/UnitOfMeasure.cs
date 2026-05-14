using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Entities;

[Table("inv_units_of_measure")]
public class UnitOfMeasure : BaseEntity
{
    [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;
    [Required] [MaxLength(10)] public string Abbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
