import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Pencil, Trash2, MessageSquare } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getRFIs,
  createRFI,
  updateRFI,
  respondRFI,
  closeRFI,
  deleteRFI,
  RFI,
} from "../../api/projects";

const STATUSES = ["Open", "Responded", "Closed", "Cancelled"];

const statusColors: Record<string, string> = {
  Open: "bg-blue-50 text-blue-700",
  Responded: "bg-amber-50 text-amber-700",
  Closed: "bg-green-50 text-green-700",
  Cancelled: "bg-gray-100 text-gray-600",
};

const emptyForm = {
  subject: "",
  description: "",
  responseDueDate: "",
  isUrgent: false,
};

export default function RFIsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [respondModal, setRespondModal] = useState(false);
  const [editing, setEditing] = useState<RFI | null>(null);
  const [selected, setSelected] = useState<RFI | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [respondForm, setRespondForm] = useState({ response: "", responseDate: new Date().toISOString().substring(0, 10) });

  const { data, isLoading } = useQuery({
    queryKey: ["rfis", projectId, filterStatus, page],
    queryFn: () => getRFIs({ page, pageSize: 50 }, projectId, filterStatus || undefined),
    enabled: !!projectId,
  });

  const rfis = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createRFI({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["rfis", projectId] }); closeModal(); },
  });

  const updateMutation = useMutation({
    mutationFn: () => updateRFI(editing!.id, form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["rfis", projectId] }); closeModal(); },
  });

  const respondMutation = useMutation({
    mutationFn: () => respondRFI(selected!.id, respondForm),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["rfis", projectId] }); setRespondModal(false); },
  });

  const closeMutation = useMutation({
    mutationFn: (id: string) => closeRFI(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rfis", projectId] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteRFI(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rfis", projectId] }),
  });

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModal(true);
  }

  function openEdit(r: RFI) {
    setEditing(r);
    setForm({
      subject: r.subject,
      description: r.description,
      responseDueDate: r.responseDueDate?.substring(0, 10) ?? "",
      isUrgent: r.isUrgent,
    });
    setModal(true);
  }

  function openRespond(r: RFI) {
    setSelected(r);
    setRespondForm({ response: r.response ?? "", responseDate: new Date().toISOString().substring(0, 10) });
    setRespondModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
    setForm(emptyForm);
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
          <h2 className="text-base font-medium text-gray-900">Requests for Information (RFI)</h2>
          <p className="text-xs text-gray-400">Manage project RFIs and track responses</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Raise RFI
        </button>
      </div>

      <div className="flex gap-3 mb-4">
        <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : rfis.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No RFIs found</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">RFI No.</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Subject</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Raised By</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Raised Date</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Due Date</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {rfis.map((r) => (
                <tr key={r.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 font-medium text-gray-900">
                    {r.rfiNumber}
                    {r.isUrgent && <span className="ml-1 text-xs bg-red-50 text-red-700 px-1.5 py-0.5 rounded">Urgent</span>}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700 max-w-xs truncate">{r.subject}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{r.raisedByName}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{new Date(r.raisedDate).toLocaleDateString()}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">
                    {r.responseDueDate ? new Date(r.responseDueDate).toLocaleDateString() : "—"}
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[r.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {r.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      {r.status === "Open" && (
                        <button onClick={() => openRespond(r)} className="text-blue-500 hover:text-blue-700" title="Respond">
                          <MessageSquare size={14} />
                        </button>
                      )}
                      {r.status === "Responded" && (
                        <button onClick={() => { if (confirm("Close this RFI?")) closeMutation.mutate(r.id); }}
                          className="text-green-500 hover:text-green-700 text-xs font-medium">Close</button>
                      )}
                      <button onClick={() => openEdit(r)} className="text-gray-400 hover:text-gray-700"><Pencil size={14} /></button>
                      <button onClick={() => { if (confirm("Delete this RFI?")) deleteMutation.mutate(r.id); }}
                        className="text-gray-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title={editing ? "Edit RFI" : "Raise RFI"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Subject *</label>
            <input value={form.subject} onChange={(e) => setForm((f) => ({ ...f, subject: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <textarea value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Response Due Date</label>
              <input type="date" value={form.responseDueDate} onChange={(e) => setForm((f) => ({ ...f, responseDueDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div className="flex items-end pb-2">
              <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                <input type="checkbox" checked={form.isUrgent} onChange={(e) => setForm((f) => ({ ...f, isUrgent: e.target.checked }))}
                  className="rounded border-gray-300" />
                Mark as Urgent
              </label>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => editing ? updateMutation.mutate() : createMutation.mutate()} disabled={isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {isPending ? "Saving..." : editing ? "Save Changes" : "Raise RFI"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Respond to RFI" open={respondModal} onClose={() => setRespondModal(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Response *</label>
            <textarea value={respondForm.response} onChange={(e) => setRespondForm((f) => ({ ...f, response: e.target.value }))} rows={4}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Response Date</label>
            <input type="date" value={respondForm.responseDate} onChange={(e) => setRespondForm((f) => ({ ...f, responseDate: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setRespondModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => respondMutation.mutate()} disabled={respondMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {respondMutation.isPending ? "Submitting..." : "Submit Response"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
