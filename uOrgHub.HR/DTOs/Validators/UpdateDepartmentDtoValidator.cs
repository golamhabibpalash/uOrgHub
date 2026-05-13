using FluentValidation;

namespace uOrgHub.HR.DTOs.Validators;

public class UpdateDepartmentDtoValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}
