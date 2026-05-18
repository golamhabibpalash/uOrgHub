import { useParams, Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import {
  ArrowLeft,
  MapPin,
  Calendar,
  FileText,
  Package,
  Receipt,
  TrendingUp,
} from "lucide-react";
import {
  getProjectById,
  getProjectBudgetSummary,
  getProjectTeam,
  getMilestones,
  getProjectProgress,
} from "../../api/projects";

export default function ProjectDetail() {
  const { id } = useParams<{ id: string }>();

  const { data: projectData, isLoading: projectLoading } = useQuery({
    queryKey: ["project", id],
    queryFn: () => getProjectById(id!),
    enabled: !!id,
  });

  const { data: budgetData } = useQuery({
    queryKey: ["projectBudget", id],
    queryFn: () => getProjectBudgetSummary(id!),
    enabled: !!id,
  });

  const { data: teamData } = useQuery({
    queryKey: ["projectTeam", id],
    queryFn: () => getProjectTeam(id!),
    enabled: !!id,
  });

  const { data: milestonesData } = useQuery({
    queryKey: ["milestones", id],
    queryFn: () => getMilestones(id!),
    enabled: !!id,
  });

  const { data: progressData } = useQuery({
    queryKey: ["projectProgress", id],
    queryFn: () => getProjectProgress(id!),
    enabled: !!id,
  });

  const project = projectData?.data?.data;
  const budget = budgetData?.data?.data;
  const team = teamData?.data?.data ?? [];
  const milestones = milestonesData?.data?.data ?? [];
  const progress = progressData?.data?.data;

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Active: "bg-green-50 text-green-700",
      InProgress: "bg-blue-50 text-blue-700",
      OnHold: "bg-amber-50 text-amber-700",
      Cancelled: "bg-red-50 text-red-700",
      Completed: "bg-green-50 text-green-700",
      Pending: "bg-amber-50 text-amber-700",
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

  if (projectLoading) {
    return <div className="text-center py-10 text-gray-500">Loading...</div>;
  }

  if (!project) {
    return <div className="text-center py-10 text-gray-500">Project not found</div>;
  }

  const quickLinks = [
    { label: "WBS", path: `/projects/${id}/wbs`, icon: FileText },
    { label: "BOQ", path: `/projects/${id}/boq`, icon: FileText },
    { label: "DPR", path: `/projects/${id}/dpr`, icon: Receipt },
    { label: "Materials", path: `/projects/${id}/materials`, icon: Package },
    { label: "Expenses", path: `/projects/${id}/expenses`, icon: Receipt },
    { label: "Milestones", path: `/projects/${id}/milestones`, icon: TrendingUp },
    { label: "Drawings", path: `/projects/${id}/drawings`, icon: FileText },
    { label: "RFIs", path: `/projects/${id}/rfis`, icon: FileText },
    { label: "Submittals", path: `/projects/${id}/submittals`, icon: Package },
    { label: "Resources", path: `/projects/${id}/resource-allocations`, icon: Package },
    { label: "QA Checklists", path: `/projects/${id}/qa-checklists`, icon: Receipt },
    { label: "NCRs", path: `/projects/${id}/ncrs`, icon: FileText },
    { label: "Safety", path: `/projects/${id}/safety-incidents`, icon: Receipt },
    { label: "RA Bills", path: `/projects/${id}/ra-bills`, icon: TrendingUp },
  ];

  return (
    <div>
      <div className="mb-4">
        <Link
          to="/projects"
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft size={16} /> Back to Projects
        </Link>
      </div>

      <div className="mb-6">
        <h2 className="text-lg font-medium text-gray-900">{project.projectName}</h2>
        <p className="text-sm text-gray-500">
          {project.projectCode} • {project.categoryName}
        </p>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="col-span-2 bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">
            Project Information
          </h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-xs text-gray-500">Client</p>
              <p className="text-sm font-medium text-gray-900">
                {project.clientName}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Project Manager</p>
              <p className="text-sm font-medium text-gray-900">
                {project.projectManagerName}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Location</p>
              <p className="text-sm text-gray-700 flex items-center gap-1">
                <MapPin size={12} /> {project.location || "N/A"}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Site Address</p>
              <p className="text-sm text-gray-700">{project.siteAddress || "N/A"}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Start Date</p>
              <p className="text-sm text-gray-700 flex items-center gap-1">
                <Calendar size={12} />{" "}
                {project.startDate
                  ? new Date(project.startDate).toLocaleDateString()
                  : "N/A"}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Planned End Date</p>
              <p className="text-sm text-gray-700 flex items-center gap-1">
                <Calendar size={12} />{" "}
                {project.plannedEndDate
                  ? new Date(project.plannedEndDate).toLocaleDateString()
                  : "N/A"}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Contract Value</p>
              <p className="text-sm font-medium text-gray-900">
                BDT {project.contractValue.toLocaleString()}
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Status</p>
              <span
                className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(
                  project.status
                )}`}
              >
                {project.status}
              </span>
            </div>
            <div>
              <p className="text-xs text-gray-500">Priority</p>
              <span
                className={`text-xs px-2 py-0.5 rounded-full ${getPriorityBadge(
                  project.priority
                )}`}
              >
                {project.priority}
              </span>
            </div>
          </div>

          {project.description && (
            <div className="mt-4">
              <p className="text-xs text-gray-500">Description</p>
              <p className="text-sm text-gray-700">{project.description}</p>
            </div>
          )}
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">
            Budget Summary
          </h3>
          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-xs text-gray-500">Contract Value</span>
              <span className="text-sm font-medium text-gray-900">
                BDT {(budget?.contractValue ?? 0).toLocaleString()}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-xs text-gray-500">BOQ Estimated</span>
              <span className="text-sm font-medium text-gray-900">
                BDT {(budget?.boqEstimated ?? 0).toLocaleString()}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-xs text-gray-500">BOQ Approved</span>
              <span className="text-sm font-medium text-gray-900">
                BDT {(budget?.boqApproved ?? 0).toLocaleString()}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-xs text-gray-500">Total Expenses</span>
              <span className="text-sm font-medium text-gray-900">
                BDT {(budget?.totalExpenses ?? 0).toLocaleString()}
              </span>
            </div>
            <div className="border-t border-gray-100 pt-3">
              <div className="flex justify-between">
                <span className="text-xs text-gray-500">Remaining Budget</span>
                <span className="text-sm font-medium text-green-600">
                  BDT {(budget?.remainingBudget ?? 0).toLocaleString()}
                </span>
              </div>
            </div>
            <div className="mt-2">
              <div className="flex justify-between text-xs mb-1">
                <span className="text-gray-500">Budget Used</span>
                <span className="text-gray-700">
                  {budget?.percentUsed ?? 0}%
                </span>
              </div>
              <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary-500 rounded-full"
                  style={{ width: `${budget?.percentUsed ?? 0}%` }}
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">
            Overall Progress
          </h3>
          <div className="text-center">
            <div className="relative inline-block">
              <div className="w-24 h-24 rounded-full border-4 border-primary-500 flex items-center justify-center">
                <span className="text-2xl font-medium text-gray-900">
                  {progress?.overallProgress ?? project.progress ?? 0}%
                </span>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-3 gap-2 text-xs">
              <div>
                <p className="text-gray-500">WBS</p>
                <p className="font-medium">{progress?.wbsProgress ?? 0}%</p>
              </div>
              <div>
                <p className="text-gray-500">Milestone</p>
                <p className="font-medium">{progress?.milestoneProgress ?? 0}%</p>
              </div>
              <div>
                <p className="text-gray-500">Budget</p>
                <p className="font-medium">{progress?.budgetProgress ?? 0}%</p>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">
            Team Members
          </h3>
          {team.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-4">
              No team members
            </p>
          ) : (
            <div className="flex flex-wrap gap-2">
              {team.slice(0, 6).map((member) => (
                <div
                  key={member.id}
                  className="flex items-center gap-2 bg-gray-50 rounded-lg px-2 py-1"
                >
                  <div className="w-8 h-8 rounded-full bg-primary-500 text-white flex items-center justify-center text-xs">
                    {member.employeeName.charAt(0)}
                  </div>
                  <div>
                    <p className="text-xs font-medium text-gray-900">
                      {member.employeeName}
                    </p>
                    <p className="text-xs text-gray-500">{member.role}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">
            Quick Links
          </h3>
          <div className="space-y-2">
            {quickLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className="flex items-center gap-2 text-sm text-gray-600 hover:text-primary-600"
              >
                <link.icon size={14} /> {link.label}
              </Link>
            ))}
          </div>
        </div>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5">
        <h3 className="text-sm font-medium text-gray-900 mb-4">
          Milestones Timeline
        </h3>
        {milestones.length === 0 ? (
          <p className="text-sm text-gray-400 text-center py-4">
            No milestones defined
          </p>
        ) : (
          <div className="space-y-4">
            {milestones.slice(0, 5).map((milestone) => (
              <div key={milestone.id} className="flex items-start gap-3">
                <div
                  className={`w-3 h-3 rounded-full mt-1.5 ${
                    milestone.status === "Completed"
                      ? "bg-green-500"
                      : milestone.status === "InProgress"
                      ? "bg-blue-500"
                      : "bg-gray-300"
                  }`}
                />
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-gray-900">
                      {milestone.title}
                    </p>
                    {milestone.isCritical && (
                      <span className="text-xs bg-red-50 text-red-700 px-1.5 py-0.5 rounded">
                        Critical
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-gray-500">
                    Planned:{" "}
                    {milestone.plannedDate
                      ? new Date(milestone.plannedDate).toLocaleDateString()
                      : "N/A"}
                    {milestone.actualDate && (
                      <> • Actual: {new Date(milestone.actualDate).toLocaleDateString()}</>
                    )}
                  </p>
                </div>
                <span
                  className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(
                    milestone.status
                  )}`}
                >
                  {milestone.status}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}