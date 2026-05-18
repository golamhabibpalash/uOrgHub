import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Pencil, Trash2, ClipboardCheck } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getSubmittals,
  createSubmittal,
  updateSubmittal,
  reviewSubmittal,
  deleteSubmittal,
  Submittal,
} from "../../api/projects";

const STATUSES = ["Draft", "Submitted", "UnderReview", "Approved", "ApprovedWithComments", "Rejected", "Resubmit"];

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Submitted: "bg-blue-50 text-blue-700",
  UnderReview: "bg-amber-50 text-amber-700",
  Approved: "bg-green-50 text-green-700",
  ApprovedWithComments: "bg-teal-50 text-teal-700",
  Rejected: "bg-red-50 text-red-700",
  Resubmit: "bg-orange-50 text-orange-700",
};

const REVIEW_STATUSES = ["Approved", "ApprovedWithComments", "Rejected", "Resubmit"];

const emptyForm = {
  title: "",
  contractorReference: "",
  submittedDate: new Date().toISOString().substring(0, 10),
  filePath: "",
};

export default function SubmittalsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [reviewModal, setReviewModal] = useState(false);
  const [editing, setEditing] = useState<Submittal | null>(null);
  const [selected, setSelected] = useState<Submittal | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [reviewForm, setReviewForm] = useState({
    status: "Approved",
    reviewComments: "",
    reviewDate: new Date().toISOString().substring(0, 10),
  });

  const { data, isLoading } = useQuery({
    queryKey: ["submittals", projectId, filterStatus, page],
    queryFn: () => getSubmittals({ page, pageSize: 50 }, projectId, filterStatus || undefined),
    enabled: !!projectId,
  });

  const submittals = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createSubmittal({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["submittals", projectId] }); closeModal(); },
  });

  const updateMutation = useMutation({
    mutationFn: () => updateSubmittal(editing!.id, form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["submittals", projectId] }); closeModal(); },
  });

  const reviewMutation = useMutation({
    mutationFn: () => reviewSubmittal(selected!.id, reviewForm),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["submittals", projectId] }); setReviewModal(false); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteSubmittal(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["submittals", projectId] }),
  });

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModal(true);
  }

  function openEdit(s: Submittal) {
    setEditing(s);
    setForm({
      title: s.title,
      contractorReference: s.contractorReference ?? "",
      submittedDate: s.submittedDate?.substring(0, 10) ?? "",
      filePath: s.filePath ?? "",
    });
    setModal(true);
  }

  function openReview(s: Submittal) {
    setSelected(s);
    setReviewForm({
      status: "Approved",
      reviewComments: s.reviewComments ?? "",
      reviewDate: new Date().toISOString().substring(0, 10),
    });
    setReviewModal(true);
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
          <h2 className="text-base font-medium text-gray-900">Submittals</h2>
          <p className="text-xs text-gray-400">Track contractor submittals and review status</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Submittal
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
        ) : submittals.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No submittals found</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Submittal No.</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Title</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Contractor Ref</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Submitted By</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Submitted Date</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
              </tr>
            </thead>
            <tbody>
              {submittals.map((s) => (
                <tr key={s.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 font-medium text-gray-900">{s.submittalNumber}</td>
                  <td className="px-4 py-2.5 text-gray-700 max-w-xs truncate">{s.title}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{s.contractorReference || "—"}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{s.submittedByName}</td>
                  <td className="px-4 py-2.5 text-gray-600 text-xs">{new Date(s.submittedDate).toLocaleDateString()}</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[s.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {s.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      {(s.status === "Submitted" || s.status === "UnderReview") && (
                        <button onClick={() => openReview(s)} className="text-blue-500 hover:text-blue-700" title="Review">
                          <ClipboardCheck size={14} />
                        </button>
                      )}
                      <button onClick={() => openEdit(s)} className="text-gray-400 hover:text-gray-700"><Pencil size={14} /></button>
                      <button onClick={() => { if (confirm("Delete this submittal?")) deleteMutation.mutate(s.id); }}
                        className="text-gray-400 hover:text-red-600"><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title={editing ? "Edit Submittal" : "Add Submittal"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Contractor Reference</label>
              <input value={form.contractorReference} onChange={(e) => setForm((f) => ({ ...f, contractorReference: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Submitted Date *</label>
              <input type="date" value={form.submittedDate} onChange={(e) => setForm((f) => ({ ...f, submittedDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">File Path / Reference</label>
            <input value={form.filePath} onChange={(e) => setForm((f) => ({ ...f, filePath: e.target.value }))}
              placeholder="e.g. /submittals/SUB-001.pdf"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => editing ? updateMutation.mutate() : createMutation.mutate()} disabled={isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {isPending ? "Saving..." : editing ? "Save Changes" : "Add Submittal"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Review Submittal" open={reviewModal} onClose={() => setReviewModal(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Review Decision *</label>
            <select value={reviewForm.status} onChange={(e) => setReviewForm((f) => ({ ...f, status: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
              {REVIEW_STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Review Comments</label>
            <textarea value={reviewForm.reviewComments} onChange={(e) => setReviewForm((f) => ({ ...f, reviewComments: e.target.value }))} rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Review Date</label>
            <input type="date" value={reviewForm.reviewDate} onChange={(e) => setReviewForm((f) => ({ ...f, reviewDate: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setReviewModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => reviewMutation.mutate()} disabled={reviewMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {reviewMutation.isPending ? "Submitting..." : "Submit Review"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
