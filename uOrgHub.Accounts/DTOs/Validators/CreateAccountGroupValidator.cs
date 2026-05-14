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

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}