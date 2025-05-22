using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.HRM.Employees.DTOs;
using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Application.Features.HRM.Employees.Interfaces
{
    public interface IEmployeeService : IService<HRM_Employee, EmployeeDto>
    {
        Task<List<EmployeeDto>> GetByInfoAsync(string info);
    }
}
