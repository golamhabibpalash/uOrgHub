using FluentValidation;
using uOrgHub.HR.DTOs.Performance;

namespace uOrgHub.HR.DTOs.Validators;

public class CreateReviewCycleDtoValidator : AbstractValidator<CreateReviewCycleDto>
{
    public CreateReviewCycleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}

public class CreateKPIDtoValidator : AbstractValidator<CreateKPIDto>
{
    public CreateKPIDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Weight).InclusiveBetween(0, 100);
    }
}

public class CreateGoalDtoValidator : AbstractValidator<CreateGoalDto>
{
    public CreateGoalDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ReviewCycleId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Weight).InclusiveBetween(0, 100);
    }
}

public class CreatePerformanceReviewDtoValidator : AbstractValidator<CreatePerformanceReviewDto>
{
    public CreatePerformanceReviewDtoValidator()
    {
        RuleFor(x => x.ReviewCycleId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ReviewerId).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty();
    }
}

public class CreateTrainingProgramDtoValidator : AbstractValidator<CreateTrainingProgramDto>
{
    public CreateTrainingProgramDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationHours).GreaterThan(0);
        RuleFor(x => x.MaxParticipants).GreaterThan(0);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).When(x => x.Cost.HasValue);
    }
}
