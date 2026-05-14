using FluentValidation;
using uOrgHub.Accounts.DTOs;

namespace uOrgHub.Accounts.DTOs.Validators;

public class UpdateJournalEntryValidator : AbstractValidator<UpdateJournalEntryDto>
{
    public UpdateJournalEntryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500);

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50);

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Journal entry must have at least one line");

        RuleForEach(x => x.Lines).SetValidator(new UpdateJournalEntryLineValidator());
    }
}

public class UpdateJournalEntryLineValidator : AbstractValidator<UpdateJournalEntryLineDto>
{
    public UpdateJournalEntryLineValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account is required");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.DebitAmount > 0 || x.CreditAmount > 0)
            .WithMessage("Line must have either debit or credit amount");

        RuleFor(x => x)
            .Must(x => x.DebitAmount == 0 || x.CreditAmount == 0)
            .WithMessage("Line cannot have both debit and credit amounts");
    }
}