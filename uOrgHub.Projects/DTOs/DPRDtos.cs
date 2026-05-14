using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateDPRDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public DateTime ReportDate { get; set; }
    public Guid ReportedById { get; set; }
    public WeatherCondition WeatherCondition { get; set; }
    public string WorkDone { get; set; } = string.Empty;
    public string? Issues { get; set; }
    public string? NextDayPlan { get; set; }
    public int ManpowerCount { get; set; }
    public string? EquipmentUsed { get; set; }
}

public class UpdateDPRDto
{
    public Guid? WBSId { get; set; }
    public WeatherCondition WeatherCondition { get; set; }
    public string WorkDone { get; set; } = string.Empty;
    public string? Issues { get; set; }
    public string? NextDayPlan { get; set; }
    public int ManpowerCount { get; set; }
    public string? EquipmentUsed { get; set; }
}

public class ApproveDPRDto
{
    public Guid ApprovedById { get; set; }
}

public class DPRResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public DateTime ReportDate { get; set; }
    public Guid ReportedById { get; set; }
    public WeatherCondition WeatherCondition { get; set; }
    public string WorkDone { get; set; } = string.Empty;
    public string? Issues { get; set; }
    public string? NextDayPlan { get; set; }
    public int ManpowerCount { get; set; }
    public string? EquipmentUsed { get; set; }
    public DPRStatus Status { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
