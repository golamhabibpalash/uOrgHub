using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs;
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

    public async Task<List<Department>> GetAllForDropdownAsync()
        => await BaseQuery()
            .Include(x => x.ParentDepartment)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));

    public async Task<bool> ParentExistsAsync(Guid? parentId)
        => parentId == null || await BaseQuery().AnyAsync(x => x.Id == parentId);

    public async Task<bool> HasCircularReferenceAsync(Guid id, Guid? parentDepartmentId)
    {
        if (parentDepartmentId == null) return false;
        if (id == parentDepartmentId) return true;

        var visited = new HashSet<Guid> { id };
        var current = parentDepartmentId.Value;

        while (current != Guid.Empty)
        {
            if (visited.Contains(current)) return true;
            visited.Add(current);

            var next = await _context.Set<Department>()
                .Where(x => x.Id == current && !x.IsDeleted)
                .Select(x => x.ParentDepartmentId)
                .FirstOrDefaultAsync();

            if (next == null) break;
            current = next.Value;
        }

        return false;
    }

    public async Task<DepartmentDependenciesDto> GetDependenciesAsync(Guid id, CancellationToken ct = default)
    {
        var employees = await _context.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.DepartmentId == id, ct);

        var designations = await _context.Set<Designation>()
            .CountAsync(d => !d.IsDeleted && d.DepartmentId == id, ct);

        var children = await _context.Set<Department>()
            .CountAsync(d => !d.IsDeleted && d.ParentDepartmentId == id, ct);

        var jobPostings = await _context.Set<JobPosting>()
            .CountAsync(j => !j.IsDeleted && j.DepartmentId == id, ct);

        var kpis = await _context.Set<KPI>()
            .CountAsync(k => !k.IsDeleted && k.DepartmentId == id, ct);

        var parts = new List<string>();
        if (employees > 0) parts.Add($"{employees} employee{(employees == 1 ? "" : "s")}");
        if (designations > 0) parts.Add($"{designations} designation{(designations == 1 ? "" : "s")}");
        if (children > 0) parts.Add($"{children} sub-department{(children == 1 ? "" : "s")}");
        if (jobPostings > 0) parts.Add($"{jobPostings} job posting{(jobPostings == 1 ? "" : "s")}");
        if (kpis > 0) parts.Add($"{kpis} KPI{(kpis == 1 ? "" : "s")}");

        var canDelete = parts.Count == 0;
        var reason = canDelete
            ? null
            : $"Cannot delete department: {string.Join(", ", parts)} {(parts.Count == 1 ? "is" : "are")} still linked to it.";

        return new DepartmentDependenciesDto
        {
            DepartmentId = id,
            ActiveEmployees = employees,
            ActiveDesignations = designations,
            ActiveChildDepartments = children,
            ActiveJobPostings = jobPostings,
            ActiveKpis = kpis,
            CanDelete = canDelete,
            BlockingReason = reason,
        };
    }
}
