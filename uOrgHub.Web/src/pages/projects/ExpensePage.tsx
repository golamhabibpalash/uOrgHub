import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import Modal from "../../components/shared/Modal";
import {
  getExpenses,
  createExpense,
  approveExpense,
  getProjectBudgetSummary,
} from "../../api/projects";

export default function ExpensePage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState({
    expenseDate: "",
    expenseType: "",
    description: "",
    amount: 0,
    vendorId: "",
    invoiceNumber: "",
    notes: "",
  });

  const { data: budgetData } = useQuery({
    queryKey: ["projectBudget", projectId],
    queryFn: () => getProjectBudgetSummary(projectId!),
    enabled: !!projectId,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["expenses", projectId, page],
    queryFn: () => getExpenses(projectId!, { page, pageSize: 10 }),
    enabled: !!projectId,
  });

  const expenses = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createExpense(projectId!, form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["expenses", projectId] });
      closeModal();
    },
  });

  const approveMutation = useMutation({
    mutationFn: (expenseId: string) => approveExpense(projectId!, expenseId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["expenses", projectId] }),
  });

  function openModal() {
    setForm({
      expenseDate: new Date().toISOString().split("T")[0],
      expenseType: "",
      description: "",
      amount: 0,
      vendorId: "",
      invoiceNumber: "",
      notes: "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setForm({
      expenseDate: "",
      expenseType: "",
      description: "",
      amount: 0,
      vendorId: "",
      invoiceNumber: "",
      notes: "",
    });
  }

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Draft: "bg-gray-100 text-gray-600",
      Submitted: "bg-blue-50 text-blue-700",
      Approved: "bg-green-50 text-green-700",
      Rejected: "bg-red-50 text-red-700",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

  const getTypeBadge = (type: string) => {
    const styles: Record<string, string> = {
      Material: "bg-blue-50 text-blue-700",
      Labor: "bg-green-50 text-green-700",
      Equipment: "bg-amber-50 text-amber-700",
      Transport: "bg-purple-50 text-purple-700",
      Other: "bg-gray-100 text-gray-600",
    };
    return styles[type] || "bg-gray-100 text-gray-600";
  };

  const totalExpenses = expenses.reduce((sum, e) => sum + e.amount, 0);
  const budget = budgetData?.data?.data;

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

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Project Expenses</h2>
          <p className="text-xs text-gray-400">Track project expenses</p>
        </div>
        <button
          onClick={openModal}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Expense
        </button>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
          <p className="text-xs text-gray-500 mb-1">Budget</p>
          <p className="text-2xl font-medium text-gray-900">
            BDT {(budget?.contractValue ?? 0).toLocaleString()}
          </p>
        </div>
        <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
          <p className="text-xs text-gray-500 mb-1">Total Expenses</p>
          <p className="text-2xl font-medium text-gray-900">
            BDT {totalExpenses.toLocaleString()}
          </p>
        </div>
        <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
          <p className="text-xs text-gray-500 mb-1">Remaining</p>
          <p className="text-2xl font-medium text-green-600">
            BDT {((budget?.remainingBudget ?? 0)).toLocaleString()}
          </p>
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : expenses.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No expenses found</div>
        ) : (
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Expense No
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Date
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Type
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Description
                </th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">
                  Amount
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Vendor
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Status
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {expenses.map((exp) => (
                <tr key={exp.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 text-gray-700 font-medium">
                    {exp.expenseNumber}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">
                    {new Date(exp.expenseDate).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${getTypeBadge(exp.expenseType)}`}>
                      {exp.expenseType}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">{exp.description}</td>
                  <td className="px-4 py-2.5 text-right font-medium text-gray-900">
                    BDT {exp.amount.toLocaleString()}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">{exp.vendorName || "-"}</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(exp.status)}`}>
                      {exp.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    {exp.status === "Draft" && (
                      <button
                        onClick={() => approveMutation.mutate(exp.id)}
                        disabled={approveMutation.isPending}
                        className="text-xs text-green-600 hover:text-green-700"
                      >
                        Approve
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title="Add Expense" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Expense Date *</label>
              <input
                type="date"
                value={form.expenseDate}
                onChange={(e) => setForm((f) => ({ ...f, expenseDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Expense Type *</label>
              <select
                value={form.expenseType}
                onChange={(e) => setForm((f) => ({ ...f, expenseType: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              >
                <option value="">Select Type</option>
                <option value="Material">Material</option>
                <option value="Labor">Labor</option>
                <option value="Equipment">Equipment</option>
                <option value="Transport">Transport</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <input
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Amount *</label>
              <input
                type="number"
                value={form.amount}
                onChange={(e) =>
                  setForm((f) => ({ ...f, amount: parseFloat(e.target.value) || 0 }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Invoice Number</label>
              <input
                value={form.invoiceNumber}
                onChange={(e) => setForm((f) => ({ ...f, invoiceNumber: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Vendor</label>
            <select
              value={form.vendorId}
              onChange={(e) => setForm((f) => ({ ...f, vendorId: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              <option value="">Select Vendor</option>
              <option value="1">Vendor 1</option>
            </select>
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
              onClick={() => createMutation.mutate()}
              disabled={createMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {createMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}