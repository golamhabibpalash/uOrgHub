using FluentValidation;
using uOrgHub.Inventory.DTOs;

namespace uOrgHub.Inventory.DTOs.Validators;

public class CreateItemValidator : AbstractValidator<CreateItemDto>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.BaseName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TypeId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.UnitOfMeasureId).NotEmpty();
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StandardCost).GreaterThanOrEqualTo(0);
    }
}

public class UpdateItemValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.BaseName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TypeId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.UnitOfMeasureId).NotEmpty();
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StandardCost).GreaterThanOrEqualTo(0);
    }
}

public class CreateItemVariantValidator : AbstractValidator<CreateItemVariantDto>
{
    public CreateItemVariantValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Attributes).NotNull();
    }
}

public class UpdateItemVariantValidator : AbstractValidator<UpdateItemVariantDto>
{
    public UpdateItemVariantValidator()
    {
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Attributes).NotNull();
    }
}
