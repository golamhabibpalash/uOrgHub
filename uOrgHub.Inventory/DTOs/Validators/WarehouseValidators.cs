using FluentValidation;
using uOrgHub.Inventory.DTOs;

namespace uOrgHub.Inventory.DTOs.Validators;

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.ContactPerson).MaximumLength(100);
        RuleFor(x => x.ContactPhone).MaximumLength(20);
    }
}

public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseDto>
{
    public UpdateWarehouseValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.ContactPerson).MaximumLength(100);
        RuleFor(x => x.ContactPhone).MaximumLength(20);
    }
}

public class CreateStockTransactionValidator : AbstractValidator<CreateStockTransactionDto>
{
    public CreateStockTransactionValidator()
    {
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.ItemVariantId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReferenceNumber).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.FromWarehouseId)
            .NotEmpty()
            .When(x => x.TransactionType == Models.Enums.StockTransactionType.Transfer)
            .WithMessage("FromWarehouseId is required for Transfer transactions.");
    }
}

public class UpdateStockTransactionValidator : AbstractValidator<UpdateStockTransactionDto>
{
    public UpdateStockTransactionValidator()
    {
        RuleFor(x => x.ItemVariantId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReferenceNumber).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
