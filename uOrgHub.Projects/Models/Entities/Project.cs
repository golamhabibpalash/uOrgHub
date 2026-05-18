using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_projects")]
public class Project : BaseEntity
{
    [Required][MaxLength(20)]   public string ProjectCode { get; set; } = string.Empty;
    [Required][MaxLength(300)]  public string ProjectName { get; set; } = string.Empty;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public ProjectCategory Category { get; set; } = null!;

    public Guid ProjectManagerId { get; set; }

    [MaxLength(500)]  public string? Location { get; set; }
    [MaxLength(500)]  public string? SiteAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal ContractValue { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Inquiry;
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public ICollection<ProjectTeam> Team { get; set; } = new List<ProjectTeam>();
    public ICollection<WorkBreakdownStructure> WBSItems { get; set; } = new List<WorkBreakdownStructure>();
    public ICollection<BillOfQuantity> BOQs { get; set; } = new List<BillOfQuantity>();
    public ICollection<ProjectBudget> Budgets { get; set; } = new List<ProjectBudget>();
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    public ICollection<DailyProgressReport> DPRs { get; set; } = new List<DailyProgressReport>();
    public ICollection<ProjectMaterialRequest> MaterialRequests { get; set; } = new List<ProjectMaterialRequest>();
    public ICollection<ProjectExpense> Expenses { get; set; } = new List<ProjectExpense>();
    public ICollection<ProjectDrawing> Drawings { get; set; } = new List<ProjectDrawing>();
    public ICollection<ProjectRFI> RFIs { get; set; } = new List<ProjectRFI>();
    public ICollection<ProjectSubmittal> Submittals { get; set; } = new List<ProjectSubmittal>();
    public ICollection<SiteResourceAllocation> ResourceAllocations { get; set; } = new List<SiteResourceAllocation>();
    public ICollection<QAChecklist> QAChecklists { get; set; } = new List<QAChecklist>();
    public ICollection<NonConformanceReport> NCRs { get; set; } = new List<NonConformanceReport>();
    public ICollection<SafetyIncident> SafetyIncidents { get; set; } = new List<SafetyIncident>();
    public ICollection<RABill> RABills { get; set; } = new List<RABill>();
}
