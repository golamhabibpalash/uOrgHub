using AutoMapper;
using OrgHub.Application.Features.FixedAssets.Equipments.DTOs;
using OrgHub.Domain.Entities.FixedAssets;

namespace OrgHub.Application.Mapping;
public class EquipmentProfile : Profile
{
    public EquipmentProfile()
    {
        CreateMap<Equipment, EquipmentDto>().ReverseMap();
    }
}