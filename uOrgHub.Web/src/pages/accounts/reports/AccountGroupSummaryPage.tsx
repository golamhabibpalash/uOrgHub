import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getAccountGroupSummary, ReportFilter } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";

const typeColors: Record<string, string> = {
  Asset: "text-blue-600", Liability: "text-red-600", Equity: "text-purple-600",
  Income: "text-green-600", Expense: "text-orange-600",
};

export default function AccountGroupSummaryPage() {
  const [filter] = useState<ReportFilter>({});

  const { data, isLoading } = useQuery({
    queryKey: ["report-account-group-summary", filter],
    queryFn: () => getAccountGroupSummary(filter),
  });

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

  const totalDebit = rows.reduce((s, r) => s + r.totalDebit, 0);
  const totalCredit = rows.reduce((s, r) => s + r.totalCredit, 0);

  return (
    <ReportLayout
      title="Account Group Summary"
      subtitle="Group-wise account balance summaries"
      loading={isLoading}
    >
      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Code</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Group Name</th>
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Total Debit</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Total Credit</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Net Balance</th>
              <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Accounts</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.groupId} className="border-b border-gray-100 hover:bg-gray-50/50">
                <td className="px-4 py-2 text-xs font-mono text-gray-500">{row.groupCode}</td>
                <td className="px-4 py-2 text-sm">{row.groupName}</td>
                <td className={`px-4 py-2 text-xs font-medium ${typeColors[row.groupType]}`}>{row.groupType}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.totalDebit)}</td>
                <td className="px-4 py-2 text-right tabular-nums">{fmt(row.totalCredit)}</td>
                <td className={`px-4 py-2 text-right tabular-nums font-medium ${row.balance >= 0 ? "text-gray-800" : "text-red-600"}`}>
                  {fmt(row.balance)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums text-gray-500">{row.accountCount}</td>
              </tr>
            ))}
          </tbody>
          {rows.length > 0 && (
            <tfoot className="bg-gray-50 border-t-2 border-gray-200">
              <tr>
                <td colSpan={3} className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalDebit)}</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalCredit)}</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{fmt(totalDebit - totalCredit)}</td>
                <td className="px-4 py-2.5 text-right text-xs font-semibold tabular-nums">{rows.reduce((s, r) => s + r.accountCount, 0)}</td>
              </tr>
            </tfoot>
          )}
        </table>
        {rows.length === 0 && !isLoading && (
          <div className="text-center py-12 text-sm text-gray-400">No account groups found</div>
        )}
      </div>
    </ReportLayout>
  );
}
