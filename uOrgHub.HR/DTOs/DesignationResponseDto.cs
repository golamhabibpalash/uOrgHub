namespace uOrgHub.HR.DTOs;

public class DesignationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? ParentDesignationId { get; set; }
    public string? ParentDesignationName { get; set; }
    public Guid? SalaryGradeId { get; set; }
    public DateTime CreatedAt { get; set; }
}
