using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs;

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }

    public Gender Gender { get; set; }
    public Religion Religion { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? BirthPlace { get; set; }
    public string? NationalId { get; set; }
    public string? BirthCertificateNo { get; set; }
    public string? PassportNo { get; set; }
    public DateTime? PassportExpiry { get; set; }
    public string? TIN { get; set; }
    public string Nationality { get; set; } = "Bangladeshi";

    public string? PermanentAddress { get; set; }
    public string? CurrentAddress { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public string? Upazila { get; set; }

    public EmploymentType EmploymentType { get; set; }
    public EmployeeStatus Status { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }

    public Guid DesignationId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? SalaryGradeId { get; set; }
    public decimal BasicSalary { get; set; }
}
