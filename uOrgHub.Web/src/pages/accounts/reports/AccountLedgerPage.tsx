import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getReportAccountLedger, getChartOfAccounts } from "../../../api/accounts";
import SearchableDropdown from "../../../components/shared/SearchableDropdown";
import ReportLayout from "../../../components/shared/ReportLayout";

export default function AccountLedgerPage() {
  const [accountId, setAccountId] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const { data: accountsData } = useQuery({
    queryKey: ["chart-of-accounts-for-ledger"],
    queryFn: () => getChartOfAccounts({ page: 1, pageSize: 500 }),
  });

  const accountOptions = (accountsData?.data?.data?.items ?? []).map((a) => ({
    value: a.id,
    label: `${a.accountCode} — ${a.accountName}`,
    searchText: `${a.accountName} ${a.accountCode}`,
  }));

  const { data, isLoading } = useQuery({
    queryKey: ["report-account-ledger", accountId, dateFrom, dateTo],
    queryFn: () => getReportAccountLedger(accountId, dateFrom || undefined, dateTo || undefined),
    enabled: !!accountId,
  });

  const rows = data?.data?.data ?? [];
  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  const selectedAccount = accountOptions.find((o) => o.value === accountId);

  return (
    <ReportLayout
      title="Account Ledger"
      subtitle={selectedAccount ? `Transaction history for ${selectedAccount.label}` : "Select an account to view ledger"}
      loading={isLoading}
    >
      {/* Filters */}
      <div className="no-print mb-4">
        <div className="grid grid-cols-4 gap-3">
          <SearchableDropdown
            label="Account *"
            options={accountOptions}
            value={accountId}
            onChange={(v) => setAccountId(v ?? "")}
            placeholder="Select account"
            searchPlaceholder="Search accounts..."
          />
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Date From</label>
            <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Date To</label>
            <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
          </div>
          <div className="flex items-end">
            <button
              onClick={() => { setAccountId(""); setDateFrom(""); setDateTo(""); }}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 w-full"
            >
              Clear
            </button>
          </div>
        </div>
      </div>

      {accountId ? (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Date</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Entry #</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Reference</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Narration</th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Balance</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row, i) => (
                <tr key={i} className="border-b border-gray-100 hover:bg-gray-50/50">
                  <td className="px-4 py-2 text-xs">{dateFmt(row.entryDate)}</td>
                  <td className="px-4 py-2 text-xs font-mono text-gray-500">{row.entryNumber}</td>
                  <td className="px-4 py-2 text-xs text-gray-500">{row.referenceNumber ?? "—"}</td>
                  <td className="px-4 py-2 text-sm max-w-xs truncate">{row.narration}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{row.debit > 0 ? fmt(row.debit) : "—"}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{row.credit > 0 ? fmt(row.credit) : "—"}</td>
                  <td className="px-4 py-2 text-right tabular-nums font-medium">{fmt(row.runningBalance)}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {rows.length === 0 && !isLoading && (
            <div className="text-center py-12 text-sm text-gray-400">No transactions found for this account</div>
          )}
        </div>
      ) : (
        <div className="text-center py-12 text-sm text-gray-400">Select an account above to view ledger transactions</div>
      )}
    </ReportLayout>
  );
}
