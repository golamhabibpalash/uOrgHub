namespace uOrgHub.Projects.DTOs;

/// <summary>
/// One project's financial position, derived on read. Cost comes from posted journal entry lines
/// on the project's cost centers; revenue comes from RA bills, which never reach the GL.
/// </summary>
public class ProjectFinancialSummaryDto
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal ContractValue { get; set; }

    // Revenue side — what has been billed to the client.
    public decimal RABilledCertified { get; set; }
    public decimal RABilledPending { get; set; }
    public decimal RemainingToBill { get; set; }
    public decimal ContractUtilizationPercent { get; set; }
    public bool IsOverContractValue { get; set; }

    // Cost side — what has been spent on the project.
    public decimal CostCeiling { get; set; }
    public string CeilingSource { get; set; } = CostCeilingSource.ContractValue;
    public decimal ActualSpend { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal BudgetUtilizationPercent { get; set; }
    public bool IsOverBudget { get; set; }

    // Cash side — what actually moved through cash and bank on this project, from posted vouchers.
    // Deliberately separate from the billing figures above: an RA bill is revenue billed, a
    // receipt is money in hand, and the two rarely land in the same period.
    public decimal VoucherReceipts { get; set; }
    public decimal VoucherPayments { get; set; }
    public decimal NetCashPosition { get; set; }

    /// <summary>Income recognised against the project in the GL, from posted journal entry lines.</summary>
    public decimal IncomeRecognised { get; set; }

    public decimal Margin { get; set; }
    public decimal MarginPercent { get; set; }

    public List<ProjectSpendByAccountDto> SpendByAccount { get; set; } = new();
}

public static class CostCeilingSource
{
    public const string Budget = "Budget";
    public const string ContractValue = "ContractValue";
}

public class ProjectSpendByAccountDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
