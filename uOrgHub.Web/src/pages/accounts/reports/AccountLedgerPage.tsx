import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getReportAccountLedger, getReportAllAccountsLedger, getChartOfAccounts } from "../../../api/accounts";
import SearchableDropdown from "../../../components/shared/SearchableDropdown";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";

const ALL_ACCOUNTS = "ALL";

export default function AccountLedgerPage() {
  const [accountId, setAccountId] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const isAll = accountId === ALL_ACCOUNTS;

  const { data: accountsData } = useQuery({
    queryKey: ["chart-of-accounts-for-ledger"],
    queryFn: () => getChartOfAccounts({ page: 1, pageSize: 500 }),
  });

  const accountOptions = [
    { value: ALL_ACCOUNTS, label: "All Accounts", searchText: "all accounts" },
    ...(accountsData?.data?.data?.items ?? []).map((a) => ({
      value: a.id,
      label: `${a.accountCode} — ${a.accountName}`,
      searchText: `${a.accountName} ${a.accountCode}`,
    })),
  ];

  const { data: singleData, isLoading: singleLoading } = useQuery({
    queryKey: ["report-account-ledger", accountId, dateFrom, dateTo],
    queryFn: () => getReportAccountLedger(accountId, dateFrom || undefined, dateTo || undefined),
    enabled: !!accountId && !isAll,
  });

  const { data: allData, isLoading: allLoading } = useQuery({
    queryKey: ["report-account-ledger-all", dateFrom, dateTo],
    queryFn: () => getReportAllAccountsLedger(dateFrom || undefined, dateTo || undefined),
    enabled: isAll,
  });

  const isLoading = isAll ? allLoading : singleLoading;
  const rows = singleData?.data?.data ?? [];
  const groups = allData?.data?.data ?? [];

  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  const selectedAccount = accountOptions.find((o) => o.value === accountId);
  const subtitle = !accountId
    ? "Select an account to view ledger"
    : isAll
      ? "Transaction history for all accounts"
      : `Transaction history for ${selectedAccount?.label ?? ""}`;

  return (
    <ReportLayout title="Account Ledger" subtitle={subtitle} loading={isLoading}>
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
            <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Date To</label>
            <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
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

      {!accountId ? (
        <div className="text-center py-12 text-sm text-gray-400">Select an account above to view ledger transactions</div>
      ) : isAll ? (
        groups.length === 0 && !isLoading ? (
          <div className="text-center py-12 text-sm text-gray-400">No transactions found for the selected period</div>
        ) : (
          <div className="space-y-6">
            {groups.map((g) => (
              <div key={g.accountId} className="bg-white border border-gray-200 rounded-xl overflow-hidden">
                <div className="flex items-center justify-between bg-gray-50 border-b border-gray-200 px-4 py-2.5">
                  <div className="text-sm font-medium text-gray-900">{g.accountCode} — {g.accountName}</div>
                  <div className="text-xs text-gray-500">{g.accountGroupName}</div>
                </div>
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left px-4 py-2 text-xs font-medium text-gray-500">Date</th>
                      <th className="text-left px-4 py-2 text-xs font-medium text-gray-500">Entry #</th>
                      <th className="text-left px-4 py-2 text-xs font-medium text-gray-500">Reference</th>
                      <th className="text-left px-4 py-2 text-xs font-medium text-gray-500">Narration</th>
                      <th className="text-right px-4 py-2 text-xs font-medium text-gray-500">Debit</th>
                      <th className="text-right px-4 py-2 text-xs font-medium text-gray-500">Credit</th>
                      <th className="text-right px-4 py-2 text-xs font-medium text-gray-500">Balance</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr className="border-b border-gray-100 bg-gray-50/40">
                      <td className="px-4 py-2 text-xs text-gray-500" colSpan={6}>Opening Balance</td>
                      <td className="px-4 py-2 text-right tabular-nums font-medium">{fmt(g.openingBalance)}</td>
                    </tr>
                    {g.rows.map((row, i) => (
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
                    <tr className="border-t border-gray-200 bg-gray-50/60 font-medium">
                      <td className="px-4 py-2 text-xs" colSpan={4}>Closing Balance</td>
                      <td className="px-4 py-2 text-right tabular-nums">{fmt(g.totalDebit)}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{fmt(g.totalCredit)}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{fmt(g.closingBalance)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            ))}
          </div>
        )
      ) : (
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
      )}
    </ReportLayout>
  );
}
