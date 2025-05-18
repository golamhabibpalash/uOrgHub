using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.Employees.DTOs;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Employees.Interfaces
{
    public interface IEmployeeService : IService<Employee, EmployeeDto>
    {
        Task<List<EmployeeDto>> GetByInfoAsync(string info);
    }
}
