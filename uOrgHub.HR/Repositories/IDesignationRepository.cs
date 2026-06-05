using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.HR.Repositories;

public interface IDesignationRepository : IBaseRepository<Designation>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<DesignationDependenciesDto> GetDependenciesAsync(Guid id, CancellationToken ct = default);
    Task<List<Designation>> GetAllForDropdownAsync();
    Task<bool> ParentExistsAsync(Guid? parentId);
    Task<bool> HasCircularReferenceAsync(Guid id, Guid? parentDesignationId);
}
