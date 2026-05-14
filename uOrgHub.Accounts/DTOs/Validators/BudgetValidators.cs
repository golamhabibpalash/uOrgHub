using FluentValidation;
using uOrgHub.Accounts.DTOs.Budget;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateBudgetValidator : AbstractValidator<CreateBudgetDto>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Budget must have at least one line");
        RuleForEach(x => x.Lines).SetValidator(new CreateBudgetLineValidator());
    }
}

public class CreateBudgetLineValidator : AbstractValidator<CreateBudgetLineDto>
{
    public CreateBudgetLineValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Account is required");
        RuleFor(x => x.PlannedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Period).GreaterThanOrEqualTo(0);
    }
}

public class UpdateBudgetValidator : AbstractValidator<UpdateBudgetDto>
{
    public UpdateBudgetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
