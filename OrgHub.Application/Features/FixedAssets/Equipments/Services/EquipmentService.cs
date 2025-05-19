using AutoMapper;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;
using OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.FixedAssets.Equipments.Services;

public class EquipmentService : Service<Equipment, EquipmentDto>, IEquipmentService
{
    public EquipmentService(IRepository<Equipment> repository, IMapper mapper) : base(repository, mapper)
    {
    }

}