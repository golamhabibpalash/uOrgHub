import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
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
  const [page, setPage] = useState(1);
  const [modal, setModal] = useState(false);
  const [modalType, setModalType] = useState<"schedule" | "shift" | "log">("schedule");
  const [form, setForm] = useState({ name: "", description: "" });
  const [shiftForm, setShiftForm] = useState({ name: "", startTime: "", endTime: "", workScheduleId: "" });
  const [logForm, setLogForm] = useState({ employeeId: "", attendanceDate: "", checkIn: "", checkOut: "", status: "Present", remarks: "" });
  const [editingLog, setEditingLog] = useState<AttendanceLog | null>(null);

  const { data: schedulesData, isLoading: schedulesLoading } = useQuery({
    queryKey: ["work-schedules", page],
    queryFn: () => getWorkSchedules({ page, pageSize: 10 }),
  });

  const { data: shiftsData, isLoading: shiftsLoading } = useQuery({
    queryKey: ["shifts", page],
    queryFn: () => getShifts({ page, pageSize: 10 }),
  });

  const { data: logsData, isLoading: logsLoading } = useQuery({
    queryKey: ["attendance-logs", page],
    queryFn: () => getAttendanceLogs({ page, pageSize: 10 }),
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

  const totalPages = activeTab === "schedules"
    ? schedulesData?.data?.data?.totalPages ?? 1
    : activeTab === "shifts"
    ? shiftsData?.data?.data?.totalPages ?? 1
    : logsData?.data?.data?.totalPages ?? 1;

  const createScheduleMutation = useMutation({
    mutationFn: () => createWorkSchedule(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["work-schedules"] }); setModal(false); },
  });

  const createShiftMutation = useMutation({
    mutationFn: () => createShift(shiftForm),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["shifts"] }); setModal(false); },
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
    if (type === "shift") setShiftForm({ name: "", startTime: "", endTime: "", workScheduleId: "" });
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
    { key: "isActive", label: "Status", render: (row: WorkSchedule) => <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{row.isActive ? "Active" : "Inactive"}</span> },
  ];

  const shiftCols = [
    { key: "name", label: "Shift Name" },
    { key: "startTime", label: "Start Time" },
    { key: "endTime", label: "End Time" },
    { key: "workScheduleName", label: "Schedule" },
    { key: "isActive", label: "Status", render: (row: Shift) => <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{row.isActive ? "Active" : "Inactive"}</span> },
  ];

  const logCols = [
    { key: "employeeName", label: "Employee" },
    { key: "attendanceDate", label: "Date", render: (row: AttendanceLog) => new Date(row.attendanceDate).toLocaleDateString() },
    { key: "checkIn", label: "Check In" },
    { key: "checkOut", label: "Check Out" },
    { key: "workHours", label: "Hours" },
    { key: "status", label: "Status", render: (row: AttendanceLog) => <span className={`text-xs px-2 py-0.5 rounded-full ${row.status === "Present" ? "bg-green-50 text-green-700" : row.status === "Absent" ? "bg-red-50 text-red-600" : "bg-yellow-50 text-yellow-700"}`}>{row.status}</span> },
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
          <button key={tab} onClick={() => { setActiveTab(tab); setPage(1); }} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {activeTab === "schedules" && <DataTable columns={scheduleCols} data={schedules} loading={schedulesLoading} />}
        {activeTab === "shifts" && <DataTable columns={shiftCols} data={shifts} loading={shiftsLoading} />}
        {activeTab === "logs" && <DataTable columns={logCols} data={logs} loading={logsLoading} onEdit={editLog} />}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

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
            <div><label className="text-xs text-gray-500 mb-1 block">Shift Name *</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.name} onChange={e => setShiftForm(f => ({ ...f, name: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Start Time</label><input type="time" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.startTime} onChange={e => setShiftForm(f => ({ ...f, startTime: e.target.value }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">End Time</label><input type="time" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.endTime} onChange={e => setShiftForm(f => ({ ...f, endTime: e.target.value }))} /></div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Schedule</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={shiftForm.workScheduleId} onChange={e => setShiftForm(f => ({ ...f, workScheduleId: e.target.value }))}><option value="">Select Schedule</option>{allSchedules.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}</select></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => createShiftMutation.mutate()} disabled={createShiftMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{createShiftMutation.isPending ? "Saving..." : "Save"}</button></div>
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