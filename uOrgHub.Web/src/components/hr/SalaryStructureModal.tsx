import { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import Modal from "../shared/Modal";
import SearchableDropdown from "../shared/SearchableDropdown";
import DateInput from "../shared/DateInput";
import { useEmployeeLookup, useSalaryGradeLookup } from "../../hooks/useEntityLookup";
import {
  getAllSalaryGrades,
  getSalaryComponents,
  createSalaryStructure,
  updateSalaryStructure,
  DEDUCTION_COMPONENT_TYPES,
  SalaryComponent,
  SalaryStructure,
} from "../../api/hr";

const inputCls = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";
const money = (n: number) => n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

interface Props {
  /**
   * "new" starts a salary structure (a first one, or a revision from a later date);
   * "edit" corrects the current one in place.
   */
  mode: "new" | "edit";
  /** The current structure — required for "edit", optional for a "new" revision (pre-fills it). */
  structure?: SalaryStructure | null;
  onClose: () => void;
  onSaved: () => void;
}

interface Row { salaryComponentId: string; value: number }

/**
 * Same rules as the backend PayrollCalculator.Monthly, so the totals shown while editing match
 * what gets saved. Allowances may be fixed or % of basic; deductions may also be % of gross.
 */
function amountFor(c: SalaryComponent, value: number, basic: number, gross: number) {
  if (c.calculationType === "PercentageOfBasic") return (basic * value) / 100;
  if (c.calculationType === "PercentageOfGross") return (gross * value) / 100;
  return value;
}

export default function SalaryStructureModal({ mode, structure, onClose, onSaved }: Props) {
  const { options: empOptions, isLoading: empLoading } = useEmployeeLookup();
  const { options: gradeOptions, isLoading: gradeLoading } = useSalaryGradeLookup();
  const { data: gradesData } = useQuery({ queryKey: ["salary-grades-all"], queryFn: getAllSalaryGrades, staleTime: 60000 });
  const { data: compsData } = useQuery({
    queryKey: ["salary-components-all"],
    queryFn: () => getSalaryComponents({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });

  const [employeeId, setEmployeeId] = useState(structure?.employeeId ?? "");
  const [effectiveDate, setEffectiveDate] = useState(new Date().toISOString().split("T")[0]);
  const [salaryGradeId, setSalaryGradeId] = useState(structure?.salaryGradeId ?? "");
  const [basicSalary, setBasicSalary] = useState(structure?.basicSalary ?? 0);
  const [rows, setRows] = useState<Row[]>(
    structure?.components.map(c => ({ salaryComponentId: c.salaryComponentId, value: c.value })) ?? [],
  );

  // Basic salary is entered on the structure itself, so basic-salary components aren't offered.
  const allComponents = useMemo(
    () => (compsData?.data?.data?.items ?? []).filter(c => c.isActive && c.componentType !== "BasicSalary"),
    [compsData],
  );
  const byId = useMemo(() => new Map(allComponents.map(c => [c.id, c])), [allComponents]);
  const addableOptions = allComponents
    .filter(c => !rows.some(r => r.salaryComponentId === c.id))
    .map(c => ({ value: c.id, label: `${c.name} (${DEDUCTION_COMPONENT_TYPES.includes(c.componentType) ? "deduction" : "allowance"})` }));

  const grade = gradesData?.data?.data?.find(g => g.id === salaryGradeId);

  const totals = useMemo(() => {
    const isDeduction = (c: SalaryComponent) => DEDUCTION_COMPONENT_TYPES.includes(c.componentType);
    const earnings = rows
      .map(r => ({ r, c: byId.get(r.salaryComponentId) }))
      .filter(x => x.c && !isDeduction(x.c))
      .reduce((sum, x) => sum + amountFor(x.c!, x.r.value, basicSalary, 0), 0);
    const gross = basicSalary + earnings;
    const deductions = rows
      .map(r => ({ r, c: byId.get(r.salaryComponentId) }))
      .filter(x => x.c && isDeduction(x.c))
      .reduce((sum, x) => sum + amountFor(x.c!, x.r.value, basicSalary, gross), 0);
    return { earnings, gross, deductions, net: Math.max(0, gross - deductions) };
  }, [rows, byId, basicSalary]);

  const saveMutation = useMutation({
    mutationFn: () => {
      const body = { salaryGradeId, basicSalary, components: rows };
      return mode === "edit" && structure
        ? updateSalaryStructure(structure.id, body)
        : createSalaryStructure({ ...body, employeeId, effectiveDate });
    },
    onSuccess: () => { toast.success("Salary structure saved."); onSaved(); },
  });

  function submit() {
    if (!employeeId || !salaryGradeId || basicSalary <= 0 || (mode === "new" && !effectiveDate)) {
      toast.error("Employee, grade, basic salary and effective date are required.");
      return;
    }
    const badAllowance = rows
      .map(r => byId.get(r.salaryComponentId))
      .find(c => c && !DEDUCTION_COMPONENT_TYPES.includes(c.componentType) && c.calculationType === "PercentageOfGross");
    if (badAllowance) {
      toast.error(`"${badAllowance.name}" is an allowance set as % of gross. Change it to fixed or % of basic.`);
      return;
    }
    saveMutation.mutate();
  }

  const title = mode === "edit" ? "Edit Salary Structure" : structure ? "Revise Salary (new structure)" : "Set Salary Structure";

  return (
    <Modal title={title} open onClose={onClose} size="3xl">
      <div className="space-y-4">
        {mode === "new" && structure && (
          <p className="text-xs text-gray-500 bg-blue-50 border border-blue-100 rounded-lg px-3 py-2">
            The current structure (from {new Date(structure.effectiveDate).toLocaleDateString()}) will end the day before the new effective date and stay in history.
          </p>
        )}
        <div className="grid grid-cols-2 gap-3">
          <SearchableDropdown label="Employee *" options={empOptions} value={employeeId} onChange={v => setEmployeeId(v || "")}
            placeholder="Select Employee" searchPlaceholder="Search employee..." loading={empLoading} disabled={!!structure} required />
          {mode === "new" ? (
            <div><label className="text-xs text-gray-500 mb-1 block">Effective From *</label><DateInput className={inputCls} value={effectiveDate} onChange={e => setEffectiveDate(e.target.value)} /></div>
          ) : (
            <div><label className="text-xs text-gray-500 mb-1 block">Effective From</label><div className={`${inputCls} bg-gray-50 text-gray-500`}>{structure && new Date(structure.effectiveDate).toLocaleDateString()}</div></div>
          )}
        </div>
        <div className="grid grid-cols-2 gap-3">
          <SearchableDropdown label="Salary Grade *" options={gradeOptions} value={salaryGradeId} onChange={v => setSalaryGradeId(v || "")}
            placeholder="Select Grade" searchPlaceholder="Search grades..." loading={gradeLoading} required />
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Basic Salary (monthly) *</label>
            <input type="number" min={0} className={inputCls} value={basicSalary} onChange={e => setBasicSalary(Number(e.target.value))} />
            {grade && (totals.gross < grade.minSalary || totals.gross > grade.maxSalary) && (
              <p className="text-xs text-amber-600 mt-1">Gross {money(totals.gross)} is outside {grade.gradeCode} range ({money(grade.minSalary)}–{money(grade.maxSalary)})</p>
            )}
          </div>
        </div>

        <div>
          <div className="flex items-end justify-between gap-3 mb-2">
            <h4 className="text-sm font-medium text-gray-700">Allowances &amp; Deductions</h4>
            <div className="w-72">
              <SearchableDropdown options={addableOptions} value="" placeholder="+ Add component"
                searchPlaceholder="Search components..."
                onChange={v => {
                  const c = v ? byId.get(v) : undefined;
                  if (c) setRows(r => [...r, { salaryComponentId: c.id, value: c.defaultValue ?? 0 }]);
                }} />
            </div>
          </div>
          <div className="border border-gray-100 rounded-lg overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500">
                <tr><th className="text-left px-3 py-2">Component</th><th className="text-left px-3 py-2">Basis</th><th className="text-right px-3 py-2 w-32">Value</th><th className="text-right px-3 py-2">Monthly</th><th className="w-10" /></tr>
              </thead>
              <tbody>
                {rows.length === 0 && (
                  <tr><td colSpan={5} className="px-3 py-4 text-center text-xs text-gray-400">No components — the employee is paid basic salary only.</td></tr>
                )}
                {rows.map((r, i) => {
                  const c = byId.get(r.salaryComponentId);
                  if (!c) return null;
                  const deduction = DEDUCTION_COMPONENT_TYPES.includes(c.componentType);
                  const amount = amountFor(c, r.value, basicSalary, totals.gross);
                  return (
                    <tr key={r.salaryComponentId} className="border-t border-gray-100">
                      <td className="px-3 py-2">{c.name} <span className={`ml-1 text-xs ${deduction ? "text-red-500" : "text-green-600"}`}>{deduction ? "deduction" : "allowance"}</span></td>
                      <td className="px-3 py-2 text-xs text-gray-500">{c.calculationType === "PercentageOfBasic" ? "% of basic" : c.calculationType === "PercentageOfGross" ? "% of gross" : "Fixed amount"}</td>
                      <td className="px-3 py-2"><input type="number" min={0} className={`${inputCls} text-right`} value={r.value}
                        onChange={e => setRows(all => all.map((x, j) => (j === i ? { ...x, value: Number(e.target.value) } : x)))} /></td>
                      <td className={`px-3 py-2 text-right ${deduction ? "text-red-600" : ""}`}>{deduction ? "−" : ""}{money(amount)}</td>
                      <td className="px-2"><button onClick={() => setRows(all => all.filter((_, j) => j !== i))} className="text-gray-400 hover:text-red-600" title="Remove"><Trash2 size={15} /></button></td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>

        <div className="grid grid-cols-4 gap-3 text-sm">
          {[["Basic", basicSalary], ["Gross", totals.gross], ["Deductions", totals.deductions], ["Net (monthly)", totals.net]].map(([label, value]) => (
            <div key={label as string} className="rounded-lg bg-gray-50 border border-gray-100 px-3 py-2">
              <div className="text-xs text-gray-500">{label}</div>
              <div className="font-semibold text-gray-900">{money(value as number)}</div>
            </div>
          ))}
        </div>

        <div className="flex justify-end gap-2 pt-1">
          <button onClick={onClose} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
          <button onClick={submit} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{saveMutation.isPending ? "Saving..." : "Save"}</button>
        </div>
      </div>
    </Modal>
  );
}
