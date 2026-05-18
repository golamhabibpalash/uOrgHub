using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateRABillDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public Guid SubmittedById { get; set; }
    public decimal RetentionPercent { get; set; }
    public string? Notes { get; set; }
    public List<CreateRABillItemDto> Items { get; set; } = new();
}

public class UpdateRABillDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal RetentionPercent { get; set; }
    public string? Notes { get; set; }
}

public class CertifyRABillDto
{
    public Guid CertifiedById { get; set; }
    public DateTime CertifiedDate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
}

public class CreateRABillItemDto
{
    public Guid? BOQItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal Rate { get; set; }
    public int Sequence { get; set; }
}

public class RABillResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public int BillSequence { get; set; }
    public Guid SubmittedById { get; set; }
    public Guid? CertifiedById { get; set; }
    public DateTime? CertifiedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal RetentionPercent { get; set; }
    public decimal RetentionAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PreviousBilledAmount { get; set; }
    public decimal CumulativeBilledAmount { get; set; }
    public RABillStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<RABillItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class RABillItemResponseDto
{
    public Guid Id { get; set; }
    public Guid RABillId { get; set; }
    public Guid? BOQItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public int Sequence { get; set; }
}
