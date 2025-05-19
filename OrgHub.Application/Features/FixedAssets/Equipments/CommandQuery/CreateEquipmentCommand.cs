using MediatR;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;

namespace OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
public class CreateEquipmentCommand : IRequest<EquipmentDto>
{
    public EquipmentDto Equipment { get; set; } = default!;
}