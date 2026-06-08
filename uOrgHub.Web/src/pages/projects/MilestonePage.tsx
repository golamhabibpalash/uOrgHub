import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import ProjectNav from "../../components/projects/ProjectNav";
import { getMilestones, createMilestone, updateMilestone, deleteMilestone, Milestone } from "../../api/projects";

export default function MilestonePage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Milestone | null>(null);
  const [form, setForm] = useState({
    title: "",
    description: "",
    plannedDate: "",
    isCritical: false,
    wbsId: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["milestones", projectId],
    queryFn: () => getMilestones(projectId!),
    enabled: !!projectId,
  });

  const milestones = data?.data?.data ?? [];

  const saveMutation = useMutation({
    mutationFn: () =>
      editing
        ? updateMilestone(projectId!, editing.id, form)
        : createMilestone(projectId!, form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["milestones", projectId] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (milestoneId: string) => deleteMilestone(projectId!, milestoneId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["milestones", projectId] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      title: "",
      description: "",
      plannedDate: "",
      isCritical: false,
      wbsId: "",
    });
    setModal(true);
  }

  function openEdit(milestone: Milestone) {
    setEditing(milestone);
    setForm({
      title: milestone.title,
      description: milestone.description || "",
      plannedDate: milestone.plannedDate?.split("T")[0] || "",
      isCritical: milestone.isCritical,
      wbsId: milestone.wbsId || "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const wbsOptions: { value: string; label: string }[] = [];

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Pending: "bg-amber-50 text-amber-700",
      InProgress: "bg-blue-50 text-blue-700",
      Completed: "bg-green-50 text-green-700",
      Missed: "bg-red-50 text-red-700",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

  return (
    <div>
      <div className="mb-4">
        <Link
          to={`/projects/${projectId}`}
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft size={16} /> Back to Project
        </Link>
      </div>

      <ProjectNav />

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Project Milestones</h2>
          <p className="text-xs text-gray-400">Track project milestones</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Milestone
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-6">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : milestones.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No milestones found</div>
        ) : (
          <div className="relative">
            <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-200" />

            <div className="space-y-6">
              {milestones.map((milestone) => (
                <div key={milestone.id} className="relative pl-10">
                  <div
                    className={`absolute left-2.5 w-3 h-3 rounded-full ${
                      milestone.status === "Completed"
                        ? "bg-green-500"
                        : milestone.status === "InProgress"
                        ? "bg-blue-500"
                        : milestone.status === "Missed"
                        ? "bg-red-500"
                        : "bg-amber-500"
                    }`}
                  />

                  <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <h3 className="text-sm font-medium text-gray-900">
                            {milestone.title}
                          </h3>
                          {milestone.isCritical && (
                            <span className="text-xs bg-red-50 text-red-700 px-1.5 py-0.5 rounded">
                              Critical
                            </span>
                          )}
                        </div>
                        {milestone.description && (
                          <p className="text-xs text-gray-500 mb-2">
                            {milestone.description}
                          </p>
                        )}
                        <div className="flex items-center gap-4 text-xs text-gray-500">
                          <span>
                            Planned:{" "}
                            {milestone.plannedDate
                              ? new Date(milestone.plannedDate).toLocaleDateString()
                              : "N/A"}
                          </span>
                          {milestone.actualDate && (
                            <span>
                              Actual:{" "}
                              {new Date(milestone.actualDate).toLocaleDateString()}
                            </span>
                          )}
                          {milestone.wbsCode && (
                            <span className="text-gray-400">
                              WBS: {milestone.wbsCode}
                            </span>
                          )}
                        </div>
                      </div>

                      <div className="flex items-center gap-2">
                        <span
                          className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(
                            milestone.status
                          )}`}
                        >
                          {milestone.status}
                        </span>
                        <button
                          onClick={() => openEdit(milestone)}
                          className="text-xs text-gray-500 hover:text-primary-600"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => {
                            if (confirm("Delete this milestone?")) {
                              deleteMutation.mutate(milestone.id);
                            }
                          }}
                          className="text-xs text-red-500 hover:text-red-700"
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      <Modal
        title={editing ? "Edit Milestone" : "Add Milestone"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input
              value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Planned Date *</label>
            <input
              type="date"
              value={form.plannedDate}
              onChange={(e) => setForm((f) => ({ ...f, plannedDate: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <SearchableDropdown label="WBS" options={wbsOptions} value={form.wbsId} onChange={(v) => setForm((f) => ({ ...f, wbsId: v ?? "" }))} placeholder="Select WBS" searchPlaceholder="Search WBS..." />
          </div>
          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="isCritical"
              checked={form.isCritical}
              onChange={(e) => setForm((f) => ({ ...f, isCritical: e.target.checked }))}
              className="w-4 h-4 text-primary-500 border-gray-300 rounded focus:ring-primary-500"
            />
            <label htmlFor="isCritical" className="text-sm text-gray-700">
              Mark as Critical
            </label>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}