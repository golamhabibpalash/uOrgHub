import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Plus } from "lucide-react";
import toast from "react-hot-toast";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getDesignations,
  getAllDesignations,
  createDesignation,
  updateDesignation,
  deleteDesignation,
  getDesignationDependencies,
  Designation,
  getDepartments,
  getAllSalaryGrades,
} from "../../api/hr";

export default function Designations() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [deptFilter, setDeptFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Designation | null>(null);
  const [form, setForm] = useState({ name: "", code: "", departmentId: "", level: 1, isActive: true, parentDesignationId: "", salaryGradeId: "" });
  const [deleteTarget, setDeleteTarget] = useState<Designation | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["designations", page, search, deptFilter],
    queryFn: () => getDesignations({ page, pageSize: 10, search }, deptFilter || undefined),
  });

  const { data: deptData } = useQuery({
    queryKey: ["departments-all"],
    queryFn: () => getDepartments({ page: 1, pageSize: 100 }),
  });

  const { data: allDesigData } = useQuery({
    queryKey: ["designations-all"],
    queryFn: () => getAllDesignations(),
  });

  const { data: salaryGradeData } = useQuery({
    queryKey: ["salary-grades-all"],
    queryFn: () => getAllSalaryGrades(),
  });

  const designations = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const departments = deptData?.data?.data?.items ?? [];
  const allDesignations = allDesigData?.data?.data ?? [];
  const salaryGrades = salaryGradeData?.data?.data ?? [];

  const parentOptions = allDesignations.filter((d) =>
    editing ? d.id !== editing.id : true
  );

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload = {
        name: form.name,
        code: form.code,
        departmentId: form.departmentId,
        level: form.level,
        isActive: form.isActive,
        parentDesignationId: form.parentDesignationId || undefined,
        salaryGradeId: form.salaryGradeId || undefined,
      };
      return editing
        ? updateDesignation(editing.id, payload)
        : createDesignation(payload);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["designations"] });
      qc.invalidateQueries({ queryKey: ["designations-all"] });
      closeModal();
    },
    onError: (err) => {
      toast.error(extractApiError(err));
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteDesignation(id),
    onSuccess: () => {
      setDeleteTarget(null);
      qc.invalidateQueries({ queryKey: ["designations"] });
      toast.success("Designation deleted successfully.");
    },
    onError: (err) => {
      toast.error(extractApiError(err));
    },
  });

  async function handleDeleteClick(desig: Designation) {
    try {
      const res = await getDesignationDependencies(desig.id);
      const deps = res.data.data;
      if (!deps) { toast.error("Could not check dependencies."); return; }
      if (!deps.canDelete) {
        toast.error(deps.blockingReason || "This designation cannot be deleted.");
        return;
      }
      setDeleteTarget(desig);
    } catch {
      toast.error("Could not check dependencies.");
    }
  }

  function extractApiError(err: unknown): string {
    const axiosErr = err as AxiosError<{
      message?: string;
      errors?: string[] | Record<string, string[]>;
    }>;
    const body = axiosErr.response?.data;
    if (typeof body?.message === "string") return body.message;
    if (body?.errors) {
      if (Array.isArray(body.errors)) return body.errors[0] ?? "";
      const first = Object.values(body.errors).flat()[0];
      if (first) return first;
    }
    return (err as Error)?.message ?? "An error occurred";
  }

  function openAdd() {
    setEditing(null);
    setForm({ name: "", code: "", departmentId: "", level: 1, isActive: true, parentDesignationId: "", salaryGradeId: "" });
    setModal(true);
  }

  function openEdit(desig: Designation) {
    setEditing(desig);
    setForm({
      name: desig.name,
      code: desig.code,
      departmentId: desig.departmentId,
      level: desig.level,
      isActive: desig.isActive,
      parentDesignationId: desig.parentDesignationId ?? "",
      salaryGradeId: desig.salaryGradeId ?? "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Designation Name" },
    { key: "departmentName", label: "Department" },
    {
      key: "parentDesignationName",
      label: "Parent Designation",
      render: (row: Designation) => (
        <span className="text-sm text-gray-600">
          {row.parentDesignationName || "—"}
        </span>
      ),
    },
    {
      key: "level",
      label: "Level",
      render: (row: Designation) => (
        <span className="text-sm text-gray-600">{row.level}</span>
      ),
    },
    {
      key: "isActive",
      label: "Status",
      render: (row: Designation) => (
        <span
          className={`text-xs px-2 py-0.5 rounded-full ${
            row.isActive
              ? "bg-green-50 text-green-700"
              : "bg-red-50 text-red-600"
          }`}
        >
          {row.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Designations</h2>
          <p className="text-xs text-gray-400">Manage job titles and positions</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Designation
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
        <div className="flex gap-3">
          <input
            type="text"
            placeholder="Search designations..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <select
            value={deptFilter}
            onChange={(e) => {
              setDeptFilter(e.target.value);
              setPage(1);
            }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All Departments</option>
            {departments.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
        </div>
        <ExportMenu baseUrl="designations" filters={{ search: search || undefined, departmentId: deptFilter || undefined }} />
      </div>
        <DataTable
          columns={columns}
          data={designations}
          loading={isLoading}
          onEdit={openEdit}
          onDelete={(row) => handleDeleteClick(row)}
        />
        <Pagination
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      </div>

      <Modal
        title={editing ? "Edit Designation" : "Add Designation"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Designation Name *</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.name}
              onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Code *</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.code}
              onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Department *</label>
            <select
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.departmentId}
              onChange={(e) => setForm((f) => ({ ...f, departmentId: e.target.value }))}
            >
              <option value="">Select Department</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>{d.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Level *</label>
            <input
              type="number"
              min="1"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.level}
              onChange={(e) => setForm((f) => ({ ...f, level: parseInt(e.target.value) || 1 }))}
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Parent Designation</label>
            <select
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.parentDesignationId}
              onChange={(e) => setForm((f) => ({ ...f, parentDesignationId: e.target.value }))}
            >
              <option value="">None (Top-Level Designation)</option>
              {parentOptions.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Salary Grade</label>
            <select
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.salaryGradeId}
              onChange={(e) => setForm((f) => ({ ...f, salaryGradeId: e.target.value }))}
            >
              <option value="">None</option>
              {salaryGrades.map((g) => (
                <option key={g.id} value={g.id}>
                  {g.gradeCode} — {g.name}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Status</label>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => setForm((f) => ({ ...f, isActive: true }))}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  form.isActive
                    ? "bg-green-50 text-green-700 border border-green-200"
                    : "bg-gray-50 text-gray-400 border border-gray-200"
                }`}
              >
                Active
              </button>
              <button
                type="button"
                onClick={() => setForm((f) => ({ ...f, isActive: false }))}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  !form.isActive
                    ? "bg-red-50 text-red-600 border border-red-200"
                    : "bg-gray-50 text-gray-400 border border-gray-200"
                }`}
              >
                Inactive
              </button>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => {
                if (!form.name.trim() || !form.code.trim() || !form.departmentId) {
                  toast.error("Designation Name, Code, and Department are required.");
                  return;
                }
                saveMutation.mutate();
              }}
              disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Designation"
        message={
          <span>
            Are you sure you want to delete{" "}
            <strong>{deleteTarget?.name}</strong>? This action cannot be undone.
          </span>
        }
        confirmLabel="Delete"
        tone="danger"
        loading={deleteMutation.isPending}
        onConfirm={() => { if (deleteTarget) deleteMutation.mutate(deleteTarget.id); }}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
