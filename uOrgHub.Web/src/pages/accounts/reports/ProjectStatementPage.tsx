import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { ArrowDown, ArrowUp, ChevronLeft, ChevronRight, Search, X } from "lucide-react";
import {
  getConsolidatedProjectStatement,
  getProjectStatement,
  reportPdfUrls,
  type ConsolidatedProjectRow,
  type ProjectStatementRow,
  type ProjectStatementTransactionFilter,
} from "../../../api/projects";
import { useProjectLookup } from "../../../hooks/useEntityLookup";
import SearchableDropdown from "../../../components/shared/SearchableDropdown";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import { useReportPdf } from "../../../hooks/useReportPdf";

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

type TxSortKey =
  | "entryDate"
  | "entryNumber"
  | "referenceNumber"
  | "accountCode"
  | "narration"
  | "debit"
  | "credit"
  | "runningNet";

const TX_PAGE_SIZES = [10, 25, 50, 100];

/** The rows each Transaction Type keeps — the same rule the server applies to the PDF. */
const TX_TYPE_FILTER: Record<ProjectStatementTransactionFilter, (r: ProjectStatementRow) => boolean> = {
  All: () => true,
  Receipts: (r) => r.cashFlow === "Receipt",
  Payments: (r) => r.cashFlow === "Payment",
};

/**
 * Distinct parties named in the ledger, for a party filter. Built from the loaded rows, so the
 * choices are only ever the selected project's own parties in the selected period. Names are free
 * text on the voucher, so "abc traders" and "ABC Traders" are folded together; the label carries
 * how many voucher entries name the party, so the user can see the weight before filtering.
 */
function partyOptions(rows: ProjectStatementRow[], field: "paidTo" | "receivedFrom") {
  const parties = new Map<string, { name: string; entries: Set<string> }>();
  for (const r of rows) {
    const name = r[field]?.trim();
    if (!name) continue;
    const key = name.toLowerCase();
    const party = parties.get(key) ?? { name, entries: new Set<string>() };
    party.entries.add(r.entryNumber);
    parties.set(key, party);
  }
  return [...parties.entries()]
    .map(([key, p]) => ({
      value: key,
      label: `${p.name} (${p.entries.size})`,
      searchText: p.name,
    }))
    .sort((a, b) => a.searchText.localeCompare(b.searchText));
}

const partyMatches = (name: string | undefined, key: string) => name?.trim().toLowerCase() === key;

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
  const rows = useMemo(() => statement?.rows ?? [], [statement]);
  const byAccount = useMemo(() => statement?.byAccount ?? [], [statement]);
  const { downloadPdf, isDownloading } = useReportPdf();

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

  // ── Transactions ledger: client-side search / filter / sort / pagination ──
  // The ledger is already fully loaded with the statement, so the view is refined in memory
  // rather than re-fetched. Print and the PDF carry the same filtered, sorted rows (all pages):
  // Print renders `txSorted` directly, and the PDF request sends every filter to the server,
  // which applies the same rules (ProjectStatementService.FilterRows — keep the two in step).
  // Transaction Type also decides which parties the party dropdowns offer, so it comes first.
  const [txType, setTxType] = useState<ProjectStatementTransactionFilter>("All");
  const typeRows = useMemo(() => rows.filter(TX_TYPE_FILTER[txType]), [rows, txType]);

  const [txSearch, setTxSearch] = useState("");
  const [txAccount, setTxAccount] = useState("");
  const [txDirection, setTxDirection] = useState<"" | "debit" | "credit">("");
  const [txPaidTo, setTxPaidTo] = useState("");
  const [txReceivedFrom, setTxReceivedFrom] = useState("");
  const [txSort, setTxSort] = useState<{ key: TxSortKey; dir: "asc" | "desc" }>({ key: "entryDate", dir: "asc" });
  const [txPage, setTxPage] = useState(1);
  const [txPageSize, setTxPageSize] = useState(25);

  const paidToOptions = useMemo(() => partyOptions(typeRows, "paidTo"), [typeRows]);
  const receivedFromOptions = useMemo(() => partyOptions(typeRows, "receivedFrom"), [typeRows]);

  const txFilterActive =
    txType !== "All" || txSearch.trim() !== "" || txAccount !== "" || txDirection !== ""
    || txPaidTo !== "" || txReceivedFrom !== "";

  // Any change to what's shown drops the user back to page 1 — otherwise a filter that shrinks
  // the list can strand them on a now-empty page. (Out-of-range pages from a project/date switch
  // are handled by clamping below.)
  const resetTxPage = () => setTxPage(1);
  const changeTxSearch = (v: string) => { setTxSearch(v); resetTxPage(); };
  const changeTxAccount = (v: string) => { setTxAccount(v); resetTxPage(); };
  const changeTxDirection = (v: "" | "debit" | "credit") => { setTxDirection(v); resetTxPage(); };
  const changeTxPaidTo = (v: string) => { setTxPaidTo(v); resetTxPage(); };
  const changeTxReceivedFrom = (v: string) => { setTxReceivedFrom(v); resetTxPage(); };
  // A payment has no "Received from" and a receipt no "Paid to", so narrowing the type drops the
  // party filter that can no longer match anything instead of leaving an empty table behind.
  const changeTxType = (v: ProjectStatementTransactionFilter) => {
    setTxType(v);
    if (v === "Receipts") setTxPaidTo("");
    if (v === "Payments") setTxReceivedFrom("");
    resetTxPage();
  };
  const changeTxPageSize = (n: number) => { setTxPageSize(n); resetTxPage(); };
  const clearTxFilters = () => {
    setTxType("All"); setTxSearch(""); setTxAccount(""); setTxDirection(""); setTxPaidTo(""); setTxReceivedFrom("");
    resetTxPage();
  };

  // Ledger filters belong to one project's ledger — an account or party picked on one project
  // means nothing on the next, and would silently empty its table.
  const selectProject = (id: string) => { setProjectId(id); clearTxFilters(); };

  const toggleTxSort = (key: TxSortKey) => {
    setTxSort((s) => (s.key === key ? { key, dir: s.dir === "asc" ? "desc" : "asc" } : { key, dir: "asc" }));
    resetTxPage();
  };

  const txFiltered = useMemo(() => {
    const q = txSearch.trim().toLowerCase();
    return typeRows.filter((r) => {
      if (q) {
        const hay = [
          r.entryNumber,
          r.referenceNumber ?? "",
          r.accountCode,
          r.accountName,
          r.narration ?? "",
          r.paidTo ?? "",
          r.receivedFrom ?? "",
          r.costCenterName,
        ].join(" ").toLowerCase();
        if (!hay.includes(q)) return false;
      }
      if (txAccount && r.accountId !== txAccount) return false;
      if (txDirection === "debit" && !(r.debit > 0)) return false;
      if (txDirection === "credit" && !(r.credit > 0)) return false;
      if (txPaidTo && !partyMatches(r.paidTo, txPaidTo)) return false;
      if (txReceivedFrom && !partyMatches(r.receivedFrom, txReceivedFrom)) return false;
      return true;
    });
  }, [typeRows, txSearch, txAccount, txDirection, txPaidTo, txReceivedFrom]);

  const txSorted = useMemo(() => {
    const list = [...txFiltered];
    const { key, dir } = txSort;
    list.sort((a, b) => {
      let cmp: number;
      if (key === "entryDate") {
        cmp = new Date(a.entryDate).getTime() - new Date(b.entryDate).getTime();
      } else {
        const av = a[key];
        const bv = b[key];
        cmp = typeof av === "number" && typeof bv === "number"
          ? av - bv
          : String(av ?? "").localeCompare(String(bv ?? ""));
      }
      return dir === "asc" ? cmp : -cmp;
    });
    return list;
  }, [txFiltered, txSort]);

  const txTotalPages = Math.max(1, Math.ceil(txSorted.length / txPageSize));
  const txPageSafe = Math.min(txPage, txTotalPages);
  const txPageRows = useMemo(
    () => txSorted.slice((txPageSafe - 1) * txPageSize, txPageSafe * txPageSize),
    [txSorted, txPageSafe, txPageSize],
  );

  const txDebitTotal = useMemo(() => txFiltered.reduce((s, r) => s + r.debit, 0), [txFiltered]);
  const txCreditTotal = useMemo(() => txFiltered.reduce((s, r) => s + r.credit, 0), [txFiltered]);
  const txRangeStart = txSorted.length === 0 ? 0 : (txPageSafe - 1) * txPageSize + 1;
  const txRangeEnd = Math.min(txPageSafe * txPageSize, txSorted.length);

  // Party filters hold a case-folded key; the printout, the PDF request and its heading need the
  // name as the user saw it in the dropdown.
  const paidToName = paidToOptions.find((o) => o.value === txPaidTo)?.searchText ?? "";
  const receivedFromName = receivedFromOptions.find((o) => o.value === txReceivedFrom)?.searchText ?? "";
  const txAccountRow = byAccount.find((a) => a.accountId === txAccount);

  /** The active ledger filters in words — shown on screen, in the print header and the PDF. */
  const txFilterSummary = [
    txType === "Receipts" ? "Receipts only" : txType === "Payments" ? "Payments only" : "",
    txSearch.trim() ? `Search "${txSearch.trim()}"` : "",
    txAccountRow ? `Account ${txAccountRow.accountCode} · ${txAccountRow.accountName}` : "",
    txDirection === "debit" ? "Debit lines only" : txDirection === "credit" ? "Credit lines only" : "",
    paidToName ? `Paid to ${paidToName}` : "",
    receivedFromName ? `Received from ${receivedFromName}` : "",
  ].filter(Boolean).join(" · ");

  // Posting order is the server's default, so only a sort the user actually chose is sent.
  const txSortIsDefault = txSort.key === "entryDate" && txSort.dir === "asc";
  const txPdfParams = {
    transactionType: txType !== "All" ? txType : undefined,
    search: txSearch.trim() || undefined,
    accountId: txAccount || undefined,
    side: txDirection === "debit" ? "Debit" : txDirection === "credit" ? "Credit" : undefined,
    paidTo: paidToName || undefined,
    receivedFrom: receivedFromName || undefined,
    sortBy: txSortIsDefault ? undefined : txSort.key,
    sortDescending: txSortIsDefault ? undefined : txSort.dir === "desc",
  };

  const period =
    dateFrom && dateTo ? `${dateFmt(dateFrom)} — ${dateFmt(dateTo)}`
    : dateFrom ? `From ${dateFmt(dateFrom)}`
    : dateTo ? `Up to ${dateFmt(dateTo)}`
    : "All time";

  const invalidRange = dateFrom !== "" && dateTo !== "" && dateFrom > dateTo;

  const subtitle =
    isAll
      ? cs ? `All projects (${cs.projects.length}) · ${period}` : "All projects across every cost centre"
      : statement
        ? `${statement.projectName} (${statement.projectCode}) · ${period}${txFilterSummary ? ` · Transactions filtered by: ${txFilterSummary}` : ""}`
      : "Select a project to view its accounting statement";

  return (
    <ReportLayout
      title="Project Statement"
      subtitle={subtitle}
      loading={single.isLoading || consolidated.isLoading}
      onExportPdf={
        projectId
          ? () =>
              downloadPdf({
                url: isAll ? reportPdfUrls.consolidatedProjectStatement : reportPdfUrls.projectStatement(projectId),
                params: {
                  dateFrom: dateFrom || undefined,
                  dateTo: dateTo || undefined,
                  ...(isAll ? {} : txPdfParams),
                },
                filename: "ProjectStatement.pdf",
              })
          : undefined
      }
      exportingPdf={isDownloading}
    >
      {/* Filters */}
      <div className="no-print mb-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
          <SearchableDropdown
            label="Project *"
            options={projectOptions}
            loading={loadingProjects}
            value={projectId}
            onChange={(v) => selectProject(v ?? "")}
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
              onClick={() => { selectProject(""); setDateFrom(""); setDateTo(""); }}
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
                        onClick={() => selectProject(p.projectId)}
                        className="border-b border-gray-100 hover:bg-gray-50/50 cursor-pointer"
                      >
                        <td data-col="projectCode" className="px-4 py-2 whitespace-nowrap">
                          <span className="text-xs font-mono text-gray-500">{p.projectCode}</span>
                          <span className="text-sm ml-2">{p.projectName}</span>
                        </td>
                        <td data-col="contractValue" className="px-4 py-2 text-right tabular-nums">{fmt(p.contractValue)}</td>
                        <td data-col="openingSpend" className="px-4 py-2 text-right tabular-nums">{fmt(p.openingSpend)}</td>
                        <td data-col="periodExpense" className="px-4 py-2 text-right tabular-nums text-red-600">{fmt(p.periodExpense)}</td>
                        <td data-col="periodIncome" className="px-4 py-2 text-right tabular-nums text-green-600">{fmt(p.periodIncome)}</td>
                        <td data-col="closingSpend" className="px-4 py-2 text-right tabular-nums text-red-600">{fmt(p.closingSpend)}</td>
                        <td data-col="receipts" className="px-4 py-2 text-right tabular-nums text-green-600">{fmt(p.receipts)}</td>
                        <td data-col="payments" className="px-4 py-2 text-right tabular-nums text-red-600">{fmt(p.payments)}</td>
                        <td data-col="netCashPosition" className={`px-4 py-2 text-right tabular-nums font-medium ${p.netCashPosition >= 0 ? "text-green-600" : "text-red-600"}`}>
                          {fmt(p.netCashPosition)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  {sortedProjects.length > 0 && (
                    <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                      <tr>
                        <td className="px-4 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                        <td data-col="contractValue" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.contractValue)}</td>
                        <td data-col="openingSpend" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.openingSpend)}</td>
                        <td data-col="periodExpense" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.periodExpense)}</td>
                        <td data-col="periodIncome" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.periodIncome)}</td>
                        <td data-col="closingSpend" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.closingSpend)}</td>
                        <td data-col="receipts" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.receipts)}</td>
                        <td data-col="payments" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.payments)}</td>
                        <td data-col="netCashPosition" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(cs.netCashPosition)}</td>
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
                    <th data-col="account" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Account</th>
                    <th data-col="acctType" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
                    <th data-col="debit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
                    <th data-col="credit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
                    <th data-col="acctNet" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Net</th>
                  </tr>
                </thead>
                <tbody>
                  {byAccount.map((a) => (
                    <tr key={a.accountId} className="border-b border-gray-100 hover:bg-gray-50/50">
                      <td data-col="account" className="px-4 py-2">
                        <span className="text-xs font-mono text-gray-500">{a.accountCode}</span>
                        <span className="text-sm ml-2">{a.accountName}</span>
                      </td>
                      <td data-col="acctType" className="px-4 py-2 text-xs text-gray-500">{a.accountType}</td>
                      <td data-col="debit" className="px-4 py-2 text-right tabular-nums">{a.debit > 0 ? fmt(a.debit) : "—"}</td>
                      <td data-col="credit" className="px-4 py-2 text-right tabular-nums">{a.credit > 0 ? fmt(a.credit) : "—"}</td>
                      <td data-col="acctNet" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(a.net)}</td>
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
            <div className="px-4 py-3 border-b border-gray-100 flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-gray-900">
                  Transactions
                  {txFilterActive && <span className="ml-2 text-xs font-normal text-gray-500">(filtered)</span>}
                </p>
                <p className="text-xs text-gray-400 mt-0.5">
                  Posted journal entry lines charged to this project
                </p>
              </div>
              {rows.length > 0 && (
                <label className="no-print flex items-center gap-2 text-xs text-gray-500">
                  Transaction Type
                  <select
                    value={txType}
                    onChange={(e) => changeTxType(e.target.value as ProjectStatementTransactionFilter)}
                    className="text-sm text-gray-900 border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
                  >
                    <option value="All">All</option>
                    <option value="Receipts">Receipts</option>
                    <option value="Payments">Payments</option>
                  </select>
                </label>
              )}
            </div>

            {/* Search + filters — screen only, so a printed statement keeps the full ledger */}
            {rows.length > 0 && (
              <div className="no-print px-4 py-3 border-b border-gray-100 flex flex-wrap items-center gap-2">
                <div className="relative flex-1 min-w-[200px] max-w-xs">
                  <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="text"
                    value={txSearch}
                    onChange={(e) => changeTxSearch(e.target.value)}
                    placeholder="Search entry #, reference, account, narration, party…"
                    className="w-full text-sm border border-gray-200 rounded-lg pl-9 pr-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
                  />
                </div>
                <select
                  value={txAccount}
                  onChange={(e) => changeTxAccount(e.target.value)}
                  className="text-sm border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
                >
                  <option value="">All accounts</option>
                  {byAccount.map((a) => (
                    <option key={a.accountId} value={a.accountId}>
                      {a.accountCode} · {a.accountName}
                    </option>
                  ))}
                </select>
                <select
                  value={txDirection}
                  onChange={(e) => changeTxDirection(e.target.value as "" | "debit" | "credit")}
                  className="text-sm border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
                >
                  <option value="">Debit &amp; credit</option>
                  <option value="debit">Debit lines only</option>
                  <option value="credit">Credit lines only</option>
                </select>
                <SearchableDropdown
                  className="w-52"
                  buttonClassName="py-1.5"
                  options={paidToOptions}
                  value={txPaidTo}
                  onChange={(v) => changeTxPaidTo(v ?? "")}
                  disabled={paidToOptions.length === 0}
                  placeholder={paidToOptions.length === 0 ? "No payees in period" : "Paid to: anyone"}
                  searchPlaceholder="Search payee…"
                  noResultsMessage="No matching payee"
                />
                <SearchableDropdown
                  className="w-52"
                  buttonClassName="py-1.5"
                  options={receivedFromOptions}
                  value={txReceivedFrom}
                  onChange={(v) => changeTxReceivedFrom(v ?? "")}
                  disabled={receivedFromOptions.length === 0}
                  placeholder={receivedFromOptions.length === 0 ? "No payers in period" : "Received from: anyone"}
                  searchPlaceholder="Search payer…"
                  noResultsMessage="No matching payer"
                />
                {txFilterActive && (
                  <button
                    onClick={clearTxFilters}
                    className="inline-flex items-center gap-1 px-2 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500"
                  >
                    <X size={12} /> Clear
                  </button>
                )}
              </div>
            )}

            <div className="overflow-x-auto">
              {/* Interactive table — the paged / sorted / filtered view (hidden when printing) */}
              <table className="w-full text-sm print:hidden">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-200">
                    <SortHeader label="Date" sortKey="entryDate" sort={txSort} onSort={toggleTxSort} align="left" />
                    <SortHeader label="Entry #" sortKey="entryNumber" sort={txSort} onSort={toggleTxSort} align="left" />
                    <SortHeader label="Reference" sortKey="referenceNumber" sort={txSort} onSort={toggleTxSort} align="left" />
                    <SortHeader label="Account" sortKey="accountCode" sort={txSort} onSort={toggleTxSort} align="left" />
                    <SortHeader label="Narration" sortKey="narration" sort={txSort} onSort={toggleTxSort} align="left" />
                    <SortHeader label="Debit" sortKey="debit" sort={txSort} onSort={toggleTxSort} />
                    <SortHeader label="Credit" sortKey="credit" sort={txSort} onSort={toggleTxSort} />
                    <SortHeader label="Running Net" sortKey="runningNet" sort={txSort} onSort={toggleTxSort} />
                  </tr>
                </thead>
                <tbody>
                  {txPageRows.map((r, i) => (
                    <tr key={`${r.entryNumber}-${r.accountId}-${i}`} className="border-b border-gray-100 hover:bg-gray-50/50">
                      <td data-col="entryDate" className="px-4 py-2 text-xs whitespace-nowrap">{dateFmt(r.entryDate)}</td>
                      <td data-col="entryNumber" className="px-4 py-2 text-xs font-mono text-gray-500 whitespace-nowrap">{r.entryNumber}</td>
                      <td data-col="referenceNumber" className="px-4 py-2 text-xs text-gray-500 whitespace-nowrap">{r.referenceNumber ?? "—"}</td>
                      <td data-col="accountCode" className="px-4 py-2 text-xs">
                        <span className="font-mono text-gray-500">{r.accountCode}</span>
                        <span className="ml-2">{r.accountName}</span>
                      </td>
                      <td data-col="narration" className="px-4 py-2 text-sm max-w-xs">
                        <p className="truncate">{r.narration ?? "—"}</p>
                        {(r.paidTo || r.receivedFrom) && (
                          <p className="text-[11px] text-gray-400 truncate">
                            {r.paidTo ? `Paid to ${r.paidTo}` : `Received from ${r.receivedFrom}`}
                          </p>
                        )}
                      </td>
                      <td data-col="debit" className="px-4 py-2 text-right tabular-nums">{r.debit > 0 ? fmt(r.debit) : "—"}</td>
                      <td data-col="credit" className="px-4 py-2 text-right tabular-nums">{r.credit > 0 ? fmt(r.credit) : "—"}</td>
                      <td data-col="runningNet" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(r.runningNet)}</td>
                    </tr>
                  ))}
                </tbody>
                {txSorted.length > 0 && (
                  <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                    <tr>
                      <td
                        colSpan={5}
                        data-col-span="entryDate,entryNumber,referenceNumber,accountCode,narration"
                        className="px-4 py-2.5 text-xs font-semibold text-gray-600"
                      >
                        {txFilterActive ? "Totals (filtered)" : "Totals"}
                      </td>
                      <td data-col="debit" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(txDebitTotal)}</td>
                      <td data-col="credit" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(txCreditTotal)}</td>
                      <td data-col="runningNet" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(txDebitTotal - txCreditTotal)}</td>
                    </tr>
                  </tfoot>
                )}
              </table>

              {/* Print-only: exactly the rows the user filtered and sorted to, every page of them
                  (paging is a screen convenience; a printout should not stop at page 1) */}
              <table className="hidden print:table w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-200">
                    <th data-col="entryDate" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Date</th>
                    <th data-col="entryNumber" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Entry #</th>
                    <th data-col="referenceNumber" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Reference</th>
                    <th data-col="accountCode" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Account</th>
                    <th data-col="narration" className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Narration</th>
                    <th data-col="debit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Debit</th>
                    <th data-col="credit" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Credit</th>
                    <th data-col="runningNet" className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Running Net</th>
                  </tr>
                </thead>
                <tbody>
                  {txSorted.map((r, i) => (
                    <tr key={`print-${r.entryNumber}-${i}`} className="border-b border-gray-100">
                      <td data-col="entryDate" className="px-4 py-2 text-xs whitespace-nowrap">{dateFmt(r.entryDate)}</td>
                      <td data-col="entryNumber" className="px-4 py-2 text-xs font-mono text-gray-500 whitespace-nowrap">{r.entryNumber}</td>
                      <td data-col="referenceNumber" className="px-4 py-2 text-xs text-gray-500 whitespace-nowrap">{r.referenceNumber ?? "—"}</td>
                      <td data-col="accountCode" className="px-4 py-2 text-xs">
                        <span className="font-mono text-gray-500">{r.accountCode}</span>
                        <span className="ml-2">{r.accountName}</span>
                      </td>
                      <td data-col="narration" className="px-4 py-2 text-sm">
                        <p>{r.narration ?? "—"}</p>
                        {(r.paidTo || r.receivedFrom) && (
                          <p className="text-[11px] text-gray-400">
                            {r.paidTo ? `Paid to ${r.paidTo}` : `Received from ${r.receivedFrom}`}
                          </p>
                        )}
                      </td>
                      <td data-col="debit" className="px-4 py-2 text-right tabular-nums">{r.debit > 0 ? fmt(r.debit) : "—"}</td>
                      <td data-col="credit" className="px-4 py-2 text-right tabular-nums">{r.credit > 0 ? fmt(r.credit) : "—"}</td>
                      <td data-col="runningNet" className="px-4 py-2 text-right tabular-nums font-medium">{fmt(r.runningNet)}</td>
                    </tr>
                  ))}
                </tbody>
                {txSorted.length > 0 && (
                  <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                    <tr>
                      <td
                        colSpan={5}
                        data-col-span="entryDate,entryNumber,referenceNumber,accountCode,narration"
                        className="px-4 py-2.5 text-xs font-semibold text-gray-600"
                      >
                        {txFilterActive ? "Totals (filtered)" : "Totals"}
                      </td>
                      {/* Same figures as the screen's footer, which totals the same rows */}
                      <td data-col="debit" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(txDebitTotal)}</td>
                      <td data-col="credit" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(txCreditTotal)}</td>
                      <td data-col="runningNet" className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">{fmt(txDebitTotal - txCreditTotal)}</td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>

            {/* Pagination — screen only */}
            {txSorted.length > 0 && (
              <div className="no-print flex items-center justify-between gap-3 px-4 py-3 border-t border-gray-100 text-xs text-gray-500">
                <div className="flex items-center gap-2">
                  <span>
                    {txRangeStart}–{txRangeEnd} of {txSorted.length}
                    {txFilterActive && rows.length !== txSorted.length ? ` (filtered from ${rows.length})` : ""}
                  </span>
                  <span className="text-gray-200">|</span>
                  <span>Show</span>
                  <select
                    value={txPageSize}
                    onChange={(e) => changeTxPageSize(Number(e.target.value))}
                    className="border border-gray-200 rounded px-1.5 py-0.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500"
                  >
                    {TX_PAGE_SIZES.map((s) => (
                      <option key={s} value={s}>{s}</option>
                    ))}
                  </select>
                </div>
                {txTotalPages > 1 && (
                  <div className="flex items-center gap-2">
                    <span>Page {txPageSafe} of {txTotalPages}</span>
                    <button
                      disabled={txPageSafe <= 1}
                      onClick={() => setTxPage(txPageSafe - 1)}
                      className="p-1 border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50"
                    >
                      <ChevronLeft size={14} />
                    </button>
                    <button
                      disabled={txPageSafe >= txTotalPages}
                      onClick={() => setTxPage(txPageSafe + 1)}
                      className="p-1 border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50"
                    >
                      <ChevronRight size={14} />
                    </button>
                  </div>
                )}
              </div>
            )}

            {rows.length === 0 && !single.isLoading && (
              <div className="text-center py-12 text-sm text-gray-400">
                No posted transactions for this project in the selected period
              </div>
            )}
            {/* Shown in print too: a filtered printout with no rows must say why it is empty */}
            {rows.length > 0 && txSorted.length === 0 && (
              <div className="text-center py-12 text-sm text-gray-400">
                {typeRows.length === 0
                  ? `No ${txType === "Receipts" ? "receipt" : "payment"} transactions for this project in the selected period`
                  : "No transactions match your search or filters"}
              </div>
            )}
          </div>
        </div>
      ) : null}
    </ReportLayout>
  );
}

function SortHeader<K extends string>({ label, sortKey, sort, onSort, align = "right" }: {
  label: string;
  sortKey: K;
  sort: { key: K; dir: "asc" | "desc" };
  onSort: (key: K) => void;
  align?: "left" | "right";
}) {
  const active = sort.key === sortKey;
  return (
    <th
      onClick={() => onSort(sortKey)}
      data-col={sortKey}
      className={`px-4 py-2.5 text-xs font-medium text-gray-500 cursor-pointer select-none hover:text-gray-700 ${align === "left" ? "text-left" : "text-right"}`}
    >
      <span className={`inline-flex items-center gap-1 ${align === "right" ? "flex-row-reverse" : ""}`}>
        {label}
        {active && (sort.dir === "asc" ? <ArrowUp size={12} /> : <ArrowDown size={12} />)}
      </span>
    </th>
  );
}
