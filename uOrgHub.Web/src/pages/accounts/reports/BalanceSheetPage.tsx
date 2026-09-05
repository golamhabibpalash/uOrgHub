import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getBalanceSheet, BalanceSheetLine } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import { useFiscalYearLookup } from "../../../hooks/useEntityLookup";

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

export default function BalanceSheetPage() {
  const [fiscalYearId, setFiscalYearId] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const { fiscalYears, options: fiscalYearOptions } = useFiscalYearLookup();

  const { data, isLoading } = useQuery({
    queryKey: ["report-balance-sheet", dateFrom, dateTo],
    queryFn: () =>
      getBalanceSheet({
        ...(dateFrom && { dateFrom }),
        ...(dateTo && { dateTo }),
      }),
  });

  const bs = data?.data?.data;
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  /** Picking a fiscal year fills the date range from it, but the range stays freely editable after. */
  function selectFiscalYear(id: string) {
    setFiscalYearId(id);
    const fy = fiscalYears.find((f) => f.id === id);
    if (fy) {
      setDateFrom(fy.startDate.split("T")[0]);
      setDateTo(fy.endDate.split("T")[0]);
    }
  }

  function resetFilters() {
    setFiscalYearId("");
    setDateFrom("");
    setDateTo("");
  }

  const subtitle =
    dateFrom && dateTo
      ? `Financial position as at the period ${dateFmt(dateFrom)} to ${dateFmt(dateTo)}`
      : dateFrom
        ? `Financial position from ${dateFmt(dateFrom)} onwards`
        : dateTo
          ? `Financial position up to ${dateFmt(dateTo)}`
          : "Financial position — assets, liabilities, and equity";

  function renderLines(lines: BalanceSheetLine[] | undefined, depth = 0) {
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

  const filters = (
    <div className="flex flex-wrap items-end gap-3">
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Fiscal Year</label>
        <select className={selectClass} value={fiscalYearId} onChange={(e) => selectFiscalYear(e.target.value)}>
          <option value="">Custom range</option>
          {fiscalYearOptions.map((o) => (
            <option key={o.value} value={o.value}>{o.label}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date From</label>
        <DateInput
          className={selectClass}
          value={dateFrom}
          onChange={(e) => { setDateFrom(e.target.value); setFiscalYearId(""); }}
        />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date To</label>
        <DateInput
          className={selectClass}
          value={dateTo}
          onChange={(e) => { setDateTo(e.target.value); setFiscalYearId(""); }}
        />
      </div>
      {(dateFrom || dateTo) && (
        <button onClick={resetFilters} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500">
          Clear
        </button>
      )}
    </div>
  );

  return (
    <ReportLayout title="Balance Sheet" subtitle={subtitle} filters={filters} loading={isLoading}>
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