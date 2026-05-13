using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_leave_requests")]
public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Column(TypeName = "decimal(5,1)")] public decimal TotalDays { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Draft;
    public int CurrentApprovalLevel { get; set; } = 0;
    [MaxLength(500)] public string? DocumentPath { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }

    public ICollection<LeaveApproval> Approvals { get; set; } = new List<LeaveApproval>();
}
