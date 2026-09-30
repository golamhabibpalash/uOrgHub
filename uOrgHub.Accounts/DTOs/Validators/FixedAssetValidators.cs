using FluentValidation;
using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateAssetCategoryValidator : AbstractValidator<CreateAssetCategoryDto>
{
    public CreateAssetCategoryValidator()
    {
        RuleFor(x => x.Code).MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DepreciationMethod).IsInEnum();
        RuleFor(x => x.UsefulLifeMonths).InclusiveBetween(1, 1200).WithMessage("Useful life must be between 1 and 1200 months");
        RuleFor(x => x.SalvageValuePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.AssetAccountId).NotEmpty().WithMessage("Asset account is required");
        RuleFor(x => x.AccumulatedDepreciationAccountId).NotEmpty().WithMessage("Accumulated depreciation account is required");
        RuleFor(x => x.DepreciationExpenseAccountId).NotEmpty().WithMessage("Depreciation expense account is required");
        RuleFor(x => x.HireRecoveryAccountId).NotEmpty()
            .When(x => x.HireExpenseAccountId.HasValue)
            .WithMessage("Set both hire accounts or neither: the recovery account is missing");
        RuleFor(x => x.HireExpenseAccountId).NotEmpty()
            .When(x => x.HireRecoveryAccountId.HasValue)
            .WithMessage("Set both hire accounts or neither: the hire expense account is missing");
    }
}

public class UpdateAssetCategoryValidator : AbstractValidator<UpdateAssetCategoryDto>
{
    public UpdateAssetCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DepreciationMethod).IsInEnum();
        RuleFor(x => x.UsefulLifeMonths).InclusiveBetween(1, 1200).WithMessage("Useful life must be between 1 and 1200 months");
        RuleFor(x => x.SalvageValuePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.AssetAccountId).NotEmpty().WithMessage("Asset account is required");
        RuleFor(x => x.AccumulatedDepreciationAccountId).NotEmpty().WithMessage("Accumulated depreciation account is required");
        RuleFor(x => x.DepreciationExpenseAccountId).NotEmpty().WithMessage("Depreciation expense account is required");
        RuleFor(x => x.HireRecoveryAccountId).NotEmpty()
            .When(x => x.HireExpenseAccountId.HasValue)
            .WithMessage("Set both hire accounts or neither: the recovery account is missing");
        RuleFor(x => x.HireExpenseAccountId).NotEmpty()
            .When(x => x.HireRecoveryAccountId.HasValue)
            .WithMessage("Set both hire accounts or neither: the hire expense account is missing");
    }
}

public class CreateFixedAssetValidator : AbstractValidator<CreateFixedAssetDto>
{
    public CreateFixedAssetValidator()
    {
        RuleFor(x => x.AssetCode).MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required");
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.ChassisNumber).MaximumLength(100);
        RuleFor(x => x.EngineNumber).MaximumLength(100);
        RuleFor(x => x.RegistrationNumber).MaximumLength(50);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.DepreciationMethod).IsInEnum().When(x => x.DepreciationMethod.HasValue);

        RuleFor(x => x.PurchaseDate).NotEmpty();
        RuleFor(x => x.DepreciationStartDate).GreaterThanOrEqualTo(x => x.PurchaseDate)
            .When(x => x.DepreciationStartDate.HasValue)
            .WithMessage("Depreciation cannot start before the purchase date");
        RuleFor(x => x.PurchaseCost).GreaterThan(0);
        RuleFor(x => x.SalvageValue).GreaterThanOrEqualTo(0).LessThan(x => x.PurchaseCost)
            .When(x => x.SalvageValue.HasValue)
            .WithMessage("Salvage value must be at least zero and less than the purchase cost");
        RuleFor(x => x.UsefulLifeMonths).InclusiveBetween(1, 1200).When(x => x.UsefulLifeMonths.HasValue);

        RuleFor(x => x.OpeningAccumulatedDepreciation).GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.PurchaseCost)
            .WithMessage("Opening accumulated depreciation cannot exceed the purchase cost");
        RuleFor(x => x.OpeningDepreciatedUpTo).NotEmpty()
            .When(x => x.OpeningAccumulatedDepreciation > 0)
            .WithMessage("Give the month-end the opening depreciation covers");
    }
}

public class UpdateFixedAssetValidator : AbstractValidator<UpdateFixedAssetDto>
{
    public UpdateFixedAssetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required");
        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.ChassisNumber).MaximumLength(100);
        RuleFor(x => x.EngineNumber).MaximumLength(100);
        RuleFor(x => x.RegistrationNumber).MaximumLength(50);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.DepreciationMethod).IsInEnum();

        RuleFor(x => x.PurchaseDate).NotEmpty();
        RuleFor(x => x.DepreciationStartDate).GreaterThanOrEqualTo(x => x.PurchaseDate)
            .WithMessage("Depreciation cannot start before the purchase date");
        RuleFor(x => x.PurchaseCost).GreaterThan(0);
        RuleFor(x => x.SalvageValue).GreaterThanOrEqualTo(0).LessThan(x => x.PurchaseCost)
            .WithMessage("Salvage value must be at least zero and less than the purchase cost");
        RuleFor(x => x.UsefulLifeMonths).InclusiveBetween(1, 1200);
    }
}

public class DeployAssetValidator : AbstractValidator<DeployAssetDto>
{
    public DeployAssetValidator()
    {
        RuleFor(x => x.FixedAssetId).NotEmpty().WithMessage("Asset is required");
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Project is required");
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.ChargeMode).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(500);

        When(x => x.ChargeMode == DeploymentChargeMode.HireRate, () =>
        {
            RuleFor(x => x.RateUnit).NotNull().IsInEnum().WithMessage("Choose per day or per month for the hire rate");
            RuleFor(x => x.Rate).NotNull().GreaterThan(0).WithMessage("Hire rate must be greater than zero");
        });
    }
}

public class ReturnAssetValidator : AbstractValidator<ReturnAssetDto>
{
    public ReturnAssetValidator()
    {
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.ReturnLocation).MaximumLength(200);
        RuleFor(x => x.ReturnNotes).MaximumLength(500);
        RuleFor(x => x.ReturnStatus).IsInEnum()
            .NotEqual(FixedAssetStatus.Deployed).WithMessage("A returned asset cannot stay Deployed");
    }
}

public class PostHireChargeRunValidator : AbstractValidator<PostHireChargeRunDto>
{
    public PostHireChargeRunValidator()
    {
        RuleFor(x => x.FromDate).NotEmpty();
        RuleFor(x => x.ToDate).NotEmpty().GreaterThanOrEqualTo(x => x.FromDate)
            .WithMessage("To date must be on or after the from date");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class PostDepreciationRunValidator : AbstractValidator<PostDepreciationRunDto>
{
    public PostDepreciationRunValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
