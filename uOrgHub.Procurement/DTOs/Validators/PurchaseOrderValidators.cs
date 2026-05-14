using FluentValidation;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.DTOs.Validators;

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.ExpectedDeliveryDate).GreaterThan(x => x.PODate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.OrderedQuantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.TaxPercent).InclusiveBetween(0, 100);
            item.RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        });
    }
}

public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderDto>
{
    public UpdatePurchaseOrderValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.ExpectedDeliveryDate).GreaterThan(x => x.PODate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.OrderedQuantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.TaxPercent).InclusiveBetween(0, 100);
            item.RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        });
    }
}
