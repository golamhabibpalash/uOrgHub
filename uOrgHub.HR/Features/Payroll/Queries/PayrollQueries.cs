using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Payroll.Queries;

public record GetSalaryGradesQuery(PaginationRequest Request) : IQuery<PagedResult<SalaryGradeResponseDto>>;
public record GetAllSalaryGradesQuery : IQuery<List<SalaryGradeResponseDto>>;
public record GetSalaryComponentsQuery(PaginationRequest Request) : IQuery<PagedResult<SalaryComponentResponseDto>>;
public record GetAllSalaryComponentsQuery : IQuery<List<SalaryComponentResponseDto>>;
public record GetPayrollCyclesQuery(PaginationRequest Request) : IQuery<PagedResult<PayrollCycleResponseDto>>;
public record GetAllPayrollCyclesQuery : IQuery<List<PayrollCycleResponseDto>>;
public record GetPayrollEntriesQuery(Guid PayrollCycleId, PaginationRequest Request) : IQuery<PagedResult<PayrollEntryResponseDto>>;
public record GetAllPayrollEntriesQuery(Guid PayrollCycleId) : IQuery<List<PayrollEntryResponseDto>>;
public record GetExpenseRequestsQuery(PaginationRequest Request, Guid? EmployeeId = null) : IQuery<PagedResult<ExpenseRequestResponseDto>>;
public record GetAllExpenseRequestsQuery : IQuery<List<ExpenseRequestResponseDto>>;

public class GetSalaryGradesQueryHandler : IRequestHandler<GetSalaryGradesQuery, PagedResult<SalaryGradeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetSalaryGradesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<SalaryGradeResponseDto>> Handle(GetSalaryGradesQuery request, CancellationToken ct)
    {
        var query = _context.Set<SalaryGrade>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name, x => x.GradeCode);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.GradeCode)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<SalaryGradeResponseDto>
        {
            Items = items.Select(e => new SalaryGradeResponseDto
            {
                Id = e.Id, GradeCode = e.GradeCode, Name = e.Name,
                MinSalary = e.MinSalary, MaxSalary = e.MaxSalary,
                Description = e.Description, IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllSalaryGradesQueryHandler : IRequestHandler<GetAllSalaryGradesQuery, List<SalaryGradeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllSalaryGradesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<SalaryGradeResponseDto>> Handle(GetAllSalaryGradesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<SalaryGrade>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.GradeCode)
            .Select(x => new SalaryGradeResponseDto
            {
                Id = x.Id, GradeCode = x.GradeCode, Name = x.Name,
                MinSalary = x.MinSalary, MaxSalary = x.MaxSalary,
                Description = x.Description, IsActive = x.IsActive, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetSalaryComponentsQueryHandler : IRequestHandler<GetSalaryComponentsQuery, PagedResult<SalaryComponentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetSalaryComponentsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<SalaryComponentResponseDto>> Handle(GetSalaryComponentsQuery request, CancellationToken ct)
    {
        var query = _context.Set<SalaryComponent>().Where(x => !x.IsDeleted);
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<SalaryComponentResponseDto>
        {
            Items = items.Select(e => new SalaryComponentResponseDto
            {
                Id = e.Id, Name = e.Name, Code = e.Code, ComponentType = e.ComponentType,
                CalculationType = e.CalculationType, DefaultValue = e.DefaultValue,
                IsTaxable = e.IsTaxable, IsFixed = e.IsFixed, IsActive = e.IsActive,
                SortOrder = e.SortOrder, Description = e.Description, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllSalaryComponentsQueryHandler : IRequestHandler<GetAllSalaryComponentsQuery, List<SalaryComponentResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllSalaryComponentsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<SalaryComponentResponseDto>> Handle(GetAllSalaryComponentsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<SalaryComponent>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Select(x => new SalaryComponentResponseDto
            {
                Id = x.Id, Name = x.Name, Code = x.Code, ComponentType = x.ComponentType,
                CalculationType = x.CalculationType, DefaultValue = x.DefaultValue,
                IsTaxable = x.IsTaxable, IsFixed = x.IsFixed, IsActive = x.IsActive,
                SortOrder = x.SortOrder, Description = x.Description, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetPayrollCyclesQueryHandler : IRequestHandler<GetPayrollCyclesQuery, PagedResult<PayrollCycleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPayrollCyclesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<PayrollCycleResponseDto>> Handle(GetPayrollCyclesQuery request, CancellationToken ct)
    {
        var query = _context.Set<PayrollCycle>().Where(x => !x.IsDeleted);
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<PayrollCycleResponseDto>
        {
            Items = items.Select(e => new PayrollCycleResponseDto
            {
                Id = e.Id, Year = e.Year, Month = e.Month, Title = e.Title,
                StartDate = e.StartDate, EndDate = e.EndDate, ProcessedDate = e.ProcessedDate,
                Status = e.Status, TotalBasic = e.TotalBasic, TotalAllowances = e.TotalAllowances,
                TotalDeductions = e.TotalDeductions, TotalNetPay = e.TotalNetPay,
                TotalEmployees = e.TotalEmployees, Remarks = e.Remarks, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllPayrollCyclesQueryHandler : IRequestHandler<GetAllPayrollCyclesQuery, List<PayrollCycleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllPayrollCyclesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<PayrollCycleResponseDto>> Handle(GetAllPayrollCyclesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<PayrollCycle>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Select(x => new PayrollCycleResponseDto
            {
                Id = x.Id, Year = x.Year, Month = x.Month, Title = x.Title,
                StartDate = x.StartDate, EndDate = x.EndDate, ProcessedDate = x.ProcessedDate,
                Status = x.Status, TotalBasic = x.TotalBasic, TotalAllowances = x.TotalAllowances,
                TotalDeductions = x.TotalDeductions, TotalNetPay = x.TotalNetPay,
                TotalEmployees = x.TotalEmployees, Remarks = x.Remarks, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetPayrollEntriesQueryHandler : IRequestHandler<GetPayrollEntriesQuery, PagedResult<PayrollEntryResponseDto>>
{
    private readonly AppDbContext _context;
    public GetPayrollEntriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<PayrollEntryResponseDto>> Handle(GetPayrollEntriesQuery request, CancellationToken ct)
    {
        var query = _context.Set<PayrollEntry>()
            .Include(x => x.Employee)
            .Where(x => !x.IsDeleted && x.PayrollCycleId == request.PayrollCycleId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Employee.EmployeeCode)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<PayrollEntryResponseDto>
        {
            Items = items.Select(e => new PayrollEntryResponseDto
            {
                Id = e.Id, PayrollCycleId = e.PayrollCycleId, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                EmployeeCode = e.Employee?.EmployeeCode ?? string.Empty,
                GrossSalary = e.GrossSalary, BasicSalary = e.BasicSalary,
                TotalAllowances = e.TotalAllowances, TotalDeductions = e.TotalDeductions,
                TaxAmount = e.TaxAmount, NetSalary = e.NetSalary,
                OvertimePay = e.OvertimePay, BonusAmount = e.BonusAmount,
                TotalWorkingDays = e.TotalWorkingDays, PresentDays = e.PresentDays,
                AbsentDays = e.AbsentDays, LeaveDays = e.LeaveDays,
                OvertimeHours = e.OvertimeHours, Status = e.Status, PayslipPath = e.PayslipPath
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllPayrollEntriesQueryHandler : IRequestHandler<GetAllPayrollEntriesQuery, List<PayrollEntryResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllPayrollEntriesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<PayrollEntryResponseDto>> Handle(GetAllPayrollEntriesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<PayrollEntry>()
            .Include(x => x.Employee)
            .Where(x => !x.IsDeleted && x.PayrollCycleId == request.PayrollCycleId)
            .OrderBy(x => x.Employee.EmployeeCode)
            .Select(x => new PayrollEntryResponseDto
            {
                Id = x.Id, PayrollCycleId = x.PayrollCycleId, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : string.Empty,
                GrossSalary = x.GrossSalary, BasicSalary = x.BasicSalary,
                TotalAllowances = x.TotalAllowances, TotalDeductions = x.TotalDeductions,
                TaxAmount = x.TaxAmount, NetSalary = x.NetSalary,
                OvertimePay = x.OvertimePay, BonusAmount = x.BonusAmount,
                TotalWorkingDays = x.TotalWorkingDays, PresentDays = x.PresentDays,
                AbsentDays = x.AbsentDays, LeaveDays = x.LeaveDays,
                OvertimeHours = x.OvertimeHours, Status = x.Status, PayslipPath = x.PayslipPath
            }).ToListAsync(ct);
        return items;
    }
}

public class GetExpenseRequestsQueryHandler : IRequestHandler<GetExpenseRequestsQuery, PagedResult<ExpenseRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetExpenseRequestsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ExpenseRequestResponseDto>> Handle(GetExpenseRequestsQuery request, CancellationToken ct)
    {
        var query = _context.Set<ExpenseRequest>()
            .Include(x => x.Employee).Include(x => x.Approver)
            .Where(x => !x.IsDeleted);

        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.ExpenseDate)
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<ExpenseRequestResponseDto>
        {
            Items = items.Select(e => new ExpenseRequestResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                Category = e.Category, Amount = e.Amount, ExpenseDate = e.ExpenseDate,
                Description = e.Description, ReceiptFilePath = e.ReceiptFilePath, Status = e.Status,
                ApproverId = e.ApproverId,
                ApproverName = e.Approver != null ? $"{e.Approver.FirstName} {e.Approver.LastName}" : null,
                ApprovedAt = e.ApprovedAt, PaidAt = e.PaidAt, RejectionReason = e.RejectionReason,
                CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllExpenseRequestsQueryHandler : IRequestHandler<GetAllExpenseRequestsQuery, List<ExpenseRequestResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllExpenseRequestsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ExpenseRequestResponseDto>> Handle(GetAllExpenseRequestsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<ExpenseRequest>()
            .Include(x => x.Employee).Include(x => x.Approver)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ExpenseDate)
            .Select(x => new ExpenseRequestResponseDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                Category = x.Category, Amount = x.Amount, ExpenseDate = x.ExpenseDate,
                Description = x.Description, ReceiptFilePath = x.ReceiptFilePath, Status = x.Status,
                ApproverId = x.ApproverId,
                ApproverName = x.Approver != null ? $"{x.Approver.FirstName} {x.Approver.LastName}" : null,
                ApprovedAt = x.ApprovedAt, PaidAt = x.PaidAt, RejectionReason = x.RejectionReason,
                CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}
