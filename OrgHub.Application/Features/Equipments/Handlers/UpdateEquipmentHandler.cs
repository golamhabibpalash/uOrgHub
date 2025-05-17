using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Equipment.CommandQuery;
using OrgHub.Application.Features.Equipment.DTOs;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.Equipments.Handlers;
public class UpdateEquipmentHandler : IRequestHandler<UpdateEquipmentCommand, EquipmentDto>
{
    private readonly IEquipmentRepository _repository;
    private readonly IMapper _mapper;

    public UpdateEquipmentHandler(IEquipmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EquipmentDto> Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetByIdAsync(request.Id);
        if (equipment == null) return null!;

        equipment.Name = request.Name;
        equipment.Type = request.Type;
        equipment.Status = request.Status;
        equipment.PurchaseDate = request.PurchaseDate;

        await _repository.UpdateAsync(equipment);
        return _mapper.Map<EquipmentDto>(equipment);
    }
}