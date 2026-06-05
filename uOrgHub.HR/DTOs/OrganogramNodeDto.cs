namespace uOrgHub.HR.DTOs;

public class OrganogramNodeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    public string DesignationName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public int Depth { get; set; }
    public bool HasChildren { get; set; }
    public List<OrganogramNodeDto> Children { get; set; } = [];
}
