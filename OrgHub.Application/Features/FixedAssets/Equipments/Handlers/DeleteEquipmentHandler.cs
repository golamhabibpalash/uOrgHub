using MediatR;
using OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.FixedAssets.Equipments.Handlers;
public class DeleteEquipmentHandler : IRequestHandler<DeleteEquipmentCommand>
{
    private readonly IEquipmentRepository _repository;

    public DeleteEquipmentHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteEquipmentCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id);
    }
}