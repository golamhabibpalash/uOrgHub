namespace uOrgHub.HR.DTOs;

public class DesignationDependenciesDto
{
    public Guid DesignationId { get; set; }
    public int EmployeeCount { get; set; }
    public int ChildDesignationCount { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockingReason { get; set; }
}
