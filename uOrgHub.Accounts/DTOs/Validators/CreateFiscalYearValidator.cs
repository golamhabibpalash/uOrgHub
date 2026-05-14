using FluentValidation;
using uOrgHub.Accounts.DTOs;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateFiscalYearValidator : AbstractValidator<CreateFiscalYearDto>
{
    public CreateFiscalYearValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}