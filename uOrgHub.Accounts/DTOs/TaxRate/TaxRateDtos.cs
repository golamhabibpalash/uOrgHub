using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.TaxRate;

public class CreateTaxRateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TaxType TaxType { get; set; }
    public decimal Rate { get; set; }
    public string? Description { get; set; }
    public Guid? TaxAccountId { get; set; }
}

public class UpdateTaxRateDto
{
    public string Name { get; set; } = string.Empty;
    public TaxType TaxType { get; set; }
    public decimal Rate { get; set; }
    public string? Description { get; set; }
    public Guid? TaxAccountId { get; set; }
    public bool IsActive { get; set; }
}

public class TaxRateResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TaxType TaxType { get; set; }
    public decimal Rate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid? TaxAccountId { get; set; }
}
