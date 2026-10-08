import { useRef } from "react";
import { useQuery } from "@tanstack/react-query";
import { Printer } from "lucide-react";
import Modal from "../shared/Modal";
import { getPayslip } from "../../api/hr";
import { useAuthStore } from "../../store/authStore";
import { printDocument } from "../../utils/printDocument";

const money = (n: number) => n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const date = (d?: string) => (d ? new Date(d).toLocaleDateString() : "—");

interface Props {
  cycleId: string;
  entryId: string;
  onClose: () => void;
}

export default function PayslipModal({ cycleId, entryId, onClose }: Props) {
  const printRef = useRef<HTMLDivElement>(null);
  const companyName = useAuthStore(s => s.user?.activeCompanyName);
  const { data, isLoading } = useQuery({
    queryKey: ["payslip", cycleId, entryId],
    queryFn: () => getPayslip(cycleId, entryId),
  });
  const p = data?.data?.data;
  const earnings = p?.lines.filter(l => l.lineType === "Earning") ?? [];
  const deductions = p?.lines.filter(l => l.lineType === "Deduction") ?? [];
  const rows = Math.max(earnings.length, deductions.length);

  return (
    <Modal title="Payslip" open onClose={onClose} size="3xl">
      {isLoading || !p ? (
        <div className="h-64 rounded-lg bg-gray-50 animate-pulse" />
      ) : (
        <>
          <div className="flex justify-end mb-3">
            <button onClick={() => printRef.current && printDocument(`Payslip ${p.employeeCode} ${p.cycleTitle}`, printRef.current)}
              className="flex items-center gap-2 text-sm border border-gray-200 rounded-lg px-3 py-1.5 hover:bg-gray-50">
              <Printer size={15} /> Print
            </button>
          </div>

          <div ref={printRef} className="bg-white text-gray-900 text-sm">
            <div className="text-center border-b border-gray-200 pb-3 mb-4">
              {companyName && <div className="text-lg font-semibold">{companyName}</div>}
              <div className="text-base font-medium">Payslip — {p.cycleTitle}</div>
              <div className="text-xs text-gray-500">Period {date(p.periodStart)} to {date(p.periodEnd)}</div>
            </div>

            <div className="grid grid-cols-2 gap-x-8 gap-y-1 mb-4">
              <div><span className="text-gray-500">Employee:</span> {p.employeeName}</div>
              <div><span className="text-gray-500">Employee ID:</span> {p.employeeCode}</div>
              <div><span className="text-gray-500">Designation:</span> {p.designationName ?? "—"}</div>
              <div><span className="text-gray-500">Department:</span> {p.departmentName ?? "—"}</div>
              <div><span className="text-gray-500">Joining date:</span> {date(p.joiningDate)}</div>
              <div><span className="text-gray-500">Status:</span> {p.status}</div>
            </div>

            <div className="grid grid-cols-5 gap-2 mb-4 text-center">
              {[["Days in period", p.totalWorkingDays], ["Present", p.presentDays], ["Absent", p.absentDays], ["Leave", p.leaveDays], ["Overtime (h)", p.overtimeHours]].map(([label, value]) => (
                <div key={label as string} className="border border-gray-200 rounded-md py-1.5">
                  <div className="text-xs text-gray-500">{label}</div>
                  <div className="font-medium">{value}</div>
                </div>
              ))}
            </div>

            <table className="w-full border border-gray-200 mb-4">
              <thead className="bg-gray-50 text-xs text-gray-600">
                <tr>
                  <th className="text-left px-3 py-2 w-1/3">Earnings</th><th className="text-right px-3 py-2">Amount</th>
                  <th className="text-left px-3 py-2 w-1/3 border-l border-gray-200">Deductions</th><th className="text-right px-3 py-2">Amount</th>
                </tr>
              </thead>
              <tbody>
                {Array.from({ length: rows }).map((_, i) => (
                  <tr key={i} className="border-t border-gray-100">
                    <td className="px-3 py-1.5">{earnings[i]?.name}</td>
                    <td className="px-3 py-1.5 text-right">{earnings[i] && money(earnings[i].amount)}</td>
                    <td className="px-3 py-1.5 border-l border-gray-200">{deductions[i]?.name}</td>
                    <td className="px-3 py-1.5 text-right">{deductions[i] && money(deductions[i].amount)}</td>
                  </tr>
                ))}
                <tr className="border-t border-gray-300 font-medium">
                  <td className="px-3 py-2">Gross earnings</td><td className="px-3 py-2 text-right">{money(p.grossSalary)}</td>
                  <td className="px-3 py-2 border-l border-gray-200">Total deductions</td><td className="px-3 py-2 text-right">{money(p.totalDeductions + p.taxAmount)}</td>
                </tr>
              </tbody>
            </table>

            <div className="flex justify-between items-center rounded-lg bg-primary-50 border border-primary-100 px-4 py-3">
              <span className="font-medium text-primary-800">Net pay</span>
              <span className="text-lg font-bold text-primary-800">{money(p.netSalary)}</span>
            </div>
            <p className="text-xs text-gray-400 mt-4">This is a system-generated payslip.</p>
          </div>
        </>
      )}
    </Modal>
  );
}
