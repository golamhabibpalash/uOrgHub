using FluentValidation;

namespace uOrgHub.HR.DTOs.Validators;

public class CreateDesignationDtoValidator : AbstractValidator<CreateDesignationDto>
{
    public CreateDesignationDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Level).GreaterThan(0);
    }
}
