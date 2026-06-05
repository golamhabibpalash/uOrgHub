using FluentValidation;
using uOrgHub.HR.DTOs.Recruitment;

namespace uOrgHub.HR.DTOs.Validators;

public class CreateJobPostingDtoValidator : AbstractValidator<CreateJobPostingDto>
{
    public CreateJobPostingDtoValidator()
    {
        // JobCode is optional: auto-generated server-side when left blank.
        RuleFor(x => x.JobCode).MaximumLength(30);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DesignationId).NotEmpty();
        RuleFor(x => x.RequiredCount).GreaterThan(0);
        RuleFor(x => x.ClosingDate).GreaterThan(x => x.PostedDate)
            .When(x => x.PostedDate.HasValue && x.ClosingDate.HasValue)
            .WithMessage("Closing date must be after posted date.");
    }
}

public class CreateCandidateDtoValidator : AbstractValidator<CreateCandidateDto>
{
    public CreateCandidateDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
        RuleFor(x => x.TotalExperienceYears).GreaterThanOrEqualTo(0);
    }
}

public class CreateJobApplicationDtoValidator : AbstractValidator<CreateJobApplicationDto>
{
    public CreateJobApplicationDtoValidator()
    {
        RuleFor(x => x.CandidateId).NotEmpty();
        RuleFor(x => x.JobPostingId).NotEmpty();
    }
}
