using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs;

public class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? NationalId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public Guid DesignationId { get; set; }
    public string DesignationName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime CreatedAt { get; set; }
}
