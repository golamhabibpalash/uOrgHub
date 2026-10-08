using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.Payroll.Commands;

/// <summary>
/// Calculates a payslip for every active employee for the cycle's period and marks the cycle
/// Processed. Safe to re-run until the cycle is approved: entries are recalculated in place.
/// </summary>
public record ProcessPayrollCycleCommand(Guid Id) : ICommand<ProcessPayrollResultDto>;

public class ProcessPayrollCycleCommandHandler : IRequestHandler<ProcessPayrollCycleCommand, ProcessPayrollResultDto>
{
    private static readonly AttendanceStatus[] PresentStatuses =
        [AttendanceStatus.Present, AttendanceStatus.Late, AttendanceStatus.WFH, AttendanceStatus.HalfDay];

    private readonly AppDbContext _context;
    public ProcessPayrollCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<ProcessPayrollResultDto> Handle(ProcessPayrollCycleCommand request, CancellationToken ct)
    {
        var cycle = await _context.Set<PayrollCycle>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PayrollCycle), request.Id);
        if (cycle.Status is not (PayrollStatus.Draft or PayrollStatus.Processed))
            throw new AppException($"A {cycle.Status} payroll can't be processed. Only Draft or Processed cycles can be (re)calculated.");

        var start = cycle.StartDate.Date;
        var end = cycle.EndDate.Date;

        var employees = await _context.Set<Employee>()
            .Where(x => !x.IsDeleted && x.Status == EmployeeStatus.Active && x.JoiningDate <= end)
            .OrderBy(x => x.EmployeeCode)
            .ToListAsync(ct);
        var ids = employees.Select(x => x.Id).ToList();

        var structures = (await _context.Set<EmployeeSalaryStructure>()
                .Include(x => x.Components).ThenInclude(x => x.SalaryComponent)
                .Where(x => !x.IsDeleted && ids.Contains(x.EmployeeId)
                    && x.EffectiveDate <= end && (x.EndDate == null || x.EndDate >= start))
                .AsSplitQuery()
                .ToListAsync(ct))
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EffectiveDate).First());

        var attendance = (await _context.Set<AttendanceLog>()
                .Where(x => !x.IsDeleted && ids.Contains(x.EmployeeId) && x.AttendanceDate >= start && x.AttendanceDate <= end)
                .Select(x => new { x.EmployeeId, x.Status, x.OvertimeHours })
                .ToListAsync(ct))
            .ToLookup(x => x.EmployeeId);

        var leaves = (await _context.Set<LeaveRequest>()
                .Include(x => x.LeaveType)
                .Where(x => !x.IsDeleted && x.Status == LeaveStatus.Approved && ids.Contains(x.EmployeeId)
                    && x.StartDate <= end && x.EndDate >= start)
                .ToListAsync(ct))
            .ToLookup(x => x.EmployeeId);

        var rule = await _context.Set<OvertimeRule>()
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new OvertimePolicy(x.CalculationType, x.Multiplier, x.MaxHoursPerMonth))
            .FirstOrDefaultAsync(ct);

        // (cycle, employee) is unique even across soft-deleted rows, so entries are reused, not re-added.
        var entries = await _context.Set<PayrollEntry>()
            .Include(x => x.Lines)
            .Where(x => x.PayrollCycleId == cycle.Id)
            .ToDictionaryAsync(x => x.EmployeeId, ct);

        var result = new ProcessPayrollResultDto();
        var included = new HashSet<Guid>();

        foreach (var employee in employees)
        {
            structures.TryGetValue(employee.Id, out var structure);
            var basic = structure?.BasicSalary ?? employee.BasicSalary;
            if (basic <= 0)
            {
                result.Skipped.Add($"{employee.FirstName} {employee.LastName} ({employee.EmployeeCode}): no salary structure or basic salary");
                continue;
            }

            var logs = attendance[employee.Id].ToList();
            var (paidLeave, unpaidLeave) = LeaveDaysInPeriod(leaves[employee.Id], start, end);

            var calc = PayrollCalculator.Calculate(new PayrollInput(
                start, end, employee.JoiningDate, basic,
                structure != null ? PayrollMapping.ToStructureComponents(structure) : [],
                PresentDays: logs.Count(l => PresentStatuses.Contains(l.Status)),
                AbsentDays: logs.Count(l => l.Status == AttendanceStatus.Absent),
                PaidLeaveDays: paidLeave, UnpaidLeaveDays: unpaidLeave,
                OvertimeHours: logs.Sum(l => l.OvertimeHours),
                Overtime: rule));

            if (!entries.TryGetValue(employee.Id, out var entry))
            {
                entry = new PayrollEntry { PayrollCycleId = cycle.Id, EmployeeId = employee.Id, CreatedAt = DateTime.UtcNow };
                _context.Set<PayrollEntry>().Add(entry);
                entries[employee.Id] = entry;
            }
            else
            {
                entry.IsDeleted = false; entry.DeletedAt = null; entry.UpdatedAt = DateTime.UtcNow;
                foreach (var old in entry.Lines.Where(l => !l.IsDeleted))
                {
                    old.IsDeleted = true; old.DeletedAt = DateTime.UtcNow;
                }
            }

            entry.BasicSalary = calc.Basic;
            entry.TotalAllowances = calc.Allowances;
            entry.OvertimePay = calc.OvertimePay;
            entry.BonusAmount = 0;
            entry.GrossSalary = calc.Gross;
            entry.TotalDeductions = calc.Deductions;
            entry.TaxAmount = calc.Tax;
            entry.NetSalary = calc.Net;
            entry.TotalWorkingDays = calc.TotalDays;
            entry.PresentDays = logs.Count(l => PresentStatuses.Contains(l.Status));
            entry.AbsentDays = logs.Count(l => l.Status == AttendanceStatus.Absent);
            entry.LeaveDays = (int)Math.Round(paidLeave + unpaidLeave, MidpointRounding.AwayFromZero);
            entry.OvertimeHours = logs.Sum(l => l.OvertimeHours);
            entry.Status = PayrollStatus.Processed;

            // Added through the set, not just the collection: BaseEntity pre-assigns Id, so EF would
            // take a new line reached via an existing entry for an existing row and try to UPDATE it.
            foreach (var line in calc.Lines)
            {
                var row = new PayrollEntryLine
                {
                    PayrollEntry = entry, SalaryComponentId = line.ComponentId, Code = line.Code, Name = line.Name,
                    LineType = line.LineType, Amount = line.Amount, SortOrder = line.SortOrder,
                    CreatedAt = DateTime.UtcNow
                };
                entry.Lines.Add(row);
                _context.Set<PayrollEntryLine>().Add(row);
            }

            included.Add(employee.Id);
        }

        // Employees paid in an earlier run who no longer qualify (left, salary removed).
        foreach (var stale in entries.Values.Where(e => !included.Contains(e.EmployeeId) && !e.IsDeleted))
        {
            stale.IsDeleted = true; stale.DeletedAt = DateTime.UtcNow;
            foreach (var line in stale.Lines) { line.IsDeleted = true; line.DeletedAt = DateTime.UtcNow; }
        }

        var paid = entries.Values.Where(e => included.Contains(e.EmployeeId)).ToList();
        cycle.TotalEmployees = paid.Count;
        cycle.TotalBasic = paid.Sum(e => e.BasicSalary);
        cycle.TotalAllowances = paid.Sum(e => e.TotalAllowances + e.OvertimePay + e.BonusAmount);
        cycle.TotalDeductions = paid.Sum(e => e.TotalDeductions + e.TaxAmount);
        cycle.TotalNetPay = paid.Sum(e => e.NetSalary);
        cycle.Status = PayrollStatus.Processed;
        cycle.ProcessedDate = DateTime.UtcNow;
        cycle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        result.Cycle = PayrollMapping.CycleToDto(cycle);
        result.ProcessedEmployees = paid.Count;
        return result;
    }

    /// <summary>
    /// Approved leave days that fall inside the period, split paid/unpaid by leave type. A request
    /// straddling the period counts its share of TotalDays (which may exclude weekends/half days).
    /// </summary>
    internal static (decimal Paid, decimal Unpaid) LeaveDaysInPeriod(IEnumerable<LeaveRequest> requests, DateTime start, DateTime end)
    {
        decimal paid = 0, unpaid = 0;
        foreach (var r in requests)
        {
            var from = r.StartDate.Date > start ? r.StartDate.Date : start;
            var to = r.EndDate.Date < end ? r.EndDate.Date : end;
            if (to < from) continue;

            var overlap = (to - from).Days + 1;
            var span = (r.EndDate.Date - r.StartDate.Date).Days + 1;
            var days = r.TotalDays > 0 && span > 0 ? Math.Round(r.TotalDays * overlap / span, 1) : overlap;

            if (r.LeaveType?.IsPaidLeave == false) unpaid += days; else paid += days;
        }
        return (paid, unpaid);
    }
}
