using FluentAssertions;
using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.HR.DTOs.Validators;

namespace uOrgHub.Tests.HR.Validators;

public class CreateJobPostingDtoValidatorTests
{
    private readonly CreateJobPostingDtoValidator _validator = new();

    private CreateJobPostingDto ValidDto() => new()
    {
        JobCode = "JOB-001",
        Title = "Senior Developer",
        DepartmentId = Guid.NewGuid(),
        DesignationId = Guid.NewGuid(),
        RequiredCount = 2
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_job_code_fails()
    {
        var dto = ValidDto(); dto.JobCode = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Job_code_over_30_chars_fails()
    {
        var dto = ValidDto(); dto.JobCode = new string('J', 31);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_title_fails()
    {
        var dto = ValidDto(); dto.Title = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Title_over_200_chars_fails()
    {
        var dto = ValidDto(); dto.Title = new string('T', 201);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_department_id_fails()
    {
        var dto = ValidDto(); dto.DepartmentId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_designation_id_fails()
    {
        var dto = ValidDto(); dto.DesignationId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_required_count_fails()
    {
        var dto = ValidDto(); dto.RequiredCount = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Closing_date_before_posted_date_fails()
    {
        var dto = ValidDto();
        dto.PostedDate = DateTime.Today.AddDays(10);
        dto.ClosingDate = DateTime.Today;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Closing date must be after posted date"));
    }

    [Fact]
    public void Closing_date_after_posted_date_passes()
    {
        var dto = ValidDto();
        dto.PostedDate = DateTime.Today;
        dto.ClosingDate = DateTime.Today.AddDays(30);
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }
}

public class CreateCandidateDtoValidatorTests
{
    private readonly CreateCandidateDtoValidator _validator = new();

    private CreateCandidateDto ValidDto() => new()
    {
        FirstName = "Alice",
        LastName = "Smith",
        Email = "alice@example.com",
        TotalExperienceYears = 3
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_first_name_fails()
    {
        var dto = ValidDto(); dto.FirstName = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_last_name_fails()
    {
        var dto = ValidDto(); dto.LastName = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var dto = ValidDto(); dto.Email = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var dto = ValidDto(); dto.Email = "not-an-email";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_experience_years_fails()
    {
        var dto = ValidDto(); dto.TotalExperienceYears = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_experience_years_passes()
    {
        var dto = ValidDto(); dto.TotalExperienceYears = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }
}

public class CreateJobApplicationDtoValidatorTests
{
    private readonly CreateJobApplicationDtoValidator _validator = new();

    [Fact]
    public void Valid_dto_passes()
    {
        var dto = new CreateJobApplicationDto
        {
            CandidateId = Guid.NewGuid(),
            JobPostingId = Guid.NewGuid()
        };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_candidate_id_fails()
    {
        var dto = new CreateJobApplicationDto { CandidateId = Guid.Empty, JobPostingId = Guid.NewGuid() };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_job_posting_id_fails()
    {
        var dto = new CreateJobApplicationDto { CandidateId = Guid.NewGuid(), JobPostingId = Guid.Empty };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
