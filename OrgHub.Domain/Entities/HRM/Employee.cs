using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrgHub.Domain.Entities.HRM;

public class Employee : CommonProps
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public required string EmployeeCode { get; set; }
    public required string Name { get; set; }
    public int DesignationId { get; set; }
    public int DepartmentId { get; set; }
    public int EmployeeCategoryId { get; set; }
    public int EmployeeTypeId { get; set; }
    public string Phone { get; set; } = default!;
    public string SecondaryPhone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime JoiningDate { get; set; }
    public DateTime ResignDate { get; set; }
    public DateTime DoB { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfilePhotoPath { get; set; }
    [StringLength(4)]
    public string? BloodGroup { get; set; }

    [StringLength(100)]
    public required string Nationality { get; set; }

    public virtual required Designation Designation { get; set; }
    public virtual required Department Department { get; set; }
    public virtual required EmployeeCategory EmployeeCategory { get; set; }
    public virtual required EmployeeType EmployeeType { get; set; }
    public virtual required ICollection<EmployeeAddress> EmployeeAddress { get; set; }

}
