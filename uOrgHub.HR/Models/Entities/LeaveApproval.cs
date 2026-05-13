using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_leave_approvals")]
public class LeaveApproval : BaseEntity
{
    public Guid LeaveRequestId { get; set; }
    public LeaveRequest LeaveRequest { get; set; } = null!;

    public Guid ApproverId { get; set; }
    public Employee Approver { get; set; } = null!;

    public int ApprovalLevel { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    [MaxLength(1000)] public string? Comments { get; set; }
    public DateTime? ActionedAt { get; set; }
}
