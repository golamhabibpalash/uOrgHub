using FluentValidation;
using uOrgHub.Projects.DTOs;

namespace uOrgHub.Projects.DTOs.Validators;

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ProjectManagerId).NotEmpty();
        RuleFor(x => x.ContractValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PlannedEndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ContractValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PlannedEndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
