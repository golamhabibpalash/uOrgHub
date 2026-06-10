using FluentValidation;
using uOrgHub.Accounts.DTOs;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateChartOfAccountValidator : AbstractValidator<CreateChartOfAccountDto>
{
    public CreateChartOfAccountValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account Name is required")
            .MaximumLength(200);

        RuleFor(x => x.AccountGroupId)
            .NotEmpty().WithMessage("Account Group is required");

        RuleFor(x => x.CustomCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomCode));

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}