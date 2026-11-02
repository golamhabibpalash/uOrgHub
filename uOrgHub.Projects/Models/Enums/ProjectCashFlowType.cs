namespace uOrgHub.Projects.Models.Enums;

/// <summary>
/// Which side of the project's cash flow a statement ledger line belongs to. Taken from the
/// voucher that raised the entry — a Credit voucher is a receipt, a Debit voucher a payment —
/// the same basis the statement's Receipts and Payments figures are summed on.
/// </summary>
public enum ProjectCashFlowType
{
    Receipt,
    Payment
}
