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
  const dg = useDataGrid({ defaultSortBy: "voucherDate", defaultSortDescending: true });

  // Filters are read from (and written back to) the URL query string, so leaving the page to view
  // a voucher and coming back restores the exact list state the user was looking at via browser
  // history, and the filtered view stays shareable/bookmarkable.
  const paramType = (searchParams.get("type") as VoucherType | "") || "";
  const paramStatus = (searchParams.get("status") as VoucherStatus | "") || "";
  const paramFrom = searchParams.get("from") ?? "";
  const paramTo = searchParams.get("to") ?? "";
  const paramProject = searchParams.get("project") ?? "";

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

  /** Reflects a changed filter to the URL (and thus to browser history), replacing the current entry. */
  function setFilter(key: string, value: string) {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value);
    else next.delete(key);
    setSearchParams(next, { replace: true });
    dg.resetPage();
  }

  function resetFilters() {
    const next = new URLSearchParams(searchParams);
    for (const key of ["type", "status", "from", "to", "project"]) next.delete(key);
    setSearchParams(next, { replace: true });
    dg.resetPage();
  }

  const columns: DataGridColumn<Voucher>[] = [
    { key: "voucherNumber", label: "Voucher #" },
    {
      key: "voucherType",
      label: "Type",
      render: (row) => {
        const theme = voucherThemes[row.voucherType];
        return (
          <span className={`inline-flex items-center gap-1.5 text-xs px-2 py-0.5 rounded-full font-medium ${theme.badge}`}>
            <span className={`w-1.5 h-1.5 rounded-full ${theme.bar}`} />
            {theme.code}
          </span>
        );
      },
    },
    {
      key: "voucherDate",
      label: "Date",
      render: (row) => row.voucherDate?.split("T")[0] ?? "",
    },
    { key: "name", label: "Name", render: (row) => row.name ?? "—" },
    {
      key: "referenceNumber",
      label: "Ref. No.",
      render: (row) => row.referenceNumber ?? "—",
      className: "text-xs text-gray-500",
    },
    {
      // The cost center name is the project name for project vouchers — one is auto-created
      // per project — so this one column covers both project and overhead vouchers.
      key: "costCenterName",
      label: "Charged To",
      sortable: false,
      render: (row) =>
        row.costCenterName ? (
          <span className="inline-flex items-center gap-1.5">
            <span
              className={`w-1.5 h-1.5 rounded-full ${row.projectId ? "bg-primary-400" : "bg-gray-300"}`}
              title={row.projectId ? "Project" : "Head office / overhead"}
            />
            <span className="text-xs text-gray-600">{row.costCenterName}</span>
          </span>
        ) : (
          <span className="text-xs text-gray-300">—</span>
        ),
    },
    {
      key: "description",
      label: "Description",
      render: (row) => (
        <span className="block max-w-xs truncate" title={row.description}>
          {row.description}
        </span>
      ),
    },
    {
      key: "amount",
      label: "Amount",
      className: "text-right tabular-nums",
      headerClassName: "text-right",
      render: (row) => formatTaka(row.amount),
    },
    {
      key: "status",
      label: "Status",
      render: (row) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span>
      ),
    },
    {
      key: "journalEntryNumber",
      label: "Journal Entry",
      sortable: false,
      render: (row) =>
        row.journalEntryNumber ? (
          <span className="text-xs text-gray-500 tabular-nums">{row.journalEntryNumber}</span>
        ) : (
          <span className="text-xs text-gray-300">—</span>
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
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search voucher no, description, name..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
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
