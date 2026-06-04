import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import toast from "react-hot-toast";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import {
  getDepartments,
  createDepartment,
  updateDepartment,
  deleteDepartment,
  getDepartmentDependencies,
  Department,
} from "../../api/hr";

export default function Departments() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);
  const [form, setForm] = useState({ name: "", code: "", description: "" });
  const [deleteTarget, setDeleteTarget] = useState<Department | null>(null);
  const [checkingDeps, setCheckingDeps] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["departments", page, search],
    queryFn: () => getDepartments({ page, pageSize: 10, search }),
  });

  const departments = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;

  const saveMutation = useMutation({
    mutationFn: () =>
      editing ? updateDepartment(editing.id, form) : createDepartment(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["departments"] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteDepartment(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["departments"] });
      qc.invalidateQueries({ queryKey: ["departments-all"] });
      setDeleteTarget(null);
    },
    onError: () => setDeleteTarget(null),
  });

  async function handleDeleteClick(dep: Department) {
    setCheckingDeps(true);
    try {
      const response = await getDepartmentDependencies(dep.id);
      const deps = response.data.data;
      if (!deps) {
        toast.error("Could not check dependencies. Please try again.");
        return;
      }
      if (!deps.canDelete) {
        toast.error(deps.blockingReason || "This department cannot be deleted.");
        return;
      }
      setDeleteTarget(dep);
    } catch {
      toast.error("Could not check dependencies. Please try again.");
    } finally {
      setCheckingDeps(false);
    }
  }

  function openAdd() {
    setEditing(null);
    setForm({ name: "", code: "", description: "" });
    setModal(true);
  }

  function openEdit(dep: Department) {
    setEditing(dep);
    setForm({ name: dep.name, code: dep.code, description: dep.description });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Department Name" },
    { key: "description", label: "Description" },
    {
      key: "isActive",
      label: "Status",
      render: (row: Department) => (
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
          <h2 className="text-base font-medium text-gray-900">Departments</h2>
          <p className="text-xs text-gray-400">Manage company departments</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Department
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search departments..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
        <DataTable
          columns={columns}
          data={departments}
          loading={isLoading}
          onEdit={openEdit}
          onDelete={handleDeleteClick}
        />
        <Pagination
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      </div>

      <Modal
        title={editing ? "Edit Department" : "Add Department"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">
              Department Name *
            </label>
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
            <label className="text-xs text-gray-500 mb-1 block">
              Description
            </label>
            <textarea
              rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.description}
              onChange={(e) =>
                setForm((f) => ({ ...f, description: e.target.value }))
              }
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

      <ConfirmDialog
        open={!!deleteTarget}
        tone="danger"
        title="Delete Department"
        message={
          <>
            Are you sure you want to delete{" "}
            <span className="font-medium text-gray-900">
              {deleteTarget?.name}
            </span>
            ? This action cannot be undone.
          </>
        }
        confirmLabel="Delete"
        loading={deleteMutation.isPending}
        onConfirm={() => {
          if (deleteTarget) deleteMutation.mutate(deleteTarget.id);
        }}
        onCancel={() => setDeleteTarget(null)}
      />

      {checkingDeps && (
        <div className="fixed inset-0 z-[55] flex items-center justify-center bg-black/20 pointer-events-none">
          <div className="bg-white rounded-lg shadow-md px-4 py-2 text-sm text-gray-600">
            Checking dependencies...
          </div>
        </div>
      )}
    </div>
  );
}
