using FluentValidation;

namespace uOrgHub.Settings.DTOs.Validators;

public class CreateValidationRuleDtoValidator : AbstractValidator<CreateValidationRuleDto>
{
    public CreateValidationRuleDtoValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RuleType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ErrorMessage).MaximumLength(500).When(x => x.ErrorMessage != null);
        RuleFor(x => x.Severity).NotEmpty().MaximumLength(50)
            .Must(s => s is "Error" or "Warning")
            .WithMessage("Severity must be one of: Error, Warning");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateValidationRuleDtoValidator : AbstractValidator<UpdateValidationRuleDto>
{
    public UpdateValidationRuleDtoValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RuleType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ErrorMessage).MaximumLength(500).When(x => x.ErrorMessage != null);
        RuleFor(x => x.Severity).NotEmpty().MaximumLength(50)
            .Must(s => s is "Error" or "Warning")
            .WithMessage("Severity must be one of: Error, Warning");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
