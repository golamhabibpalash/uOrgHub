using FluentAssertions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Shared;

public class ApiResponseTests
{
    [Fact]
    public void Ok_sets_success_true_and_data()
    {
        var response = ApiResponse<string>.Ok("hello");
        response.Success.Should().BeTrue();
        response.Data.Should().Be("hello");
        response.Message.Should().Be("Success");
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ok_accepts_custom_message()
    {
        var response = ApiResponse<int>.Ok(42, "Created");
        response.Message.Should().Be("Created");
        response.Data.Should().Be(42);
    }

    [Fact]
    public void Fail_sets_success_false_and_message()
    {
        var response = ApiResponse<string>.Fail("Something went wrong");
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Something went wrong");
        response.Data.Should().BeNull();
    }

    [Fact]
    public void Fail_includes_errors_list()
    {
        var errors = new List<string> { "Field A is required", "Field B is invalid" };
        var response = ApiResponse<object>.Fail("Validation failed", errors);
        response.Errors.Should().HaveCount(2);
        response.Errors.Should().Contain("Field A is required");
    }

    [Fact]
    public void Fail_with_null_errors_defaults_to_empty_list()
    {
        var response = ApiResponse<object>.Fail("Error", null);
        response.Errors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void NotFound_sets_success_false()
    {
        var response = ApiResponse<string>.NotFound();
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Record not found");
    }

    [Fact]
    public void NotFound_accepts_custom_message()
    {
        var response = ApiResponse<string>.NotFound("Department not found");
        response.Message.Should().Be("Department not found");
    }

    [Fact]
    public void Timestamp_is_set_on_creation()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var response = ApiResponse<string>.Ok("data");
        response.Timestamp.Should().BeAfter(before);
    }
}
