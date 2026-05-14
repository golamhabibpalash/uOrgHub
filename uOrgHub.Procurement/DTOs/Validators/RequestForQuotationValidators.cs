using FluentValidation;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.DTOs.Validators;

public class CreateRFQValidator : AbstractValidator<CreateRFQDto>
{
    public CreateRFQValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClosingDate).GreaterThan(x => x.RFQDate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.RequestedQuantity).GreaterThan(0);
        });
    }
}

public class UpdateRFQValidator : AbstractValidator<UpdateRFQDto>
{
    public UpdateRFQValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClosingDate).GreaterThan(x => x.RFQDate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.RequestedQuantity).GreaterThan(0);
        });
    }
}
