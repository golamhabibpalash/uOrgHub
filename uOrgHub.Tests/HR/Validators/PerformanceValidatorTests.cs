using FluentAssertions;
using uOrgHub.HR.DTOs.Performance;
using uOrgHub.HR.DTOs.Validators;

namespace uOrgHub.Tests.HR.Validators;

public class CreateReviewCycleDtoValidatorTests
{
    private readonly CreateReviewCycleDtoValidator _validator = new();

    private CreateReviewCycleDto ValidDto() => new()
    {
        Name = "Q1 2026 Review",
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2026, 3, 31)
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
    public void Name_over_200_chars_fails()
    {
        var dto = ValidDto(); dto.Name = new string('N', 201);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void End_date_before_start_date_fails()
    {
        var dto = ValidDto();
        dto.StartDate = new DateTime(2026, 3, 31);
        dto.EndDate = new DateTime(2026, 1, 1);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("End date must be after start date"));
    }
}

public class CreateKPIDtoValidatorTests
{
    private readonly CreateKPIDtoValidator _validator = new();

    private CreateKPIDto ValidDto() => new()
    {
        Name = "Sales Target",
        Weight = 50
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
    public void Weight_zero_passes()
    {
        var dto = ValidDto(); dto.Weight = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Weight_100_passes()
    {
        var dto = ValidDto(); dto.Weight = 100;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Weight_over_100_fails()
    {
        var dto = ValidDto(); dto.Weight = 101;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_weight_fails()
    {
        var dto = ValidDto(); dto.Weight = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreateGoalDtoValidatorTests
{
    private readonly CreateGoalDtoValidator _validator = new();

    private CreateGoalDto ValidDto() => new()
    {
        EmployeeId = Guid.NewGuid(),
        ReviewCycleId = Guid.NewGuid(),
        Title = "Achieve 100% sales target",
        Weight = 80
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
    public void Empty_review_cycle_id_fails()
    {
        var dto = ValidDto(); dto.ReviewCycleId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_title_fails()
    {
        var dto = ValidDto(); dto.Title = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Title_over_300_chars_fails()
    {
        var dto = ValidDto(); dto.Title = new string('T', 301);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Weight_over_100_fails()
    {
        var dto = ValidDto(); dto.Weight = 101;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreatePerformanceReviewDtoValidatorTests
{
    private readonly CreatePerformanceReviewDtoValidator _validator = new();

    private CreatePerformanceReviewDto ValidDto() => new()
    {
        ReviewCycleId = Guid.NewGuid(),
        EmployeeId = Guid.NewGuid(),
        ReviewerId = Guid.NewGuid(),
        DueDate = DateTime.Today.AddDays(30)
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_review_cycle_id_fails()
    {
        var dto = ValidDto(); dto.ReviewCycleId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_employee_id_fails()
    {
        var dto = ValidDto(); dto.EmployeeId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_reviewer_id_fails()
    {
        var dto = ValidDto(); dto.ReviewerId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Default_due_date_fails()
    {
        var dto = ValidDto(); dto.DueDate = default;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
