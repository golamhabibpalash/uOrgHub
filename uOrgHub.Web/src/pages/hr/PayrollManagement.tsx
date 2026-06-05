import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Check, X } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getSalaryGrades,
  createSalaryGrade,
  getSalaryComponents,
  createSalaryComponent,
  getPayrollCycles,
  createPayrollCycle,
  getExpenses,
  createExpense,
  approveExpense,
  SalaryComponent,
  PayrollCycle,
  ExpenseRequest,
  getEmployees,
} from "../../api/hr";

export default function PayrollManagement() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<"grades" | "components" | "cycles" | "expenses">("grades");
  const [page, setPage] = useState(1);
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<PayrollCycle | null>(null);

  const [gradeForm, setGradeForm] = useState({ name: "", gradeCode: "", description: "", minSalary: 0, maxSalary: 0 });
  const [compForm, setCompForm] = useState({ name: "", code: "", componentType: "Allowance", isTaxable: true });
  const [cycleForm, setCycleForm] = useState({ title: "", startDate: "", endDate: "", paymentDate: "" });
  const [expForm, setExpForm] = useState({ employeeId: "", amount: 0, category: "", description: "" });

  const { data: gradesData, isLoading: gradesLoading } = useQuery({ queryKey: ["salary-grades", page], queryFn: () => getSalaryGrades({ page, pageSize: 10 }) });
  const { data: compsData, isLoading: compsLoading } = useQuery({ queryKey: ["salary-components", page], queryFn: () => getSalaryComponents({ page, pageSize: 10 }) });
  const { data: cyclesData, isLoading: cyclesLoading } = useQuery({ queryKey: ["payroll-cycles", page], queryFn: () => getPayrollCycles({ page, pageSize: 10 }) });
  const { data: expData, isLoading: expLoading } = useQuery({ queryKey: ["expenses", page], queryFn: () => getExpenses({ page, pageSize: 10 }) });
  const { data: empData } = useQuery({ queryKey: ["employees-all"], queryFn: () => getEmployees({ page: 1, pageSize: 100 }) });

  const grades = gradesData?.data?.data?.items ?? [];
  const components = compsData?.data?.data?.items ?? [];
  const cycles = cyclesData?.data?.data?.items ?? [];
  const expenses = expData?.data?.data?.items ?? [];
  const employees = empData?.data?.data?.items ?? [];

  const totalPages = activeTab === "grades" ? gradesData?.data?.data?.totalPages ?? 1
    : activeTab === "components" ? compsData?.data?.data?.totalPages ?? 1
    : activeTab === "cycles" ? cyclesData?.data?.data?.totalPages ?? 1
    : expData?.data?.data?.totalPages ?? 1;

  const gradeMutation = useMutation({ mutationFn: () => createSalaryGrade(gradeForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["salary-grades"] }); setModal(false); } });
  const compMutation = useMutation({ mutationFn: () => createSalaryComponent(compForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["salary-components"] }); setModal(false); } });
  const cycleMutation = useMutation({ mutationFn: async () => { if (!editing) { await createPayrollCycle(cycleForm); } }, onSuccess: () => { qc.invalidateQueries({ queryKey: ["payroll-cycles"] }); setModal(false); setEditing(null); } });
  const expMutation = useMutation({ mutationFn: () => createExpense(expForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["expenses"] }); setModal(false); } });
  const approveExpMutation = useMutation({ mutationFn: ({ id, approved }: { id: string; approved: boolean }) => approveExpense(id, { isApproved: approved, remarks: "" }), onSuccess: () => qc.invalidateQueries({ queryKey: ["expenses"] }) });

  function openModal(tab: typeof activeTab) {
    setActiveTab(tab);
    setModal(true);
    if (tab === "grades") setGradeForm({ name: "", gradeCode: "", description: "", minSalary: 0, maxSalary: 0 });
    if (tab === "components") setCompForm({ name: "", code: "", componentType: "Allowance", isTaxable: true });
    if (tab === "cycles") { setCycleForm({ title: "", startDate: "", endDate: "", paymentDate: "" }); setEditing(null); }
    if (tab === "expenses") setExpForm({ employeeId: "", amount: 0, category: "", description: "" });
  }

  const gradeCols = [{ key: "gradeCode", label: "Code" }, { key: "name", label: "Grade Name" }, { key: "minSalary", label: "Min Salary" }, { key: "maxSalary", label: "Max Salary" }];
  const compCols = [{ key: "code", label: "Code" }, { key: "name", label: "Component Name" }, { key: "componentType", label: "Type" }, { key: "isTaxable", label: "Taxable", render: (r: SalaryComponent) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.isTaxable ? "bg-yellow-50 text-yellow-700" : "bg-gray-50 text-gray-600"}`}>{r.isTaxable ? "Yes" : "No"}</span> }];
  const cycleCols = [{ key: "title", label: "Cycle Name" }, { key: "startDate", label: "Start", render: (r: PayrollCycle) => new Date(r.startDate).toLocaleDateString() }, { key: "endDate", label: "End", render: (r: PayrollCycle) => new Date(r.endDate).toLocaleDateString() }, { key: "paymentDate", label: "Payment Date", render: (r: PayrollCycle) => r.paymentDate ? new Date(r.paymentDate).toLocaleDateString() : "-" }, { key: "status", label: "Status", render: (r: PayrollCycle) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Active" ? "bg-green-50 text-green-700" : "bg-gray-50 text-gray-600"}`}>{r.status}</span> }];
  const expCols = [{ key: "employeeName", label: "Employee" }, { key: "amount", label: "Amount" }, { key: "category", label: "Category" }, { key: "description", label: "Description" }, { key: "status", label: "Status", render: (r: ExpenseRequest) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Approved" ? "bg-green-50 text-green-700" : r.status === "Pending" ? "bg-yellow-50 text-yellow-700" : "bg-red-50 text-red-600"}`}>{r.status}</span> }, { key: "actions", label: "Actions", render: (r: ExpenseRequest) => r.status === "Pending" ? <div className="flex gap-2"><button onClick={() => approveExpMutation.mutate({ id: r.id, approved: true })} className="text-green-600 hover:text-green-800"><Check size={16} /></button><button onClick={() => approveExpMutation.mutate({ id: r.id, approved: false })} className="text-red-600 hover:text-red-800"><X size={16} /></button></div> : null }];

  const tabs = ["grades", "components", "cycles", "expenses"] as const;

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div><h2 className="text-base font-medium text-gray-900">Payroll Management</h2><p className="text-xs text-gray-400">Manage salary structures and payroll</p></div>
        <button onClick={() => openModal(activeTab)} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"><Plus size={15} /> Add {activeTab === "grades" ? "Grade" : activeTab === "components" ? "Component" : activeTab === "cycles" ? "Cycle" : "Expense"}</button>
      </div>

      <div className="flex gap-4 mb-4">
        {tabs.map(tab => <button key={tab} onClick={() => { setActiveTab(tab); setPage(1); }} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>{tab.charAt(0).toUpperCase() + tab.slice(1)}</button>)}
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {activeTab === "grades" && (
          <>
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <ExportMenu baseUrl="payroll/salary-grades" />
            </div>
            <DataTable columns={gradeCols} data={grades} loading={gradesLoading} />
          </>
        )}
        {activeTab === "components" && (
          <>
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <ExportMenu baseUrl="payroll/salary-components" />
            </div>
            <DataTable columns={compCols} data={components} loading={compsLoading} />
          </>
        )}
        {activeTab === "cycles" && (
          <>
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <ExportMenu baseUrl="payroll/payroll-cycles" />
            </div>
            <DataTable columns={cycleCols} data={cycles} loading={cyclesLoading} />
          </>
        )}
        {activeTab === "expenses" && (
          <>
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <ExportMenu baseUrl="payroll/expenses" />
            </div>
            <DataTable columns={expCols} data={expenses} loading={expLoading} />
          </>
        )}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={activeTab === "grades" ? "Add Salary Grade" : activeTab === "components" ? "Add Salary Component" : activeTab === "cycles" ? "Add Payroll Cycle" : "Submit Expense"} open={modal} onClose={() => setModal(false)}>
        {activeTab === "grades" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={gradeForm.name} onChange={e => setGradeForm(f => ({ ...f, name: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Code</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={gradeForm.gradeCode} onChange={e => setGradeForm(f => ({ ...f, gradeCode: e.target.value }))} /></div></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={gradeForm.description} onChange={e => setGradeForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Min Salary</label><input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={gradeForm.minSalary} onChange={e => setGradeForm(f => ({ ...f, minSalary: Number(e.target.value) }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Max Salary</label><input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={gradeForm.maxSalary} onChange={e => setGradeForm(f => ({ ...f, maxSalary: Number(e.target.value) }))} /></div></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => gradeMutation.mutate()} disabled={gradeMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{gradeMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "components" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={compForm.name} onChange={e => setCompForm(f => ({ ...f, name: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Code</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={compForm.code} onChange={e => setCompForm(f => ({ ...f, code: e.target.value }))} /></div></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Type</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={compForm.componentType} onChange={e => setCompForm(f => ({ ...f, componentType: e.target.value }))}><option value="Allowance">Allowance</option><option value="Deduction">Deduction</option></select></div><div><label className="text-xs text-gray-500 mb-1 block">Taxable</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={String(compForm.isTaxable)} onChange={e => setCompForm(f => ({ ...f, isTaxable: e.target.value === "true" }))}><option value="true">Yes</option><option value="false">No</option></select></div></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => compMutation.mutate()} disabled={compMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{compMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "cycles" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Cycle Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.title} onChange={e => setCycleForm(f => ({ ...f, title: e.target.value }))} /></div>
            <div className="grid grid-cols-3 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Start Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.startDate} onChange={e => setCycleForm(f => ({ ...f, startDate: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">End Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.endDate} onChange={e => setCycleForm(f => ({ ...f, endDate: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Payment Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.paymentDate} onChange={e => setCycleForm(f => ({ ...f, paymentDate: e.target.value }))} /></div></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => cycleMutation.mutate()} disabled={cycleMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{cycleMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "expenses" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Employee</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={expForm.employeeId} onChange={e => setExpForm(f => ({ ...f, employeeId: e.target.value }))}><option value="">Select Employee</option>{employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}</select></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Amount</label><input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={expForm.amount} onChange={e => setExpForm(f => ({ ...f, amount: Number(e.target.value) }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Category</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={expForm.category} onChange={e => setExpForm(f => ({ ...f, category: e.target.value }))}><option value="">Select</option><option value="Travel">Travel</option><option value="Food">Food</option><option value="Accommodation">Accommodation</option><option value="Other">Other</option></select></div></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={expForm.description} onChange={e => setExpForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => expMutation.mutate()} disabled={expMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{expMutation.isPending ? "Submitting..." : "Submit"}</button></div>
          </div>
        )}
      </Modal>
    </div>
  );
}