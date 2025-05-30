using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrgHub.Core.Interfaces;
using OrgHub.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace OrgHub.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet;
    private readonly AppDbContext _context;
    //private readonly ILogger<Repository<TEntity>> _logger;

    public Repository(AppDbContext context, ILogger<Repository<TEntity>> logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        //_logger = logger;
    }

    public virtual async Task<TEntity> GetByIdAsync(int id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                //_logger.LogWarning("Entity of type {EntityType} with ID {Id} not found.", typeof(TEntity).Name, id);
                throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with ID {id} was not found.");
            }

            //_logger.LogInformation("Retrieved entity of type {EntityType} with ID {Id}.", typeof(TEntity).Name, id);
            return entity;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {Id}.", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        try
        {
            var result = await _dbSet.ToListAsync();

            //_logger.LogInformation("Retrieved {Count} entities of type {EntityType}.", result.Count, typeof(TEntity).Name);
            return result;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error retrieving entities of type {EntityType}.", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        //_logger.LogInformation("Querying {EntityType} with predicate.", typeof(TEntity).Name);
        return _dbSet.Where(predicate);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            //_logger.LogInformation("Added new entity of type {EntityType}.", typeof(TEntity).Name);
            return entity;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error adding entity of type {EntityType}.", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            //_logger.LogInformation("Updated entity of type {EntityType}.", typeof(TEntity).Name);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error updating entity of type {EntityType}.", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            //_logger.LogInformation("Deleted entity of type {EntityType}.", typeof(TEntity).Name);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error deleting entity of type {EntityType}.", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            var exists = await _dbSet.FindAsync(id) != null;
            //_logger.LogInformation("Checked existence for entity of type {EntityType} with ID {Id}: {Exists}.", typeof(TEntity).Name, id, exists);
            return exists;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error checking existence of entity of type {EntityType} with ID {Id}.", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            //_logger.LogInformation("Changes saved to database for context {Context}.", nameof(AppDbContext));
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error saving changes for context {Context}.", nameof(AppDbContext));
            throw;
        }
    }

    public IQueryable<TEntity> Table()
    {
        //_logger.LogInformation("Accessed Table<{EntityType}>.", typeof(TEntity).Name);
        return _dbSet;
    }
}
