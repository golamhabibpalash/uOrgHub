using FluentAssertions;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Projects.Reporting.Pdf;

namespace uOrgHub.Tests.Projects;

/// <summary>Smoke test for the Project Statement PDF builder (hybrid summary-band + table shape).</summary>
public class ProjectStatementPdfDocumentTests
{
    private static void AssertValidPdf(byte[] bytes)
    {
        bytes.Should().NotBeNullOrEmpty();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public void Single_project_statement_produces_valid_pdf()
    {
        var dto = new ProjectStatementDto(
            ProjectId: Guid.NewGuid(), ProjectCode: "PRJ-001", ProjectName: "Site A Construction",
            ContractValue: 500000m, DateFrom: DateTime.UtcNow.AddMonths(-1), DateTo: DateTime.UtcNow,
            OpeningSpend: 10000m, PeriodExpense: 5000m, PeriodIncome: 20000m, ClosingSpend: 15000m,
            Receipts: 20000m, Payments: 5000m, NetCashPosition: 15000m,
            ByAccount: [new ProjectStatementAccountDto(Guid.NewGuid(), "5000", "Materials", AccountGroupType.Expense, 5000m, 0m, 5000m)],
            Rows: [new ProjectStatementRowDto(DateTime.UtcNow, "JE-1", "REF-1", Guid.NewGuid(), "5000", "Materials",
                AccountGroupType.Expense, "Cement purchase", "Site A", 5000m, 0m, 5000m)]);

        var bytes = ProjectStatementPdfDocument.BuildSingle(dto);

        AssertValidPdf(bytes);
    }

    [Fact]
    public void Consolidated_project_statement_produces_valid_pdf()
    {
        var dto = new ConsolidatedProjectStatementDto(
            DateFrom: DateTime.UtcNow.AddMonths(-1), DateTo: DateTime.UtcNow,
            ContractValue: 500000m, OpeningSpend: 10000m, PeriodExpense: 5000m, PeriodIncome: 20000m,
            ClosingSpend: 15000m, Receipts: 20000m, Payments: 5000m, NetCashPosition: 15000m,
            Projects: [new ConsolidatedProjectRowDto(Guid.NewGuid(), "PRJ-001", "Site A Construction",
                500000m, 10000m, 5000m, 20000m, 15000m, 20000m, 5000m, 15000m)]);

        var bytes = ProjectStatementPdfDocument.BuildConsolidated(dto);

        AssertValidPdf(bytes);
    }
}
