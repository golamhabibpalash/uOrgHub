using MediatR;
using OrgHub.Application.Features.Equipment.DTOs;

namespace OrgHub.Application.Features.Equipment.CommandQuery;
public class UpdateEquipmentCommand : IRequest<EquipmentDto>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime PurchaseDate { get; set; }
}