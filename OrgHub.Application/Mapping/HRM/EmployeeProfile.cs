using AutoMapper;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Mapping.HRM;

public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<HRM_Employee, EmployeeDto>().ReverseMap();
    }
}
