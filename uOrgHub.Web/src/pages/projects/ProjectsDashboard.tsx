import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import {
  getProjects,
  getProjectDashboard,
  type Project,
} from "../../api/projects";
import ProjectForm from "./ProjectForm";

export default function ProjectsDashboard() {
  const navigate = useNavigate();
  const dg = useDataGrid({ defaultSortBy: "projectName" });
  const [status, setStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<Project | null>(null);

  const { data: dashboardData } = useQuery({
    queryKey: ["projectDashboard"],
    queryFn: getProjectDashboard,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["projects", dg.page, dg.search, dg.sortBy, dg.sortDescending, status],
    queryFn: () => getProjects(dg.queryParams, status),
  });

  const projects = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

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
      sortable: false,
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
      sortable: false,
      render: (row: Project) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(row.status)}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: "priority",
      label: "Priority",
      sortable: false,
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
      sortable: false,
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

      <DataGrid
        columns={columns}
        data={projects}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search projects..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No projects found"
        toolbarPrefix={
          <select
            value={status}
            onChange={(e) => { setStatus(e.target.value); dg.setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All Status</option>
            <option value="Active">Active</option>
            <option value="InProgress">In Progress</option>
            <option value="OnHold">On Hold</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        }
        actions={
          <div className="flex items-center gap-2">
            <ExportMenu baseUrl="/projects" filters={{ search: dg.search || undefined, status: status || undefined }} />
            <button
              onClick={openAdd}
              className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
            >
              <Plus size={15} /> Add Project
            </button>
          </div>
        }
      />

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