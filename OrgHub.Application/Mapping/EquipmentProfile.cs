using AutoMapper;
using OrgHub.Application.Features.Equipments.DTOs;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Mapping;
public class EquipmentProfile : Profile
{
    public EquipmentProfile()
    {
        CreateMap<Equipment, EquipmentDto>();
    }
}