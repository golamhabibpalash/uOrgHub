import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getJobPostings,
  createJobPosting,
  getCandidates,
  createCandidate,
  getApplications,
  createApplication,
  getInterviews,
  createInterview,
  JobPosting,
  Candidate,
  JobApplication,
  InterviewSchedule,
  getDepartments,
  getDesignations,
} from "../../api/hr";

export default function Recruitment() {
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<"postings" | "candidates" | "applications" | "interviews">("postings");
  const dg = useDataGrid({ defaultSortBy: "title" });
  const [modal, setModal] = useState(false);

  const [postingForm, setPostingForm] = useState({ title: "", departmentId: "", designationId: "", description: "", requirements: "", location: "", status: "Published", closingDate: "" });
  const [candidateForm, setCandidateForm] = useState({ firstName: "", lastName: "", email: "", phone: "", source: "" });
  const [appForm, setAppForm] = useState({ candidateId: "", jobPostingId: "" });
  const [interviewForm, setInterviewForm] = useState({ applicationId: "", scheduledAt: "", location: "" });

  const { data: postingsData, isLoading: postingsLoading } = useQuery({ queryKey: ["job-postings", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getJobPostings(dg.queryParams) });
  const { data: candidatesData, isLoading: candidatesLoading } = useQuery({ queryKey: ["candidates", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getCandidates(dg.queryParams) });
  const { data: appsData, isLoading: appsLoading } = useQuery({ queryKey: ["applications", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getApplications(dg.queryParams) });
  const { data: interviewsData, isLoading: interviewsLoading } = useQuery({ queryKey: ["interviews", dg.page, dg.search, dg.sortBy, dg.sortDescending], queryFn: () => getInterviews(dg.queryParams) });
  const { data: deptData } = useQuery({ queryKey: ["departments-all"], queryFn: () => getDepartments({ page: 1, pageSize: 100 }) });
  const { data: desigData } = useQuery({ queryKey: ["designations-all"], queryFn: () => getDesignations({ page: 1, pageSize: 100 }) });

  const postings = postingsData?.data?.data?.items ?? [];
  const candidates = candidatesData?.data?.data?.items ?? [];
  const applications = appsData?.data?.data?.items ?? [];
  const interviews = interviewsData?.data?.data?.items ?? [];
  const departments = deptData?.data?.data?.items ?? [];
  const designations = desigData?.data?.data?.items ?? [];

  const postingMutation = useMutation({ mutationFn: () => createJobPosting(postingForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["job-postings"] }); setModal(false); } });
  const candidateMutation = useMutation({ mutationFn: () => createCandidate(candidateForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["candidates"] }); setModal(false); } });
  const appMutation = useMutation({ mutationFn: () => createApplication(appForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["applications"] }); setModal(false); } });
  const interviewMutation = useMutation({ mutationFn: () => createInterview(interviewForm), onSuccess: () => { qc.invalidateQueries({ queryKey: ["interviews"] }); setModal(false); } });

  function openModal(tab: typeof activeTab) {
    setActiveTab(tab);
    setModal(true);
    if (tab === "postings") setPostingForm({ title: "", departmentId: "", designationId: "", description: "", requirements: "", location: "", status: "Published", closingDate: "" });
    if (tab === "candidates") setCandidateForm({ firstName: "", lastName: "", email: "", phone: "", source: "" });
    if (tab === "applications") setAppForm({ candidateId: "", jobPostingId: "" });
    if (tab === "interviews") setInterviewForm({ applicationId: "", scheduledAt: "", location: "" });
  }

  const postingCols = [{ key: "jobCode", label: "Code" }, { key: "title", label: "Job Title" }, { key: "departmentName", label: "Department" }, { key: "location", label: "Location" }, { key: "postedDate", label: "Posted", sortable: false, render: (r: JobPosting) => new Date(r.postedDate).toLocaleDateString() }, { key: "status", label: "Status", sortable: false, render: (r: JobPosting) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Published" ? "bg-green-50 text-green-700" : r.status === "Closed" || r.status === "Cancelled" ? "bg-red-50 text-red-600" : "bg-gray-50 text-gray-600"}`}>{r.status}</span> }];
  const candidateCols = [{ key: "firstName", label: "Name", sortable: false, render: (r: Candidate) => `${r.firstName} ${r.lastName}` }, { key: "email", label: "Email" }, { key: "phone", label: "Phone" }, { key: "source", label: "Source" }, { key: "status", label: "Status", sortable: false, render: (r: Candidate) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Active" ? "bg-green-50 text-green-700" : "bg-gray-50 text-gray-600"}`}>{r.status}</span> }];
  const appCols = [{ key: "candidateName", label: "Candidate" }, { key: "jobTitle", label: "Position" }, { key: "appliedAt", label: "Applied", sortable: false, render: (r: JobApplication) => new Date(r.appliedAt).toLocaleDateString() }, { key: "status", label: "Status", sortable: false, render: (r: JobApplication) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Shortlisted" ? "bg-green-50 text-green-700" : r.status === "Rejected" ? "bg-red-50 text-red-600" : "bg-yellow-50 text-yellow-700"}`}>{r.status}</span> }];
  const interviewCols = [{ key: "candidateName", label: "Candidate" }, { key: "jobTitle", label: "Position" }, { key: "scheduledAt", label: "Date/Time", sortable: false, render: (r: InterviewSchedule) => new Date(r.scheduledAt).toLocaleString() }, { key: "location", label: "Location" }, { key: "status", label: "Status", sortable: false, render: (r: InterviewSchedule) => <span className={`text-xs px-2 py-0.5 rounded-full ${r.status === "Completed" ? "bg-green-50 text-green-700" : r.status === "Cancelled" ? "bg-red-50 text-red-600" : "bg-blue-50 text-blue-700"}`}>{r.status}</span> }];

  const tabs = ["postings", "candidates", "applications", "interviews"] as const;

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div><h2 className="text-base font-medium text-gray-900">Recruitment (ATS)</h2><p className="text-xs text-gray-400">Manage job postings and candidates</p></div>
        <button onClick={() => openModal(activeTab)} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600"><Plus size={15} /> Add {activeTab === "postings" ? "Job" : activeTab === "candidates" ? "Candidate" : activeTab === "applications" ? "Application" : "Interview"}</button>
      </div>

      <div className="flex gap-4 mb-4">
        {tabs.map(tab => <button key={tab} onClick={() => { setActiveTab(tab); dg.setPage(1); }} className={`px-4 py-2 rounded text-sm ${activeTab === tab ? "bg-primary-500 text-white" : "bg-gray-200"}`}>{tab.charAt(0).toUpperCase() + tab.slice(1)}</button>)}
      </div>

      {activeTab === "postings" && (
        <DataGrid
          columns={postingCols}
          data={postings}
          loading={postingsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search job postings..."
          page={dg.page}
          totalPages={postingsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={postingsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No job postings found"
          actions={<ExportMenu baseUrl="recruitment/job-postings" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "candidates" && (
        <DataGrid
          columns={candidateCols}
          data={candidates}
          loading={candidatesLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search candidates..."
          page={dg.page}
          totalPages={candidatesData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={candidatesData?.data?.data?.totalCount ?? 0}
          emptyMessage="No candidates found"
          actions={<ExportMenu baseUrl="recruitment/candidates" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "applications" && (
        <DataGrid
          columns={appCols}
          data={applications}
          loading={appsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search applications..."
          page={dg.page}
          totalPages={appsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={appsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No applications found"
          actions={<ExportMenu baseUrl="recruitment/applications" filters={{ search: dg.search || undefined }} />}
        />
      )}
      {activeTab === "interviews" && (
        <DataGrid
          columns={interviewCols}
          data={interviews}
          loading={interviewsLoading}
          sortBy={dg.sortBy}
          sortDescending={dg.sortDescending}
          onSort={dg.handleSort}
          search={dg.search}
          onSearch={dg.setSearch}
          searchPlaceholder="Search interviews..."
          page={dg.page}
          totalPages={interviewsData?.data?.data?.totalPages ?? 1}
          onPageChange={dg.setPage}
          pageSize={dg.pageSize}
          onPageSizeChange={dg.setPageSize}
          totalCount={interviewsData?.data?.data?.totalCount ?? 0}
          emptyMessage="No interviews found"
          actions={<ExportMenu baseUrl="recruitment/interviews" filters={{ search: dg.search || undefined }} />}
        />
      )}

      <Modal title={activeTab === "postings" ? "Add Job Posting" : activeTab === "candidates" ? "Add Candidate" : activeTab === "applications" ? "Add Application" : "Schedule Interview"} open={modal} onClose={() => setModal(false)}>
        {activeTab === "postings" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Job Title</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.title} onChange={e => setPostingForm(f => ({ ...f, title: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Department</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.departmentId} onChange={e => setPostingForm(f => ({ ...f, departmentId: e.target.value }))}><option value="">Select</option>{departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select></div><div><label className="text-xs text-gray-500 mb-1 block">Designation</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.designationId} onChange={e => setPostingForm(f => ({ ...f, designationId: e.target.value }))}><option value="">Select</option>{designations.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select></div></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Location</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.location} onChange={e => setPostingForm(f => ({ ...f, location: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Description</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.description} onChange={e => setPostingForm(f => ({ ...f, description: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Requirements</label><textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.requirements} onChange={e => setPostingForm(f => ({ ...f, requirements: e.target.value }))} /></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Status</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.status} onChange={e => setPostingForm(f => ({ ...f, status: e.target.value }))}><option value="Published">Published</option><option value="Closed">Closed</option><option value="OnHold">On Hold</option></select></div><div><label className="text-xs text-gray-500 mb-1 block">Closing Date</label><input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={postingForm.closingDate} onChange={e => setPostingForm(f => ({ ...f, closingDate: e.target.value }))} /></div></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => postingMutation.mutate()} disabled={postingMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{postingMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "candidates" && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">First Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={candidateForm.firstName} onChange={e => setCandidateForm(f => ({ ...f, firstName: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Last Name</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={candidateForm.lastName} onChange={e => setCandidateForm(f => ({ ...f, lastName: e.target.value }))} /></div></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Email</label><input type="email" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={candidateForm.email} onChange={e => setCandidateForm(f => ({ ...f, email: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Phone</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={candidateForm.phone} onChange={e => setCandidateForm(f => ({ ...f, phone: e.target.value }))} /></div></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Source</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={candidateForm.source} onChange={e => setCandidateForm(f => ({ ...f, source: e.target.value }))}><option value="">Select</option><option value="LinkedIn">LinkedIn</option><option value="Referral">Referral</option><option value="JobPortal">Job Portal</option><option value="Direct">Direct</option></select></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => candidateMutation.mutate()} disabled={candidateMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{candidateMutation.isPending ? "Saving..." : "Save"}</button></div>
          </div>
        )}
        {activeTab === "applications" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Candidate</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={appForm.candidateId} onChange={e => setAppForm(f => ({ ...f, candidateId: e.target.value }))}><option value="">Select</option>{candidates.map(c => <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>)}</select></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Job Posting</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={appForm.jobPostingId} onChange={e => setAppForm(f => ({ ...f, jobPostingId: e.target.value }))}><option value="">Select</option>{postings.filter(p => p.status === "Published").map(p => <option key={p.id} value={p.id}>{p.title}</option>)}</select></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => appMutation.mutate()} disabled={appMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{appMutation.isPending ? "Submitting..." : "Submit"}</button></div>
          </div>
        )}
        {activeTab === "interviews" && (
          <div className="space-y-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Application</label><select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={interviewForm.applicationId} onChange={e => setInterviewForm(f => ({ ...f, applicationId: e.target.value }))}><option value="">Select</option>{applications.map(a => <option key={a.id} value={a.id}>{a.candidateName} - {a.jobTitle}</option>)}</select></div>
            <div className="grid grid-cols-2 gap-3"><div><label className="text-xs text-gray-500 mb-1 block">Date/Time</label><input type="datetime-local" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={interviewForm.scheduledAt} onChange={e => setInterviewForm(f => ({ ...f, scheduledAt: e.target.value }))} /></div><div><label className="text-xs text-gray-500 mb-1 block">Location</label><input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={interviewForm.location} onChange={e => setInterviewForm(f => ({ ...f, location: e.target.value }))} /></div></div>
            <div className="flex justify-end gap-2 pt-2"><button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button><button onClick={() => interviewMutation.mutate()} disabled={interviewMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">{interviewMutation.isPending ? "Scheduling..." : "Schedule"}</button></div>
          </div>
        )}
      </Modal>
    </div>
  );
}