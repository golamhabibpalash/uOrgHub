import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getReviewCycles,
  createReviewCycle,
  getGoals,
  createGoal,
  getPerformanceReviews,
  createPerformanceReview,
  getTrainingPrograms,
  createTrainingProgram,
  getEmployeeTrainings,
  createEmployeeTraining,
  ReviewCycle,
  Goal,
  PerformanceReview,
  TrainingProgram,
  EmployeeTraining,
  getEmployees,
} from "../../api/hr";

export default function PerformanceManagement() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<"cycles" | "goals" | "reviews" | "training">("cycles");
  const [page, setPage] = useState(1);
  const [modal, setModal] = useState(false);
  const [trainingMode, setTrainingMode] = useState<"add" | "enroll">("add");

  const [cycleForm, setCycleForm] = useState({ name: "", startDate: "", endDate: "" });
  const [goalForm, setGoalForm] = useState({ employeeId: "", title: "", description: "", targetDate: "" });
  const [reviewForm, setReviewForm] = useState({ employeeId: "", reviewerId: "", reviewCycleId: "" });
  const [trainingForm, setTrainingForm] = useState({ employeeId: "", trainingProgramId: "" });
  const [programForm, setProgramForm] = useState({ title: "", description: "", provider: "", startDate: "", endDate: "", maxParticipants: 0 });

  const { data: cyclesData, isLoading: cyclesLoading } = useQuery({ queryKey: ["review-cycles", page], queryFn: () => getReviewCycles({ page, pageSize: 10 }) });
  const { data: goalsData, isLoading: goalsLoading } = useQuery({ queryKey: ["goals", page], queryFn: () => getGoals({ page, pageSize: 10 }) });
  const { data: reviewsData, isLoading: reviewsLoading } = useQuery({ queryKey: ["performance-reviews", page], queryFn: () => getPerformanceReviews({ page, pageSize: 10 }) });
  const { data: programsData, isLoading: programsLoading } = useQuery({ queryKey: ["training-programs", page], queryFn: () => getTrainingPrograms({ page, pageSize: 10 }) });
  const { data: empTrainingsData, isLoading: empTrainingsLoading } = useQuery({ queryKey: ["employee-trainings", page], queryFn: () => getEmployeeTrainings({ page, pageSize: 10 }) });
  const { data: empData } = useQuery({ queryKey: ["employees-all"], queryFn: () => getEmployees({ page: 1, pageSize: 100 }) });

  const cycles = cyclesData?.data?.data?.items ?? [];
  const goals = goalsData?.data?.data?.items ?? [];
  const reviews = reviewsData?.data?.data?.items ?? [];
  const programs = programsData?.data?.data?.items ?? [];
  const empTrainings = empTrainingsData?.data?.data?.items ?? [];
  const employees = empData?.data?.data?.items ?? [];

  const totalPages = activeTab === "cycles" ? cyclesData?.data?.data?.totalPages ?? 1
    : activeTab === "goals" ? goalsData?.data?.data?.totalPages ?? 1
    : activeTab === "reviews" ? reviewsData?.data?.data?.totalPages ?? 1
    : activeTab === "training" ? programsData?.data?.data?.totalPages ?? 1
    : 1;

  const cycleMutation = useMutation({ mutationFn: () => createReviewCycle(cycleForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["review-cycles"] }); setModal(false); } });
  const goalMutation = useMutation({ mutationFn: () => createGoal(goalForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["goals"] }); setModal(false); } });
  const reviewMutation = useMutation({ mutationFn: () => createPerformanceReview(reviewForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["performance-reviews"] }); setModal(false); } });
  const programMutation = useMutation({ mutationFn: () => createTrainingProgram(programForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["training-programs"] }); setModal(false); } });
  const enrollMutation = useMutation({ mutationFn: () => createEmployeeTraining(trainingForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["employee-trainings"] }); setModal(false); }, });

  function openModal(tab: typeof activeTab, action: "add" | "enroll" = "add") {
    setActiveTab(tab);
    setModal(true);
    if (tab === "cycles") setCycleForm({ name: "", startDate: "", endDate: "" });
    if (tab === "goals") setGoalForm({ employeeId: "", title: "", description: "", targetDate: "" });
    if (tab === "reviews") setReviewForm({ employeeId: "", reviewerId: "", reviewCycleId: "" });
    if (tab === "training") {
      setTrainingMode(action);
      if (action === "enroll") {
        setTrainingForm({ employeeId: "", trainingProgramId: "" });
      } else {
        setProgramForm({ title: "", description: "", provider: "", startDate: "", endDate: "", maxParticipants: 0 });
      }
    }
  }

  function handleEnroll() {
    if (trainingForm.employeeId && trainingForm.trainingProgramId) {
      enrollMutation.mutate();
    }
  }

  const cycleCols = [{ key: "name", label: "Cycle Name" }, { key: "startDate", label: "Start", render: (r: ReviewCycle) => new Date(r.startDate).toLocaleDateString() }, { key: "endDate", label: "End", render: (r: ReviewCycle) => new Date(r.endDate).toLocaleDateString() }, { key: "status", label: "Status", render: (r: ReviewCycle) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Active" ? "bg-green-50 text-green-700" : "bg-gray-50 text-gray-600"}`}>{r.status}</span> }];
  const goalCols = [{ key: "employeeName", label: "Employee" }, { key: "title", label: "Goal" }, { key: "description", label: "Description" }, { key: "targetDate", label: "Target", render: (r: Goal) => new Date(r.targetDate).toLocaleDateString() }, { key: "progress", label: "Progress", render: (r: Goal) => <div className="w-20 bg-gray-200 rounded-full h-2"><div className="bg-primary-500 h-2 rounded-full" style={{ width: `${r.progress}%` }} /></div> }, { key: "status", label: "Status", render: (r: Goal) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Completed" ? "bg-green-50 text-green-700" : r.status === "InProgress" ? "bg-blue-50 text-blue-700" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> }];
  const reviewCols = [{ key: "employeeName", label: "Employee" }, { key: "reviewerName", label: "Reviewer" }, { key: "reviewCycleName", label: "Cycle" }, { key: "rating", label: "Rating", render: (r: PerformanceReview) => `${r.rating}/5` }, { key: "status", label: "Status", render: (r: PerformanceReview) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Submitted" ? "bg-green-50 text-green-700" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> }];
  const programCols = [{ key: "title", label: "Program Name" }, { key: "provider", label: "Trainer" }, { key: "startDate", label: "Start", render: (r: TrainingProgram) => new Date(r.startDate).toLocaleDateString() }, { key: "endDate", label: "End", render: (r: TrainingProgram) => new Date(r.endDate).toLocaleDateString() }, { key: "maxParticipants", label: "Max Participants" }, { key: "isActive", label: "Status", render: (r: TrainingProgram) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{r.isActive ? "Active" : "Inactive"}</span> }];

  const tabs = ["cycles", "goals", "reviews", "training"] as const;

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div><h2 className="text-base font-medium text-gray-900">Performance & Training</h2><p className="text-xs text-gray-400">Manage goals, reviews and training</p></div>
        <button onClick={() => openModal(activeTab)} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"><Plus size={15} /> Add {activeTab === "cycles" ? "Cycle" : activeTab === "goals" ? "Goal" : activeTab === "reviews" ? "Review" : "Program"}</button>
      </div>

      <div className="flex gap-4 mb-4">
        {tabs.map(tab => <button key={tab} onClick={() => { setActiveTab(tab); setPage(1); }} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>{tab === "cycles" ? "Review Cycles" : tab === "goals" ? "Goals" : tab === "reviews" ? "Reviews" : "Training"}</button>)}
      </div>

      {activeTab !== "training" && (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          {activeTab === "cycles" && (
            <>
              <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
                <ExportMenu baseUrl="performance/review-cycles" />
              </div>
              <DataTable columns={cycleCols} data={cycles} loading={cyclesLoading} />
            </>
          )}
          {activeTab === "goals" && (
            <>
              <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
                <ExportMenu baseUrl="performance/goals" />
              </div>
              <DataTable columns={goalCols} data={goals} loading={goalsLoading} />
            </>
          )}
          {activeTab === "reviews" && (
            <>
              <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
                <ExportMenu baseUrl="performance/reviews" />
              </div>
              <DataTable columns={reviewCols} data={reviews} loading={reviewsLoading} />
            </>
          )}
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </div>
      )}

      {activeTab === "training" && (
        <div className="space-y-4">
          <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <h3 className="text-sm font-medium">Training Programs</h3>
              <div className="flex items-center gap-2">
                <ExportMenu baseUrl="performance/training-programs" />
                <button onClick={() => openModal("training", "add")} className="text-xs bg-primary-500 text-white px-3 py-1 rounded hover:bg-primary-600">Add Program</button>
              </div>
            </div>
            <DataTable columns={programCols} data={programs} loading={programsLoading} />
            <Pagination page={page} totalPages={programsData?.data?.data?.totalPages ?? 1} onPageChange={setPage} />
          </div>
          <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
              <h3 className="text-sm font-medium">Employee Enrollments</h3>
              <div className="flex items-center gap-2">
                <ExportMenu baseUrl="performance/employee-trainings" />
                <button onClick={() => openModal("training", "enroll")} className="text-xs bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600">Enroll Employee</button>
              </div>
            </div>
            <DataTable columns={[{ key: "employeeName", label: "Employee" }, { key: "trainingTitle", label: "Program" }, { key: "status", label: "Status", render: (r: EmployeeTraining) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Completed" ? "bg-green-50 text-green-700" : r.status === "InProgress" ? "bg-blue-50 text-blue-700" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> }, { key: "completionDate", label: "Completed", render: (r: EmployeeTraining) => r.completionDate ? new Date(r.completionDate).toLocaleDateString() : "-" }]} data={empTrainings} loading={empTrainingsLoading} />
          </div>
        </div>
      )}

      <Modal title={activeTab === "cycles" ? "Add Review Cycle" : activeTab === "goals" ? "Add Goal" : activeTab === "reviews" ? "Add Review" : trainingMode === "enroll" ? "Enroll Employee" : "Add Training Program"} open={modal} onClose={() => setModal(false)}>
        {activeTab === "cycles" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Cycle Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.name} onChange={e => setCycleForm(f => ({ ...f, name: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Start Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.startDate} onChange={e => setCycleForm(f => ({ ...f, startDate: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">End Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={cycleForm.endDate} onChange={e => setCycleForm(f => ({ ...f, endDate: e.target.value }))} /></div></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => cycleMutation.mutate()} disabled={cycleMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{cycleMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "goals" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Employee</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.employeeId} onChange={e => setGoalForm(f => ({ ...f, employeeId: e.target.value }))}><option value="">Select</option>{employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}</select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Goal Title</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.title} onChange={e => setGoalForm(f => ({ ...f, title: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.description} onChange={e => setGoalForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Target Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.targetDate} onChange={e => setGoalForm(f => ({ ...f, targetDate: e.target.value }))} /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => goalMutation.mutate()} disabled={goalMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{goalMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "reviews" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Employee</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={reviewForm.employeeId} onChange={e => setReviewForm(f => ({ ...f, employeeId: e.target.value }))}><option value="">Select</option>{employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}</select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Reviewer</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={reviewForm.reviewerId} onChange={e => setReviewForm(f => ({ ...f, reviewerId: e.target.value }))}><option value="">Select</option>{employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}</select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Review Cycle</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={reviewForm.reviewCycleId} onChange={e => setReviewForm(f => ({ ...f, reviewCycleId: e.target.value }))}><option value="">Select</option>{cycles.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}</select></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => reviewMutation.mutate()} disabled={reviewMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{reviewMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "training" && trainingMode === "enroll" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Employee</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={trainingForm.employeeId} onChange={e => setTrainingForm(f => ({ ...f, employeeId: e.target.value }))}><option value="">Select</option>{employees.map(e => <option key={e.id} value={e.id}>{e.firstName} {e.lastName}</option>)}</select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Training Program</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={trainingForm.trainingProgramId} onChange={e => setTrainingForm(f => ({ ...f, trainingProgramId: e.target.value }))}><option value="">Select</option>{programs.map(p => <option key={p.id} value={p.id}>{p.title}</option>)}</select></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={handleEnroll} disabled={enrollMutation.isPending || !trainingForm.employeeId || !trainingForm.trainingProgramId} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{enrollMutation.isPending ? "Enrolling..." : "Enroll"}</button></div>
          </div>
        )}
        {activeTab === "training" && trainingMode === "add" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Program Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={programForm.title} onChange={e => setProgramForm(f => ({ ...f, title: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Trainer</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={programForm.provider} onChange={e => setProgramForm(f => ({ ...f, provider: e.target.value }))} /></div>
            <div className="grid grid-cols-3 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Start</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={programForm.startDate} onChange={e => setProgramForm(f => ({ ...f, startDate: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">End</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={programForm.endDate} onChange={e => setProgramForm(f => ({ ...f, endDate: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Max</label><input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={programForm.maxParticipants} onChange={e => setProgramForm(f => ({ ...f, maxParticipants: Number(e.target.value) }))} /></div></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={programForm.description} onChange={e => setProgramForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => programMutation.mutate()} disabled={programMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{programMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
      </Modal>
    </div>
  );
}