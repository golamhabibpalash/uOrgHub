import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { ArrowDown, ArrowUp } from "lucide-react";
import {
  getConsolidatedProjectStatement,
  getProjectStatement,
  type ConsolidatedProjectRow,
} from "../../../api/projects";
import { useProjectLookup } from "../../../hooks/useEntityLookup";
import SearchableDropdown from "../../../components/shared/SearchableDropdown";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";

const ALL_PROJECTS = "all";

const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

/**
 * A summary figure. `tone` carries the accounting meaning rather than decoration — cost reads
 * red, money earned or received reads green — so the band can be scanned without reading labels.
 */
function Tile({ label, value, tone = "default", hint }: {
  label: string;
  value: number;
  tone?: "default" | "cost" | "income";
  hint?: string;
}) {
  const toneClass = {
    default: "text-gray-900",
    cost: "text-red-600",
    income: "text-green-600",
  }[tone];

  return (
    <div className="bg-white border border-gray-200 rounded-xl px-4 py-3">
      <p className="text-xs text-gray-400">{label}</p>
      <p className={`text-lg font-semibold tabular-nums mt-0.5 ${toneClass}`}>{fmt(value)}</p>
      {hint && <p className="text-[11px] text-gray-400 mt-0.5">{hint}</p>}
    </div>
  );
}

type SortKey = keyof Pick<
  ConsolidatedProjectRow,
  "projectCode" | "contractValue" | "openingSpend" | "periodExpense" | "periodIncome" | "closingSpend" | "receipts" | "payments" | "netCashPosition"
>;

export default function ProjectStatementPage() {
  const [projectId, setProjectId] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [sort, setSort] = useState<{ key: SortKey; dir: "asc" | "desc" }>({ key: "projectCode", dir: "asc" });

  const { options: projects, isLoading: loadingProjects } = useProjectLookup();
  const isAll = projectId === ALL_PROJECTS;

  const projectOptions = useMemo(
    () => [{ value: ALL_PROJECTS, label: "All Projects" }, ...projects],
    [projects],
  );

  const single = useQuery({
    queryKey: ["report-project-statement", projectId, dateFrom, dateTo],
    queryFn: () => getProjectStatement(projectId, dateFrom || undefined, dateTo || undefined),
    enabled: !!projectId && !isAll,
  });

  const consolidated = useQuery({
    queryKey: ["report-project-statement-all", dateFrom, dateTo],
    queryFn: () => getConsolidatedProjectStatement(dateFrom || undefined, dateTo || undefined),
    enabled: isAll,
  });

  const statement = single.data?.data?.data;
  const rows = statement?.rows ?? [];
  const byAccount = statement?.byAccount ?? [];

  const cs = consolidated.data?.data?.data;
  const sortedProjects = useMemo(() => {
    const list = [...(cs?.projects ?? [])];
    list.sort((a, b) => {
      const av = a[sort.key];
      const bv = b[sort.key];
      const cmp = typeof av === "number" && typeof bv === "number"
        ? av - bv
        : String(av).localeCompare(String(bv));
      return sort.dir === "asc" ? cmp : -cmp;
    });
    return list;
  }, [cs, sort]);

  const toggleSort = (key: SortKey) =>
    setSort((s) => (s.key === key ? { key, dir: s.dir === "asc" ? "desc" : "asc" } : { key, dir: "asc" }));

  const period =
    dateFrom && dateTo ? `${dateFmt(dateFrom)} — ${dateFmt(dateTo)}`
    : dateFrom ? `From ${dateFmt(dateFrom)}`
    : dateTo ? `Up to ${dateFmt(dateTo)}`
    : "All time";

  const invalidRange = dateFrom !== "" && dateTo !== "" && dateFrom > dateTo;

  const subtitle =
    isAll
      ? cs ? `All projects (${cs.projects.length}) · ${period}` : "All projects across every cost centre"
      : statement ? `${statement.projectName} (${statement.projectCode}) · ${period}`
      : "Select a project to view its accounting statement";

  return (
    <ReportLayout
      title="Project Statement"
      subtitle={subtitle}
      loading={single.isLoading || consolidated.isLoading}
    >
      {/* Filters */}
      <div className="no-print mb-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <SearchableDropdown
            label="Project *"
            options={projectOptions}
            loading={loadingProjects}
            value={projectId}
            onChange={(v) => setProjectId(v ?? "")}
            placeholder="Select project"
            searchPlaceholder="Search by name or code…"
            noResultsMessage="No projects found"
          />
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Start Date</label>
            <DateInput
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">End Date</label>
            <DateInput
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
            />
          </div>
          <div className="flex items-end">
            <button
              onClick={() => { setProjectId(""); setDateFrom(""); setDateTo(""); }}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 w-full"
            >
              Clear
            </button>
          </div>
        </div>
        {invalidRange && (
          <p className="text-xs text-red-500 mt-2">Start date is after end date.</p>
        )}
      </div>

      {!projectId ? (
        <div className="text-center py-12 text-sm text-gray-400">
          Select a project above to view its statement
        </div>
      ) : isAll ? (
        consolidated.isError ? (
          <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
            {(consolidated.error as Error)?.message ?? "Could not load the consolidated statement."}
          </div>
        ) : cs ? (
          <div className="space-y-4">
            {/* Organisation-wide summary band */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
              <Tile label="Contract Value" value={cs.contractValue} hint="All projects" />
              <Tile label="Opening Spend" value={cs.openingSpend} hint="Before the start date" />
              <Tile label="Period Expense" value={cs.periodExpense} tone="cost" />
              <Tile label="Closing Spend" value={cs.closingSpend} tone="cost" hint="Opening + period" />
              <Tile label="Period Income" value={cs.periodIncome} tone="income" />
              <Tile label="Receipts" value={cs.receipts} tone="income" hint="Posted credit vouchers" />
              <Tile label="Payments" value={cs.payments} tone="cost" hint="Posted debit vouchers" />
              <Tile
                label="Net Cash Position"
                value={cs.netCashPosition}
                tone={cs.netCashPosition >= 0 ? "income" : "cost"}
                hint="Receipts less payments"
              />
            </div>

            {/* Per-project breakdown */}
            <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
              <div className="px-4 py-3 border-b border-gray-100">
                <p className="text-sm font-medium text-gray-900">Project-wise Breakdown</p>
                <p className="text-xs text-gray-400 mt-0.5">
                  One row per project for the selected period — click a project to open its full statement
                </p>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-gray-50 border-b border-gray-200">
                      <SortHeader label="Project" sortKey="projectCode" sort={sort} onSort={toggleSort} align="left" />
                      <SortHeader label="Contract Value" sortKey="contractValue" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Opening Spend" sortKey="openingSpend" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Period Expense" sortKey="periodExpense" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Period Income" sortKey="periodIncome" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Closing Spend" sortKey="closingSpend" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Receipts" sortKey="receipts" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Payments" sortKey="payments" sort={sort} onSort={toggleSort} />
                      <SortHeader label="Net Cash" sortKey="netCashPosition" sort={sort} onSort={toggleSort} />
                    </tr>
                  </thead>
                  <tbody>
                    {sortedProjects.map((p) => (
                      <tr
                        key={p.projectId}
                        onClick={() => setProjectId(p.projectId)}
                        className="border-b border-gray-100 hover:bg-gray-50/50 cursor-pointer"
                      >
                        <td className="px-4 py-2 whitespace-nowrap">
                          <span className="text-xs font-mono text-gray-500">{p.projectCode}</span>
                          <span className="text-sm ml-2">{p.projectName}</span>
                        </td>
                        <td className="px-4 py-2 text-right tabular-nums">{fmt(p.contractValue)}</td>
                        <td className="px-4 py-2 text-right tabular-nums">{fmt(p.openingSpend)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-red-600">{fmt(p.periodExpense)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-green-600">{fmt(p.periodIncome)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-red-600">{fmt(p.closingSpend)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-green-600">{fmt(p.receipts)}</td>
                        <td className="px-4 py-2 text-right tabular-nums text-red-600">{fmt(p.payments)}</td>
                        <td className={`px-4 py-2 text-right tabular-nums font-medium ${p.netCashPosition >= 0 ? "text-green-600" : "text-red-600"}`}>
                          {fmt(p.netCashPosition)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  {sortedProjects.length > 0 && (
                    <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                      <tr>
                        <td className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.contractValue)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.openingSpend)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.periodExpense)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.periodIncome)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.closingSpend)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.receipts)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.payments)}</td>
                        <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.netCashPosition)}</td>
                      </tr>
                    </tfoot>
                  )}
                </table>
              </div>
              {cs.projects.length === 0 && (
                <div className="text-center py-8 text-sm text-gray-400">No projects to report on</div>
              )}
            </div>
          </div>
        ) : null
      ) : single.isError ? (
        <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
          {(single.error as Error)?.message ?? "Could not load the statement."}
        </div>
      ) : statement ? (
        <div className="space-y-4">
          {/* Summary band */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <Tile label="Contract Value" value={statement.contractValue} />
            <Tile label="Opening Spend" value={statement.openingSpend} hint="Before the start date" />
            <Tile label="Period Expense" value={statement.periodExpense} tone="cost" />
            <Tile label="Closing Spend" value={statement.closingSpend} tone="cost" hint="Opening + period" />
            <Tile label="Period Income" value={statement.periodIncome} tone="income" />
            <Tile label="Receipts" value={statement.receipts} tone="income" hint="Posted credit vouchers" />
            <Tile label="Payments" value={statement.payments} tone="cost" hint="Posted debit vouchers" />
            <Tile
              label="Net Cash Position"
              value={statement.netCashPosition}
              tone={statement.netCashPosition >= 0 ? "income" : "cost"}
              hint="Receipts less payments"
            />
          </div>

          {/* Account-wise breakdown */}
          <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100">
              <p className="text-sm font-medium text-gray-900">Account-wise Summary</p>
              <p className="text-xs text-gray-400 mt-0.5">Totals per account for the selected period</p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-200">
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Account</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
                    <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
                    <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
                    <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Net</th>
                  </tr>
                </thead>
                <tbody>
                  {byAccount.map((a) => (
                    <tr key={a.accountId} className="border-b border-gray-100 hover:bg-gray-50/50">
                      <td className="px-4 py-2">
                        <span className="text-xs font-mono text-gray-500">{a.accountCode}</span>
                        <span className="text-sm ml-2">{a.accountName}</span>
                      </td>
                      <td className="px-4 py-2 text-xs text-gray-500">{a.accountType}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{a.debit > 0 ? fmt(a.debit) : "—"}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{a.credit > 0 ? fmt(a.credit) : "—"}</td>
                      <td className="px-4 py-2 text-right tabular-nums font-medium">{fmt(a.net)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {byAccount.length === 0 && (
              <div className="text-center py-8 text-sm text-gray-400">
                Nothing posted against this project for the selected period
              </div>
            )}
          </div>

          {/* Transaction ledger */}
          <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100">
              <p className="text-sm font-medium text-gray-900">Transactions</p>
              <p className="text-xs text-gray-400 mt-0.5">
                Posted journal entry lines charged to this project
              </p>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-200">
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Date</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Entry #</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Reference</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Account</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Narration</th>
                    <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
                    <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
                    <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Running Net</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r, i) => (
                    <tr key={`${r.entryNumber}-${i}`} className="border-b border-gray-100 hover:bg-gray-50/50">
                      <td className="px-4 py-2 text-xs whitespace-nowrap">{dateFmt(r.entryDate)}</td>
                      <td className="px-4 py-2 text-xs font-mono text-gray-500 whitespace-nowrap">{r.entryNumber}</td>
                      <td className="px-4 py-2 text-xs text-gray-500 whitespace-nowrap">{r.referenceNumber ?? "—"}</td>
                      <td className="px-4 py-2 text-xs">
                        <span className="font-mono text-gray-500">{r.accountCode}</span>
                        <span className="ml-2">{r.accountName}</span>
                      </td>
                      <td className="px-4 py-2 text-sm max-w-xs truncate">{r.narration ?? "—"}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{r.debit > 0 ? fmt(r.debit) : "—"}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{r.credit > 0 ? fmt(r.credit) : "—"}</td>
                      <td className="px-4 py-2 text-right tabular-nums font-medium">{fmt(r.runningNet)}</td>
                    </tr>
                  ))}
                </tbody>
                {rows.length > 0 && (
                  <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                    <tr>
                      <td colSpan={5} className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                      <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">
                        {fmt(rows.reduce((s, r) => s + r.debit, 0))}
                      </td>
                      <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">
                        {fmt(rows.reduce((s, r) => s + r.credit, 0))}
                      </td>
                      <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">
                        {fmt(rows.length > 0 ? rows[rows.length - 1].runningNet : 0)}
                      </td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>
            {rows.length === 0 && !single.isLoading && (
              <div className="text-center py-12 text-sm text-gray-400">
                No posted transactions for this project in the selected period
              </div>
            )}
          </div>
        </div>
      ) : null}
    </ReportLayout>
  );
}

function SortHeader({ label, sortKey, sort, onSort, align = "right" }: {
  label: string;
  sortKey: SortKey;
  sort: { key: SortKey; dir: "asc" | "desc" };
  onSort: (key: SortKey) => void;
  align?: "left" | "right";
}) {
  const active = sort.key === sortKey;
  return (
    <th
      onClick={() => onSort(sortKey)}
      className={`px-4 py-2.5 text-xs font-medium text-gray-500 cursor-pointer select-none hover:text-gray-700 ${align === "left" ? "text-left" : "text-right"}`}
    >
      <span className={`inline-flex items-center gap-1 ${align === "right" ? "flex-row-reverse" : ""}`}>
        {label}
        {active && (sort.dir === "asc" ? <ArrowUp size={12} /> : <ArrowDown size={12} />)}
      </span>
    </th>
  );
}
