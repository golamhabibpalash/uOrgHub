import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getARAging } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

export default function ARAgingPage() {
  const today = new Date().toISOString().split("T")[0];
  const [asOfDate, setAsOfDate] = useState(today);

  const { data, isLoading } = useQuery({
    queryKey: ["report-ar-aging", asOfDate],
    queryFn: () => getARAging(asOfDate),
    enabled: !!asOfDate,
  });

  const summary = data?.data?.data;
  const rows = summary?.rows ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  const bucketColors: Record<string, string> = {
    Current: "bg-green-50 text-green-700",
    "1-30 Days": "bg-yellow-50 text-yellow-700",
    "31-60 Days": "bg-orange-50 text-orange-700",
    "61-90 Days": "bg-red-50 text-red-700",
    "90+ Days": "bg-red-100 text-red-800",
  };

  return (
    <ReportLayout
      title="Accounts Receivable Aging"
      subtitle={`Outstanding invoices as of ${asOfDate}`}
      loading={isLoading}
    >
      <div className="no-print mb-4">
        <div className="w-64">
          <label className="text-xs text-gray-500 mb-1 block">As of Date</label>
          <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={asOfDate} onChange={(e) => setAsOfDate(e.target.value)} />
        </div>
      </div>

      {summary && (
        <div className="grid grid-cols-5 gap-3 mb-4">
          {[
            { label: "Current", value: summary.currentAmount, color: "text-green-600" },
            { label: "1-30 Days", value: summary.days1To30, color: "text-yellow-600" },
            { label: "31-60 Days", value: summary.days31To60, color: "text-orange-600" },
            { label: "61-90 Days", value: summary.days61To90, color: "text-red-600" },
            { label: "90+ Days", value: summary.daysOver90, color: "text-red-800" },
          ].map((b) => (
            <div key={b.label} className="bg-white border border-gray-200 rounded-lg px-4 py-3 text-center">
              <p className="text-xs text-gray-500 mb-1">{b.label}</p>
              <p className={`text-sm font-semibold ${b.color} tabular-nums`}>{fmt(b.value)}</p>
            </div>
          ))}
        </div>
      )}

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Customer</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Invoice #</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Date</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Due Date</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Total</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Paid</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Balance</th>
              <th className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Days</th>
              <th className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Bucket</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, i) => (
              <tr key={i} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td className="px-4 py-2 text-sm">{row.customerOrVendor}</td>
                <td className="px-4 py-2 text-xs font-mono text-gray-500">{row.documentNumber}</td>
                <td className="px-4 py-2 text-xs">{dateFmt(row.documentDate)}</td>
                <td className="px-4 py-2 text-xs">{dateFmt(row.dueDate)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.totalAmount)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.paidAmount)}</td>
                <td className="px-4 py-2 text-right tabular-nums font-medium">{fmt(row.balanceDue)}</td>
                <td className="px-4 py-2 text-center text-xs text-gray-500">{row.daysOverdue}</td>
                <td className="px-4 py-2 text-center">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${bucketColors[row.agingBucket] ?? "bg-gray-100 text-gray-600"}`}>
                    {row.agingBucket}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
          {summary && (
            <tfoot className="bg-gray-50 border-t-2 border-gray-200">
              <tr>
                <td colSpan={4} className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(summary.rows.reduce((s, r) => s + r.totalAmount, 0))}</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(summary.rows.reduce((s, r) => s + r.paidAmount, 0))}</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(summary.totalOutstanding)}</td>
                <td colSpan={2}></td>
              </tr>
            </tfoot>
          )}
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No outstanding invoices found</div>
        )}
      </div>
    </ReportLayout>
  );
}
