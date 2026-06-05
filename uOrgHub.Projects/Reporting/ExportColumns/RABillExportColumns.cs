using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class RABillExportColumns
{
    public static List<ExportColumn<RABillResponseDto>> Get() =>
    [
        new("billNumber", "Bill Number", x => x.BillNumber),
        new("title", "Title", x => x.Title),
        new("billDate", "Bill Date", x => x.BillDate),
        new("periodFrom", "Period From", x => x.PeriodFrom),
        new("periodTo", "Period To", x => x.PeriodTo),
        new("billSequence", "Bill Sequence", x => x.BillSequence),
        new("grossAmount", "Gross Amount", x => x.GrossAmount),
        new("deductionAmount", "Deductions", x => x.DeductionAmount),
        new("retentionPercent", "Retention %", x => x.RetentionPercent),
        new("retentionAmount", "Retention Amount", x => x.RetentionAmount),
        new("netAmount", "Net Amount", x => x.NetAmount),
        new("previousBilledAmount", "Previous Billed", x => x.PreviousBilledAmount),
        new("cumulativeBilledAmount", "Cumulative Billed", x => x.CumulativeBilledAmount),
        new("status", "Status", x => x.Status.ToString()),
        new("certifiedDate", "Certified Date", x => x.CertifiedDate),
        new("paidDate", "Paid Date", x => x.PaidDate),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
