using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Settings.Models.Entities;

[Table("sett_validation_rules")]
public class ValidationRule : BaseEntity
{
    [Required][MaxLength(200)] public string EntityType { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string FieldName { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string RuleType { get; set; } = string.Empty;
    public string? RuleValue { get; set; }
    [MaxLength(500)] public string? ErrorMessage { get; set; }
    [MaxLength(50)] public string Severity { get; set; } = "Error";
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; }
}
