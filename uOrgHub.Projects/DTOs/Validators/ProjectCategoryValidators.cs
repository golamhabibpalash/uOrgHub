using FluentValidation;
using uOrgHub.Projects.DTOs;

namespace uOrgHub.Projects.DTOs.Validators;

public class CreateProjectCategoryDtoValidator : AbstractValidator<CreateProjectCategoryDto>
{
    public CreateProjectCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateProjectCategoryDtoValidator : AbstractValidator<UpdateProjectCategoryDto>
{
    public UpdateProjectCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
