import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getChartOfAccountsReport, reportPdfUrls, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import { useReportPdf } from "../../../hooks/useReportPdf";

const typeColors: Record<string, string> = {
  Asset: "bg-blue-50 text-blue-700", Liability: "bg-red-50 text-red-700", Equity: "bg-purple-50 text-purple-700",
  Income: "bg-green-50 text-green-700", Expense: "bg-orange-50 text-orange-700",
};

export default function ChartOfAccountsReportPage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-chart-of-accounts", filter],
    queryFn: () => getChartOfAccountsReport(filter),
  });

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const { downloadPdf, isDownloading } = useReportPdf();

  return (
    <ReportLayout
      title="Chart of Accounts Report"
      subtitle="Complete listing of all accounts"
      loading={isLoading}
      onExportPdf={() => downloadPdf({ url: reportPdfUrls.chartOfAccountsReport, params: filter, filename: "ChartOfAccountsReport.pdf" })}
      exportingPdf={isDownloading}
    >
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th data-col="code" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Code</th>
              <th data-col="accountName" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Account Name</th>
              <th data-col="group" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Group</th>
              <th data-col="type" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
              <th data-col="balance" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Balance</th>
              <th data-col="status" className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
              <th data-col="customCode" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Custom Code</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.accountId} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td data-col="code" className="px-4 py-2 text-xs font-mono text-gray-500">{row.accountCode}</td>
                <td data-col="accountName" className="px-4 py-2 text-sm">{row.accountName}</td>
                <td data-col="group" className="px-4 py-2 text-xs text-gray-500">{row.accountGroupName}</td>
                <td data-col="type" className="px-4 py-2">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${typeColors[row.accountType]}`}>
                    {row.accountType}
                  </span>
                </td>
                <td data-col="balance" className="px-4 py-2 text-right tabular-nums">{fmt(row.currentBalance)}</td>
                <td data-col="status" className="px-4 py-2 text-center">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
                    {row.isActive ? "Active" : "Inactive"}
                  </span>
                </td>
                <td data-col="customCode" className="px-4 py-2 text-xs text-gray-500 font-mono">{row.customCode ?? "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No accounts found</div>
        )}
      </div>
    </ReportLayout>
  );
}
