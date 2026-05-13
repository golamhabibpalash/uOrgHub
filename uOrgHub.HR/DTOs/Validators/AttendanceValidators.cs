using FluentValidation;
using uOrgHub.HR.DTOs.Attendance;

namespace uOrgHub.HR.DTOs.Validators;

public class CreateWorkScheduleDtoValidator : AbstractValidator<CreateWorkScheduleDto>
{
    public CreateWorkScheduleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TotalHours).GreaterThan(0).LessThanOrEqualTo(24);
        RuleFor(x => x.WorkingDaysPerWeek).InclusiveBetween(1, 7);
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
    }
}

public class CreateShiftDtoValidator : AbstractValidator<CreateShiftDto>
{
    public CreateShiftDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.WorkScheduleId).NotEmpty();
    }
}

public class CreateEmployeeRosterDtoValidator : AbstractValidator<CreateEmployeeRosterDto>
{
    public CreateEmployeeRosterDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.RosterDate).NotEmpty();
    }
}

public class CreateAttendanceLogDtoValidator : AbstractValidator<CreateAttendanceLogDto>
{
    public CreateAttendanceLogDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.AttendanceDate).NotEmpty();
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn)
            .When(x => x.CheckIn.HasValue && x.CheckOut.HasValue)
            .WithMessage("Check-out must be after check-in.");
    }
}
