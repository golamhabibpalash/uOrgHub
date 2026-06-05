import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getCostCenters,
  createCostCenter,
  updateCostCenter,
  deleteCostCenter,
  CostCenter,
} from "../../api/accounts";

export default function CostCenters() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<CostCenter | null>(null);
  const [form, setForm] = useState({
    code: "",
    name: "",
    description: "",
    parentCostCenterId: "",
    isActive: true,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["cost-centers", page, search],
    queryFn: () => getCostCenters({ page, pageSize: 10, search }),
  });

  const costCenters = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const [saveError, setSaveError] = useState("");

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload = {
        ...form,
        parentCostCenterId: form.parentCostCenterId || undefined,
      };
      return editing ? updateCostCenter(editing.id, payload) : createCostCenter(payload);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["cost-centers"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save cost center.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteCostCenter(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["cost-centers"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ code: "", name: "", description: "", parentCostCenterId: "", isActive: true });
    setSaveError("");
    setModal(true);
  }

  function openEdit(cc: CostCenter) {
    setEditing(cc);
    setForm({ code: cc.code, name: cc.name, description: cc.description ?? "", parentCostCenterId: cc.parentCostCenterId ?? "", isActive: cc.isActive });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Name" },
    { key: "parentCostCenterName", label: "Parent" },
    { key: "description", label: "Description" },
    {
      key: "isActive",
      label: "Status",
      render: (row: CostCenter) => (
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
          <h2 className="text-base font-medium text-gray-900">Cost Centers</h2>
          <p className="text-xs text-gray-400">Track costs by department or project</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Cost Center
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <input
            type="text"
            placeholder="Search cost centers..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <ExportMenu baseUrl="/accounts/cost-centers" filters={{ search: search || undefined }} />
        </div>
        <DataTable columns={columns} data={costCenters} loading={isLoading} onEdit={openEdit} onDelete={(row) => deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit Cost Center" : "Add Cost Center"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Code *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.code} onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))} disabled={!!editing} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Parent Cost Center</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.parentCostCenterId} onChange={(e) => setForm((f) => ({ ...f, parentCostCenterId: e.target.value }))}>
              <option value="">None (Top Level)</option>
              {costCenters.filter((cc) => cc.id !== editing?.id).map((cc) => (
                <option key={cc.id} value={cc.id}>{cc.code} — {cc.name}</option>
              ))}
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
