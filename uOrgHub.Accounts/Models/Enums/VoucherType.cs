namespace uOrgHub.Accounts.Models.Enums;

/// <summary>
/// Values are explicit because they are persisted as integers — reordering would silently
/// re-label every voucher already in the database.
/// </summary>
public enum VoucherType
{
    /// <summary>Money paid out. Credit cash/bank, debit the expense or payable.</summary>
    Debit = 0,

    /// <summary>Money received. Debit cash/bank, credit the income, liability or party.</summary>
    Credit = 1,

    /// <summary>
    /// Money moved between the organisation's own accounts — a bank withdrawal, a cash deposit,
    /// a transfer between two banks. Both sides are money accounts, so nothing enters or leaves
    /// the organisation and it counts as neither a receipt nor a payment.
    /// </summary>
    Contra = 2
}
