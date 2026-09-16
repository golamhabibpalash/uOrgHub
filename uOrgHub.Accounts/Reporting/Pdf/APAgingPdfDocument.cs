using uOrgHub.Accounts.DTOs.Reports;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class APAgingPdfDocument
{
    public static byte[] Build(AgingSummaryDto report, DateTime asOfDate) =>
        AgingPdfDocument.Build("AP Aging Report", "Vendor", report, asOfDate);
}
