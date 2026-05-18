import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

export interface Project {
  id: string;
  projectCode: string;
  projectName: string;
  clientId: string;
  clientName: string;
  categoryId: string;
  categoryName: string;
  projectManagerId: string;
  projectManagerName: string;
  location: string;
  siteAddress: string;
  startDate: string;
  plannedEndDate: string;
  actualEndDate?: string;
  contractValue: number;
  status: string;
  priority: string;
  description?: string;
  progress: number;
  createdAt: string;
}

export interface ProjectCategory {
  id: string;
  name: string;
  code: string;
  description?: string;
}

export interface ProjectDashboard {
  activeProjects: number;
  totalBOQValue: number;
  pendingDPRs: number;
  materialRequests: number;
}

export interface ProjectBudgetSummary {
  contractValue: number;
  boqEstimated: number;
  boqApproved: number;
  totalExpenses: number;
  remainingBudget: number;
  percentUsed: number;
}

export interface ProjectTeamMember {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  role: string;
  assignedDate: string;
}

export interface ProjectProgress {
  overallProgress: number;
  wbsProgress: number;
  milestoneProgress: number;
  budgetProgress: number;
}

export interface WBS {
  id: string;
  projectId: string;
  wbsCode: string;
  title: string;
  parentWbsId?: string;
  parentWbsCode?: string;
  plannedStartDate: string;
  plannedEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  completionPercent: number;
  status: string;
  description?: string;
  children?: WBS[];
}

export interface BOQ {
  id: string;
  projectId: string;
  boqNumber: string;
  title: string;
  status: string;
  totalEstimatedCost: number;
  totalActualCost: number;
  approvedBy?: string;
  approvedDate?: string;
  createdAt: string;
}

export interface BOQItem {
  id: string;
  boqId: string;
  sequence: number;
  description: string;
  specification?: string;
  uom: string;
  estimatedQty: number;
  unitRate: number;
  estimatedAmount: number;
  actualQty?: number;
  actualAmount?: number;
  itemVariantId?: string;
  itemVariantName?: string;
  wbsId?: string;
  wbsCode?: string;
}

export interface Milestone {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  plannedDate: string;
  actualDate?: string;
  status: string;
  isCritical: boolean;
  wbsId?: string;
  wbsCode?: string;
}

export interface DPR {
  id: string;
  projectId: string;
  dprNumber: string;
  reportDate: string;
  weatherCondition: string;
  workDone: string;
  issues?: string;
  nextDayPlan?: string;
  manpowerCount: number;
  equipmentUsed?: string;
  reportedById: string;
  reportedByName: string;
  status: string;
  approvedById?: string;
  approvedByName?: string;
  createdAt: string;
}

export interface MaterialRequest {
  id: string;
  projectId: string;
  requestNumber: string;
  requestDate: string;
  requiredDate: string;
  notes?: string;
  status: string;
  itemsCount: number;
  submittedById?: string;
  submittedByName?: string;
  approvedById?: string;
  approvedByName?: string;
}

export interface MaterialRequestItem {
  id: string;
  materialRequestId: string;
  itemVariantId?: string;
  itemVariantName?: string;
  boqItemId?: string;
  quantity: number;
  uom: string;
  notes?: string;
}

export interface Expense {
  id: string;
  projectId: string;
  expenseNumber: string;
  expenseDate: string;
  expenseType: string;
  description: string;
  amount: number;
  vendorId?: string;
  vendorName?: string;
  invoiceNumber?: string;
  notes?: string;
  status: string;
  approvedById?: string;
  approvedByName?: string;
  createdAt: string;
}

export interface Client {
  id: string;
  clientCode: string;
  companyName: string;
  contactPerson: string;
  email: string;
  phone: string;
  address?: string;
  type: string;
  status: string;
  createdAt: string;
}

export const getProjects = (params: PaginationRequest, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<Project>>>("/projects", {
    params: { ...params, status },
  });

export const getProjectById = (id: string) =>
  apiClient.get<ApiResponse<Project>>(`/projects/${id}`);

export const createProject = (data: Partial<Project>) =>
  apiClient.post<ApiResponse<Project>>("/projects", data);

export const updateProject = (id: string, data: Partial<Project>) =>
  apiClient.put<ApiResponse<Project>>(`/projects/${id}`, data);

export const deleteProject = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/projects/${id}`);

export const getProjectDashboard = () =>
  apiClient.get<ApiResponse<ProjectDashboard>>("/projects/dashboard");

export const getProjectWBS = (projectId: string) =>
  apiClient.get<ApiResponse<WBS[]>>(`/projects/${projectId}/wbs`);

export const getProjectBudgetSummary = (projectId: string) =>
  apiClient.get<ApiResponse<ProjectBudgetSummary>>(`/projects/${projectId}/budget-summary`);

export const getProjectTeam = (projectId: string) =>
  apiClient.get<ApiResponse<ProjectTeamMember[]>>(`/projects/${projectId}/team`);

export const addTeamMember = (projectId: string, data: { employeeId: string; role: string }) =>
  apiClient.post<ApiResponse<ProjectTeamMember>>(`/projects/${projectId}/team`, data);

export const removeTeamMember = (projectId: string, memberId: string) =>
  apiClient.delete<ApiResponse<null>>(`/projects/${projectId}/team/${memberId}`);

export const getProjectProgress = (projectId: string) =>
  apiClient.get<ApiResponse<ProjectProgress>>(`/projects/${projectId}/progress`);

export const getProjectCategories = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<ProjectCategory>>>("/projects/categories", { params });

export const getProjectByIdWithDetails = (id: string) =>
  apiClient.get<ApiResponse<Project>>(`/projects/${id}/details`);

export const getWBSList = (projectId: string) =>
  apiClient.get<ApiResponse<WBS[]>>(`/projects/${projectId}/wbs`);

export const createWBS = (projectId: string, data: Partial<WBS>) =>
  apiClient.post<ApiResponse<WBS>>(`/projects/${projectId}/wbs`, data);

export const updateWBS = (projectId: string, wbsId: string, data: Partial<WBS>) =>
  apiClient.put<ApiResponse<WBS>>(`/projects/${projectId}/wbs/${wbsId}`, data);

export const deleteWBS = (projectId: string, wbsId: string) =>
  apiClient.delete<ApiResponse<null>>(`/projects/${projectId}/wbs/${wbsId}`);

export const updateWBSCompletion = (projectId: string, wbsId: string, completionPercent: number) =>
  apiClient.put<ApiResponse<WBS>>(`/projects/${projectId}/wbs/${wbsId}/completion`, { completionPercent });

export const getBOQList = (projectId: string) =>
  apiClient.get<ApiResponse<BOQ[]>>(`/projects/${projectId}/boqs`);

export const getBOQById = (projectId: string, boqId: string) =>
  apiClient.get<ApiResponse<BOQ>>(`/projects/${projectId}/boqs/${boqId}`);

export const createBOQ = (projectId: string, data: Partial<BOQ>) =>
  apiClient.post<ApiResponse<BOQ>>(`/projects/${projectId}/boqs`, data);

export const updateBOQ = (projectId: string, boqId: string, data: Partial<BOQ>) =>
  apiClient.put<ApiResponse<BOQ>>(`/projects/${projectId}/boqs/${boqId}`, data);

export const approveBOQ = (projectId: string, boqId: string) =>
  apiClient.post<ApiResponse<BOQ>>(`/projects/${projectId}/boqs/${boqId}/approve`, {});

export const getBOQItems = (projectId: string, boqId: string) =>
  apiClient.get<ApiResponse<BOQItem[]>>(`/projects/${projectId}/boqs/${boqId}/items`);

export const createBOQItem = (projectId: string, boqId: string, data: Partial<BOQItem>) =>
  apiClient.post<ApiResponse<BOQItem>>(`/projects/${projectId}/boqs/${boqId}/items`, data);

export const updateBOQItem = (projectId: string, boqId: string, itemId: string, data: Partial<BOQItem>) =>
  apiClient.put<ApiResponse<BOQItem>>(`/projects/${projectId}/boqs/${boqId}/items/${itemId}`, data);

export const deleteBOQItem = (projectId: string, boqId: string, itemId: string) =>
  apiClient.delete<ApiResponse<null>>(`/projects/${projectId}/boqs/${boqId}/items/${itemId}`);

export const getMilestones = (projectId: string) =>
  apiClient.get<ApiResponse<Milestone[]>>(`/projects/${projectId}/milestones`);

export const createMilestone = (projectId: string, data: Partial<Milestone>) =>
  apiClient.post<ApiResponse<Milestone>>(`/projects/${projectId}/milestones`, data);

export const updateMilestone = (projectId: string, milestoneId: string, data: Partial<Milestone>) =>
  apiClient.put<ApiResponse<Milestone>>(`/projects/${projectId}/milestones/${milestoneId}`, data);

export const deleteMilestone = (projectId: string, milestoneId: string) =>
  apiClient.delete<ApiResponse<null>>(`/projects/${projectId}/milestones/${milestoneId}`);

export const getDPRs = (projectId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<DPR>>>(`/projects/${projectId}/dprs`, { params });

export const getDPRById = (projectId: string, dprId: string) =>
  apiClient.get<ApiResponse<DPR>>(`/projects/${projectId}/dprs/${dprId}`);

export const createDPR = (projectId: string, data: Partial<DPR>) =>
  apiClient.post<ApiResponse<DPR>>(`/projects/${projectId}/dprs`, data);

export const updateDPR = (projectId: string, dprId: string, data: Partial<DPR>) =>
  apiClient.put<ApiResponse<DPR>>(`/projects/${projectId}/dprs/${dprId}`, data);

export const submitDPR = (projectId: string, dprId: string) =>
  apiClient.post<ApiResponse<DPR>>(`/projects/${projectId}/dprs/${dprId}/submit`, {});

export const approveDPR = (projectId: string, dprId: string) =>
  apiClient.post<ApiResponse<DPR>>(`/projects/${projectId}/dprs/${dprId}/approve`, {});

export const getMaterialRequests = (projectId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<MaterialRequest>>>(`/projects/${projectId}/material-requests`, { params });

export const getMaterialRequestById = (projectId: string, requestId: string) =>
  apiClient.get<ApiResponse<MaterialRequest>>(`/projects/${projectId}/material-requests/${requestId}`);

export const createMaterialRequest = (projectId: string, data: Partial<MaterialRequest>) =>
  apiClient.post<ApiResponse<MaterialRequest>>(`/projects/${projectId}/material-requests`, data);

export const updateMaterialRequest = (projectId: string, requestId: string, data: Partial<MaterialRequest>) =>
  apiClient.put<ApiResponse<MaterialRequest>>(`/projects/${projectId}/material-requests/${requestId}`, data);

export const submitMaterialRequest = (projectId: string, requestId: string) =>
  apiClient.post<ApiResponse<MaterialRequest>>(`/projects/${projectId}/material-requests/${requestId}/submit`, {});

export const approveMaterialRequest = (projectId: string, requestId: string) =>
  apiClient.post<ApiResponse<MaterialRequest>>(`/projects/${projectId}/material-requests/${requestId}/approve`, {});

export const getMaterialRequestItems = (projectId: string, requestId: string) =>
  apiClient.get<ApiResponse<MaterialRequestItem[]>>(`/projects/${projectId}/material-requests/${requestId}/items`);

export const createMaterialRequestItem = (projectId: string, requestId: string, data: Partial<MaterialRequestItem>) =>
  apiClient.post<ApiResponse<MaterialRequestItem>>(`/projects/${projectId}/material-requests/${requestId}/items`, data);

export const updateMaterialRequestItem = (projectId: string, requestId: string, itemId: string, data: Partial<MaterialRequestItem>) =>
  apiClient.put<ApiResponse<MaterialRequestItem>>(`/projects/${projectId}/material-requests/${requestId}/items/${itemId}`, data);

export const deleteMaterialRequestItem = (projectId: string, requestId: string, itemId: string) =>
  apiClient.delete<ApiResponse<null>>(`/projects/${projectId}/material-requests/${requestId}/items/${itemId}`);

export const getExpenses = (projectId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Expense>>>(`/projects/${projectId}/expenses`, { params });

export const getExpenseById = (projectId: string, expenseId: string) =>
  apiClient.get<ApiResponse<Expense>>(`/projects/${projectId}/expenses/${expenseId}`);

export const createExpense = (projectId: string, data: Partial<Expense>) =>
  apiClient.post<ApiResponse<Expense>>(`/projects/${projectId}/expenses`, data);

export const updateExpense = (projectId: string, expenseId: string, data: Partial<Expense>) =>
  apiClient.put<ApiResponse<Expense>>(`/projects/${projectId}/expenses/${expenseId}`, data);

export const approveExpense = (projectId: string, expenseId: string) =>
  apiClient.post<ApiResponse<Expense>>(`/projects/${projectId}/expenses/${expenseId}/approve`, {});

export const getClients = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Client>>>("/clients", { params });

export const getClientById = (id: string) =>
  apiClient.get<ApiResponse<Client>>(`/clients/${id}`);

export const createClient = (data: Partial<Client>) =>
  apiClient.post<ApiResponse<Client>>("/clients", data);

export const updateClient = (id: string, data: Partial<Client>) =>
  apiClient.put<ApiResponse<Client>>(`/clients/${id}`, data);

export const deleteClient = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/clients/${id}`);

// ─── Civil Engineering Interfaces ────────────────────────────────────────────

export interface Drawing {
  id: string;
  projectId: string;
  drawingNumber: string;
  title: string;
  revision: string;
  discipline: string;
  status: string;
  drawnById?: string;
  drawnByName?: string;
  checkedById?: string;
  checkedByName?: string;
  approvedById?: string;
  approvedByName?: string;
  issuedDate?: string;
  filePath?: string;
  notes?: string;
  createdAt: string;
}

export interface RFI {
  id: string;
  projectId: string;
  rfiNumber: string;
  subject: string;
  description: string;
  status: string;
  raisedById: string;
  raisedByName: string;
  raisedDate: string;
  assignedToId?: string;
  assignedToName?: string;
  responseDueDate?: string;
  responseDate?: string;
  response?: string;
  isUrgent: boolean;
  createdAt: string;
}

export interface Submittal {
  id: string;
  projectId: string;
  submittalNumber: string;
  title: string;
  status: string;
  contractorReference?: string;
  submittedById: string;
  submittedByName: string;
  submittedDate: string;
  reviewedById?: string;
  reviewedByName?: string;
  reviewDate?: string;
  reviewComments?: string;
  filePath?: string;
  createdAt: string;
}

export interface ResourceAllocation {
  id: string;
  projectId: string;
  resourceType: string;
  description: string;
  status: string;
  employeeId?: string;
  employeeName?: string;
  equipmentCode?: string;
  unitOfMeasure?: string;
  plannedStartDate: string;
  plannedEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  plannedQuantity: number;
  actualQuantity?: number;
  unitCost: number;
  plannedCost: number;
  actualCost?: number;
  createdAt: string;
}

export interface QAChecklistItem {
  id: string;
  checklistId: string;
  sequence: number;
  checkpointDescription: string;
  inspectionType: string;
  result?: string;
  remarks?: string;
  isRequired: boolean;
}

export interface QAChecklist {
  id: string;
  projectId: string;
  checklistNumber: string;
  title: string;
  status: string;
  inspectionDate?: string;
  location?: string;
  inspectedById?: string;
  inspectedByName?: string;
  approvedById?: string;
  approvedByName?: string;
  overallResult?: string;
  remarks?: string;
  items: QAChecklistItem[];
  createdAt: string;
}

export interface NCR {
  id: string;
  projectId: string;
  ncrNumber: string;
  title: string;
  description: string;
  category: string;
  severity: string;
  status: string;
  raisedById: string;
  raisedByName: string;
  raisedDate: string;
  responsibleParty?: string;
  correctiveAction?: string;
  verifiedById?: string;
  verifiedByName?: string;
  verifiedDate?: string;
  closedDate?: string;
  createdAt: string;
}

export interface SafetyIncident {
  id: string;
  projectId: string;
  incidentNumber: string;
  incidentDate: string;
  severity: string;
  status: string;
  location: string;
  description: string;
  injuredPersonName?: string;
  injuryType?: string;
  rootCause?: string;
  correctiveAction?: string;
  preventiveAction?: string;
  investigatedById?: string;
  investigatedByName?: string;
  investigationDate?: string;
  closedDate?: string;
  createdAt: string;
}

export interface RABillItem {
  id: string;
  raBillId: string;
  boqItemId?: string;
  boqItemDescription?: string;
  uom: string;
  previousQuantity: number;
  currentQuantity: number;
  totalQuantity: number;
  rate: number;
  amount: number;
}

export interface RABill {
  id: string;
  projectId: string;
  billNumber: string;
  billSequence: number;
  billDate: string;
  status: string;
  grossAmount: number;
  deductionAmount: number;
  retentionPercent: number;
  retentionAmount: number;
  netAmount: number;
  previousBilledAmount: number;
  cumulativeBilledAmount: number;
  certifiedById?: string;
  certifiedByName?: string;
  certifiedDate?: string;
  certificationNotes?: string;
  paidDate?: string;
  items: RABillItem[];
  createdAt: string;
}

// ─── Drawings ────────────────────────────────────────────────────────────────

export const getDrawings = (params: PaginationRequest, projectId?: string, discipline?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<Drawing>>>("/drawings", { params: { ...params, projectId, discipline, status } });

export const getDrawingById = (id: string) =>
  apiClient.get<ApiResponse<Drawing>>(`/drawings/${id}`);

export const createDrawing = (data: Partial<Drawing>) =>
  apiClient.post<ApiResponse<Drawing>>("/drawings", data);

export const updateDrawing = (id: string, data: Partial<Drawing>) =>
  apiClient.put<ApiResponse<Drawing>>(`/drawings/${id}`, data);

export const deleteDrawing = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/drawings/${id}`);

// ─── RFIs ─────────────────────────────────────────────────────────────────────

export const getRFIs = (params: PaginationRequest, projectId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<RFI>>>("/rfis", { params: { ...params, projectId, status } });

export const getRFIById = (id: string) =>
  apiClient.get<ApiResponse<RFI>>(`/rfis/${id}`);

export const createRFI = (data: Partial<RFI>) =>
  apiClient.post<ApiResponse<RFI>>("/rfis", data);

export const updateRFI = (id: string, data: Partial<RFI>) =>
  apiClient.put<ApiResponse<RFI>>(`/rfis/${id}`, data);

export const respondRFI = (id: string, data: { response: string; responseDate: string }) =>
  apiClient.post<ApiResponse<RFI>>(`/rfis/${id}/respond`, data);

export const closeRFI = (id: string) =>
  apiClient.post<ApiResponse<RFI>>(`/rfis/${id}/close`, {});

export const deleteRFI = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/rfis/${id}`);

// ─── Submittals ───────────────────────────────────────────────────────────────

export const getSubmittals = (params: PaginationRequest, projectId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<Submittal>>>("/submittals", { params: { ...params, projectId, status } });

export const getSubmittalById = (id: string) =>
  apiClient.get<ApiResponse<Submittal>>(`/submittals/${id}`);

export const createSubmittal = (data: Partial<Submittal>) =>
  apiClient.post<ApiResponse<Submittal>>("/submittals", data);

export const updateSubmittal = (id: string, data: Partial<Submittal>) =>
  apiClient.put<ApiResponse<Submittal>>(`/submittals/${id}`, data);

export const reviewSubmittal = (id: string, data: { status: string; reviewComments: string; reviewDate: string }) =>
  apiClient.post<ApiResponse<Submittal>>(`/submittals/${id}/review`, data);

export const deleteSubmittal = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/submittals/${id}`);

// ─── Resource Allocations ─────────────────────────────────────────────────────

export const getResourceAllocations = (params: PaginationRequest, projectId?: string, resourceType?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<ResourceAllocation>>>("/resourceallocations", { params: { ...params, projectId, resourceType, status } });

export const getResourceAllocationById = (id: string) =>
  apiClient.get<ApiResponse<ResourceAllocation>>(`/resourceallocations/${id}`);

export const createResourceAllocation = (data: Partial<ResourceAllocation>) =>
  apiClient.post<ApiResponse<ResourceAllocation>>("/resourceallocations", data);

export const updateResourceAllocation = (id: string, data: Partial<ResourceAllocation>) =>
  apiClient.put<ApiResponse<ResourceAllocation>>(`/resourceallocations/${id}`, data);

export const deleteResourceAllocation = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/resourceallocations/${id}`);

// ─── QA Checklists ────────────────────────────────────────────────────────────

export const getQAChecklists = (params: PaginationRequest, projectId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<QAChecklist>>>("/qachecklists", { params: { ...params, projectId, status } });

export const getQAChecklistById = (id: string) =>
  apiClient.get<ApiResponse<QAChecklist>>(`/qachecklists/${id}`);

export const createQAChecklist = (data: object) =>
  apiClient.post<ApiResponse<QAChecklist>>("/qachecklists", data);

export const updateQAChecklist = (id: string, data: Partial<QAChecklist>) =>
  apiClient.put<ApiResponse<QAChecklist>>(`/qachecklists/${id}`, data);

export const submitQAChecklist = (id: string, data: object) =>
  apiClient.post<ApiResponse<QAChecklist>>(`/qachecklists/${id}/submit`, data);

export const deleteQAChecklist = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/qachecklists/${id}`);

// ─── NCRs ─────────────────────────────────────────────────────────────────────

export const getNCRs = (params: PaginationRequest, projectId?: string, status?: string, severity?: string) =>
  apiClient.get<ApiResponse<PagedResult<NCR>>>("/ncrs", { params: { ...params, projectId, status, severity } });

export const getNCRById = (id: string) =>
  apiClient.get<ApiResponse<NCR>>(`/ncrs/${id}`);

export const createNCR = (data: Partial<NCR>) =>
  apiClient.post<ApiResponse<NCR>>("/ncrs", data);

export const updateNCR = (id: string, data: Partial<NCR>) =>
  apiClient.put<ApiResponse<NCR>>(`/ncrs/${id}`, data);

export const verifyNCR = (id: string, data: { verifiedDate: string; notes?: string }) =>
  apiClient.post<ApiResponse<NCR>>(`/ncrs/${id}/verify`, data);

export const closeNCR = (id: string) =>
  apiClient.post<ApiResponse<NCR>>(`/ncrs/${id}/close`, {});

export const voidNCR = (id: string) =>
  apiClient.post<ApiResponse<NCR>>(`/ncrs/${id}/void`, {});

export const deleteNCR = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/ncrs/${id}`);

// ─── Safety Incidents ─────────────────────────────────────────────────────────

export const getSafetyIncidents = (params: PaginationRequest, projectId?: string, severity?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<SafetyIncident>>>("/safetyincidents", { params: { ...params, projectId, severity, status } });

export const getSafetyIncidentById = (id: string) =>
  apiClient.get<ApiResponse<SafetyIncident>>(`/safetyincidents/${id}`);

export const createSafetyIncident = (data: Partial<SafetyIncident>) =>
  apiClient.post<ApiResponse<SafetyIncident>>("/safetyincidents", data);

export const updateSafetyIncident = (id: string, data: Partial<SafetyIncident>) =>
  apiClient.put<ApiResponse<SafetyIncident>>(`/safetyincidents/${id}`, data);

export const deleteSafetyIncident = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/safetyincidents/${id}`);

// ─── RA Bills ─────────────────────────────────────────────────────────────────

export const getRABills = (params: PaginationRequest, projectId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<RABill>>>("/rabills", { params: { ...params, projectId, status } });

export const getRABillById = (id: string) =>
  apiClient.get<ApiResponse<RABill>>(`/rabills/${id}`);

export const createRABill = (data: object) =>
  apiClient.post<ApiResponse<RABill>>("/rabills", data);

export const updateRABill = (id: string, data: Partial<RABill>) =>
  apiClient.put<ApiResponse<RABill>>(`/rabills/${id}`, data);

export const submitRABill = (id: string) =>
  apiClient.post<ApiResponse<RABill>>(`/rabills/${id}/submit`, {});

export const certifyRABill = (id: string, data: { certifiedAmount: number; certificationNotes?: string }) =>
  apiClient.post<ApiResponse<RABill>>(`/rabills/${id}/certify`, data);

export const markRABillPaid = (id: string) =>
  apiClient.post<ApiResponse<RABill>>(`/rabills/${id}/mark-paid`, {});

export const deleteRABill = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`/rabills/${id}`);