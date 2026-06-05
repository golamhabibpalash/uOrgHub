import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Plus, Search } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import ExportMenu from "../../components/shared/ExportMenu";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getProjects,
  getProjectDashboard,
  type Project,
} from "../../api/projects";
import ProjectForm from "./ProjectForm";

export default function ProjectsDashboard() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Project | null>(null);

  const { data: dashboardData } = useQuery({
    queryKey: ["projectDashboard"],
    queryFn: getProjectDashboard,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["projects", page, search, status],
    queryFn: () => getProjects({ page, pageSize: 10, search }, status),
  });

  const projects = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;

  function openAdd() {
    setEditing(null);
    setModal(true);
  }

  function openEdit(project: Project) {
    setEditing(project);
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Active: "bg-green-50 text-green-700",
      InProgress: "bg-blue-50 text-blue-700",
      OnHold: "bg-amber-50 text-amber-700",
      Cancelled: "bg-red-50 text-red-700",
      Completed: "bg-green-50 text-green-700",
      Draft: "bg-gray-100 text-gray-600",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

  const getPriorityBadge = (priority: string) => {
    const styles: Record<string, string> = {
      High: "bg-red-50 text-red-700",
      Medium: "bg-amber-50 text-amber-700",
      Low: "bg-blue-50 text-blue-700",
      Critical: "bg-red-50 text-red-700",
    };
    return styles[priority] || "bg-gray-100 text-gray-600";
  };

  const columns = [
    {
      key: "projectCode",
      label: "Code",
      render: (row: Project) => (
        <span className="font-medium text-gray-900">{row.projectCode}</span>
      ),
    },
    {
      key: "projectName",
      label: "Project Name",
      render: (row: Project) => (
        <span className="font-medium text-gray-900">{row.projectName}</span>
      ),
    },
    { key: "clientName", label: "Client" },
    { key: "categoryName", label: "Category" },
    {
      key: "progress",
      label: "Progress",
      render: (row: Project) => (
        <div className="flex items-center gap-2 w-24">
          <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
            <div
              className="h-full bg-primary-500 rounded-full"
              style={{ width: `${row.progress}%` }}
            />
          </div>
          <span className="text-xs text-gray-500">{row.progress}%</span>
        </div>
      ),
    },
    {
      key: "status",
      label: "Status",
      render: (row: Project) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(row.status)}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: "priority",
      label: "Priority",
      render: (row: Project) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${getPriorityBadge(row.priority)}`}>
          {row.priority}
        </span>
      ),
    },
    { key: "projectManagerName", label: "PM" },
    {
      key: "actions",
      label: "Actions",
      render: (row: Project) => (
        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate(`/projects/${row.id}`)}
            className="text-xs text-primary-600 hover:underline"
          >
            View
          </button>
          <button
            onClick={() => openEdit(row)}
            className="text-xs text-gray-500 hover:text-primary-600"
          >
            Edit
          </button>
        </div>
      ),
    },
  ];

  const stats = [
    {
      label: "Active Projects",
      value: dashboardData?.data?.data?.activeProjects ?? 0,
    },
    {
      label: "Total BOQ Value",
      value: `BDT ${(dashboardData?.data?.data?.totalBOQValue ?? 0).toLocaleString()}`,
    },
    {
      label: "Pending DPRs",
      value: dashboardData?.data?.data?.pendingDPRs ?? 0,
    },
    {
      label: "Material Requests",
      value: dashboardData?.data?.data?.materialRequests ?? 0,
    },
  ];

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-lg font-medium text-gray-900">Projects</h2>
        <p className="text-sm text-gray-500">Manage your construction projects</p>
      </div>

      <div className="grid grid-cols-4 gap-4 mb-6">
        {stats.map((stat) => (
          <div
            key={stat.label}
            className="bg-gray-50 border border-gray-200 rounded-xl p-4"
          >
            <p className="text-xs text-gray-500 mb-1">{stat.label}</p>
            <p className="text-2xl font-medium text-gray-900">{stat.value}</p>
          </div>
        ))}
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="relative">
              <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search projects..."
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value);
                  setPage(1);
                }}
                className="text-sm border border-gray-200 rounded-lg pl-9 pr-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value);
                setPage(1);
              }}
              className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              <option value="">All Status</option>
              <option value="Active">Active</option>
              <option value="InProgress">In Progress</option>
              <option value="OnHold">On Hold</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
          <div className="ml-auto flex items-center gap-2">
            <ExportMenu baseUrl="/projects" filters={{ search: search || undefined, status: status || undefined }} />
            <button
              onClick={openAdd}
              className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
            >
              <Plus size={15} /> Add Project
            </button>
          </div>
        </div>

        <div
          onClick={(e) => {
            const row = (e.target as HTMLElement).closest("tr");
            if (row && !((e.target as HTMLElement).closest("button"))) {
              const project = projects.find((p) => p.id === row.getAttribute("data-id"));
              if (project) navigate(`/projects/${project.id}`);
            }
          }}
        >
          <DataTable
            columns={columns}
            data={projects}
            loading={isLoading}
          />
        </div>

        <Pagination
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      </div>

      <Modal
        title={editing ? "Edit Project" : "Add Project"}
        open={modal}
        onClose={closeModal}
      >
        <ProjectForm
          project={editing}
          onClose={closeModal}
        />
      </Modal>
    </div>
  );
}