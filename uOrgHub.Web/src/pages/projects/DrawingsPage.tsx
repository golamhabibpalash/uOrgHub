import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Pencil, Trash2 } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getDrawings,
  createDrawing,
  updateDrawing,
  deleteDrawing,
  Drawing,
} from "../../api/projects";

const DISCIPLINES = ["Architectural", "Structural", "Civil", "Mechanical", "Electrical", "Plumbing", "Landscape", "Survey"];
const STATUSES = ["Draft", "Issued", "Superseded", "Obsolete"];

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Issued: "bg-green-50 text-green-700",
  Superseded: "bg-amber-50 text-amber-700",
  Obsolete: "bg-red-50 text-red-700",
};

const emptyForm = {
  title: "",
  revision: "A",
  discipline: "Civil",
  status: "Draft",
  issuedDate: "",
  filePath: "",
  notes: "",
};

export default function DrawingsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterDiscipline, setFilterDiscipline] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Drawing | null>(null);
  const [form, setForm] = useState(emptyForm);

  const { data, isLoading } = useQuery({
    queryKey: ["drawings", projectId, filterDiscipline, filterStatus, page],
    queryFn: () => getDrawings({ page, pageSize: 50 }, projectId, filterDiscipline || undefined, filterStatus || undefined),
    enabled: !!projectId,
  });

  const drawings = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createDrawing({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["drawings", projectId] }); closeModal(); },
  });

  const updateMutation = useMutation({
    mutationFn: () => updateDrawing(editing!.id, form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["drawings", projectId] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteDrawing(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["drawings", projectId] }),
  });

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModal(true);
  }

  function openEdit(d: Drawing) {
    setEditing(d);
    setForm({
      title: d.title,
      revision: d.revision,
      discipline: d.discipline,
      status: d.status,
      issuedDate: d.issuedDate?.substring(0, 10) ?? "",
      filePath: d.filePath ?? "",
      notes: d.notes ?? "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
    setForm(emptyForm);
  }

  function handleSubmit() {
    editing ? updateMutation.mutate() : createMutation.mutate();
  }

  const isPending = createMutation.isPending || updateMutation.isPending;

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
          <h2 className="text-base font-medium text-gray-900">Drawings Register</h2>
          <p className="text-xs text-gray-400">Manage project drawings and document register</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Drawing
        </button>
      </div>

      <div className="flex gap-3 mb-4">
        <select
          value={filterDiscipline}
          onChange={(e) => setFilterDiscipline(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
        >
          <option value="">All Disciplines</option>
          {DISCIPLINES.map((d) => <option key={d} value={d}>{d}</option>)}
        </select>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
        >
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : drawings.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No drawings found</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Drawing No.</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Title</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Rev</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Discipline</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Issued Date</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {drawings.map((d) => (
                <tr key={d.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 font-medium text-gray-900">{d.drawingNumber}</td>
                  <td className="px-4 py-2.5 text-gray-700">{d.title}</td>
                  <td className="px-4 py-2.5 text-gray-600">{d.revision}</td>
                  <td className="px-4 py-2.5 text-gray-600">{d.discipline}</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[d.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {d.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">
                    {d.issuedDate ? new Date(d.issuedDate).toLocaleDateString() : "—"}
                  </td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      <button onClick={() => openEdit(d)} className="text-gray-400 hover:text-gray-700"><Pencil size={14} /></button>
                      <button
                        onClick={() => { if (confirm("Delete this drawing?")) deleteMutation.mutate(d.id); }}
                        className="text-gray-400 hover:text-red-600"
                      ><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title={editing ? "Edit Drawing" : "Add Drawing"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Revision</label>
              <input value={form.revision} onChange={(e) => setForm((f) => ({ ...f, revision: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Discipline</label>
              <select value={form.discipline} onChange={(e) => setForm((f) => ({ ...f, discipline: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                {DISCIPLINES.map((d) => <option key={d} value={d}>{d}</option>)}
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Status</label>
              <select value={form.status} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Issued Date</label>
              <input type="date" value={form.issuedDate} onChange={(e) => setForm((f) => ({ ...f, issuedDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">File Path / Reference</label>
            <input value={form.filePath} onChange={(e) => setForm((f) => ({ ...f, filePath: e.target.value }))}
              placeholder="e.g. /drawings/DWG-001.pdf"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={handleSubmit} disabled={isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {isPending ? "Saving..." : editing ? "Save Changes" : "Add Drawing"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
