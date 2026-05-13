using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.CoreHR;

public class CreateEmployeeContractDto
{
    public Guid EmployeeId { get; set; }
    public ContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal SalaryAmount { get; set; }
    public string? FilePath { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateEmployeeContractDto
{
    public ContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal SalaryAmount { get; set; }
    public string? FilePath { get; set; }
    public bool IsActive { get; set; }
    public string? Remarks { get; set; }
}

public class EmployeeContractResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public ContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal SalaryAmount { get; set; }
    public string? FilePath { get; set; }
    public bool IsActive { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
