import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getJournalEntryReport, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600", Posted: "bg-green-100 text-green-700", Cancelled: "bg-red-100 text-red-600",
};

export default function JournalEntryReportPage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-journal-entries", filter],
    queryFn: () => getJournalEntryReport(filter),
  });

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  return (
    <ReportLayout
      title="Journal Entry Report"
      subtitle="All journal entries with filters"
      loading={isLoading}
    >
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Entry #</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Date</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Reference</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Description</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
              <th className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Created By</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={`${row.entryNumber}`} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td className="px-4 py-2 text-xs font-mono text-gray-500">{row.entryNumber}</td>
                <td className="px-4 py-2 text-xs">{dateFmt(row.entryDate)}</td>
                <td className="px-4 py-2 text-xs text-gray-500">{row.referenceNumber ?? "—"}</td>
                <td className="px-4 py-2 text-sm max-w-xs truncate">{row.description}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.totalDebit)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.totalCredit)}</td>
                <td className="px-4 py-2 text-center">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status] ?? "bg-gray-100 text-gray-600"}`}>
                    {row.status}
                  </span>
                </td>
                <td className="px-4 py-2 text-xs text-gray-500">{row.createdBy}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No journal entries found</div>
        )}
      </div>
    </ReportLayout>
  );
}
