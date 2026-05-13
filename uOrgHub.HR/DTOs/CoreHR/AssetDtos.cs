using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.CoreHR;

public class CreateAssetDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AssetCategory Category { get; set; }
    public AssetCondition Condition { get; set; } = AssetCondition.New;
    public AssetStatus Status { get; set; } = AssetStatus.Available;
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? SerialNo { get; set; }
    public string? Vendor { get; set; }
    public string? Location { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string? Description { get; set; }
}

public class UpdateAssetDto
{
    public string Name { get; set; } = string.Empty;
    public AssetCategory Category { get; set; }
    public AssetCondition Condition { get; set; }
    public AssetStatus Status { get; set; }
    public string? Location { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string? Description { get; set; }
}

public class AssetResponseDto
{
    public Guid Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AssetCategory Category { get; set; }
    public AssetCondition Condition { get; set; }
    public AssetStatus Status { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchaseCost { get; set; }
    public string? SerialNo { get; set; }
    public string? Vendor { get; set; }
    public string? Location { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAssetAssignmentDto
{
    public Guid AssetId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime AssignedDate { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateAssetAssignmentDto
{
    public DateTime? ReturnedDate { get; set; }
    public string? Remarks { get; set; }
}

public class AssetAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
