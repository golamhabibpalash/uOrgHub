using OrgHub.Domain.Entities.HRM;

namespace OrgHub.Core.Interfaces;

public interface IEmployeeRepository
{
    Task<List<HRM_Employee>> GetAllAsync();
    Task<List<HRM_Employee>> GetAllByInfoAsync(string info);
    Task<HRM_Employee?> GetByIdAsync(int id);
    Task<HRM_Employee> AddAsync(HRM_Employee employee);
    Task UpdateAsync(HRM_Employee employee);
    Task DeleteAsync(int id);
}
