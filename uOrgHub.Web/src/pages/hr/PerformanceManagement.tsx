import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useEmployeeLookup } from "../../hooks/useEntityLookup";
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
} from "../../api/hr";

export default function PerformanceManagement() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<"cycles" | "goals" | "reviews" | "training">("cycles");
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [trainingMode, setTrainingMode] = useState<"add" | "enroll">("add");

  const [cycleForm, setCycleForm] = useState({ name: "", startDate: "", endDate: "" });
  const [goalForm, setGoalForm] = useState({ employeeId: "", title: "", description: "", targetDate: "" });
  const [reviewForm, setReviewForm] = useState({ employeeId: "", reviewerId: "", reviewCycleId: "" });
  const [trainingForm, setTrainingForm] = useState({ employeeId: "", trainingProgramId: "" });
  const [programForm, setProgramForm] = useState({ title: "", description: "", provider: "", startDate: "", endDate: "", maxParticipants: 0 });

  const { data: cyclesData, isLoading: cyclesLoading } = useQuery({ queryKey: ["review-cycles", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getReviewCycles(dg.queryParams) });
  const { data: goalsData, isLoading: goalsLoading } = useQuery({ queryKey: ["goals", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getGoals(dg.queryParams) });
  const { data: reviewsData, isLoading: reviewsLoading } = useQuery({ queryKey: ["performance-reviews", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getPerformanceReviews(dg.queryParams) });
  const { data: programsData, isLoading: programsLoading } = useQuery({ queryKey: ["training-programs", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getTrainingPrograms(dg.queryParams) });
  const { data: empTrainingsData, isLoading: empTrainingsLoading } = useQuery({ queryKey: ["employee-trainings", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getEmployeeTrainings(dg.queryParams) });
  const { options: empOptions, isLoading: empLoading } = useEmployeeLookup();

  const cycles = cyclesData?.data?.data?.items ?? [];
  const goals = goalsData?.data?.data?.items ?? [];
  const reviews = reviewsData?.data?.data?.items ?? [];
  const programs = programsData?.data?.data?.items ?? [];
  const empTrainings = empTrainingsData?.data?.data?.items ?? [];

  const cycleOptions = useMemo(
    () => cycles.map((c) => ({ value: c.id, label: c.name })),
    [cycles],
  );
  const programOptions = useMemo(
    () => programs.map((p) => ({ value: p.id, label: p.title })),
    [programs],
  );

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

  const cycleCols = [{ key: "name", label: "Cycle Name" }, { key: "startDate", label: "Start", sortable: false, render: (r: ReviewCycle) => new Date(r.startDate).toLocaleDateString() }, { key: "endDate", label: "End", sortable: false, render: (r: ReviewCycle) => new Date(r.endDate).toLocaleDateString() }, { key: "status", label: "Status", sortable: false, render: (r: ReviewCycle) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Active" ? "bg-green-50 text-green-700" : "bg-gray-50 text-gray-600"}`}>{r.status}</span> }];
  const goalCols = [{ key: "employeeName", label: "Employee" }, { key: "title", label: "Goal" }, { key: "description", label: "Description" }, { key: "targetDate", label: "Target", sortable: false, render: (r: Goal) => new Date(r.targetDate).toLocaleDateString() }, { key: "progress", label: "Progress", sortable: false, render: (r: Goal) => <div className="w-20 bg-gray-200 rounded-full h-2"><div className="bg-primary-500 h-2 rounded-full" style={{ width: `${r.progress}%` }} /></div> }, { key: "status", label: "Status", sortable: false, render: (r: Goal) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Completed" ? "bg-green-50 text-green-700" : r.status === "InProgress" ? "bg-blue-50 text-blue-700" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> }];
  const reviewCols = [{ key: "employeeName", label: "Employee" }, { key: "reviewerName", label: "Reviewer" }, { key: "reviewCycleName", label: "Cycle" }, { key: "rating", label: "Rating", sortable: false, render: (r: PerformanceReview) => `${r.rating}/5` }, { key: "status", label: "Status", sortable: false, render: (r: PerformanceReview) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Submitted" ? "bg-green-50 text-green-700" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> }];
  const programCols = [{ key: "title", label: "Program Name" }, { key: "provider", label: "Trainer" }, { key: "startDate", label: "Start", sortable: false, render: (r: TrainingProgram) => new Date(r.startDate).toLocaleDateString() }, { key: "endDate", label: "End", sortable: false, render: (r: TrainingProgram) => new Date(r.endDate).toLocaleDateString() }, { key: "maxParticipants", label: "Max Participants" }, { key: "isActive", label: "Status", sortable: false, render: (r: TrainingProgram) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>{r.isActive ? "Active" : "Inactive"}</span> }];

  const tabs = ["cycles", "goals", "reviews", "training"] as const;

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div><h2 className="text-base font-medium text-gray-900">Performance & Training</h2><p className="text-xs text-gray-400">Manage goals, reviews and training</p></div>
        <button onClick={() => openModal(activeTab)} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"><Plus size={15} /> Add {activeTab === "cycles" ? "Cycle" : activeTab === "goals" ? "Goal" : activeTab === "reviews" ? "Review" : "Program"}</button>
      </div>

      <div className="flex gap-4 mb-4">
        {tabs.map(tab => <button key={tab} onClick={() => { setActiveTab(tab); dg.setPage(1); }} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>{tab === "cycles" ? "Review Cycles" : tab === "goals" ? "Goals" : tab === "reviews" ? "Reviews" : "Training"}</button>)}
      </div>

      {activeTab === "cycles" && (
        <DataGrid
          columns={cycleCols}
          data={cycles}
          loading={cyclesLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search cycles..."
          page={dg.page}
          totalPages={cyclesData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={cyclesData?.data?.data?.totalCount ?? 0}
          emptyMessage="No review cycles found"
          actions={<ExportMenu baseUrl="performance/review-cycles" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "goals" && (
        <DataGrid
          columns={goalCols}
          data={goals}
          loading={goalsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search goals..."
          page={dg.page}
          totalPages={goalsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={goalsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No goals found"
          actions={<ExportMenu baseUrl="performance/goals" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "reviews" && (
        <DataGrid
          columns={reviewCols}
          data={reviews}
          loading={reviewsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search reviews..."
          page={dg.page}
          totalPages={reviewsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={reviewsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No reviews found"
          actions={<ExportMenu baseUrl="performance/reviews" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "training" && (
        <div className="space-y-4">
          <DataGrid
            columns={programCols}
            data={programs}
            loading={programsLoading}
            sortBy={dg.sortBy}
            sortDescending={dg.sortDescending}
            onSort={dg.handleSort}
            search={dg.search}
            onSearch={dg.setSearch}
            searchPlaceholder="Search programs..."
            page={dg.page}
            totalPages={programsData?.data?.data?.totalPages ?? 1}
            onPageChange={dg.setPage}
            pageSize={dg.pageSize}
            onPageSizeChange={dg.setPageSize}
            totalCount={programsData?.data?.data?.totalCount ?? 0}
            emptyMessage="No training programs found"
            toolbarPrefix={<h3 className="text-sm font-medium text-gray-700 whitespace-nowrap">Training Programs</h3>}
            actions={
              <div className="flex items-center gap-2">
                <ExportMenu baseUrl="performance/training-programs" filters={{ search: dg.search || undefined }} />
                <button onClick={() => openModal("training", "add")} className="text-xs bg-primary-500 text-white px-3 py-1 rounded hover:bg-primary-600">Add Program</button>
              </div>
            }
          />
          <DataGrid
            columns={[
              { key: "employeeName", label: "Employee" },
              { key: "trainingTitle", label: "Program" },
              { key: "status", label: "Status", sortable: false, render: (r: EmployeeTraining) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Completed" ? "bg-green-50 text-green-700" : r.status === "InProgress" ? "bg-blue-50 text-blue-700" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> },
              { key: "completionDate", label: "Completed", sortable: false, render: (r: EmployeeTraining) => r.completionDate ? new Date(r.completionDate).toLocaleDateString() : "-" },
            ]}
            data={empTrainings}
            loading={empTrainingsLoading}
            sortBy={dg.sortBy}
            sortDescending={dg.sortDescending}
            onSort={dg.handleSort}
            search={dg.search}
            onSearch={dg.setSearch}
            searchPlaceholder="Search enrollments..."
            page={dg.page}
            totalPages={empTrainingsData?.data?.data?.totalPages ?? 1}
            onPageChange={dg.setPage}
            pageSize={dg.pageSize}
            onPageSizeChange={dg.setPageSize}
            totalCount={empTrainingsData?.data?.data?.totalCount ?? 0}
            emptyMessage="No enrollments found"
            toolbarPrefix={<h3 className="text-sm font-medium text-gray-700 whitespace-nowrap">Employee Enrollments</h3>}
            actions={
              <div className="flex items-center gap-2">
                <ExportMenu baseUrl="performance/employee-trainings" filters={{ search: dg.search || undefined }} />
                <button onClick={() => openModal("training", "enroll")} className="text-xs bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600">Enroll Employee</button>
              </div>
            }
          />
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
            <div><SearchableDropdown label="Employee" options={empOptions} value={goalForm.employeeId} onChange={v => setGoalForm(f => ({ ...f, employeeId: v || "" }))} placeholder="Select" searchPlaceholder="Search employee..." loading={empLoading} required /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Goal Title</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.title} onChange={e => setGoalForm(f => ({ ...f, title: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.description} onChange={e => setGoalForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Target Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={goalForm.targetDate} onChange={e => setGoalForm(f => ({ ...f, targetDate: e.target.value }))} /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => goalMutation.mutate()} disabled={goalMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{goalMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "reviews" && (
          <div className="space-y-3">
            <div><SearchableDropdown label="Employee" options={empOptions} value={reviewForm.employeeId} onChange={v => setReviewForm(f => ({ ...f, employeeId: v || "" }))} placeholder="Select" searchPlaceholder="Search employee..." loading={empLoading} required /></div>
            <div><SearchableDropdown label="Reviewer" options={empOptions} value={reviewForm.reviewerId} onChange={v => setReviewForm(f => ({ ...f, reviewerId: v || "" }))} placeholder="Select" searchPlaceholder="Search employee..." loading={empLoading} required /></div>
            <div><SearchableDropdown label="Review Cycle" options={cycleOptions} value={reviewForm.reviewCycleId} onChange={v => setReviewForm(f => ({ ...f, reviewCycleId: v ?? "" }))} placeholder="Select" searchPlaceholder="Search cycles..." /></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => reviewMutation.mutate()} disabled={reviewMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{reviewMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "training" && trainingMode === "enroll" && (
          <div className="space-y-3">
            <div><SearchableDropdown label="Employee" options={empOptions} value={trainingForm.employeeId} onChange={v => setTrainingForm(f => ({ ...f, employeeId: v || "" }))} placeholder="Select" searchPlaceholder="Search employee..." loading={empLoading} required /></div>
            <div><SearchableDropdown label="Training Program" options={programOptions} value={trainingForm.trainingProgramId} onChange={v => setTrainingForm(f => ({ ...f, trainingProgramId: v ?? "" }))} placeholder="Select" searchPlaceholder="Search programs..." /></div>
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