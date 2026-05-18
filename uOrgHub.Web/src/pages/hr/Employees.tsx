import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getEmployees,
  createEmployee,
  updateEmployee,
  deleteEmployee,
  Employee,
  getDepartments,
  getDesignations,
} from "../../api/hr";

export default function Employees() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Employee | null>(null);
  const [error, setError] = useState("");
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    departmentId: "",
    designationId: "",
    employmentType: "Permanent",
    basicSalary: 0,
    joiningDate: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["employees", page, search],
    queryFn: () => getEmployees({ page, pageSize: 10, search }),
  });

  const { data: deptData } = useQuery({
    queryKey: ["departments-all"],
    queryFn: () => getDepartments({ page: 1, pageSize: 100 }),
  });

  const { data: desigData } = useQuery({
    queryKey: ["designations-all"],
    queryFn: () => getDesignations({ page: 1, pageSize: 100 }),
  });

  const employees = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const departments = deptData?.data?.data?.items ?? [];
  const allDesignations = desigData?.data?.data?.items ?? [];
  const designations = form.departmentId
    ? allDesignations.filter((d) => d.departmentId === form.departmentId)
    : allDesignations;

  const saveMutation = useMutation({
    mutationFn: () => {
      const body = { ...form };
      if (!body.joiningDate) delete (body as any).joiningDate;
      return editing
        ? updateEmployee(editing.id, body)
        : createEmployee(body);
    },
    onSuccess: () => {
      closeModal();
      qc.invalidateQueries({ queryKey: ["employees"] });
    },
    onError: (err: Error) => {
      const axiosErr = err as AxiosError<{
        message?: string;
        errors?: string[] | Record<string, string[]>;
      }>;
      const data = axiosErr.response?.data;
      let msg = "";
      if (typeof data?.message === "string") {
        msg = data.message;
      } else if (data?.errors) {
        if (Array.isArray(data.errors)) {
          msg = data.errors[0];
        } else {
          const vals = Object.values(data.errors);
          msg = vals.flat()[0] || "";
        }
      }
      setError(msg || err.message || "An error occurred");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteEmployee(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["employees"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      firstName: "",
      lastName: "",
      email: "",
      phone: "",
      departmentId: "",
      designationId: "",
      employmentType: "Permanent",
      basicSalary: 0,
      joiningDate: "",
    });
    setModal(true);
  }

  function openEdit(emp: Employee) {
    setEditing(emp);
    setForm({
      firstName: emp.firstName,
      lastName: emp.lastName,
      email: emp.email,
      phone: emp.phone,
      departmentId: emp.departmentId,
      designationId: emp.designationId,
      employmentType: emp.employmentType,
      basicSalary: emp.basicSalary,
      joiningDate: emp.joiningDate.split("T")[0],
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const columns = [
    { key: "employeeCode", label: "Code" },
    {
      key: "name",
      label: "Name",
      render: (row: Employee) => `${row.firstName} ${row.lastName}`,
    },
    { key: "email", label: "Email" },
    { key: "departmentName", label: "Department" },
    { key: "designationName", label: "Designation" },
    {
      key: "status",
      label: "Status",
      render: (row: Employee) => (
        <span
          className={`text-xs px-2 py-0.5 rounded-full ${
            row.status === "Active"
              ? "bg-green-50 text-green-700"
              : row.status === "OnLeave"
              ? "bg-yellow-50 text-yellow-700"
              : "bg-gray-50 text-gray-600"
          }`}
        >
          {row.status}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Employees</h2>
          <p className="text-xs text-gray-400">Manage employee records</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Employee
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search employees..."
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
          data={employees}
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
        title={editing ? "Edit Employee" : "Add Employee"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">First Name *</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.firstName}
                onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Last Name *</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.lastName}
                onChange={(e) => setForm((f) => ({ ...f, lastName: e.target.value }))}
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Email *</label>
            <input
              type="email"
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.email}
              onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Phone</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.phone}
                onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Joining Date</label>
              <input
                type="date"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.joiningDate}
                onChange={(e) => setForm((f) => ({ ...f, joiningDate: e.target.value }))}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Department</label>
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
              <label className="text-xs text-gray-500 mb-1 block">Designation</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.designationId}
                onChange={(e) => setForm((f) => ({ ...f, designationId: e.target.value }))}
              >
                <option value="">Select Designation</option>
                {designations.map((d) => (
                  <option key={d.id} value={d.id}>{d.name}</option>
                ))}
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Employment Type</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.employmentType}
                onChange={(e) => setForm((f) => ({ ...f, employmentType: e.target.value }))}
              >
                <option value="Permanent">Permanent</option>
                <option value="Contract">Contract</option>
                <option value="Daily">Daily</option>
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Basic Salary</label>
              <input
                type="number"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.basicSalary}
                onChange={(e) => setForm((f) => ({ ...f, basicSalary: Number(e.target.value) }))}
              />
            </div>
          </div>
          {error && (
            <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {error}
            </div>
          )}
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