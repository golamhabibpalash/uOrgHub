import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, UserMinus } from "lucide-react";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import ProjectNav from "../../components/projects/ProjectNav";
import { useEmployeeLookup } from "../../hooks/useEntityLookup";
import {
  getProjectTeam,
  addTeamMember,
  removeTeamMember,
  ProjectTeamMember,
} from "../../api/projects";

const TEAM_ROLES = ["ProjectManager", "SiteEngineer", "Supervisor", "Foreman", "SafetyOfficer", "QualityInspector", "Other"];

const roleOptions = TEAM_ROLES.map((r) => ({ value: r, label: r }));

const roleLabel: Record<string, string> = {
  ProjectManager: "Project Manager",
  SiteEngineer: "Site Engineer",
  Supervisor: "Supervisor",
  Foreman: "Foreman",
  SafetyOfficer: "Safety Officer",
  QualityInspector: "Quality Inspector",
  Other: "Other",
};

export default function ProjectTeamPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<ProjectTeamMember | null>(null);
  const [form, setForm] = useState({
    employeeId: "",
    role: "SiteEngineer",
    joinedDate: new Date().toISOString().split("T")[0],
    notes: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["project-team", projectId],
    queryFn: () => getProjectTeam(projectId!),
    enabled: !!projectId,
  });

  const team = data?.data?.data ?? [];

  const { options: employeeOptions, isLoading: empLoading } = useEmployeeLookup();

  const addMutation = useMutation({
    mutationFn: () =>
      addTeamMember(projectId!, {
        employeeId: form.employeeId,
        role: form.role,
        joinedDate: form.joinedDate,
        notes: form.notes || undefined,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["project-team", projectId] });
      closeModal();
    },
  });

  const removeMutation = useMutation({
    mutationFn: (employeeId: string) => removeTeamMember(projectId!, employeeId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["project-team", projectId] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      employeeId: "",
      role: "SiteEngineer",
      joinedDate: new Date().toISOString().split("T")[0],
      notes: "",
    });
    setModal(true);
  }

  function openEdit(member: ProjectTeamMember) {
    setEditing(member);
    setForm({
      employeeId: member.employeeId,
      role: member.role,
      joinedDate: member.joinedDate.split("T")[0] || "",
      notes: member.notes || "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setEditing(null);
  }

  const getRoleBadge = (role: string) => {
    const styles: Record<string, string> = {
      ProjectManager: "bg-indigo-50 text-indigo-700",
      SiteEngineer: "bg-blue-50 text-blue-700",
      Supervisor: "bg-amber-50 text-amber-700",
      Foreman: "bg-green-50 text-green-700",
      SafetyOfficer: "bg-red-50 text-red-700",
      QualityInspector: "bg-purple-50 text-purple-700",
      Other: "bg-gray-50 text-gray-600",
    };
    return styles[role] || "bg-gray-100 text-gray-600";
  };

  const activeMembers = team.filter((m) => m.isActive);
  const pastMembers = team.filter((m) => !m.isActive);

  return (
    <div>
      <div className="mb-4">
        <Link
          to={`/projects/${projectId}`}
          className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft size={16} /> Back to Project
        </Link>
      </div>

      <ProjectNav />

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Project Team</h2>
          <p className="text-xs text-gray-400">Manage project team members</p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Add Member
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : team.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No team members found</div>
        ) : (
          <>
            {activeMembers.length > 0 && (
              <table className="w-full text-sm border-collapse">
                <thead>
                  <tr className="bg-gray-50">
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Employee</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Role</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Joined Date</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Notes</th>
                    <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {activeMembers.map((m) => (
                    <tr key={m.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <div className="w-7 h-7 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-xs font-medium">
                            {m.employeeName.charAt(0)}
                          </div>
                          <span className="font-medium text-gray-900">{m.employeeName}</span>
                        </div>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${getRoleBadge(m.role)}`}>
                          {roleLabel[m.role] || m.role}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-gray-700">
                        {m.joinedDate ? new Date(m.joinedDate).toLocaleDateString() : "-"}
                      </td>
                      <td className="px-4 py-2.5 text-gray-500 text-xs max-w-[200px] truncate">
                        {m.notes || "-"}
                      </td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => openEdit(m)}
                            className="text-xs text-gray-500 hover:text-primary-600"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => {
                              if (confirm(`Remove ${m.employeeName} from the team?`)) {
                                removeMutation.mutate(m.employeeId);
                              }
                            }}
                            className="flex items-center gap-1 text-xs text-red-500 hover:text-red-700"
                          >
                            <UserMinus size={12} /> Remove
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {pastMembers.length > 0 && (
              <div className="border-t border-gray-200">
                <div className="px-4 py-2 bg-gray-50 text-xs font-medium text-gray-500">
                  Past Members ({pastMembers.length})
                </div>
                <table className="w-full text-sm border-collapse">
                  <tbody>
                    {pastMembers.map((m) => (
                      <tr key={m.id} className="border-t border-gray-100 hover:bg-gray-50 opacity-60">
                        <td className="px-4 py-2.5">
                          <span className="text-gray-900">{m.employeeName}</span>
                        </td>
                        <td className="px-4 py-2.5">
                          <span className={`text-xs px-2 py-0.5 rounded-full ${getRoleBadge(m.role)}`}>
                            {roleLabel[m.role] || m.role}
                          </span>
                        </td>
                        <td className="px-4 py-2.5 text-gray-500 text-xs">
                          {m.leftDate ? `Left ${new Date(m.leftDate).toLocaleDateString()}` : "Inactive"}
                        </td>
                        <td className="px-4 py-2.5" />
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </>
        )}
      </div>

      <Modal
        title={editing ? "Edit Team Member" : "Add Team Member"}
        open={modal}
        onClose={closeModal}
      >
        <div className="space-y-3">
          <div>
            <SearchableDropdown
              label="Employee *"
              options={employeeOptions}
              value={form.employeeId}
              onChange={(v) => setForm((f) => ({ ...f, employeeId: v ?? "" }))}
              placeholder="Select employee"
              searchPlaceholder="Search employees..."
              loading={empLoading}
              disabled={!!editing}
            />
          </div>
          <div>
            <SearchableDropdown
              label="Role *"
              options={roleOptions}
              value={form.role}
              onChange={(v) => setForm((f) => ({ ...f, role: v ?? "SiteEngineer" }))}
              placeholder="Select role"
              searchPlaceholder="Search roles..."
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Joined Date *</label>
            <input
              type="date"
              value={form.joinedDate}
              onChange={(e) => setForm((f) => ({ ...f, joinedDate: e.target.value }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea
              value={form.notes}
              onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => addMutation.mutate()}
              disabled={addMutation.isPending || !form.employeeId}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {addMutation.isPending ? "Adding..." : "Add Member"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
