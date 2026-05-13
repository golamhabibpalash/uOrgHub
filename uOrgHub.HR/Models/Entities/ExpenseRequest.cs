using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_expense_requests")]
public class ExpenseRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public ExpenseCategory Category { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    [Required][MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(500)] public string? ReceiptFilePath { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;

    public Guid? ApproverId { get; set; }
    public Employee? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }
}
