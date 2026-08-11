using FluentAssertions;
using uOrgHub.Accounts.Features.Voucher;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Tests.Accounts;

/// <summary>
/// Pins the double-entry direction and account eligibility for vouchers. These rules drive both
/// the dropdowns and the save-time guard, so a change here silently changes what the form offers.
/// </summary>
public class VoucherAccountRulesTests
{
    private static ChartOfAccount Account(AccountGroupType type, bool active = true, bool allowDirectEntry = true)
        => new()
        {
            Id = Guid.NewGuid(),
            AccountCode = "1010",
            AccountName = "Test Account",
            AccountType = type,
            IsActive = active,
            AllowDirectEntry = allowDirectEntry
        };

    // --- Direction: which side holds the cash/bank account ---

    [Fact]
    public void Credit_voucher_takes_money_in_so_cash_sits_on_the_debit_side()
    {
        VoucherAccountRules.MoneyIsOnDebitSide(VoucherType.Credit).Should().BeTrue();
        VoucherAccountRules.RoleOfDebitSide(VoucherType.Credit).Should().Be(VoucherAccountRole.Money);
        VoucherAccountRules.RoleOfCreditSide(VoucherType.Credit).Should().Be(VoucherAccountRole.Party);
    }

    [Fact]
    public void Debit_voucher_pays_money_out_so_cash_sits_on_the_credit_side()
    {
        VoucherAccountRules.MoneyIsOnDebitSide(VoucherType.Debit).Should().BeFalse();
        VoucherAccountRules.RoleOfCreditSide(VoucherType.Debit).Should().Be(VoucherAccountRole.Money);
        VoucherAccountRules.RoleOfDebitSide(VoucherType.Debit).Should().Be(VoucherAccountRole.Party);
    }

    // --- Money side: only assets, whatever the voucher type ---

    [Theory]
    [InlineData(VoucherType.Credit)]
    [InlineData(VoucherType.Debit)]
    public void Only_asset_accounts_qualify_as_money_accounts(VoucherType voucherType)
    {
        VoucherAccountRules
            .IsEligible(Account(AccountGroupType.Asset), voucherType, VoucherAccountRole.Money)
            .Should().BeTrue();

        foreach (var type in new[]
        {
            AccountGroupType.Liability, AccountGroupType.Equity,
            AccountGroupType.Income, AccountGroupType.Expense
        })
        {
            VoucherAccountRules
                .IsEligible(Account(type), voucherType, VoucherAccountRole.Money)
                .Should().BeFalse($"{type} is not somewhere money can sit");
        }
    }

    // --- Party side: the one type that would invert the transaction is excluded ---

    [Fact]
    public void Receipt_cannot_be_credited_to_an_expense_account()
    {
        VoucherAccountRules
            .IsEligible(Account(AccountGroupType.Expense), VoucherType.Credit, VoucherAccountRole.Party)
            .Should().BeFalse();
    }

    [Theory]
    [InlineData(AccountGroupType.Income)]
    [InlineData(AccountGroupType.Liability)]
    [InlineData(AccountGroupType.Equity)]
    [InlineData(AccountGroupType.Asset)]
    public void Receipt_may_be_credited_to_any_genuine_source_of_funds(AccountGroupType type)
    {
        VoucherAccountRules
            .IsEligible(Account(type), VoucherType.Credit, VoucherAccountRole.Party)
            .Should().BeTrue();
    }

    [Fact]
    public void Payment_cannot_be_debited_to_an_income_account()
    {
        VoucherAccountRules
            .IsEligible(Account(AccountGroupType.Income), VoucherType.Debit, VoucherAccountRole.Party)
            .Should().BeFalse();
    }

    [Theory]
    [InlineData(AccountGroupType.Expense)]
    [InlineData(AccountGroupType.Liability)]
    [InlineData(AccountGroupType.Equity)]
    [InlineData(AccountGroupType.Asset)]
    public void Payment_may_be_debited_to_any_genuine_destination(AccountGroupType type)
    {
        VoucherAccountRules
            .IsEligible(Account(type), VoucherType.Debit, VoucherAccountRole.Party)
            .Should().BeTrue();
    }

    // --- Postability applies regardless of type ---

    [Fact]
    public void Inactive_account_is_never_eligible()
    {
        VoucherAccountRules
            .IsEligible(Account(AccountGroupType.Asset, active: false), VoucherType.Credit, VoucherAccountRole.Money)
            .Should().BeFalse();
    }

    [Fact]
    public void Header_account_that_forbids_direct_entry_is_never_eligible()
    {
        VoucherAccountRules
            .IsEligible(Account(AccountGroupType.Asset, allowDirectEntry: false), VoucherType.Credit, VoucherAccountRole.Money)
            .Should().BeFalse();
    }

    [Fact]
    public void Soft_deleted_account_is_never_eligible()
    {
        var account = Account(AccountGroupType.Asset);
        account.IsDeleted = true;

        VoucherAccountRules
            .IsEligible(account, VoucherType.Credit, VoucherAccountRole.Money)
            .Should().BeFalse();
    }

    // --- Labels the user sees ---

    [Fact]
    public void Money_field_is_labelled_for_the_direction_of_the_voucher()
    {
        VoucherAccountRules.FieldLabel(VoucherType.Credit, VoucherAccountRole.Money).Should().Be("Receive Into");
        VoucherAccountRules.FieldLabel(VoucherType.Debit, VoucherAccountRole.Money).Should().Be("Pay From");
        VoucherAccountRules.FieldLabel(VoucherType.Credit, VoucherAccountRole.Party).Should().Be("Party Account");
    }
}
