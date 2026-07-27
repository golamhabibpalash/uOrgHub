using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class ProjectStatusLogResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
