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
/// Starts a new salary structure for an employee from <c>EffectiveDate</c>. The employee's current
/// structure, if any, is closed the day before — past structures stay as history.
/// </summary>
public record CreateEmployeeSalaryStructureCommand(CreateEmployeeSalaryStructureDto Dto) : ICommand<EmployeeSalaryStructureResponseDto>;

/// <summary>Corrects the current structure in place (no new revision).</summary>
public record UpdateEmployeeSalaryStructureCommand(Guid Id, UpdateEmployeeSalaryStructureDto Dto) : ICommand<EmployeeSalaryStructureResponseDto>;

public class CreateEmployeeSalaryStructureCommandHandler : IRequestHandler<CreateEmployeeSalaryStructureCommand, EmployeeSalaryStructureResponseDto>
{
    private readonly AppDbContext _context;
    public CreateEmployeeSalaryStructureCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeSalaryStructureResponseDto> Handle(CreateEmployeeSalaryStructureCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var effectiveDate = dto.EffectiveDate.Date;

        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == dto.EmployeeId, ct)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);
        await SalaryStructureRules.EnsureGradeAsync(_context, dto.SalaryGradeId, ct);
        var components = await SalaryStructureRules.LoadComponentsAsync(_context, dto.Components, ct);

        var current = await _context.Set<EmployeeSalaryStructure>()
            .Where(x => !x.IsDeleted && x.IsActive && x.EmployeeId == dto.EmployeeId)
            .OrderByDescending(x => x.EffectiveDate)
            .FirstOrDefaultAsync(ct);
        if (current != null)
        {
            if (current.EffectiveDate >= effectiveDate)
                throw new AppException($"The current salary structure is effective from {current.EffectiveDate:dd MMM yyyy}. " +
                                       "Edit it, or start the new one on a later date.");
            current.EndDate = effectiveDate.AddDays(-1);
            current.IsActive = false;
            current.UpdatedAt = DateTime.UtcNow;
        }

        var structure = new EmployeeSalaryStructure
        {
            EmployeeId = dto.EmployeeId, SalaryGradeId = dto.SalaryGradeId,
            BasicSalary = dto.BasicSalary, EffectiveDate = effectiveDate,
            IsActive = true, CreatedAt = DateTime.UtcNow
        };
        foreach (var input in dto.Components)
            structure.Components.Add(new EmployeeSalaryComponent
            {
                SalaryComponentId = input.SalaryComponentId, SalaryComponent = components[input.SalaryComponentId],
                Value = input.Value, IsActive = true, CreatedAt = DateTime.UtcNow
            });
        structure.GrossSalary = PayrollCalculator.Monthly(structure.BasicSalary, PayrollMapping.ToStructureComponents(structure)).Gross;

        // Keep the employee record's headline salary in step with the structure in force.
        employee.BasicSalary = structure.BasicSalary;
        employee.SalaryGradeId = structure.SalaryGradeId;

        _context.Set<EmployeeSalaryStructure>().Add(structure);
        await _context.SaveChangesAsync(ct);

        return await SalaryStructureRules.LoadDtoAsync(_context, structure.Id, ct);
    }
}

public class UpdateEmployeeSalaryStructureCommandHandler : IRequestHandler<UpdateEmployeeSalaryStructureCommand, EmployeeSalaryStructureResponseDto>
{
    private readonly AppDbContext _context;
    public UpdateEmployeeSalaryStructureCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeSalaryStructureResponseDto> Handle(UpdateEmployeeSalaryStructureCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var structure = await _context.Set<EmployeeSalaryStructure>()
            .Include(x => x.Employee)
            .Include(x => x.Components)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(EmployeeSalaryStructure), request.Id);
        if (!structure.IsActive)
            throw new AppException("Only the current salary structure can be edited. Past structures are kept as history.");

        await SalaryStructureRules.EnsureGradeAsync(_context, dto.SalaryGradeId, ct);
        var components = await SalaryStructureRules.LoadComponentsAsync(_context, dto.Components, ct);

        structure.SalaryGradeId = dto.SalaryGradeId;
        structure.BasicSalary = dto.BasicSalary;
        structure.UpdatedAt = DateTime.UtcNow;

        // (structure, component) is unique even across soft-deleted rows, so reuse a component's
        // existing row rather than adding a second one.
        var wanted = dto.Components.ToDictionary(x => x.SalaryComponentId, x => x.Value);
        foreach (var row in structure.Components)
        {
            if (wanted.Remove(row.SalaryComponentId, out var value))
            {
                row.Value = value; row.IsActive = true; row.IsDeleted = false; row.DeletedAt = null;
                row.SalaryComponent = components[row.SalaryComponentId];
                row.UpdatedAt = DateTime.UtcNow;
            }
            else if (!row.IsDeleted)
            {
                row.IsDeleted = true; row.DeletedAt = DateTime.UtcNow;
            }
        }
        foreach (var (componentId, value) in wanted)
        {
            // Added through the set: BaseEntity pre-assigns Id, which EF would otherwise read as an existing row.
            var row = new EmployeeSalaryComponent
            {
                SalaryStructure = structure, SalaryComponentId = componentId, SalaryComponent = components[componentId],
                Value = value, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            structure.Components.Add(row);
            _context.Set<EmployeeSalaryComponent>().Add(row);
        }

        structure.GrossSalary = PayrollCalculator.Monthly(structure.BasicSalary, PayrollMapping.ToStructureComponents(structure)).Gross;
        structure.Employee.BasicSalary = structure.BasicSalary;
        structure.Employee.SalaryGradeId = structure.SalaryGradeId;

        await _context.SaveChangesAsync(ct);
        return await SalaryStructureRules.LoadDtoAsync(_context, structure.Id, ct);
    }
}

internal static class SalaryStructureRules
{
    internal static async Task EnsureGradeAsync(AppDbContext context, Guid gradeId, CancellationToken ct)
    {
        if (!await context.Set<SalaryGrade>().AnyAsync(x => !x.IsDeleted && x.Id == gradeId, ct))
            throw new NotFoundException(nameof(SalaryGrade), gradeId);
    }

    internal static async Task<Dictionary<Guid, SalaryComponent>> LoadComponentsAsync(
        AppDbContext context, List<SalaryStructureComponentInputDto> inputs, CancellationToken ct)
    {
        var ids = inputs.Select(x => x.SalaryComponentId).ToList();
        var components = await context.Set<SalaryComponent>()
            .Where(x => !x.IsDeleted && ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        foreach (var id in ids)
        {
            if (!components.TryGetValue(id, out var c) || !c.IsActive)
                throw new AppException("A selected salary component no longer exists or is inactive.");
            if (c.ComponentType == SalaryComponentType.BasicSalary)
                throw new AppException($"'{c.Name}' is a basic-salary component; basic salary is entered on the structure itself.");
            if (!PayrollCalculator.IsDeduction(c.ComponentType) && c.CalculationType == CalculationType.PercentageOfGross)
                throw new AppException($"Allowance '{c.Name}' can't be a percentage of gross, because gross is the total of the allowances. Use a fixed amount or % of basic.");
        }
        return components;
    }

    internal static async Task<EmployeeSalaryStructureResponseDto> LoadDtoAsync(AppDbContext context, Guid id, CancellationToken ct)
    {
        var structure = await context.Set<EmployeeSalaryStructure>()
            .Include(x => x.Employee).ThenInclude(x => x.Designation)
            .Include(x => x.SalaryGrade)
            .Include(x => x.Components).ThenInclude(x => x.SalaryComponent)
            .AsNoTracking()
            .FirstAsync(x => x.Id == id, ct);
        return PayrollMapping.StructureToDto(structure);
    }
}
