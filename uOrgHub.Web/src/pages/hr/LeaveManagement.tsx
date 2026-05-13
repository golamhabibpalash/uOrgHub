import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Check, X } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getLeaveTypes,
  createLeaveType,
  updateLeaveType,
  getLeaveRequests,
  createLeaveRequest,
  approveLeaveRequest,
  LeaveType,
  LeaveRequest,
  getEmployees,
} from "../../api/hr";

export default function LeaveManagement() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<"types" | "requests">("types");
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<LeaveType | null>(null);
  const [form, setForm] = useState({ name: "", code: "", description: "", maxDaysPerYear: 0, isPaid: true });

  const [reqModal, setReqModal] = useState(false);
  const [reqForm, setReqForm] = useState({
    leaveTypeId: "",
    startDate: "",
    endDate: "",
    reason: "",
    employeeId: "",
  });

  const { data: typesData, isLoading: typesLoading } = useQuery({
    queryKey: ["leave-types", page],
    queryFn: () => getLeaveTypes({ page, pageSize: 10 }),
  });

  const { data: requestsData, isLoading: requestsLoading } = useQuery({
    queryKey: ["leave-requests", page, statusFilter],
    queryFn: () => getLeaveRequests({ page, pageSize: 10 }, undefined, statusFilter || undefined),
  });

  const { data: empData } = useQuery({
    queryKey: ["employees-all"],
    queryFn: () => getEmployees({ page: 1, pageSize: 100 }),
  });

  const leaveTypes = typesData?.data?.data?.items ?? [];
  const leaveRequests = requestsData?.data?.data?.items ?? [];
  const totalPages = activeTab === "types"
    ? typesData?.data?.data?.totalPages ?? 1
    : requestsData?.data?.data?.totalPages ?? 1;
  const employees = empData?.data?.data?.items ?? [];

  const saveTypeMutation = useMutation({
    mutationFn: () => editing
      ? updateLeaveType(editing.id, form)
      : createLeaveType(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leave-types"] });
      setModal(false);
    },
  });

  const saveReqMutation = useMutation({
    mutationFn: () => createLeaveRequest(reqForm),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leave-requests"] });
      setReqModal(false);
    },
  });

  const approveMutation = useMutation({
    mutationFn: ({ id, approved }: { id: string; approved: boolean }) =>
      approveLeaveRequest(id, { isApproved: approved, remarks: "" }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["leave-requests"] }),
  });

  const typeColumns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Leave Type" },
    { key: "description", label: "Description" },
    { key: "maxDaysPerYear", label: "Max Days/Year" },
    {
      key: "isPaid",
      label: "Paid",
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
    { key: "startDate", label: "Start Date", render: (row: LeaveRequest) => new Date(row.startDate).toLocaleDateString() },
    { key: "endDate", label: "End Date", render: (row: LeaveRequest) => new Date(row.endDate).toLocaleDateString() },
    { key: "totalDays", label: "Days" },
    {
      key: "status",
      label: "Status",
      render: (row: LeaveRequest) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${
          row.status === "Approved" ? "bg-green-50 text-green-700" :
          row.status === "Pending" ? "bg-yellow-50 text-yellow-700" :
          "bg-red-50 text-red-600"
        }`}>
          {row.status}
        </span>
      ),
    },
    {
      key: "actions",
      label: "Actions",
      render: (row: LeaveRequest) => row.status === "Pending" ? (
        <div className="flex gap-2">
          <button onClick={() => approveMutation.mutate({ id: row.id, approved: true })} className="text-green-600 hover:text-green-800">
            <Check size={16} />
          </button>
          <button onClick={() => approveMutation.mutate({ id: row.id, approved: false })} className="text-red-600 hover:text-red-800">
            <X size={16} />
          </button>
        </div>
      ) : null,
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Leave Management</h2>
          <p className="text-xs text-gray-400">Manage leave types and requests</p>
        </div>
        {activeTab === "types" && (
          <button
            onClick={() => { setEditing(null); setForm({ name: "", code: "", description: "", maxDaysPerYear: 0, isPaid: true }); setModal(true); }}
            className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
          >
            <Plus size={15} /> Add Leave Type
          </button>
        )}
        {activeTab === "requests" && (
          <button
            onClick={() => { setReqForm({ leaveTypeId: "", startDate: "", endDate: "", reason: "", employeeId: "" }); setReqModal(true); }}
            className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
          >
            <Plus size={15} /> New Request
          </button>
        )}
      </div>

      <div className="flex gap-4 mb-4">
        <button
          onClick={() => setActiveTab("types")}
          className={`px-4 py-2 rounded text-sm ${activeTab === "types" ? "bg-primary-500 text-white" : "bg-gray-200"}`}
        >
          Leave Types
        </button>
        <button
          onClick={() => setActiveTab("requests")}
          className={`px-4 py-2 rounded text-sm ${activeTab === "requests" ? "bg-primary-500 text-white" : "bg-gray-200"}`}
        >
          Leave Requests
        </button>
      </div>

      {activeTab === "requests" && (
        <div className="mb-4 flex gap-3">
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All Status</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
        </div>
      )}

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {activeTab === "types" ? (
          <>
            <DataTable columns={typeColumns} data={leaveTypes} loading={typesLoading} onEdit={(row) => { setEditing(row); setForm({ name: row.name, code: row.code, description: row.description, maxDaysPerYear: row.maxDaysPerYear, isPaid: row.isPaid }); setModal(true); }} />
            <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
          </>
        ) : (
          <>
            <DataTable columns={requestColumns} data={leaveRequests} loading={requestsLoading} />
            <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
          </>
        )}
      </div>

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
              <input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.maxDaysPerYear} onChange={(e) => setForm(f => ({ ...f, maxDaysPerYear: Number(e.target.value) }))} />
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

      <Modal title="New Leave Request" open={reqModal} onClose={() => setReqModal(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Employee</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.employeeId} onChange={(e) => setReqForm(f => ({ ...f, employeeId: e.target.value }))}>
              <option value="">Select Employee</option>
              {employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Leave Type</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.leaveTypeId} onChange={(e) => setReqForm(f => ({ ...f, leaveTypeId: e.target.value }))}>
              <option value="">Select Leave Type</option>
              {leaveTypes.map(l => <option key={l.id} value={l.id}>{l.name}</option>)}
            </select>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Start Date</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.startDate} onChange={(e) => setReqForm(f => ({ ...f, startDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">End Date</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.endDate} onChange={(e) => setReqForm(f => ({ ...f, endDate: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Reason</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={reqForm.reason} onChange={(e) => setReqForm(f => ({ ...f, reason: e.target.value }))} />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setReqModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveReqMutation.mutate()} disabled={saveReqMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveReqMutation.isPending ? "Submitting..." : "Submit"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}