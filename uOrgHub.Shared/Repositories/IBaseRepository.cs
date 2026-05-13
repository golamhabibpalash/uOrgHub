using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Shared.Repositories;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<PagedResult<T>> GetAllAsync(PaginationRequest request);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}