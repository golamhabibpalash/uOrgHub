import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getTrialBalance, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

const typeColors: Record<string, string> = {
  Asset: "text-blue-600", Liability: "text-red-600", Equity: "text-purple-600",
  Income: "text-green-600", Expense: "text-orange-600",
};

export default function TrialBalancePage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-trial-balance", filter],
    queryFn: () => getTrialBalance(filter),
  });

  const tb = data?.data?.data;
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

  return (
    <ReportLayout
      title="Trial Balance"
      subtitle="Account-wise debit and credit balances"
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
              <th data-col="openingDebit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Opening Dr</th>
              <th data-col="openingCredit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Opening Cr</th>
              <th data-col="debit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
              <th data-col="credit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
              <th data-col="closingDebit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Closing Dr</th>
              <th data-col="closingCredit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Closing Cr</th>
            </tr>
          </thead>
          <tbody>
            {tb?.rows.map((row) => (
              <tr key={row.accountId} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td data-col="code" className="px-4 py-2 text-xs font-mono text-gray-500">{row.accountCode}</td>
                <td data-col="account" className="px-4 py-2 text-sm">{row.accountName}</td>
                <td data-col="group" className="px-4 py-2 text-xs text-gray-500">{row.accountGroupName}</td>
                <td data-col="type" className={`px-4 py-2 text-xs font-medium ${typeColors[row.accountType]}`}>{row.accountType}</td>
                <td data-col="openingDebit" className="px-4 py-2 text-right tabular-nums">{fmt(row.openingDebit)}</td>
                <td data-col="openingCredit" className="px-4 py-2 text-right tabular-nums">{fmt(row.openingCredit)}</td>
                <td data-col="debit" className="px-4 py-2 text-right tabular-nums">{fmt(row.debit)}</td>
                <td data-col="credit" className="px-4 py-2 text-right tabular-nums">{fmt(row.credit)}</td>
                <td data-col="closingDebit" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(row.closingDebit)}</td>
                <td data-col="closingCredit" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(row.closingCredit)}</td>
              </tr>
            ))}
          </tbody>
          {tb && (
            <tfoot className="bg-gray-50 border-t-2 border-gray-200">
              <tr>
                <td colSpan={4} data-col-span="code,account,group,type" className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                <td data-col="openingDebit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(tb.totalOpeningDebit)}</td>
                <td data-col="openingCredit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(tb.totalOpeningCredit)}</td>
                <td data-col="debit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(tb.totalDebit)}</td>
                <td data-col="credit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(tb.totalCredit)}</td>
                <td data-col="closingDebit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(tb.totalClosingDebit)}</td>
                <td data-col="closingCredit" className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(tb.totalClosingCredit)}</td>
              </tr>
            </tfoot>
          )}
        </table>
        {!tb && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No data available</div>
        )}
      </div>
    </ReportLayout>
  );
}
