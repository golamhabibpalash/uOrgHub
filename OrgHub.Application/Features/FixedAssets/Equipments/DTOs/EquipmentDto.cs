namespace OrgHub.Application.Features.Equipments.DTOs;

public class EquipmentDto
{
    public int Id { get; set; }
    public string EquipmentCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime PurchaseDate { get; set; }
}