using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Tests.Projects;

/// <summary>
/// The project form posts Status as a string, so its dropdown values have to be real
/// ProjectStatus members. When they are not, the whole body fails to deserialize and the
/// request dies as a 400 before any handler runs.
/// </summary>
public class ProjectStatusSerializationTests
{
    // Mirrors the Status dropdown in uOrgHub.Web/src/pages/projects/ProjectForm.tsx.
    private static readonly string[] FormStatusOptions =
        ["Inquiry", "Tender", "Planning", "Active", "OnHold", "Handover", "Completed", "Cancelled"];

    // Matches Program.cs, which registers JsonStringEnumConverter for all controllers.
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Theory]
    [MemberData(nameof(FormStatuses))]
    public void Every_status_offered_by_the_project_form_deserializes(string status)
    {
        var json = $$"""{"projectName":"Test","status":"{{status}}"}""";

        var dto = JsonSerializer.Deserialize<CreateProjectDto>(json, Options);

        dto.Should().NotBeNull();
        dto!.Status.Should().Be(Enum.Parse<ProjectStatus>(status));
    }

    public static TheoryData<string> FormStatuses()
    {
        var data = new TheoryData<string>();
        foreach (var status in FormStatusOptions) data.Add(status);
        return data;
    }

    [Fact]
    public void Form_options_cover_every_status_the_enum_defines()
    {
        FormStatusOptions.Should().BeEquivalentTo(Enum.GetNames<ProjectStatus>());
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("InProgress")]
    public void A_status_outside_the_enum_breaks_the_whole_body(string status)
    {
        // This is what the projects page was sending: the failure is not "invalid status",
        // it is the entire payload failing to bind, which surfaces as a bare 400.
        var json = $$"""{"projectName":"Test","status":"{{status}}"}""";

        var act = () => JsonSerializer.Deserialize<CreateProjectDto>(json, Options);

        act.Should().Throw<JsonException>();
    }
}
