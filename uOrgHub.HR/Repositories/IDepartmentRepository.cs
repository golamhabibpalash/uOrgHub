using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.HR.Repositories;

public interface IDepartmentRepository : IBaseRepository<Department>
{
    Task<List<Department>> GetAllForDropdownAsync();
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> ParentExistsAsync(Guid? parentId);
    Task<bool> HasCircularReferenceAsync(Guid id, Guid? parentDepartmentId);
    Task<DepartmentDependenciesDto> GetDependenciesAsync(Guid id, CancellationToken ct = default);
}
