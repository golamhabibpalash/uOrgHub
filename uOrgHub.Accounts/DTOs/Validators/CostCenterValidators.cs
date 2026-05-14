using FluentValidation;
using uOrgHub.Accounts.DTOs.CostCenter;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateCostCenterValidator : AbstractValidator<CreateCostCenterDto>
{
    public CreateCostCenterValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCostCenterValidator : AbstractValidator<UpdateCostCenterDto>
{
    public UpdateCostCenterValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
