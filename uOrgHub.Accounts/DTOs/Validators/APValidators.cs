using FluentValidation;
using uOrgHub.Accounts.DTOs.AP;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateVendorValidator : AbstractValidator<CreateVendorDto>
{
    public CreateVendorValidator()
    {
        RuleFor(x => x.VendorCode).MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.TIN).MaximumLength(20);
        RuleFor(x => x.BIN).MaximumLength(20);
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PayableAccountId).NotEmpty().WithMessage("Payable account is required");
    }
}

public class UpdateVendorValidator : AbstractValidator<UpdateVendorDto>
{
    public UpdateVendorValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.TIN).MaximumLength(20);
        RuleFor(x => x.BIN).MaximumLength(20);
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
    }
}

public class CreateBillValidator : AbstractValidator<CreateBillDto>
{
    public CreateBillValidator()
    {
        RuleFor(x => x.BillNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.VendorBillNumber).MaximumLength(50);
        RuleFor(x => x.VendorId).NotEmpty().WithMessage("Vendor is required");
        RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required");
        RuleFor(x => x.BillDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.BillDate)
            .WithMessage("Due date must be on or after bill date");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Bill must have at least one line");
        RuleForEach(x => x.Lines).SetValidator(new CreateBillLineValidator());
    }
}

public class CreateBillLineValidator : AbstractValidator<CreateBillLineDto>
{
    public CreateBillLineValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.ExpenseAccountId).NotEmpty().WithMessage("Expense account is required");
    }
}

public class UpdateBillValidator : AbstractValidator<UpdateBillDto>
{
    public UpdateBillValidator()
    {
        RuleFor(x => x.VendorBillNumber).MaximumLength(50);
        RuleFor(x => x.BillDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.BillDate)
            .WithMessage("Due date must be on or after bill date");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Bill must have at least one line");
        RuleForEach(x => x.Lines).SetValidator(new CreateBillLineValidator());
    }
}
