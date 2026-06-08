import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import ProjectNav from "../../components/projects/ProjectNav";
import { useFiscalYearLookup } from "../../hooks/useEntityLookup";
import {
  getProjectBudgets,
  createProjectBudget,
  updateProjectBudget,
  deleteProjectBudget,
  ProjectBudget,
} from "../../api/projects";

const BUDGET_TYPES = ["Labor", "Material", "Equipment", "Subcontract", "Overhead", "Contingency"];

const budgetTypeOptions = BUDGET_TYPES.map((t) => ({ value: t, label: t }));

export default function ProjectBudgetPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<ProjectBudget | null>(null);
  const [form, setForm] = useState({
    budgetType: "Labor",
    fiscalYearId: "",
    allocatedAmount: 0,
    revisedAmount: "",
    notes: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["project-budgets", projectId],
    queryFn: () => getProjectBudgets(projectId!, { page: 1, pageSize: 50 }),
    enabled: !!projectId,
  });

  const budgets = data?.data?.data?.items ?? [];

  const { options: fyOptions } = useFiscalYearLookup();

  const saveMutation = useMutation({
    mutationFn: () =>
      editing
        ? updateProjectBudget(editing.id, {
            allocatedAmount: form.allocatedAmount,
            revisedAmount: form.revisedAmount ? parseFloat(form.revisedAmount) : undefined,
            notes: form.notes || undefined,
          })
        : createProjectBudget({
            projectId: projectId!,
            budgetType: form.budgetType,
            fiscalYearId: form.fiscalYearId || undefined,
            allocatedAmount: form.allocatedAmount,
            revisedAmount: form.revisedAmount ? parseFloat(form.revisedAmount) : undefined,
            notes: form.notes || undefined,
          }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["project-budgets", projectId] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteProjectBudget(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["project-budgets", projectId] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ budgetType: "Labor", fiscalYearId: "", allocatedAmount: 0, revisedAmount: "", notes: "" });
    setModal(true);
  }

  function openEdit(budget: ProjectBudget) {
    setEditing(budget);
    setForm({
      budgetType: budget.budgetType,
      fiscalYearId: budget.fiscalYearId || "",
      allocatedAmount: budget.allocatedAmount,
      revisedAmount: budget.revisedAmount ? String(budget.revisedAmount) : "",
      notes: budget.notes || "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const getBadge = (type: string) => {
    const styles: Record<string, string> = {
      Labor: "bg-blue-50 text-blue-700",
      Material: "bg-amber-50 text-amber-700",
      Equipment: "bg-purple-50 text-purple-700",
      Subcontract: "bg-cyan-50 text-cyan-700",
      Overhead: "bg-gray-50 text-gray-700",
      Contingency: "bg-red-50 text-red-600",
    };
    return styles[type] || "bg-gray-100 text-gray-600";
  };

  return (
    <div>
      <div className="mb-4">
        <Link
          to={`/projects/${projectId}`}
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft size={16} /> Back to Project
        </Link>
      </div>

      <ProjectNav />

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Project Budgets</h2>
          <p className="text-xs text-gray-400">Manage budget allocations by type</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Budget
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : budgets.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No budgets found</div>
        ) : (
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Allocated</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Revised</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Spent</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Remaining</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {budgets.map((b) => (
                <tr key={b.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${getBadge(b.budgetType)}`}>
                      {b.budgetType}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">
                    ${b.allocatedAmount.toLocaleString()}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">
                    {b.revisedAmount ? `$${b.revisedAmount.toLocaleString()}` : "-"}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">
                    ${b.spentAmount.toLocaleString()}
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={b.isOverBudget ? "text-red-600 font-medium" : "text-gray-700"}>
                      ${b.remainingAmount.toLocaleString()}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${b.isOverBudget ? "bg-red-50 text-red-700" : "bg-green-50 text-green-700"}`}>
                      {b.isOverBudget ? "Over Budget" : "On Track"}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => openEdit(b)}
                        className="text-xs text-gray-500 hover:text-primary-600"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => {
                          if (confirm("Delete this budget entry?")) deleteMutation.mutate(b.id);
                        }}
                        className="text-xs text-red-500 hover:text-red-700"
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal
        title={editing ? "Edit Budget" : "Add Budget"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div>
            <SearchableDropdown
              label="Budget Type *"
              options={budgetTypeOptions}
              value={form.budgetType}
              onChange={(v) => setForm((f) => ({ ...f, budgetType: v ?? "Labor" }))}
              placeholder="Select type"
              searchPlaceholder="Search types..."
              disabled={!!editing}
            />
          </div>
          <div>
            <SearchableDropdown
              label="Fiscal Year"
              options={fyOptions}
              value={form.fiscalYearId}
              onChange={(v) => setForm((f) => ({ ...f, fiscalYearId: v ?? "" }))}
              placeholder="Select fiscal year"
              searchPlaceholder="Search..."
              clearable
              disabled={!!editing}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Allocated Amount *</label>
            <input
              type="number"
              value={form.allocatedAmount}
              onChange={(e) => setForm((f) => ({ ...f, allocatedAmount: parseFloat(e.target.value) || 0 }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Revised Amount</label>
            <input
              type="number"
              value={form.revisedAmount}
              onChange={(e) => setForm((f) => ({ ...f, revisedAmount: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              placeholder="Optional"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea
              value={form.notes}
              onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
