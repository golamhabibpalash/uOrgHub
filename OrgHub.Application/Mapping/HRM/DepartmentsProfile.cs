using AutoMapper;
using OrgHub.Application.Features.HRM.Departments.DTOs;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Mapping.HRM;

public class DepartmentsProfile : Profile
{
    public DepartmentsProfile()
    {
        CreateMap<HRM_Department, HRM_DepartmentDtos>().ReverseMap();
    }
}