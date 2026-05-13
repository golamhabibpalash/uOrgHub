using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employees")]
public class Employee : BaseEntity
{
    // Identity
    [Required][MaxLength(20)]  public string EmployeeCode { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(100)]           public string? MiddleName { get; set; }
    [Required][MaxLength(100)] public string LastName { get; set; } = string.Empty;

    // Contact
    [Required][MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(200)]           public string? PersonalEmail { get; set; }
    [MaxLength(20)]            public string? Phone { get; set; }
    [MaxLength(20)]            public string? MobilePhone { get; set; }

    // Personal (Bangladesh context)
    public Gender Gender { get; set; }
    public Religion Religion { get; set; } = Religion.Islam;
    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;
    public BloodGroup? BloodGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(100)] public string? BirthPlace { get; set; }
    [MaxLength(30)]  public string? NationalId { get; set; }
    [MaxLength(30)]  public string? BirthCertificateNo { get; set; }
    [MaxLength(30)]  public string? PassportNo { get; set; }
    public DateTime? PassportExpiry { get; set; }
    [MaxLength(30)]  public string? TIN { get; set; }
    [MaxLength(50)]  public string Nationality { get; set; } = "Bangladeshi";

    // Address
    [MaxLength(500)] public string? PermanentAddress { get; set; }
    [MaxLength(500)] public string? CurrentAddress { get; set; }
    [MaxLength(100)] public string? District { get; set; }
    [MaxLength(100)] public string? Division { get; set; }

    // Employment
    public DateTime JoiningDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public Guid DesignationId { get; set; }
    public Designation Designation { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    public Guid? SalaryGradeId { get; set; }
    public SalaryGrade? SalaryGrade { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal BasicSalary { get; set; }
    [MaxLength(500)] public string? ProfilePicturePath { get; set; }

    // Navigation
    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeContract> Contracts { get; set; } = new List<EmployeeContract>();
    public ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<PayrollEntry> PayrollEntries { get; set; } = new List<PayrollEntry>();
}
