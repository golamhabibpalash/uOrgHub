import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getDayBook } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

export default function DayBookPage() {
  const today = new Date().toISOString().split("T")[0];
  const [date, setDate] = useState(today);

  const { data, isLoading } = useQuery({
    queryKey: ["report-day-book", date],
    queryFn: () => getDayBook(date),
    enabled: !!date,
  });

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const totalDebit = rows.reduce((s, r) => s + r.debitTotal, 0);
  const totalCredit = rows.reduce((s, r) => s + r.creditTotal, 0);

  return (
    <ReportLayout
      title="Day Book"
      subtitle={`Daily transaction register for ${date}`}
      loading={isLoading}
    >
      <div className="no-print mb-4">
        <div className="w-64">
          <label className="text-xs text-gray-500 mb-1 block">Date</label>
          <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={date} onChange={(e) => setDate(e.target.value)} />
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Entry #</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Ref</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Description</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
              <th className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Entered By</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, i) => (
              <tr key={i} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td className="px-4 py-2 text-xs font-mono text-gray-500">{row.entryNumber}</td>
                <td className="px-4 py-2 text-xs text-gray-500">{row.referenceNumber ?? "—"}</td>
                <td className="px-4 py-2 text-sm max-w-xs truncate">{row.description}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.debitTotal)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.creditTotal)}</td>
                <td className="px-4 py-2 text-center">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${
                    row.status === "Posted" ? "bg-green-50 text-green-700" :
                    row.status === "Draft" ? "bg-gray-100 text-gray-600" : "bg-red-100 text-red-600"
                  }`}>
                    {row.status}
                  </span>
                </td>
                <td className="px-4 py-2 text-xs text-gray-500">{row.createdBy}</td>
              </tr>
            ))}
          </tbody>
          {rows.length > 0 && (
            <tfoot className="bg-gray-50 border-t-2 border-gray-200">
              <tr>
                <td colSpan={3} className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalDebit)}</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalCredit)}</td>
                <td colSpan={2}></td>
              </tr>
            </tfoot>
          )}
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No transactions recorded for this date</div>
        )}
      </div>
    </ReportLayout>
  );
}
