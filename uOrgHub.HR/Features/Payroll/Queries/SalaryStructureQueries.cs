using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Payroll.Queries;

/// <param name="CurrentOnly">True lists each employee's structure in force; false includes history.</param>
public record GetSalaryStructuresQuery(PaginationRequest Request, Guid? EmployeeId = null, bool CurrentOnly = true)
    : IQuery<PagedResult<EmployeeSalaryStructureResponseDto>>;

public record GetPayslipQuery(Guid PayrollCycleId, Guid EntryId) : IQuery<PayslipDto>;

public class GetSalaryStructuresQueryHandler : IRequestHandler<GetSalaryStructuresQuery, PagedResult<EmployeeSalaryStructureResponseDto>>
{
    private readonly AppDbContext _context;
    public GetSalaryStructuresQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<EmployeeSalaryStructureResponseDto>> Handle(GetSalaryStructuresQuery request, CancellationToken ct)
    {
        var query = _context.Set<EmployeeSalaryStructure>()
            .Include(x => x.Employee).ThenInclude(x => x.Designation)
            .Include(x => x.SalaryGrade)
            .Include(x => x.Components).ThenInclude(x => x.SalaryComponent)
            .Where(x => !x.IsDeleted && !x.Employee.IsDeleted);
        if (request.CurrentOnly) query = query.Where(x => x.IsActive);
        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Employee.FirstName, x => x.Employee.LastName, x => x.Employee.EmployeeCode);

        var totalCount = await query.CountAsync(ct);
        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["employeeName"] = "Employee.FirstName",
            ["employeeCode"] = "Employee.EmployeeCode",
            ["basicSalary"] = "BasicSalary",
            ["grossSalary"] = "GrossSalary",
            ["effectiveDate"] = "EffectiveDate",
        };
        query = query.ApplySorting(request.Request.SortBy ?? "employeeCode", request.Request.SortDescending, x => x.Id, propertyMappings: mappings);
        var items = await query.AsSplitQuery()
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<EmployeeSalaryStructureResponseDto>
        {
            Items = items.Select(PayrollMapping.StructureToDto).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetPayslipQueryHandler : IRequestHandler<GetPayslipQuery, PayslipDto>
{
    private readonly AppDbContext _context;
    public GetPayslipQueryHandler(AppDbContext context) => _context = context;

    public async Task<PayslipDto> Handle(GetPayslipQuery request, CancellationToken ct)
    {
        var e = await _context.Set<PayrollEntry>()
            .Include(x => x.PayrollCycle)
            .Include(x => x.Employee).ThenInclude(x => x.Designation)
            .Include(x => x.Employee).ThenInclude(x => x.Department)
            .Include(x => x.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.EntryId && x.PayrollCycleId == request.PayrollCycleId, ct)
            ?? throw new NotFoundException(nameof(PayrollEntry), request.EntryId);

        return new PayslipDto
        {
            Id = e.Id, PayrollCycleId = e.PayrollCycleId, EmployeeId = e.EmployeeId,
            EmployeeName = $"{e.Employee.FirstName} {e.Employee.LastName}", EmployeeCode = e.Employee.EmployeeCode,
            DesignationName = e.Employee.Designation?.Name, DepartmentName = e.Employee.Department?.Name,
            JoiningDate = e.Employee.JoiningDate,
            CycleTitle = e.PayrollCycle.Title, PeriodStart = e.PayrollCycle.StartDate, PeriodEnd = e.PayrollCycle.EndDate,
            GrossSalary = e.GrossSalary, BasicSalary = e.BasicSalary,
            TotalAllowances = e.TotalAllowances, TotalDeductions = e.TotalDeductions,
            TaxAmount = e.TaxAmount, NetSalary = e.NetSalary,
            OvertimePay = e.OvertimePay, BonusAmount = e.BonusAmount,
            TotalWorkingDays = e.TotalWorkingDays, PresentDays = e.PresentDays,
            AbsentDays = e.AbsentDays, LeaveDays = e.LeaveDays,
            OvertimeHours = e.OvertimeHours, Status = e.Status, PayslipPath = e.PayslipPath,
            Lines = e.Lines.Where(l => !l.IsDeleted).OrderBy(l => l.LineType).ThenBy(l => l.SortOrder)
                .Select(l => new PayslipLineDto { Code = l.Code, Name = l.Name, LineType = l.LineType, Amount = l.Amount })
                .ToList()
        };
    }
}
