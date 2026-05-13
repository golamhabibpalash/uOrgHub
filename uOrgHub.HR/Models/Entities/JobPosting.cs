using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_job_postings")]
public class JobPosting : BaseEntity
{
    [Required][MaxLength(30)]   public string JobCode { get; set; } = string.Empty;
    [Required][MaxLength(200)]  public string Title { get; set; } = string.Empty;
    [MaxLength(2000)]           public string? Description { get; set; }
    [MaxLength(2000)]           public string? Requirements { get; set; }
    public int RequiredCount { get; set; } = 1;
    public int ExperienceYearsMin { get; set; } = 0;
    public int ExperienceYearsMax { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal? SalaryMin { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? SalaryMax { get; set; }
    public JobPostingStatus Status { get; set; } = JobPostingStatus.Draft;
    public DateTime? PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public Guid DesignationId { get; set; }
    public Designation Designation { get; set; } = null!;

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
