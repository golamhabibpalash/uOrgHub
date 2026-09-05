import { useNavigate, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Plus, Eye, Pencil, Printer } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getVouchers, Voucher, VoucherFilters, VoucherStatus, VoucherType } from "../../api/accounts";
import { useProjectLookup } from "../../hooks/useEntityLookup";
import { voucherThemes } from "../../components/accounts/voucherTheme";
import { formatTaka } from "../../utils/format";
import DateInput from "../../components/shared/DateInput";

const statusColors: Record<VoucherStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Submitted: "bg-amber-50 text-amber-700",
  Approved: "bg-blue-50 text-blue-700",
  Posted: "bg-green-50 text-green-700",
  Rejected: "bg-red-50 text-red-700",
  Cancelled: "bg-gray-100 text-gray-400",
};

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

export default function Vouchers() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // Every piece of list state — filters, search, sort, page — is read from and written back to the
  // URL query string. Leaving the page to view a voucher and coming back then restores exactly what
  // the user was looking at (including the page number) via browser history, and stays shareable.
  const paramPage = Number(searchParams.get("page")) || 1;
  const paramPageSize = Number(searchParams.get("pageSize")) || 10;
  const paramSearch = searchParams.get("search") ?? "";
  const paramSortBy = searchParams.get("sortBy") || undefined;
  const paramSortDesc = searchParams.get("sortDesc") === "true";
  const paramType = (searchParams.get("type") as VoucherType | "") || "";
  const paramStatus = (searchParams.get("status") as VoucherStatus | "") || "";
  const paramFrom = searchParams.get("from") ?? "";
  const paramTo = searchParams.get("to") ?? "";
  const paramProject = searchParams.get("project") ?? "";

  const dg = useDataGrid({
    defaultSortBy: "voucherDate",
    defaultSortDescending: true,
    initialPage: paramPage,
    initialPageSize: paramPageSize,
    initialSearch: paramSearch,
    initialSortBy: paramSortBy,
    initialSortDescending: paramSortDesc,
  });

  const { options: projects } = useProjectLookup();

  const filters: VoucherFilters = {
    ...(paramType && { type: paramType }),
    ...(paramStatus && { status: paramStatus }),
    ...(paramFrom && { fromDate: paramFrom }),
    ...(paramTo && { toDate: paramTo }),
    ...(paramProject && { projectId: paramProject }),
  };

  const { data, isLoading } = useQuery({
    queryKey: ["vouchers", ...dg.queryKey, paramType, paramStatus, paramFrom, paramTo, paramProject],
    queryFn: () => getVouchers(dg.queryParams, filters),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

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
      type: paramType || undefined,
      status: paramStatus || undefined,
      from: paramFrom || undefined,
      to: paramTo || undefined,
      project: paramProject || undefined,
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

  /** Wraps the DataGrid's page setter so a page change is also written to the URL. */
  const handlePageChange = (page: number) => {
    persist(buildEntries({ page: String(page) }));
    dg.setPage(page);
  };

  /** Wraps the DataGrid's pageSize setter. */
  const handlePageSizeChange = (size: number) => {
    persist(buildEntries({ page: "1", pageSize: String(size) }));
    dg.setPageSize(size);
  };

  /** Wraps the DataGrid's sort handler. */
  const handleSort = (column: string) => {
    let nextSortBy = dg.sortBy;
    let nextDesc = dg.sortDescending;
    if (nextSortBy === column) nextDesc = !nextDesc;
    else nextSortBy = column;
    persist(buildEntries({ page: "1", sortBy: nextSortBy, sortDesc: nextDesc ? "true" : undefined }));
    dg.handleSort(column);
  };

  /** Wraps the DataGrid's search setter. */
  const handleSearch = (value: string) => {
    persist(buildEntries({ page: "1", search: value || undefined }));
    dg.setSearch(value);
  };

  function resetFilters() {
    persist(buildEntries({ status: undefined, type: undefined, from: undefined, to: undefined, project: undefined, page: "1" }));
    dg.resetPage();
  }

  const columns: DataGridColumn<Voucher>[] = [
    {
      // Number, type badge and date used to be three separate columns — stacked here since
      // they're always read together and each was too narrow to earn its own column.
      key: "voucherNumber",
      label: "Voucher",
      width: "165px",
      render: (row) => {
        const theme = voucherThemes[row.voucherType];
        return (
          <div className="flex flex-col gap-1">
            <span className="font-medium text-gray-900 tabular-nums whitespace-nowrap">{row.voucherNumber}</span>
            <div className="flex items-center gap-1.5">
              <span className={`inline-flex items-center gap-1 text-[10px] px-1.5 py-0.5 rounded-full font-medium ${theme.badge}`}>
                <span className={`w-1 h-1 rounded-full ${theme.bar}`} />
                {theme.code}
              </span>
              <span className="text-xs text-gray-400">{row.voucherDate?.split("T")[0] ?? ""}</span>
            </div>
          </div>
        );
      },
    },
    {
      // Name is the "who", description is the "what" — reading them together as a two-line
      // block reads more naturally than two columns, and gives the description room to breathe
      // instead of truncating inside a narrow fixed column.
      key: "name",
      label: "Party / Description",
      render: (row) => (
        <div className="flex flex-col gap-1 max-w-sm">
          <span className="text-gray-800">{row.name ?? "—"}</span>
          {row.description && (
            <span className="block text-xs text-gray-400 truncate" title={row.description}>
              {row.description}
            </span>
          )}
        </div>
      ),
    },
    {
      // The cost center name is the project name for project vouchers — one is auto-created
      // per project — so this one column covers both project and overhead vouchers. The
      // physical-slip reference number is folded in as a second line since it belongs to the
      // same context and rarely needs its own column.
      key: "costCenterName",
      label: "Charged To",
      width: "190px",
      sortable: false,
      render: (row) => (
        <div className="flex flex-col gap-1">
          {row.costCenterName ? (
            <span className="inline-flex items-center gap-1.5">
              <span
                className={`w-1.5 h-1.5 rounded-full ${row.projectId ? "bg-primary-400" : "bg-gray-300"}`}
                title={row.projectId ? "Project" : "Head office / overhead"}
              />
              <span className="text-xs text-gray-600">{row.costCenterName}</span>
            </span>
          ) : (
            <span className="text-xs text-gray-300">—</span>
          )}
          {row.referenceNumber && <span className="text-[11px] text-gray-400">Ref: {row.referenceNumber}</span>}
        </div>
      ),
    },
    {
      key: "amount",
      label: "Amount",
      className: "text-right tabular-nums font-medium text-gray-900 whitespace-nowrap",
      headerClassName: "text-right",
      width: "150px",
      render: (row) => formatTaka(row.amount),
    },
    {
      // Journal entry number only exists once a voucher is posted, so it reads naturally as a
      // sub-line under Status rather than its own column.
      key: "status",
      label: "Status",
      width: "130px",
      render: (row) => (
        <div className="flex flex-col gap-1">
          <span className={`inline-block w-fit text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>
            {row.status}
          </span>
          {row.journalEntryNumber && (
            <span className="text-[11px] text-gray-400 tabular-nums">JE {row.journalEntryNumber}</span>
          )}
        </div>
      ),
    },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row) => (
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate(`/accounts/vouchers/${row.id}`)}
            className="text-gray-400 hover:text-primary-600"
            title="View details"
          >
            <Eye size={15} />
          </button>
          {row.status === "Draft" && (
            <button
              onClick={() => navigate(`/accounts/vouchers/${row.id}/edit`)}
              className="text-gray-400 hover:text-primary-600"
              title="Edit draft"
            >
              <Pencil size={14} />
            </button>
          )}
          <button
            onClick={() => navigate(`/accounts/vouchers/${row.id}?print=1`)}
            className="text-gray-400 hover:text-primary-600"
            title="Print voucher"
          >
            <Printer size={14} />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Vouchers</h2>
          <p className="text-xs text-gray-400">Debit and credit vouchers and the journal entries they create</p>
        </div>
        <button
          onClick={() => navigate("/accounts/voucher-entry")}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> New Voucher
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={handleSort}
        search={dg.search}
        onSearch={handleSearch}
        searchPlaceholder="Search voucher no, description, name..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={handlePageChange}
        pageSize={dg.pageSize}
        onPageSizeChange={handlePageSizeChange}
        totalCount={totalCount}
        emptyMessage="No vouchers found"
        toolbarPrefix={
          <div className="flex items-center gap-2">
            <select
              className={selectClass}
              value={paramType}
              onChange={(e) => setFilter("type", e.target.value)}
            >
              <option value="">All types</option>
              <option value="Debit">Debit (DR)</option>
              <option value="Credit">Credit (CR)</option>
              <option value="Contra">Contra (CN)</option>
            </select>
            <select
              className={selectClass}
              value={paramStatus}
              onChange={(e) => setFilter("status", e.target.value)}
            >
              <option value="">All statuses</option>
              <option value="Draft">Draft</option>
              <option value="Submitted">Submitted</option>
              <option value="Approved">Approved</option>
              <option value="Posted">Posted</option>
              <option value="Rejected">Rejected</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
        }
        filterBar={
          <div className="flex items-center gap-2">
            <DateInput
              className={selectClass}
              value={paramFrom}
              onChange={(e) => setFilter("from", e.target.value)}
              title="From date"
            />
            <span className="text-xs text-gray-400">to</span>
            <DateInput
              className={selectClass}
              value={paramTo}
              onChange={(e) => setFilter("to", e.target.value)}
              title="To date"
            />
            <select
              className={selectClass}
              value={paramProject}
              onChange={(e) => setFilter("project", e.target.value)}
              title="Project"
            >
              <option value="">All projects</option>
              {projects.map((p) => (
                <option key={p.value} value={p.value}>{p.label}</option>
              ))}
            </select>
            {(paramType || paramStatus || paramFrom || paramTo || paramProject) && (
              <button onClick={resetFilters} className="text-xs text-gray-400 hover:text-gray-600 underline">
                Clear
              </button>
            )}
          </div>
        }
        actions={<ExportMenu baseUrl="/accounts/vouchers" filters={{ search: dg.search || undefined, ...filters }} />}
      />
    </div>
  );
}
