using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.HR.Repositories;

public interface IDesignationRepository : IBaseRepository<Designation>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<DesignationDependenciesDto> GetDependenciesAsync(Guid id, CancellationToken ct = default);
}
