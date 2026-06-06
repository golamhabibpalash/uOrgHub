using FluentValidation;
using uOrgHub.HR.DTOs.Leave;

namespace uOrgHub.HR.DTOs.Validators;

public class CreateLeaveTypeDtoValidator : AbstractValidator<CreateLeaveTypeDto>
{
    public CreateLeaveTypeDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TotalDaysPerYear).GreaterThan(0);
        RuleFor(x => x.ApprovalLevels).InclusiveBetween(1, 5);
    }
}

public class CreateLeaveRequestDtoValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class UpdateLeaveRequestDtoValidator : AbstractValidator<UpdateLeaveRequestDto>
{
    public UpdateLeaveRequestDtoValidator()
    {
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class ApproveLeaveRequestDtoValidator : AbstractValidator<ApproveLeaveRequestDto>
{
    public ApproveLeaveRequestDtoValidator()
    {
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.Comments).MaximumLength(1000).When(x => x.Comments != null);
        RuleFor(x => x.RejectReason).NotEmpty().WithMessage("Reject reason is required.")
            .MaximumLength(1000)
            .When(x => !x.IsApproved);
    }
}
