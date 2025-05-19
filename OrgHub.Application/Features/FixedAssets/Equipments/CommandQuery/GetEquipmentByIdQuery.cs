using MediatR;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;

namespace OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;

public class GetEquipmentByIdQuery : IRequest<EquipmentDto>
{
    public int Id { get; set; }
    public GetEquipmentByIdQuery(int id) => Id = id;
}