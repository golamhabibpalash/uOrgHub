using FluentValidation;
using uOrgHub.Projects.DTOs;

namespace uOrgHub.Projects.DTOs.Validators;

public class CreateWBSDtoValidator : AbstractValidator<CreateWBSDto>
{
    public CreateWBSDtoValidator()
    {
        RuleFor(x => x.WBSCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PlannedEndDate).GreaterThan(x => x.PlannedStartDate);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateBOQDtoValidator : AbstractValidator<CreateBOQDto>
{
    public CreateBOQDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class CreateDPRDtoValidator : AbstractValidator<CreateDPRDto>
{
    public CreateDPRDtoValidator()
    {
        RuleFor(x => x.WorkDone).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ReportedById).NotEmpty();
        RuleFor(x => x.ManpowerCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Issues).MaximumLength(1000);
        RuleFor(x => x.NextDayPlan).MaximumLength(1000);
        RuleFor(x => x.EquipmentUsed).MaximumLength(500);
    }
}

public class CreateMaterialRequestDtoValidator : AbstractValidator<CreateMaterialRequestDto>
{
    public CreateMaterialRequestDtoValidator()
    {
        RuleFor(x => x.RequestedById).NotEmpty();
        RuleFor(x => x.RequiredDate).GreaterThanOrEqualTo(x => x.RequestDate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class CreateProjectExpenseDtoValidator : AbstractValidator<CreateProjectExpenseDto>
{
    public CreateProjectExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.RecordedById).NotEmpty();
        RuleFor(x => x.InvoiceNumber).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdateProjectExpenseDtoValidator : AbstractValidator<UpdateProjectExpenseDto>
{
    public UpdateProjectExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.InvoiceNumber).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class CreateProjectBudgetDtoValidator : AbstractValidator<CreateProjectBudgetDto>
{
    public CreateProjectBudgetDtoValidator()
    {
        RuleFor(x => x.AllocatedAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreateProjectMilestoneDtoValidator : AbstractValidator<CreateProjectMilestoneDto>
{
    public CreateProjectMilestoneDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
