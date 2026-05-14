using FluentValidation;
using uOrgHub.Accounts.DTOs;

namespace uOrgHub.Accounts.DTOs.Validators;

public class UpdateAccountGroupValidator : AbstractValidator<UpdateAccountGroupDto>
{
    public UpdateAccountGroupValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

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