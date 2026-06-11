import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Check, X, Pencil, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useEmployeeLookup, useLeaveTypeLookup } from "../../hooks/useEntityLookup";
import { useAuthStore } from "../../store/authStore";
import {
  getLeaveTypes,
  createLeaveType,
  updateLeaveType,
  getLeaveRequests,
  createLeaveRequest,
  updateLeaveRequest,
  deleteLeaveRequest,
  approveLeaveRequest,
  cancelLeaveRequest,
  LeaveType,
  LeaveRequest,
} from "../../api/hr";

export default function LeaveManagement() {
  const qc = useQueryClient();
  const { hasClaim, hasRole, user } = useAuthStore();
  const isAdmin = hasRole("Admin");
  const canViewLeaveTypes = isAdmin || hasClaim("HR.LeaveTypes.View");
  const canCreateLeaveType = isAdmin || hasClaim("HR.LeaveTypes.Create");
  const canEditLeaveType = isAdmin || hasClaim("HR.LeaveTypes.Edit");
  const canExportLeaveTypes = isAdmin || hasClaim("HR.LeaveTypes.Export");

  const canViewRequests = isAdmin || hasClaim("HR.LeaveRequests.View") || hasClaim("Self.ViewLeave");
  const canCreateRequest = isAdmin || hasClaim("HR.LeaveRequests.Create") || hasClaim("Self.SubmitLeave");
  const canApproveRequest = isAdmin || hasClaim("HR.LeaveRequests.Approve");
  const canEditAnyRequest = isAdmin || hasClaim("HR.LeaveRequests.Edit");
  const canDeleteAnyRequest = isAdmin || hasClaim("HR.LeaveRequests.Delete");
  const canCancelAnyRequest = isAdmin || hasClaim("HR.LeaveRequests.Edit");
  const canExportRequests = isAdmin || hasClaim("HR.LeaveRequests.Export");

  const isHrAdmin = isAdmin || hasClaim("HR.LeaveRequests.View");
  const userEmployeeId = user?.employeeId;
  const canSelfService = isAdmin || hasClaim("Self.SubmitLeave");

  const [activeTab, setActiveTab] = useState<"types" | "requests">(
    canViewLeaveTypes ? "types" : "requests"
  );
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [statusFilter, setStatusFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<LeaveType | null>(null);
  const [form, setForm] = useState({ name: "", code: "", description: "", totalDaysPerYear: 0, isPaid: true });

  const [reqModal, setReqModal] = useState(false);
  const [editingReq, setEditingReq] = useState<LeaveRequest | null>(null);
  const [rejectModal, setRejectModal] = useState<{ id: string } | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [rejectSubmitted, setRejectSubmitted] = useState(false);
  const [reqForm, setReqForm] = useState({
    leaveTypeId: "",
    startDate: "",
    endDate: "",
    reason: "",
    employeeId: "",
  });
  const [reqFormError, setReqFormError] = useState("");

  const reqFormValidation = useMemo(() => {
    const s = reqForm.startDate;
    const e = reqForm.endDate;
    if (s && e) {
      const start = new Date(s);
      const end = new Date(e);
      if (start > end) {
        return { valid: false, error: "Start date cannot be greater than end date.", totalDays: null };
      }
      const diff = Math.round((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1;
      return { valid: true, error: "", totalDays: diff };
    }
    return { valid: true, error: "", totalDays: null };
  }, [reqForm.startDate, reqForm.endDate]);

  const { data: typesData, isLoading: typesLoading } = useQuery({
    queryKey: ["leave-types", ...dg.queryKey],
    queryFn: () => getLeaveTypes(dg.queryParams),
    enabled: canViewLeaveTypes,
  });

  const { options: leaveTypeOptions, isLoading: leaveTypeLoading } = useLeaveTypeLookup();
  const { options: empOptions, isLoading: empLoading } = useEmployeeLookup();

  const { data: requestsData, isLoading: requestsLoading } = useQuery({
    queryKey: ["leave-requests", ...dg.queryKey, statusFilter],
    queryFn: () => getLeaveRequests(
      dg.queryParams,
      isHrAdmin ? undefined : userEmployeeId,
      statusFilter || undefined
    ),
    enabled: canViewRequests,
  });

  const leaveTypes = typesData?.data?.data?.items ?? [];
  const leaveRequests = requestsData?.data?.data?.items ?? [];

  const saveTypeMutation = useMutation({
    mutationFn: () => editing
      ? updateLeaveType(editing.id, form)
      : createLeaveType(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leave-types"] });
      setModal(false);
      toast.success(editing ? "Leave type updated." : "Leave type created.");
    },
    onError: () => toast.error("Failed to save leave type."),
  });

  const saveReqMutation = useMutation({
    mutationFn: () => editingReq
      ? updateLeaveRequest(editingReq.id, {
          leaveTypeId: reqForm.leaveTypeId,
          startDate: reqForm.startDate,
          endDate: reqForm.endDate,
          reason: reqForm.reason,
        })
      : createLeaveRequest({
          ...reqForm,
          employeeId: reqForm.employeeId || userEmployeeId || "",
        }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leave-requests"] });
      setReqModal(false);
      setEditingReq(null);
      toast.success(editingReq ? "Leave request updated." : "Leave request submitted.");
    },
    onError: () => toast.error(editingReq ? "Failed to update leave request." : "Failed to submit leave request."),
  });

  const approveMutation = useMutation({
    mutationFn: ({ id, approved, reason }: { id: string; approved: boolean; reason?: string }) =>
      approveLeaveRequest(id, { isApproved: approved, remarks: "", rejectReason: reason }),
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ["leave-requests"] });
      setRejectModal(null);
      setRejectReason("");
      setRejectSubmitted(false);
      toast.success(vars.approved ? "Leave request approved." : "Leave request rejected.");
    },
    onError: () => toast.error("Failed to process leave request."),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => cancelLeaveRequest(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leave-requests"] });
      toast.success("Leave request cancelled.");
    },
    onError: () => toast.error("Failed to cancel leave request."),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteLeaveRequest(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leave-requests"] });
      toast.success("Leave request deleted.");
    },
    onError: () => toast.error("Failed to delete leave request."),
  });

  const canEditRow = (row: LeaveRequest) => {
    if (row.status !== "Pending") return false;
    return canEditAnyRequest || (canSelfService && userEmployeeId === row.employeeId);
  };

  const canDeleteRow = (row: LeaveRequest) => {
    if (row.status !== "Pending") return false;
    return canDeleteAnyRequest || (canSelfService && userEmployeeId === row.employeeId);
  };

  const canCancelRow = (row: LeaveRequest) => {
    if (row.status !== "Pending") return false;
    return canCancelAnyRequest || (canSelfService && userEmployeeId === row.employeeId);
  };

  const typeColumns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Leave Type" },
    { key: "description", label: "Description" },
    { key: "totalDaysPerYear", label: "Max Days/Year" },
    {
      key: "isPaid",
      label: "Paid",
      sortable: false,
      render: (row: LeaveType) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isPaid ? "bg-green-50 text-green-700" : "bg-gray-50 text-gray-600"}`}>
          {row.isPaid ? "Yes" : "No"}
        </span>
      ),
    },
  ];

  const requestColumns = [
    { key: "employeeName", label: "Employee" },
    { key: "leaveTypeName", label: "Leave Type" },
    { key: "startDate", label: "Start Date", sortable: false, render: (row: LeaveRequest) => new Date(row.startDate).toLocaleDateString() },
    { key: "endDate", label: "End Date", sortable: false, render: (row: LeaveRequest) => new Date(row.endDate).toLocaleDateString() },
    { key: "totalDays", label: "Days" },
    {
      key: "status",
      label: "Status",
      sortable: false,
      render: (row: LeaveRequest) => (
        <div>
          <span className={`text-xs px-2 py-0.5 rounded-full ${
            row.status === "Approved" ? "bg-green-50 text-green-700" :
            row.status === "Pending" ? "bg-yellow-50 text-yellow-700" :
            row.status === "Cancelled" ? "bg-gray-50 text-gray-600" :
            "bg-red-50 text-red-600"
          }`}>
            {row.status}
          </span>
          {row.status === "Rejected" && row.rejectionReason && (
            <div className="mt-1 group relative">
              <span className="text-xs text-red-500 cursor-pointer underline decoration-dotted">Reason</span>
              <div className="absolute left-0 top-full z-10 mt-1 w-72 bg-white border border-gray-200 rounded-lg shadow-lg p-3 text-xs text-gray-700 hidden group-hover:block whitespace-normal">
                <p className="font-medium text-red-600 mb-1">Rejection Reason</p>
                <p>{row.rejectionReason}</p>
                {row.rejectedBy && <p className="mt-1 text-gray-400">by {row.rejectedBy}</p>}
                {row.rejectedAt && <p className="text-gray-400">{new Date(row.rejectedAt).toLocaleString()}</p>}
              </div>
            </div>
          )}
        </div>
      ),
    },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row: LeaveRequest) => (
        <div className="flex gap-2 items-center">
          {canApproveRequest && row.status === "Pending" && (
            <>
              <button
                onClick={() => approveMutation.mutate({ id: row.id, approved: true })}
                className="text-green-600 hover:text-green-800"
                title="Approve"
              >
                <Check size={16} />
              </button>
              <button
                onClick={() => { setRejectModal({ id: row.id }); setRejectReason(""); }}
                className="text-red-600 hover:text-red-800"
                title="Reject"
              >
                <X size={16} />
              </button>
            </>
          )}
          {canEditRow(row) && (
            <button
              onClick={() => {
                setEditingReq(row);
                setReqForm({
                  leaveTypeId: row.leaveTypeId,
                  startDate: row.startDate.split("T")[0],
                  endDate: row.endDate.split("T")[0],
                  reason: row.reason || "",
                  employeeId: row.employeeId,
                });
                setReqFormError("");
                setReqModal(true);
              }}
              className="text-blue-600 hover:text-blue-800"
              title="Edit"
            >
              <Pencil size={16} />
            </button>
          )}
          {canDeleteRow(row) && (
            <button
              onClick={() => {
                if (confirm("Delete this leave request permanently?")) deleteMutation.mutate(row.id);
              }}
              className="text-red-600 hover:text-red-800"
              title="Delete"
            >
              <Trash2 size={16} />
            </button>
          )}
          {canCancelRow(row) && !canApproveRequest && (
            <button
              onClick={() => { if (confirm("Cancel this leave request?")) cancelMutation.mutate(row.id); }}
              className="text-gray-500 hover:text-gray-700 text-xs underline"
              title="Cancel"
            >
              Cancel
            </button>
          )}
        </div>
      ),
    },
  ];

  const tabs = ([
    { key: "types" as const, label: "Leave Types", visible: canViewLeaveTypes },
    { key: "requests" as const, label: "Leave Requests", visible: canViewRequests },
  ] as const).filter(t => t.visible);

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Leave Management</h2>
          <p className="text-xs text-gray-400">Manage leave types and requests</p>
        </div>
        <div className="flex gap-2">
          {activeTab === "types" && canCreateLeaveType && (
            <button
              onClick={() => { setEditing(null); setForm({ name: "", code: "", description: "", totalDaysPerYear: 0, isPaid: true }); setModal(true); }}
              className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
            >
              <Plus size={15} /> Add Leave Type
            </button>
          )}
          {activeTab === "requests" && canCreateRequest && (
            <button
              onClick={() => {
                setEditingReq(null);
                setReqForm({
                  leaveTypeId: "",
                  startDate: "",
                  endDate: "",
                  reason: "",
                  employeeId: isHrAdmin ? "" : userEmployeeId || "",
                });
                setReqFormError("");
                setReqModal(true);
              }}
              className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
            >
              <Plus size={15} /> New Request
            </button>
          )}
        </div>
      </div>

      {tabs.length > 1 && (
        <div className="flex gap-4 mb-4">
          {tabs.map(tab => (
            <button
              key={tab.key}
              onClick={() => { setActiveTab(tab.key); dg.setPage(1); }}
              className={`px-4 py-2 rounded text-sm ${activeTab === tab.key ? "bg-primary-500 text-white" : "bg-gray-200"}`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      )}

      {activeTab === "types" && canViewLeaveTypes && (
        <DataGrid
          columns={typeColumns}
          data={leaveTypes}
          loading={typesLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search leave types..."
          page={dg.page}
          totalPages={typesData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={typesData?.data?.data?.totalCount ?? 0}
          onEdit={canEditLeaveType ? (row) => { setEditing(row); setForm({ name: row.name, code: row.code, description: row.description, totalDaysPerYear: row.totalDaysPerYear, isPaid: row.isPaid }); setModal(true); } : undefined}
          emptyMessage="No leave types found"
          actions={canExportLeaveTypes ? <ExportMenu baseUrl="leave/types" filters={{ search: dg.search || undefined }} /> : undefined}
        />
      )}
      {activeTab === "requests" && canViewRequests && (
        <DataGrid
          columns={requestColumns}
          data={leaveRequests}
          loading={requestsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search requests..."
          page={dg.page}
          totalPages={requestsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={requestsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No leave requests found"
          toolbarPrefix={
            isHrAdmin ? (
              <select
                value={statusFilter}
                onChange={(e) => { setStatusFilter(e.target.value); dg.setPage(1); }}
                className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
              >
                <option value="">All Status</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
            ) : undefined
          }
          actions={canExportRequests ? <ExportMenu baseUrl="leave/requests" filters={{ search: dg.search || undefined, status: statusFilter || undefined }} /> : undefined}
        />
      )}

      {canCreateLeaveType && (
        <Modal title={editing ? "Edit Leave Type" : "Add Leave Type"} open={modal} onClose={() => setModal(false)}>
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Name *</label>
                <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Code *</label>
                <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.code} onChange={(e) => setForm(f => ({ ...f, code: e.target.value }))} />
              </div>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Description</label>
              <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))} />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Max Days/Year</label>
                <input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.totalDaysPerYear} onChange={(e) => setForm(f => ({ ...f, totalDaysPerYear: Number(e.target.value) }))} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Is Paid</label>
                <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={String(form.isPaid)} onChange={(e) => setForm(f => ({ ...f, isPaid: e.target.value === "true" }))}>
                  <option value="true">Yes</option>
                  <option value="false">No</option>
                </select>
              </div>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
              <button onClick={() => saveTypeMutation.mutate()} disabled={saveTypeMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
                {saveTypeMutation.isPending ? "Saving..." : "Save"}
              </button>
            </div>
          </div>
        </Modal>
      )}

      {(canCreateRequest || canEditAnyRequest || canSelfService) && (
        <Modal title={editingReq ? "Edit Leave Request" : "New Leave Request"} open={reqModal} onClose={() => { setReqModal(false); setEditingReq(null); }}>
          <div className="space-y-3">
            {editingReq ? (
              <div className="text-xs text-gray-500 bg-gray-50 border border-gray-200 rounded-lg px-3 py-2">
                Editing leave request for <strong>{editingReq.employeeName}</strong>
              </div>
            ) : isHrAdmin ? (
              <div>
                <SearchableDropdown
                  label="Employee"
                  options={empOptions}
                  value={reqForm.employeeId}
                  onChange={(v) => setReqForm(f => ({ ...f, employeeId: v || "" }))}
                  placeholder="Select Employee"
                  searchPlaceholder="Search employee..."
                  loading={empLoading}
                  required
                />
              </div>
            ) : (
              <div className="text-xs text-gray-500 bg-gray-50 border border-gray-200 rounded-lg px-3 py-2">
                Submitting leave request for yourself
              </div>
            )}
            <div>
              <SearchableDropdown
                label="Leave Type"
                options={leaveTypeOptions}
                value={reqForm.leaveTypeId}
                onChange={(v) => setReqForm(f => ({ ...f, leaveTypeId: v || "" }))}
                placeholder="Select Leave Type"
                searchPlaceholder="Search leave types..."
                loading={leaveTypeLoading}
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Start Date</label>
                <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.startDate} onChange={(e) => { setReqForm(f => ({ ...f, startDate: e.target.value })); setReqFormError(""); }} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">End Date</label>
                <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.endDate} onChange={(e) => { setReqForm(f => ({ ...f, endDate: e.target.value })); setReqFormError(""); }} />
              </div>
            </div>
            {reqFormValidation.totalDays !== null && reqFormValidation.valid && (
              <div className="text-xs text-gray-500">
                Total days: <strong>{reqFormValidation.totalDays}</strong>
              </div>
            )}
            {reqFormValidation.error && (
              <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
                {reqFormValidation.error}
              </div>
            )}
            {reqFormError && (
              <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
                {reqFormError}
              </div>
            )}
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Reason</label>
              <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.reason} onChange={(e) => setReqForm(f => ({ ...f, reason: e.target.value }))} />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button onClick={() => { setReqModal(false); setEditingReq(null); }} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
              <button
                onClick={() => {
                  if (!reqFormValidation.valid) { setReqFormError(reqFormValidation.error); return; }
                  saveReqMutation.mutate();
                }}
                disabled={saveReqMutation.isPending}
                className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
              >
                {saveReqMutation.isPending ? "Saving..." : "Save"}
              </button>
            </div>
          </div>
        </Modal>
      )}

      <Modal title="Reject Leave Request" open={rejectModal !== null} onClose={() => { setRejectModal(null); setRejectReason(""); setRejectSubmitted(false); }}>
        <div className="space-y-3">
          <p className="text-sm text-gray-600">Please provide a reason for rejecting this leave request.</p>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Reject Reason *</label>
            <textarea
              rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={rejectReason}
              onChange={(e) => { setRejectReason(e.target.value); setRejectSubmitted(false); }}
              placeholder="Enter reason for rejection"
            />
          </div>
          {rejectSubmitted && !rejectReason.trim() && (
            <p className="text-xs text-red-600">Reject reason is required.</p>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={() => { setRejectModal(null); setRejectReason(""); setRejectSubmitted(false); }}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => {
                setRejectSubmitted(true);
                if (!rejectReason.trim()) return;
                approveMutation.mutate({ id: rejectModal!.id, approved: false, reason: rejectReason.trim() });
              }}
              disabled={approveMutation.isPending}
              className="px-4 py-2 text-sm bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
            >
              {approveMutation.isPending ? "Rejecting..." : "Confirm Reject"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
