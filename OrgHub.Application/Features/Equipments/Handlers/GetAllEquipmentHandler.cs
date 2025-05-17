using AutoMapper;
using MediatR;
using OrgHub.Application.Features.Equipment.DTOs;
using OrgHub.Application.Features.Equipments.CommandQuery;
using OrgHub.Core.Interfaces;

namespace OrgHub.Application.Features.Equipments.Handlers;
public class GetAllEquipmentHandler : IRequestHandler<GetAllEquipmentQuery, List<EquipmentDto>>
{
    private readonly IEquipmentRepository _repository;
    private readonly IMapper _mapper;

    public GetAllEquipmentHandler(IEquipmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<EquipmentDto>> Handle(GetAllEquipmentQuery request, CancellationToken cancellationToken)
    {
        var equipmentList = await _repository.GetAllAsync();
        return _mapper.Map<List<EquipmentDto>>(equipmentList);
    }
}