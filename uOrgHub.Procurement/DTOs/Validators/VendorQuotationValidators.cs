using FluentValidation;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.DTOs.Validators;

public class CreateVendorQuotationValidator : AbstractValidator<CreateVendorQuotationDto>
{
    public CreateVendorQuotationValidator()
    {
        RuleFor(x => x.RFQId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.ValidUntil).GreaterThan(x => x.QuotationDate);
        RuleFor(x => x.DeliveryDays).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.RFQItemId).NotEmpty();
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.QuotedQuantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateVendorQuotationValidator : AbstractValidator<UpdateVendorQuotationDto>
{
    public UpdateVendorQuotationValidator()
    {
        RuleFor(x => x.ValidUntil).GreaterThan(x => x.QuotationDate);
        RuleFor(x => x.DeliveryDays).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.RFQItemId).NotEmpty();
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.QuotedQuantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
