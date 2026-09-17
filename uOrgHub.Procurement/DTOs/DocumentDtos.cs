using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CompanyInfoDto
{
    public string Name { get; init; } = string.Empty;
    public string? TagLine { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? LogoUrl { get; init; }
    public string Currency { get; init; } = "BDT";
}

public record DocumentItemDto
{
    public int LineNo { get; init; }
    public string VariantName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? UOM { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitCost { get; init; }
    public decimal? TotalCost { get; init; }
    public string? Notes { get; init; }
}

public class PRDocumentResponseDto
{
    public Guid Id { get; set; }
    public string PRNumber { get; set; } = string.Empty;
    public DateTime PRDate { get; set; }
    public DateTime RequiredDate { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public PRStatus Status { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public string? DocumentText { get; set; }
    public bool IsDocumentEdited { get; set; }
    public DateTime? DocumentEditedAt { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public CompanyInfoDto Company { get; set; } = new();
    public List<DocumentItemDto> Items { get; set; } = new();
}

public record UpdatePRDocumentDto(string? DocumentText);

public class RfqDocumentResponseDto
{
    public Guid Id { get; set; }
    public string RFQNumber { get; set; } = string.Empty;
    public DateTime RFQDate { get; set; }
    public DateTime ClosingDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PRNumber { get; set; }
    public string? PRPurpose { get; set; }
    public RFQStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? DocumentText { get; set; }
    public bool IsDocumentEdited { get; set; }
    public DateTime? DocumentEditedAt { get; set; }
    public CompanyInfoDto Company { get; set; } = new();
    public List<DocumentItemDto> Items { get; set; } = new();
}

public record UpdateRfqDocumentDto(string? DocumentText);
