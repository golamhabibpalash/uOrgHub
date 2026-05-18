import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Pencil, Trash2, CheckCircle, ShieldOff } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getNCRs,
  createNCR,
  updateNCR,
  verifyNCR,
  closeNCR,
  voidNCR,
  deleteNCR,
  NCR,
} from "../../api/projects";

const CATEGORIES = ["Workmanship", "Material", "Process", "Design", "Documentation", "Safety"];
const SEVERITIES = ["Minor", "Major", "Critical"];
const STATUSES = ["Open", "Responded", "Verified", "Closed", "Voided"];

const statusColors: Record<string, string> = {
  Open: "bg-blue-50 text-blue-700",
  Responded: "bg-amber-50 text-amber-700",
  Verified: "bg-purple-50 text-purple-700",
  Closed: "bg-green-50 text-green-700",
  Voided: "bg-gray-100 text-gray-500",
};

const severityColors: Record<string, string> = {
  Minor: "bg-yellow-50 text-yellow-700",
  Major: "bg-orange-50 text-orange-700",
  Critical: "bg-red-50 text-red-700",
};

const emptyForm = {
  title: "",
  description: "",
  category: "Workmanship",
  severity: "Minor",
  raisedDate: new Date().toISOString().substring(0, 10),
  responsibleParty: "",
  correctiveAction: "",
};

export default function NCRsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterStatus, setFilterStatus] = useState("");
  const [filterSeverity, setFilterSeverity] = useState("");
  const [modal, setModal] = useState(false);
  const [verifyModal, setVerifyModal] = useState(false);
  const [editing, setEditing] = useState<NCR | null>(null);
  const [selected, setSelected] = useState<NCR | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [verifyForm, setVerifyForm] = useState({ verifiedDate: new Date().toISOString().substring(0, 10), notes: "" });

  const { data, isLoading } = useQuery({
    queryKey: ["ncrs", projectId, filterStatus, filterSeverity, page],
    queryFn: () => getNCRs({ page, pageSize: 50 }, projectId, filterStatus || undefined, filterSeverity || undefined),
    enabled: !!projectId,
  });

  const ncrs = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createNCR({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["ncrs", projectId] }); closeModal(); },
  });

  const updateMutation = useMutation({
    mutationFn: () => updateNCR(editing!.id, form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["ncrs", projectId] }); closeModal(); },
  });

  const verifyMutation = useMutation({
    mutationFn: () => verifyNCR(selected!.id, verifyForm),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["ncrs", projectId] }); setVerifyModal(false); },
  });

  const closeMutation = useMutation({
    mutationFn: (id: string) => closeNCR(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["ncrs", projectId] }),
  });

  const voidMutation = useMutation({
    mutationFn: (id: string) => voidNCR(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["ncrs", projectId] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteNCR(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["ncrs", projectId] }),
  });

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModal(true);
  }

  function openEdit(n: NCR) {
    setEditing(n);
    setForm({
      title: n.title,
      description: n.description,
      category: n.category,
      severity: n.severity,
      raisedDate: n.raisedDate?.substring(0, 10) ?? "",
      responsibleParty: n.responsibleParty ?? "",
      correctiveAction: n.correctiveAction ?? "",
    });
    setModal(true);
  }

  function openVerify(n: NCR) {
    setSelected(n);
    setVerifyForm({ verifiedDate: new Date().toISOString().substring(0, 10), notes: "" });
    setVerifyModal(true);
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
          <h2 className="text-base font-medium text-gray-900">Non-Conformance Reports (NCR)</h2>
          <p className="text-xs text-gray-400">Track and manage quality non-conformances</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Raise NCR
        </button>
      </div>

      <div className="flex gap-3 mb-4">
        <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
        <select value={filterSeverity} onChange={(e) => setFilterSeverity(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Severities</option>
          {SEVERITIES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : ncrs.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No NCRs found</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">NCR No.</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Title</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Category</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Severity</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Raised Date</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Responsible</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {ncrs.map((n) => (
                <tr key={n.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 font-medium text-gray-900">{n.ncrNumber}</td>
                  <td className="px-4 py-2.5 text-gray-700 max-w-xs truncate">{n.title}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{n.category}</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${severityColors[n.severity] ?? "bg-gray-100 text-gray-600"}`}>
                      {n.severity}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[n.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {n.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{new Date(n.raisedDate).toLocaleDateString()}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{n.responsibleParty || "—"}</td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-1.5">
                      {n.status === "Responded" && (
                        <button onClick={() => openVerify(n)} title="Verify" className="text-purple-500 hover:text-purple-700">
                          <CheckCircle size={14} />
                        </button>
                      )}
                      {n.status === "Verified" && (
                        <button onClick={() => { if (confirm("Close this NCR?")) closeMutation.mutate(n.id); }}
                          className="text-green-600 hover:text-green-700 text-xs font-medium">Close</button>
                      )}
                      {n.status !== "Closed" && n.status !== "Voided" && (
                        <button onClick={() => { if (confirm("Void this NCR?")) voidMutation.mutate(n.id); }}
                          title="Void" className="text-gray-400 hover:text-red-600">
                          <ShieldOff size={14} />
                        </button>
                      )}
                      <button onClick={() => openEdit(n)} className="text-gray-400 hover:text-gray-700"><Pencil size={14} /></button>
                      <button onClick={() => { if (confirm("Delete this NCR?")) deleteMutation.mutate(n.id); }}
                        className="text-gray-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title={editing ? "Edit NCR" : "Raise NCR"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <textarea value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Category</label>
              <select value={form.category} onChange={(e) => setForm((f) => ({ ...f, category: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                {CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Severity</label>
              <select value={form.severity} onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                {SEVERITIES.map((s) => <option key={s} value={s}>{s}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Raised Date</label>
              <input type="date" value={form.raisedDate} onChange={(e) => setForm((f) => ({ ...f, raisedDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Responsible Party</label>
            <input value={form.responsibleParty} onChange={(e) => setForm((f) => ({ ...f, responsibleParty: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Corrective Action</label>
            <textarea value={form.correctiveAction} onChange={(e) => setForm((f) => ({ ...f, correctiveAction: e.target.value }))} rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => editing ? updateMutation.mutate() : createMutation.mutate()} disabled={isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {isPending ? "Saving..." : editing ? "Save Changes" : "Raise NCR"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Verify NCR" open={verifyModal} onClose={() => setVerifyModal(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Verified Date *</label>
            <input type="date" value={verifyForm.verifiedDate} onChange={(e) => setVerifyForm((f) => ({ ...f, verifiedDate: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Verification Notes</label>
            <textarea value={verifyForm.notes} onChange={(e) => setVerifyForm((f) => ({ ...f, notes: e.target.value }))} rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setVerifyModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => verifyMutation.mutate()} disabled={verifyMutation.isPending}
              className="px-4 py-2 text-sm bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50">
              {verifyMutation.isPending ? "Verifying..." : "Verify NCR"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
