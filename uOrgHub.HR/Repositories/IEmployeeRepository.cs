using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.HR.Repositories;

public interface IEmployeeRepository : IBaseRepository<Employee>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
    Task<string> GetNextEmployeeCodeAsync();
}
