import { useState, useEffect, useMemo } from "react";
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { createProject, updateProject, getProjectCategories, getClients, Project } from "../../api/projects";
import { useEmployeeLookup } from "../../hooks/useEntityLookup";
import SearchableDropdown from "../../components/shared/SearchableDropdown";

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
    status: "Draft",
    priority: "Medium",
    description: "",
  });

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
    onError: (err: Error) => {
      const axiosErr = err as AxiosError<{ message?: string; errors?: string[] }>;
      const msg = axiosErr.response?.data?.message
        || axiosErr.response?.data?.errors?.[0]
        || err.message
        || "An error occurred while saving the project.";
      setError(msg);
    },
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setForm((f) => ({
      ...f,
      [name]: name === "contractValue" ? parseFloat(value) || 0 : value,
    }));
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
          <SearchableDropdown label="Client *" options={clientOptions} value={form.clientId} onChange={(v) => setForm((f) => ({ ...f, clientId: v ?? "" }))} placeholder="Select Client" searchPlaceholder="Search clients..." required />
        </div>
        <div>
          <SearchableDropdown label="Category *" options={categoryOptions} value={form.categoryId} onChange={(v) => setForm((f) => ({ ...f, categoryId: v ?? "" }))} placeholder="Select Category" searchPlaceholder="Search categories..." loading={catLoading} required />
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
            value={form.contractValue}
            onChange={handleChange}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
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
          <input
            type="date"
            name="startDate"
            value={form.startDate}
            onChange={handleChange}
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>
        <div>
          <label className="text-xs text-gray-500 mb-1 block">
            Planned End Date *
          </label>
          <input
            type="date"
            name="plannedEndDate"
            value={form.plannedEndDate}
            onChange={handleChange}
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
            className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="Draft">Draft</option>
            <option value="Active">Active</option>
            <option value="InProgress">In Progress</option>
            <option value="OnHold">On Hold</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
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