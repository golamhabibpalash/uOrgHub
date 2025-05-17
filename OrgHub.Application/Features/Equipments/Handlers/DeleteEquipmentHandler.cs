using MediatR;
using OrgHub.Application.Features.Equipment.CommandQuery;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.Equipments.Handlers;
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