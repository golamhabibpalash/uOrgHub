using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;
using OrgHub.Domain.Entities.FixedAssets;

namespace OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;

public interface IEquipmentService : IService<Equipment, EquipmentDto>
{
}
