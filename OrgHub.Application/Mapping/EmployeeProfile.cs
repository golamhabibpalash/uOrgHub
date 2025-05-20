using AutoMapper;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Mapping;

public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<Employee, EmployeeDto>().ReverseMap();
    }
}
