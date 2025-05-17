using MediatR;

namespace OrgHub.Application.Features.Equipment.CommandQuery;
public class DeleteEquipmentCommand : IRequest
{
    public int Id { get; set; }
    public DeleteEquipmentCommand(int id) => Id = id;
}