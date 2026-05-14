using FluentValidation;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.DTOs.Validators;

public class CreateVendorValidator : AbstractValidator<CreateVendorDto>
{
    public CreateVendorValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactPerson).MaximumLength(100).When(x => x.ContactPerson != null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentTermDays).GreaterThanOrEqualTo(0);
    }
}

public class UpdateVendorValidator : AbstractValidator<UpdateVendorDto>
{
    public UpdateVendorValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactPerson).MaximumLength(100).When(x => x.ContactPerson != null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentTermDays).GreaterThanOrEqualTo(0);
    }
}
