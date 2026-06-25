using AutoMapper;
using OrgHub.Application.Common.Services;
using OrgHub.Application.Features.HRM.Departments.DTOs;
using OrgHub.Application.Features.HRM.Departments.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Features.HRM.Departments.Services;

public class DepartmentService : Service<HRM_Department, HRM_DepartmentDtos>, IDepartmentService
{
    public DepartmentService(IRepository<HRM_Department> repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
