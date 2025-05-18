using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.Equipments.DTOs;

namespace OrgHub.Application.Features.Equipments.Interfaces;

public interface IEquipmentService : IService<Domain.Entities.Equipment, EquipmentDto>
{
}
