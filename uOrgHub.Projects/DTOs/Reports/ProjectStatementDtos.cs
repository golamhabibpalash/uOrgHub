using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Projects.DTOs.Reports;

/// <summary>
/// One posted journal entry line attributed to the project, as it appears in the statement's
/// transaction ledger.
/// </summary>
public record ProjectStatementRowDto(
    DateTime EntryDate,
    string EntryNumber,
    string? ReferenceNumber,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountGroupType AccountType,
    string? Narration,
    string CostCenterName,
    decimal Debit,
    decimal Credit,
    decimal RunningNet
);

/// <summary>Period totals for one account the project was charged against.</summary>
public record ProjectStatementAccountDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountGroupType AccountType,
    decimal Debit,
    decimal Credit,
    decimal Net
);

/// <summary>
/// A project's accounting activity over a date range: what it has cost, what it has earned, and
/// the cash that moved through it, with the ledger lines those figures come from.
///
/// Every figure is derived from *posted* journal entry lines carrying one of the project's cost
/// centers — the same basis <see cref="Services.ProjectFinancialService"/> uses, so the statement
/// and the project's financial summary can never disagree.
/// </summary>
public record ProjectStatementDto(
    Guid ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal ContractValue,
    DateTime? DateFrom,
    DateTime? DateTo,

    /// <summary>Net cost booked before <see cref="DateFrom"/>, so the period reads as a movement.</summary>
    decimal OpeningSpend,
    decimal PeriodExpense,
    decimal PeriodIncome,
    decimal ClosingSpend,

    /// <summary>Money received on posted Credit Vouchers dated in the period.</summary>
    decimal Receipts,

    /// <summary>Money paid out on posted Debit Vouchers dated in the period.</summary>
    decimal Payments,
    decimal NetCashPosition,

    List<ProjectStatementAccountDto> ByAccount,
    List<ProjectStatementRowDto> Rows
);
