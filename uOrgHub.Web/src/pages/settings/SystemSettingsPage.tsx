import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Plus, Lock, Shield } from "lucide-react";
import toast from "react-hot-toast";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import {
  getSystemSettings,
  createSystemSetting,
  updateSystemSetting,
  deleteSystemSetting,
  SystemSetting,
} from "../../api/settings";

const DATA_TYPES = ["String", "Number", "Boolean", "Json", "Password"];

function extractApiError(err: unknown): string {
  const axiosErr = err as AxiosError<{
    message?: string;
    errors?: string[] | Record<string, string[]>;
  }>;
  const body = axiosErr.response?.data;
  if (typeof body?.message === "string") return body.message;
  if (body?.errors) {
    if (Array.isArray(body.errors)) return body.errors[0] ?? "";
    const first = Object.values(body.errors).flat()[0];
    if (first) return first;
  }
  return (err as Error)?.message ?? "An error occurred";
}

export default function SystemSettingsPage() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "category" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<SystemSetting | null>(null);
  const [form, setForm] = useState({
    category: "",
    key: "",
    value: "",
    dataType: "String",
    description: "",
    isActive: true,
    isSystem: false,
  });
  const [deleteTarget, setDeleteTarget] = useState<SystemSetting | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["system-settings", ...dg.queryKey],
    queryFn: () => getSystemSettings(dg.queryParams),
  });

  const settings = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () =>
      editing
        ? updateSystemSetting(editing.id, form)
        : createSystemSetting(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["system-settings"] });
      closeModal();
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteSystemSetting(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["system-settings"] });
      setDeleteTarget(null);
    },
    onError: () => setDeleteTarget(null),
  });

  function openAdd() {
    setEditing(null);
    setForm({ category: "", key: "", value: "", dataType: "String", description: "", isActive: true, isSystem: false });
    setModal(true);
  }

  function openEdit(s: SystemSetting) {
    if (s.isSystem) return;
    setEditing(s);
    setForm({
      category: s.category,
      key: s.key,
      value: s.value,
      dataType: s.dataType,
      description: s.description ?? "",
      isActive: s.isActive,
      isSystem: s.isSystem,
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const columns = [
    { key: "category", label: "Category" },
    { key: "key", label: "Key" },
    { key: "value", label: "Value" },
    { key: "dataType", label: "Type" },
    {
      key: "isSystem",
      label: "",
      sortable: false,
      render: (row: SystemSetting) =>
        row.isSystem ? (
          <span title="System setting (locked)">
            <Lock size={14} className="text-amber-500" />
          </span>
        ) : null,
    },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row: SystemSetting) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${
          row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"
        }`}>
          {row.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
    {
      key: "description",
      label: "Description",
      render: (row: SystemSetting) => (
        <span className="text-xs text-gray-500">{row.description || "—"}</span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">System Settings</h2>
          <p className="text-xs text-gray-400">Manage application configuration settings</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Setting
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={settings}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search settings..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => (row.isSystem ? toast.error("System settings cannot be deleted.") : setDeleteTarget(row))}
        emptyMessage="No settings found"
      />

      <Modal title={editing ? "Edit Setting" : "Add Setting"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Category *</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.category}
              onChange={(e) => setForm((f) => ({ ...f, category: e.target.value }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Key *</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.key}
              onChange={(e) => setForm((f) => ({ ...f, key: e.target.value }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Value *</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.value}
              onChange={(e) => setForm((f) => ({ ...f, value: e.target.value }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Data Type *</label>
            <select
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.dataType}
              onChange={(e) => setForm((f) => ({ ...f, dataType: e.target.value }))}
            >
              {DATA_TYPES.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Status</label>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => setForm((f) => ({ ...f, isActive: true }))}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  form.isActive ? "bg-green-50 text-green-700 border border-green-200" : "bg-gray-50 text-gray-400 border border-gray-200"
                }`}
              >
                Active
              </button>
              <button
                type="button"
                onClick={() => setForm((f) => ({ ...f, isActive: false }))}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  !form.isActive ? "bg-red-50 text-red-600 border border-red-200" : "bg-gray-50 text-gray-400 border border-gray-200"
                }`}
              >
                Inactive
              </button>
            </div>
          </div>
          {editing?.isSystem && (
            <div className="flex items-center gap-2 text-xs text-amber-600 bg-amber-50 px-3 py-2 rounded-lg">
              <Shield size={14} />
              This is a system setting. Some fields may be locked.
            </div>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => {
                if (!form.category.trim() || !form.key.trim()) {
                  toast.error("Category and Key are required.");
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
        title="Delete Setting"
        message={
          <>
            Are you sure you want to delete <span className="font-medium text-gray-900">{deleteTarget?.key}</span>? This action cannot be undone.
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
