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