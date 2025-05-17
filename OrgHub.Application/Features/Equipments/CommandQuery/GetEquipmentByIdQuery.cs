using MediatR;
using OrgHub.Application.Features.Equipment.DTOs;

namespace OrgHub.Application.Features.Equipment.CommandQuery;

public class GetEquipmentByIdQuery : IRequest<EquipmentDto>
{
    public int Id { get; set; }
    public GetEquipmentByIdQuery(int id) => Id = id;
}