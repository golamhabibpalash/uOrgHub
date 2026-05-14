using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Accounts.Repositories;

public interface IAccountGroupRepository : IBaseRepository<AccountGroup>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
}