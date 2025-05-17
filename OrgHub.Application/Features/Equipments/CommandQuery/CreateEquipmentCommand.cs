using MediatR;
using OrgHub.Application.Features.Equipments.DTOs;

namespace OrgHub.Application.Features.Equipments.CommandQuery;
public class CreateEquipmentCommand : IRequest<EquipmentDto>
{
    public EquipmentDto Equipment { get; set; } = default!;
}