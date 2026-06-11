import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getIncomeStatement, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

export default function IncomeStatementPage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-income-statement", filter],
    queryFn: () => getIncomeStatement(filter),
  });

  const stmt = data?.data?.data;
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

  function renderLines(lines: { label: string; amount: number; isBold: boolean; children?: any[] }[] | undefined, depth = 0) {
    if (!lines) return null;
    return lines.map((line, i) => (
      <div key={i}>
        <div
          className={`flex items-center justify-between px-4 py-1.5 ${line.isBold ? "border-t border-gray-200 bg-gray-50/50" : ""}`}
          style={{ paddingLeft: `${12 + depth * 20}px` }}
        >
          <span className={`text-sm ${line.isBold ? "font-semibold text-gray-800" : "text-gray-600"}`}>
            {line.label}
          </span>
          <span className={`text-sm tabular-nums ${line.isBold ? "font-semibold" : ""}`}>
            {fmt(line.amount)}
          </span>
        </div>
        {line.children && renderLines(line.children, depth + 1)}
      </div>
    ));
  }

  return (
    <ReportLayout
      title="Income Statement"
      subtitle="Profit & Loss statement — revenue, expenses, and net profit/loss"
      loading={isLoading}
    >
      {stmt ? (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden max-w-3xl mx-auto">
          <div className="px-4 py-3 border-b border-gray-200 bg-gray-50">
            <h3 className="text-sm font-semibold text-gray-800">Profit & Loss Statement</h3>
          </div>
          {renderLines(stmt.lines)}

          {/* Summary box */}
          <div className="grid grid-cols-5 gap-px bg-gray-200 mt-2">
            <div className="bg-blue-50 px-4 py-3 text-center">
              <p className="text-xs text-blue-600 font-medium">Revenue</p>
              <p className="text-sm font-bold text-blue-700 tabular-nums">{fmt(stmt.totalRevenue)}</p>
            </div>
            <div className="bg-orange-50 px-4 py-3 text-center">
              <p className="text-xs text-orange-600 font-medium">Expenses</p>
              <p className="text-sm font-bold text-orange-700 tabular-nums">{fmt(stmt.totalExpenses)}</p>
            </div>
            <div className="bg-green-50 px-4 py-3 text-center">
              <p className="text-xs text-green-600 font-medium">Gross Profit</p>
              <p className="text-sm font-bold text-green-700 tabular-nums">{fmt(stmt.grossProfit)}</p>
            </div>
            <div className={stmt.netProfit >= 0 ? "bg-emerald-50 px-4 py-3 text-center" : "bg-red-50 px-4 py-3 text-center"}>
              <p className={`text-xs font-medium ${stmt.netProfit >= 0 ? "text-emerald-600" : "text-red-600"}`}>Net Profit</p>
              <p className={`text-sm font-bold tabular-nums ${stmt.netProfit >= 0 ? "text-emerald-700" : "text-red-700"}`}>
                {fmt(stmt.netProfit)}
              </p>
            </div>
          </div>
        </div>
      ) : (
        <div className="text-center py-12 text-sm text-gray-400">No data available</div>
      )}
    </ReportLayout>
  );
}
