import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getDesignations,
  createDesignation,
  updateDesignation,
  deleteDesignation,
  Designation,
  getDepartments,
} from "../../api/hr";

export default function Designations() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [deptFilter, setDeptFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Designation | null>(null);
  const [form, setForm] = useState({ name: "", code: "", departmentId: "" });

  const { data, isLoading } = useQuery({
    queryKey: ["designations", page, search, deptFilter],
    queryFn: () => getDesignations({ page, pageSize: 10, search }, deptFilter || undefined),
  });

  const { data: deptData } = useQuery({
    queryKey: ["departments-all"],
    queryFn: () => getDepartments({ page: 1, pageSize: 100 }),
  });

  const designations = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const departments = deptData?.data?.data?.items ?? [];

  const saveMutation = useMutation({
    mutationFn: () =>
      editing
        ? updateDesignation(editing.id, form)
        : createDesignation(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["designations"] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteDesignation(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["designations"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", code: "", departmentId: "" });
    setModal(true);
  }

  function openEdit(desig: Designation) {
    setEditing(desig);
    setForm({ name: desig.name, code: desig.code, departmentId: desig.departmentId });
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
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3">
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
        <DataTable
          columns={columns}
          data={designations}
          loading={isLoading}
          onEdit={openEdit}
          onDelete={(row) => deleteMutation.mutate(row.id)}
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