using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_numbering_sequences")]
public class NumberingSequence : ICompanyScoped
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    // Each sister concern must count its own document numbers independently — see
    // SISTER_CONCERN_PLAN.md §7. Stamped automatically by AuditInterceptor, scoped for reads by
    // AppDbContext's global filter, same as every other ICompanyScoped entity.
    public Guid CompanyId { get; set; }

    [Required][MaxLength(50)] public string DocumentType { get; set; } = string.Empty;

    [Required][MaxLength(20)] public string Prefix { get; set; } = string.Empty;

    public int Year { get; set; }

    public int? Month { get; set; }

    public int LastSequence { get; set; }

    [Required][MaxLength(100)] public string Pattern { get; set; } = string.Empty;
}
