using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.CoreHR;

public class CreateEmployeeDocumentDto
{
    public Guid EmployeeId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FilePath { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateEmployeeDocumentDto
{
    public DocumentType DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FilePath { get; set; }
    public bool IsVerified { get; set; }
    public string? Remarks { get; set; }
}

public class EmployeeDocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FilePath { get; set; }
    public bool IsVerified { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
