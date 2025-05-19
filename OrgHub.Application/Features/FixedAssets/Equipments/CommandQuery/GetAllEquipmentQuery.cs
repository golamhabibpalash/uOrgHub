using MediatR;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;

namespace OrgHub.Application.Features.FixedAssets.Equipments.CommandQuery;

public class GetAllEquipmentQuery : IRequest<List<EquipmentDto>> { }