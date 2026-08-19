import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ProjectForm from "./ProjectForm";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getProjects, VALID_STATUS_TRANSITIONS, type Project } from "../../api/projects";
import { formatBDT, formatDate } from "../../utils/format";

const STATUS_STYLES: Record<string, string> = {
  Inquiry: "bg-gray-100 text-gray-600",
  Planning: "bg-purple-50 text-purple-700",
  Tender: "bg-cyan-50 text-cyan-700",
  Active: "bg-green-50 text-green-700",
  InProgress: "bg-blue-50 text-blue-700",
  OnHold: "bg-amber-50 text-amber-700",
  Handover: "bg-indigo-50 text-indigo-700",
  Completed: "bg-green-50 text-green-700",
  Cancelled: "bg-red-50 text-red-700",
};

const PRIORITY_STYLES: Record<string, string> = {
  Low: "text-gray-500",
  Medium: "text-blue-600",
  High: "text-amber-600",
  Critical: "text-red-600",
};

export default function AllProjectsPage() {
  const navigate = useNavigate();
  const dg = useDataGrid({ defaultSortBy: "ProjectName" });
  const [showForm, setShowForm] = useState(false);

  const status = dg.filters.status;

  const { data, isLoading } = useQuery({
    queryKey: ["projects", "all", ...dg.queryKey],
    queryFn: () => getProjects(dg.queryParams, status),
  });

  const projects = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const columns = [
    { key: "projectCode", label: "Code" },
    { key: "projectName", label: "Project Name" },
    { key: "clientName", label: "Client", sortable: false },
    { key: "categoryName", label: "Category", sortable: false },
    {
      key: "contractValue",
      label: "Contract Value",
      render: (row: Project) => (
        <span className="font-medium text-gray-900">{formatBDT(Number(row.contractValue))}</span>
      ),
    },
    {
      key: "status",
      label: "Status",
      render: (row: Project) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${STATUS_STYLES[row.status] ?? "bg-gray-100 text-gray-600"}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: "priority",
      label: "Priority",
      render: (row: Project) => (
        <span className={`text-xs font-medium ${PRIORITY_STYLES[row.priority] ?? "text-gray-500"}`}>
          {row.priority}
        </span>
      ),
    },
    {
      key: "startDate",
      label: "Start Date",
      render: (row: Project) => <span>{formatDate(row.startDate)}</span>,
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-medium text-gray-900">All Projects</h2>
          <p className="text-sm text-gray-500">
            {totalCount} project{totalCount !== 1 ? "s" : ""} in total
          </p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> New Project
        </button>
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
        searchPlaceholder="Search by name or code..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onView={(row) => navigate(`/projects/${row.id}`)}
        emptyMessage="No projects found"
        filterBar={
          <select
            value={status ?? ""}
            onChange={(e) => dg.setFilter("status", e.target.value)}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All statuses</option>
            {Object.keys(VALID_STATUS_TRANSITIONS).map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        }
      />

      <Modal title="New Project" open={showForm} onClose={() => setShowForm(false)} size="2xl">
        <ProjectForm project={null} onClose={() => setShowForm(false)} />
      </Modal>
    </div>
  );
}