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
public record CreateSalaryComponentCommand(CreateSalaryComponentDto Dto) : ICommand<SalaryComponentResponseDto>;
public record CreatePayrollCycleCommand(CreatePayrollCycleDto Dto) : ICommand<PayrollCycleResponseDto>;
public record UpdatePayrollCycleCommand(Guid Id, UpdatePayrollCycleDto Dto) : ICommand<PayrollCycleResponseDto>;
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

public class CreatePayrollCycleCommandHandler : IRequestHandler<CreatePayrollCycleCommand, PayrollCycleResponseDto>
{
    private readonly AppDbContext _context;
    public CreatePayrollCycleCommandHandler(AppDbContext context) => _context = context;

    public async Task<PayrollCycleResponseDto> Handle(CreatePayrollCycleCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<PayrollCycle>()
            .AnyAsync(x => !x.IsDeleted && x.Year == request.Dto.Year && x.Month == request.Dto.Month, ct);
        if (exists) throw new AppException($"Payroll cycle for {request.Dto.Year}/{request.Dto.Month:D2} already exists.");

        var entity = new PayrollCycle
        {
            Year = request.Dto.Year, Month = request.Dto.Month, Title = request.Dto.Title,
            StartDate = request.Dto.StartDate, EndDate = request.Dto.EndDate,
            Status = PayrollStatus.Draft, CreatedAt = DateTime.UtcNow
        };
        _context.Set<PayrollCycle>().Add(entity);
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

        entity.Status = request.Dto.Status;
        entity.Remarks = request.Dto.Remarks;
        if (request.Dto.Status == PayrollStatus.Processed) entity.ProcessedDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<PayrollCycle>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return PayrollMappingHelper.MapCycleToDto(entity);
    }
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
    internal static PayrollCycleResponseDto MapCycleToDto(PayrollCycle e) => new()
    {
        Id = e.Id, Year = e.Year, Month = e.Month, Title = e.Title,
        StartDate = e.StartDate, EndDate = e.EndDate, ProcessedDate = e.ProcessedDate,
        Status = e.Status, TotalBasic = e.TotalBasic, TotalAllowances = e.TotalAllowances,
        TotalDeductions = e.TotalDeductions, TotalNetPay = e.TotalNetPay,
        TotalEmployees = e.TotalEmployees, Remarks = e.Remarks, CreatedAt = e.CreatedAt
    };

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
