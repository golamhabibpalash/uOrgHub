using MediatR;
using OrgHub.Application.Features.Equipment.DTOs;

namespace OrgHub.Application.Features.Equipments.CommandQuery;

public class GetAllEquipmentQuery : IRequest<List<EquipmentDto>> { }