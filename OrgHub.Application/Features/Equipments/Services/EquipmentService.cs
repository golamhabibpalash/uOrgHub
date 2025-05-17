using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.Equipments.DTOs;
using OrgHub.Application.Features.Equipments.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Equipments.Services;

public class EquipmentService : Service<Equipment, EquipmentDto>, IEquipmentService
{
    private readonly IRepository<Equipment> _repository;
    private readonly Func<EquipmentDto, Equipment> _mapToEntity;
    private readonly Func<Equipment, EquipmentDto> _mapToDto;

    public EquipmentService(IRepository<Equipment> repository, Func<EquipmentDto, Equipment> mapToEntity, Func<Equipment, EquipmentDto> mapToDto) : base(repository, mapToEntity, mapToDto)
    {
    }

}