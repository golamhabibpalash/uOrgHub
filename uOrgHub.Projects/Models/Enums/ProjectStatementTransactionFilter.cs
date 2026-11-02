namespace uOrgHub.Projects.Models.Enums;

/// <summary>
/// Which ledger lines a project statement's transaction list carries. Narrows the list only —
/// the statement's summary figures always cover the whole ledger.
/// </summary>
public enum ProjectStatementTransactionFilter
{
    All,
    Receipts,
    Payments
}
