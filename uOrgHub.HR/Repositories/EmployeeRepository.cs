using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context) => _context = context;

    private IQueryable<Employee> BaseQuery()
        => _context.Set<Employee>()
            .Include(x => x.Designation)
            .Include(x => x.Department)
            .Where(x => !x.IsDeleted);

    public async Task<Employee?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<Employee>> GetAllAsync(PaginationRequest request)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x =>
                x.FirstName.Contains(request.Search) ||
                x.LastName.Contains(request.Search) ||
                x.EmployeeCode.Contains(request.Search) ||
                x.Email.Contains(request.Search));

        query = request.SortDescending
            ? query.OrderByDescending(x => x.FirstName)
            : query.OrderBy(x => x.FirstName);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<Employee> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<Employee> CreateAsync(Employee entity)
    {
        _context.Set<Employee>().Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(x => x.Department).LoadAsync();
        await _context.Entry(entity).Reference(x => x.Designation).LoadAsync();
        return entity;
    }

    public async Task<Employee> UpdateAsync(Employee entity)
    {
        _context.Set<Employee>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<Employee>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.EmployeeCode == code && (excludeId == null || x.Id != excludeId));

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Email == email && (excludeId == null || x.Id != excludeId));

    public async Task<string> GetNextEmployeeCodeAsync()
    {
        var maxCode = await _context.Set<Employee>()
            .IgnoreQueryFilters()
            .Where(x => x.EmployeeCode.StartsWith("EMP-"))
            .MaxAsync(x => x.EmployeeCode);

        if (maxCode is null) return "EMP-00001";

        var parts = maxCode.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var num))
            return "EMP-00001";

        return $"EMP-{(num + 1):D5}";
    }

    public async Task<EmployeeDependenciesDto> GetDependenciesAsync(Guid id, CancellationToken ct = default)
    {
        var hasUser = await _context.Set<ApplicationUser>()
            .AnyAsync(u => u.EmployeeId == id && !u.IsDeleted, ct);

        var leaves = await _context.Set<LeaveRequest>()
            .CountAsync(l => !l.IsDeleted && l.EmployeeId == id, ct);

        var attendance = await _context.Set<AttendanceLog>()
            .CountAsync(a => !a.IsDeleted && a.EmployeeId == id, ct);

        var payrollEntries = await _context.Set<PayrollEntry>()
            .CountAsync(p => !p.IsDeleted && p.EmployeeId == id, ct);

        var assets = await _context.Set<AssetAssignment>()
            .CountAsync(a => !a.IsDeleted && a.EmployeeId == id, ct);

        var trainings = await _context.Set<EmployeeTraining>()
            .CountAsync(t => !t.IsDeleted && t.EmployeeId == id, ct);

        var expenses = await _context.Set<ExpenseRequest>()
            .CountAsync(e => !e.IsDeleted && e.EmployeeId == id, ct);

        var directReports = await _context.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.ManagerId == id, ct);

        var parts = new List<string>();
        if (hasUser) parts.Add("a linked user account (deactivate instead)");
        if (leaves > 0) parts.Add($"{leaves} leave request{(leaves == 1 ? "" : "s")}");
        if (attendance > 0) parts.Add($"{attendance} attendance record{(attendance == 1 ? "" : "s")}");
        if (payrollEntries > 0) parts.Add($"{payrollEntries} payroll entr{(payrollEntries == 1 ? "y" : "ies")}");
        if (assets > 0) parts.Add($"{assets} asset assignment{(assets == 1 ? "" : "s")}");
        if (trainings > 0) parts.Add($"{trainings} training enrollment{(trainings == 1 ? "" : "s")}");
        if (expenses > 0) parts.Add($"{expenses} expense request{(expenses == 1 ? "" : "s")}");
        if (directReports > 0) parts.Add($"{directReports} direct report{(directReports == 1 ? "" : "s")}");

        var canDelete = parts.Count == 0;
        var reason = canDelete ? null : $"Cannot delete employee: {string.Join(", ", parts)} exist.";

        return new EmployeeDependenciesDto
        {
            EmployeeId = id,
            HasUserAccount = hasUser,
            LeaveRequests = leaves,
            AttendanceLogs = attendance,
            PayrollEntries = payrollEntries,
            AssetAssignments = assets,
            TrainingEnrollments = trainings,
            ExpenseRequests = expenses,
            DirectReports = directReports,
            CanDelete = canDelete,
            BlockingReason = reason,
        };
    }
}
