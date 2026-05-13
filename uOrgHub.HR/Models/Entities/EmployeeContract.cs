using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_contracts")]
public class EmployeeContract : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public ContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [MaxLength(2000)] public string? Terms { get; set; }
    [MaxLength(500)]  public string? FilePath { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(500)]  public string? Remarks { get; set; }
}
