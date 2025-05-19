using MediatR;

namespace OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
public class DeleteEquipmentCommand : IRequest
{
    public int Id { get; set; }
    public DeleteEquipmentCommand(int id) => Id = id;
}