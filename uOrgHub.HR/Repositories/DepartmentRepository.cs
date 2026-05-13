using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context) => _context = context;

    private IQueryable<Department> BaseQuery()
        => _context.Set<Department>().Where(x => !x.IsDeleted);

    public async Task<Department?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<Department>> GetAllAsync(PaginationRequest request)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.Name.Contains(request.Search) || x.Code.Contains(request.Search));

        query = request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<Department> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<Department> CreateAsync(Department entity)
    {
        _context.Set<Department>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Department> UpdateAsync(Department entity)
    {
        _context.Set<Department>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<Department>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));
}
