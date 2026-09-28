using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_fiscalyears")]
public class FiscalYear : BaseEntity, ICompanyScoped
{
    // Sister-concern isolation (SISTER_CONCERN_PLAN.md) — each sister concern closes its own
    // periods independently, so "the current fiscal year" (FiscalYearService.GetCurrentAsync/
    // SetCurrentAsync) must resolve per company. Needs no handler changes: both already query
    // through AppDbContext's global company-scope filter.
    public Guid CompanyId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public FiscalYearStatus Status { get; set; } = FiscalYearStatus.Pending;

    public bool IsCurrent { get; set; } = false;
}