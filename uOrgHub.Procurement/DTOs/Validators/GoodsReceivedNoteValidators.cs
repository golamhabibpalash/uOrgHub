using FluentValidation;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.DTOs.Validators;

public class CreateGRNValidator : AbstractValidator<CreateGRNDto>
{
    public CreateGRNValidator()
    {
        RuleFor(x => x.POId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ReceivedById).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.POItemId).NotEmpty();
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.ReceivedQuantity).GreaterThan(0);
            item.RuleFor(x => x.RejectedQuantity).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateGRNValidator : AbstractValidator<UpdateGRNDto>
{
    public UpdateGRNValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ReceivedById).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.POItemId).NotEmpty();
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.ReceivedQuantity).GreaterThan(0);
            item.RuleFor(x => x.RejectedQuantity).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
