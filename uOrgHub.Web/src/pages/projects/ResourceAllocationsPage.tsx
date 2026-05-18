import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Pencil, Trash2 } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getResourceAllocations,
  createResourceAllocation,
  updateResourceAllocation,
  deleteResourceAllocation,
  ResourceAllocation,
} from "../../api/projects";

const RESOURCE_TYPES = ["Labour", "Equipment", "Material"];
const STATUSES = ["Planned", "Active", "Completed", "Cancelled"];

const statusColors: Record<string, string> = {
  Planned: "bg-blue-50 text-blue-700",
  Active: "bg-green-50 text-green-700",
  Completed: "bg-gray-100 text-gray-600",
  Cancelled: "bg-red-50 text-red-700",
};

const typeColors: Record<string, string> = {
  Labour: "bg-purple-50 text-purple-700",
  Equipment: "bg-amber-50 text-amber-700",
  Material: "bg-cyan-50 text-cyan-700",
};

const emptyForm = {
  resourceType: "Labour",
  description: "",
  status: "Planned",
  employeeId: "",
  equipmentCode: "",
  unitOfMeasure: "",
  plannedStartDate: "",
  plannedEndDate: "",
  actualStartDate: "",
  actualEndDate: "",
  plannedQuantity: 0,
  actualQuantity: 0,
  unitCost: 0,
};

export default function ResourceAllocationsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterType, setFilterType] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<ResourceAllocation | null>(null);
  const [form, setForm] = useState(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["resourceallocations", projectId, filterType, filterStatus, page],
    queryFn: () => getResourceAllocations({ page, pageSize: 50 }, projectId, filterType || undefined, filterStatus || undefined),
    enabled: !!projectId,
  });

  const allocations = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createResourceAllocation({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["resourceallocations", projectId] }); closeModal(); },
  });

  const updateMutation = useMutation({
    mutationFn: () => updateResourceAllocation(editing!.id, form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["resourceallocations", projectId] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteResourceAllocation(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["resourceallocations", projectId] }),
  });

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModal(true);
  }

  function openEdit(a: ResourceAllocation) {
    setEditing(a);
    setForm({
      resourceType: a.resourceType,
      description: a.description,
      status: a.status,
      employeeId: a.employeeId ?? "",
      equipmentCode: a.equipmentCode ?? "",
      unitOfMeasure: a.unitOfMeasure ?? "",
      plannedStartDate: a.plannedStartDate?.substring(0, 10) ?? "",
      plannedEndDate: a.plannedEndDate?.substring(0, 10) ?? "",
      actualStartDate: a.actualStartDate?.substring(0, 10) ?? "",
      actualEndDate: a.actualEndDate?.substring(0, 10) ?? "",
      plannedQuantity: a.plannedQuantity,
      actualQuantity: a.actualQuantity ?? 0,
      unitCost: a.unitCost,
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
    setForm(emptyForm);
  }

  const isPending = createMutation.isPending || updateMutation.isPending;
  const plannedCost = form.plannedQuantity * form.unitCost;

  return (
    <div>
      <div className="mb-4">
        <Link to={`/projects/${projectId}`} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back to Project
        </Link>
      </div>

      <ProjectNav />

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Resource Allocations</h2>
          <p className="text-xs text-gray-400">Manage labour, equipment and material allocations</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Resource
        </button>
      </div>

      <div className="flex gap-3 mb-4">
        <select value={filterType} onChange={(e) => setFilterType(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Types</option>
          {RESOURCE_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : allocations.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No resource allocations found</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Type</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Description</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Planned Start</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Planned End</th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Planned Qty</th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Actual Qty</th>
                <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500">Planned Cost</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {allocations.map((a) => (
                <tr key={a.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${typeColors[a.resourceType] ?? "bg-gray-100 text-gray-600"}`}>
                      {a.resourceType}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-gray-700 max-w-xs truncate">{a.description}</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[a.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {a.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{new Date(a.plannedStartDate).toLocaleDateString()}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{new Date(a.plannedEndDate).toLocaleDateString()}</td>
                  <td className="px-4 py-2.5 text-right text-gray-700">{a.plannedQuantity.toLocaleString()}</td>
                  <td className="px-4 py-2.5 text-right text-gray-700">{a.actualQuantity?.toLocaleString() ?? "—"}</td>
                  <td className="px-4 py-2.5 text-right font-medium text-gray-900">{a.plannedCost.toLocaleString()}</td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      <button onClick={() => openEdit(a)} className="text-gray-400 hover:text-gray-700"><Pencil size={14} /></button>
                      <button onClick={() => { if (confirm("Delete this allocation?")) deleteMutation.mutate(a.id); }}
                        className="text-gray-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title={editing ? "Edit Resource" : "Add Resource Allocation"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Resource Type *</label>
              <select value={form.resourceType} onChange={(e) => setForm((f) => ({ ...f, resourceType: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                {RESOURCE_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Status</label>
              <select value={form.status} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <input value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Unit of Measure</label>
              <input value={form.unitOfMeasure} onChange={(e) => setForm((f) => ({ ...f, unitOfMeasure: e.target.value }))}
                placeholder="e.g. days, hours, m3"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            {form.resourceType === "Equipment" && (
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Equipment Code</label>
                <input value={form.equipmentCode} onChange={(e) => setForm((f) => ({ ...f, equipmentCode: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
              </div>
            )}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Planned Start *</label>
              <input type="date" value={form.plannedStartDate} onChange={(e) => setForm((f) => ({ ...f, plannedStartDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Planned End *</label>
              <input type="date" value={form.plannedEndDate} onChange={(e) => setForm((f) => ({ ...f, plannedEndDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Planned Qty *</label>
              <input type="number" value={form.plannedQuantity} onChange={(e) => setForm((f) => ({ ...f, plannedQuantity: parseFloat(e.target.value) || 0 }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Actual Qty</label>
              <input type="number" value={form.actualQuantity} onChange={(e) => setForm((f) => ({ ...f, actualQuantity: parseFloat(e.target.value) || 0 }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Unit Cost *</label>
              <input type="number" value={form.unitCost} onChange={(e) => setForm((f) => ({ ...f, unitCost: parseFloat(e.target.value) || 0 }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>
          {plannedCost > 0 && (
            <p className="text-xs text-gray-500">Planned Cost: BDT {plannedCost.toLocaleString()}</p>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => editing ? updateMutation.mutate() : createMutation.mutate()} disabled={isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {isPending ? "Saving..." : editing ? "Save Changes" : "Add Resource"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
