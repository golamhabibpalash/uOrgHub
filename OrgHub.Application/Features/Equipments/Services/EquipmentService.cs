using AutoMapper;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.Equipments.DTOs;
using OrgHub.Application.Features.Equipments.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Equipments.Services;

public class EquipmentService : Service<Equipment, EquipmentDto>, IEquipmentService
{
    private readonly IRepository<Equipment> _repository;
    private readonly IMapper _mapper;

    public EquipmentService(IRepository<Equipment> repository, IMapper mapper) : base(repository, mapper)
    {
    }

}