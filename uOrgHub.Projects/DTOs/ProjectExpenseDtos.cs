using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateProjectExpenseDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? POId { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid RecordedById { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProjectExpenseDto
{
    public Guid? WBSId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? POId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
}

public class ApproveExpenseDto
{
    public Guid ApprovedById { get; set; }
}

public class ProjectExpenseResponseDto
{
    public Guid Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? POId { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid RecordedById { get; set; }
    public ExpenseStatus Status { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
