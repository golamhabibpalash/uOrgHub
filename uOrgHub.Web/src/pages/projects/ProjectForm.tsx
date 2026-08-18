import { useState, useEffect, useMemo } from "react";
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import { createProject, updateProject, getProjectCategories, getClients, createClient, createProjectCategory, Project, VALID_STATUS_TRANSITIONS } from "../../api/projects";
import { extractApiError } from "../../utils/apiError";
import { amountInWords } from "../../utils/format";
import { useEmployeeLookup } from "../../hooks/useEntityLookup";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import Modal from "../../components/shared/Modal";
import DateInput from "../../components/shared/DateInput";

interface ProjectFormProps {
  project: Project | null;
  onClose: () => void;
}

export default function ProjectForm({ project, onClose }: ProjectFormProps) {
  const qc = useQueryClient();
  const [error, setError] = useState("");
  const [form, setForm] = useState({
    projectName: "",
    clientId: "",
    categoryId: "",
    projectManagerId: "",
    location: "",
    siteAddress: "",
    startDate: "",
    plannedEndDate: "",
    contractValue: 0,
    status: "Inquiry",
    priority: "Medium",
    description: "",
  });
  const [cvDisplay, setCvDisplay] = useState("0");
  const [quickCreate, setQuickCreate] = useState<{ type: "client" | "category" } | null>(null);
  const [newClientForm, setNewClientForm] = useState({ companyName: "", contactPerson: "", email: "", phone: "" });
  const [newCategoryForm, setNewCategoryForm] = useState({ name: "", code: "", description: "" });
  const [quickCreateError, setQuickCreateError] = useState("");

  const { data: clientData } = useQuery({
    queryKey: ["clients"],
    queryFn: () => getClients({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const clientOptions = useMemo(
    () => (clientData?.data?.data?.items ?? []).map((c) => ({ value: c.id, label: `${c.clientCode} — ${c.companyName}` })),
    [clientData],
  );

  const { options: pmOptions, isLoading: pmLoading } = useEmployeeLookup();

  const { data: catData, isLoading: catLoading } = useQuery({
    queryKey: ["project-categories"],
    queryFn: () => getProjectCategories({ page: 1, pageSize: 200 }),
    staleTime: 60000,
  });
  const categoryOptions = useMemo(
    () => (catData?.data?.data?.items ?? []).map((c) => ({ value: c.id, label: `${c.code} — ${c.name}` })),
    [catData],
  );

  const allowedStatuses = useMemo(
    () => VALID_STATUS_TRANSITIONS[project?.status ?? "Inquiry"] ?? ["Inquiry"],
    [project],
  );
  const isTerminal = project && (project.status === "Completed" || project.status === "Cancelled");

  const createClientMutation = useMutation({
    mutationFn: (data: { companyName: string; contactPerson?: string; email?: string; phone?: string }) =>
      createClient(data),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ["clients"] });
      const client = res.data.data;
      if (client) setForm((f) => ({ ...f, clientId: client.id }));
      setQuickCreate(null);
      setQuickCreateError("");
    },
    onError: (err: Error) => setQuickCreateError(extractApiError(err)),
  });

  const createCategoryMutation = useMutation({
    mutationFn: (data: { name: string; code: string; description?: string }) =>
      createProjectCategory(data),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ["project-categories"] });
      const cat = res.data.data;
      if (cat) setForm((f) => ({ ...f, categoryId: cat.id }));
      setQuickCreate(null);
      setQuickCreateError("");
    },
    onError: (err: Error) => setQuickCreateError(extractApiError(err)),
  });

  useEffect(() => {
    if (project) {
      setForm({
        projectName: project.projectName,
        clientId: project.clientId,
        categoryId: project.categoryId,
        projectManagerId: project.projectManagerId,
        location: project.location,
        siteAddress: project.siteAddress,
        startDate: project.startDate?.split("T")[0] || "",
        plannedEndDate: project.plannedEndDate?.split("T")[0] || "",
        contractValue: project.contractValue,
        status: project.status,
        priority: project.priority,
        description: project.description || "",
      });
      setCvDisplay(String(project.contractValue));
    }
  }, [project]);

  const saveMutation = useMutation({
    mutationFn: () =>
      project
        ? updateProject(project.id, form)
        : createProject(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["projects"] });
      qc.invalidateQueries({ queryKey: ["projectDashboard"] });
      onClose();
    },
    onError: (err: Error) => setError(extractApiError(err)),
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setForm((f) => ({ ...f, [name]: value }));
  };

  return (
    <div className="space-y-3">
      <div>
        <label className="text-xs text-gray-500 mb-1 block">
          Project Name *
        </label>
        <input
          name="projectName"
          value={form.projectName}
          onChange={handleChange}
          className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
        />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <SearchableDropdown label="Client *" options={clientOptions} value={form.clientId} onChange={(v) => setForm((f) => ({ ...f, clientId: v ?? "" }))} placeholder="Select Client" searchPlaceholder="Search clients..." required creatable onCreate={(label) => { setNewClientForm({ companyName: label, contactPerson: "", email: "", phone: "" }); setQuickCreate({ type: "client" }); setQuickCreateError(""); }} />
        </div>
        <div>
          <SearchableDropdown label="Category *" options={categoryOptions} value={form.categoryId} onChange={(v) => setForm((f) => ({ ...f, categoryId: v ?? "" }))} placeholder="Select Category" searchPlaceholder="Search categories..." loading={catLoading} required creatable onCreate={(label) => { const code = label.replace(/[^a-zA-Z0-9]/g, "").substring(0, 4).toUpperCase(); setNewCategoryForm({ name: label, code, description: "" }); setQuickCreate({ type: "category" }); setQuickCreateError(""); }} />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <SearchableDropdown label="Project Manager *" options={pmOptions} value={form.projectManagerId} onChange={(v) => setForm((f) => ({ ...f, projectManagerId: v ?? "" }))} placeholder="Select PM" searchPlaceholder="Search employees..." loading={pmLoading} required />
        </div>
        <div>
          <label className="text-xs text-gray-500 mb-1 block">
            Contract Value *
          </label>
          <input
            type="number"
            name="contractValue"
            value={cvDisplay}
            onFocus={() => { if (form.contractValue === 0) setCvDisplay(""); }}
            onBlur={() => { if (cvDisplay === "") { setCvDisplay("0"); setForm((f) => ({ ...f, contractValue: 0 })); } }}
            onChange={(e) => {
              const raw = e.target.value;
              setCvDisplay(raw);
              const parsed = parseFloat(raw);
              if (!isNaN(parsed)) setForm((f) => ({ ...f, contractValue: parsed }));
            }}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          {form.contractValue > 0 && (
            <p className="text-xs text-gray-500 mt-1">
              {amountInWords(form.contractValue)}
            </p>
          )}
        </div>
      </div>

      <div>
        <label className="text-xs text-gray-500 mb-1 block">Location</label>
        <input
          name="location"
          value={form.location}
          onChange={handleChange}
          className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
        />
      </div>

      <div>
        <label className="text-xs text-gray-500 mb-1 block">Site Address</label>
        <input
          name="siteAddress"
          value={form.siteAddress}
          onChange={handleChange}
          className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
        />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="text-xs text-gray-500 mb-1 block">Start Date *</label>
          <DateInput
            name="startDate"
            value={form.startDate}
            onChange={(e) => setForm((f) => ({ ...f, startDate: e.target.value }))}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
        <div>
          <label className="text-xs text-gray-500 mb-1 block">
            Planned End Date *
          </label>
          <DateInput
            name="plannedEndDate"
            value={form.plannedEndDate}
            onChange={(e) => setForm((f) => ({ ...f, plannedEndDate: e.target.value }))}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="text-xs text-gray-500 mb-1 block">Status</label>
          <select
            name="status"
            value={form.status}
            onChange={handleChange}
            disabled={!!isTerminal}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 disabled:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {allowedStatuses.map((s) => (
              <option key={s} value={s}>{s === "OnHold" ? "On Hold" : s}</option>
            ))}
          </select>
          {project && !isTerminal && (
            <p className="text-xs text-gray-400 mt-1">Current: {project.status}</p>
          )}
          {!!isTerminal && (
            <p className="text-xs text-amber-600 mt-1">Terminal state — status cannot be changed.</p>
          )}
        </div>
        <div>
          <label className="text-xs text-gray-500 mb-1 block">Priority</label>
          <select
            name="priority"
            value={form.priority}
            onChange={handleChange}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Critical">Critical</option>
          </select>
        </div>
      </div>

      <div>
        <label className="text-xs text-gray-500 mb-1 block">Description</label>
        <textarea
          name="description"
          value={form.description}
          onChange={handleChange}
          rows={3}
          className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
        />
      </div>

      <Modal title="New Client" open={quickCreate?.type === "client"} onClose={() => setQuickCreate(null)} size="sm">
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Company Name *</label>
            <input value={newClientForm.companyName} onChange={(e) => setNewClientForm((f) => ({ ...f, companyName: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Contact Person</label>
            <input value={newClientForm.contactPerson} onChange={(e) => setNewClientForm((f) => ({ ...f, contactPerson: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="grid grid-cols-2 gap-2">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Email</label>
              <input value={newClientForm.email} onChange={(e) => setNewClientForm((f) => ({ ...f, email: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Phone</label>
              <input value={newClientForm.phone} onChange={(e) => setNewClientForm((f) => ({ ...f, phone: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>
          {quickCreateError && <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{quickCreateError}</div>}
          <div className="flex justify-end gap-2 pt-1">
            <button onClick={() => setQuickCreate(null)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => createClientMutation.mutate(newClientForm)} disabled={createClientMutation.isPending || !newClientForm.companyName.trim()} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{createClientMutation.isPending ? "Creating..." : "Create"}</button>
          </div>
        </div>
      </Modal>

      <Modal title="New Category" open={quickCreate?.type === "category"} onClose={() => setQuickCreate(null)} size="sm">
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Name *</label>
            <input value={newCategoryForm.name} onChange={(e) => setNewCategoryForm((f) => ({ ...f, name: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Code *</label>
            <input value={newCategoryForm.code} onChange={(e) => setNewCategoryForm((f) => ({ ...f, code: e.target.value }))} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea value={newCategoryForm.description} onChange={(e) => setNewCategoryForm((f) => ({ ...f, description: e.target.value }))} rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          {quickCreateError && <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{quickCreateError}</div>}
          <div className="flex justify-end gap-2 pt-1">
            <button onClick={() => setQuickCreate(null)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => createCategoryMutation.mutate(newCategoryForm)} disabled={createCategoryMutation.isPending || !newCategoryForm.name.trim() || !newCategoryForm.code.trim()} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{createCategoryMutation.isPending ? "Creating..." : "Create"}</button>
          </div>
        </div>
      </Modal>

      {error && (
        <div className="text-xs text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
          {error}
        </div>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <button
          onClick={onClose}
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
  );
}