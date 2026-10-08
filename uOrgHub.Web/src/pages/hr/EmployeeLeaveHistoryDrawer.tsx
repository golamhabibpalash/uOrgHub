import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  X, Search, Mail, Phone, Briefcase, Building2,
  LayoutGrid, Table2, ArrowUp, ArrowDown, ArrowUpDown,
  ChevronLeft, ChevronRight, CalendarDays, AlertCircle,
} from "lucide-react";
import Avatar from "../../components/shared/Avatar";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import ExportMenu from "../../components/shared/ExportMenu";
import PrintButton from "../../components/shared/PrintButton";
import { useDataGrid } from "../../hooks/useDataGrid";
import { useLeaveTypeLookup } from "../../hooks/useEntityLookup";
import { useAuthStore } from "../../store/authStore";
import { getEmployeeById, getLeaveRequests, LeaveRequest } from "../../api/hr";
import type { PrintColumn } from "../../utils/print";

interface Props {
  employeeId: string | null;
  onClose: () => void;
}

const STATUS_OPTIONS = ["Pending", "Approved", "Rejected", "Cancelled"] as const;

const HISTORY_COLUMNS: { key: string; label: string; sortable: boolean; className?: string }[] = [
  { key: "leaveTypeName", label: "Leave Type", sortable: true },
  { key: "startDate", label: "Start Date", sortable: true },
  { key: "endDate", label: "End Date", sortable: true },
  { key: "totalDays", label: "Days", sortable: true },
  { key: "reason", label: "Reason", sortable: false },
  { key: "status", label: "Status", sortable: true },
  { key: "appliedDate", label: "Applied", sortable: true },
  { key: "by", label: "Approved / Rejected By", sortable: false },
];

function fmtDate(value?: string) {
  if (!value) return "—";
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? "—" : d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

function statusClasses(status: string) {
  switch (status) {
    case "Approved": return "bg-green-50 text-green-700 ring-green-200";
    case "Pending": return "bg-yellow-50 text-yellow-700 ring-yellow-200";
    case "Cancelled": return "bg-gray-50 text-gray-600 ring-gray-200";
    case "Rejected": return "bg-red-50 text-red-600 ring-red-200";
    default: return "bg-gray-50 text-gray-600 ring-gray-200";
  }
}

function StatusBadge({ status }: { status: string }) {
  return <span className={`text-xs px-2 py-0.5 rounded-full ring-1 ${statusClasses(status)}`}>{status}</span>;
}

function approverLabel(row: LeaveRequest) {
  return row.rejectedBy || row.approverName || "—";
}

function SortIcon({ active, descending }: { active: boolean; descending: boolean }) {
  if (!active) return <ArrowUpDown size={12} className="text-gray-300 group-hover:text-gray-400" />;
  return descending ? <ArrowDown size={12} className="text-primary-500" /> : <ArrowUp size={12} className="text-primary-500" />;
}

export default function EmployeeLeaveHistoryDrawer({ employeeId, onClose }: Props) {
  const { hasRole, hasClaim } = useAuthStore();
  const canExport = hasRole("Admin") || hasClaim("HR.LeaveRequests.Export");

  const [show, setShow] = useState(false);
  const [view, setView] = useState<"grid" | "card">("grid");
  const [statusFilter, setStatusFilter] = useState("");
  const [leaveTypeFilter, setLeaveTypeFilter] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  const dg = useDataGrid({ defaultSortBy: "startDate", defaultSortDescending: true });
  const { options: leaveTypeOptions, isLoading: leaveTypeLoading } = useLeaveTypeLookup();

  // Drive the slide-in/out animation off mount.
  useEffect(() => {
    if (employeeId) {
      const t = requestAnimationFrame(() => setShow(true));
      return () => cancelAnimationFrame(t);
    }
    setShow(false);
  }, [employeeId]);

  // Close on Escape.
  useEffect(() => {
    if (!employeeId) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [employeeId, onClose]);

  const { data: empData, isLoading: empLoading, isError: empError } = useQuery({
    queryKey: ["employee-detail", employeeId],
    queryFn: () => getEmployeeById(employeeId as string),
    enabled: !!employeeId,
  });
  const emp = empData?.data?.data;
  const fullName = emp ? [emp.firstName, emp.middleName, emp.lastName].filter(Boolean).join(" ") : "";

  const { data: reqData, isLoading: reqLoading, isError: reqError } = useQuery({
    queryKey: ["employee-leave-history", employeeId, ...dg.queryKey, statusFilter, leaveTypeFilter, fromDate, toDate],
    queryFn: () => getLeaveRequests(
      dg.queryParams,
      employeeId as string,
      statusFilter || undefined,
      leaveTypeFilter || undefined,
      fromDate || undefined,
      toDate || undefined,
    ),
    enabled: !!employeeId,
  });
  const rows = reqData?.data?.data?.items ?? [];
  const totalPages = reqData?.data?.data?.totalPages ?? 1;
  const totalCount = reqData?.data?.data?.totalCount ?? 0;

  const printColumns: PrintColumn<LeaveRequest>[] = [
    { header: "Leave Type", value: (r) => r.leaveTypeName },
    { header: "Start Date", value: (r) => fmtDate(r.startDate) },
    { header: "End Date", value: (r) => fmtDate(r.endDate) },
    { header: "Days", value: (r) => r.totalDays },
    { header: "Reason", value: (r) => r.reason || "" },
    { header: "Status", value: (r) => r.status },
    { header: "Applied", value: (r) => fmtDate(r.createdAt) },
    { header: "Approved / Rejected By", value: (r) => approverLabel(r) },
  ];

  const fetchForPrint = async () => {
    const res = await getLeaveRequests(
      {
        page: 1,
        pageSize: 100000,
        ...(dg.search ? { search: dg.search } : {}),
        ...(dg.sortBy ? { sortBy: dg.sortBy, sortDescending: dg.sortDescending } : {}),
      },
      employeeId as string,
      statusFilter || undefined,
      leaveTypeFilter || undefined,
      fromDate || undefined,
      toDate || undefined,
    );
    return res.data?.data?.items ?? [];
  };

  const exportFilters = {
    employeeId: employeeId || undefined,
    status: statusFilter || undefined,
    leaveTypeId: leaveTypeFilter || undefined,
    fromDate: fromDate || undefined,
    toDate: toDate || undefined,
    search: dg.search || undefined,
  };

  if (!employeeId) return null;

  const loading = reqLoading;
  const isEmpty = !loading && rows.length === 0;

  return (
    <div className="fixed inset-0 z-50">
      {/* Backdrop */}
      <div
        className={`absolute inset-0 bg-black/40 transition-opacity duration-300 ${show ? "opacity-100" : "opacity-0"}`}
        onClick={onClose}
      />

      {/* Panel */}
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Employee leave history"
        className={`absolute right-0 top-0 h-full w-full sm:max-w-2xl lg:max-w-3xl bg-gray-50 shadow-2xl flex flex-col transition-transform duration-300 ease-out ${show ? "translate-x-0" : "translate-x-full"}`}
      >
        {/* ── Sticky employee header ─────────────────────────────── */}
        <div className="shrink-0 bg-white border-b border-gray-200">
          <div className="relative">
            <div className="h-24 bg-gradient-to-r from-primary-600 via-primary-500 to-primary-600" />
            <button
              onClick={onClose}
              className="absolute top-4 right-4 inline-flex items-center justify-center h-8 w-8 rounded-full bg-white/15 text-white hover:bg-white/30 transition-colors"
              aria-label="Close"
              title="Close"
            >
              <X size={18} />
            </button>
          </div>

          <div className="px-6 pb-5 -mt-14">
            {/* The -mt-14 pull overlaps the avatar onto the banner; the text column is pushed back
                down past the banner (sm:pt-16) so the name never renders over the gradient. */}
            <div className="flex flex-col sm:flex-row sm:items-start gap-4 sm:gap-5">
              <div className="ring-4 ring-white rounded-full shadow-lg shrink-0 mx-auto sm:mx-0">
                <Avatar
                  src={emp?.profilePictureUrl}
                  firstName={emp?.firstName}
                  lastName={emp?.lastName}
                  size="xl"
                  className="!w-28 !h-28"
                />
              </div>
              <div className="min-w-0 flex-1 sm:pt-16 text-center sm:text-left">
                <div className="flex items-center justify-center sm:justify-start gap-2 flex-wrap">
                  <h2 className="text-xl font-bold leading-snug break-words">
                    <span className="box-decoration-clone rounded-md bg-primary-50 px-2 py-0.5 text-primary-700">
                      {empLoading ? "Loading…" : fullName || "Employee"}
                    </span>
                  </h2>
                  {emp?.status && <EmpStatusPill status={emp.status} />}
                </div>
                <p className="text-sm text-gray-500 mt-1 truncate">
                  {emp?.designationName || "—"}{emp?.departmentName ? ` · ${emp.departmentName}` : ""}
                </p>
                {emp?.employeeCode && (
                  <p className="text-xs text-gray-400 mt-0.5">Employee ID: {emp.employeeCode}</p>
                )}
              </div>
            </div>

            {emp && (
              <div className="mt-5 rounded-xl bg-gray-50 border border-gray-100 p-4 grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-4">
                <InfoRow icon={<Mail size={15} />} label="Email" value={emp.email} />
                <InfoRow icon={<Phone size={15} />} label="Mobile" value={emp.phone} />
                <InfoRow icon={<Briefcase size={15} />} label="Designation" value={emp.designationName} />
                <InfoRow icon={<Building2 size={15} />} label="Department" value={emp.departmentName} />
              </div>
            )}
            {empLoading && !emp && (
              <div className="mt-5 h-28 rounded-xl bg-gray-50 border border-gray-100 animate-pulse" />
            )}
            {empError && (
              <div className="mt-4 text-xs text-red-500">Could not load employee details.</div>
            )}
          </div>
        </div>

        {/* ── Controls bar ───────────────────────────────────────── */}
        <div className="shrink-0 bg-white border-b border-gray-100 px-6 py-3 space-y-3">
          <div className="flex items-center justify-between gap-3">
            <h3 className="text-sm font-medium text-gray-700">Leave History</h3>
            <div className="flex items-center gap-2">
              {/* View toggle */}
              <div className="inline-flex rounded-lg border border-gray-200 overflow-hidden">
                <button
                  onClick={() => setView("grid")}
                  className={`flex items-center gap-1.5 px-2.5 py-1.5 text-xs ${view === "grid" ? "bg-primary-500 text-white" : "bg-white text-gray-600 hover:bg-gray-50"}`}
                  title="Grid view"
                  aria-pressed={view === "grid"}
                >
                  <Table2 size={14} /> Grid
                </button>
                <button
                  onClick={() => setView("card")}
                  className={`flex items-center gap-1.5 px-2.5 py-1.5 text-xs ${view === "card" ? "bg-primary-500 text-white" : "bg-white text-gray-600 hover:bg-gray-50"}`}
                  title="Card view"
                  aria-pressed={view === "card"}
                >
                  <LayoutGrid size={14} /> Card
                </button>
              </div>
              <PrintButton
                title={`${fullName || "Employee"} — Leave History`}
                subtitle={emp?.employeeCode ? `Employee ID: ${emp.employeeCode}` : undefined}
                columns={printColumns}
                fetchRows={fetchForPrint}
              />
              {canExport && <ExportMenu baseUrl="leave/leave-requests" filters={exportFilters} />}
            </div>
          </div>

          {/* Search + filters */}
          <div className="flex flex-wrap items-center gap-2">
            <div className="relative flex-1 min-w-[180px] max-w-xs">
              <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search leave type or reason..."
                value={dg.search}
                onChange={(e) => dg.setSearch(e.target.value)}
                className="w-full text-sm border border-gray-200 rounded-lg pl-9 pr-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <SearchableDropdown
              options={leaveTypeOptions}
              value={leaveTypeFilter || undefined}
              onChange={(v) => { setLeaveTypeFilter(v ?? ""); dg.setPage(1); }}
              placeholder="All Leave Types"
              searchPlaceholder="Search leave types..."
              clearable
              loading={leaveTypeLoading}
              className="w-40"
            />
            <select
              value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); dg.setPage(1); }}
              className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              <option value="">All Status</option>
              {STATUS_OPTIONS.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
            <div className="flex items-center gap-1.5">
              <label className="text-xs text-gray-400">From</label>
              <input
                type="date"
                value={fromDate}
                max={toDate || undefined}
                onChange={(e) => { setFromDate(e.target.value); dg.setPage(1); }}
                className="text-sm border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
              <label className="text-xs text-gray-400">To</label>
              <input
                type="date"
                value={toDate}
                min={fromDate || undefined}
                onChange={(e) => { setToDate(e.target.value); dg.setPage(1); }}
                className="text-sm border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            {(fromDate || toDate || statusFilter || leaveTypeFilter || dg.search) && (
              <button
                onClick={() => { setFromDate(""); setToDate(""); setStatusFilter(""); setLeaveTypeFilter(""); dg.setSearch(""); dg.setPage(1); }}
                className="text-xs text-primary-600 hover:underline"
              >
                Clear
              </button>
            )}
          </div>
        </div>

        {/* ── Body (scrollable) ──────────────────────────────────── */}
        <div className="relative flex-1 overflow-y-auto px-6 py-4">
          {loading && (
            <div className="absolute inset-0 bg-gray-50/70 z-10 flex items-center justify-center">
              <div className="flex items-center gap-2 text-sm text-gray-400">
                <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
                Loading…
              </div>
            </div>
          )}

          {reqError ? (
            <div className="flex flex-col items-center justify-center py-16 text-sm text-red-500 gap-2">
              <AlertCircle size={28} className="text-red-300" />
              Could not load leave history. Please try again.
            </div>
          ) : isEmpty ? (
            <div className="flex flex-col items-center justify-center py-16 text-sm text-gray-400 gap-2">
              <CalendarDays size={32} className="text-gray-200" />
              No leave records found for the selected filters.
            </div>
          ) : view === "grid" ? (
            <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="bg-gray-50">
                      {HISTORY_COLUMNS.map((col) => (
                        <th key={col.key} className="text-left px-3 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200 whitespace-nowrap">
                          {col.sortable ? (
                            <button onClick={() => dg.handleSort(col.key)} className="group inline-flex items-center gap-1.5 hover:text-gray-700">
                              {col.label}
                              <SortIcon active={dg.sortBy === col.key} descending={dg.sortDescending} />
                            </button>
                          ) : col.label}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {rows.map((r) => (
                      <tr key={r.id} className="border-t border-gray-100 hover:bg-gray-50">
                        <td className="px-3 py-2.5 text-gray-800 whitespace-nowrap">{r.leaveTypeName}</td>
                        <td className="px-3 py-2.5 text-gray-700 whitespace-nowrap">{fmtDate(r.startDate)}</td>
                        <td className="px-3 py-2.5 text-gray-700 whitespace-nowrap">{fmtDate(r.endDate)}</td>
                        <td className="px-3 py-2.5 text-gray-700">{r.totalDays}</td>
                        <td className="px-3 py-2.5 text-gray-600 max-w-[200px] truncate" title={r.reason || ""}>{r.reason || "—"}</td>
                        <td className="px-3 py-2.5"><StatusBadge status={r.status} /></td>
                        <td className="px-3 py-2.5 text-gray-700 whitespace-nowrap">{fmtDate(r.createdAt)}</td>
                        <td className="px-3 py-2.5 text-gray-600 whitespace-nowrap">
                          {approverLabel(r)}
                          {r.status === "Rejected" && r.rejectionReason && (
                            <span className="block text-[11px] text-red-500 truncate max-w-[160px]" title={r.rejectionReason}>{r.rejectionReason}</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {rows.map((r) => (
                <div key={r.id} className="bg-white border border-gray-200 rounded-xl p-4 hover:shadow-sm transition-shadow">
                  <div className="flex items-start justify-between gap-2 mb-2">
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">{r.leaveTypeName}</p>
                      <p className="text-xs text-gray-400">Applied {fmtDate(r.createdAt)}</p>
                    </div>
                    <StatusBadge status={r.status} />
                  </div>
                  <div className="flex items-center gap-2 text-sm text-gray-700 mb-2">
                    <CalendarDays size={14} className="text-gray-400 shrink-0" />
                    <span>{fmtDate(r.startDate)} → {fmtDate(r.endDate)}</span>
                    <span className="ml-auto text-xs font-medium text-gray-500">{r.totalDays} day{r.totalDays === 1 ? "" : "s"}</span>
                  </div>
                  {r.reason && <p className="text-xs text-gray-600 mb-2 line-clamp-2">{r.reason}</p>}
                  <div className="flex items-center justify-between text-[11px] text-gray-400 pt-2 border-t border-gray-100">
                    <span>By: {approverLabel(r)}</span>
                    {r.status === "Rejected" && r.rejectionReason && (
                      <span className="text-red-500 truncate max-w-[160px]" title={r.rejectionReason}>{r.rejectionReason}</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* ── Footer: pagination ─────────────────────────────────── */}
        <div className="shrink-0 bg-white border-t border-gray-200 px-6 py-3 flex items-center justify-between">
          <div className="flex items-center gap-2 text-xs text-gray-400">
            <span>{totalCount} record{totalCount !== 1 ? "s" : ""}</span>
            <span className="text-gray-200">|</span>
            <span>Show</span>
            <select
              value={dg.pageSize}
              onChange={(e) => dg.setPageSize(Number(e.target.value))}
              className="border border-gray-200 rounded px-1.5 py-0.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              {[10, 25, 50, 100].map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          {totalPages > 1 && (
            <div className="flex items-center gap-2">
              <span className="text-xs text-gray-400">Page {dg.page} of {totalPages}</span>
              <button
                disabled={dg.page <= 1}
                onClick={() => dg.setPage(dg.page - 1)}
                className="p-1 border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50"
                aria-label="Previous page"
              >
                <ChevronLeft size={14} />
              </button>
              <button
                disabled={dg.page >= totalPages}
                onClick={() => dg.setPage(dg.page + 1)}
                className="p-1 border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50"
                aria-label="Next page"
              >
                <ChevronRight size={14} />
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function EmpStatusPill({ status }: { status: string }) {
  const cls =
    status === "Active" ? "bg-green-50 text-green-700 ring-green-200"
    : status === "OnLeave" ? "bg-yellow-50 text-yellow-700 ring-yellow-200"
    : "bg-gray-100 text-gray-600 ring-gray-200";
  return <span className={`text-xs px-2.5 py-0.5 rounded-full ring-1 ${cls}`}>{status}</span>;
}

function InfoRow({ icon, label, value }: { icon: React.ReactNode; label: string; value?: string }) {
  const empty = value === null || value === undefined || value === "";
  return (
    <div className="flex items-start gap-2.5 min-w-0">
      <span className="mt-0.5 text-primary-400 shrink-0">{icon}</span>
      <div className="min-w-0">
        <p className="text-[11px] uppercase tracking-wide text-gray-400">{label}</p>
        <p className={`text-sm truncate ${empty ? "text-gray-300 italic" : "text-gray-800"}`} title={empty ? undefined : value}>
          {empty ? "N/A" : value}
        </p>
      </div>
    </div>
  );
}
