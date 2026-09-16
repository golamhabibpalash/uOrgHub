using FluentAssertions;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Reporting.Pdf;

namespace uOrgHub.Tests.Accounts;

/// <summary>
/// Smoke tests for the QuestPDF report builders — one per structural pattern (flat table,
/// grouped/running-balance, recursive tree, multi-section, aging summary), asserting each
/// produces a non-empty, well-formed PDF without throwing. Not layout/pixel assertions.
/// </summary>
public class ReportPdfDocumentTests
{
    private static void AssertValidPdf(byte[] bytes)
    {
        bytes.Should().NotBeNullOrEmpty();
        // "%PDF-" magic header identifies a well-formed PDF file.
        System.Text.Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public void TrialBalance_flat_table_produces_valid_pdf()
    {
        var report = new TrialBalanceResponseDto(
            Rows:
            [
                new TrialBalanceRowDto(Guid.NewGuid(), "1000", "Cash", "Current Assets", AccountGroupType.Asset,
                    1000m, 0m, 500m, 200m, 1300m, 0m)
            ],
            TotalOpeningDebit: 1000m, TotalOpeningCredit: 0m,
            TotalDebit: 500m, TotalCredit: 200m,
            TotalClosingDebit: 1300m, TotalClosingCredit: 0m);

        var bytes = TrialBalancePdfDocument.Build(report, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        AssertValidPdf(bytes);
    }

    [Fact]
    public void AccountLedger_grouped_running_balance_produces_valid_pdf()
    {
        var rows = new List<AccountLedgerRowDto>
        {
            new(DateTime.UtcNow, "JE-0001", "REF-1", "Opening entry", 500m, 0m, 500m)
        };
        var groups = new List<AccountLedgerGroupDto>
        {
            new(Guid.NewGuid(), "1000", "Cash", "Current Assets", AccountGroupType.Asset, 0m, 500m, 500m, 0m, rows)
        };

        var single = AccountLedgerPdfDocument.BuildSingle("1000", "Cash", rows, null, null);
        var all = AccountLedgerPdfDocument.BuildAll(groups, null, null);

        AssertValidPdf(single);
        AssertValidPdf(all);
    }

    [Fact]
    public void IncomeStatement_recursive_tree_produces_valid_pdf()
    {
        var report = new IncomeStatementDto(
            TotalRevenue: 10000m, CostOfSales: 4000m, GrossProfit: 6000m,
            TotalExpenses: 2000m, NetProfit: 4000m,
            Lines:
            [
                new IncomeStatementLineDto("Revenue", 10000m, true,
                [
                    new IncomeStatementLineDto("Sales", 10000m, false, null)
                ]),
                new IncomeStatementLineDto("Expenses", 2000m, true, null)
            ]);

        var bytes = IncomeStatementPdfDocument.Build(report, null, null);

        AssertValidPdf(bytes);
    }

    [Fact]
    public void ReceiptsPayments_multi_section_produces_valid_pdf()
    {
        var report = new ReceiptsPaymentsReportDto(
            Transfers: [new TransferRowDto(DateTime.UtcNow, "JE-1", "Cash", "Bank", "Deposit", 200m)],
            TotalTransfers: 200m,
            Receipts: [new ReceiptsPaymentsGroupDto(Guid.NewGuid(), "CC1", "Site A", null, 500m,
                [new ReceiptsPaymentsRowDto(Guid.NewGuid(), "4000", "Sales", 500m)])],
            TotalReceiptsExclTransfers: 500m,
            TotalReceiptsInclTransfers: 700m,
            Payments: [new ReceiptsPaymentsGroupDto(Guid.NewGuid(), "CC1", "Site A", null, 100m,
                [new ReceiptsPaymentsRowDto(Guid.NewGuid(), "5000", "Conveyance", 100m)])],
            TotalPayments: 100m,
            Balances: [new CashBankBalanceRowDto(Guid.NewGuid(), "1000", "Cash", 1000m, 500m, 100m, 1400m)],
            TotalOpening: 1000m, TotalPeriodReceipts: 500m, TotalPeriodPayments: 100m, TotalClosing: 1400m);

        var bytes = ReceiptsPaymentsPdfDocument.Build(report, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        AssertValidPdf(bytes);
    }

    [Fact]
    public void ARAging_summary_tiles_produces_valid_pdf()
    {
        var report = new AgingSummaryDto(
            CurrentAmount: 1000m, Days1To30: 500m, Days31To60: 200m, Days61To90: 100m, DaysOver90: 50m,
            TotalOutstanding: 1850m,
            Rows: [new AgingRowDto(Guid.NewGuid(), "Acme Corp", "INV-0001", DateTime.UtcNow.AddDays(-40),
                DateTime.UtcNow.AddDays(-10), 500m, 0m, 500m, 10, "1-30 Days")]);

        var bytes = ARAgingPdfDocument.Build(report, DateTime.UtcNow);

        AssertValidPdf(bytes);
    }
}
