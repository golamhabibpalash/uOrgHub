using MediatR;
using OrgHub.Application.Features.Equipments.CommandQuery;
using OrgHub.Application.Features.Equipments.DTOs;
using OrgHub.Application.Features.Equipments.Interfaces;

namespace OrgHub.Application.Features.Equipments.Handlers;
public class CreateEquipmentHandler : IRequestHandler<CreateEquipmentCommand, EquipmentDto>
{
    private readonly IEquipmentService _service;

    public CreateEquipmentHandler(IEquipmentService services)
    {
        _service = services;
    }

    public async Task<EquipmentDto> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {

        return await _service.AddAsync(request.Equipment);
    }
}