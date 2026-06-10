using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Accounts.Repositories;

public interface IAccountGroupRepository : IBaseRepository<AccountGroup>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> CustomCodeExistsAsync(string customCode, Guid? excludeId = null);
    Task<string> GetNextCodeAsync(AccountGroupType type, Guid? parentId);
    Task<List<AccountGroup>> GetAllFlatAsync();
    Task<List<Guid>> GetDescendantIdsAsync(Guid id);
    Task<bool> HasChildrenAsync(Guid id);
}