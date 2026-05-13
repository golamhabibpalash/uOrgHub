using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_payroll_cycles")]
public class PayrollCycle : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    [Required][MaxLength(100)] public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

    [Column(TypeName = "decimal(18,2)")] public decimal TotalBasic { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalAllowances { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalDeductions { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalNetPay { get; set; }
    public int TotalEmployees { get; set; }

    public Guid? ProcessedBy { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }

    public ICollection<PayrollEntry> Entries { get; set; } = new List<PayrollEntry>();
}
