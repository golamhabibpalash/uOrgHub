using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class AddProjectTeamMemberDto
{
    public Guid EmployeeId { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProjectTeamMemberDto
{
    public TeamRole Role { get; set; }
    public DateTime? LeftDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class ProjectTeamResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public TeamRole Role { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LeftDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
