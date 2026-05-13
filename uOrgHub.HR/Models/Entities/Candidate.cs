using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_candidates")]
public class Candidate : BaseEntity
{
    [Required][MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(20)]            public string? Phone { get; set; }
    [MaxLength(500)]           public string? ResumeFilePath { get; set; }
    [MaxLength(500)]           public string? LinkedInProfile { get; set; }
    [MaxLength(500)]           public string? PortfolioUrl { get; set; }
    public int TotalExperienceYears { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? CurrentSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? ExpectedSalary { get; set; }
    [MaxLength(200)] public string? CurrentCompany { get; set; }
    [MaxLength(200)] public string? CurrentDesignation { get; set; }
    [MaxLength(100)] public string? Source { get; set; }
    [MaxLength(100)] public string? ReferredBy { get; set; }
    public Gender? Gender { get; set; }
    [MaxLength(500)] public string? Skills { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
