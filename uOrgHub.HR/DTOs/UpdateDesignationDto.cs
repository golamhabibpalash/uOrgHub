namespace uOrgHub.HR.DTOs;

public class UpdateDesignationDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public bool IsActive { get; set; }
}
