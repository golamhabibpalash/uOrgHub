using System.Linq.Expressions;

namespace OrgHub.Core.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
    IQueryable<TEntity> Table();

}
