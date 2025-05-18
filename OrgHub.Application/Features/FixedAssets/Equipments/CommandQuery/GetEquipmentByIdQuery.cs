using MediatR;
using OrgHub.Application.Features.Equipments.DTOs;

namespace OrgHub.Application.Features.Equipments.CommandQuery;

public class GetEquipmentByIdQuery : IRequest<EquipmentDto>
{
    public int Id { get; set; }
    public GetEquipmentByIdQuery(int id) => Id = id;
}