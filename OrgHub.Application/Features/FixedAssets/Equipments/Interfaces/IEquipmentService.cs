using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;

namespace OrgHub.Application.Features.FixedAssets.Equipments.Interfaces;

public interface IEquipmentService : IService<Domain.Entities.Equipment, EquipmentDto>
{
}
