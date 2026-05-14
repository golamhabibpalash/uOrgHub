using FluentValidation;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.DTOs.Validators;

public class CreatePRValidator : AbstractValidator<CreatePRDto>
{
    public CreatePRValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.RequestedById).NotEmpty();
        RuleFor(x => x.RequiredDate).GreaterThan(x => x.PRDate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.WarehouseId).NotEmpty();
            item.RuleFor(x => x.RequestedQuantity).GreaterThan(0);
            item.RuleFor(x => x.EstimatedUnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdatePRValidator : AbstractValidator<UpdatePRDto>
{
    public UpdatePRValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.RequestedById).NotEmpty();
        RuleFor(x => x.RequiredDate).GreaterThan(x => x.PRDate);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemVariantId).NotEmpty();
            item.RuleFor(x => x.WarehouseId).NotEmpty();
            item.RuleFor(x => x.RequestedQuantity).GreaterThan(0);
            item.RuleFor(x => x.EstimatedUnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
