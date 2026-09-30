import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Search } from "lucide-react";
import { getAPAging, reportPdfUrls, type AgingFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import { useReportPdf } from "../../../hooks/useReportPdf";

const inputClass = "border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";

export default function APAgingPage() {
  const today = new Date().toISOString().split("T")[0];

  // Filters live in the URL, like the other report pages, so a narrowed report survives a reload
  // and can be shared as a link.
  const [params, setParams] = useSearchParams();
  const asOfDate = params.get("asOf") || today;
  const dateFrom = params.get("from") ?? "";
  const dateTo = params.get("to") ?? "";
  const search = params.get("search") ?? "";

  const setParam = (key: string, value: string) => {
    const next = new URLSearchParams(params);
    if (value) next.set(key, value);
    else next.delete(key);
    setParams(next, { replace: true });
  };

  // Typing stays instant; the request waits until the user pauses.
  const [searchInput, setSearchInput] = useState(search);
  useEffect(() => {
    if (searchInput === search) return;
    const timer = setTimeout(() => setParam("search", searchInput.trim()), 350);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput]);

  const filter: AgingFilter = {
    asOfDate,
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    search: search || undefined,
  };
  const isNarrowed = Boolean(dateFrom || dateTo || search);

  const { data, isLoading } = useQuery({
    queryKey: ["report-ap-aging", filter],
    queryFn: () => getAPAging(filter),
    enabled: !!asOfDate,
    placeholderData: keepPreviousData,
  });

  function clearFilters() {
    setSearchInput("");
    const next = new URLSearchParams(params);
    ["from", "to", "search"].forEach((k) => next.delete(k));
    setParams(next, { replace: true });
  }

  const summary = data?.data?.data;
  const rows = summary?.rows ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");
  const { downloadPdf, isDownloading } = useReportPdf();

  const bucketColors: Record<string, string> = {
    Current: "bg-green-50 text-green-700",
    "1-30 Days": "bg-yellow-50 text-yellow-700",
    "31-60 Days": "bg-orange-50 text-orange-700",
    "61-90 Days": "bg-red-50 text-red-700",
    "90+ Days": "bg-red-100 text-red-800",
  };

  return (
    <ReportLayout
      title="Accounts Payable Aging"
      subtitle={`Outstanding bills as of ${asOfDate}${
        dateFrom || dateTo ? ` · dated ${dateFrom || "…"} to ${dateTo || "…"}` : ""
      }${search ? ` · matching "${search}"` : ""}`}
      filters={
        <div className="flex flex-wrap items-end gap-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">As of Date</label>
            <DateInput className={inputClass} value={asOfDate} onChange={(e) => setParam("asOf", e.target.value)} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Bill Date From</label>
            <DateInput className={inputClass} value={dateFrom} onChange={(e) => setParam("from", e.target.value)} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Bill Date To</label>
            <DateInput className={inputClass} value={dateTo} onChange={(e) => setParam("to", e.target.value)} />
          </div>
          <div className="flex-1 min-w-[220px] max-w-sm">
            <label className="text-xs text-gray-500 mb-1 block">Search</label>
            <div className="relative">
              <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Vendor, bill # or vendor bill #"
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                className={`w-full pl-9 ${inputClass}`}
              />
            </div>
          </div>
          {isNarrowed && (
            <button
              onClick={clearFilters}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500"
            >
              Clear
            </button>
          )}
        </div>
      }
      loading={isLoading}
      onExportPdf={() => downloadPdf({ url: reportPdfUrls.apAging, params: filter, filename: "APAging.pdf" })}
      exportingPdf={isDownloading}
    >
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
              <th data-col="vendor" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Vendor</th>
              <th data-col="bill" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Bill #</th>
              <th data-col="date" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Date</th>
              <th data-col="dueDate" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Due Date</th>
              <th data-col="total" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Total</th>
              <th data-col="paid" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Paid</th>
              <th data-col="balance" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Balance</th>
              <th data-col="days" className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Days</th>
              <th data-col="bucket" className="text-center px-4 py-2.5 text-xs font-medium text-gray-500">Bucket</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, i) => (
              <tr key={i} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td data-col="vendor" className="px-4 py-2 text-sm">{row.customerOrVendor}</td>
                <td data-col="bill" className="px-4 py-2 text-xs font-mono text-gray-500">{row.documentNumber}</td>
                <td data-col="date" className="px-4 py-2 text-xs">{dateFmt(row.documentDate)}</td>
                <td data-col="dueDate" className="px-4 py-2 text-xs">{dateFmt(row.dueDate)}</td>
                <td data-col="total" className="px-4 py-2 text-right tabular-nums">{fmt(row.totalAmount)}</td>
                <td data-col="paid" className="px-4 py-2 text-right tabular-nums">{fmt(row.paidAmount)}</td>
                <td data-col="balance" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(row.balanceDue)}</td>
                <td data-col="days" className="px-4 py-2 text-center text-xs text-gray-500">{row.daysOverdue}</td>
                <td data-col="bucket" className="px-4 py-2 text-center">
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
                <td colSpan={4} data-col-span="vendor,bill,date,dueDate" className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                <td data-col="total" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(summary.rows.reduce((s, r) => s + r.totalAmount, 0))}</td>
                <td data-col="paid" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(summary.rows.reduce((s, r) => s + r.paidAmount, 0))}</td>
                <td data-col="balance" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(summary.totalOutstanding)}</td>
                <td colSpan={2} data-col-span="days,bucket"></td>
              </tr>
            </tfoot>
          )}
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">
            {isNarrowed ? "No outstanding bills match these filters" : "No outstanding bills found"}
          </div>
        )}
      </div>
    </ReportLayout>
  );
}
