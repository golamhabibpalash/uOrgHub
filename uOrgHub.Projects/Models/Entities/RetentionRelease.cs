using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

/// <summary>
/// Part of the retention held back on a project's RA bills, invoiced to the client once released
/// (typically at handover). RA bills invoice their net amount, so retention reaches AR only here.
/// </summary>
[Table("proj_retention_releases")]
public class RetentionRelease : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid InvoiceId { get; set; }
    public uOrgHub.Accounts.Models.Entities.Invoice Invoice { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public DateTime ReleaseDate { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
