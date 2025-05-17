using MediatR;
using OrgHub.Application.Features.Equipment.DTOs;

namespace OrgHub.Application.Features.Equipment.CommandQuery;
public class CreateEquipmentCommand : IRequest<EquipmentDto>
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime PurchaseDate { get; set; }
}