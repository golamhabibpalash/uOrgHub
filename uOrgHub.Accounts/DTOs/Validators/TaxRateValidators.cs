using FluentValidation;
using uOrgHub.Accounts.DTOs.TaxRate;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateTaxRateValidator : AbstractValidator<CreateTaxRateDto>
{
    public CreateTaxRateValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100).WithMessage("Rate must be between 0 and 100");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateTaxRateValidator : AbstractValidator<UpdateTaxRateDto>
{
    public UpdateTaxRateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Rate).InclusiveBetween(0, 100).WithMessage("Rate must be between 0 and 100");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
