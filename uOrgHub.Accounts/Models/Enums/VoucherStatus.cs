namespace uOrgHub.Accounts.Models.Enums;

public enum VoucherStatus
{
    Draft,
    Submitted,
    Approved,
    Posted,
    Rejected,
    Cancelled,

    /// <summary>
    /// Was posted, then undone by a mirror journal entry ("Correct this voucher"). Its original entry
    /// stays posted in its own period; the reversal entry cancels it out from the reversal date.
    /// </summary>
    Reversed
}