using FluentValidation;
using uOrgHub.Inventory.DTOs;

namespace uOrgHub.Inventory.DTOs.Validators;

public class CreateInventoryTypeValidator : AbstractValidator<CreateInventoryTypeDto>
{
    public CreateInventoryTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateInventoryTypeValidator : AbstractValidator<UpdateInventoryTypeDto>
{
    public UpdateInventoryTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateInventoryCategoryValidator : AbstractValidator<CreateInventoryCategoryDto>
{
    public CreateInventoryCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TypeId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateInventoryCategoryValidator : AbstractValidator<UpdateInventoryCategoryDto>
{
    public UpdateInventoryCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TypeId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateUnitOfMeasureValidator : AbstractValidator<CreateUnitOfMeasureDto>
{
    public CreateUnitOfMeasureValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(10);
    }
}

public class UpdateUnitOfMeasureValidator : AbstractValidator<UpdateUnitOfMeasureDto>
{
    public UpdateUnitOfMeasureValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Abbreviation).NotEmpty().MaximumLength(10);
    }
}

public class CreateAttributeDefinitionValidator : AbstractValidator<CreateAttributeDefinitionDto>
{
    public CreateAttributeDefinitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DataType).IsInEnum();
    }
}

public class UpdateAttributeDefinitionValidator : AbstractValidator<UpdateAttributeDefinitionDto>
{
    public UpdateAttributeDefinitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DataType).IsInEnum();
    }
}
