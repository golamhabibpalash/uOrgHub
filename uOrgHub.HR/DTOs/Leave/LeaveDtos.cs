using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.Leave;

public class CreateLeaveTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDaysPerYear { get; set; }
    public int MaxConsecutiveDays { get; set; } = 30;
    public int MinDaysNotice { get; set; } = 0;
    public int ApprovalLevels { get; set; } = 1;
    public bool IsPaidLeave { get; set; } = true;
    public bool CarryForward { get; set; } = false;
    public int MaxCarryForwardDays { get; set; } = 0;
    public bool RequiresDocument { get; set; } = false;
    public Gender? GenderRestriction { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateLeaveTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDaysPerYear { get; set; }
    public int ApprovalLevels { get; set; } = 1;
    public bool IsPaidLeave { get; set; }
    public bool CarryForward { get; set; }
    public int MaxCarryForwardDays { get; set; }
    public bool IsActive { get; set; }
}

public class LeaveTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDaysPerYear { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinDaysNotice { get; set; }
    public int ApprovalLevels { get; set; }
    public bool IsPaidLeave { get; set; }
    public bool CarryForward { get; set; }
    public int MaxCarryForwardDays { get; set; }
    public bool RequiresDocument { get; set; }
    public Gender? GenderRestriction { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaveBalanceResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal TotalPending { get; set; }
    public decimal CarriedForward { get; set; }
    public decimal Remaining { get; set; }
}

public class CreateLeaveRequestDto
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public string? DocumentPath { get; set; }
}

public class UpdateLeaveRequestDto
{
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public string? DocumentPath { get; set; }
}

public class LeaveRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; }
    public int CurrentApprovalLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApproveLeaveRequestDto
{
    public Guid LeaveRequestId { get; set; }
    public Guid ApproverId { get; set; }
    public int ApprovalLevel { get; set; }
    public bool IsApproved { get; set; }
    public string? Comments { get; set; }
}

public class LeaveApprovalResponseDto
{
    public Guid Id { get; set; }
    public Guid LeaveRequestId { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public int ApprovalLevel { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTime? ActionedAt { get; set; }
}
