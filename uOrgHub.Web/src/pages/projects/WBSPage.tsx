import { useState, useMemo } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, ChevronDown, ChevronRight } from "lucide-react";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import ProjectNav from "../../components/projects/ProjectNav";
import { getWBSList, createWBS, updateWBS, deleteWBS, updateWBSCompletion, WBS } from "../../api/projects";

export default function WBSPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<WBS | null>(null);
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [form, setForm] = useState({
    wbsCode: "",
    title: "",
    parentWbsId: "",
    plannedStartDate: "",
    plannedEndDate: "",
    description: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["wbs", projectId],
    queryFn: () => getWBSList(projectId!),
    enabled: !!projectId,
  });

  const wbsList = data?.data?.data ?? [];

  const wbsParentOptions = useMemo(
    () => wbsList.filter((w) => w.id !== editing?.id).map((w) => ({ value: w.id, label: `${w.wbsCode} - ${w.title}` })),
    [wbsList, editing],
  );

  const saveMutation = useMutation({
    mutationFn: () =>
      editing
        ? updateWBS(projectId!, editing.id, form)
        : createWBS(projectId!, form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["wbs", projectId] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (wbsId: string) => deleteWBS(projectId!, wbsId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["wbs", projectId] }),
  });

  const completionMutation = useMutation({
    mutationFn: ({ wbsId, percent }: { wbsId: string; percent: number }) =>
      updateWBSCompletion(projectId!, wbsId, percent),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["wbs", projectId] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      wbsCode: "",
      title: "",
      parentWbsId: "",
      plannedStartDate: "",
      plannedEndDate: "",
      description: "",
    });
    setModal(true);
  }

  function openEdit(wbs: WBS) {
    setEditing(wbs);
    setForm({
      wbsCode: wbs.wbsCode,
      title: wbs.title,
      parentWbsId: wbs.parentWbsId || "",
      plannedStartDate: wbs.plannedStartDate?.split("T")[0] || "",
      plannedEndDate: wbs.plannedEndDate?.split("T")[0] || "",
      description: wbs.description || "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  function toggleExpand(wbsId: string) {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(wbsId)) {
        next.delete(wbsId);
      } else {
        next.add(wbsId);
      }
      return next;
    });
  }

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      NotStarted: "bg-gray-100 text-gray-600",
      InProgress: "bg-blue-50 text-blue-700",
      Completed: "bg-green-50 text-green-700",
      OnHold: "bg-amber-50 text-amber-700",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

  const buildTree = (items: WBS[], parentId?: string): WBS[] => {
    return items
      .filter((item) => item.parentWbsId === parentId)
      .map((item) => ({
        ...item,
        children: buildTree(items, item.id),
      }));
  };

  const tree = buildTree(wbsList);

  const renderRow = (wbs: WBS, level: number = 0) => {
    const hasChildren = wbs.children && wbs.children.length > 0;
    const isExpanded = expanded.has(wbs.id);

    return (
      <tbody key={wbs.id}>
        <tr className="border-t border-gray-100 hover:bg-gray-50">
          <td className="px-4 py-2.5" style={{ paddingLeft: `${level * 24 + 16}px` }}>
            <div className="flex items-center gap-2">
              {hasChildren ? (
                <button
                  onClick={() => toggleExpand(wbs.id)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  {isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                </button>
              ) : (
                <span className="w-[14px]" />
              )}
              <span className="font-medium text-gray-900">{wbs.wbsCode}</span>
            </div>
          </td>
          <td className="px-4 py-2.5 text-gray-700">{wbs.title}</td>
          <td className="px-4 py-2.5 text-gray-700">
            {wbs.plannedStartDate
              ? new Date(wbs.plannedStartDate).toLocaleDateString()
              : "-"}
          </td>
          <td className="px-4 py-2.5 text-gray-700">
            {wbs.plannedEndDate
              ? new Date(wbs.plannedEndDate).toLocaleDateString()
              : "-"}
          </td>
          <td className="px-4 py-2.5">
            <div className="flex items-center gap-2 w-24">
              <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary-500 rounded-full"
                  style={{ width: `${wbs.completionPercent}%` }}
                />
              </div>
              <span className="text-xs text-gray-500">
                {wbs.completionPercent}%
              </span>
            </div>
          </td>
          <td className="px-4 py-2.5">
            <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(wbs.status)}`}>
              {wbs.status}
            </span>
          </td>
          <td className="px-4 py-2.5">
            <div className="flex items-center gap-2">
              <button
                onClick={() => openEdit(wbs)}
                className="text-xs text-gray-500 hover:text-primary-600"
              >
                Edit
              </button>
              <button
                onClick={() => {
                  const newPercent = prompt(
                    "Enter completion percentage:",
                    String(wbs.completionPercent)
                  );
                  if (newPercent !== null) {
                    completionMutation.mutate({
                      wbsId: wbs.id,
                      percent: parseInt(newPercent) || 0,
                    });
                  }
                }}
                className="text-xs text-blue-500 hover:text-blue-700"
              >
                Update %
              </button>
              <button
                onClick={() => {
                  if (confirm("Delete this WBS?")) {
                    deleteMutation.mutate(wbs.id);
                  }
                }}
                className="text-xs text-red-500 hover:text-red-700"
              >
                Delete
              </button>
            </div>
          </td>
        </tr>
        {hasChildren &&
          isExpanded &&
          wbs.children!.map((child) => renderRow(child, level + 1))}
      </tbody>
    );
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
          <h2 className="text-base font-medium text-gray-900">Work Breakdown Structure</h2>
          <p className="text-xs text-gray-400">Manage project WBS hierarchy</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add WBS
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : wbsList.length === 0 ? (
          <div className="text-center py-10 text-gray-400">
            No WBS items found
          </div>
        ) : (
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  WBS Code
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Title
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Planned Start
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Planned End
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Completion
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Status
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            {tree.map((item) => renderRow(item))}
          </table>
        )}
      </div>

      <Modal
        title={editing ? "Edit WBS" : "Add WBS"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">WBS Code *</label>
            <input
              value={form.wbsCode}
              onChange={(e) => setForm((f) => ({ ...f, wbsCode: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input
              value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <SearchableDropdown label="Parent WBS" options={wbsParentOptions} value={form.parentWbsId} onChange={(v) => setForm((f) => ({ ...f, parentWbsId: v ?? "" }))} placeholder="No Parent" searchPlaceholder="Search WBS..." clearable />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Planned Start</label>
              <input
                type="date"
                value={form.plannedStartDate}
                onChange={(e) => setForm((f) => ({ ...f, plannedStartDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Planned End</label>
              <input
                type="date"
                value={form.plannedEndDate}
                onChange={(e) => setForm((f) => ({ ...f, plannedEndDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
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