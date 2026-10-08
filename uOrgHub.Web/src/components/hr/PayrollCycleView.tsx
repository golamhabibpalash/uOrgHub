import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FileText, Play, RefreshCw } from "lucide-react";
import toast from "react-hot-toast";
import Modal from "../shared/Modal";
import DataGrid from "../shared/DataGrid";
import ExportMenu from "../shared/ExportMenu";
import ConfirmDialog from "../shared/ConfirmDialog";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getPayrollEntries, processPayrollCycle, updatePayrollCycle, PayrollCycle, PayrollEntry } from "../../api/hr";
import PayslipModal from "./PayslipModal";
import { PAYROLL_STATUS_BADGE } from "./payrollStatus";

const money = (n: number) => n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Status changes offered per status — mirrors PayrollCycleTransitions on the backend. */
const ACTIONS: Record<string, { to: string; label: string; confirm: string; tone?: "danger" }[]> = {
  Processed: [
    { to: "Approved", label: "Approve", confirm: "Approve this payroll? Payslips can no longer be recalculated unless it is un-approved." },
    { to: "Cancelled", label: "Cancel payroll", confirm: "Cancel this payroll cycle?", tone: "danger" },
  ],
  Approved: [
    { to: "Paid", label: "Mark as paid", confirm: "Mark salaries as paid? This is final — a paid payroll can't be changed." },
    { to: "Processed", label: "Un-approve", confirm: "Move this payroll back to Processed so it can be recalculated?" },
    { to: "Cancelled", label: "Cancel payroll", confirm: "Cancel this payroll cycle?", tone: "danger" },
  ],
  Draft: [{ to: "Cancelled", label: "Cancel payroll", confirm: "Cancel this payroll cycle?", tone: "danger" }],
  Cancelled: [{ to: "Draft", label: "Reopen as draft", confirm: "Reopen this cycle as a draft?" }],
};

interface Props {
  cycle: PayrollCycle;
  onClose: () => void;
}

export default function PayrollCycleView({ cycle: initial, onClose }: Props) {
  const qc = useQueryClient();
  const grid = useDataGrid({ defaultSortBy: "Employee.EmployeeCode" });
  const [cycle, setCycle] = useState(initial);
  const [pending, setPending] = useState<(typeof ACTIONS)[string][number] | null>(null);
  const [payslipEntryId, setPayslipEntryId] = useState<string | null>(null);
  const [skipped, setSkipped] = useState<string[]>([]);

  const { data, isLoading } = useQuery({
    queryKey: ["payroll-entries", cycle.id, ...grid.queryKey],
    queryFn: () => getPayrollEntries(cycle.id, grid.queryParams),
  });

  function refresh() {
    qc.invalidateQueries({ queryKey: ["payroll-cycles"] });
    qc.invalidateQueries({ queryKey: ["payroll-entries", cycle.id] });
  }

  const processMutation = useMutation({
    mutationFn: () => processPayrollCycle(cycle.id),
    onSuccess: res => {
      const result = res.data.data;
      if (!result) return;
      setCycle(result.cycle);
      setSkipped(result.skipped);
      refresh();
    },
  });

  const statusMutation = useMutation({
    mutationFn: (to: string) => updatePayrollCycle(cycle.id, { status: to, remarks: cycle.remarks }),
    onSuccess: res => {
      const updated = res.data.data;
      setPending(null);
      if (!updated) return;
      setCycle(updated);
      toast.success(`Payroll is now ${updated.status}.`);
      refresh();
    },
    onError: () => setPending(null),
  });

  const canProcess = cycle.status === "Draft" || cycle.status === "Processed";
  const entries = data?.data?.data?.items ?? [];

  const columns = [
    { key: "employeeCode", label: "ID", sortable: false },
    { key: "employeeName", label: "Employee", sortable: false },
    { key: "days", label: "P / A / L", sortable: false, render: (r: PayrollEntry) => `${r.presentDays} / ${r.absentDays} / ${r.leaveDays}` },
    { key: "basicSalary", label: "Basic", render: (r: PayrollEntry) => money(r.basicSalary) },
    { key: "totalAllowances", label: "Allowances", render: (r: PayrollEntry) => money(r.totalAllowances) },
    { key: "overtimePay", label: "Overtime", render: (r: PayrollEntry) => money(r.overtimePay) },
    { key: "grossSalary", label: "Gross", render: (r: PayrollEntry) => money(r.grossSalary) },
    { key: "totalDeductions", label: "Deductions", render: (r: PayrollEntry) => money(r.totalDeductions + r.taxAmount) },
    { key: "netSalary", label: "Net Pay", render: (r: PayrollEntry) => <span className="font-medium">{money(r.netSalary)}</span> },
    { key: "payslip", label: "", sortable: false, render: (r: PayrollEntry) => (
      <button onClick={() => setPayslipEntryId(r.id)} className="flex items-center gap-1 text-xs text-primary-600 hover:text-primary-800"><FileText size={14} /> Payslip</button>
    ) },
  ];

  return (
    <Modal title={cycle.title} open onClose={onClose} size="5xl">
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3 text-sm text-gray-600">
            <span className={`text-xs px-2 py-0.5 rounded-full ${PAYROLL_STATUS_BADGE[cycle.status] ?? "bg-gray-100 text-gray-600"}`}>{cycle.status}</span>
            <span>{new Date(cycle.startDate).toLocaleDateString()} – {new Date(cycle.endDate).toLocaleDateString()}</span>
            {cycle.processedDate && <span className="text-xs text-gray-400">Processed {new Date(cycle.processedDate).toLocaleString()}</span>}
          </div>
          <div className="flex flex-wrap gap-2">
            {canProcess && (
              <button onClick={() => processMutation.mutate()} disabled={processMutation.isPending}
                className="flex items-center gap-1.5 text-sm bg-primary-500 text-white rounded-lg px-3 py-1.5 hover:bg-primary-600 disabled:opacity-50">
                {cycle.status === "Draft" ? <Play size={14} /> : <RefreshCw size={14} />}
                {processMutation.isPending ? "Processing..." : cycle.status === "Draft" ? "Process payroll" : "Re-process"}
              </button>
            )}
            {(ACTIONS[cycle.status] ?? []).map(a => (
              <button key={a.to} onClick={() => setPending(a)}
                className={`text-sm rounded-lg px-3 py-1.5 border ${a.tone === "danger" ? "border-red-200 text-red-600 hover:bg-red-50" : "border-gray-200 hover:bg-gray-50"}`}>
                {a.label}
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 text-sm">
          {[["Employees", String(cycle.totalEmployees)], ["Basic", money(cycle.totalBasic)], ["Allowances + OT", money(cycle.totalAllowances)],
            ["Deductions", money(cycle.totalDeductions)], ["Net payable", money(cycle.totalNetPay)]].map(([label, value]) => (
            <div key={label} className="rounded-lg bg-gray-50 border border-gray-100 px-3 py-2">
              <div className="text-xs text-gray-500">{label}</div>
              <div className="font-semibold text-gray-900">{value}</div>
            </div>
          ))}
        </div>

        {skipped.length > 0 && (
          <div className="text-xs text-amber-700 bg-amber-50 border border-amber-100 rounded-lg px-3 py-2">
            <p className="font-medium mb-1">{skipped.length} active employee(s) were not paid:</p>
            <ul className="list-disc pl-4 space-y-0.5">{skipped.map(s => <li key={s}>{s}</li>)}</ul>
            <p className="mt-1">Set their salary structure on the Structures tab, then re-process.</p>
          </div>
        )}

        {cycle.status === "Draft" && entries.length === 0 ? (
          <div className="text-center text-sm text-gray-500 border border-dashed border-gray-200 rounded-lg py-10">
            Not processed yet. <strong>Process payroll</strong> calculates every active employee's pay from their salary structure, attendance, leave and overtime for this period.
          </div>
        ) : (
          <DataGrid
            columns={columns}
            data={entries}
            loading={isLoading}
            sortBy={grid.sortBy}
            sortDescending={grid.sortDescending}
            onSort={grid.handleSort}
            page={grid.page}
            totalPages={data?.data?.data?.totalPages ?? 1}
            onPageChange={grid.setPage}
            pageSize={grid.pageSize}
            onPageSizeChange={grid.setPageSize}
            totalCount={data?.data?.data?.totalCount ?? 0}
            emptyMessage="No payroll entries"
            actions={<ExportMenu baseUrl={`payroll/cycles/${cycle.id}/entries`} />}
          />
        )}
      </div>

      <ConfirmDialog
        open={!!pending}
        title={pending?.label ?? ""}
        message={pending?.confirm ?? ""}
        confirmLabel={pending?.label}
        tone={pending?.tone ?? "default"}
        loading={statusMutation.isPending}
        onConfirm={() => pending && statusMutation.mutate(pending.to)}
        onCancel={() => setPending(null)}
      />
      {payslipEntryId && <PayslipModal cycleId={cycle.id} entryId={payslipEntryId} onClose={() => setPayslipEntryId(null)} />}
    </Modal>
  );
}
