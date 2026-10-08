import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Check, X, History } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import DateInput from "../../components/shared/DateInput";
import SalaryStructureModal from "../../components/hr/SalaryStructureModal";
import PayrollCycleView from "../../components/hr/PayrollCycleView";
import { PAYROLL_STATUS_BADGE } from "../../components/hr/payrollStatus";
import { useEmployeeLookup } from "../../hooks/useEntityLookup";
import { useAuthStore } from "../../store/authStore";
import {
  getSalaryGrades,
  createSalaryGrade,
  updateSalaryGrade,
  deleteSalaryGrade,
  getSalaryComponents,
  createSalaryComponent,
  updateSalaryComponent,
  deleteSalaryComponent,
  getSalaryStructures,
  getPayrollCycles,
  createPayrollCycle,
  deletePayrollCycle,
  getExpenses,
  createExpense,
  approveExpense,
  DEDUCTION_COMPONENT_TYPES,
  SalaryGrade,
  SalaryComponent,
  SalaryStructure,
  PayrollCycle,
  ExpenseRequest,
} from "../../api/hr";

type Tab = "grades" | "components" | "structures" | "cycles" | "expenses";

const TAB_LABEL: Record<Tab, string> = { grades: "Grades", components: "Components", structures: "Salary Structures", cycles: "Payroll Cycles", expenses: "Expenses" };
const ADD_LABEL: Record<Tab, string> = { grades: "Grade", components: "Component", structures: "Salary Structure", cycles: "Payroll Cycle", expenses: "Expense" };

const COMPONENT_TYPES = [
  ["HouseRentAllowance", "House Rent Allowance"], ["MedicalAllowance", "Medical Allowance"], ["TransportAllowance", "Transport Allowance"],
  ["FoodAllowance", "Food Allowance"], ["OtherAllowance", "Other Allowance"], ["PF", "Provident Fund (deduction)"], ["Tax", "Tax (deduction)"],
  ["Loan", "Loan (deduction)"], ["Advance", "Advance (deduction)"], ["OtherDeduction", "Other Deduction"],
] as const;
const CALCULATION_LABEL: Record<string, string> = { Fixed: "Fixed amount", PercentageOfBasic: "% of basic", PercentageOfGross: "% of gross" };

const inputCls = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";
const money = (n: number) => n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const toIsoDate = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

const emptyGrade = { name: "", gradeCode: "", description: "", minSalary: 0, maxSalary: 0, isActive: true };
const emptyComponent = { name: "", code: "", componentType: "OtherAllowance", calculationType: "Fixed", defaultValue: 0, sortOrder: 0, isTaxable: true, isActive: true, description: "" };
const emptyExpense = { employeeId: "", amount: 0, category: "", description: "" };

/** A calendar month as a payroll cycle: "2026-09" → September 2026, 1st to last day. */
function cycleForMonth(month: string) {
  const [y, m] = month.split("-").map(Number);
  const start = new Date(y, m - 1, 1);
  const end = new Date(y, m, 0);
  return {
    year: y, month: m,
    title: start.toLocaleDateString(undefined, { month: "long", year: "numeric" }),
    startDate: toIsoDate(start), endDate: toIsoDate(end),
  };
}

export default function PayrollManagement() {
  const qc = useQueryClient();
  const hasClaim = useAuthStore(s => s.hasClaim);
  const canViewStructures = hasClaim("HR.SalaryStructures.View");
  const [activeTab, setActiveTab] = useState<Tab>("grades");
  // One grid state per tab: each list pages and sorts on its own columns.
  const gradeGrid = useDataGrid({ defaultSortBy: "name" });
  const compGrid = useDataGrid({ defaultSortBy: "name" });
  const structureGrid = useDataGrid({ defaultSortBy: "employeeCode" });
  const cycleGrid = useDataGrid();
  const expGrid = useDataGrid();

  const [modal, setModal] = useState(false);
  const [editingGrade, setEditingGrade] = useState<SalaryGrade | null>(null);
  const [editingComponent, setEditingComponent] = useState<SalaryComponent | null>(null);
  const [gradeDeleteTarget, setGradeDeleteTarget] = useState<SalaryGrade | null>(null);
  const [compDeleteTarget, setCompDeleteTarget] = useState<SalaryComponent | null>(null);
  const [cycleDeleteTarget, setCycleDeleteTarget] = useState<PayrollCycle | null>(null);
  const [structureModal, setStructureModal] = useState<{ mode: "new" | "edit"; structure?: SalaryStructure } | null>(null);
  const [showStructureHistory, setShowStructureHistory] = useState(false);
  const [viewCycle, setViewCycle] = useState<PayrollCycle | null>(null);

  const [gradeForm, setGradeForm] = useState(emptyGrade);
  const [gradeErrors, setGradeErrors] = useState<{ name?: string; gradeCode?: string }>({});
  const [compForm, setCompForm] = useState(emptyComponent);
  const [cycleMonth, setCycleMonth] = useState("");
  const [cycleForm, setCycleForm] = useState({ year: 0, month: 0, title: "", startDate: "", endDate: "" });
  const [expForm, setExpForm] = useState(emptyExpense);

  const { data: gradesData, isLoading: gradesLoading } = useQuery({ queryKey: ["salary-grades", ...gradeGrid.queryKey], queryFn: () => getSalaryGrades(gradeGrid.queryParams) });
  const { data: compsData, isLoading: compsLoading } = useQuery({ queryKey: ["salary-components", ...compGrid.queryKey], queryFn: () => getSalaryComponents(compGrid.queryParams) });
  const { data: structuresData, isLoading: structuresLoading } = useQuery({
    queryKey: ["salary-structures", showStructureHistory, ...structureGrid.queryKey],
    queryFn: () => getSalaryStructures(structureGrid.queryParams, { currentOnly: !showStructureHistory }),
    enabled: canViewStructures,
  });
  const { data: cyclesData, isLoading: cyclesLoading } = useQuery({ queryKey: ["payroll-cycles", ...cycleGrid.queryKey], queryFn: () => getPayrollCycles(cycleGrid.queryParams) });
  const { data: expData, isLoading: expLoading } = useQuery({ queryKey: ["expenses", ...expGrid.queryKey], queryFn: () => getExpenses(expGrid.queryParams) });
  const { options: empOptions, isLoading: empLoading } = useEmployeeLookup();

  function closeModal() { setModal(false); setEditingGrade(null); setEditingComponent(null); setGradeErrors({}); }

  function validateGrade(): boolean {
    const errs: typeof gradeErrors = {};
    if (!gradeForm.name.trim()) errs.name = "Grade name is required";
    if (!gradeForm.gradeCode.trim()) errs.gradeCode = "Grade code is required";
    setGradeErrors(errs);
    return Object.keys(errs).length === 0;
  }

  // API errors are toasted by the shared client; mutations only handle success.
  const gradeMutation = useMutation({
    mutationFn: () => (editingGrade ? updateSalaryGrade(editingGrade.id, gradeForm) : createSalaryGrade(gradeForm)),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["salary-grades"] }); qc.invalidateQueries({ queryKey: ["salary-grades-all"] }); closeModal(); },
  });
  const gradeDeleteMutation = useMutation({
    mutationFn: (id: string) => deleteSalaryGrade(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["salary-grades"] }),
    onSettled: () => setGradeDeleteTarget(null),
  });
  const compMutation = useMutation({
    mutationFn: () => (editingComponent ? updateSalaryComponent(editingComponent.id, compForm) : createSalaryComponent(compForm)),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["salary-components"] }); qc.invalidateQueries({ queryKey: ["salary-components-all"] }); closeModal(); },
  });
  const compDeleteMutation = useMutation({
    mutationFn: (id: string) => deleteSalaryComponent(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["salary-components"] }),
    onSettled: () => setCompDeleteTarget(null),
  });
  const cycleMutation = useMutation({
    mutationFn: () => createPayrollCycle(cycleForm),
    onSuccess: res => { qc.invalidateQueries({ queryKey: ["payroll-cycles"] }); closeModal(); if (res.data.data) setViewCycle(res.data.data); },
  });
  const cycleDeleteMutation = useMutation({
    mutationFn: (id: string) => deletePayrollCycle(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["payroll-cycles"] }),
    onSettled: () => setCycleDeleteTarget(null),
  });
  const expMutation = useMutation({ mutationFn: () => createExpense(expForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["expenses"] }); closeModal(); } });
  const approveExpMutation = useMutation({ mutationFn: ({ id, approved }: { id: string; approved: boolean }) => approveExpense(id, { isApproved: approved, remarks: "" }), onSuccess: () => qc.invalidateQueries({ queryKey: ["expenses"] }) });

  function openAdd() {
    if (activeTab === "structures") { setStructureModal({ mode: "new" }); return; }
    setGradeErrors({});
    if (activeTab === "grades") { setGradeForm(emptyGrade); setEditingGrade(null); }
    if (activeTab === "components") { setCompForm(emptyComponent); setEditingComponent(null); }
    if (activeTab === "cycles") {
      const now = new Date();
      const month = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
      setCycleMonth(month);
      setCycleForm(cycleForMonth(month));
    }
    if (activeTab === "expenses") setExpForm(emptyExpense);
    setModal(true);
  }

  function openEditGrade(grade: SalaryGrade) {
    setEditingGrade(grade);
    setGradeForm({ name: grade.name, gradeCode: grade.gradeCode, description: grade.description ?? "", minSalary: grade.minSalary, maxSalary: grade.maxSalary, isActive: grade.isActive });
    setModal(true);
  }

  function openEditComponent(comp: SalaryComponent) {
    setEditingComponent(comp);
    setCompForm({
      name: comp.name, code: comp.code, componentType: comp.componentType, calculationType: comp.calculationType,
      defaultValue: comp.defaultValue, sortOrder: comp.sortOrder, isTaxable: comp.isTaxable, isActive: comp.isActive,
      description: comp.description ?? "",
    });
    setModal(true);
  }

  const activeBadge = (active: boolean) => <span className={`text-xs px-2 py-0.5 rounded-full ${active ? "bg-green-50 text-green-700" : "bg-gray-50 text-gray-600"}`}>{active ? "Active" : "Inactive"}</span>;

  const gradeCols = [
    { key: "gradeCode", label: "Code" }, { key: "name", label: "Grade Name" },
    { key: "minSalary", label: "Min Salary", render: (r: SalaryGrade) => money(r.minSalary) },
    { key: "maxSalary", label: "Max Salary", render: (r: SalaryGrade) => money(r.maxSalary) },
    { key: "isActive", label: "Status", sortable: false, render: (r: SalaryGrade) => activeBadge(r.isActive) },
  ];
  const compCols = [
    { key: "code", label: "Code" }, { key: "name", label: "Component Name" },
    { key: "componentType", label: "Kind", sortable: false, render: (r: SalaryComponent) => DEDUCTION_COMPONENT_TYPES.includes(r.componentType)
      ? <span className="text-xs text-red-600">Deduction</span> : <span className="text-xs text-green-700">Allowance</span> },
    { key: "calculationType", label: "Basis", sortable: false, render: (r: SalaryComponent) => CALCULATION_LABEL[r.calculationType] ?? r.calculationType },
    { key: "defaultValue", label: "Default", sortable: false, render: (r: SalaryComponent) => r.calculationType === "Fixed" ? money(r.defaultValue) : `${r.defaultValue}%` },
    { key: "isActive", label: "Status", sortable: false, render: (r: SalaryComponent) => activeBadge(r.isActive) },
  ];
  const structureCols = [
    { key: "employeeCode", label: "ID" },
    { key: "employeeName", label: "Employee" },
    { key: "designationName", label: "Designation", sortable: false },
    { key: "salaryGradeName", label: "Grade", sortable: false },
    { key: "basicSalary", label: "Basic", render: (r: SalaryStructure) => money(r.basicSalary) },
    { key: "grossSalary", label: "Gross", render: (r: SalaryStructure) => money(r.grossSalary) },
    { key: "netSalary", label: "Net", sortable: false, render: (r: SalaryStructure) => money(r.netSalary) },
    { key: "effectiveDate", label: "Effective", render: (r: SalaryStructure) => (
      <span>{new Date(r.effectiveDate).toLocaleDateString()}{r.endDate && <span className="text-gray-400"> – {new Date(r.endDate).toLocaleDateString()}</span>}</span>
    ) },
    { key: "revise", label: "", sortable: false, render: (r: SalaryStructure) => r.isActive
      ? <button onClick={() => setStructureModal({ mode: "new", structure: r })} className="text-xs text-primary-600 hover:text-primary-800" title="Start a new structure from a later date (increment, promotion)">Revise</button>
      : <span className="text-xs text-gray-400">History</span> },
  ];
  const cycleCols = [
    { key: "title", label: "Cycle", sortable: false },
    { key: "period", label: "Period", sortable: false, render: (r: PayrollCycle) => `${new Date(r.startDate).toLocaleDateString()} – ${new Date(r.endDate).toLocaleDateString()}` },
    { key: "totalEmployees", label: "Employees", sortable: false },
    { key: "totalNetPay", label: "Net Payable", sortable: false, render: (r: PayrollCycle) => money(r.totalNetPay) },
    { key: "status", label: "Status", sortable: false, render: (r: PayrollCycle) => <span className={`text-xs px-2 py-0.5 rounded-full ${PAYROLL_STATUS_BADGE[r.status] ?? "bg-gray-100 text-gray-600"}`}>{r.status}</span> },
    { key: "open", label: "", sortable: false, render: (r: PayrollCycle) => <button onClick={() => setViewCycle(r)} className="text-xs text-primary-600 hover:text-primary-800">{r.status === "Draft" ? "Process" : "Open"}</button> },
  ];
  const expCols = [
    { key: "employeeName", label: "Employee" }, { key: "amount", label: "Amount" }, { key: "category", label: "Category" }, { key: "description", label: "Description" },
    { key: "status", label: "Status", sortable: false, render: (r: ExpenseRequest) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Approved" ? "bg-green-50 text-green-700" : r.status === "Pending" ? "bg-yellow-50 text-yellow-700" : "bg-red-50 text-red-600"}`}>{r.status}</span> },
    { key: "actions", label: "Actions", sortable: false, render: (r: ExpenseRequest) => r.status === "Pending" ? <div className="flex gap-2"><button onClick={() => approveExpMutation.mutate({ id: r.id, approved: true })} className="text-green-600 hover:text-green-800"><Check size={16} /></button><button onClick={() => approveExpMutation.mutate({ id: r.id, approved: false })} className="text-red-600 hover:text-red-800"><X size={16} /></button></div> : null },
  ];

  const tabs: Tab[] = ["grades", "components", ...(canViewStructures ? ["structures" as const] : []), "cycles", "expenses"];
  const saveRow = (onSave: () => void, pending: boolean, label = "Save") => (
    <div className="flex justify-end gap-2 pt-2">
      <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
      <button onClick={onSave} disabled={pending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{pending ? "Saving..." : label}</button>
    </div>
  );

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div><h2 className="text-base font-medium text-gray-900">Payroll Management</h2><p className="text-xs text-gray-400">Grades and components → each employee's salary structure → monthly payroll cycles and payslips</p></div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"><Plus size={15} /> Add {ADD_LABEL[activeTab]}</button>
      </div>

      <div className="flex flex-wrap gap-2 mb-4">
        {tabs.map(tab => <button key={tab} onClick={() => setActiveTab(tab)} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>{TAB_LABEL[tab]}</button>)}
      </div>

      {activeTab === "grades" && (
        <DataGrid columns={gradeCols} data={gradesData?.data?.data?.items ?? []} loading={gradesLoading}
          sortBy={gradeGrid.sortBy} sortDescending={gradeGrid.sortDescending} onSort={gradeGrid.handleSort}
          search={gradeGrid.search} onSearch={gradeGrid.setSearch} searchPlaceholder="Search salary grades..."
          page={gradeGrid.page} totalPages={gradesData?.data?.data?.totalPages ?? 1} onPageChange={gradeGrid.setPage}
          pageSize={gradeGrid.pageSize} onPageSizeChange={gradeGrid.setPageSize} totalCount={gradesData?.data?.data?.totalCount ?? 0}
          onEdit={openEditGrade} onDelete={row => setGradeDeleteTarget(row)} emptyMessage="No salary grades found"
          actions={<ExportMenu baseUrl="payroll/salary-grades" filters={{ search: gradeGrid.search || undefined }} />} />
      )}
      {activeTab === "components" && (
        <DataGrid columns={compCols} data={compsData?.data?.data?.items ?? []} loading={compsLoading}
          sortBy={compGrid.sortBy} sortDescending={compGrid.sortDescending} onSort={compGrid.handleSort}
          search={compGrid.search} onSearch={compGrid.setSearch} searchPlaceholder="Search salary components..."
          page={compGrid.page} totalPages={compsData?.data?.data?.totalPages ?? 1} onPageChange={compGrid.setPage}
          pageSize={compGrid.pageSize} onPageSizeChange={compGrid.setPageSize} totalCount={compsData?.data?.data?.totalCount ?? 0}
          onEdit={openEditComponent} onDelete={row => setCompDeleteTarget(row)} emptyMessage="No salary components found"
          actions={<ExportMenu baseUrl="payroll/salary-components" filters={{ search: compGrid.search || undefined }} />} />
      )}
      {activeTab === "structures" && (
        <DataGrid columns={structureCols} data={structuresData?.data?.data?.items ?? []} loading={structuresLoading}
          sortBy={structureGrid.sortBy} sortDescending={structureGrid.sortDescending} onSort={structureGrid.handleSort}
          search={structureGrid.search} onSearch={structureGrid.setSearch} searchPlaceholder="Search by employee name or ID..."
          page={structureGrid.page} totalPages={structuresData?.data?.data?.totalPages ?? 1} onPageChange={structureGrid.setPage}
          pageSize={structureGrid.pageSize} onPageSizeChange={structureGrid.setPageSize} totalCount={structuresData?.data?.data?.totalCount ?? 0}
          onEdit={row => row.isActive && setStructureModal({ mode: "edit", structure: row })}
          emptyMessage={showStructureHistory ? "No salary structures found" : "No salary structures yet — use Add Salary Structure to set an employee's pay"}
          toolbarPrefix={
            <label className="flex items-center gap-1.5 text-xs text-gray-600 whitespace-nowrap">
              <input type="checkbox" checked={showStructureHistory} onChange={e => { setShowStructureHistory(e.target.checked); structureGrid.setPage(1); }} />
              <History size={13} /> Include history
            </label>
          } />
      )}
      {activeTab === "cycles" && (
        <DataGrid columns={cycleCols} data={cyclesData?.data?.data?.items ?? []} loading={cyclesLoading}
          page={cycleGrid.page} totalPages={cyclesData?.data?.data?.totalPages ?? 1} onPageChange={cycleGrid.setPage}
          pageSize={cycleGrid.pageSize} onPageSizeChange={cycleGrid.setPageSize} totalCount={cyclesData?.data?.data?.totalCount ?? 0}
          onView={row => setViewCycle(row)} onDelete={row => setCycleDeleteTarget(row)} emptyMessage="No payroll cycles found"
          actions={<ExportMenu baseUrl="payroll/cycles" />} />
      )}
      {activeTab === "expenses" && (
        <DataGrid columns={expCols} data={expData?.data?.data?.items ?? []} loading={expLoading}
          sortBy={expGrid.sortBy} sortDescending={expGrid.sortDescending} onSort={expGrid.handleSort}
          search={expGrid.search} onSearch={expGrid.setSearch} searchPlaceholder="Search expenses..."
          page={expGrid.page} totalPages={expData?.data?.data?.totalPages ?? 1} onPageChange={expGrid.setPage}
          pageSize={expGrid.pageSize} onPageSizeChange={expGrid.setPageSize} totalCount={expData?.data?.data?.totalCount ?? 0}
          emptyMessage="No expenses found"
          actions={<ExportMenu baseUrl="payroll/expenses" filters={{ search: expGrid.search || undefined }} />} />
      )}

      <Modal title={activeTab === "grades" ? (editingGrade ? "Edit Salary Grade" : "Add Salary Grade") : activeTab === "components" ? (editingComponent ? "Edit Salary Component" : "Add Salary Component") : activeTab === "cycles" ? "Add Payroll Cycle" : "Submit Expense"} open={modal} onClose={closeModal}>
        {activeTab === "grades" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Name *</label>
                <input className={`${inputCls} ${gradeErrors.name ? "border-red-400" : ""}`} value={gradeForm.name} onChange={e => { setGradeForm(f => ({ ...f, name: e.target.value })); setGradeErrors(p => ({ ...p, name: undefined })); }} />
                {gradeErrors.name && <p className="text-xs text-red-500 mt-1">{gradeErrors.name}</p>}
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Code *</label>
                <input className={`${inputCls} ${gradeErrors.gradeCode ? "border-red-400" : ""}`} value={gradeForm.gradeCode} onChange={e => { setGradeForm(f => ({ ...f, gradeCode: e.target.value })); setGradeErrors(p => ({ ...p, gradeCode: undefined })); }} />
                {gradeErrors.gradeCode && <p className="text-xs text-red-500 mt-1">{gradeErrors.gradeCode}</p>}
              </div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><input className={inputCls} value={gradeForm.description} onChange={e => setGradeForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Min Gross Salary</label><input type="number" className={inputCls} value={gradeForm.minSalary} onChange={e => setGradeForm(f => ({ ...f, minSalary: Number(e.target.value) }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Max Gross Salary</label><input type="number" className={inputCls} value={gradeForm.maxSalary} onChange={e => setGradeForm(f => ({ ...f, maxSalary: Number(e.target.value) }))} /></div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Status</label><select className={inputCls} value={String(gradeForm.isActive)} onChange={e => setGradeForm(f => ({ ...f, isActive: e.target.value === "true" }))}><option value="true">Active</option><option value="false">Inactive</option></select></div>
            {saveRow(() => { if (validateGrade()) gradeMutation.mutate(); }, gradeMutation.isPending)}
          </div>
        )}
        {activeTab === "components" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Name *</label><input className={inputCls} value={compForm.name} onChange={e => setCompForm(f => ({ ...f, name: e.target.value }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Code *</label><input className={inputCls} value={compForm.code} onChange={e => setCompForm(f => ({ ...f, code: e.target.value }))} /></div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Type</label>
                <select className={inputCls} value={compForm.componentType} onChange={e => setCompForm(f => ({ ...f, componentType: e.target.value }))}>
                  {COMPONENT_TYPES.map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                </select>
              </div>
              <div><label className="text-xs text-gray-500 mb-1 block">Calculated as</label>
                <select className={inputCls} value={compForm.calculationType} onChange={e => setCompForm(f => ({ ...f, calculationType: e.target.value }))}>
                  <option value="Fixed">Fixed amount</option>
                  <option value="PercentageOfBasic">% of basic salary</option>
                  {DEDUCTION_COMPONENT_TYPES.includes(compForm.componentType) && <option value="PercentageOfGross">% of gross salary</option>}
                </select>
              </div>
            </div>
            <div className="grid grid-cols-3 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Default {compForm.calculationType === "Fixed" ? "amount" : "%"}</label><input type="number" min={0} className={inputCls} value={compForm.defaultValue} onChange={e => setCompForm(f => ({ ...f, defaultValue: Number(e.target.value) }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Payslip order</label><input type="number" className={inputCls} value={compForm.sortOrder} onChange={e => setCompForm(f => ({ ...f, sortOrder: Number(e.target.value) }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Taxable</label><select className={inputCls} value={String(compForm.isTaxable)} onChange={e => setCompForm(f => ({ ...f, isTaxable: e.target.value === "true" }))}><option value="true">Yes</option><option value="false">No</option></select></div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Status</label><select className={inputCls} value={String(compForm.isActive)} onChange={e => setCompForm(f => ({ ...f, isActive: e.target.value === "true" }))}><option value="true">Active</option><option value="false">Inactive</option></select></div>
            <p className="text-xs text-gray-400">The default is pre-filled when you add this component to an employee's salary structure; it can be changed per employee.</p>
            {saveRow(() => compMutation.mutate(), compMutation.isPending)}
          </div>
        )}
        {activeTab === "cycles" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Month *</label>
                <input type="month" className={inputCls} value={cycleMonth} onChange={e => { setCycleMonth(e.target.value); if (e.target.value) setCycleForm(cycleForMonth(e.target.value)); }} />
              </div>
              <div><label className="text-xs text-gray-500 mb-1 block">Cycle Name *</label><input className={inputCls} value={cycleForm.title} onChange={e => setCycleForm(f => ({ ...f, title: e.target.value }))} /></div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Period Start</label><DateInput className={inputCls} value={cycleForm.startDate} onChange={e => setCycleForm(f => ({ ...f, startDate: e.target.value }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Period End</label><DateInput className={inputCls} value={cycleForm.endDate} onChange={e => setCycleForm(f => ({ ...f, endDate: e.target.value }))} /></div>
            </div>
            <p className="text-xs text-gray-400">Attendance, leave and overtime inside this period are used when the payroll is processed.</p>
            {saveRow(() => cycleMutation.mutate(), cycleMutation.isPending, "Create")}
          </div>
        )}
        {activeTab === "expenses" && (
          <div className="space-y-3">
            <div><SearchableDropdown label="Employee" options={empOptions} value={expForm.employeeId} onChange={v => setExpForm(f => ({ ...f, employeeId: v || "" }))} placeholder="Select Employee" searchPlaceholder="Search employee..." loading={empLoading} required /></div>
            <div className="grid grid-cols-2 gap-3">
              <div><label className="text-xs text-gray-500 mb-1 block">Amount</label><input type="number" className={inputCls} value={expForm.amount} onChange={e => setExpForm(f => ({ ...f, amount: Number(e.target.value) }))} /></div>
              <div><label className="text-xs text-gray-500 mb-1 block">Category</label><select className={inputCls} value={expForm.category} onChange={e => setExpForm(f => ({ ...f, category: e.target.value }))}><option value="">Select</option><option value="Travel">Travel</option><option value="Food">Food</option><option value="Accommodation">Accommodation</option><option value="Other">Other</option></select></div>
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className={inputCls} value={expForm.description} onChange={e => setExpForm(f => ({ ...f, description: e.target.value }))} /></div>
            {saveRow(() => expMutation.mutate(), expMutation.isPending, "Submit")}
          </div>
        )}
      </Modal>

      {structureModal && (
        <SalaryStructureModal
          mode={structureModal.mode}
          structure={structureModal.structure}
          onClose={() => setStructureModal(null)}
          onSaved={() => {
            setStructureModal(null);
            qc.invalidateQueries({ queryKey: ["salary-structures"] });
            qc.invalidateQueries({ queryKey: ["employees"] });
          }}
        />
      )}
      {viewCycle && <PayrollCycleView cycle={viewCycle} onClose={() => setViewCycle(null)} />}

      <ConfirmDialog open={!!gradeDeleteTarget} title="Delete Salary Grade"
        message={<span>Are you sure you want to delete <strong>{gradeDeleteTarget?.name}</strong>? This action cannot be undone.</span>}
        confirmLabel="Delete" tone="danger" loading={gradeDeleteMutation.isPending}
        onConfirm={() => { if (gradeDeleteTarget) gradeDeleteMutation.mutate(gradeDeleteTarget.id); }} onCancel={() => setGradeDeleteTarget(null)} />
      <ConfirmDialog open={!!compDeleteTarget} title="Delete Salary Component"
        message={<span>Are you sure you want to delete <strong>{compDeleteTarget?.name}</strong>? This action cannot be undone.</span>}
        confirmLabel="Delete" tone="danger" loading={compDeleteMutation.isPending}
        onConfirm={() => { if (compDeleteTarget) compDeleteMutation.mutate(compDeleteTarget.id); }} onCancel={() => setCompDeleteTarget(null)} />
      <ConfirmDialog open={!!cycleDeleteTarget} title="Delete Payroll Cycle"
        message={<span>Delete <strong>{cycleDeleteTarget?.title}</strong>? Only draft or cancelled cycles can be deleted.</span>}
        confirmLabel="Delete" tone="danger" loading={cycleDeleteMutation.isPending}
        onConfirm={() => { if (cycleDeleteTarget) cycleDeleteMutation.mutate(cycleDeleteTarget.id); }} onCancel={() => setCycleDeleteTarget(null)} />
    </div>
  );
}
