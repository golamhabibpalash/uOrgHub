import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Plus, X, Upload, Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useDepartmentLookup, useDesignationLookup, useEmployeeLookup } from "../../hooks/useEntityLookup";
import Avatar from "../../components/shared/Avatar";
import ProfilePictureUploader from "../../components/shared/ProfilePictureUploader";
import EmployeeDetailsModal from "./EmployeeDetailsModal";
import {
  getEmployees,
  createEmployee,
  createEmployeeWithUser,
  updateEmployee,
  deleteEmployee,
  getEmployeeDependencies,
  uploadEmployeeProfilePicture,
  deleteEmployeeProfilePicture,
  uploadEmployeeNidPhoto,
  deleteEmployeeNidPhoto,
  Employee,
  Department,
  Designation,
  createDepartment,
  createDesignation,
} from "../../api/hr";
import { getRoles } from "../../api/auth";

export default function Employees() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "firstName" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Employee | null>(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<Employee | null>(null);
  const [detailsId, setDetailsId] = useState<string | null>(null);
  const [checkingDeps, setCheckingDeps] = useState(false);
  const [form, setForm] = useState({
    firstName: "",
    middleName: "",
    lastName: "",
    email: "",
    phone: "",
    gender: "Male",
    bloodGroup: "",
    dateOfBirth: "",
    maritalStatus: "Single",
    religion: "Islam",
    nationality: "Bangladeshi",
    nationalId: "",
    passportNo: "",
    passportExpiry: "",
    currentAddress: "",
    permanentAddress: "",
    division: "",
    district: "",
    upazila: "",
    departmentId: "",
    designationId: "",
    employmentType: "Permanent",
    basicSalary: 0,
    joiningDate: "",
    managerId: "",
  });
  const [createUser, setCreateUser] = useState(false);
  const [userForm, setUserForm] = useState({
    username: "",
    email: "",
    password: "",
    autoGeneratePassword: false,
    isActive: true,
    roleIds: [] as string[],
  });

  // Inline create state — Department / Designation
  const [deptInlineOpen, setDeptInlineOpen] = useState(false);
  const [deptForm, setDeptForm] = useState({ name: "", code: "", description: "" });
  const [deptInlineError, setDeptInlineError] = useState("");
  const [desigInlineOpen, setDesigInlineOpen] = useState(false);
  const [desigForm, setDesigForm] = useState({ name: "", code: "" });
  const [desigInlineError, setDesigInlineError] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["employees", ...dg.queryKey],
    queryFn: () => getEmployees(dg.queryParams),
  });

  const { options: deptOptions, isLoading: deptLoading } = useDepartmentLookup();
  const { options: desigOptions, isLoading: desigLoading } = useDesignationLookup();
  const { options: empOptions, isLoading: empLoading } = useEmployeeLookup();

  const { data: rolesData } = useQuery({
    queryKey: ["roles-all"],
    queryFn: () => getRoles(),
  });

  const employees = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const roles = rolesData ?? [];

  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: async () => {
      const body = { ...form };
      if (!body.joiningDate) delete (body as any).joiningDate;
      if (!body.managerId) delete (body as any).managerId;
      if (!body.bloodGroup) delete (body as any).bloodGroup;
      if (!body.dateOfBirth) delete (body as any).dateOfBirth;
      if (!body.passportExpiry) delete (body as any).passportExpiry;

      if (editing) {
        return updateEmployee(editing.id, body);
      }

      if (createUser) {
        return createEmployeeWithUser({
          employee: body,
          createUserAccount: true,
          userAccount: {
            username: userForm.username,
            email: userForm.email || undefined,
            password: userForm.password,
            autoGeneratePassword: userForm.autoGeneratePassword,
            isActive: userForm.isActive,
            roleIds: userForm.roleIds,
          },
        });
      }

      return createEmployee(body);
    },
    onSuccess: (res) => {
      if (editing) {
        closeModal();
      } else {
        const emp = res?.data?.data as Employee | undefined;
        if (emp?.id) setEditing(emp);
        toast.success("Employee created.");
      }
      qc.invalidateQueries({ queryKey: ["employees"] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteEmployee(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["employees"] });
      toast.success("Employee deleted successfully.");
    },
    onError: (err) => {
      toast.error(extractApiError(err));
    },
  });

  const uploadPicMutation = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => uploadEmployeeProfilePicture(id, file),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["employees"] });
      if (editing) {
        const fresh = qc.getQueryData<{ data: { data: { items: Employee[] } } }>(["employees", ...dg.queryKey]);
        const items = fresh?.data?.data?.items ?? [];
        const found = items.find((e) => e.id === editing.id);
        if (found) setEditing(found);
      }
      toast.success("Profile picture updated.");
    },
  });

  const deletePicMutation = useMutation({
    mutationFn: (id: string) => deleteEmployeeProfilePicture(id),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["employees"] });
      if (editing) {
        setEditing({ ...editing, profilePicturePath: undefined, profilePictureUrl: undefined });
      }
      toast.success("Profile picture removed.");
    },
  });

  const uploadNidMutation = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => uploadEmployeeNidPhoto(id, file),
    onSuccess: async (url) => {
      await qc.invalidateQueries({ queryKey: ["employees"] });
      if (editing) setEditing({ ...editing, nidPhotoUrl: url });
      toast.success("NID photo updated.");
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const deleteNidMutation = useMutation({
    mutationFn: (id: string) => deleteEmployeeNidPhoto(id),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["employees"] });
      if (editing) setEditing({ ...editing, nidPhotoPath: undefined, nidPhotoUrl: undefined });
      toast.success("NID photo removed.");
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  async function handleDeleteClick(emp: Employee) {
    setCheckingDeps(true);
    try {
      const res = await getEmployeeDependencies(emp.id);
      const deps = res.data.data;
      if (!deps) { toast.error("Could not check dependencies."); return; }
      if (!deps.canDelete) {
        toast.error(deps.blockingReason || "This employee cannot be deleted.");
        return;
      }
      setDeleteTarget(emp);
    } catch {
      toast.error("Could not check dependencies.");
    } finally {
      setCheckingDeps(false);
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

  const createDeptMutation = useMutation({
    mutationFn: () => createDepartment(deptForm),
    onSuccess: async (response) => {
      const created = response.data?.data as Department | undefined;
      await qc.invalidateQueries({ queryKey: ["departments-all"] });
      if (created?.id) {
        setForm((f) => ({ ...f, departmentId: created.id, designationId: "" }));
      }
      setDeptForm({ name: "", code: "", description: "" });
      setDeptInlineOpen(false);
      setDeptInlineError("");
      setSuccess(`Department "${created?.name ?? ""}" created and selected.`);
      setTimeout(() => setSuccess(""), 4000);
    },
    onError: (err: Error) => setDeptInlineError(extractApiError(err)),
  });

  const createDesigMutation = useMutation({
    mutationFn: () => {
      if (!form.departmentId) {
        return Promise.reject(new Error("Select a Department before creating a Designation."));
      }
      return createDesignation({ ...desigForm, departmentId: form.departmentId });
    },
    onSuccess: async (response) => {
      const created = response.data?.data as Designation | undefined;
      await qc.invalidateQueries({ queryKey: ["designations-all"] });
      if (created?.id) {
        setForm((f) => ({ ...f, designationId: created.id }));
      }
      setDesigForm({ name: "", code: "" });
      setDesigInlineOpen(false);
      setDesigInlineError("");
      setSuccess(`Designation "${created?.name ?? ""}" created and selected.`);
      setTimeout(() => setSuccess(""), 4000);
    },
    onError: (err: Error) => setDesigInlineError(extractApiError(err)),
  });

  function closeDeptInline() {
    setDeptInlineOpen(false);
    setDeptForm({ name: "", code: "", description: "" });
    setDeptInlineError("");
  }

  function closeDesigInline() {
    setDesigInlineOpen(false);
    setDesigForm({ name: "", code: "" });
    setDesigInlineError("");
  }

  function openAdd() {
    setEditing(null);
    setForm({
      firstName: "",
      middleName: "",
      lastName: "",
      email: "",
      phone: "",
      gender: "Male",
      bloodGroup: "",
      dateOfBirth: "",
      maritalStatus: "Single",
      religion: "Islam",
      nationality: "Bangladeshi",
      nationalId: "",
      passportNo: "",
      passportExpiry: "",
      currentAddress: "",
      permanentAddress: "",
      division: "",
      district: "",
      upazila: "",
      departmentId: "",
      designationId: "",
      employmentType: "Permanent",
      basicSalary: 0,
      joiningDate: "",
      managerId: "",
    });
    setCreateUser(false);
    setUserForm({ username: "", email: "", password: "", autoGeneratePassword: false, isActive: true, roleIds: [] });
    setError("");
    setModal(true);
  }

  function openEdit(emp: Employee) {
    setEditing(emp);
    setForm({
      firstName: emp.firstName,
      middleName: emp.middleName || "",
      lastName: emp.lastName,
      email: emp.email,
      phone: emp.phone,
      gender: emp.gender || "Male",
      bloodGroup: emp.bloodGroup || "",
      dateOfBirth: emp.dateOfBirth ? emp.dateOfBirth.split("T")[0] : "",
      maritalStatus: emp.maritalStatus || "Single",
      religion: emp.religion || "Islam",
      nationality: emp.nationality || "Bangladeshi",
      nationalId: emp.nationalId || "",
      passportNo: emp.passportNo || "",
      passportExpiry: emp.passportExpiry ? emp.passportExpiry.split("T")[0] : "",
      currentAddress: emp.currentAddress || "",
      permanentAddress: emp.permanentAddress || "",
      division: emp.division || "",
      district: emp.district || "",
      upazila: emp.upazila || "",
      departmentId: emp.departmentId,
      designationId: emp.designationId,
      employmentType: emp.employmentType,
      basicSalary: emp.basicSalary,
      joiningDate: emp.joiningDate.split("T")[0],
      managerId: emp.managerId || "",
    });
    setCreateUser(false);
    setError("");
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
    setCreateUser(false);
    setError("");
    closeDeptInline();
    closeDesigInline();
  }

  function toggleRole(id: string) {
    setUserForm((f) => ({
      ...f,
      roleIds: f.roleIds.includes(id)
        ? f.roleIds.filter((x) => x !== id)
        : [...f.roleIds, id],
    }));
  }

  const columns = [
    {
      key: "avatar",
      label: "",
      render: (row: Employee) => (
        <Avatar
          src={row.profilePictureUrl}
          firstName={row.firstName}
          lastName={row.lastName}
          size="sm"
        />
      ),
    },
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
      sortable: false,
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

      {success && (
        <div className="mb-4 bg-emerald-50 border border-emerald-200 text-emerald-700 text-sm rounded-lg px-4 py-2.5">
          {success}
        </div>
      )}

      <DataGrid
        columns={columns}
        data={employees}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search employees..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onView={(row) => setDetailsId(row.id)}
        onEdit={openEdit}
        onDelete={(row) => handleDeleteClick(row)}
        emptyMessage="No employees found"
        actions={<ExportMenu baseUrl="employees" filters={{ search: dg.search || undefined }} />}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        tone="danger"
        title="Delete Employee"
        message={
          <>
            Are you sure you want to delete{" "}
            <span className="font-medium text-gray-900">
              {deleteTarget ? `${deleteTarget.firstName} ${deleteTarget.lastName}` : ""}
            </span>
            ? This action cannot be undone.
          </>
        }
        confirmLabel="Delete"
        loading={deleteMutation.isPending}
        onConfirm={() => {
          if (deleteTarget) deleteMutation.mutate(deleteTarget.id);
          setDeleteTarget(null);
        }}
        onCancel={() => setDeleteTarget(null)}
      />

      <EmployeeDetailsModal employeeId={detailsId} onClose={() => setDetailsId(null)} />

      {checkingDeps && (
        <div className="fixed inset-0 z-[55] flex items-center justify-center bg-black/20 pointer-events-none">
          <div className="bg-white rounded-lg shadow-md px-4 py-2 text-sm text-gray-600">
            Checking dependencies...
          </div>
        </div>
      )}

      <Modal
        title={editing ? "Edit Employee" : "Add Employee"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          {editing ? (
            <div className="bg-gray-50 -mx-5 -mt-3 px-5 py-4 border-b border-gray-100">
              <ProfilePictureUploader
                currentPath={editing.profilePicturePath}
                firstName={editing.firstName}
                lastName={editing.lastName}
                disabled={uploadPicMutation.isPending || deletePicMutation.isPending}
                onUpload={async (file) => { await uploadPicMutation.mutateAsync({ id: editing.id, file }); }}
                onDelete={async () => { await deletePicMutation.mutateAsync(editing.id); }}
              />
            </div>
          ) : (
            <div className="bg-gray-50 -mx-5 -mt-3 px-5 py-6 border-b border-gray-100 text-center">
              <p className="text-sm text-slate-400">Profile picture can be added after saving the employee.</p>
            </div>
          )}
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">First Name *</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.firstName}
                onChange={(e) => setForm((f) => ({ ...f, firstName: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Middle Name</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.middleName}
                onChange={(e) => setForm((f) => ({ ...f, middleName: e.target.value }))}
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
              <label className="text-xs text-gray-500 mb-1 block">Gender</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.gender}
                onChange={(e) => setForm((f) => ({ ...f, gender: e.target.value }))}
              >
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Blood Group</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.bloodGroup}
                onChange={(e) => setForm((f) => ({ ...f, bloodGroup: e.target.value }))}
              >
                <option value="">Select</option>
                <option value="APositive">A+</option>
                <option value="ANegative">A-</option>
                <option value="BPositive">B+</option>
                <option value="BNegative">B-</option>
                <option value="ABPositive">AB+</option>
                <option value="ABNegative">AB-</option>
                <option value="OPositive">O+</option>
                <option value="ONegative">O-</option>
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Date of Birth</label>
              <input
                type="date"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.dateOfBirth}
                onChange={(e) => setForm((f) => ({ ...f, dateOfBirth: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Marital Status</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.maritalStatus}
                onChange={(e) => setForm((f) => ({ ...f, maritalStatus: e.target.value }))}
              >
                <option value="Single">Single</option>
                <option value="Married">Married</option>
                <option value="Divorced">Divorced</option>
                <option value="Widowed">Widowed</option>
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Religion</label>
              <select
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.religion}
                onChange={(e) => setForm((f) => ({ ...f, religion: e.target.value }))}
              >
                <option value="Islam">Islam</option>
                <option value="Hinduism">Hinduism</option>
                <option value="Christianity">Christianity</option>
                <option value="Buddhism">Buddhism</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Nationality</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.nationality}
                onChange={(e) => setForm((f) => ({ ...f, nationality: e.target.value }))}
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">NID Number</label>
            <input
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={form.nationalId}
              onChange={(e) => setForm((f) => ({ ...f, nationalId: e.target.value }))}
              placeholder="National ID number"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Passport No.</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.passportNo}
                onChange={(e) => setForm((f) => ({ ...f, passportNo: e.target.value }))}
                placeholder="Passport number"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Passport Expiry</label>
              <input
                type="date"
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.passportExpiry}
                onChange={(e) => setForm((f) => ({ ...f, passportExpiry: e.target.value }))}
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">NID Photo</label>
            {editing ? (
              <div className="flex items-center gap-3">
                {editing.nidPhotoUrl ? (
                  <img
                    src={editing.nidPhotoUrl}
                    alt="NID"
                    className="h-20 w-32 object-cover rounded-lg border border-gray-200"
                  />
                ) : (
                  <div className="h-20 w-32 flex items-center justify-center rounded-lg border border-dashed border-gray-300 text-[11px] text-gray-400">
                    No photo
                  </div>
                )}
                <div className="flex flex-col gap-1.5">
                  <label className="cursor-pointer inline-flex items-center gap-1.5 text-xs border border-gray-200 rounded-lg px-3 py-1.5 hover:bg-gray-50">
                    <Upload size={13} /> {editing.nidPhotoUrl ? "Change" : "Upload"}
                    <input
                      type="file"
                      accept="image/*"
                      className="hidden"
                      disabled={uploadNidMutation.isPending}
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file && editing) uploadNidMutation.mutate({ id: editing.id, file });
                        e.target.value = "";
                      }}
                    />
                  </label>
                  {editing.nidPhotoUrl && (
                    <button
                      type="button"
                      onClick={() => editing && deleteNidMutation.mutate(editing.id)}
                      disabled={deleteNidMutation.isPending}
                      className="inline-flex items-center gap-1.5 text-xs text-red-600 border border-red-200 rounded-lg px-3 py-1.5 hover:bg-red-50 disabled:opacity-50"
                    >
                      <Trash2 size={13} /> Remove
                    </button>
                  )}
                </div>
              </div>
            ) : (
              <p className="text-xs text-slate-400">NID photo can be uploaded after saving the employee.</p>
            )}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Present Address</label>
              <textarea
                rows={2}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.currentAddress}
                onChange={(e) => setForm((f) => ({ ...f, currentAddress: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Permanent Address</label>
              <textarea
                rows={2}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.permanentAddress}
                onChange={(e) => setForm((f) => ({ ...f, permanentAddress: e.target.value }))}
              />
            </div>
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Division</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.division}
                onChange={(e) => setForm((f) => ({ ...f, division: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">District</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.district}
                onChange={(e) => setForm((f) => ({ ...f, district: e.target.value }))}
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Upazila</label>
              <input
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.upazila}
                onChange={(e) => setForm((f) => ({ ...f, upazila: e.target.value }))}
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <div className="flex gap-1.5">
                <SearchableDropdown
                  label="Department"
                  options={deptOptions}
                  value={form.departmentId}
                  onChange={(v) => setForm((f) => ({ ...f, departmentId: v || "", designationId: "" }))}
                  placeholder="Select Department"
                  searchPlaceholder="Search departments..."
                  loading={deptLoading}
                  required
                  className="flex-1"
                />
                <button
                  type="button"
                  onClick={() => {
                    setDeptInlineError("");
                    setDeptInlineOpen((v) => !v);
                  }}
                  title="Create new department"
                  className={`shrink-0 w-9 rounded-lg border transition-colors flex items-center justify-center ${
                    deptInlineOpen
                      ? "bg-primary-500 border-primary-500 text-white hover:bg-primary-600"
                      : "border-primary-200 bg-primary-50 text-primary-600 hover:bg-primary-100"
                  }`}
                >
                  <Plus size={14} />
                </button>
              </div>
            </div>
            <div>
              <div className="flex gap-1.5">
                <SearchableDropdown
                  label="Designation"
                  options={desigOptions}
                  value={form.designationId}
                  onChange={(v) => setForm((f) => ({ ...f, designationId: v || "" }))}
                  placeholder="Select Designation"
                  searchPlaceholder="Search designations..."
                  loading={desigLoading}
                  required
                  className="flex-1"
                />
                <button
                  type="button"
                  onClick={() => {
                    setDesigInlineError("");
                    setDesigInlineOpen((v) => !v);
                  }}
                  title="Create new designation"
                  className={`shrink-0 w-9 rounded-lg border transition-colors flex items-center justify-center ${
                    desigInlineOpen
                      ? "bg-primary-500 border-primary-500 text-white hover:bg-primary-600"
                      : "border-primary-200 bg-primary-50 text-primary-600 hover:bg-primary-100"
                  }`}
                >
                  <Plus size={14} />
                </button>
              </div>
            </div>
          </div>

          {/* Manager */}
          <div>
            <SearchableDropdown
              label="Reports To (Manager)"
              options={empOptions}
              value={form.managerId}
              onChange={(v) => setForm((f) => ({ ...f, managerId: v || "" }))}
              placeholder="No Manager (Top Level)"
              searchPlaceholder="Search employees..."
              loading={empLoading}
              clearable
            />
          </div>

          {deptInlineOpen && (
            <div className="border border-primary-200 bg-primary-50/40 rounded-lg p-3 space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-primary-700">New Department</span>
                <button
                  type="button"
                  onClick={closeDeptInline}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <X size={12} />
                </button>
              </div>
              <div className="grid grid-cols-2 gap-2">
                <input
                  autoFocus
                  placeholder="Department Name *"
                  value={deptForm.name}
                  onChange={(e) => setDeptForm((f) => ({ ...f, name: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 bg-white"
                />
                <input
                  placeholder="Code *"
                  value={deptForm.code}
                  onChange={(e) => setDeptForm((f) => ({ ...f, code: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 bg-white"
                />
              </div>
              <input
                placeholder="Description (optional)"
                value={deptForm.description}
                onChange={(e) => setDeptForm((f) => ({ ...f, description: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 bg-white"
              />
              {deptInlineError && (
                <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded px-2 py-1">
                  {deptInlineError}
                </div>
              )}
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={closeDeptInline}
                  className="px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (!deptForm.name.trim() || !deptForm.code.trim()) {
                      setDeptInlineError("Department Name and Code are required.");
                      return;
                    }
                    createDeptMutation.mutate();
                  }}
                  disabled={createDeptMutation.isPending}
                  className="px-3 py-1.5 text-xs bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
                >
                  {createDeptMutation.isPending ? "Saving..." : "Save Department"}
                </button>
              </div>
            </div>
          )}

          {desigInlineOpen && (
            <div className="border border-primary-200 bg-primary-50/40 rounded-lg p-3 space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-primary-700">New Designation</span>
                <button
                  type="button"
                  onClick={closeDesigInline}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <X size={12} />
                </button>
              </div>
              {!form.departmentId ? (
                <div className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded px-2 py-1.5">
                  Select a Department first — the new designation will belong to it.
                </div>
              ) : (
                <div className="text-xs text-gray-500">
                  Will be added under <span className="text-gray-700 font-medium">{deptOptions.find((d) => d.value === form.departmentId)?.label}</span>.
                </div>
              )}
              <div className="grid grid-cols-2 gap-2">
                <input
                  autoFocus
                  placeholder="Designation Name *"
                  value={desigForm.name}
                  onChange={(e) => setDesigForm((f) => ({ ...f, name: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 bg-white"
                />
                <input
                  placeholder="Code *"
                  value={desigForm.code}
                  onChange={(e) => setDesigForm((f) => ({ ...f, code: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 bg-white"
                />
              </div>
              {desigInlineError && (
                <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded px-2 py-1">
                  {desigInlineError}
                </div>
              )}
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={closeDesigInline}
                  className="px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  onClick={() => {
                    if (!form.departmentId) {
                      setDesigInlineError("Select a Department first.");
                      return;
                    }
                    if (!desigForm.name.trim() || !desigForm.code.trim()) {
                      setDesigInlineError("Designation Name and Code are required.");
                      return;
                    }
                    createDesigMutation.mutate();
                  }}
                  disabled={createDesigMutation.isPending || !form.departmentId}
                  className="px-3 py-1.5 text-xs bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
                >
                  {createDesigMutation.isPending ? "Saving..." : "Save Designation"}
                </button>
              </div>
            </div>
          )}
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

          {/* Create User Account Section */}
          {!editing && (
            <>
              <hr className="border-gray-200" />
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="createUser"
                  checked={createUser}
                  onChange={(e) => setCreateUser(e.target.checked)}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <label htmlFor="createUser" className="text-sm font-medium text-gray-700">
                  Create User Account
                </label>
              </div>

              {createUser && (
                <div className="space-y-3 pl-1 border-l-2 border-primary-200 pl-3">
                  <p className="text-xs text-gray-400">User account will be linked to this employee record.</p>

                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-xs text-gray-500 mb-1 block">Username *</label>
                      <input
                        className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={userForm.username}
                        onChange={(e) => setUserForm((f) => ({ ...f, username: e.target.value }))}
                        placeholder="e.g. johndoe"
                      />
                    </div>
                    <div>
                      <label className="text-xs text-gray-500 mb-1 block">Email</label>
                      <input
                        type="email"
                        className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={userForm.email}
                        onChange={(e) => setUserForm((f) => ({ ...f, email: e.target.value }))}
                        placeholder="Leave blank to use employee email"
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-xs text-gray-500 mb-1 block">Password</label>
                      <input
                        type="password"
                        className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 disabled:bg-gray-100 disabled:text-gray-400"
                        value={userForm.password}
                        onChange={(e) => setUserForm((f) => ({ ...f, password: e.target.value }))}
                        disabled={userForm.autoGeneratePassword}
                        placeholder={userForm.autoGeneratePassword ? "Auto-generated" : "Min 8 chars"}
                      />
                    </div>
                    <div className="flex items-end pb-2">
                      <label className="flex items-center gap-2 text-sm text-gray-600">
                        <input
                          type="checkbox"
                          checked={userForm.autoGeneratePassword}
                          onChange={(e) => setUserForm((f) => ({ ...f, autoGeneratePassword: e.target.checked }))}
                          className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                        />
                        Auto-generate
                      </label>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <label className="text-xs text-gray-500">Account Status</label>
                    <button
                      type="button"
                      onClick={() => setUserForm((f) => ({ ...f, isActive: true }))}
                      className={`px-3 py-1 text-xs rounded-full border transition-colors ${
                        userForm.isActive
                          ? "bg-green-50 border-green-300 text-green-700"
                          : "border-gray-200 text-gray-400"
                      }`}
                    >
                      Active
                    </button>
                    <button
                      type="button"
                      onClick={() => setUserForm((f) => ({ ...f, isActive: false }))}
                      className={`px-3 py-1 text-xs rounded-full border transition-colors ${
                        !userForm.isActive
                          ? "bg-red-50 border-red-300 text-red-700"
                          : "border-gray-200 text-gray-400"
                      }`}
                    >
                      Inactive
                    </button>
                  </div>

                  <div>
                    <label className="text-xs text-gray-500 mb-1 block">Roles</label>
                    <div className="flex flex-wrap gap-1.5">
                      {roles.map((role) => (
                        <button
                          key={role.id}
                          type="button"
                          onClick={() => toggleRole(role.id)}
                          className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                            userForm.roleIds.includes(role.id)
                              ? "bg-primary-50 border-primary-300 text-primary-700"
                              : "border-gray-200 text-gray-500 hover:border-gray-300"
                          }`}
                        >
                          {role.name}
                        </button>
                      ))}
                    </div>
                  </div>
                </div>
              )}
            </>
          )}

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
              {saveMutation.isPending ? "Saving..." : editing ? "Update" : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
