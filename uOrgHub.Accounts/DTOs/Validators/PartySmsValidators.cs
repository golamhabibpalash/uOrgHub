using FluentValidation;
using uOrgHub.Accounts.DTOs.PartySms;

namespace uOrgHub.Accounts.DTOs.Validators;

public class SendPartySmsDtoValidator : AbstractValidator<SendPartySmsDto>
{
    public SendPartySmsDtoValidator()
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty().Length(8, 100);
        RuleFor(x => x.Message).MaximumLength(1600);
    }
}
