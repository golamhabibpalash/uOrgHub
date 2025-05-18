using MediatR;
using OrgHub.Application.Features.Equipments.DTOs;

namespace OrgHub.Application.Features.Equipments.CommandQuery;

public class GetAllEquipmentQuery : IRequest<List<EquipmentDto>> { }