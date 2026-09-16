using uOrgHub.Accounts.DTOs.Reports;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class ARAgingPdfDocument
{
    public static byte[] Build(AgingSummaryDto report, DateTime asOfDate) =>
        AgingPdfDocument.Build("AR Aging Report", "Customer", report, asOfDate);
}
