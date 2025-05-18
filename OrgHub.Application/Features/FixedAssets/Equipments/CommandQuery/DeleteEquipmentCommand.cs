using MediatR;

namespace OrgHub.Application.Features.Equipments.CommandQuery;
public class DeleteEquipmentCommand : IRequest
{
    public int Id { get; set; }
    public DeleteEquipmentCommand(int id) => Id = id;
}