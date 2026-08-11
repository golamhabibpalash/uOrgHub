using FluentValidation;
using uOrgHub.Accounts.DTOs.Voucher;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateVoucherValidator : AbstractValidator<CreateVoucherDto>
{
    public CreateVoucherValidator()
    {
        RuleFor(x => x.VoucherType)
            .IsInEnum().WithMessage("Voucher type is required");

        RuleFor(x => x.VoucherDate)
            .NotEmpty().WithMessage("Voucher date is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500);

        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.Section).MaximumLength(200);

        RuleFor(x => x.DebitAccountId)
            .NotEmpty().WithMessage("Debit account is required");

        RuleFor(x => x.CreditAccountId)
            .NotEmpty().WithMessage("Credit account is required");

        RuleFor(x => x)
            .Must(x => x.DebitAccountId != x.CreditAccountId)
            .WithMessage("Debit and credit accounts must be different");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PreparedBy).MaximumLength(200);
        RuleFor(x => x.ReceivedBy).MaximumLength(200);
    }
}

public class UpdateVoucherValidator : AbstractValidator<UpdateVoucherDto>
{
    public UpdateVoucherValidator()
    {
        RuleFor(x => x.VoucherDate)
            .NotEmpty().WithMessage("Voucher date is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500);

        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.Section).MaximumLength(200);

        RuleFor(x => x.DebitAccountId)
            .NotEmpty().WithMessage("Debit account is required");

        RuleFor(x => x.CreditAccountId)
            .NotEmpty().WithMessage("Credit account is required");

        RuleFor(x => x)
            .Must(x => x.DebitAccountId != x.CreditAccountId)
            .WithMessage("Debit and credit accounts must be different");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PreparedBy).MaximumLength(200);
        RuleFor(x => x.ReceivedBy).MaximumLength(200);
    }
}

public class RejectVoucherValidator : AbstractValidator<RejectVoucherDto>
{
    public RejectVoucherValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(500);
    }
}