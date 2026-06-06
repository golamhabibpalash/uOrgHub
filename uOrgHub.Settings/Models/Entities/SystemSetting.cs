using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Settings.Models.Entities;

[Table("sett_settings")]
public class SystemSetting : BaseEntity
{
    [Required][MaxLength(100)] public string Category { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Key { get; set; } = string.Empty;
    [Required] public string Value { get; set; } = string.Empty;
    [Required][MaxLength(50)] public string DataType { get; set; } = "String";
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } = false;
}
