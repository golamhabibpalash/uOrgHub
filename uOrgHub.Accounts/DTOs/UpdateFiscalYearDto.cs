using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class UpdateFiscalYearDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public FiscalYearStatus Status { get; set; } = FiscalYearStatus.Pending;
    public bool IsCurrent { get; set; } = false;
}