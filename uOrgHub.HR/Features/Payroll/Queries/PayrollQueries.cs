using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Payroll.Queries;

public record GetSalaryGradesQuery(PaginationRequest Request) : IQuery<PagedResult<SalaryGradeResponseDto>>;
public record GetSalaryComponentsQuery(PaginationRequest Request) : IQuery<PagedResult<SalaryComponentResponseDto>>;
public record GetPayrollCyclesQuery(PaginationRequest Request) : IQuery<PagedResult<PayrollCycleResponseDto>>;
public record GetPayrollEntriesQuery(Guid PayrollCycleId, PaginationRequest Request) : IQuery<PagedResult<PayrollEntryResponseDto>>;
public record GetExpenseRequestsQuery(PaginationRequest Request, Guid? EmployeeId = null) : IQuery<PagedResult<ExpenseRequestResponseDto>>;

public class GetSalaryGradesQueryHandler : IRequestHandler<GetSalaryGradesQuery, PagedResult<SalaryGradeResponseDto>>
{
    private readonly AppDbContext _context;
    public GetSalaryGradesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<SalaryGradeResponseDto>> Handle(GetSalaryGradesQuery request, CancellationToken ct)
    {
        var query = _context.Set<SalaryGrade>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.Where(x => x.Name.Contains(request.Request.Search) || x.GradeCode.Contains(request.Request.Search));

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
