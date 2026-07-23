using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.RABills.Commands;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Data;

namespace uOrgHub.Tests.Projects;

public class CertifyRABillCommandHandlerTests
{
    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_CertifyRABill_" + Guid.NewGuid());

    private static Project SeedProject(AppDbContext ctx, decimal contractValue)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectCode = "PRJ-001",
            ProjectName = "Test Project",
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            ProjectManagerId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            PlannedEndDate = DateTime.UtcNow.AddMonths(6),
            ContractValue = contractValue,
        };
        ctx.Set<Project>().Add(project);
        ctx.SaveChanges();
        return project;
    }

    private static RABill SeedBill(AppDbContext ctx, Project project, decimal net,
        RABillStatus status, int sequence)
    {
        var bill = new RABill
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            BillNumber = $"RAB-{sequence:D3}",
            Title = $"Running bill {sequence}",
            BillDate = DateTime.UtcNow,
            PeriodFrom = DateTime.UtcNow.AddDays(-30),
            PeriodTo = DateTime.UtcNow,
            BillSequence = sequence,
            SubmittedById = Guid.NewGuid(),
            GrossAmount = net,
            NetAmount = net,
            Status = status,
        };
        ctx.Set<RABill>().Add(bill);
        ctx.SaveChanges();
        return bill;
    }

    private static CertifyRABillDto Certify(decimal gross) => new()
    {
        GrossAmount = gross,
        DeductionAmount = 0,
        CertifiedById = Guid.NewGuid(),
        CertifiedDate = DateTime.UtcNow,
    };

    [Fact]
    public async Task Certifying_recomputes_the_cumulative_from_previously_certified_bills()
    {
        using var ctx = NewContext();
        var project = SeedProject(ctx, 5_000_000);
        SeedBill(ctx, project, 50_000, RABillStatus.Certified, 1);
        var second = SeedBill(ctx, project, 0, RABillStatus.Submitted, 2);

        var result = await new CertifyRABillCommandHandler(ctx)
            .Handle(new CertifyRABillCommand(second.Id, Certify(30_000)), default);

        result.PreviousBilledAmount.Should().Be(50_000);
        result.CumulativeBilledAmount.Should().Be(80_000);
        result.Warning.Should().BeNull();
    }

    [Fact]
    public async Task Cumulative_reflects_the_certified_amount_not_the_draft_estimate()
    {
        using var ctx = NewContext();
        var project = SeedProject(ctx, 5_000_000);
        var bill = SeedBill(ctx, project, 10_000, RABillStatus.Submitted, 1);
        // Stale figures written when the bill was drafted.
        bill.PreviousBilledAmount = 999_999;
        bill.CumulativeBilledAmount = 999_999;
        ctx.SaveChanges();

        var result = await new CertifyRABillCommandHandler(ctx)
            .Handle(new CertifyRABillCommand(bill.Id, Certify(25_000)), default);

        result.PreviousBilledAmount.Should().Be(0);
        result.CumulativeBilledAmount.Should().Be(25_000);
    }

    [Fact]
    public async Task Draft_and_rejected_siblings_do_not_feed_the_cumulative()
    {
        using var ctx = NewContext();
        var project = SeedProject(ctx, 5_000_000);
        SeedBill(ctx, project, 40_000, RABillStatus.Certified, 1);
        SeedBill(ctx, project, 999_000, RABillStatus.Draft, 2);
        SeedBill(ctx, project, 999_000, RABillStatus.Rejected, 3);
        var current = SeedBill(ctx, project, 0, RABillStatus.Submitted, 4);

        var result = await new CertifyRABillCommandHandler(ctx)
            .Handle(new CertifyRABillCommand(current.Id, Certify(10_000)), default);

        result.PreviousBilledAmount.Should().Be(40_000);
        result.CumulativeBilledAmount.Should().Be(50_000);
    }

    [Fact]
    public async Task Certifying_past_the_contract_value_warns_but_still_certifies()
    {
        using var ctx = NewContext();
        var project = SeedProject(ctx, 100_000);
        SeedBill(ctx, project, 80_000, RABillStatus.Certified, 1);
        var second = SeedBill(ctx, project, 0, RABillStatus.Submitted, 2);

        var result = await new CertifyRABillCommandHandler(ctx)
            .Handle(new CertifyRABillCommand(second.Id, Certify(50_000)), default);

        result.Status.Should().Be(RABillStatus.Certified);
        result.CumulativeBilledAmount.Should().Be(130_000);
        result.Warning.Should().NotBeNull();
        result.Warning.Should().Contain("30,000.00");
    }

    [Fact]
    public async Task Retention_is_applied_before_the_cumulative_is_taken()
    {
        using var ctx = NewContext();
        var project = SeedProject(ctx, 5_000_000);
        var bill = SeedBill(ctx, project, 0, RABillStatus.Submitted, 1);
        bill.RetentionPercent = 10;
        ctx.SaveChanges();

        var result = await new CertifyRABillCommandHandler(ctx)
            .Handle(new CertifyRABillCommand(bill.Id, Certify(100_000)), default);

        result.RetentionAmount.Should().Be(10_000);
        result.NetAmount.Should().Be(90_000);
        result.CumulativeBilledAmount.Should().Be(90_000);
    }
}
