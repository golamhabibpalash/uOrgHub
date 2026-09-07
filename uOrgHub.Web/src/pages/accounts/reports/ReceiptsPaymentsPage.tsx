import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  getReceiptsPayments,
  ReceiptsPaymentsGroup,
} from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import {
  useFiscalYearLookup,
  useCostCenterLookup,
  useProjectLookup,
} from "../../../hooks/useEntityLookup";

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

const fmt = (v: number) =>
  v.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

/** One cost-center group with its counterpart-account rows and a subtotal line. */
function GroupBlock({ group }: { group: ReceiptsPaymentsGroup }) {
  return (
    <div className="border border-gray-100 rounded-lg overflow-hidden">
      <div className="flex items-center justify-between bg-gray-50 px-3 py-1.5">
        <span className="text-xs font-medium text-gray-600">
          {group.costCenterCode ? `${group.costCenterCode} · ` : ""}
          {group.costCenterName}
        </span>
        <span className="text-xs font-semibold tabular-nums text-gray-700">{fmt(group.subtotal)}</span>
      </div>
      <div className="divide-y divide-gray-50">
        {group.rows.map((r) => (
          <div key={r.accountId} className="flex items-center justify-between px-3 py-1.5">
            <span className="text-sm text-gray-600">
              <span className="text-gray-400 font-mono text-xs mr-1.5">{r.accountCode}</span>
              {r.accountName}
            </span>
            <span className="text-sm tabular-nums text-gray-700">{fmt(r.amount)}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

export default function ReceiptsPaymentsPage() {
  const today = new Date().toISOString().split("T")[0];
  const [fiscalYearId, setFiscalYearId] = useState("");
  const [dateFrom, setDateFrom] = useState(today);
  const [dateTo, setDateTo] = useState(today);
  const [costCenterId, setCostCenterId] = useState("");
  const [projectId, setProjectId] = useState("");

  const { fiscalYears, options: fiscalYearOptions } = useFiscalYearLookup();
  const { options: costCenterOptions } = useCostCenterLookup();
  const { options: projectOptions } = useProjectLookup();

  const { data, isLoading } = useQuery({
    queryKey: ["report-receipts-payments", dateFrom, dateTo, costCenterId, projectId],
    queryFn: () =>
      getReceiptsPayments({
        ...(dateFrom && { dateFrom }),
        ...(dateTo && { dateTo }),
        ...(costCenterId && { costCenterId }),
        ...(projectId && { projectId }),
      }),
    placeholderData: (prev) => prev,
  });

  const report = data?.data?.data;
  const scoped = !!costCenterId || !!projectId;

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
    setDateFrom(today);
    setDateTo(today);
    setCostCenterId("");
    setProjectId("");
  }

  const subtitle =
    dateFrom && dateTo
      ? `Receipts & payments for ${dateFmt(dateFrom)} to ${dateFmt(dateTo)}`
      : "Cash & bank receipts, payments and closing position";

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
        <DateInput className={selectClass} value={dateFrom} onChange={(e) => { setDateFrom(e.target.value); setFiscalYearId(""); }} />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date To</label>
        <DateInput className={selectClass} value={dateTo} onChange={(e) => { setDateTo(e.target.value); setFiscalYearId(""); }} />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Cost Center</label>
        <select className={selectClass} value={costCenterId} onChange={(e) => setCostCenterId(e.target.value)}>
          <option value="">All cost centers</option>
          {costCenterOptions.map((o) => (
            <option key={o.value} value={o.value}>{o.label}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Project</label>
        <select className={selectClass} value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">All projects</option>
          {projectOptions.map((o) => (
            <option key={o.value} value={o.value}>{o.label}</option>
          ))}
        </select>
      </div>
      {(fiscalYearId || costCenterId || projectId || dateFrom !== today || dateTo !== today) && (
        <button onClick={resetFilters} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500">
          Clear
        </button>
      )}
    </div>
  );

  return (
    <ReportLayout title="Receipts & Payments Statement" subtitle={subtitle} filters={filters} loading={isLoading}>
      {!report ? (
        <p className="text-sm text-gray-400 py-8 text-center">No data for the selected period.</p>
      ) : (
        <div className="space-y-5">
          {report.balances.length === 0 && (
            <div className="border border-amber-200 bg-amber-50 rounded-lg px-4 py-3 text-sm text-amber-800">
              No cash or bank accounts are set up yet, so there is nothing to report. Open the{" "}
              <span className="font-medium">Chart of Accounts</span> and tick{" "}
              <span className="font-medium">“Cash / Bank account”</span> on your cash-in-hand and
              bank accounts (bank accounts registered under Bank Accounts are picked up
              automatically).
            </div>
          )}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
            {/* ── Left portion: Receipts ─────────────────────────────── */}
            <section className="border border-gray-200 rounded-xl overflow-hidden">
              <div className="bg-green-50 px-4 py-2 border-b border-green-100">
                <h3 className="text-sm font-semibold text-green-800">Receipts (money in)</h3>
              </div>
              <div className="p-4 space-y-4">
                {/* Transfers between own accounts */}
                <div>
                  <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
                    Transfers between own accounts
                  </p>
                  {report.transfers.length === 0 ? (
                    <p className="text-xs text-gray-400">No transfers in this period.</p>
                  ) : (
                    <div className="border border-gray-100 rounded-lg divide-y divide-gray-50">
                      {report.transfers.map((t, i) => (
                        <div key={`${t.entryNumber}-${i}`} className="flex items-center justify-between px-3 py-1.5">
                          <span className="text-sm text-gray-600">
                            <span className="text-gray-400 font-mono text-xs mr-1.5">{dateFmt(t.entryDate)}</span>
                            {t.fromAccount || "—"} <span className="text-gray-400">→</span> {t.toAccount || "—"}
                          </span>
                          <span className="text-sm tabular-nums text-gray-700">{fmt(t.amount)}</span>
                        </div>
                      ))}
                      <div className="flex items-center justify-between px-3 py-1.5 bg-gray-50">
                        <span className="text-xs font-semibold text-gray-600">Total transfers</span>
                        <span className="text-xs font-semibold tabular-nums text-gray-800">{fmt(report.totalTransfers)}</span>
                      </div>
                    </div>
                  )}
                </div>

                {/* Incoming transactions grouped by cost center */}
                <div>
                  <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
                    Incoming transactions
                  </p>
                  {report.receipts.length === 0 ? (
                    <p className="text-xs text-gray-400">No receipts in this period.</p>
                  ) : (
                    <div className="space-y-2">
                      {report.receipts.map((g) => (
                        <GroupBlock key={g.costCenterId ?? "unallocated"} group={g} />
                      ))}
                    </div>
                  )}
                </div>

                <div className="border-t border-gray-200 pt-3 space-y-1.5">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">Total receipts (excl. transfers)</span>
                    <span className="text-sm font-semibold tabular-nums text-gray-900">
                      {fmt(report.totalReceiptsExclTransfers)}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-semibold text-gray-800">Total receipts (incl. transfers)</span>
                    <span className="text-sm font-bold tabular-nums text-green-700">
                      {fmt(report.totalReceiptsInclTransfers)}
                    </span>
                  </div>
                </div>
              </div>
            </section>

            {/* ── Right portion: Payments ────────────────────────────── */}
            <section className="border border-gray-200 rounded-xl overflow-hidden">
              <div className="bg-red-50 px-4 py-2 border-b border-red-100">
                <h3 className="text-sm font-semibold text-red-800">Payments (money out)</h3>
              </div>
              <div className="p-4 space-y-4">
                <div>
                  <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
                    Expenses, payments & costs
                  </p>
                  {report.payments.length === 0 ? (
                    <p className="text-xs text-gray-400">No payments in this period.</p>
                  ) : (
                    <div className="space-y-2">
                      {report.payments.map((g) => (
                        <GroupBlock key={g.costCenterId ?? "unallocated"} group={g} />
                      ))}
                    </div>
                  )}
                </div>

                <div className="border-t border-gray-200 pt-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-semibold text-gray-800">Total payments</span>
                    <span className="text-sm font-bold tabular-nums text-red-700">{fmt(report.totalPayments)}</span>
                  </div>
                </div>
              </div>
            </section>
          </div>

          {/* ── Bottom portion: Cash & bank balances ─────────────────── */}
          <section className="border border-gray-200 rounded-xl overflow-hidden">
            <div className="bg-gray-50 px-4 py-2 border-b border-gray-200">
              <h3 className="text-sm font-semibold text-gray-700">Cash & bank balances</h3>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-xs text-gray-500 border-b border-gray-100">
                    <th className="text-left font-medium px-4 py-2">Account</th>
                    <th className="text-right font-medium px-4 py-2">Opening</th>
                    <th className="text-right font-medium px-4 py-2">Receipts</th>
                    <th className="text-right font-medium px-4 py-2">Payments</th>
                    <th className="text-right font-medium px-4 py-2">Closing</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {report.balances.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="px-4 py-4 text-center text-xs text-gray-400">
                        No accounts are flagged as “Cash / Bank” in the Chart of Accounts.
                      </td>
                    </tr>
                  ) : (
                    report.balances.map((b) => (
                      <tr key={b.accountId}>
                        <td className="px-4 py-2 text-gray-600">
                          <span className="text-gray-400 font-mono text-xs mr-1.5">{b.accountCode}</span>
                          {b.accountName}
                        </td>
                        <td className="px-4 py-2 text-right tabular-nums text-gray-600">{fmt(b.opening)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-green-700">{fmt(b.receipts)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-red-700">{fmt(b.payments)}</td>
                        <td className="px-4 py-2 text-right tabular-nums font-semibold text-gray-900">{fmt(b.closing)}</td>
                      </tr>
                    ))
                  )}
                </tbody>
                {report.balances.length > 0 && (
                  <tfoot>
                    <tr className="border-t border-gray-200 bg-gray-50 font-semibold">
                      <td className="px-4 py-2 text-gray-700">Total</td>
                      <td className="px-4 py-2 text-right tabular-nums text-gray-700">{fmt(report.totalOpening)}</td>
                      <td className="px-4 py-2 text-right tabular-nums text-green-800">{fmt(report.totalPeriodReceipts)}</td>
                      <td className="px-4 py-2 text-right tabular-nums text-red-800">{fmt(report.totalPeriodPayments)}</td>
                      <td className="px-4 py-2 text-right tabular-nums text-gray-900">{fmt(report.totalClosing)}</td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>
          </section>

          {scoped && (
            <p className="text-xs text-gray-400">
              Receipts and payments above are limited to the selected cost center / project. The cash
              &amp; bank balances always show the whole organisation, so they will not tie back to the
              filtered totals.
            </p>
          )}
          <p className="text-xs text-gray-400">
            Includes posted and draft journal entries. Cancelled entries are excluded.
          </p>
        </div>
      )}
    </ReportLayout>
  );
}
