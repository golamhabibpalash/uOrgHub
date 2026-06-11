import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getBalanceSheet, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

export default function BalanceSheetPage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-balance-sheet", filter],
    queryFn: () => getBalanceSheet(filter),
  });

  const bs = data?.data?.data;
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
      title="Balance Sheet"
      subtitle="Financial position — assets, liabilities, and equity"
      loading={isLoading}
    >
      {bs ? (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden max-w-3xl mx-auto">
          <div className="px-4 py-3 border-b border-gray-200 bg-gray-50">
            <h3 className="text-sm font-semibold text-gray-800">Balance Sheet</h3>
          </div>
          {renderLines(bs.lines)}

          {/* Summary widgets */}
          <div className="grid grid-cols-3 gap-px bg-gray-200 mt-2">
            <div className="bg-blue-50 px-4 py-3 text-center">
              <p className="text-xs text-blue-600 font-medium">Total Assets</p>
              <p className="text-sm font-bold text-blue-700 tabular-nums">{fmt(bs.totalAssets)}</p>
            </div>
            <div className="bg-red-50 px-4 py-3 text-center">
              <p className="text-xs text-red-600 font-medium">Total Liabilities</p>
              <p className="text-sm font-bold text-red-700 tabular-nums">{fmt(bs.totalLiabilities)}</p>
            </div>
            <div className="bg-purple-50 px-4 py-3 text-center">
              <p className="text-xs text-purple-600 font-medium">Total Equity</p>
              <p className="text-sm font-bold text-purple-700 tabular-nums">{fmt(bs.totalEquity)}</p>
            </div>
          </div>
        </div>
      ) : (
        <div className="text-center py-12 text-sm text-gray-400">No data available</div>
      )}
    </ReportLayout>
  );
}
