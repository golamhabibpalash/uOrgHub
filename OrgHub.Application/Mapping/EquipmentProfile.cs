using AutoMapper;
using OrgHub.Application.Features.Equipment.DTOs;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Mapping;
public class EquipmentProfile : Profile
{
    public EquipmentProfile()
    {
        CreateMap<Equipment, EquipmentDto>();
    }
}