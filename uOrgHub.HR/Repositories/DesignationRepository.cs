using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Repositories;

public class DesignationRepository : IDesignationRepository
{
    private readonly AppDbContext _context;

    public DesignationRepository(AppDbContext context) => _context = context;

    private IQueryable<Designation> BaseQuery()
        => _context.Set<Designation>()
            .Include(x => x.Department)
            .Where(x => !x.IsDeleted);

    public async Task<Designation?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<Designation>> GetAllAsync(PaginationRequest request)
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

        return new PagedResult<Designation> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<Designation> CreateAsync(Designation entity)
    {
        _context.Set<Designation>().Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(x => x.Department).LoadAsync();
        return entity;
    }

    public async Task<Designation> UpdateAsync(Designation entity)
    {
        _context.Set<Designation>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<Designation>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));

    public async Task<DesignationDependenciesDto> GetDependenciesAsync(Guid id, CancellationToken ct = default)
    {
        var employeeCount = await _context.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.DesignationId == id, ct);

        var childCount = await _context.Set<Designation>()
            .CountAsync(d => !d.IsDeleted && d.ParentDesignationId == id, ct);

        var parts = new List<string>();
        if (employeeCount > 0) parts.Add($"{employeeCount} employee{(employeeCount == 1 ? "" : "s")}");
        if (childCount > 0) parts.Add($"{childCount} child designation{(childCount == 1 ? "" : "s")}");

        var canDelete = parts.Count == 0;
        var reason = canDelete ? null : $"Cannot delete designation: {string.Join(", ", parts)} assigned.";

        return new DesignationDependenciesDto
        {
            DesignationId = id,
            EmployeeCount = employeeCount,
            ChildDesignationCount = childCount,
            CanDelete = canDelete,
            BlockingReason = reason,
        };
    }

    public async Task<List<Designation>> GetAllForDropdownAsync()
        => await BaseQuery()
            .Include(x => x.ParentDesignation)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<bool> ParentExistsAsync(Guid? parentId)
        => parentId == null || await BaseQuery().AnyAsync(x => x.Id == parentId);

    public async Task<bool> HasCircularReferenceAsync(Guid id, Guid? parentDesignationId)
    {
        if (parentDesignationId == null) return false;
        if (id == parentDesignationId) return true;

        var visited = new HashSet<Guid> { id };
        var current = parentDesignationId.Value;

        while (current != Guid.Empty)
        {
            if (visited.Contains(current)) return true;
            visited.Add(current);

            var next = await _context.Set<Designation>()
                .Where(x => x.Id == current && !x.IsDeleted)
                .Select(x => x.ParentDesignationId)
                .FirstOrDefaultAsync();

            if (next == null) break;
            current = next.Value;
        }

        return false;
    }
}
