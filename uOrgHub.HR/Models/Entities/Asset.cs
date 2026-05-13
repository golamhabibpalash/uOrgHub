using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_assets")]
public class Asset : BaseEntity
{
    [Required][MaxLength(30)]  public string AssetCode { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    public AssetCategory Category { get; set; }
    [MaxLength(100)] public string? Brand { get; set; }
    [MaxLength(100)] public string? Model { get; set; }
    [MaxLength(100)] public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? PurchaseValue { get; set; }
    public AssetCondition Condition { get; set; } = AssetCondition.New;
    public AssetStatus Status { get; set; } = AssetStatus.Available;
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(500)] public string? Location { get; set; }

    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}
