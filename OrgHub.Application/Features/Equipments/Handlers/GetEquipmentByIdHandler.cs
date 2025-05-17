using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Equipment.CommandQuery;
using OrgHub.Application.Features.Equipment.DTOs;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.Equipments.Handlers;
public class GetEquipmentByIdHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDto>
{
    private readonly IEquipmentRepository _repository;
    private readonly IMapper _mapper;

    public GetEquipmentByIdHandler(IEquipmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EquipmentDto> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetByIdAsync(request.Id);
        return equipment == null ? null! : _mapper.Map<EquipmentDto>(equipment);
    }
}