using FluentAssertions;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Tests.Accounts;

public class APAgingReportTests
{
    private static readonly DateTime AsOf = new(2026, 9, 30);

    private static uOrgHub.Shared.Data.AppDbContext Seed()
    {
        var ctx = TestDb.NewContext("TestDb_APAging_" + Guid.NewGuid());
        var acme = new Vendor { Id = Guid.NewGuid(), VendorCode = "V1", Name = "Acme Steel", Status = VendorStatus.Active };
        var bright = new Vendor { Id = Guid.NewGuid(), VendorCode = "V2", Name = "Bright Cement", Status = VendorStatus.Active };
        ctx.Set<Vendor>().AddRange(acme, bright);

        Bill Bill(string number, Vendor v, DateTime date, BillStatus status = BillStatus.Received, string? vendorRef = null) => new()
        {
            Id = Guid.NewGuid(), BillNumber = number, VendorBillNumber = vendorRef, VendorId = v.Id,
            FiscalYearId = Guid.NewGuid(), BillDate = date, DueDate = date.AddDays(30),
            Status = status, TotalAmount = 1000m, PaidAmount = 0m
        };

        ctx.Set<Bill>().AddRange(
            Bill("BILL-001", acme, new DateTime(2026, 6, 10), vendorRef: "INV-A-77"),
            Bill("BILL-002", acme, new DateTime(2026, 8, 5)),
            Bill("BILL-003", bright, new DateTime(2026, 8, 31, 15, 30, 0)),
            Bill("BILL-004", bright, new DateTime(2026, 9, 15)),
            Bill("BILL-005", bright, new DateTime(2026, 8, 10), BillStatus.Paid));
        ctx.SaveChanges();
        return ctx;
    }

    private static async Task<List<string>> Numbers(AgingFilterDto? filter)
    {
        using var ctx = Seed();
        var report = await new AccountingReportService(ctx).GetAPAgingReportAsync(AsOf, filter);
        return report.Rows.Select(r => r.DocumentNumber).OrderBy(n => n).ToList();
    }

    [Fact]
    public async Task Without_filters_every_open_bill_is_listed()
        => (await Numbers(null)).Should().Equal("BILL-001", "BILL-002", "BILL-003", "BILL-004");

    [Fact]
    public async Task Date_range_filters_on_bill_date_including_the_whole_last_day()
        => (await Numbers(new AgingFilterDto { DateFrom = new DateTime(2026, 8, 1), DateTo = new DateTime(2026, 8, 31) }))
            .Should().Equal("BILL-002", "BILL-003");

    [Fact]
    public async Task Open_ended_ranges_work_on_either_side()
    {
        (await Numbers(new AgingFilterDto { DateFrom = new DateTime(2026, 9, 1) })).Should().Equal("BILL-004");
        (await Numbers(new AgingFilterDto { DateTo = new DateTime(2026, 7, 1) })).Should().Equal("BILL-001");
    }

    [Theory]
    [InlineData("acme", new[] { "BILL-001", "BILL-002" })]
    [InlineData("BILL-004", new[] { "BILL-004" })]
    [InlineData("inv-a-77", new[] { "BILL-001" })]
    public async Task Search_matches_vendor_name_bill_number_or_vendor_reference_case_insensitively(string term, string[] expected)
        => (await Numbers(new AgingFilterDto { Search = term })).Should().Equal(expected);

    [Fact]
    public async Task Totals_reflect_only_the_filtered_bills()
    {
        using var ctx = Seed();
        var report = await new AccountingReportService(ctx).GetAPAgingReportAsync(AsOf, new AgingFilterDto { Search = "bright" });

        report.TotalOutstanding.Should().Be(2000m);
    }
}
