using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.Payroll.Commands;

public record CreateSalaryGradeCommand(CreateSalaryGradeDto Dto) : ICommand<SalaryGradeResponseDto>;
public record UpdateSalaryGradeCommand(Guid Id, UpdateSalaryGradeDto Dto) : ICommand<SalaryGradeResponseDto>;
public record DeleteSalaryGradeCommand(Guid Id) : ICommand<Unit>;
public record CreateSalaryComponentCommand(CreateSalaryComponentDto Dto) : ICommand<SalaryComponentResponseDto>;
public record UpdateSalaryComponentCommand(Guid Id, UpdateSalaryComponentDto Dto) : ICommand<SalaryComponentResponseDto>;
public record DeleteSalaryComponentCommand(Guid Id) : ICommand<Unit>;
public record CreatePayrollCycleCommand(CreatePayrollCycleDto Dto) : ICommand<PayrollCycleResponseDto>;
public record UpdatePayrollCycleCommand(Guid Id, UpdatePayrollCycleDto Dto) : ICommand<PayrollCycleResponseDto>;
public record DeletePayrollCycleCommand(Guid Id) : ICommand<Unit>;
public record CreateOvertimeRuleCommand(CreateOvertimeRuleDto Dto) : ICommand<OvertimeRuleResponseDto>;
public record CreateExpenseRequestCommand(CreateExpenseRequestDto Dto) : ICommand<ExpenseRequestResponseDto>;
public record ApproveExpenseRequestCommand(Guid Id, ApproveExpenseDto Dto) : ICommand<ExpenseRequestResponseDto>;

public class CreateSalaryGradeCommandHandler : IRequestHandler<CreateSalaryGradeCommand, SalaryGradeResponseDto>
{
    private readonly AppDbContext _context;
    public CreateSalaryGradeCommandHandler(AppDbContext context) => _context = context;

    public async Task<SalaryGradeResponseDto> Handle(CreateSalaryGradeCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<SalaryGrade>().AnyAsync(x => !x.IsDeleted && x.GradeCode == request.Dto.GradeCode, ct);
        if (exists) throw new AppException($"Salary grade code '{request.Dto.GradeCode}' already exists.");

        var entity = new SalaryGrade
        {
            GradeCode = request.Dto.GradeCode, Name = request.Dto.Name,
            MinSalary = request.Dto.MinSalary, MaxSalary = request.Dto.MaxSalary,
            Description = request.Dto.Description, IsActive = request.Dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<SalaryGrade>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return new SalaryGradeResponseDto
        {
            Id = entity.Id, GradeCode = entity.GradeCode, Name = entity.Name,
            MinSalary = entity.MinSalary, MaxSalary = entity.MaxSalary,
            Description = entity.Description, IsActive = entity.IsActive, CreatedAt = entity.CreatedAt
        };
    }
}

public class UpdateSalaryGradeCommandHandler : IRequestHandler<UpdateSalaryGradeCommand, SalaryGradeResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateSalaryGradeCommandHandler(AppDbContext context) => _context = context;

    public async Task<SalaryGradeResponseDto> Handle(UpdateSalaryGradeCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SalaryGrade>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SalaryGrade), request.Id);

        var codeTaken = await _context.Set<SalaryGrade>()
            .AnyAsync(x => !x.IsDeleted && x.Id != request.Id && x.GradeCode == request.Dto.GradeCode, ct);
        if (codeTaken) throw new AppException($"Salary grade code '{request.Dto.GradeCode}' already exists.");

        entity.GradeCode = request.Dto.GradeCode;
        entity.Name = request.Dto.Name;
        entity.MinSalary = request.Dto.MinSalary;
        entity.MaxSalary = request.Dto.MaxSalary;
        entity.Description = request.Dto.Description;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<SalaryGrade>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return new SalaryGradeResponseDto
        {
            Id = entity.Id, GradeCode = entity.GradeCode, Name = entity.Name,
            MinSalary = entity.MinSalary, MaxSalary = entity.MaxSalary,
            Description = entity.Description, IsActive = entity.IsActive, CreatedAt = entity.CreatedAt
        };
    }
}

public class DeleteSalaryGradeCommandHandler : IRequestHandler<DeleteSalaryGradeCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteSalaryGradeCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteSalaryGradeCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SalaryGrade>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SalaryGrade), request.Id);

        var usedByDesignation = await _context.Set<Designation>()
            .AnyAsync(x => !x.IsDeleted && x.SalaryGradeId == request.Id, ct);
        var usedByEmployee = await _context.Set<Employee>()
            .AnyAsync(x => !x.IsDeleted && x.SalaryGradeId == request.Id, ct);
        var usedBySalaryStructure = await _context.Set<EmployeeSalaryStructure>()
            .AnyAsync(x => !x.IsDeleted && x.SalaryGradeId == request.Id, ct);
        if (usedByDesignation || usedByEmployee || usedBySalaryStructure)
            throw new AppException("This salary grade is assigned to designations or employees and cannot be deleted.", 409);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class CreateSalaryComponentCommandHandler : IRequestHandler<CreateSalaryComponentCommand, SalaryComponentResponseDto>
{
    private readonly AppDbContext _context;
    public CreateSalaryComponentCommandHandler(AppDbContext context) => _context = context;

    public async Task<SalaryComponentResponseDto> Handle(CreateSalaryComponentCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<SalaryComponent>().AnyAsync(x => !x.IsDeleted && x.Code == request.Dto.Code, ct);
        if (exists) throw new AppException($"Salary component code '{request.Dto.Code}' already exists.");

        var entity = new SalaryComponent
        {
            Name = request.Dto.Name, Code = request.Dto.Code,
            ComponentType = request.Dto.ComponentType, CalculationType = request.Dto.CalculationType,
            DefaultValue = request.Dto.DefaultValue, IsTaxable = request.Dto.IsTaxable,
            IsFixed = request.Dto.IsFixed, IsActive = request.Dto.IsActive,
            SortOrder = request.Dto.SortOrder, Description = request.Dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<SalaryComponent>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return new SalaryComponentResponseDto
        {
            Id = entity.Id, Name = entity.Name, Code = entity.Code,
            ComponentType = entity.ComponentType, CalculationType = entity.CalculationType,
            DefaultValue = entity.DefaultValue, IsTaxable = entity.IsTaxable,
            IsFixed = entity.IsFixed, IsActive = entity.IsActive,
            SortOrder = entity.SortOrder, Description = entity.Description, CreatedAt = entity.CreatedAt
        };
    }
}

public class UpdateSalaryComponentCommandHandler : IRequestHandler<UpdateSalaryComponentCommand, SalaryComponentResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateSalaryComponentCommandHandler(AppDbContext context) => _context = context;

    public async Task<SalaryComponentResponseDto> Handle(UpdateSalaryComponentCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SalaryComponent>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SalaryComponent), request.Id);

        var codeTaken = await _context.Set<SalaryComponent>()
            .AnyAsync(x => !x.IsDeleted && x.Id != request.Id && x.Code == request.Dto.Code, ct);
        if (codeTaken) throw new AppException($"Salary component code '{request.Dto.Code}' already exists.");

        entity.Name = request.Dto.Name;
        entity.Code = request.Dto.Code;
        entity.ComponentType = request.Dto.ComponentType;
        entity.CalculationType = request.Dto.CalculationType;
        entity.DefaultValue = request.Dto.DefaultValue;
        entity.IsTaxable = request.Dto.IsTaxable;
        entity.IsFixed = request.Dto.IsFixed;
        entity.IsActive = request.Dto.IsActive;
        entity.SortOrder = request.Dto.SortOrder;
        entity.Description = request.Dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<SalaryComponent>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return new SalaryComponentResponseDto
        {
            Id = entity.Id, Name = entity.Name, Code = entity.Code,
            ComponentType = entity.ComponentType, CalculationType = entity.CalculationType,
            DefaultValue = entity.DefaultValue, IsTaxable = entity.IsTaxable,
            IsFixed = entity.IsFixed, IsActive = entity.IsActive,
            SortOrder = entity.SortOrder, Description = entity.Description, CreatedAt = entity.CreatedAt
        };
    }
}

public class DeleteSalaryComponentCommandHandler : IRequestHandler<DeleteSalaryComponentCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeleteSalaryComponentCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeleteSalaryComponentCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<SalaryComponent>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(SalaryComponent), request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class CreatePayrollCycleCommandHandler : IRequestHandler<CreatePayrollCycleCommand, PayrollCycleResponseDto>
{
    private readonly AppDbContext _context;
    public CreatePayrollCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<PayrollCycleResponseDto> Handle(CreatePayrollCycleCommand request, CancellationToken ct)
    {
        var existing = await _context.Set<PayrollCycle>()
            .FirstOrDefaultAsync(x => x.Year == request.Dto.Year && x.Month == request.Dto.Month, ct);
        if (existing is { IsDeleted: false })
            throw new AppException($"Payroll cycle for {request.Dto.Year}/{request.Dto.Month:D2} already exists.");

        // (Year, Month) is unique even across soft-deleted cycles, so a deleted month is reused
        // as a fresh draft rather than inserted again.
        var entity = existing ?? new PayrollCycle { CreatedAt = DateTime.UtcNow };
        entity.Year = request.Dto.Year; entity.Month = request.Dto.Month; entity.Title = request.Dto.Title;
        entity.StartDate = request.Dto.StartDate; entity.EndDate = request.Dto.EndDate;
        entity.Status = PayrollStatus.Draft; entity.ProcessedDate = null; entity.Remarks = null;
        entity.TotalBasic = 0; entity.TotalAllowances = 0; entity.TotalDeductions = 0;
        entity.TotalNetPay = 0; entity.TotalEmployees = 0;
        if (existing != null)
        {
            entity.IsDeleted = false; entity.DeletedAt = null; entity.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Set<PayrollCycle>().Add(entity);
        }
        await _context.SaveChangesAsync(ct);
        return PayrollMappingHelper.MapCycleToDto(entity);
    }
}

public class UpdatePayrollCycleCommandHandler : IRequestHandler<UpdatePayrollCycleCommand, PayrollCycleResponseDto>
{
    private readonly AppDbContext _context;
    public UpdatePayrollCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<PayrollCycleResponseDto> Handle(UpdatePayrollCycleCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<PayrollCycle>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PayrollCycle), request.Id);

        var to = request.Dto.Status;
        if (to != entity.Status)
        {
            if (to == PayrollStatus.Processed && entity.Status == PayrollStatus.Draft)
                throw new AppException("Use Process to calculate the payroll; that marks the cycle Processed.");
            if (!PayrollCycleTransitions.IsAllowed(entity.Status, to))
                throw new AppException($"A payroll cycle can't move from {entity.Status} to {to}.");

            entity.Status = to;
            // Entries follow the cycle (a reopened draft keeps its figures until re-processed).
            var entries = await _context.Set<PayrollEntry>()
                .Where(x => !x.IsDeleted && x.PayrollCycleId == entity.Id)
                .ToListAsync(ct);
            foreach (var entry in entries) { entry.Status = to; entry.UpdatedAt = DateTime.UtcNow; }
        }
        entity.Remarks = request.Dto.Remarks;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<PayrollCycle>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return PayrollMappingHelper.MapCycleToDto(entity);
    }
}

public class DeletePayrollCycleCommandHandler : IRequestHandler<DeletePayrollCycleCommand, Unit>
{
    private readonly AppDbContext _context;
    public DeletePayrollCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(DeletePayrollCycleCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<PayrollCycle>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(PayrollCycle), request.Id);
        if (entity.Status is not (PayrollStatus.Draft or PayrollStatus.Cancelled))
            throw new AppException($"A {entity.Status} payroll can't be deleted. Cancel it first.");

        var entries = await _context.Set<PayrollEntry>()
            .Include(x => x.Lines)
            .Where(x => !x.IsDeleted && x.PayrollCycleId == entity.Id)
            .ToListAsync(ct);
        foreach (var entry in entries)
        {
            entry.IsDeleted = true; entry.DeletedAt = DateTime.UtcNow;
            foreach (var line in entry.Lines) { line.IsDeleted = true; line.DeletedAt = DateTime.UtcNow; }
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

/// <summary>
/// Payroll cycle lifecycle: Draft → (Process) → Processed → Approved → Paid. Processed/Approved can
/// step back for corrections; Paid is final. Draft/Processed/Approved can be cancelled, and a
/// cancelled cycle can be reopened as a draft.
/// </summary>
public static class PayrollCycleTransitions
{
    private static readonly Dictionary<PayrollStatus, PayrollStatus[]> Allowed = new()
    {
        [PayrollStatus.Draft] = [PayrollStatus.Cancelled],
        [PayrollStatus.Processed] = [PayrollStatus.Approved, PayrollStatus.Draft, PayrollStatus.Cancelled],
        [PayrollStatus.Approved] = [PayrollStatus.Paid, PayrollStatus.Processed, PayrollStatus.Cancelled],
        [PayrollStatus.Cancelled] = [PayrollStatus.Draft],
    };

    public static bool IsAllowed(PayrollStatus from, PayrollStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}

public class CreateOvertimeRuleCommandHandler : IRequestHandler<CreateOvertimeRuleCommand, OvertimeRuleResponseDto>
{
    private readonly AppDbContext _context;
    public CreateOvertimeRuleCommandHandler(AppDbContext context) => _context = context;

    public async Task<OvertimeRuleResponseDto> Handle(CreateOvertimeRuleCommand request, CancellationToken ct)
    {
        var entity = new OvertimeRule
        {
            Name = request.Dto.Name, CalculationType = request.Dto.CalculationType,
            Multiplier = request.Dto.Multiplier, MaxHoursPerMonth = request.Dto.MaxHoursPerMonth,
            AppliesWeekends = request.Dto.AppliesWeekends, IsActive = request.Dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<OvertimeRule>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return new OvertimeRuleResponseDto
        {
            Id = entity.Id, Name = entity.Name, CalculationType = entity.CalculationType,
            Multiplier = entity.Multiplier, MaxHoursPerMonth = entity.MaxHoursPerMonth,
            AppliesWeekends = entity.AppliesWeekends, IsActive = entity.IsActive, CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateExpenseRequestCommandHandler : IRequestHandler<CreateExpenseRequestCommand, ExpenseRequestResponseDto>
{
    private readonly AppDbContext _context;
    public CreateExpenseRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<ExpenseRequestResponseDto> Handle(CreateExpenseRequestCommand request, CancellationToken ct)
    {
        var entity = new ExpenseRequest
        {
            EmployeeId = request.Dto.EmployeeId, Category = request.Dto.Category,
            Amount = request.Dto.Amount, ExpenseDate = request.Dto.ExpenseDate,
            Description = request.Dto.Description, ReceiptFilePath = request.Dto.ReceiptFilePath,
            Status = ExpenseStatus.Draft, CreatedAt = DateTime.UtcNow
        };
        _context.Set<ExpenseRequest>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        return PayrollMappingHelper.MapExpenseToDto(entity, employee, null);
    }
}

public class ApproveExpenseRequestCommandHandler : IRequestHandler<ApproveExpenseRequestCommand, ExpenseRequestResponseDto>
{
    private readonly AppDbContext _context;
    public ApproveExpenseRequestCommandHandler(AppDbContext context) => _context = context;

    public async Task<ExpenseRequestResponseDto> Handle(ApproveExpenseRequestCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<ExpenseRequest>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ExpenseRequest), request.Id);

        entity.ApproverId = request.Dto.ApproverId;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.Status = request.Dto.IsApproved ? ExpenseStatus.ApprovedByFinance : ExpenseStatus.Rejected;
        entity.RejectionReason = request.Dto.RejectionReason;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<ExpenseRequest>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var approver = entity.ApproverId.HasValue
            ? await _context.Set<Employee>().FindAsync(entity.ApproverId.Value)
            : null;
        return PayrollMappingHelper.MapExpenseToDto(entity, employee, approver);
    }
}

file static class PayrollMappingHelper
{
    internal static PayrollCycleResponseDto MapCycleToDto(PayrollCycle e) => PayrollMapping.CycleToDto(e);

    internal static ExpenseRequestResponseDto MapExpenseToDto(ExpenseRequest e, Employee? emp, Employee? approver) => new()
    {
        Id = e.Id, EmployeeId = e.EmployeeId,
        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : string.Empty,
        Category = e.Category, Amount = e.Amount, ExpenseDate = e.ExpenseDate,
        Description = e.Description, ReceiptFilePath = e.ReceiptFilePath, Status = e.Status,
        ApproverId = e.ApproverId,
        ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null,
        ApprovedAt = e.ApprovedAt, PaidAt = e.PaidAt, RejectionReason = e.RejectionReason,
        CreatedAt = e.CreatedAt
    };
}
