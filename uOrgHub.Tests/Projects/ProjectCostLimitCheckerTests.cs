using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Services;

namespace uOrgHub.Tests.Projects;

public class ProjectCostLimitCheckerTests
{
    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_CostLimits_" + Guid.NewGuid());

    private static (Project Project, CostCenter CostCenter) SeedProject(
        AppDbContext ctx, string code, decimal contractValue)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectCode = code,
            ProjectName = code,
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            ProjectManagerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            PlannedEndDate = DateTime.UtcNow.AddMonths(3),
            ContractValue = contractValue,
        };
        var costCenter = new CostCenter
        {
            Id = Guid.NewGuid(), Code = code, Name = code, ProjectId = project.Id,
        };
        ctx.Set<Project>().Add(project);
        ctx.Set<CostCenter>().Add(costCenter);
        ctx.SaveChanges();
        return (project, costCenter);
    }

    private static ProjectCostLimitChecker NewChecker(AppDbContext ctx)
        => new(ctx, new ProjectFinancialService(ctx));

    [Fact]
    public async Task Returns_null_when_the_charge_stays_within_the_ceiling()
    {
        using var ctx = NewContext();
        var (_, cc) = SeedProject(ctx, "PRJ-001", 100_000);

        var warning = await NewChecker(ctx).CheckAsync(
            new[] { new ProjectCostAllocation(cc.Id, 80_000) });

        warning.Should().BeNull();
    }

    [Fact]
    public async Task Returns_null_exactly_at_the_ceiling()
    {
        using var ctx = NewContext();
        var (_, cc) = SeedProject(ctx, "PRJ-001", 100_000);

        var warning = await NewChecker(ctx).CheckAsync(
            new[] { new ProjectCostAllocation(cc.Id, 100_000) });

        warning.Should().BeNull();
    }

    [Fact]
    public async Task Warns_with_the_overrun_amount_when_the_charge_crosses_the_ceiling()
    {
        using var ctx = NewContext();
        var (_, cc) = SeedProject(ctx, "PRJ-001", 100_000);

        var warning = await NewChecker(ctx).CheckAsync(
            new[] { new ProjectCostAllocation(cc.Id, 120_000) });

        warning.Should().NotBeNull();
        warning.Should().Contain("PRJ-001").And.Contain("contract value").And.Contain("20,000.00");
    }

    [Fact]
    public async Task Amounts_on_the_same_cost_center_are_summed_before_comparing()
    {
        using var ctx = NewContext();
        var (_, cc) = SeedProject(ctx, "PRJ-001", 100_000);

        // Neither line alone crosses the ceiling; together they do.
        var warning = await NewChecker(ctx).CheckAsync(new[]
        {
            new ProjectCostAllocation(cc.Id, 60_000),
            new ProjectCostAllocation(cc.Id, 60_000),
        });

        warning.Should().NotBeNull();
    }

    [Fact]
    public async Task A_document_spanning_projects_is_judged_per_project()
    {
        using var ctx = NewContext();
        var (_, small) = SeedProject(ctx, "PRJ-SMALL", 10_000);
        var (_, big) = SeedProject(ctx, "PRJ-BIG", 1_000_000);

        var warning = await NewChecker(ctx).CheckAsync(new[]
        {
            new ProjectCostAllocation(small.Id, 50_000),
            new ProjectCostAllocation(big.Id, 50_000),
        });

        warning.Should().NotBeNull();
        warning.Should().Contain("PRJ-SMALL");
        warning.Should().NotContain("PRJ-BIG");
    }

    [Fact]
    public async Task Cost_centers_not_tied_to_a_project_are_ignored()
    {
        using var ctx = NewContext();
        var orphan = new CostCenter { Id = Guid.NewGuid(), Code = "ADMIN", Name = "Admin" };
        ctx.Set<CostCenter>().Add(orphan);
        ctx.SaveChanges();

        var warning = await NewChecker(ctx).CheckAsync(
            new[] { new ProjectCostAllocation(orphan.Id, 999_999) });

        warning.Should().BeNull();
    }

    [Fact]
    public async Task Existing_posted_spend_counts_toward_the_projection()
    {
        using var ctx = NewContext();
        var (_, cc) = SeedProject(ctx, "PRJ-001", 100_000);

        var expense = new ChartOfAccount
        {
            Id = Guid.NewGuid(), AccountCode = "5010", AccountName = "Materials",
            AccountType = AccountGroupType.Expense, AccountGroupId = Guid.NewGuid(),
        };
        ctx.Set<ChartOfAccount>().Add(expense);
        var je = new JournalEntry
        {
            Id = Guid.NewGuid(), EntryNumber = "JV-1", EntryDate = DateTime.UtcNow,
            Status = JournalEntryStatus.Posted, TotalDebit = 90_000, TotalCredit = 90_000,
        };
        je.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = je.Id, AccountId = expense.Id,
            DebitAmount = 90_000, LineOrder = 1, CostCenterId = cc.Id,
        });
        ctx.Set<JournalEntry>().Add(je);
        ctx.SaveChanges();

        // 90,000 already spent + 20,000 more = 110,000 against a 100,000 ceiling.
        var warning = await NewChecker(ctx).CheckAsync(
            new[] { new ProjectCostAllocation(cc.Id, 20_000) });

        warning.Should().NotBeNull();
        warning.Should().Contain("10,000.00");
    }
}
