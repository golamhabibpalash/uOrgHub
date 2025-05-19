using MediatR;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;

namespace OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
public class UpdateEquipmentCommand : IRequest<EquipmentDto>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime PurchaseDate { get; set; }
}