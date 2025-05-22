using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrgHub.Domain.Entities.FixedAssets;

public class FixedAssets_Equipment : CommonProps
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string EquipmentCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime PurchaseDate { get; set; }
    public bool IsActive { get; set; }
}