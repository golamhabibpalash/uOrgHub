import { useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getJournalEntryReport, JournalEntryReportRow } from "../../../api/accounts";
import ReportLayout from "../../../components/shared/ReportLayout";
import DateInput from "../../../components/shared/DateInput";
import DataGrid, { DataGridColumn } from "../../../components/shared/DataGrid";
import { useDataGrid } from "../../../hooks/useDataGrid";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Posted: "bg-green-50 text-green-700",
  Cancelled: "bg-red-100 text-red-600",
};

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

export default function JournalEntryReportPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const paramPage = Number(searchParams.get("page")) || 1;
  const paramPageSize = Number(searchParams.get("pageSize")) || 10;
  const paramSearch = searchParams.get("search") ?? "";
  const paramSortBy = searchParams.get("sortBy") || undefined;
  const paramSortDesc = searchParams.get("sortDesc") === "true";
  const paramFrom = searchParams.get("from") ?? "";
  const paramTo = searchParams.get("to") ?? "";
  const paramStatus = searchParams.get("status") ?? "";

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
    queryKey: ["report-journal-entries", ...dg.queryKey, paramFrom, paramTo, paramStatus],
    queryFn: () =>
      getJournalEntryReport({
        ...(paramFrom && { dateFrom: paramFrom }),
        ...(paramTo && { dateTo: paramTo }),
        ...(paramStatus && { status: paramStatus }),
        ...dg.queryParams,
      }),
    // Keeps the previous page on screen while the next one loads, so typing in the search box
    // or paging doesn't blank the table out from under the reader (or reset the search input).
    placeholderData: (prev) => prev,
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });
  const dateFmt = (d: string) => new Date(d).toLocaleDateString("en-BD");

  const rangePart =
    paramFrom && paramTo
      ? `From ${dateFmt(paramFrom)} to ${dateFmt(paramTo)}`
      : paramFrom
        ? `From ${dateFmt(paramFrom)} onwards`
        : paramTo
          ? `Up to ${dateFmt(paramTo)}`
          : "";
  const subtitle = [rangePart, paramStatus].filter(Boolean).join(" · ") || "All journal entries with filters";

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
      status: paramStatus || undefined,
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
    persist(buildEntries({ from: undefined, to: undefined, status: undefined, page: "1" }));
    dg.resetPage();
  }

  const columns: DataGridColumn<JournalEntryReportRow>[] = [
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
      key: "totalDebit",
      label: "Debit",
      className: "text-right tabular-nums",
      headerClassName: "text-right",
      render: (row) => fmt(row.totalDebit),
    },
    {
      key: "totalCredit",
      label: "Credit",
      className: "text-right tabular-nums",
      headerClassName: "text-right",
      render: (row) => fmt(row.totalCredit),
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
      label: "Created By",
      width: "130px",
      render: (row) => <span className="text-xs text-gray-500">{row.createdBy}</span>,
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
        <label className="text-xs text-gray-500 mb-1 block">Status</label>
        <select className={selectClass} value={paramStatus} onChange={(e) => setFilter("status", e.target.value)}>
          <option value="">All statuses</option>
          <option value="Draft">Draft</option>
          <option value="Posted">Posted</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>
      {(paramFrom || paramTo || paramStatus) && (
        <button onClick={resetFilters} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-500">
          Clear
        </button>
      )}
    </div>
  );

  return (
    <ReportLayout title="Journal Entry Report" subtitle={subtitle} filters={filters}>
      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={handleSort}
        search={dg.search}
        onSearch={handleSearch}
        searchPlaceholder="Search entry #, reference, description..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={handlePageChange}
        pageSize={dg.pageSize}
        onPageSizeChange={handlePageSizeChange}
        totalCount={totalCount}
        emptyMessage="No journal entries found"
      />
    </ReportLayout>
  );
}
