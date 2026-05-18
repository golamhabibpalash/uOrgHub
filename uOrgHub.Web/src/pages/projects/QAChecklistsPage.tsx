import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Trash2, ClipboardCheck } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getQAChecklists,
  createQAChecklist,
  submitQAChecklist,
  deleteQAChecklist,
  QAChecklist,
} from "../../api/projects";

const STATUSES = ["Draft", "Open", "InProgress", "Passed", "Failed", "Waived"];
const INSPECTION_TYPES = ["Pre", "During", "Post", "Final"];
const RESULTS = ["Pass", "Fail", "ConditionalPass", "NotApplicable"];

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Open: "bg-blue-50 text-blue-700",
  InProgress: "bg-amber-50 text-amber-700",
  Passed: "bg-green-50 text-green-700",
  Failed: "bg-red-50 text-red-700",
  Waived: "bg-purple-50 text-purple-700",
};

const resultColors: Record<string, string> = {
  Pass: "bg-green-50 text-green-700",
  Fail: "bg-red-50 text-red-700",
  ConditionalPass: "bg-amber-50 text-amber-700",
  NotApplicable: "bg-gray-100 text-gray-500",
};

const emptyItem = () => ({
  sequence: 1,
  checkpointDescription: "",
  inspectionType: "During",
  isRequired: true,
});

export default function QAChecklistsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [submitModal, setSubmitModal] = useState(false);
  const [selected, setSelected] = useState<QAChecklist | null>(null);
  const [form, setForm] = useState({ title: "", location: "", inspectionDate: "", items: [emptyItem()] });
  const [submitForm, setSubmitForm] = useState<{
    overallResult: string;
    remarks: string;
    itemResults: Record<string, { result: string; remarks: string }>;
  }>({ overallResult: "Pass", remarks: "", itemResults: {} });

  const { data, isLoading } = useQuery({
    queryKey: ["qachecklists", projectId, filterStatus, page],
    queryFn: () => getQAChecklists({ page, pageSize: 50 }, projectId, filterStatus || undefined),
    enabled: !!projectId,
  });

  const checklists = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createQAChecklist({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["qachecklists", projectId] }); closeModal(); },
  });

  const submitMutation = useMutation({
    mutationFn: () => {
      const items = Object.entries(submitForm.itemResults).map(([id, v]) => ({ id, result: v.result, remarks: v.remarks }));
      return submitQAChecklist(selected!.id, { overallResult: submitForm.overallResult, remarks: submitForm.remarks, items });
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["qachecklists", projectId] }); setSubmitModal(false); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteQAChecklist(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["qachecklists", projectId] }),
  });

  function openCreate() {
    setForm({ title: "", location: "", inspectionDate: "", items: [emptyItem()] });
    setModal(true);
  }

  function openSubmit(checklist: QAChecklist) {
    setSelected(checklist);
    const itemResults: Record<string, { result: string; remarks: string }> = {};
    checklist.items.forEach((item) => {
      itemResults[item.id] = { result: item.result ?? "Pass", remarks: item.remarks ?? "" };
    });
    setSubmitForm({ overallResult: "Pass", remarks: "", itemResults });
    setSubmitModal(true);
  }

  function closeModal() {
    setModal(false);
    setForm({ title: "", location: "", inspectionDate: "", items: [emptyItem()] });
  }

  function addItem() {
    setForm((f) => ({
      ...f,
      items: [...f.items, { ...emptyItem(), sequence: f.items.length + 1 }],
    }));
  }

  function removeItem(idx: number) {
    setForm((f) => ({ ...f, items: f.items.filter((_, i) => i !== idx) }));
  }

  function updateItem(idx: number, field: string, value: string | boolean) {
    setForm((f) => ({
      ...f,
      items: f.items.map((item, i) => (i === idx ? { ...item, [field]: value } : item)),
    }));
  }

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
          <h2 className="text-base font-medium text-gray-900">QA/QC Checklists</h2>
          <p className="text-xs text-gray-400">Manage quality assurance inspection checklists</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Create Checklist
        </button>
      </div>

      <div className="flex gap-3 mb-4">
        <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="space-y-3">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : checklists.length === 0 ? (
          <div className="text-center py-10 text-gray-400 bg-white border border-gray-200 rounded-xl">No checklists found</div>
        ) : (
          checklists.map((cl) => (
            <div key={cl.id} className="bg-white border border-gray-200 rounded-xl p-4">
              <div className="flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-medium text-gray-900 text-sm">{cl.checklistNumber}</span>
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[cl.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {cl.status}
                    </span>
                    {cl.overallResult && (
                      <span className={`text-xs px-2 py-0.5 rounded-full ${resultColors[cl.overallResult] ?? "bg-gray-100 text-gray-600"}`}>
                        {cl.overallResult}
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-700">{cl.title}</p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {cl.location && `${cl.location} • `}
                    {cl.items.length} checkpoints
                    {cl.inspectionDate && ` • ${new Date(cl.inspectionDate).toLocaleDateString()}`}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  {(cl.status === "Open" || cl.status === "InProgress") && (
                    <button onClick={() => openSubmit(cl)}
                      className="flex items-center gap-1 px-3 py-1.5 text-sm bg-green-500 text-white rounded-lg hover:bg-green-600">
                      <ClipboardCheck size={14} /> Submit Inspection
                    </button>
                  )}
                  <button onClick={() => { if (confirm("Delete this checklist?")) deleteMutation.mutate(cl.id); }}
                    className="text-gray-400 hover:text-red-600"><Trash2 size={14} /></button>
                </div>
              </div>
              {cl.items.length > 0 && (
                <div className="mt-3 border-t border-gray-100 pt-3">
                  <div className="space-y-1">
                    {cl.items.slice(0, 3).map((item) => (
                      <div key={item.id} className="flex items-center gap-2 text-xs text-gray-600">
                        <span className="w-5 text-gray-400">{item.sequence}.</span>
                        <span className="flex-1">{item.checkpointDescription}</span>
                        <span className="text-gray-400">{item.inspectionType}</span>
                        {item.result && (
                          <span className={`px-1.5 py-0.5 rounded ${resultColors[item.result] ?? "bg-gray-100 text-gray-600"}`}>
                            {item.result}
                          </span>
                        )}
                      </div>
                    ))}
                    {cl.items.length > 3 && (
                      <p className="text-xs text-gray-400">+{cl.items.length - 3} more checkpoints</p>
                    )}
                  </div>
                </div>
              )}
            </div>
          ))
        )}
      </div>

      <Modal title="Create QA Checklist" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Location</label>
              <input value={form.location} onChange={(e) => setForm((f) => ({ ...f, location: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Inspection Date</label>
              <input type="date" value={form.inspectionDate} onChange={(e) => setForm((f) => ({ ...f, inspectionDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Checkpoints *</label>
              <button onClick={addItem} className="text-xs text-primary-600 hover:text-primary-700 font-medium">+ Add Checkpoint</button>
            </div>
            <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
              {form.items.map((item, idx) => (
                <div key={idx} className="flex gap-2 items-start border border-gray-100 rounded-lg p-2">
                  <span className="text-xs text-gray-400 mt-2 w-4">{idx + 1}</span>
                  <div className="flex-1 space-y-1">
                    <input value={item.checkpointDescription}
                      onChange={(e) => updateItem(idx, "checkpointDescription", e.target.value)}
                      placeholder="Checkpoint description"
                      className="w-full border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500" />
                    <div className="flex gap-2">
                      <select value={item.inspectionType} onChange={(e) => updateItem(idx, "inspectionType", e.target.value)}
                        className="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none">
                        {INSPECTION_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                      </select>
                      <label className="flex items-center gap-1 text-xs text-gray-500">
                        <input type="checkbox" checked={item.isRequired}
                          onChange={(e) => updateItem(idx, "isRequired", e.target.checked)} className="rounded border-gray-300" />
                        Required
                      </label>
                    </div>
                  </div>
                  <button onClick={() => removeItem(idx)} className="text-gray-300 hover:text-red-500 mt-1">
                    <Trash2 size={12} />
                  </button>
                </div>
              ))}
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => createMutation.mutate()} disabled={createMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {createMutation.isPending ? "Creating..." : "Create Checklist"}
            </button>
          </div>
        </div>
      </Modal>

      {selected && (
        <Modal title={`Submit Inspection — ${selected.checklistNumber}`} open={submitModal} onClose={() => setSubmitModal(false)}>
          <div className="space-y-3">
            <div className="space-y-2 max-h-60 overflow-y-auto pr-1">
              {selected.items.map((item) => (
                <div key={item.id} className="border border-gray-100 rounded-lg p-2">
                  <p className="text-xs font-medium text-gray-700 mb-1">{item.sequence}. {item.checkpointDescription}</p>
                  <div className="flex gap-2">
                    <select
                      value={submitForm.itemResults[item.id]?.result ?? "Pass"}
                      onChange={(e) => setSubmitForm((f) => ({
                        ...f,
                        itemResults: { ...f.itemResults, [item.id]: { ...f.itemResults[item.id], result: e.target.value } },
                      }))}
                      className="border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none">
                      {RESULTS.map((r) => <option key={r} value={r}>{r}</option>)}
                    </select>
                    <input
                      value={submitForm.itemResults[item.id]?.remarks ?? ""}
                      onChange={(e) => setSubmitForm((f) => ({
                        ...f,
                        itemResults: { ...f.itemResults, [item.id]: { ...f.itemResults[item.id], remarks: e.target.value } },
                      }))}
                      placeholder="Remarks (optional)"
                      className="flex-1 border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none" />
                  </div>
                </div>
              ))}
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Overall Result *</label>
                <select value={submitForm.overallResult} onChange={(e) => setSubmitForm((f) => ({ ...f, overallResult: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
                  {RESULTS.map((r) => <option key={r} value={r}>{r}</option>)}
                </select>
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Overall Remarks</label>
                <input value={submitForm.remarks} onChange={(e) => setSubmitForm((f) => ({ ...f, remarks: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
              </div>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <button onClick={() => setSubmitModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
              <button onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending}
                className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
                {submitMutation.isPending ? "Submitting..." : "Submit Inspection"}
              </button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
}
