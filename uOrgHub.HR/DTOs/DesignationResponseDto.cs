namespace uOrgHub.HR.DTOs;

public class DesignationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
