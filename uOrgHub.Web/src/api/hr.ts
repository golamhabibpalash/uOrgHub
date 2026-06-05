import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

export interface Employee {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  departmentId: string;
  departmentName: string;
  designationId: string;
  designationName: string;
  employmentType: string;
  status: string;
  joiningDate: string;
  basicSalary: number;
  managerId?: string;
  managerName?: string;
}

export interface Department {
  id: string;
  name: string;
  code: string;
  description: string;
  isActive: boolean;
  parentDepartmentId?: string;
  parentDepartmentName?: string;
}

export interface Designation {
  id: string;
  name: string;
  code: string;
  level: number;
  isActive: boolean;
  departmentId: string;
  departmentName: string;
  parentDesignationId?: string;
  parentDesignationName?: string;
  salaryGradeId?: string;
}

export interface LeaveType {
  id: string;
  name: string;
  code: string;
  description: string;
  totalDaysPerYear: number;
  isPaid: boolean;
  isActive: boolean;
}

export interface LeaveRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  status: string;
  approverId?: string;
  approverName?: string;
  createdAt: string;
}

export interface LeaveBalance {
  employeeId: string;
  leaveTypeId: string;
  leaveTypeName: string;
  totalDays: number;
  usedDays: number;
  balanceDays: number;
  year: number;
}

export interface WorkSchedule {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
}

export interface Shift {
  id: string;
  name: string;
  startTime: string;
  endTime: string;
  workScheduleId: string;
  workScheduleName: string;
  isActive: boolean;
}

export interface AttendanceLog {
  id: string;
  employeeId: string;
  employeeName: string;
  attendanceDate: string;
  checkIn: string;
  checkOut: string;
  workHours: number;
  status: string;
  remarks?: string;
}

export interface SalaryGrade {
  id: string;
  name: string;
  gradeCode: string;
  description: string;
  minSalary: number;
  maxSalary: number;
  isActive: boolean;
}

export interface SalaryComponent {
  id: string;
  name: string;
  code: string;
  componentType: string;
  isTaxable: boolean;
  isActive: boolean;
}

export interface PayrollCycle {
  id: string;
  title: string;
  year: number;
  month: number;
  startDate: string;
  endDate: string;
  paymentDate?: string;
  status: string;
}

export interface ExpenseRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  amount: number;
  category: string;
  description: string;
  status: string;
  submittedAt: string;
}

export interface JobPosting {
  id: string;
  title: string;
  departmentId: string;
  departmentName: string;
  designationId: string;
  designationName: string;
  description: string;
  requirements: string;
  location: string;
  status: string;
  postedDate: string;
  closingDate: string;
}

export interface Candidate {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  resumeUrl?: string;
  status: string;
  source: string;
}

export interface JobApplication {
  id: string;
  candidateId: string;
  candidateName: string;
  jobPostingId: string;
  jobTitle: string;
  status: string;
  appliedAt: string;
}

export interface InterviewSchedule {
  id: string;
  applicationId: string;
  candidateName: string;
  jobTitle: string;
  scheduledAt: string;
  location: string;
  status: string;
  notes?: string;
}

export interface OnboardingChecklist {
  id: string;
  name: string;
  description: string;
  items: string[];
}

export interface EmployeeOnboarding {
  id: string;
  employeeId: string;
  employeeName: string;
  checklistId: string;
  checklistName: string;
  startDate: string;
  status: string;
}

export interface ReviewCycle {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: string;
}

export interface Goal {
  id: string;
  employeeId: string;
  employeeName: string;
  title: string;
  description: string;
  targetDate: string;
  progress: number;
  status: string;
}

export interface PerformanceReview {
  id: string;
  employeeId: string;
  employeeName: string;
  reviewerId: string;
  reviewerName: string;
  reviewCycleId: string;
  reviewCycleName: string;
  rating: number;
  comments: string;
  status: string;
  submittedAt: string;
}

export interface TrainingProgram {
  id: string;
  title: string;
  description: string;
  provider: string;
  startDate: string;
  endDate: string;
  maxParticipants: number;
  isActive: boolean;
}

export interface EmployeeTraining {
  id: string;
  employeeId: string;
  employeeName: string;
  trainingProgramId: string;
  trainingTitle: string;
  status: string;
  completionDate?: string;
}

export interface Asset {
  id: string;
  name: string;
  type: string;
  serialNumber: string;
  assignedToId?: string;
  assignedToName?: string;
  status: string;
  purchaseDate: string;
}

// Employees
export const getEmployees = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Employee>>>("employees", { params });

export const getEmployeeById = (id: string) =>
  apiClient.get<ApiResponse<Employee>>(`employees/${id}`);

export const createEmployee = (data: Partial<Employee>) =>
  apiClient.post<ApiResponse<Employee>>("employees", data);

export const createEmployeeWithUser = (data: {
  employee: Partial<Employee>;
  createUserAccount: boolean;
  userAccount?: {
    username: string;
    email?: string;
    password: string;
    autoGeneratePassword: boolean;
    isActive: boolean;
    roleIds: string[];
  };
}) => apiClient.post<ApiResponse<Employee>>("employees/with-user", data);

export const updateEmployee = (id: string, data: Partial<Employee>) =>
  apiClient.put<ApiResponse<Employee>>(`employees/${id}`, data);

export const deleteEmployee = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`employees/${id}`);

export interface EmployeeDependencies {
  employeeId: string;
  hasUserAccount: boolean;
  leaveRequests: number;
  attendanceLogs: number;
  payrollEntries: number;
  assetAssignments: number;
  trainingEnrollments: number;
  expenseRequests: number;
  directReports: number;
  canDelete: boolean;
  blockingReason?: string;
}

export const getEmployeeDependencies = (id: string) =>
  apiClient.get<ApiResponse<EmployeeDependencies>>(`employees/${id}/dependencies`);

// Departments
export const getDepartments = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Department>>>("departments", {
    params,
  });

export const getAllDepartments = () =>
  apiClient.get<ApiResponse<Department[]>>("departments/all");

export const getDepartmentById = (id: string) =>
  apiClient.get<ApiResponse<Department>>(`departments/${id}`);

export const createDepartment = (data: Partial<Department>) =>
  apiClient.post<ApiResponse<Department>>("departments", data);

export const updateDepartment = (id: string, data: Partial<Department>) =>
  apiClient.put<ApiResponse<Department>>(`departments/${id}`, data);

export const deleteDepartment = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`departments/${id}`);

export interface DepartmentDependencies {
  departmentId: string;
  activeEmployees: number;
  activeDesignations: number;
  activeChildDepartments: number;
  activeJobPostings: number;
  activeKpis: number;
  canDelete: boolean;
  blockingReason?: string;
}

export const getDepartmentDependencies = (id: string) =>
  apiClient.get<ApiResponse<DepartmentDependencies>>(`departments/${id}/dependencies`);

export interface DesignationDependencies {
  designationId: string;
  employeeCount: number;
  childDesignationCount: number;
  canDelete: boolean;
  blockingReason?: string;
}

export const getDesignationDependencies = (id: string) =>
  apiClient.get<ApiResponse<DesignationDependencies>>(`designations/${id}/dependencies`);

// Designations
export const getDesignations = (params: PaginationRequest, departmentId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Designation>>>("designations", {
    params: { ...params, departmentId },
  });

export const getAllDesignations = () =>
  apiClient.get<ApiResponse<Designation[]>>("designations/all");

export const getDesignationById = (id: string) =>
  apiClient.get<ApiResponse<Designation>>(`designations/${id}`);

export const createDesignation = (data: Partial<Designation>) =>
  apiClient.post<ApiResponse<Designation>>("designations", data);

export const updateDesignation = (id: string, data: Partial<Designation>) =>
  apiClient.put<ApiResponse<Designation>>(`designations/${id}`, data);

export const deleteDesignation = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`designations/${id}`);

// Leave Management
export const getLeaveTypes = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<LeaveType>>>("leave/types", { params });

export const createLeaveType = (data: Partial<LeaveType>) =>
  apiClient.post<ApiResponse<LeaveType>>("leave/types", data);

export const updateLeaveType = (id: string, data: Partial<LeaveType>) =>
  apiClient.put<ApiResponse<LeaveType>>(`leave/types/${id}`, data);

export const getLeaveRequests = (params: PaginationRequest, employeeId?: string, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<LeaveRequest>>>("leave/requests", {
    params: { ...params, employeeId, status },
  });

export const createLeaveRequest = (data: Partial<LeaveRequest>) =>
  apiClient.post<ApiResponse<LeaveRequest>>("leave/requests", data);

export const approveLeaveRequest = (id: string, data: { isApproved: boolean; remarks?: string }) =>
  apiClient.post<ApiResponse<LeaveRequest>>("leave/requests/approve", { ...data, leaveRequestId: id });

export const cancelLeaveRequest = (id: string) =>
  apiClient.put<ApiResponse<null>>(`leave/requests/${id}/cancel`, {});

export const getLeaveBalances = (employeeId: string, year?: number) =>
  apiClient.get<ApiResponse<LeaveBalance[]>>(`leave/balances/${employeeId}`, {
    params: { year },
  });

// Attendance
export const getWorkSchedules = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<WorkSchedule>>>("attendance/work-schedules", { params });

export const createWorkSchedule = (data: Partial<WorkSchedule>) =>
  apiClient.post<ApiResponse<WorkSchedule>>("attendance/work-schedules", data);

export const getShifts = (params: PaginationRequest, workScheduleId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Shift>>>("attendance/shifts", {
    params: { ...params, workScheduleId },
  });

export const createShift = (data: Partial<Shift>) =>
  apiClient.post<ApiResponse<Shift>>("attendance/shifts", data);

export const getAttendanceLogs = (params: PaginationRequest, employeeId?: string, fromDate?: string, toDate?: string) =>
  apiClient.get<ApiResponse<PagedResult<AttendanceLog>>>("attendance/logs", {
    params: { ...params, employeeId, fromDate, toDate },
  });

export const createAttendanceLog = (data: Partial<AttendanceLog>) =>
  apiClient.post<ApiResponse<AttendanceLog>>("attendance/logs", data);

export const updateAttendanceLog = (id: string, data: Partial<AttendanceLog>) =>
  apiClient.put<ApiResponse<AttendanceLog>>(`attendance/logs/${id}`, data);

// Payroll
export const getSalaryGrades = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<SalaryGrade>>>("payroll/salary-grades", { params });

export const createSalaryGrade = (data: Partial<SalaryGrade>) =>
  apiClient.post<ApiResponse<SalaryGrade>>("payroll/salary-grades", data);

export const getSalaryComponents = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<SalaryComponent>>>("payroll/salary-components", { params });

export const createSalaryComponent = (data: Partial<SalaryComponent>) =>
  apiClient.post<ApiResponse<SalaryComponent>>("payroll/salary-components", data);

export const getPayrollCycles = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<PayrollCycle>>>("payroll/cycles", { params });

export const createPayrollCycle = (data: Partial<PayrollCycle>) =>
  apiClient.post<ApiResponse<PayrollCycle>>("payroll/cycles", data);

export const getExpenses = (params: PaginationRequest, employeeId?: string) =>
  apiClient.get<ApiResponse<PagedResult<ExpenseRequest>>>("payroll/expenses", {
    params: { ...params, employeeId },
  });

export const createExpense = (data: Partial<ExpenseRequest>) =>
  apiClient.post<ApiResponse<ExpenseRequest>>("payroll/expenses", data);

export const approveExpense = (id: string, data: { isApproved: boolean; remarks?: string }) =>
  apiClient.put<ApiResponse<ExpenseRequest>>(`payroll/expenses/${id}/approve`, data);

// Recruitment
export const getJobPostings = (params: PaginationRequest, status?: string) =>
  apiClient.get<ApiResponse<PagedResult<JobPosting>>>("recruitment/job-postings", {
    params: { ...params, status },
  });

export const createJobPosting = (data: Partial<JobPosting>) =>
  apiClient.post<ApiResponse<JobPosting>>("recruitment/job-postings", data);

export const updateJobPosting = (id: string, data: Partial<JobPosting>) =>
  apiClient.put<ApiResponse<JobPosting>>(`recruitment/job-postings/${id}`, data);

export const getCandidates = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<Candidate>>>("recruitment/candidates", { params });

export const createCandidate = (data: Partial<Candidate>) =>
  apiClient.post<ApiResponse<Candidate>>("recruitment/candidates", data);

export const getApplications = (params: PaginationRequest, jobPostingId?: string) =>
  apiClient.get<ApiResponse<PagedResult<JobApplication>>>("recruitment/applications", {
    params: { ...params, jobPostingId },
  });

export const createApplication = (data: Partial<JobApplication>) =>
  apiClient.post<ApiResponse<JobApplication>>("recruitment/applications", data);

export const updateApplication = (id: string, data: Partial<JobApplication>) =>
  apiClient.put<ApiResponse<JobApplication>>(`recruitment/applications/${id}`, data);

export const getInterviews = (params: PaginationRequest, jobApplicationId?: string) =>
  apiClient.get<ApiResponse<PagedResult<InterviewSchedule>>>("recruitment/interviews", {
    params: { ...params, jobApplicationId },
  });

export const createInterview = (data: Partial<InterviewSchedule>) =>
  apiClient.post<ApiResponse<InterviewSchedule>>("recruitment/interviews", data);

export const getOnboardingChecklists = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<OnboardingChecklist>>>("recruitment/onboarding-checklists", { params });

export const createOnboardingChecklist = (data: Partial<OnboardingChecklist>) =>
  apiClient.post<ApiResponse<OnboardingChecklist>>("recruitment/onboarding-checklists", data);

// Performance
export const getReviewCycles = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<ReviewCycle>>>("performance/review-cycles", { params });

export const createReviewCycle = (data: Partial<ReviewCycle>) =>
  apiClient.post<ApiResponse<ReviewCycle>>("performance/review-cycles", data);

export const updateReviewCycle = (id: string, data: Partial<ReviewCycle>) =>
  apiClient.put<ApiResponse<ReviewCycle>>(`performance/review-cycles/${id}`, data);

export const getGoals = (params: PaginationRequest, employeeId?: string, reviewCycleId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Goal>>>("performance/goals", {
    params: { ...params, employeeId, reviewCycleId },
  });

export const createGoal = (data: Partial<Goal>) =>
  apiClient.post<ApiResponse<Goal>>("performance/goals", data);

export const updateGoal = (id: string, data: Partial<Goal>) =>
  apiClient.put<ApiResponse<Goal>>(`performance/goals/${id}`, data);

export const getPerformanceReviews = (params: PaginationRequest, employeeId?: string, reviewCycleId?: string) =>
  apiClient.get<ApiResponse<PagedResult<PerformanceReview>>>("performance/reviews", {
    params: { ...params, employeeId, reviewCycleId },
  });

export const createPerformanceReview = (data: Partial<PerformanceReview>) =>
  apiClient.post<ApiResponse<PerformanceReview>>("performance/reviews", data);

export const submitPerformanceReview = (id: string, data: { rating: number; comments: string }) =>
  apiClient.put<ApiResponse<PerformanceReview>>(`performance/reviews/${id}/submit`, data);

export const getTrainingPrograms = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<TrainingProgram>>>("performance/training-programs", { params });

export const createTrainingProgram = (data: Partial<TrainingProgram>) =>
  apiClient.post<ApiResponse<TrainingProgram>>("performance/training-programs", data);

export const getEmployeeTrainings = (params: PaginationRequest, employeeId?: string) =>
  apiClient.get<ApiResponse<PagedResult<EmployeeTraining>>>("performance/employee-trainings", {
    params: { ...params, employeeId },
  });

export const createEmployeeTraining = (data: Partial<EmployeeTraining>) =>
  apiClient.post<ApiResponse<EmployeeTraining>>("performance/employee-trainings", data);

export const updateEmployeeTraining = (id: string, data: Partial<EmployeeTraining>) =>
  apiClient.put<ApiResponse<EmployeeTraining>>(`performance/employee-trainings/${id}`, data);

export interface HRDashboardData {
  totalEmployees: number;
  totalDepartments: number;
  openPositions: number;
  pendingLeaveRequests: number;
  activePayrollCycles: number;
  activeTrainings: number;
  newEmployeesThisMonth: number;
  leaveRequestsThisMonth: number;
  activeJobPostings: number;
  attendanceRate: number;
  employeesPerDepartment: { departmentName: string; employeeCount: number }[];
  recentActivities: { description: string; timestamp: string }[];
}

export const getHRDashboard = () =>
  apiClient.get<ApiResponse<HRDashboardData>>("hr/dashboard").then((r) => r.data.data!);