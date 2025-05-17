using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Equipment.CommandQuery;
using OrgHub.Application.Features.Equipment.DTOs;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Equipments.Handlers;
public class CreateEquipmentHandler : IRequestHandler<CreateEquipmentCommand, EquipmentDto>
{
    private readonly IEquipmentRepository _repository;
    private readonly IMapper _mapper;

    public CreateEquipmentHandler(IEquipmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EquipmentDto> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = new Domain.Entities.Equipment
        {
            EquipmentCode = Guid.NewGuid().ToString(),
            Name = request.Name,
            Type = request.Type,
            Status = request.Status,
            PurchaseDate = request.PurchaseDate,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 1 // Set as needed
        };

        var saved = await _repository.AddAsync(equipment);
        return _mapper.Map<EquipmentDto>(saved);
    }
}