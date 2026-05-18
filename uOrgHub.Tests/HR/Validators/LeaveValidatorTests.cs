using FluentAssertions;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.DTOs.Validators;

namespace uOrgHub.Tests.HR.Validators;

public class CreateLeaveTypeDtoValidatorTests
{
    private readonly CreateLeaveTypeDtoValidator _validator = new();

    private CreateLeaveTypeDto ValidDto() => new()
    {
        Name = "Annual Leave",
        Code = "AL",
        TotalDaysPerYear = 20,
        ApprovalLevels = 1
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Name_over_100_chars_fails()
    {
        var dto = ValidDto(); dto.Name = new string('A', 101);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_code_fails()
    {
        var dto = ValidDto(); dto.Code = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_total_days_fails()
    {
        var dto = ValidDto(); dto.TotalDaysPerYear = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Positive_total_days_passes()
    {
        var dto = ValidDto(); dto.TotalDaysPerYear = 1;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Approval_levels_zero_fails()
    {
        var dto = ValidDto(); dto.ApprovalLevels = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Approval_levels_5_passes()
    {
        var dto = ValidDto(); dto.ApprovalLevels = 5;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Approval_levels_6_fails()
    {
        var dto = ValidDto(); dto.ApprovalLevels = 6;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreateLeaveRequestDtoValidatorTests
{
    private readonly CreateLeaveRequestDtoValidator _validator = new();

    private CreateLeaveRequestDto ValidDto() => new()
    {
        EmployeeId = Guid.NewGuid(),
        LeaveTypeId = Guid.NewGuid(),
        StartDate = DateTime.Today,
        EndDate = DateTime.Today.AddDays(2),
        Reason = "Personal work"
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_employee_id_fails()
    {
        var dto = ValidDto(); dto.EmployeeId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_leave_type_id_fails()
    {
        var dto = ValidDto(); dto.LeaveTypeId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void End_date_before_start_date_fails()
    {
        var dto = ValidDto();
        dto.StartDate = DateTime.Today.AddDays(5);
        dto.EndDate = DateTime.Today;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("End date must be on or after start date"));
    }

    [Fact]
    public void Same_start_and_end_date_passes()
    {
        var dto = ValidDto();
        dto.StartDate = DateTime.Today;
        dto.EndDate = DateTime.Today;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_reason_fails()
    {
        var dto = ValidDto(); dto.Reason = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Reason_over_1000_chars_fails()
    {
        var dto = ValidDto(); dto.Reason = new string('R', 1001);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class ApproveLeaveRequestDtoValidatorTests
{
    private readonly ApproveLeaveRequestDtoValidator _validator = new();

    private ApproveLeaveRequestDto ValidDto() => new()
    {
        LeaveRequestId = Guid.NewGuid(),
        ApproverId = Guid.NewGuid(),
        ApprovalLevel = 1
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_leave_request_id_fails()
    {
        var dto = ValidDto(); dto.LeaveRequestId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_approver_id_fails()
    {
        var dto = ValidDto(); dto.ApproverId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Approval_level_zero_fails()
    {
        var dto = ValidDto(); dto.ApprovalLevel = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Comments_within_1000_chars_passes()
    {
        var dto = ValidDto(); dto.Comments = new string('C', 1000);
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Comments_over_1000_chars_fails()
    {
        var dto = ValidDto(); dto.Comments = new string('C', 1001);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
