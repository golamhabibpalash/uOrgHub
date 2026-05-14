using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateProjectDto
{
    public string ProjectName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProjectManagerId { get; set; }
    public string? Location { get; set; }
    public string? SiteAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Inquiry;
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProjectDto
{
    public string ProjectName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ProjectManagerId { get; set; }
    public string? Location { get; set; }
    public string? SiteAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ProjectManagerId { get; set; }
    public string? Location { get; set; }
    public string? SiteAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal ContractValue { get; set; }
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProjectDashboardDto
{
    public ProjectResponseDto Project { get; set; } = null!;
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal BudgetUtilizationPercent { get; set; }
    public decimal OverallCompletionPercent { get; set; }
    public int TotalWBSItems { get; set; }
    public int CompletedWBSItems { get; set; }
    public int TotalMilestones { get; set; }
    public int AchievedMilestones { get; set; }
    public int TeamCount { get; set; }
    public int DPRCount { get; set; }
    public List<ProjectTeamResponseDto> Team { get; set; } = new();
    public List<ProjectMilestoneResponseDto> UpcomingMilestones { get; set; } = new();
}

public class ProjectBudgetSummaryDto
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public decimal TotalAllocated { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRevised { get; set; }
    public decimal RemainingBudget { get; set; }
    public List<ProjectBudgetResponseDto> Budgets { get; set; } = new();
}

public class ProjectProgressDto
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public decimal OverallCompletionPercent { get; set; }
    public int TotalWBSItems { get; set; }
    public int CompletedWBSItems { get; set; }
    public int InProgressWBSItems { get; set; }
    public List<WBSResponseDto> WBSTree { get; set; } = new();
}
