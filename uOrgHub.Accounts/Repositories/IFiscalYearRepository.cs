using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Accounts.Repositories;

public interface IFiscalYearRepository : IBaseRepository<FiscalYear>
{
    Task<FiscalYear?> GetCurrentAsync();
    Task<bool> SetCurrentAsync(Guid id);
}