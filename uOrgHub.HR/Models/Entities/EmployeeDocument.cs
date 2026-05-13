using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_documents")]
public class EmployeeDocument : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DocumentType DocumentType { get; set; }
    [MaxLength(100)] public string? DocumentNumber { get; set; }
    [Required][MaxLength(500)] public string FilePath { get; set; } = string.Empty;
    [MaxLength(200)] public string? FileName { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
}
