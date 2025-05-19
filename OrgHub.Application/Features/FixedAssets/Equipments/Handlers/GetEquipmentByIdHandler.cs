using MediatR;
using OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;
using OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;

namespace OrgHub.Application.Features.FixedAssets.Equipments.Handlers;
public class GetEquipmentByIdHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDto>
{
    private readonly IEquipmentService _service;

    public GetEquipmentByIdHandler(IEquipmentService service)
    {
        _service = service;
    }

    public async Task<EquipmentDto> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var equipment = await _service.GetByIdAsync(request.Id);
        if (equipment == null)
        {
            throw new KeyNotFoundException($"Equipment with ID {request.Id} was not found.");
        }
        return equipment;
    }
}
