using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs;

public class UpdateEmployeeDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public Guid DesignationId { get; set; }
    public Guid DepartmentId { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public decimal BasicSalary { get; set; }
}
