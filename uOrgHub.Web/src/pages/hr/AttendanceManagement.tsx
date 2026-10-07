import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import toast from "react-hot-toast";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useEmployeeLookup } from "../../hooks/useEntityLookup";
import {
  getWorkSchedules,
  createWorkSchedule,
  updateWorkSchedule,
  getShifts,
  createShift,
  updateShift,
  getRosters,
  createRoster,
  updateRoster,
  getAttendanceLogs,
  createAttendanceLog,
  updateAttendanceLog,
  ATTENDANCE_STATUSES,
  WorkSchedule,
  Shift,
  EmployeeRoster,
  AttendanceLog,
} from "../../api/hr";
import DateInput from "../../components/shared/DateInput";

type Tab = "schedules" | "shifts" | "rosters" | "logs";
type ModalType = "schedule" | "shift" | "roster" | "log";

const TAB_MODAL: Record<Tab, ModalType> = { schedules: "schedule", shifts: "shift", rosters: "roster", logs: "log" };
const MODAL_LABEL: Record<ModalType, string> = { schedule: "Schedule", shift: "Shift", roster: "Roster", log: "Log" };

const inputCls = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";
const labelCls = "text-xs text-gray-500 mb-1 block";

const emptySchedule = { name: "", description: "", startTime: "09:00", endTime: "18:00", totalHours: 8, gracePeriodMinutes: 10, workingDaysPerWeek: 5, isFlexible: false, isActive: true };
const emptyShift = { name: "", code: "", startTime: "", endTime: "", workScheduleId: "", isNightShift: false, isActive: true };
const emptyRoster = { employeeId: "", shiftId: "", rosterDate: "", isOff: false, note: "" };
const emptyLog = { employeeId: "", attendanceDate: "", checkIn: "", checkOut: "", status: "Present", remarks: "" };

/** "09:00:00" (TimeSpan) → "09:00" for a time input or display. */
const hhmm = (timeSpan?: string) => (timeSpan ? timeSpan.slice(0, 5) : "");
/** "09:00" from a time input → "09:00:00" for a TimeSpan field. */
const toTimeSpan = (time: string) => (time.length === 5 ? `${time}:00` : time);

/**
 * The log's time inputs give "HH:mm"; the API wants a full DateTime, so pin the time to the
 * attendance date. A check-out at or before check-in (night shift) belongs to the next day.
 */
function toCheckTimestamp(date: string, time: string, nextDay = false): string | null {
  if (!date || !time) return null;
  const d = new Date(`${date}T${time}`);
  if (nextDay) d.setDate(d.getDate() + 1);
  return d.toISOString();
}

function toTimeInput(timestamp: string | null | undefined): string {
  if (!timestamp) return "";
  const d = new Date(timestamp);
  return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
}

const formatTime = (timestamp: string | null | undefined) =>
  timestamp ? new Date(timestamp).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : "—";

const statusLabel = (status: string) => ATTENDANCE_STATUSES.find(s => s.value === status)?.label ?? status;

function statusBadgeCls(status: string) {
  if (status === "Present") return "bg-green-50 text-green-700";
  if (status === "Absent") return "bg-red-50 text-red-600";
  if (status === "Late" || status === "HalfDay") return "bg-yellow-50 text-yellow-700";
  return "bg-blue-50 text-blue-700";
}

const activeBadge = (isActive: boolean) => (
  <span className={`text-xs px-2 py-0.5 rounded-full ${isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{isActive ? "Active" : "Inactive"}</span>
);

export default function AttendanceManagement() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<Tab>("schedules");
  // One grid state per tab: each list sorts on its own columns.
  const scheduleGrid = useDataGrid({ defaultSortBy: "name" });
  const shiftGrid = useDataGrid({ defaultSortBy: "name" });
  const rosterGrid = useDataGrid({ defaultSortBy: "rosterDate", defaultSortDescending: true });
  const logGrid = useDataGrid({ defaultSortBy: "attendanceDate", defaultSortDescending: true });

  const [modalType, setModalType] = useState<ModalType | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [scheduleForm, setScheduleForm] = useState(emptySchedule);
  const [shiftForm, setShiftForm] = useState(emptyShift);
  const [rosterForm, setRosterForm] = useState(emptyRoster);
  const [logForm, setLogForm] = useState(emptyLog);

  const { data: schedulesData, isLoading: schedulesLoading } = useQuery({
    queryKey: ["work-schedules", ...scheduleGrid.queryKey],
    queryFn: () => getWorkSchedules(scheduleGrid.queryParams),
  });
  const { data: shiftsData, isLoading: shiftsLoading } = useQuery({
    queryKey: ["shifts", ...shiftGrid.queryKey],
    queryFn: () => getShifts(shiftGrid.queryParams),
  });
  const { data: rostersData, isLoading: rostersLoading } = useQuery({
    queryKey: ["rosters", ...rosterGrid.queryKey],
    queryFn: () => getRosters(rosterGrid.queryParams),
  });
  const { data: logsData, isLoading: logsLoading } = useQuery({
    queryKey: ["attendance-logs", ...logGrid.queryKey],
    queryFn: () => getAttendanceLogs(logGrid.queryParams),
  });

  const { options: empOptions, isLoading: empLoading } = useEmployeeLookup();
  const { data: allSchedulesData } = useQuery({
    queryKey: ["work-schedules-all"],
    queryFn: () => getWorkSchedules({ page: 1, pageSize: 100 }),
  });
  const { data: allShiftsData } = useQuery({
    queryKey: ["shifts-all"],
    queryFn: () => getShifts({ page: 1, pageSize: 100 }),
  });

  const scheduleOptions = useMemo(
    () => (allSchedulesData?.data?.data?.items ?? []).filter(s => s.isActive).map(s => ({ value: s.id, label: s.name })),
    [allSchedulesData],
  );
  const shiftOptions = useMemo(
    () => (allShiftsData?.data?.data?.items ?? []).filter(s => s.isActive).map(s => ({
      value: s.id,
      label: `${s.name} (${hhmm(s.startTime)}–${hhmm(s.endTime)}) · ${s.workScheduleName}`,
    })),
    [allShiftsData],
  );

  function closeModal() { setModalType(null); setEditingId(null); }
  function onSaved(...keys: string[]) {
    keys.forEach(queryKey => qc.invalidateQueries({ queryKey: [queryKey] }));
    closeModal();
  }

  // API errors are toasted by the shared client; mutations only handle success.
  const saveScheduleMutation = useMutation({
    mutationFn: () => {
      const payload = { ...scheduleForm, startTime: toTimeSpan(scheduleForm.startTime), endTime: toTimeSpan(scheduleForm.endTime) };
      return editingId ? updateWorkSchedule(editingId, payload) : createWorkSchedule(payload);
    },
    onSuccess: () => onSaved("work-schedules", "work-schedules-all", "shifts"),
  });

  const saveShiftMutation = useMutation({
    mutationFn: () => {
      const payload = { ...shiftForm, startTime: toTimeSpan(shiftForm.startTime), endTime: toTimeSpan(shiftForm.endTime) };
      return editingId ? updateShift(editingId, payload) : createShift(payload);
    },
    onSuccess: () => onSaved("shifts", "shifts-all", "rosters"),
  });

  const saveRosterMutation = useMutation({
    mutationFn: () => {
      const { employeeId, rosterDate, ...rest } = rosterForm;
      return editingId ? updateRoster(editingId, rest) : createRoster({ employeeId, rosterDate, ...rest });
    },
    onSuccess: () => onSaved("rosters", "attendance-logs"),
  });

  const saveLogMutation = useMutation({
    mutationFn: () => {
      const overnight = !!logForm.checkIn && !!logForm.checkOut && logForm.checkOut <= logForm.checkIn;
      const payload = {
        ...logForm,
        checkIn: toCheckTimestamp(logForm.attendanceDate, logForm.checkIn),
        checkOut: toCheckTimestamp(logForm.attendanceDate, logForm.checkOut, overnight),
      };
      return editingId ? updateAttendanceLog(editingId, payload) : createAttendanceLog(payload);
    },
    onSuccess: () => onSaved("attendance-logs"),
  });

  function openAdd() {
    const type = TAB_MODAL[activeTab];
    if (type === "schedule") setScheduleForm(emptySchedule);
    if (type === "shift") setShiftForm(emptyShift);
    if (type === "roster") setRosterForm(emptyRoster);
    if (type === "log") setLogForm(emptyLog);
    setEditingId(null);
    setModalType(type);
  }

  function editSchedule(s: WorkSchedule) {
    setScheduleForm({
      name: s.name, description: s.description ?? "", startTime: hhmm(s.startTime), endTime: hhmm(s.endTime),
      totalHours: s.totalHours, gracePeriodMinutes: s.gracePeriodMinutes, workingDaysPerWeek: s.workingDaysPerWeek,
      isFlexible: s.isFlexible, isActive: s.isActive,
    });
    setEditingId(s.id);
    setModalType("schedule");
  }

  function editShift(s: Shift) {
    setShiftForm({
      name: s.name, code: s.code ?? "", startTime: hhmm(s.startTime), endTime: hhmm(s.endTime),
      workScheduleId: s.workScheduleId, isNightShift: s.isNightShift, isActive: s.isActive,
    });
    setEditingId(s.id);
    setModalType("shift");
  }

  function editRoster(r: EmployeeRoster) {
    setRosterForm({ employeeId: r.employeeId, shiftId: r.shiftId, rosterDate: r.rosterDate.split("T")[0], isOff: r.isOff, note: r.note ?? "" });
    setEditingId(r.id);
    setModalType("roster");
  }

  function editLog(log: AttendanceLog) {
    setLogForm({
      employeeId: log.employeeId,
      attendanceDate: log.attendanceDate.split("T")[0],
      checkIn: toTimeInput(log.checkIn),
      checkOut: toTimeInput(log.checkOut),
      status: log.status,
      remarks: log.remarks || "",
    });
    setEditingId(log.id);
    setModalType("log");
  }

  function submitSchedule() {
    if (!scheduleForm.name.trim()) { toast.error("Schedule name is required."); return; }
    saveScheduleMutation.mutate();
  }

  function submitShift() {
    if (!shiftForm.name.trim() || !shiftForm.code.trim() || !shiftForm.startTime || !shiftForm.endTime || !shiftForm.workScheduleId) {
      toast.error("Please fill in all required fields.");
      return;
    }
    saveShiftMutation.mutate();
  }

  function submitRoster() {
    if (!rosterForm.employeeId || !rosterForm.shiftId || !rosterForm.rosterDate) {
      toast.error("Employee, shift and date are required.");
      return;
    }
    saveRosterMutation.mutate();
  }

  function submitLog() {
    if (!logForm.employeeId || !logForm.attendanceDate) {
      toast.error("Employee and date are required.");
      return;
    }
    saveLogMutation.mutate();
  }

  const scheduleCols = [
    { key: "name", label: "Schedule Name" },
    { key: "hours", label: "Office Hours", sortable: false, render: (row: WorkSchedule) => `${hhmm(row.startTime)}–${hhmm(row.endTime)}` },
    { key: "totalHours", label: "Daily Hours", sortable: false },
    { key: "gracePeriodMinutes", label: "Grace (min)", sortable: false },
    { key: "workingDaysPerWeek", label: "Days/Week", sortable: false },
    { key: "isFlexible", label: "Flexible", sortable: false, render: (row: WorkSchedule) => (row.isFlexible ? "Yes" : "No") },
    { key: "isActive", label: "Status", sortable: false, render: (row: WorkSchedule) => activeBadge(row.isActive) },
  ];

  const shiftCols = [
    { key: "name", label: "Shift Name" },
    { key: "code", label: "Code", sortable: false },
    { key: "startTime", label: "Start", render: (row: Shift) => hhmm(row.startTime) },
    { key: "endTime", label: "End", render: (row: Shift) => hhmm(row.endTime) },
    { key: "isNightShift", label: "Night", sortable: false, render: (row: Shift) => (row.isNightShift ? "Yes" : "No") },
    { key: "workScheduleName", label: "Schedule" },
    { key: "isActive", label: "Status", sortable: false, render: (row: Shift) => activeBadge(row.isActive) },
  ];

  const rosterCols = [
    { key: "rosterDate", label: "Date", render: (row: EmployeeRoster) => new Date(row.rosterDate).toLocaleDateString() },
    { key: "employeeName", label: "Employee" },
    { key: "shiftName", label: "Shift" },
    { key: "shiftTime", label: "Shift Time", sortable: false, render: (row: EmployeeRoster) => `${hhmm(row.shiftStartTime)}–${hhmm(row.shiftEndTime)}` },
    { key: "isOff", label: "Day", sortable: false, render: (row: EmployeeRoster) => (
      <span className={`text-xs px-2 py-0.5 rounded-full ${row.isOff ? "bg-gray-100 text-gray-600" : "bg-green-50 text-green-700"}`}>{row.isOff ? "Day Off" : "Working"}</span>
    ) },
    { key: "note", label: "Note", sortable: false },
  ];

  const logCols = [
    { key: "employeeName", label: "Employee" },
    { key: "attendanceDate", label: "Date", render: (row: AttendanceLog) => new Date(row.attendanceDate).toLocaleDateString() },
    { key: "shiftName", label: "Shift", sortable: false, render: (row: AttendanceLog) => row.shiftName ?? <span className="text-xs text-gray-400">Not rostered</span> },
    { key: "checkIn", label: "Check In", render: (row: AttendanceLog) => formatTime(row.checkIn) },
    { key: "checkOut", label: "Check Out", render: (row: AttendanceLog) => formatTime(row.checkOut) },
    { key: "workHours", label: "Hours" },
    { key: "overtimeHours", label: "Overtime" },
    { key: "status", label: "Status", sortable: false, render: (row: AttendanceLog) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusBadgeCls(row.status)}`}>{statusLabel(row.status)}</span> },
  ];

  const saveButton = (onClick: () => void, isPending: boolean) => (
    <div className="flex justify-end gap-2 pt-2">
      <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
      <button onClick={onClick} disabled={isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{isPending ? "Saving..." : "Save"}</button>
    </div>
  );

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Attendance Management</h2>
          <p className="text-xs text-gray-400">Schedules → shifts → rosters (who works when) → daily attendance logs</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add {MODAL_LABEL[TAB_MODAL[activeTab]]}
        </button>
      </div>

      <div className="flex gap-4 mb-4">
        {(["schedules", "shifts", "rosters", "logs"] as const).map(tab => (
          <button key={tab} onClick={() => setActiveTab(tab)} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {activeTab === "schedules" && (
        <DataGrid
          columns={scheduleCols}
          data={schedulesData?.data?.data?.items ?? []}
          loading={schedulesLoading}
          sortBy={scheduleGrid.sortBy}
          sortDescending={scheduleGrid.sortDescending}
          onSort={scheduleGrid.handleSort}
          search={scheduleGrid.search}
          onSearch={scheduleGrid.setSearch}
          searchPlaceholder="Search schedules..."
          page={scheduleGrid.page}
          totalPages={schedulesData?.data?.data?.totalPages ?? 1}
          onPageChange={scheduleGrid.setPage}
          pageSize={scheduleGrid.pageSize}
          onPageSizeChange={scheduleGrid.setPageSize}
          totalCount={schedulesData?.data?.data?.totalCount ?? 0}
          onEdit={editSchedule}
          emptyMessage="No schedules found"
          actions={<ExportMenu baseUrl="attendance/work-schedules" filters={{ search: scheduleGrid.search || undefined }} />}
        />
      )}
      {activeTab === "shifts" && (
        <DataGrid
          columns={shiftCols}
          data={shiftsData?.data?.data?.items ?? []}
          loading={shiftsLoading}
          sortBy={shiftGrid.sortBy}
          sortDescending={shiftGrid.sortDescending}
          onSort={shiftGrid.handleSort}
          search={shiftGrid.search}
          onSearch={shiftGrid.setSearch}
          searchPlaceholder="Search shifts..."
          page={shiftGrid.page}
          totalPages={shiftsData?.data?.data?.totalPages ?? 1}
          onPageChange={shiftGrid.setPage}
          pageSize={shiftGrid.pageSize}
          onPageSizeChange={shiftGrid.setPageSize}
          totalCount={shiftsData?.data?.data?.totalCount ?? 0}
          onEdit={editShift}
          emptyMessage="No shifts found"
          actions={<ExportMenu baseUrl="attendance/shifts" filters={{ search: shiftGrid.search || undefined }} />}
        />
      )}
      {activeTab === "rosters" && (
        <DataGrid
          columns={rosterCols}
          data={rostersData?.data?.data?.items ?? []}
          loading={rostersLoading}
          sortBy={rosterGrid.sortBy}
          sortDescending={rosterGrid.sortDescending}
          onSort={rosterGrid.handleSort}
          search={rosterGrid.search}
          onSearch={rosterGrid.setSearch}
          searchPlaceholder="Search by employee or shift..."
          page={rosterGrid.page}
          totalPages={rostersData?.data?.data?.totalPages ?? 1}
          onPageChange={rosterGrid.setPage}
          pageSize={rosterGrid.pageSize}
          onPageSizeChange={rosterGrid.setPageSize}
          totalCount={rostersData?.data?.data?.totalCount ?? 0}
          onEdit={editRoster}
          emptyMessage="No roster entries found"
        />
      )}
      {activeTab === "logs" && (
        <DataGrid
          columns={logCols}
          data={logsData?.data?.data?.items ?? []}
          loading={logsLoading}
          sortBy={logGrid.sortBy}
          sortDescending={logGrid.sortDescending}
          onSort={logGrid.handleSort}
          search={logGrid.search}
          onSearch={logGrid.setSearch}
          searchPlaceholder="Search by employee..."
          page={logGrid.page}
          totalPages={logsData?.data?.data?.totalPages ?? 1}
          onPageChange={logGrid.setPage}
          pageSize={logGrid.pageSize}
          onPageSizeChange={logGrid.setPageSize}
          totalCount={logsData?.data?.data?.totalCount ?? 0}
          onEdit={editLog}
          emptyMessage="No attendance logs found"
          actions={<ExportMenu baseUrl="attendance/logs" filters={{ search: logGrid.search || undefined }} />}
        />
      )}

      <Modal title={modalType ? `${editingId ? "Edit" : "Add"} ${MODAL_LABEL[modalType]}` : ""} open={modalType !== null} onClose={closeModal}>
        {modalType === "schedule" && (
          <div className="space-y-3">
            <div><label className={labelCls}>Name *</label><input className={inputCls} value={scheduleForm.name} onChange={e => setScheduleForm(f => ({ ...f, name: e.target.value }))} /></div>
            <div><label className={labelCls}>Description</label><textarea rows={2} className={inputCls} value={scheduleForm.description} onChange={e => setScheduleForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className={labelCls}>Office Start *</label><input type="time" className={inputCls} value={scheduleForm.startTime} onChange={e => setScheduleForm(f => ({ ...f, startTime: e.target.value }))} /></div>
              <div><label className={labelCls}>Office End *</label><input type="time" className={inputCls} value={scheduleForm.endTime} onChange={e => setScheduleForm(f => ({ ...f, endTime: e.target.value }))} /></div>
            </div>
            <div className="grid grid-cols-3 gap-3">
              <div><label className={labelCls}>Daily Hours *</label><input type="number" min={1} max={24} step={0.5} className={inputCls} value={scheduleForm.totalHours} onChange={e => setScheduleForm(f => ({ ...f, totalHours: Number(e.target.value) }))} /></div>
              <div><label className={labelCls}>Grace Period (min)</label><input type="number" min={0} className={inputCls} value={scheduleForm.gracePeriodMinutes} onChange={e => setScheduleForm(f => ({ ...f, gracePeriodMinutes: Number(e.target.value) }))} /></div>
              <div><label className={labelCls}>Working Days/Week *</label><input type="number" min={1} max={7} className={inputCls} value={scheduleForm.workingDaysPerWeek} onChange={e => setScheduleForm(f => ({ ...f, workingDaysPerWeek: Number(e.target.value) }))} /></div>
            </div>
            <div className="flex items-center gap-6">
              <label className="flex items-center gap-2 text-xs text-gray-600"><input type="checkbox" checked={scheduleForm.isFlexible} onChange={e => setScheduleForm(f => ({ ...f, isFlexible: e.target.checked }))} /> Flexible timing (never marked late)</label>
              {editingId && <label className="flex items-center gap-2 text-xs text-gray-600"><input type="checkbox" checked={scheduleForm.isActive} onChange={e => setScheduleForm(f => ({ ...f, isActive: e.target.checked }))} /> Active</label>}
            </div>
            {saveButton(submitSchedule, saveScheduleMutation.isPending)}
          </div>
        )}
        {modalType === "shift" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div><label className={labelCls}>Shift Name *</label><input className={inputCls} value={shiftForm.name} onChange={e => setShiftForm(f => ({ ...f, name: e.target.value }))} /></div>
              <div><label className={labelCls}>Code *</label><input className={inputCls} value={shiftForm.code} onChange={e => setShiftForm(f => ({ ...f, code: e.target.value }))} /></div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className={labelCls}>Start Time *</label><input type="time" className={inputCls} value={shiftForm.startTime} onChange={e => setShiftForm(f => ({ ...f, startTime: e.target.value }))} /></div>
              <div><label className={labelCls}>End Time *</label><input type="time" className={inputCls} value={shiftForm.endTime} onChange={e => setShiftForm(f => ({ ...f, endTime: e.target.value }))} /></div>
            </div>
            <div><SearchableDropdown label="Work Schedule *" options={scheduleOptions} value={shiftForm.workScheduleId} onChange={v => setShiftForm(f => ({ ...f, workScheduleId: v || "" }))} placeholder="Select Schedule" searchPlaceholder="Search schedules..." required /></div>
            <div className="flex items-center gap-6">
              <label className="flex items-center gap-2 text-xs text-gray-600"><input type="checkbox" checked={shiftForm.isNightShift} onChange={e => setShiftForm(f => ({ ...f, isNightShift: e.target.checked }))} /> Night shift (ends next day)</label>
              {editingId && <label className="flex items-center gap-2 text-xs text-gray-600"><input type="checkbox" checked={shiftForm.isActive} onChange={e => setShiftForm(f => ({ ...f, isActive: e.target.checked }))} /> Active</label>}
            </div>
            {saveButton(submitShift, saveShiftMutation.isPending)}
          </div>
        )}
        {modalType === "roster" && (
          <div className="space-y-3">
            <div><SearchableDropdown label="Employee *" options={empOptions} value={rosterForm.employeeId} onChange={v => setRosterForm(f => ({ ...f, employeeId: v || "" }))} placeholder="Select Employee" searchPlaceholder="Search employee..." loading={empLoading} disabled={!!editingId} required /></div>
            <div><label className={labelCls}>Date *</label><DateInput className={inputCls} value={rosterForm.rosterDate} onChange={e => setRosterForm(f => ({ ...f, rosterDate: e.target.value }))} disabled={!!editingId} /></div>
            <div><SearchableDropdown label="Shift *" options={shiftOptions} value={rosterForm.shiftId} onChange={v => setRosterForm(f => ({ ...f, shiftId: v || "" }))} placeholder="Select Shift" searchPlaceholder="Search shifts..." required /></div>
            <label className="flex items-center gap-2 text-xs text-gray-600"><input type="checkbox" checked={rosterForm.isOff} onChange={e => setRosterForm(f => ({ ...f, isOff: e.target.checked }))} /> Day off (any hours worked count as overtime)</label>
            <div><label className={labelCls}>Note</label><input className={inputCls} value={rosterForm.note} onChange={e => setRosterForm(f => ({ ...f, note: e.target.value }))} /></div>
            {saveButton(submitRoster, saveRosterMutation.isPending)}
          </div>
        )}
        {modalType === "log" && (
          <div className="space-y-3">
            <div><SearchableDropdown label="Employee *" options={empOptions} value={logForm.employeeId} onChange={v => setLogForm(f => ({ ...f, employeeId: v || "" }))} placeholder="Select Employee" searchPlaceholder="Search employee..." loading={empLoading} disabled={!!editingId} required /></div>
            <div><label className={labelCls}>Date *</label><DateInput className={inputCls} value={logForm.attendanceDate} onChange={e => setLogForm(f => ({ ...f, attendanceDate: e.target.value }))} disabled={!!editingId} /></div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className={labelCls}>Check In</label><input type="time" className={inputCls} value={logForm.checkIn} onChange={e => setLogForm(f => ({ ...f, checkIn: e.target.value }))} /></div>
              <div><label className={labelCls}>Check Out</label><input type="time" className={inputCls} value={logForm.checkOut} onChange={e => setLogForm(f => ({ ...f, checkOut: e.target.value }))} /></div>
            </div>
            <div><label className={labelCls}>Status</label><select className={inputCls} value={logForm.status} onChange={e => setLogForm(f => ({ ...f, status: e.target.value }))}>{ATTENDANCE_STATUSES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}</select></div>
            <p className="text-xs text-gray-400">If the employee is rostered that day, a check-in after shift start + grace period is marked Late, and hours beyond the shift count as overtime.</p>
            <div><label className={labelCls}>Notes</label><input className={inputCls} value={logForm.remarks} onChange={e => setLogForm(f => ({ ...f, remarks: e.target.value }))} /></div>
            {saveButton(submitLog, saveLogMutation.isPending)}
          </div>
        )}
      </Modal>
    </div>
  );
}
