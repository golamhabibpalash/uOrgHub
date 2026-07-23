using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Projects.Services;
using uOrgHub.Shared.Data;

namespace uOrgHub.Tests.Projects;

public class ProjectFinancialServiceTests
{
    private const decimal ContractValue = 5_000_000m;

    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_ProjectFinancials_" + Guid.NewGuid());

    private sealed record Fixture(Project Project, CostCenter CostCenter, ChartOfAccount Expense, ChartOfAccount Payable);

    private static Fixture Seed(AppDbContext ctx, decimal contractValue = ContractValue)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectCode = "PRJ-001",
            ProjectName = "Test Project",
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            ProjectManagerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddMonths(-1),
            PlannedEndDate = DateTime.UtcNow.AddMonths(6),
            ContractValue = contractValue,
        };

        var costCenter = new CostCenter
        {
            Id = Guid.NewGuid(),
            Code = "PRJ-001",
            Name = "Test Project",
            ProjectId = project.Id,
        };

        var expense = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountCode = "5010",
            AccountName = "Construction Materials",
            AccountType = AccountGroupType.Expense,
            AccountGroupId = Guid.NewGuid(),
        };

        var payable = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountCode = "2010",
            AccountName = "Accounts Payable",
            AccountType = AccountGroupType.Liability,
            AccountGroupId = Guid.NewGuid(),
        };

        ctx.Set<Project>().Add(project);
        ctx.Set<CostCenter>().Add(costCenter);
        ctx.Set<ChartOfAccount>().AddRange(expense, payable);
        ctx.SaveChanges();

        return new Fixture(project, costCenter, expense, payable);
    }

    /// <summary>A bill posting: debit expense, credit payable — both stamped with the cost center.</summary>
    private static void PostBill(AppDbContext ctx, Fixture f, decimal amount,
        JournalEntryStatus status = JournalEntryStatus.Posted)
    {
        var je = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = "JV-" + Guid.NewGuid().ToString()[..8],
            EntryDate = DateTime.UtcNow,
            Status = status,
            TotalDebit = amount,
            TotalCredit = amount,
        };
        je.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = je.Id, AccountId = f.Expense.Id,
            DebitAmount = amount, LineOrder = 1, CostCenterId = f.CostCenter.Id,
        });
        je.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = je.Id, AccountId = f.Payable.Id,
            CreditAmount = amount, LineOrder = 2, CostCenterId = f.CostCenter.Id,
        });
        ctx.Set<JournalEntry>().Add(je);
        ctx.SaveChanges();
    }

    private static void AddRABill(AppDbContext ctx, Project project, decimal net, RABillStatus status)
    {
        ctx.Set<RABill>().Add(new RABill
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            BillNumber = "RAB-" + Guid.NewGuid().ToString()[..4],
            Title = "Running bill",
            BillDate = DateTime.UtcNow,
            PeriodFrom = DateTime.UtcNow.AddDays(-30),
            PeriodTo = DateTime.UtcNow,
            SubmittedById = Guid.NewGuid(),
            NetAmount = net,
            Status = status,
        });
        ctx.SaveChanges();
    }

    // --- Cost side ---

    [Fact]
    public async Task Spend_sums_posted_expense_lines_on_the_projects_cost_centers()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        PostBill(ctx, f, 50_000);
        PostBill(ctx, f, 30_000);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.ActualSpend.Should().Be(80_000);
    }

    [Fact]
    public async Task Draft_journal_entries_are_not_counted_as_spend()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        PostBill(ctx, f, 50_000);
        PostBill(ctx, f, 30_000, JournalEntryStatus.Draft);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.ActualSpend.Should().Be(50_000);
    }

    [Fact]
    public async Task Paying_a_bill_does_not_double_count_the_cost()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        PostBill(ctx, f, 50_000);

        // Payment entry: debit AP, credit bank. No expense account, so spend must not move.
        var bank = new ChartOfAccount
        {
            Id = Guid.NewGuid(), AccountCode = "1010", AccountName = "Bank",
            AccountType = AccountGroupType.Asset, AccountGroupId = Guid.NewGuid(),
        };
        ctx.Set<ChartOfAccount>().Add(bank);
        var payment = new JournalEntry
        {
            Id = Guid.NewGuid(), EntryNumber = "JV-PAY", EntryDate = DateTime.UtcNow,
            Status = JournalEntryStatus.Posted, TotalDebit = 50_000, TotalCredit = 50_000,
        };
        payment.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = payment.Id, AccountId = f.Payable.Id,
            DebitAmount = 50_000, LineOrder = 1, CostCenterId = f.CostCenter.Id,
        });
        payment.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = payment.Id, AccountId = bank.Id,
            CreditAmount = 50_000, LineOrder = 2, CostCenterId = f.CostCenter.Id,
        });
        ctx.Set<JournalEntry>().Add(payment);
        ctx.SaveChanges();

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.ActualSpend.Should().Be(50_000);
    }

    [Fact]
    public async Task Spend_on_another_projects_cost_center_is_excluded()
    {
        using var ctx = NewContext();
        var mine = Seed(ctx);
        PostBill(ctx, mine, 50_000);

        var otherProject = new Project
        {
            Id = Guid.NewGuid(), ProjectCode = "PRJ-002", ProjectName = "Other",
            ClientId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), ProjectManagerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow, PlannedEndDate = DateTime.UtcNow.AddMonths(3),
            ContractValue = 1_000_000,
        };
        var otherCc = new CostCenter
        {
            Id = Guid.NewGuid(), Code = "PRJ-002", Name = "Other", ProjectId = otherProject.Id,
        };
        ctx.Set<Project>().Add(otherProject);
        ctx.Set<CostCenter>().Add(otherCc);
        ctx.SaveChanges();
        PostBill(ctx, mine with { CostCenter = otherCc }, 999_000);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(mine.Project.Id);

        summary.ActualSpend.Should().Be(50_000);
    }

    // --- Ceiling ---

    [Fact]
    public async Task Ceiling_falls_back_to_contract_value_when_no_budget_rows_exist()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.CostCeiling.Should().Be(ContractValue);
        summary.CeilingSource.Should().Be(CostCeilingSource.ContractValue);
    }

    [Fact]
    public async Task Ceiling_uses_revised_amount_in_preference_to_allocated()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        ctx.Set<ProjectBudget>().AddRange(
            new ProjectBudget { Id = Guid.NewGuid(), ProjectId = f.Project.Id, AllocatedAmount = 1_000_000, RevisedAmount = 1_200_000 },
            new ProjectBudget { Id = Guid.NewGuid(), ProjectId = f.Project.Id, AllocatedAmount = 500_000 });
        ctx.SaveChanges();

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.CostCeiling.Should().Be(1_700_000);
        summary.CeilingSource.Should().Be(CostCeilingSource.Budget);
    }

    [Fact]
    public async Task IsOverBudget_flips_only_past_the_ceiling_not_at_it()
    {
        using var ctx = NewContext();
        var f = Seed(ctx, contractValue: 100_000);
        PostBill(ctx, f, 100_000);

        var atCeiling = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);
        atCeiling.IsOverBudget.Should().BeFalse();
        atCeiling.RemainingBudget.Should().Be(0);
        atCeiling.BudgetUtilizationPercent.Should().Be(100);

        PostBill(ctx, f, 1);
        var overCeiling = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);
        overCeiling.IsOverBudget.Should().BeTrue();
        overCeiling.RemainingBudget.Should().Be(-1);
    }

    // --- Revenue side ---

    [Fact]
    public async Task Only_certified_and_paid_RA_bills_count_as_billed()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        AddRABill(ctx, f.Project, 50_000, RABillStatus.Certified);
        AddRABill(ctx, f.Project, 30_000, RABillStatus.Paid);
        AddRABill(ctx, f.Project, 20_000, RABillStatus.Submitted);
        AddRABill(ctx, f.Project, 999_000, RABillStatus.Draft);
        AddRABill(ctx, f.Project, 999_000, RABillStatus.Rejected);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.RABilledCertified.Should().Be(80_000);
        summary.RABilledPending.Should().Be(20_000);
        summary.RemainingToBill.Should().Be(ContractValue - 80_000);
        summary.IsOverContractValue.Should().BeFalse();
    }

    [Fact]
    public async Task Billing_past_the_contract_value_is_flagged()
    {
        using var ctx = NewContext();
        var f = Seed(ctx, contractValue: 100_000);
        AddRABill(ctx, f.Project, 120_000, RABillStatus.Certified);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.IsOverContractValue.Should().BeTrue();
        summary.RemainingToBill.Should().Be(-20_000);
        summary.ContractUtilizationPercent.Should().Be(120);
    }

    // --- Margin and breakdown ---

    [Fact]
    public async Task Margin_is_certified_billing_less_actual_spend()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        AddRABill(ctx, f.Project, 100_000, RABillStatus.Certified);
        PostBill(ctx, f, 80_000);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.Margin.Should().Be(20_000);
        summary.MarginPercent.Should().Be(20);
    }

    [Fact]
    public async Task Spend_breaks_down_by_account()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        PostBill(ctx, f, 50_000);
        PostBill(ctx, f, 30_000);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.SpendByAccount.Should().ContainSingle();
        summary.SpendByAccount[0].AccountCode.Should().Be("5010");
        summary.SpendByAccount[0].Amount.Should().Be(80_000);
    }

    [Fact]
    public async Task A_project_with_no_activity_reports_zeroes_not_errors()
    {
        using var ctx = NewContext();
        var f = Seed(ctx, contractValue: 0);

        var summary = await new ProjectFinancialService(ctx).GetSummaryAsync(f.Project.Id);

        summary.ActualSpend.Should().Be(0);
        summary.RABilledCertified.Should().Be(0);
        summary.BudgetUtilizationPercent.Should().Be(0);
        summary.ContractUtilizationPercent.Should().Be(0);
        summary.MarginPercent.Should().Be(0);
        summary.IsOverBudget.Should().BeFalse();
    }
}
