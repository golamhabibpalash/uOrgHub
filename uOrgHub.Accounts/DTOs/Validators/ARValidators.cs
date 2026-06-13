using FluentValidation;
using uOrgHub.Accounts.DTOs.AR;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.CustomerCode).MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.TIN).MaximumLength(20);
        RuleFor(x => x.BIN).MaximumLength(20);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReceivableAccountId).NotEmpty().WithMessage("Receivable account is required");
    }
}

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.TIN).MaximumLength(20);
        RuleFor(x => x.BIN).MaximumLength(20);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
    }
}

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.InvoiceNumber).MaximumLength(30);
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer is required");
        RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required");
        RuleFor(x => x.InvoiceDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate)
            .WithMessage("Due date must be on or after invoice date");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Invoice must have at least one line");
        RuleForEach(x => x.Lines).SetValidator(new CreateInvoiceLineValidator());
    }
}

public class CreateInvoiceLineValidator : AbstractValidator<CreateInvoiceLineDto>
{
    public CreateInvoiceLineValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.RevenueAccountId).NotEmpty().WithMessage("Revenue account is required");
    }
}

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.InvoiceDate).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate)
            .WithMessage("Due date must be on or after invoice date");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Invoice must have at least one line");
        RuleForEach(x => x.Lines).SetValidator(new CreateInvoiceLineValidator());
    }
}
