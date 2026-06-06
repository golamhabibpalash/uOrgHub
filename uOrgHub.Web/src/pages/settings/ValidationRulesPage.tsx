import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Plus, ToggleLeft, ToggleRight } from "lucide-react";
import toast from "react-hot-toast";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import {
  getValidationRules,
  createValidationRule,
  updateValidationRule,
  deleteValidationRule,
  ValidationRule,
} from "../../api/settings";

const RULE_TYPES = ["Required", "Email", "Regex", "MinLength", "MaxLength", "Phone"];
const SEVERITIES = ["Error", "Warning", "Info"];
const ENTITY_TYPES = ["Employee", "Department", "Designation", "User", "LeaveRequest", "ExpenseRequest", "Vendor", "Customer", "Item", "PurchaseOrder"];

function extractApiError(err: unknown): string {
  const axiosErr = err as AxiosError<{ message?: string; errors?: string[] | Record<string, string[]> }>;
  const body = axiosErr.response?.data;
  if (typeof body?.message === "string") return body.message;
  if (body?.errors) {
    if (Array.isArray(body.errors)) return body.errors[0] ?? "";
    const first = Object.values(body.errors).flat()[0];
    if (first) return first;
  }
  return (err as Error)?.message ?? "An error occurred";
}

export default function ValidationRulesPage() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "entityType" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<ValidationRule | null>(null);
  const [form, setForm] = useState({
    entityType: "Employee",
    fieldName: "",
    ruleType: "Required",
    ruleValue: "",
    errorMessage: "",
    severity: "Error",
    isEnabled: true,
    sortOrder: 0,
  });
  const [deleteTarget, setDeleteTarget] = useState<ValidationRule | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["validation-rules", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getValidationRules(dg.queryParams),
  });

  const rules = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload = {
        entityType: form.entityType,
        fieldName: form.fieldName,
        ruleType: form.ruleType,
        ruleValue: form.ruleValue || undefined,
        errorMessage: form.errorMessage || undefined,
        severity: form.severity,
        isEnabled: form.isEnabled,
        sortOrder: form.sortOrder,
      };
      return editing
        ? updateValidationRule(editing.id, payload)
        : createValidationRule(payload);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["validation-rules"] });
      closeModal();
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, isEnabled }: { id: string; isEnabled: boolean }) =>
      updateValidationRule(id, { isEnabled } as Partial<ValidationRule>),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["validation-rules"] }),
    onError: (err) => toast.error(extractApiError(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteValidationRule(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["validation-rules"] });
      setDeleteTarget(null);
    },
    onError: () => setDeleteTarget(null),
  });

  function openAdd() {
    setEditing(null);
    setForm({ entityType: "Employee", fieldName: "", ruleType: "Required", ruleValue: "", errorMessage: "", severity: "Error", isEnabled: true, sortOrder: 0 });
    setModal(true);
  }

  function openEdit(rule: ValidationRule) {
    setEditing(rule);
    setForm({
      entityType: rule.entityType,
      fieldName: rule.fieldName,
      ruleType: rule.ruleType,
      ruleValue: rule.ruleValue ?? "",
      errorMessage: rule.errorMessage ?? "",
      severity: rule.severity,
      isEnabled: rule.isEnabled,
      sortOrder: rule.sortOrder,
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const ruleTypeBadge = (type: string) => {
    const colors: Record<string, string> = {
      Required: "bg-red-50 text-red-700",
      Email: "bg-blue-50 text-blue-700",
      Regex: "bg-purple-50 text-purple-700",
      MinLength: "bg-amber-50 text-amber-700",
      MaxLength: "bg-amber-50 text-amber-700",
      Phone: "bg-green-50 text-green-700",
    };
    return `text-xs px-2 py-0.5 rounded-full ${colors[type] ?? "bg-gray-50 text-gray-600"}`;
  };

  const columns = [
    { key: "entityType", label: "Entity" },
    { key: "fieldName", label: "Field" },
    {
      key: "ruleType",
      label: "Rule",
      render: (row: ValidationRule) => (
        <span className={ruleTypeBadge(row.ruleType)}>{row.ruleType}</span>
      ),
    },
    {
      key: "ruleValue",
      label: "Value",
      render: (row: ValidationRule) => (
        <span className="text-xs font-mono text-gray-500">{row.ruleValue || "—"}</span>
      ),
    },
    {
      key: "errorMessage",
      label: "Error Message",
      render: (row: ValidationRule) => (
        <span className="text-xs text-gray-500">{row.errorMessage || "—"}</span>
      ),
    },
    {
      key: "severity",
      label: "Severity",
      render: (row: ValidationRule) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${
          row.severity === "Error" ? "bg-red-50 text-red-700" :
          row.severity === "Warning" ? "bg-amber-50 text-amber-700" :
          "bg-blue-50 text-blue-700"
        }`}>{row.severity}</span>
      ),
    },
    {
      key: "sortOrder",
      label: "Order",
      render: (row: ValidationRule) => (
        <span className="text-xs text-gray-500">{row.sortOrder}</span>
      ),
    },
    {
      key: "isEnabled",
      label: "Enabled",
      sortable: false,
      render: (row: ValidationRule) => (
        <button
          onClick={() => toggleMutation.mutate({ id: row.id, isEnabled: !row.isEnabled })}
          className={`transition-colors ${row.isEnabled ? "text-green-500 hover:text-green-700" : "text-gray-300 hover:text-gray-500"}`}
          title={row.isEnabled ? "Disable" : "Enable"}
        >
          {row.isEnabled ? <ToggleRight size={18} /> : <ToggleLeft size={18} />}
        </button>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Validation Rules</h2>
          <p className="text-xs text-gray-400">Configure dynamic validation rules for entities</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Rule
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={rules}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search rules..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={setDeleteTarget}
        emptyMessage="No validation rules found"
      />

      <Modal title={editing ? "Edit Validation Rule" : "Add Validation Rule"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Entity Type *</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.entityType}
                onChange={(e) => setForm((f) => ({ ...f, entityType: e.target.value }))}
              >
                {ENTITY_TYPES.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Field Name *</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.fieldName}
                onChange={(e) => setForm((f) => ({ ...f, fieldName: e.target.value }))}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Rule Type *</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.ruleType}
                onChange={(e) => setForm((f) => ({ ...f, ruleType: e.target.value }))}
              >
                {RULE_TYPES.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">
                Rule Value {form.ruleType !== "Required" ? "*" : ""}
              </label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.ruleValue}
                onChange={(e) => setForm((f) => ({ ...f, ruleValue: e.target.value }))}
                placeholder={form.ruleType === "Regex" ? "^[A-Z].+$" : form.ruleType === "MinLength" ? "3" : form.ruleType === "MaxLength" ? "100" : ""}
                disabled={form.ruleType === "Required"}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Severity</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.severity}
                onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))}
              >
                {SEVERITIES.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Sort Order</label>
              <input
                type="number"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.sortOrder}
                onChange={(e) => setForm((f) => ({ ...f, sortOrder: parseInt(e.target.value) || 0 }))}
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Custom Error Message</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.errorMessage}
              onChange={(e) => setForm((f) => ({ ...f, errorMessage: e.target.value }))}
              placeholder="Leave empty for default error message"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Status</label>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => setForm((f) => ({ ...f, isEnabled: true }))}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  form.isEnabled ? "bg-green-50 text-green-700 border border-green-200" : "bg-gray-50 text-gray-400 border border-gray-200"
                }`}
              >
                Enabled
              </button>
              <button
                type="button"
                onClick={() => setForm((f) => ({ ...f, isEnabled: false }))}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  !form.isEnabled ? "bg-red-50 text-red-600 border border-red-200" : "bg-gray-50 text-gray-400 border border-gray-200"
                }`}
              >
                Disabled
              </button>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => {
                if (!form.fieldName.trim()) {
                  toast.error("Field Name is required.");
                  return;
                }
                if (form.ruleType !== "Required" && !form.ruleValue.trim() && form.ruleType !== "Email" && form.ruleType !== "Phone") {
                  toast.error("Rule Value is required for this rule type.");
                  return;
                }
                saveMutation.mutate();
              }}
              disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        tone="danger"
        title="Delete Validation Rule"
        message={
          <>
            Are you sure you want to delete the <span className="font-medium text-gray-900">{deleteTarget?.ruleType}</span> rule for <span className="font-medium text-gray-900">{deleteTarget?.entityType}.{deleteTarget?.fieldName}</span>? This action cannot be undone.
          </>
        }
        confirmLabel="Delete"
        loading={deleteMutation.isPending}
        onConfirm={() => { if (deleteTarget) deleteMutation.mutate(deleteTarget.id); }}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
