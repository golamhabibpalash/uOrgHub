using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_leave_balances")]
public class LeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public int Year { get; set; }
    [Column(TypeName = "decimal(5,1)")] public decimal TotalAllocated { get; set; }
    [Column(TypeName = "decimal(5,1)")] public decimal TotalUsed { get; set; }
    [Column(TypeName = "decimal(5,1)")] public decimal TotalPending { get; set; }
    [Column(TypeName = "decimal(5,1)")] public decimal CarriedForward { get; set; }
    [Column(TypeName = "decimal(5,1)")] public decimal Remaining => TotalAllocated + CarriedForward - TotalUsed - TotalPending;
}
