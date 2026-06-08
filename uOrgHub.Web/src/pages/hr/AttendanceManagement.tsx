import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Plus } from "lucide-react";
import toast from "react-hot-toast";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getWorkSchedules,
  createWorkSchedule,
  getShifts,
  createShift,
  getAttendanceLogs,
  createAttendanceLog,
  updateAttendanceLog,
  WorkSchedule,
  Shift,
  AttendanceLog,
  getEmployees,
} from "../../api/hr";

export default function AttendanceManagement() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<"schedules" | "shifts" | "logs">("schedules");
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [modalType, setModalType] = useState<"schedule" | "shift" | "log">("schedule");
  const [form, setForm] = useState({ name: "", description: "" });
  const [shiftForm, setShiftForm] = useState({ name: "", code: "", startTime: "", endTime: "", workScheduleId: "" });
  const [logForm, setLogForm] = useState({ employeeId: "", attendanceDate: "", checkIn: "", checkOut: "", status: "Present", remarks: "" });
  const [editingLog, setEditingLog] = useState<AttendanceLog | null>(null);

  const { data: schedulesData, isLoading: schedulesLoading } = useQuery({
    queryKey: ["work-schedules", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getWorkSchedules(dg.queryParams),
  });

  const { data: shiftsData, isLoading: shiftsLoading } = useQuery({
    queryKey: ["shifts", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getShifts(dg.queryParams),
  });

  const { data: logsData, isLoading: logsLoading } = useQuery({
    queryKey: ["attendance-logs", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getAttendanceLogs(dg.queryParams),
  });

  const { data: empData } = useQuery({
    queryKey: ["employees-all"],
    queryFn: () => getEmployees({ page: 1, pageSize: 100 }),
  });

  const { data: scheduleData } = useQuery({
    queryKey: ["work-schedules-all"],
    queryFn: () => getWorkSchedules({ page: 1, pageSize: 100 }),
  });

  const schedules = schedulesData?.data?.data?.items ?? [];
  const shifts = shiftsData?.data?.data?.items ?? [];
  const logs = logsData?.data?.data?.items ?? [];
  const employees = empData?.data?.data?.items ?? [];
  const allSchedules = scheduleData?.data?.data?.items ?? [];

  function extractApiError(err: unknown): string {
    const axiosErr = err as AxiosError<{ message?: string; errors?: string[] | Record<string, string[]> }>;
    const body = axiosErr.response?.data;
    if (typeof body?.message === "string") return body.message;
    if (body?.errors) {
      if (Array.isArray(body.errors)) return body.errors[0] ?? "";
      const first = Object.values(body.errors).flat()[0];
      if (first) return first;
    }
    return (err as Error)?.message ?? "An error occurred";
  }

  const createScheduleMutation = useMutation({
    mutationFn: () => createWorkSchedule(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["work-schedules"] }); setModal(false); },
  });

  const createShiftMutation = useMutation({
    mutationFn: () => createShift({
      ...shiftForm,
      startTime: `${shiftForm.startTime}:00`,
      endTime: `${shiftForm.endTime}:00`,
    }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["shifts"] }); setModal(false); },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const saveLogMutation = useMutation({
    mutationFn: () => editingLog
      ? updateAttendanceLog(editingLog.id, logForm)
      : createAttendanceLog(logForm),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["attendance-logs"] }); setModal(false); setEditingLog(null); },
  });

  function openModal(type: "schedule" | "shift" | "log") {
    setModalType(type);
    setModal(true);
    if (type === "schedule") setForm({ name: "", description: "" });
    if (type === "shift") setShiftForm({ name: "", code: "", startTime: "", endTime: "", workScheduleId: "" });
    if (type === "log") { setLogForm({ employeeId: "", attendanceDate: "", checkIn: "", checkOut: "", status: "Present", remarks: "" }); setEditingLog(null); }
  }

  function editLog(log: AttendanceLog) {
    setEditingLog(log);
    setLogForm({
      employeeId: log.employeeId,
      attendanceDate: log.attendanceDate.split("T")[0],
      checkIn: log.checkIn,
      checkOut: log.checkOut,
      status: log.status,
      remarks: log.remarks || "",
    });
    setModalType("log");
    setModal(true);
  }

  const scheduleCols = [
    { key: "name", label: "Schedule Name" },
    { key: "description", label: "Description" },
    { key: "isActive", label: "Status", sortable: false, render: (row: WorkSchedule) => <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{row.isActive ? "Active" : "Inactive"}</span> },
  ];

  const shiftCols = [
    { key: "name", label: "Shift Name" },
    { key: "startTime", label: "Start Time" },
    { key: "endTime", label: "End Time" },
    { key: "workScheduleName", label: "Schedule" },
    { key: "isActive", label: "Status", sortable: false, render: (row: Shift) => <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{row.isActive ? "Active" : "Inactive"}</span> },
  ];

  const logCols = [
    { key: "employeeName", label: "Employee" },
    { key: "attendanceDate", label: "Date", sortable: false, render: (row: AttendanceLog) => new Date(row.attendanceDate).toLocaleDateString() },
    { key: "checkIn", label: "Check In" },
    { key: "checkOut", label: "Check Out" },
    { key: "workHours", label: "Hours" },
    { key: "status", label: "Status", sortable: false, render: (row: AttendanceLog) => <span className={`text-xs px-2 py-0.5 rounded-full ${row.status === "Present" ? "bg-green-50 text-green-700" : row.status === "Absent" ? "bg-red-50 text-red-600" : "bg-yellow-50 text-yellow-700"}`}>{row.status}</span> },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Attendance Management</h2>
          <p className="text-xs text-gray-400">Manage shifts, schedules and attendance</p>
        </div>
        <button onClick={() => openModal(activeTab === "schedules" ? "schedule" : activeTab === "shifts" ? "shift" : "log")} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add {activeTab === "schedules" ? "Schedule" : activeTab === "shifts" ? "Shift" : "Log"}
        </button>
      </div>

      <div className="flex gap-4 mb-4">
        {(["schedules", "shifts", "logs"] as const).map(tab => (
          <button key={tab} onClick={() => { setActiveTab(tab); dg.setPage(1); }} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {activeTab === "schedules" && (
        <DataGrid
          columns={scheduleCols}
          data={schedules}
          loading={schedulesLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search schedules..."
          page={dg.page}
          totalPages={schedulesData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={schedulesData?.data?.data?.totalCount ?? 0}
          emptyMessage="No schedules found"
          actions={<ExportMenu baseUrl="attendance/work-schedules" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "shifts" && (
        <DataGrid
          columns={shiftCols}
          data={shifts}
          loading={shiftsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search shifts..."
          page={dg.page}
          totalPages={shiftsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={shiftsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No shifts found"
          actions={<ExportMenu baseUrl="attendance/shifts" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "logs" && (
        <DataGrid
          columns={logCols}
          data={logs}
          loading={logsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search logs..."
          page={dg.page}
          totalPages={logsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={logsData?.data?.data?.totalCount ?? 0}
          onEdit={editLog}
          emptyMessage="No attendance logs found"
          actions={<ExportMenu baseUrl="attendance/logs" filters={{ search: dg.search || undefined }} />}
        />
      )}

      <Modal title={modalType === "schedule" ? "Add Schedule" : modalType === "shift" ? "Add Shift" : editingLog ? "Edit Log" : "Add Log"} open={modal} onClose={() => setModal(false)}>
        {modalType === "schedule" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Name *</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => createScheduleMutation.mutate()} disabled={createScheduleMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{createScheduleMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {modalType === "shift" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Shift Name *</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.name} onChange={e => setShiftForm(f => ({ ...f, name: e.target.value }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Code *</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.code} onChange={e => setShiftForm(f => ({ ...f, code: e.target.value }))} /></div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Start Time *</label><input type="time" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.startTime} onChange={e => setShiftForm(f => ({ ...f, startTime: e.target.value }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">End Time *</label><input type="time" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.endTime} onChange={e => setShiftForm(f => ({ ...f, endTime: e.target.value }))} /></div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Work Schedule *</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.workScheduleId} onChange={e => setShiftForm(f => ({ ...f, workScheduleId: e.target.value }))}><option value="">Select Schedule</option>{allSchedules.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}</select></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => {
              if (!shiftForm.name.trim() || !shiftForm.code.trim() || !shiftForm.startTime || !shiftForm.endTime || !shiftForm.workScheduleId) {
                toast.error("Please fill in all required fields.");
                return;
              }
              createShiftMutation.mutate();
            }} disabled={createShiftMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{createShiftMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {modalType === "log" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Employee</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={logForm.employeeId} onChange={e => setLogForm(f => ({ ...f, employeeId: e.target.value }))}><option value="">Select Employee</option>{employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}</select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={logForm.attendanceDate} onChange={e => setLogForm(f => ({ ...f, attendanceDate: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Check In</label><input type="time" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={logForm.checkIn} onChange={e => setLogForm(f => ({ ...f, checkIn: e.target.value }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Check Out</label><input type="time" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={logForm.checkOut} onChange={e => setLogForm(f => ({ ...f, checkOut: e.target.value }))} /></div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Status</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={logForm.status} onChange={e => setLogForm(f => ({ ...f, status: e.target.value }))}><option value="Present">Present</option><option value="Absent">Absent</option><option value="Late">Late</option><option value="OnLeave">On Leave</option></select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Notes</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={logForm.remarks} onChange={e => setLogForm(f => ({ ...f, remarks: e.target.value }))} /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => saveLogMutation.mutate()} disabled={saveLogMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{saveLogMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
      </Modal>
    </div>
  );
}