import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getGeneralLedger, reportPdfUrls, ReportFilter, AccountGroupType } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import { useReportPdf } from "../../../hooks/useReportPdf";

const typeColors: Record<string, string> = {
  Asset: "text-blue-600", Liability: "text-red-600", Equity: "text-purple-600",
  Income: "text-green-600", Expense: "text-orange-600",
};

const accountTypes: AccountGroupType[] = ["Asset", "Liability", "Equity", "Income", "Expense"];

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

export default function GeneralLedgerPage() {
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [accountType, setAccountType] = useState<AccountGroupType | "">("");

  const filter: ReportFilter = {
    ...(dateFrom && { dateFrom }),
    ...(dateTo && { dateTo }),
    ...(accountType && { accountType }),
  };

  const { data, isLoading } = useQuery({
    queryKey: ["report-general-ledger", filter],
    queryFn: () => getGeneralLedger(filter),
  });
  const { downloadPdf, isDownloading } = useReportPdf();

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

  const totalOpening = rows.reduce((s, r) => s + r.openingBalance, 0);
  const totalDebit = rows.reduce((s, r) => s + r.debit, 0);
  const totalCredit = rows.reduce((s, r) => s + r.credit, 0);
  const totalClosing = rows.reduce((s, r) => s + r.closingBalance, 0);

  function resetFilters() {
    setDateFrom("");
    setDateTo("");
    setAccountType("");
  }

  const filters = (
    <div className="flex flex-wrap items-end gap-3">
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date From</label>
        <DateInput className={selectClass} value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date To</label>
        <DateInput className={selectClass} value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Account Type</label>
        <select className={selectClass} value={accountType} onChange={(e) => setAccountType(e.target.value as AccountGroupType | "")}>
          <option value="">All types</option>
          {accountTypes.map((t) => (
            <option key={t} value={t}>{t}</option>
          ))}
        </select>
      </div>
      {(dateFrom || dateTo || accountType) && (
        <button onClick={resetFilters} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500">
          Clear
        </button>
      )}
    </div>
  );

  return (
    <ReportLayout
      title="General Ledger"
      subtitle="Account-wise ledger summary with opening and closing balances"
      filters={filters}
      loading={isLoading}
      onExportPdf={() => downloadPdf({ url: reportPdfUrls.generalLedger, params: filter, filename: "GeneralLedger.pdf" })}
      exportingPdf={isDownloading}
    >
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th data-col="code" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Code</th>
              <th data-col="account" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Account</th>
              <th data-col="group" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Group</th>
              <th data-col="type" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
              <th data-col="opening" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Opening</th>
              <th data-col="debit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
              <th data-col="credit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
              <th data-col="closing" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Closing</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.accountId} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td data-col="code" className="px-4 py-2 text-xs font-mono text-gray-500">{row.accountCode}</td>
                <td data-col="account" className="px-4 py-2 text-sm">{row.accountName}</td>
                <td data-col="group" className="px-4 py-2 text-xs text-gray-500">{row.accountGroupName}</td>
                <td data-col="type" className={`px-4 py-2 text-xs font-medium ${typeColors[row.accountType]}`}>{row.accountType}</td>
                <td data-col="opening" className="px-4 py-2 text-right tabular-nums">{fmt(row.openingBalance)}</td>
                <td data-col="debit" className="px-4 py-2 text-right tabular-nums">{fmt(row.debit)}</td>
                <td data-col="credit" className="px-4 py-2 text-right tabular-nums">{fmt(row.credit)}</td>
                <td data-col="closing" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(row.closingBalance)}</td>
              </tr>
            ))}
          </tbody>
          {rows.length > 0 && (
            <tfoot className="bg-gray-50 border-t-2 border-gray-200">
              <tr>
                <td colSpan={4} data-col-span="code,account,group,type" className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                <td data-col="opening" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalOpening)}</td>
                <td data-col="debit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalDebit)}</td>
                <td data-col="credit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalCredit)}</td>
                <td data-col="closing" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalClosing)}</td>
              </tr>
            </tfoot>
          )}
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No data available</div>
        )}
      </div>
    </ReportLayout>
  );
}
