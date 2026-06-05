using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs;

public class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public Gender Gender { get; set; }
    public Religion Religion { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNo { get; set; }
    public string Nationality { get; set; } = string.Empty;

    public string? PermanentAddress { get; set; }
    public string? CurrentAddress { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }

    public DateTime JoiningDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public EmployeeStatus Status { get; set; }

    public Guid DesignationId { get; set; }
    public string DesignationName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public Guid? SalaryGradeId { get; set; }
    public decimal BasicSalary { get; set; }

    public string? ProfilePicturePath { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}
