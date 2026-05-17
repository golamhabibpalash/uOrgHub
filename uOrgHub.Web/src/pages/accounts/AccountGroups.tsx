import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getAccountGroups,
  createAccountGroup,
  updateAccountGroup,
  deleteAccountGroup,
  AccountGroup,
  AccountGroupType,
} from "../../api/accounts";

const GROUP_TYPES: AccountGroupType[] = ["Asset", "Liability", "Equity", "Income", "Expense"];

const typeColors: Record<AccountGroupType, string> = {
  Asset: "bg-blue-50 text-blue-700",
  Liability: "bg-red-50 text-red-700",
  Equity: "bg-purple-50 text-purple-700",
  Income: "bg-green-50 text-green-700",
  Expense: "bg-orange-50 text-orange-700",
};

export default function AccountGroups() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<AccountGroup | null>(null);
  const [form, setForm] = useState({ name: "", code: "", type: "Asset" as AccountGroupType, description: "", isActive: true });
  const [saveError, setSaveError] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["account-groups", page, search],
    queryFn: () => getAccountGroups({ page, pageSize: 10, search }),
  });

  const groups = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateAccountGroup(editing.id, form) : createAccountGroup(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["account-groups"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save account group.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteAccountGroup(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["account-groups"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", code: "", type: "Asset", description: "", isActive: true });
    setSaveError("");
    setModal(true);
  }

  function openEdit(g: AccountGroup) {
    setEditing(g);
    setForm({ name: g.name, code: g.code, type: g.type, description: g.description ?? "", isActive: g.isActive });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Group Name" },
    {
      key: "type",
      label: "Type",
      render: (row: AccountGroup) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${typeColors[row.type]}`}>{row.type}</span>
      ),
    },
    { key: "description", label: "Description" },
    {
      key: "isActive",
      label: "Status",
      render: (row: AccountGroup) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
          {row.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Account Groups</h2>
          <p className="text-xs text-gray-400">Classify accounts by type</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Group
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search account groups..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
        <DataTable columns={columns} data={groups} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Account Group" : "Add Account Group"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Group Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Code *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.code} onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Type *</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.type} onChange={(e) => setForm((f) => ({ ...f, type: e.target.value as AccountGroupType }))}>
              {GROUP_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>
          {editing && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} />
              <label htmlFor="isActive" className="text-xs text-gray-600">Active</label>
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
