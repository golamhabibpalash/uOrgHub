using FluentAssertions;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.DTOs.Validators;
using uOrgHub.HR.Models.Enums;

namespace uOrgHub.Tests.HR.Validators;

public class CreateWorkScheduleDtoValidatorTests
{
    private readonly CreateWorkScheduleDtoValidator _validator = new();

    private CreateWorkScheduleDto ValidDto() => new()
    {
        Name = "Standard 8h",
        TotalHours = 8,
        WorkingDaysPerWeek = 5,
        GracePeriodMinutes = 10
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
    public void TotalHours_zero_fails()
    {
        var dto = ValidDto(); dto.TotalHours = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void TotalHours_24_passes()
    {
        var dto = ValidDto(); dto.TotalHours = 24;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void TotalHours_exceeding_24_fails()
    {
        var dto = ValidDto(); dto.TotalHours = 25;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void WorkingDays_zero_fails()
    {
        var dto = ValidDto(); dto.WorkingDaysPerWeek = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void WorkingDays_7_passes()
    {
        var dto = ValidDto(); dto.WorkingDaysPerWeek = 7;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void WorkingDays_8_fails()
    {
        var dto = ValidDto(); dto.WorkingDaysPerWeek = 8;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_grace_period_fails()
    {
        var dto = ValidDto(); dto.GracePeriodMinutes = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_grace_period_passes()
    {
        var dto = ValidDto(); dto.GracePeriodMinutes = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }
}

public class CreateShiftDtoValidatorTests
{
    private readonly CreateShiftDtoValidator _validator = new();

    private CreateShiftDto ValidDto() => new()
    {
        Name = "Morning",
        Code = "MOR",
        WorkScheduleId = Guid.NewGuid()
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
    public void Empty_code_fails()
    {
        var dto = ValidDto(); dto.Code = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.Code = new string('S', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_work_schedule_id_fails()
    {
        var dto = ValidDto(); dto.WorkScheduleId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreateEmployeeRosterDtoValidatorTests
{
    private readonly CreateEmployeeRosterDtoValidator _validator = new();

    private CreateEmployeeRosterDto ValidDto() => new()
    {
        EmployeeId = Guid.NewGuid(),
        ShiftId = Guid.NewGuid(),
        RosterDate = DateTime.Today
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
    public void Empty_shift_id_fails()
    {
        var dto = ValidDto(); dto.ShiftId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Default_roster_date_fails()
    {
        var dto = ValidDto(); dto.RosterDate = default;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreateAttendanceLogDtoValidatorTests
{
    private readonly CreateAttendanceLogDtoValidator _validator = new();

    private CreateAttendanceLogDto ValidDto() => new()
    {
        EmployeeId = Guid.NewGuid(),
        AttendanceDate = DateTime.Today
    };

    [Fact]
    public void Valid_dto_no_times_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Checkout_after_checkin_passes()
    {
        var dto = ValidDto();
        dto.CheckIn = DateTime.Today.AddHours(9);
        dto.CheckOut = DateTime.Today.AddHours(17);
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Checkout_before_checkin_fails()
    {
        var dto = ValidDto();
        dto.CheckIn = DateTime.Today.AddHours(17);
        dto.CheckOut = DateTime.Today.AddHours(9);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Check-out must be after check-in"));
    }

    [Fact]
    public void Empty_employee_id_fails()
    {
        var dto = ValidDto(); dto.EmployeeId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Default_attendance_date_fails()
    {
        var dto = ValidDto(); dto.AttendanceDate = default;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
