using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateSubmittalDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ContractorReference { get; set; }
    public string? Description { get; set; }
    public Guid SubmittedById { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSubmittalDto
{
    public string Title { get; set; } = string.Empty;
    public string? ContractorReference { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
}

public class ReviewSubmittalDto
{
    public SubmittalStatus Status { get; set; }
    public Guid ReviewedById { get; set; }
    public DateTime ReviewDate { get; set; }
    public string? ReviewComments { get; set; }
}

public class SubmittalResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string SubmittalNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ContractorReference { get; set; }
    public string? Description { get; set; }
    public SubmittalStatus Status { get; set; }
    public Guid SubmittedById { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? ReviewComments { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
