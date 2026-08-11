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
        VoucherAccountRules.RoleOfDebitSide(VoucherType.Credit).Should().Be(VoucherAccountRole.Money);
        VoucherAccountRules.RoleOfCreditSide(VoucherType.Credit).Should().Be(VoucherAccountRole.Party);
    }

    [Fact]
    public void Debit_voucher_pays_money_out_so_cash_sits_on_the_credit_side()
    {
        VoucherAccountRules.RoleOfCreditSide(VoucherType.Debit).Should().Be(VoucherAccountRole.Money);
        VoucherAccountRules.RoleOfDebitSide(VoucherType.Debit).Should().Be(VoucherAccountRole.Party);
    }

    // --- Contra: money on both sides, so no party side at all ---

    [Fact]
    public void Contra_voucher_has_money_accounts_on_both_sides()
    {
        VoucherAccountRules.RoleOfDebitSide(VoucherType.Contra).Should().Be(VoucherAccountRole.Money);
        VoucherAccountRules.RoleOfCreditSide(VoucherType.Contra).Should().Be(VoucherAccountRole.Money);
    }

    [Fact]
    public void Contra_voucher_is_the_only_own_account_transfer()
    {
        VoucherAccountRules.IsOwnAccountTransfer(VoucherType.Contra).Should().BeTrue();
        VoucherAccountRules.IsOwnAccountTransfer(VoucherType.Credit).Should().BeFalse();
        VoucherAccountRules.IsOwnAccountTransfer(VoucherType.Debit).Should().BeFalse();
    }

    [Theory]
    [InlineData(AccountGroupType.Income)]
    [InlineData(AccountGroupType.Expense)]
    [InlineData(AccountGroupType.Liability)]
    [InlineData(AccountGroupType.Equity)]
    public void Contra_voucher_rejects_anything_that_is_not_an_asset_on_either_side(AccountGroupType type)
    {
        VoucherAccountRules
            .IsEligible(Account(type), VoucherType.Contra, VoucherAccountRole.Money)
            .Should().BeFalse();

        VoucherAccountRules.TypesForSide(VoucherType.Contra, isDebitSide: true)
            .Should().NotContain(type);
        VoucherAccountRules.TypesForSide(VoucherType.Contra, isDebitSide: false)
            .Should().NotContain(type);
    }

    [Fact]
    public void Contra_voucher_accepts_asset_accounts_on_both_sides()
    {
        VoucherAccountRules.TypesForSide(VoucherType.Contra, isDebitSide: true)
            .Should().Equal(AccountGroupType.Asset);
        VoucherAccountRules.TypesForSide(VoucherType.Contra, isDebitSide: false)
            .Should().Equal(AccountGroupType.Asset);
    }

    [Fact]
    public void Each_voucher_type_has_its_own_number_series()
    {
        VoucherAccountRules.NumberPrefix(VoucherType.Debit).Should().Be("DR");
        VoucherAccountRules.NumberPrefix(VoucherType.Credit).Should().Be("CR");
        VoucherAccountRules.NumberPrefix(VoucherType.Contra).Should().Be("CN");
    }

    [Fact]
    public void Persisted_voucher_type_values_are_pinned()
    {
        // Stored as integers, so reordering would silently re-label existing vouchers.
        ((int)VoucherType.Debit).Should().Be(0);
        ((int)VoucherType.Credit).Should().Be(1);
        ((int)VoucherType.Contra).Should().Be(2);
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

    // --- Party ordering drives the dropdown grouping, so the order itself is behaviour ---

    [Fact]
    public void Receipt_lists_income_first_since_that_is_the_usual_source()
    {
        VoucherAccountRules.PartyAccountTypes(VoucherType.Credit)
            .Should().StartWith(new[] { AccountGroupType.Income });
    }

    [Fact]
    public void Payment_lists_expense_first_since_that_is_the_usual_destination()
    {
        VoucherAccountRules.PartyAccountTypes(VoucherType.Debit)
            .Should().StartWith(new[] { AccountGroupType.Expense });
    }

    [Theory]
    [InlineData(VoucherType.Credit)]
    [InlineData(VoucherType.Debit)]
    public void Every_party_type_has_a_group_label(VoucherType voucherType)
    {
        foreach (var type in VoucherAccountRules.PartyAccountTypes(voucherType))
        {
            VoucherAccountRules.GroupLabel(voucherType, type, VoucherAccountRole.Party, isBankLinked: false)
                .Should().NotBeNullOrWhiteSpace()
                .And.NotBe(type.ToString(), "each party type needs a plain-language heading");
        }
    }

    [Fact]
    public void Money_side_is_grouped_by_whether_a_bank_account_is_attached()
    {
        VoucherAccountRules
            .GroupLabel(VoucherType.Credit, AccountGroupType.Asset, VoucherAccountRole.Money, isBankLinked: true)
            .Should().Be("Bank accounts");
        VoucherAccountRules
            .GroupLabel(VoucherType.Credit, AccountGroupType.Asset, VoucherAccountRole.Money, isBankLinked: false)
            .Should().Be("Cash and other asset accounts");
    }

    // --- Labels the user sees ---

    [Fact]
    public void Each_side_is_labelled_for_the_direction_of_the_voucher()
    {
        VoucherAccountRules.SideLabel(VoucherType.Credit, isDebitSide: true).Should().Be("Receive Into");
        VoucherAccountRules.SideLabel(VoucherType.Credit, isDebitSide: false).Should().Be("Party Account");
        VoucherAccountRules.SideLabel(VoucherType.Debit, isDebitSide: true).Should().Be("Party Account");
        VoucherAccountRules.SideLabel(VoucherType.Debit, isDebitSide: false).Should().Be("Pay From");
    }

    [Fact]
    public void Contra_sides_are_named_by_direction_of_travel()
    {
        VoucherAccountRules.SideLabel(VoucherType.Contra, isDebitSide: true).Should().Be("Transfer To");
        VoucherAccountRules.SideLabel(VoucherType.Contra, isDebitSide: false).Should().Be("Transfer From");
    }
}
