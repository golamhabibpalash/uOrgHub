using FluentValidation;

namespace uOrgHub.HR.DTOs.Validators;

public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PersonalEmail).EmailAddress().MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.PersonalEmail));
        RuleFor(x => x.Phone).MaximumLength(20).Matches(@"^\+?[\d\s\-\(\)]{7,20}$").When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.MobilePhone).MaximumLength(20).Matches(@"^\+?[\d\s\-\(\)]{7,20}$").When(x => !string.IsNullOrWhiteSpace(x.MobilePhone));
        RuleFor(x => x.DesignationId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0);
    }
}
