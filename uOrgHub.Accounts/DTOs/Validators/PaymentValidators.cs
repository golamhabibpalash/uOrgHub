using FluentValidation;
using uOrgHub.Accounts.DTOs.Payment;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.PaymentNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.PaymentDate).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than 0");
        RuleFor(x => x.FiscalYearId).NotEmpty().WithMessage("Fiscal year is required");
        RuleFor(x => x.ReferenceNumber).MaximumLength(50);
        RuleFor(x => x.ChequeNumber).MaximumLength(50);

        RuleFor(x => x)
            .Must(x => x.CustomerId.HasValue || x.VendorId.HasValue)
            .WithMessage("Payment must be linked to either a customer or a vendor");

        RuleFor(x => x.Allocations)
            .Must(a => a.All(x => x.InvoiceId.HasValue || x.BillId.HasValue))
            .WithMessage("Each allocation must reference an invoice or bill")
            .When(x => x.Allocations.Count > 0);

        RuleForEach(x => x.Allocations).SetValidator(new CreatePaymentAllocationValidator());
    }
}

public class CreatePaymentAllocationValidator : AbstractValidator<CreatePaymentAllocationDto>
{
    public CreatePaymentAllocationValidator()
    {
        RuleFor(x => x.AllocatedAmount).GreaterThan(0).WithMessage("Allocated amount must be greater than 0");
        RuleFor(x => x)
            .Must(x => x.InvoiceId.HasValue || x.BillId.HasValue)
            .WithMessage("Allocation must reference an invoice or bill");
    }
}
