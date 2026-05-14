using FluentValidation;
using uOrgHub.Accounts.DTOs.Banking;

namespace uOrgHub.Accounts.DTOs.Validators;

public class CreateBankAccountValidator : AbstractValidator<CreateBankAccountDto>
{
    public CreateBankAccountValidator()
    {
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AccountName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BranchName).MaximumLength(100);
        RuleFor(x => x.RoutingNumber).MaximumLength(30);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.ChartOfAccountId).NotEmpty().WithMessage("Chart of account is required");
    }
}

public class UpdateBankAccountValidator : AbstractValidator<UpdateBankAccountDto>
{
    public UpdateBankAccountValidator()
    {
        RuleFor(x => x.AccountName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BranchName).MaximumLength(100);
        RuleFor(x => x.RoutingNumber).MaximumLength(30);
    }
}

public class CreateBankTransactionValidator : AbstractValidator<CreateBankTransactionDto>
{
    public CreateBankTransactionValidator()
    {
        RuleFor(x => x.BankAccountId).NotEmpty().WithMessage("Bank account is required");
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ReferenceNumber).MaximumLength(50);
        RuleFor(x => x.ChequeNumber).MaximumLength(500);
        RuleFor(x => x.Payee).MaximumLength(200);
    }
}
