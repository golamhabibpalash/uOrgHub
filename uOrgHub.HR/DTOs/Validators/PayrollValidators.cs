using FluentValidation;
using uOrgHub.HR.DTOs.Payroll;

namespace uOrgHub.HR.DTOs.Validators;

public class CreateSalaryGradeDtoValidator : AbstractValidator<CreateSalaryGradeDto>
{
    public CreateSalaryGradeDtoValidator()
    {
        RuleFor(x => x.GradeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSalary).GreaterThan(x => x.MinSalary)
            .WithMessage("Max salary must be greater than min salary.");
    }
}

public class UpdateSalaryGradeDtoValidator : AbstractValidator<UpdateSalaryGradeDto>
{
    public UpdateSalaryGradeDtoValidator()
    {
        RuleFor(x => x.GradeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSalary).GreaterThan(x => x.MinSalary)
            .WithMessage("Max salary must be greater than min salary.");
    }
}

public class CreateSalaryComponentDtoValidator : AbstractValidator<CreateSalaryComponentDto>
{
    public CreateSalaryComponentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DefaultValue).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSalaryComponentDtoValidator : AbstractValidator<UpdateSalaryComponentDto>
{
    public UpdateSalaryComponentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DefaultValue).GreaterThanOrEqualTo(0);
    }
}

public class CreatePayrollCycleDtoValidator : AbstractValidator<CreatePayrollCycleDto>
{
    public CreatePayrollCycleDtoValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}

public class CreateExpenseRequestDtoValidator : AbstractValidator<CreateExpenseRequestDto>
{
    public CreateExpenseRequestDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExpenseDate).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public class CreateEmployeeSalaryStructureDtoValidator : AbstractValidator<CreateEmployeeSalaryStructureDto>
{
    public CreateEmployeeSalaryStructureDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.SalaryGradeId).NotEmpty().WithMessage("Salary grade is required.");
        RuleFor(x => x.BasicSalary).GreaterThan(0).WithMessage("Basic salary must be greater than 0.");
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleForEach(x => x.Components).ChildRules(c => c.RuleFor(x => x.Value).GreaterThanOrEqualTo(0));
        RuleFor(x => x.Components)
            .Must(c => c.Select(x => x.SalaryComponentId).Distinct().Count() == c.Count)
            .WithMessage("Each salary component can be added only once.");
    }
}

public class UpdateEmployeeSalaryStructureDtoValidator : AbstractValidator<UpdateEmployeeSalaryStructureDto>
{
    public UpdateEmployeeSalaryStructureDtoValidator()
    {
        RuleFor(x => x.SalaryGradeId).NotEmpty().WithMessage("Salary grade is required.");
        RuleFor(x => x.BasicSalary).GreaterThan(0).WithMessage("Basic salary must be greater than 0.");
        RuleForEach(x => x.Components).ChildRules(c => c.RuleFor(x => x.Value).GreaterThanOrEqualTo(0));
        RuleFor(x => x.Components)
            .Must(c => c.Select(x => x.SalaryComponentId).Distinct().Count() == c.Count)
            .WithMessage("Each salary component can be added only once.");
    }
}
