using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_leave_types")]
public class LeaveType : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    public int TotalDaysPerYear { get; set; }
    public bool CarryForward { get; set; } = false;
    public int MaxCarryForwardDays { get; set; } = 0;
    public bool IsPaidLeave { get; set; } = true;
    public bool RequiresDocument { get; set; } = false;
    public int MinDaysNotice { get; set; } = 0;
    public int MaxConsecutiveDays { get; set; } = 30;
    public Gender? GenderRestriction { get; set; }
    public bool IsActive { get; set; } = true;
    public int ApprovalLevels { get; set; } = 1;

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
