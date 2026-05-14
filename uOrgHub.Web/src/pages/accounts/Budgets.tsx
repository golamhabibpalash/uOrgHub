import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, CheckCircle, Trash2, ChevronDown, ChevronUp } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getBudgets,
  createBudget,
  updateBudget,
  deleteBudget,
  approveBudget,
  getFiscalYears,
  getChartOfAccounts,
  getCostCenters,
  type Budget,
  BudgetStatus,
} from "../../api/accounts";

const statusColors: Record<BudgetStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Approved: "bg-blue-50 text-blue-700",
  Active: "bg-green-50 text-green-700",
  Closed: "bg-gray-100 text-gray-400",
  Cancelled: "bg-red-50 text-red-500",
};

export default function Budgets() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Budget | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [form, setForm] = useState({
    name: "",
    description: "",
    fiscalYearId: "",
    costCenterId: "",
    lines: [{ accountId: "", costCenterId: "", period: 0, plannedAmount: 0 }],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["budgets", page, search],
    queryFn: () => getBudgets({ page, pageSize: 10, search }),
  });

  const { data: fiscalYearsData } = useQuery({ queryKey: ["fiscal-years", 1, ""], queryFn: () => getFiscalYears({ page: 1, pageSize: 50 }) });
  const { data: accountsData } = useQuery({ queryKey: ["chart-of-accounts", 1, ""], queryFn: () => getChartOfAccounts({ page: 1, pageSize: 200 }) });
  const { data: costCentersData } = useQuery({ queryKey: ["cost-centers", 1, ""], queryFn: () => getCostCenters({ page: 1, pageSize: 100 }) });

  const budgets = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const fiscalYears = fiscalYearsData?.data?.data?.items ?? [];
  const coaAccounts = accountsData?.data?.data?.items ?? [];
  const costCenters = costCentersData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () => {
      if (editing) {
        return updateBudget(editing.id, { name: form.name, description: form.description || undefined, costCenterId: form.costCenterId || undefined });
      }
      const payload = { ...form, costCenterId: form.costCenterId || undefined, lines: form.lines.map((l) => ({ ...l, costCenterId: l.costCenterId || undefined })) };
      return createBudget(payload as Parameters<typeof createBudget>[0]);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["budgets"] }); closeModal(); },
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => approveBudget(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["budgets"] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteBudget(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["budgets"] }),
  });

  function openAdd() {
    setEditing(null);
    const currentFY = fiscalYears.find((fy) => fy.isCurrent);
    setForm({ name: "", description: "", fiscalYearId: currentFY?.id ?? fiscalYears[0]?.id ?? "", costCenterId: "", lines: [{ accountId: coaAccounts[0]?.id ?? "", costCenterId: "", period: 0, plannedAmount: 0 }] });
    setModal(true);
  }

  function openEdit(b: Budget) {
    setEditing(b);
    setForm({ name: b.name, description: b.description ?? "", fiscalYearId: b.fiscalYearId, costCenterId: b.costCenterId ?? "", lines: [] });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  function addLine() {
    setForm((f) => ({ ...f, lines: [...f.lines, { accountId: coaAccounts[0]?.id ?? "", costCenterId: "", period: 0, plannedAmount: 0 }] }));
  }

  function removeLine(idx: number) {
    setForm((f) => ({ ...f, lines: f.lines.filter((_, i) => i !== idx) }));
  }

  function updateLine(idx: number, field: string, value: string | number) {
    setForm((f) => ({ ...f, lines: f.lines.map((l, i) => i === idx ? { ...l, [field]: value } : l) }));
  }

  const totalPlanned = form.lines.reduce((s, l) => s + (l.plannedAmount || 0), 0);

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Budgets</h2>
          <p className="text-xs text-gray-400">Plan and track financial budgets</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Budget
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search budgets..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center h-40 text-sm text-gray-400">Loading...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-50">
                  {["Budget Name", "Status", "Total Amount", "Fiscal Year", "Actions"].map((h) => (
                    <th key={h} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {budgets.length === 0 ? (
                  <tr><td colSpan={5} className="text-center py-10 text-gray-400">No budgets found</td></tr>
                ) : budgets.map((budget) => (
                  <>
                    <tr key={budget.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5 font-medium">{budget.name}</td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[budget.status]}`}>{budget.status}</span>
                      </td>
                      <td className="px-4 py-2.5">{budget.totalAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-gray-500">{fiscalYears.find((fy) => fy.id === budget.fiscalYearId)?.name ?? budget.fiscalYearId}</td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <button onClick={() => setExpandedId(expandedId === budget.id ? null : budget.id)} className="text-gray-400 hover:text-primary-600">
                            {expandedId === budget.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                          </button>
                          {budget.status === "Draft" && (
                            <>
                              <button onClick={() => openEdit(budget)} className="text-gray-400 hover:text-primary-600" title="Edit"><Plus size={13} className="rotate-45" /></button>
                              <button onClick={() => approveMutation.mutate(budget.id)} className="text-green-500 hover:text-green-700" title="Approve"><CheckCircle size={13} /></button>
                              <button onClick={() => deleteMutation.mutate(budget.id)} className="text-red-400 hover:text-red-600" title="Delete"><Trash2 size={13} /></button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                    {expandedId === budget.id && (
                      <tr key={`${budget.id}-lines`} className="bg-gray-50">
                        <td colSpan={5} className="px-6 py-3">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="text-gray-500">
                                <th className="text-left pb-1">Account</th>
                                <th className="text-right pb-1">Period</th>
                                <th className="text-right pb-1">Planned</th>
                                <th className="text-right pb-1">Actual</th>
                                <th className="text-right pb-1">Variance</th>
                              </tr>
                            </thead>
                            <tbody>
                              {budget.lines.map((line) => (
                                <tr key={line.id}>
                                  <td className="py-0.5">{line.accountName}</td>
                                  <td className="py-0.5 text-right">{line.period === 0 ? "Annual" : `Month ${line.period}`}</td>
                                  <td className="py-0.5 text-right">{line.plannedAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                                  <td className="py-0.5 text-right">{line.actualAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                                  <td className={`py-0.5 text-right font-medium ${(line.plannedAmount - line.actualAmount) >= 0 ? "text-green-700" : "text-red-600"}`}>
                                    {(line.plannedAmount - line.actualAmount).toLocaleString("en-BD", { minimumFractionDigits: 2 })}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </td>
                      </tr>
                    )}
                  </>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Budget" : "New Budget"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Budget Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Fiscal Year *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.fiscalYearId} onChange={(e) => setForm((f) => ({ ...f, fiscalYearId: e.target.value }))} disabled={!!editing}>
                <option value="">Select year</option>
                {fiscalYears.map((fy) => <option key={fy.id} value={fy.id}>{fy.name}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Cost Center</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.costCenterId} onChange={(e) => setForm((f) => ({ ...f, costCenterId: e.target.value }))}>
                <option value="">None</option>
                {costCenters.map((cc) => <option key={cc.id} value={cc.id}>{cc.name}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>

          {!editing && (
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-xs text-gray-500">Budget Lines</label>
                <button onClick={addLine} className="text-xs text-primary-600 hover:underline">+ Add Line</button>
              </div>
              <div className="border border-gray-200 rounded-lg overflow-hidden">
                <table className="w-full text-xs">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="text-left px-2 py-1.5 text-gray-500">Account</th>
                      <th className="text-left px-2 py-1.5 text-gray-500">Period</th>
                      <th className="text-right px-2 py-1.5 text-gray-500">Planned Amount</th>
                      <th className="px-2 py-1.5"></th>
                    </tr>
                  </thead>
                  <tbody>
                    {form.lines.map((line, idx) => (
                      <tr key={idx} className="border-t border-gray-100">
                        <td className="px-2 py-1">
                          <select className="w-32 border border-gray-200 rounded px-1 py-1 text-xs" value={line.accountId} onChange={(e) => updateLine(idx, "accountId", e.target.value)}>
                            <option value="">Select</option>
                            {coaAccounts.map((a) => <option key={a.id} value={a.id}>{a.accountCode} — {a.accountName}</option>)}
                          </select>
                        </td>
                        <td className="px-2 py-1">
                          <select className="w-24 border border-gray-200 rounded px-1 py-1 text-xs" value={line.period} onChange={(e) => updateLine(idx, "period", parseInt(e.target.value))}>
                            <option value={0}>Annual</option>
                            {Array.from({ length: 12 }, (_, i) => <option key={i + 1} value={i + 1}>Month {i + 1}</option>)}
                          </select>
                        </td>
                        <td className="px-2 py-1">
                          <input type="number" min={0} className="w-28 border border-gray-200 rounded px-1 py-1 text-xs text-right" value={line.plannedAmount || ""} onChange={(e) => updateLine(idx, "plannedAmount", parseFloat(e.target.value) || 0)} />
                        </td>
                        <td className="px-2 py-1">
                          {form.lines.length > 1 && <button onClick={() => removeLine(idx)} className="text-red-400 hover:text-red-600">×</button>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot className="bg-gray-50 border-t border-gray-200">
                    <tr>
                      <td colSpan={2} className="px-2 py-1.5 text-xs text-gray-500 font-medium text-right">Total</td>
                      <td className="px-2 py-1.5 text-xs font-medium text-right">{totalPlanned.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td />
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
