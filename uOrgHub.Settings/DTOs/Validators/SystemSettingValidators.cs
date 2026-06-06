using FluentValidation;

namespace uOrgHub.Settings.DTOs.Validators;

public class CreateSystemSettingDtoValidator : AbstractValidator<CreateSystemSettingDto>
{
    public CreateSystemSettingDtoValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(50)
            .Must(d => d is "String" or "Int" or "Bool" or "Decimal" or "Json")
            .WithMessage("DataType must be one of: String, Int, Bool, Decimal, Json");
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}

public class UpdateSystemSettingDtoValidator : AbstractValidator<UpdateSystemSettingDto>
{
    public UpdateSystemSettingDtoValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.DataType).NotEmpty().MaximumLength(50)
            .Must(d => d is "String" or "Int" or "Bool" or "Decimal" or "Json")
            .WithMessage("DataType must be one of: String, Int, Bool, Decimal, Json");
    }
}
