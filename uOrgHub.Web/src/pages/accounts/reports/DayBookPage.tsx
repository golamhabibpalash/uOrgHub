import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { ChevronDown, ChevronUp } from "lucide-react";
import { getDayBook, getJournalEntryById, DayBookRow, DayBookType } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import DataGrid, { DataGridColumn } from "../../../components/shared/DataGrid";
import { useDataGrid } from "../../../hooks/useDataGrid";

const typeThemes: Record<DayBookType, string> = {
  DR: "bg-red-50 text-red-600",
  CR: "bg-green-50 text-green-700",
  CN: "bg-blue-50 text-blue-700",
  JV: "bg-purple-50 text-purple-600",
};

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Posted: "bg-green-50 text-green-700",
  Cancelled: "bg-red-100 text-red-600",
};

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

export default function DayBookPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const paramPage = Number(searchParams.get("page")) || 1;
  const paramPageSize = Number(searchParams.get("pageSize")) || 10;
  const paramSearch = searchParams.get("search") ?? "";
  const paramSortBy = searchParams.get("sortBy") || undefined;
  const paramSortDesc = searchParams.get("sortDesc") === "true";
  const paramFrom = searchParams.get("from") ?? "";
  const paramTo = searchParams.get("to") ?? "";
  const paramType = (searchParams.get("type") as DayBookType | "") || "";

  const dg = useDataGrid({
    defaultSortBy: "entryDate",
    defaultSortDescending: true,
    initialPage: paramPage,
    initialPageSize: paramPageSize,
    initialSearch: paramSearch,
    initialSortBy: paramSortBy,
    initialSortDescending: paramSortDesc,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["report-day-book", ...dg.queryKey, paramFrom, paramTo, paramType],
    queryFn: () =>
      getDayBook({
        ...(paramFrom && { dateFrom: paramFrom }),
        ...(paramTo && { dateTo: paramTo }),
        ...(paramType && { type: paramType }),
        ...dg.queryParams,
      }),
    // Keeps the previous page on screen while the next one loads, so typing in the search box
    // or paging doesn't blank the table out from under the reader (or reset the search input).
    placeholderData: (prev) => prev,
  });

  const report = data?.data?.data;
  const rows = report?.rows?.items ?? [];
  const totalPages = report?.rows?.totalPages ?? 1;
  const totalCount = report?.rows?.totalCount ?? 0;

  // Day book rows are lightweight aggregates (no lines) for list performance, so a row's lines
  // are only fetched once the reader actually expands it.
  const { data: detailData, isLoading: detailLoading } = useQuery({
    queryKey: ["journal-entry-detail", expandedId],
    queryFn: () => getJournalEntryById(expandedId!),
    enabled: !!expandedId,
  });
  const detail = detailData?.data?.data;

  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  const typeLabel: Record<DayBookType, string> = { DR: "Debit", CR: "Credit", CN: "Contra", JV: "Journal" };
  const rangePart =
    paramFrom && paramTo
      ? `From ${dateFmt(paramFrom)} to ${dateFmt(paramTo)}`
      : paramFrom
        ? `From ${dateFmt(paramFrom)} onwards`
        : paramTo
          ? `Up to ${dateFmt(paramTo)}`
          : "";
  const subtitle = [rangePart, paramType ? typeLabel[paramType] : ""].filter(Boolean).join(" · ") ||
    "All journal entries in the register";

  /** Overwrites (replace) the URL query string with the given entries, keeping document-navigation clean. */
  function persist(entries: [string, string | undefined][]) {
    const next = new URLSearchParams();
    for (const [key, value] of entries) {
      if (value) next.set(key, value);
    }
    setSearchParams(next, { replace: true });
  }

  /** The full list view state as URL query entries — filters plus current grid state. */
  function buildEntries(overrides: Record<string, string | undefined>): [string, string | undefined][] {
    const base: Record<string, string | undefined> = {
      from: paramFrom || undefined,
      to: paramTo || undefined,
      type: paramType || undefined,
      pageSize: String(dg.pageSize),
      search: dg.search || undefined,
      sortBy: dg.sortBy,
      sortDesc: dg.sortDescending ? "true" : undefined,
    };
    return Object.entries({ ...base, ...overrides });
  }

  /** Filters: reflect a changed dropdown/date to URL + reset to page 1. */
  function setFilter(key: string, value: string) {
    persist(buildEntries({ [key]: value || undefined, page: undefined }));
    dg.resetPage();
  }

  const handlePageChange = (page: number) => {
    persist(buildEntries({ page: String(page) }));
    dg.setPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    persist(buildEntries({ page: "1", pageSize: String(size) }));
    dg.setPageSize(size);
  };

  const handleSort = (column: string) => {
    let nextSortBy = dg.sortBy;
    let nextDesc = dg.sortDescending;
    if (nextSortBy === column) nextDesc = !nextDesc;
    else nextSortBy = column;
    persist(buildEntries({ page: "1", sortBy: nextSortBy, sortDesc: nextDesc ? "true" : undefined }));
    dg.handleSort(column);
  };

  const handleSearch = (value: string) => {
    persist(buildEntries({ page: "1", search: value || undefined }));
    dg.setSearch(value);
  };

  function resetFilters() {
    persist(buildEntries({ from: undefined, to: undefined, type: undefined, page: "1" }));
    dg.resetPage();
  }

  const columns: DataGridColumn<DayBookRow>[] = [
    {
      key: "entryNumber",
      label: "Entry",
      width: "170px",
      render: (row) => (
        <div className="flex flex-col gap-1">
          <span className="font-mono text-gray-600 tabular-nums whitespace-nowrap">{row.entryNumber}</span>
          <span className="text-[11px] text-gray-400">{dateFmt(row.entryDate)}</span>
        </div>
      ),
    },
    {
      key: "type",
      label: "Type",
      width: "100px",
      render: (row) => (
        <span className={`inline-block text-[10px] px-2 py-0.5 rounded-full font-medium ${typeThemes[row.type]}`}>
          {row.type}
        </span>
      ),
    },
    {
      key: "referenceNumber",
      label: "Reference",
      width: "130px",
      render: (row) => <span className="text-xs text-gray-500">{row.referenceNumber ?? "—"}</span>,
    },
    {
      key: "description",
      label: "Description",
      render: (row) => (
        <span className="text-sm max-w-md block truncate" title={row.description}>
          {row.description}
        </span>
      ),
    },
    {
      key: "debitTotal",
      label: "Debit",
      className: "text-right tabular-nums",
      headerClassName: "text-right",
      render: (row) => (row.debitTotal > 0 ? fmt(row.debitTotal) : "—"),
    },
    {
      key: "creditTotal",
      label: "Credit",
      className: "text-right tabular-nums",
      headerClassName: "text-right",
      render: (row) => (row.creditTotal > 0 ? fmt(row.creditTotal) : "—"),
    },
    {
      key: "status",
      label: "Status",
      width: "110px",
      render: (row) => (
        <span className={`inline-block text-xs px-2 py-0.5 rounded-full ${statusColors[row.status] ?? "bg-gray-100 text-gray-600"}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: "createdBy",
      label: "Entered By",
      width: "130px",
      render: (row) => <span className="text-xs text-gray-500">{row.createdBy}</span>,
    },
    {
      key: "actions",
      label: "",
      width: "48px",
      sortable: false,
      render: (row) => (
        <button
          onClick={() => setExpandedId(expandedId === row.id ? null : row.id)}
          className="text-gray-400 hover:text-primary-600"
          title="View lines"
        >
          {expandedId === row.id ? <ChevronUp size={15} /> : <ChevronDown size={15} />}
        </button>
      ),
    },
  ];

  const filters = (
    <div className="flex flex-wrap items-end gap-3">
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date From</label>
        <DateInput className={selectClass} value={paramFrom} onChange={(e) => setFilter("from", e.target.value)} />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Date To</label>
        <DateInput className={selectClass} value={paramTo} onChange={(e) => setFilter("to", e.target.value)} />
      </div>
      <div>
        <label className="text-xs text-gray-500 mb-1 block">Type</label>
        <select className={selectClass} value={paramType} onChange={(e) => setFilter("type", e.target.value)}>
          <option value="">All types</option>
          <option value="DR">Debit (DR)</option>
          <option value="CR">Credit (CR)</option>
          <option value="CN">Contra (CN)</option>
          <option value="JV">Journal (JV)</option>
        </select>
      </div>
      {(paramFrom || paramTo || paramType) && (
        <button onClick={resetFilters} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500">
          Clear
        </button>
      )}
    </div>
  );

  return (
    <ReportLayout title="Day Book" subtitle={subtitle} filters={filters}>
      <DataGrid
        columns={columns}
        data={rows}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={handleSort}
        search={dg.search}
        onSearch={handleSearch}
        searchPlaceholder="Search entry #, ref, description..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={handlePageChange}
        pageSize={dg.pageSize}
        onPageSizeChange={handlePageSizeChange}
        totalCount={totalCount}
        emptyMessage="No transactions found"
        expandedRowId={expandedId}
        renderExpandedRow={() =>
          detailLoading ? (
            <p className="text-xs text-gray-400 py-1">Loading lines…</p>
          ) : !detail ? (
            <p className="text-xs text-gray-400 py-1">Could not load lines for this entry.</p>
          ) : (
            <table className="w-full text-xs">
              <thead>
                <tr className="text-gray-500">
                  <th className="text-left pb-1">Account</th>
                  <th className="text-left pb-1">Description</th>
                  <th className="text-left pb-1">Cost Center</th>
                  <th className="text-right pb-1">Debit</th>
                  <th className="text-right pb-1">Credit</th>
                </tr>
              </thead>
              <tbody>
                {detail.lines.map((line) => (
                  <tr key={line.id}>
                    <td className="py-0.5">{line.accountName}</td>
                    <td className="py-0.5 text-gray-500">{line.description}</td>
                    <td className="py-0.5 text-gray-500">{line.costCenterName}</td>
                    <td className="py-0.5 text-right tabular-nums">{line.debitAmount > 0 ? fmt(line.debitAmount) : ""}</td>
                    <td className="py-0.5 text-right tabular-nums">{line.creditAmount > 0 ? fmt(line.creditAmount) : ""}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )
        }
      />
      {totalCount > 0 && (
        <div className="mt-3 flex justify-end">
          <div className="flex items-center gap-8 bg-gray-50 border border-gray-200 rounded-lg px-5 py-2.5">
            <div className="flex items-baseline gap-2">
              <span className="text-xs text-gray-500">Total Debit</span>
              <span className="text-sm font-semibold tabular-nums text-gray-900">{fmt(report?.totalDebit ?? 0)}</span>
            </div>
            <div className="flex items-baseline gap-2">
              <span className="text-xs text-gray-500">Total Credit</span>
              <span className="text-sm font-semibold tabular-nums text-gray-900">{fmt(report?.totalCredit ?? 0)}</span>
            </div>
          </div>
        </div>
      )}
    </ReportLayout>
  );
}