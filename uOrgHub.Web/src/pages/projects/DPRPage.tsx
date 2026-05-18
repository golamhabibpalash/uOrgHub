import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Cloud, Sun, CloudRain, CloudSnow } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getDPRs,
  createDPR,
  submitDPR,
  approveDPR,
  DPR,
} from "../../api/projects";

const weatherIcons: Record<string, React.ReactNode> = {
  Sunny: <Sun size={16} className="text-amber-500" />,
  Cloudy: <Cloud size={16} className="text-gray-400" />,
  Rainy: <CloudRain size={16} className="text-blue-500" />,
  Snowy: <CloudSnow size={16} className="text-blue-200" />,
};

export default function DPRPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [modal, setModal] = useState(false);
  const [viewModal, setViewModal] = useState(false);
  const [selectedDPR, setSelectedDPR] = useState<DPR | null>(null);
  const [form, setForm] = useState({
    reportDate: "",
    weatherCondition: "Sunny",
    workDone: "",
    issues: "",
    nextDayPlan: "",
    manpowerCount: 0,
    equipmentUsed: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["dprs", projectId, page],
    queryFn: () => getDPRs(projectId!, { page, pageSize: 10 }),
    enabled: !!projectId,
  });

  const dprs = data?.data?.data?.items ?? [];
  const createMutation = useMutation({
    mutationFn: () => createDPR(projectId!, form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["dprs", projectId] });
      qc.invalidateQueries({ queryKey: ["projectDashboard"] });
      closeModal();
    },
  });

  const submitMutation = useMutation({
    mutationFn: (dprId: string) => submitDPR(projectId!, dprId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["dprs", projectId] }),
  });

  const approveMutation = useMutation({
    mutationFn: (dprId: string) => approveDPR(projectId!, dprId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["dprs", projectId] }),
  });

  function openModal() {
    setForm({
      reportDate: new Date().toISOString().split("T")[0],
      weatherCondition: "Sunny",
      workDone: "",
      issues: "",
      nextDayPlan: "",
      manpowerCount: 0,
      equipmentUsed: "",
    });
    setModal(true);
  }

  function closeModal() {
    setModal(false);
    setForm({
      reportDate: "",
      weatherCondition: "Sunny",
      workDone: "",
      issues: "",
      nextDayPlan: "",
      manpowerCount: 0,
      equipmentUsed: "",
    });
  }

  function openView(dpr: DPR) {
    setSelectedDPR(dpr);
    setViewModal(true);
  }

  function closeView() {
    setViewModal(false);
    setSelectedDPR(null);
  }

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      Draft: "bg-gray-100 text-gray-600",
      Submitted: "bg-blue-50 text-blue-700",
      Approved: "bg-green-50 text-green-700",
      Rejected: "bg-red-50 text-red-700",
    };
    return styles[status] || "bg-gray-100 text-gray-600";
  };

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
          <h2 className="text-base font-medium text-gray-900">Daily Progress Reports</h2>
          <p className="text-xs text-gray-400">Track daily project progress</p>
        </div>
        <button
          onClick={openModal}
          className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"
        >
          <Plus size={15} /> Create DPR
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : dprs.length === 0 ? (
          <div className="text-center py-10 text-gray-400">No DPRs found</div>
        ) : (
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Date
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Reported By
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Weather
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Manpower
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Status
                </th>
                <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {dprs.map((dpr) => (
                <tr key={dpr.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-2.5 text-gray-700">
                    {new Date(dpr.reportDate).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">{dpr.reportedByName}</td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-1">
                      {weatherIcons[dpr.weatherCondition] || <Cloud size={16} />}
                      <span className="text-xs text-gray-600">{dpr.weatherCondition}</span>
                    </div>
                  </td>
                  <td className="px-4 py-2.5 text-gray-700">{dpr.manpowerCount}</td>
                  <td className="px-4 py-2.5">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(dpr.status)}`}>
                      {dpr.status}
                    </span>
                  </td>
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => openView(dpr)}
                        className="text-xs text-primary-600 hover:underline"
                      >
                        View
                      </button>
                      {dpr.status === "Draft" && (
                        <button
                          onClick={() => submitMutation.mutate(dpr.id)}
                          className="text-xs text-blue-600"
                        >
                          Submit
                        </button>
                      )}
                      {dpr.status === "Submitted" && (
                        <button
                          onClick={() => approveMutation.mutate(dpr.id)}
                          className="text-xs text-green-600"
                        >
                          Approve
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <Modal title="Create DPR" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Report Date *</label>
              <input
                type="date"
                value={form.reportDate}
                onChange={(e) => setForm((f) => ({ ...f, reportDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Weather *</label>
              <select
                value={form.weatherCondition}
                onChange={(e) => setForm((f) => ({ ...f, weatherCondition: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              >
                <option value="Sunny">Sunny</option>
                <option value="Cloudy">Cloudy</option>
                <option value="Rainy">Rainy</option>
                <option value="Snowy">Snowy</option>
              </select>
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Work Done *</label>
            <textarea
              value={form.workDone}
              onChange={(e) => setForm((f) => ({ ...f, workDone: e.target.value }))}
              rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Issues</label>
            <textarea
              value={form.issues}
              onChange={(e) => setForm((f) => ({ ...f, issues: e.target.value }))}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Next Day Plan</label>
            <textarea
              value={form.nextDayPlan}
              onChange={(e) => setForm((f) => ({ ...f, nextDayPlan: e.target.value }))}
              rows={2}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Manpower Count *</label>
              <input
                type="number"
                value={form.manpowerCount}
                onChange={(e) =>
                  setForm((f) => ({ ...f, manpowerCount: parseInt(e.target.value) || 0 }))
                }
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Equipment Used</label>
              <input
                value={form.equipmentUsed}
                onChange={(e) => setForm((f) => ({ ...f, equipmentUsed: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button
              onClick={closeModal}
              className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={() => createMutation.mutate()}
              disabled={createMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {createMutation.isPending ? "Creating..." : "Create"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="DPR Details" open={viewModal} onClose={closeView}>
        {selectedDPR && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-gray-500">Date</p>
                <p className="text-sm font-medium text-gray-900">
                  {new Date(selectedDPR.reportDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Status</p>
                <span className={`text-xs px-2 py-0.5 rounded-full ${getStatusBadge(selectedDPR.status)}`}>
                  {selectedDPR.status}
                </span>
              </div>
              <div>
                <p className="text-xs text-gray-500">Reported By</p>
                <p className="text-sm text-gray-900">{selectedDPR.reportedByName}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Weather</p>
                <div className="flex items-center gap-1">
                  {weatherIcons[selectedDPR.weatherCondition]}
                  <span className="text-sm">{selectedDPR.weatherCondition}</span>
                </div>
              </div>
              <div>
                <p className="text-xs text-gray-500">Manpower</p>
                <p className="text-sm text-gray-900">{selectedDPR.manpowerCount} workers</p>
              </div>
              {selectedDPR.equipmentUsed && (
                <div>
                  <p className="text-xs text-gray-500">Equipment</p>
                  <p className="text-sm text-gray-900">{selectedDPR.equipmentUsed}</p>
                </div>
              )}
            </div>
            <div>
              <p className="text-xs text-gray-500 mb-1">Work Done</p>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">
                {selectedDPR.workDone}
              </p>
            </div>
            {selectedDPR.issues && (
              <div>
                <p className="text-xs text-gray-500 mb-1">Issues</p>
                <p className="text-sm text-gray-700 whitespace-pre-wrap">
                  {selectedDPR.issues}
                </p>
              </div>
            )}
            {selectedDPR.nextDayPlan && (
              <div>
                <p className="text-xs text-gray-500 mb-1">Next Day Plan</p>
                <p className="text-sm text-gray-700 whitespace-pre-wrap">
                  {selectedDPR.nextDayPlan}
                </p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}