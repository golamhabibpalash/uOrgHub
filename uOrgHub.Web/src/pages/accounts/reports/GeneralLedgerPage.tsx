import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getGeneralLedger, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

const typeColors: Record<string, string> = {
  Asset: "text-blue-600", Liability: "text-red-600", Equity: "text-purple-600",
  Income: "text-green-600", Expense: "text-orange-600",
};

export default function GeneralLedgerPage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-general-ledger", filter],
    queryFn: () => getGeneralLedger(filter),
  });

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

  const totalOpening = rows.reduce((s, r) => s + r.openingBalance, 0);
  const totalDebit = rows.reduce((s, r) => s + r.debit, 0);
  const totalCredit = rows.reduce((s, r) => s + r.credit, 0);
  const totalClosing = rows.reduce((s, r) => s + r.closingBalance, 0);

  return (
    <ReportLayout
      title="General Ledger"
      subtitle="Account-wise ledger summary with opening and closing balances"
      loading={isLoading}
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
                <td colSpan={4} className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
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
