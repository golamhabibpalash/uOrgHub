import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import {
  HardHat,
  Users,
  Tag,
  Plus,
} from "lucide-react";
import StatCard from "../../components/shared/StatCard";
import Modal from "../../components/shared/Modal";
import ProjectForm from "./ProjectForm";
import { getProjects } from "../../api/projects";

export default function ProjectsDashboard() {
  const navigate = useNavigate();
  const [showForm, setShowForm] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["projects", 1, "", "projectName", false, ""],
    queryFn: () => getProjects({ page: 1, pageSize: 100 }),
  });

  const projects = data?.data?.data?.items ?? [];

  const totalProjects = projects.length;
  const activeProjects = projects.filter((p) => p.status === "Active" || p.status === "InProgress").length;
  const completedProjects = projects.filter((p) => p.status === "Completed").length;
  const onHoldProjects = projects.filter((p) => p.status === "OnHold").length;

  const recentProjects = [...projects].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  ).slice(0, 5);

  const modules = [
    { name: "All Projects", path: "/projects", icon: HardHat, color: "bg-blue-500" },
    { name: "Categories", path: "/projects/categories", icon: Tag, color: "bg-purple-500" },
    { name: "Clients", path: "/projects/clients", icon: Users, color: "bg-green-500" },
  ];

  const stats = [
    { label: "Total Projects", value: totalProjects, sub: `${activeProjects} active` },
    { label: "Active / In Progress", value: activeProjects, sub: `${((activeProjects / (totalProjects || 1)) * 100).toFixed(0)}% of total` },
    { label: "Completed", value: completedProjects, sub: "" },
    { label: "On Hold", value: onHoldProjects, sub: "" },
  ];

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

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Project Management</h1>
          <p className="text-sm text-gray-400">Construction project management module</p>
        </div>
        <button onClick={() => setShowForm(true)} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Project
        </button>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="bg-white border border-gray-200 rounded-xl p-5 animate-pulse">
              <div className="h-3 bg-gray-200 rounded w-24 mb-3" />
              <div className="h-6 bg-gray-200 rounded w-16 mb-2" />
              <div className="h-3 bg-gray-200 rounded w-20" />
            </div>
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {stats.map((stat) => (
            <StatCard key={stat.label} label={stat.label} value={stat.value} sub={stat.sub} />
          ))}
        </div>
      )}

      <div className="mb-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Project Modules</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {modules.map((mod) => (
            <button
              key={mod.path}
              onClick={() => navigate(mod.path)}
              className="bg-white border border-gray-200 rounded-xl p-4 text-left hover:border-primary-500 hover:shadow-md transition-all group"
            >
              <div className={`w-10 h-10 ${mod.color} rounded-lg flex items-center justify-center mb-3`}>
                <mod.icon size={20} className="text-white" />
              </div>
              <h3 className="text-sm font-medium text-gray-900 group-hover:text-primary-600">
                {mod.name}
              </h3>
              <p className="text-xs text-gray-400 mt-1">
                Manage {mod.name.toLowerCase()}
              </p>
            </button>
          ))}
        </div>
      </div>

      {recentProjects.length > 0 && (
        <div className="bg-white border border-gray-200 rounded-xl">
          <div className="px-5 py-4 border-b border-gray-100">
            <h3 className="text-sm font-medium text-gray-900">Recent Projects</h3>
          </div>
          <div className="divide-y divide-gray-100">
            {recentProjects.map((project) => (
              <button
                key={project.id}
                onClick={() => navigate(`/projects/${project.id}`)}
                className="w-full flex items-center justify-between px-5 py-3 hover:bg-gray-50 transition-colors text-left"
              >
                <div className="flex items-center gap-3 min-w-0">
                  <HardHat size={16} className="text-gray-400 shrink-0" />
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {project.projectName}
                    </p>
                    <p className="text-xs text-gray-400 truncate">
                      {project.projectCode} — {project.clientName}
                    </p>
                  </div>
                </div>
                <span className={`text-xs px-2 py-0.5 rounded-full shrink-0 ${getStatusBadge(project.status)}`}>
                  {project.status}
                </span>
              </button>
            ))}
          </div>
        </div>
      )}

      <Modal title="New Project" open={showForm} onClose={() => setShowForm(false)} size="2xl">
        <ProjectForm project={null} onClose={() => setShowForm(false)} />
      </Modal>
    </div>
  );
}
