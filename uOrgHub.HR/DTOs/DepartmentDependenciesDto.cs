namespace uOrgHub.HR.DTOs;

public class DepartmentDependenciesDto
{
    public Guid DepartmentId { get; set; }
    public int ActiveEmployees { get; set; }
    public int ActiveDesignations { get; set; }
    public int ActiveChildDepartments { get; set; }
    public int ActiveJobPostings { get; set; }
    public int ActiveKpis { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockingReason { get; set; }
}
