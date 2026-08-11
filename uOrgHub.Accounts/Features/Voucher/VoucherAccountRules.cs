using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.Features.Voucher;

/// <summary>
/// Which side of a voucher a given account sits on.
/// </summary>
public enum VoucherAccountRole
{
    /// <summary>Where the money physically lands or leaves from — cash, bank, or similar.</summary>
    Money,

    /// <summary>The other party or purpose the money came from or went to.</summary>
    Party
}

/// <summary>
/// The one place that decides which chart-of-accounts entries may appear on each side of a
/// voucher. Both the dropdown query and the save-time guard read these rules, so what the user
/// is offered and what the server will accept cannot drift apart.
///
/// Eligibility is derived entirely from existing chart-of-accounts data — the account's type,
/// whether it is active, and whether it allows direct entry. No account codes, names or ids are
/// baked in, so a change to the chart of accounts changes the dropdowns with no code change.
///
/// Everything is expressed per *side* (debit / credit) rather than per role, because a Contra
/// voucher has money on both sides and so has no party side at all.
/// </summary>
public static class VoucherAccountRules
{
    /// <summary>
    /// Money only ever moves through asset accounts — cash in hand, petty cash, bank accounts,
    /// and the like. Accounts backed by a <see cref="BankAccount"/> are a subset of these and are
    /// surfaced first in the dropdown, but are not the whole list: petty cash rarely has one.
    /// </summary>
    public static readonly IReadOnlyList<AccountGroupType> MoneyAccountTypes =
        new[] { AccountGroupType.Asset };

    /// <summary>
    /// What each side of each voucher type represents.
    ///
    /// Debit Voucher  — money out: credit cash/bank, debit the expense or payable.
    /// Credit Voucher — money in:  debit cash/bank, credit the income, liability or party.
    /// Contra Voucher — money moved between own accounts: both sides are money accounts.
    /// </summary>
    public static VoucherAccountRole RoleOfDebitSide(VoucherType voucherType)
        => voucherType == VoucherType.Debit ? VoucherAccountRole.Party : VoucherAccountRole.Money;

    public static VoucherAccountRole RoleOfCreditSide(VoucherType voucherType)
        => voucherType == VoucherType.Credit ? VoucherAccountRole.Party : VoucherAccountRole.Money;

    /// <summary>
    /// True when this voucher type only ever moves money between the organisation's own accounts,
    /// so it is neither a receipt nor a payment.
    /// </summary>
    public static bool IsOwnAccountTransfer(VoucherType voucherType)
        => voucherType == VoucherType.Contra;

    /// <summary>
    /// What the party side of each voucher type may be, by double-entry reasoning:
    ///
    /// Money in (Credit Voucher) — the credit must be a genuine source of funds: revenue earned
    /// (Income), money owed back (Liability, e.g. an investor loan or client advance), capital
    /// introduced (Equity), or a receivable being collected (Asset). Booking a receipt against an
    /// Expense account would mean the organisation was paid by its own cost, so Expense is out.
    ///
    /// Money out (Debit Voucher) — the debit must be a genuine destination: a cost incurred
    /// (Expense), a payable being settled (Liability), an asset acquired or advance given (Asset),
    /// or drawings against capital (Equity). Booking a payment against Income would mean revenue
    /// was created by spending, so Income is out.
    ///
    /// A Contra voucher has no party side; both of its sides are money accounts.
    ///
    /// Order matters: it is the order the dropdown groups appear in, most-likely first. Four of
    /// the five types qualify, so without that ordering the user faces a flat list of nearly the
    /// whole chart of accounts.
    /// </summary>
    public static IReadOnlyList<AccountGroupType> PartyAccountTypes(VoucherType voucherType)
        => voucherType switch
        {
            VoucherType.Credit => new[] { AccountGroupType.Income, AccountGroupType.Asset, AccountGroupType.Liability, AccountGroupType.Equity },
            VoucherType.Debit => new[] { AccountGroupType.Expense, AccountGroupType.Liability, AccountGroupType.Asset, AccountGroupType.Equity },
            _ => MoneyAccountTypes
        };

    public static IReadOnlyList<AccountGroupType> TypesFor(VoucherType voucherType, VoucherAccountRole role)
        => role == VoucherAccountRole.Money ? MoneyAccountTypes : PartyAccountTypes(voucherType);

    /// <summary>Account types permitted on one specific side of a voucher.</summary>
    public static IReadOnlyList<AccountGroupType> TypesForSide(VoucherType voucherType, bool isDebitSide)
        => TypesFor(voucherType, isDebitSide ? RoleOfDebitSide(voucherType) : RoleOfCreditSide(voucherType));

    /// <summary>
    /// The field label the user sees for a given side, so validation messages match the form.
    /// Contra needs both of its sides named distinctly even though both are money accounts.
    /// </summary>
    public static string SideLabel(VoucherType voucherType, bool isDebitSide)
        => (voucherType, isDebitSide) switch
        {
            (VoucherType.Credit, true) => "Receive Into",
            (VoucherType.Credit, false) => "Party Account",
            (VoucherType.Debit, true) => "Party Account",
            (VoucherType.Debit, false) => "Pay From",
            (VoucherType.Contra, true) => "Transfer To",
            _ => "Transfer From"
        };

    /// <summary>
    /// What each account type means on a given side, in the user's terms — shown as the group
    /// heading in the dropdown so the list reads as sections rather than one long roll.
    /// </summary>
    public static string GroupLabel(VoucherType voucherType, AccountGroupType accountType, VoucherAccountRole role, bool isBankLinked)
    {
        if (role == VoucherAccountRole.Money)
            return isBankLinked ? "Bank accounts" : "Cash and other asset accounts";

        return (voucherType, accountType) switch
        {
            (VoucherType.Credit, AccountGroupType.Income) => "Income — revenue earned",
            (VoucherType.Credit, AccountGroupType.Asset) => "Receivable — money owed to us",
            (VoucherType.Credit, AccountGroupType.Liability) => "Liability — loan or advance received",
            (VoucherType.Credit, AccountGroupType.Equity) => "Equity — capital introduced",
            (VoucherType.Debit, AccountGroupType.Expense) => "Expense — cost incurred",
            (VoucherType.Debit, AccountGroupType.Liability) => "Payable — settling what we owe",
            (VoucherType.Debit, AccountGroupType.Asset) => "Asset — purchase or advance given",
            (VoucherType.Debit, AccountGroupType.Equity) => "Equity — drawings against capital",
            _ => accountType.ToString()
        };
    }

    /// <summary>
    /// An account is postable when it is live and sits at the leaf of the hierarchy — parent
    /// and header accounts carry <c>AllowDirectEntry = false</c> and must never be posted to.
    /// </summary>
    public static bool IsPostable(Models.Entities.ChartOfAccount account)
        => !account.IsDeleted && account.IsActive && account.AllowDirectEntry;

    public static bool IsEligible(Models.Entities.ChartOfAccount account, VoucherType voucherType, VoucherAccountRole role)
        => IsPostable(account) && TypesFor(voucherType, role).Contains(account.AccountType);

    /// <summary>The number-series prefix each voucher type is issued under.</summary>
    public static string NumberPrefix(VoucherType voucherType)
        => voucherType switch
        {
            VoucherType.Debit => "DR",
            VoucherType.Credit => "CR",
            _ => "CN"
        };
}
