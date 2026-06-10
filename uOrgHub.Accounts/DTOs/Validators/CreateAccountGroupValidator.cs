using FluentValidation;
using uOrgHub.Accounts.DTOs;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateAccountGroupValidator : AbstractValidator<CreateAccountGroupDto>
{
    public CreateAccountGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.CustomCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomCode));

        RuleFor(x => x.ParentAccountGroupId)
            .NotEmpty().WithMessage("Parent group is invalid.")
            .When(x => x.ParentAccountGroupId.HasValue);
    }
}