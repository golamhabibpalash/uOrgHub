import apiClient from "./client";
import { ApiResponse, PagedResult, PaginationRequest } from "../types/api";

export interface OrganogramNode {
  id: string;
  employeeCode: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  fullName: string;
  designationName: string;
  departmentName: string;
  profilePicturePath?: string;
  profilePictureUrl?: string;
  status: string;
  managerName?: string;
  depth: number;
  hasChildren: boolean;
  children: OrganogramNode[];
}

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
  profilePicturePath?: string;
  profilePictureUrl?: string;
  nidPhotoPath?: string;
  nidPhotoUrl?: string;
  // Extended fields — populated by the detail endpoint (GET /employees/{id})
  middleName?: string;
  fullName?: string;
  gender?: string;
  religion?: string;
  maritalStatus?: string;
  bloodGroup?: string;
  dateOfBirth?: string;
  nationalId?: string;
  passportNo?: string;
  passportExpiry?: string;
  nationality?: string;
  permanentAddress?: string;
  currentAddress?: string;
  district?: string;
  division?: string;
  upazila?: string;
  confirmationDate?: string;
  salaryGradeId?: string;
  createdAt?: string;
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
  description?: string;
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
  isPaidLeave: boolean;
  approvalLevels: number;
  carryForward: boolean;
  maxCarryForwardDays: number;
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
  rejectionReason?: string;
  rejectedBy?: string;
  rejectedAt?: string;
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

// Times of day (startTime/endTime) travel as .NET TimeSpan strings: "HH:mm:ss".
export interface WorkSchedule {
  id: string;
  name: string;
  description: string;
  startTime: string;
  endTime: string;
  totalHours: number;
  isFlexible: boolean;
  gracePeriodMinutes: number;
  workingDaysPerWeek: number;
  isActive: boolean;
}

export interface Shift {
  id: string;
  name: string;
  code: string;
  startTime: string;
  endTime: string;
  isNightShift: boolean;
  workScheduleId: string;
  workScheduleName: string;
  isActive: boolean;
}

export interface EmployeeRoster {
  id: string;
  employeeId: string;
  employeeName: string;
  shiftId: string;
  shiftName: string;
  shiftStartTime: string;
  shiftEndTime: string;
  rosterDate: string;
  isOff: boolean;
  note?: string;
}

/** Backend AttendanceStatus enum, in display order. */
export const ATTENDANCE_STATUSES = [
  { value: "Present", label: "Present" },
  { value: "Late", label: "Late" },
  { value: "HalfDay", label: "Half Day" },
  { value: "Absent", label: "Absent" },
  { value: "WFH", label: "Work From Home" },
  { value: "LeaveApproved", label: "On Leave" },
  { value: "Holiday", label: "Holiday" },
  { value: "WeekOff", label: "Week Off" },
] as const;

export interface AttendanceLog {
  id: string;
  employeeId: string;
  employeeName: string;
  attendanceDate: string;
  checkIn: string | null;
  checkOut: string | null;
  workHours: number;
  overtimeHours: number;
  /** Shift the employee was rostered on that day, if any. */
  shiftName?: string | null;
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
  /** "Fixed" | "PercentageOfBasic" | "PercentageOfGross" */
  calculationType: string;
  /** Suggested value when assigning it to an employee: an amount, or a % for percentage types. */
  defaultValue: number;
  isTaxable: boolean;
  isFixed?: boolean;
  isActive: boolean;
  sortOrder: number;
  description?: string;
}

/** Component types that are taken off pay; the rest are allowances. Mirrors PayrollCalculator.IsDeduction. */
export const DEDUCTION_COMPONENT_TYPES = ["PF", "Tax", "Loan", "Advance", "OtherDeduction"];

export interface SalaryStructureComponent {
  salaryComponentId: string;
  code: string;
  name: string;
  componentType: string;
  calculationType: string;
  value: number;
  amount: number;
  isDeduction: boolean;
}

export interface SalaryStructure {
  id: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  designationName?: string;
  salaryGradeId: string;
  salaryGradeName: string;
  basicSalary: number;
  totalAllowances: number;
  grossSalary: number;
  totalDeductions: number;
  netSalary: number;
  effectiveDate: string;
  endDate?: string;
  isActive: boolean;
  components: SalaryStructureComponent[];
}

export interface SalaryStructureInput {
  salaryGradeId: string;
  basicSalary: number;
  components: { salaryComponentId: string; value: number }[];
}

export interface PayrollCycle {
  id: string;
  title: string;
  year: number;
  month: number;
  startDate: string;
  endDate: string;
  processedDate?: string;
  /** "Draft" | "Processing" | "Processed" | "Approved" | "Paid" | "Cancelled" */
  status: string;
  totalBasic: number;
  totalAllowances: number;
  totalDeductions: number;
  totalNetPay: number;
  totalEmployees: number;
  remarks?: string;
}

export interface PayrollEntry {
  id: string;
  payrollCycleId: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  grossSalary: number;
  basicSalary: number;
  totalAllowances: number;
  totalDeductions: number;
  taxAmount: number;
  netSalary: number;
  overtimePay: number;
  bonusAmount: number;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  leaveDays: number;
  overtimeHours: number;
  status: string;
}

export interface Payslip extends PayrollEntry {
  cycleTitle: string;
  periodStart: string;
  periodEnd: string;
  designationName?: string;
  departmentName?: string;
  joiningDate: string;
  lines: { code: string; name: string; lineType: "Earning" | "Deduction"; amount: number }[];
}

export interface ProcessPayrollResult {
  cycle: PayrollCycle;
  processedEmployees: number;
  skipped: string[];
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
  jobCode: string;
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
export const getEmployees = (
  params: PaginationRequest,
  filters: { departmentId?: string; designationId?: string; status?: string } = {},
) =>
  apiClient.get<ApiResponse<PagedResult<Employee>>>("employees", { params: { ...params, ...filters } });

export const getEmployeeById = (id: string) =>
  apiClient.get<ApiResponse<Employee>>(`employees/${id}`);

export const getAllEmployees = () =>
  apiClient.get<ApiResponse<Employee[]>>("employees/all");

export const getOrganogram = (params?: { departmentId?: string; designationId?: string }) =>
  apiClient.get<ApiResponse<OrganogramNode[]>>("employees/organogram", { params });

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

export const uploadEmployeeProfilePicture = (id: string, file: File) => {
  const fd = new FormData();
  fd.append("file", file);
  return apiClient
    .post<ApiResponse<string>>(`employees/${id}/profile-picture`, fd, {
      headers: { "Content-Type": "multipart/form-data" },
    })
    .then((r) => r.data.data ?? "");
};

export const deleteEmployeeProfilePicture = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`employees/${id}/profile-picture`);

export const uploadEmployeeNidPhoto = (id: string, file: File) => {
  const fd = new FormData();
  fd.append("file", file);
  return apiClient
    .post<ApiResponse<string>>(`employees/${id}/nid-photo`, fd, {
      headers: { "Content-Type": "multipart/form-data" },
    })
    .then((r) => r.data.data ?? "");
};

export const deleteEmployeeNidPhoto = (id: string) =>
  apiClient.delete<ApiResponse<string>>(`employees/${id}/nid-photo`);

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

export const getActiveLeaveTypes = () =>
  apiClient.get<ApiResponse<LeaveType[]>>("leave/types/active");

export const createLeaveType = (data: Partial<LeaveType>) =>
  apiClient.post<ApiResponse<LeaveType>>("leave/types", data);

export const updateLeaveType = (id: string, data: Partial<LeaveType>) =>
  apiClient.put<ApiResponse<LeaveType>>(`leave/types/${id}`, data);

export const getLeaveRequests = (
  params: PaginationRequest,
  employeeId?: string,
  status?: string,
  leaveTypeId?: string,
  fromDate?: string,
  toDate?: string,
) =>
  apiClient.get<ApiResponse<PagedResult<LeaveRequest>>>("leave/requests", {
    params: { ...params, employeeId, status, leaveTypeId, fromDate, toDate },
  });

export const createLeaveRequest = (data: Partial<LeaveRequest>) =>
  apiClient.post<ApiResponse<LeaveRequest>>("leave/requests", data);

export const approveLeaveRequest = (id: string, data: { isApproved: boolean; remarks?: string; rejectReason?: string }) =>
  apiClient.post<ApiResponse<LeaveRequest>>("leave/requests/approve", { ...data, leaveRequestId: id });

export const updateLeaveRequest = (id: string, data: { leaveTypeId: string; startDate: string; endDate: string; reason: string }) =>
  apiClient.put<ApiResponse<LeaveRequest>>(`leave/requests/${id}`, data);

export const deleteLeaveRequest = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`leave/requests/${id}`);

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

export const updateWorkSchedule = (id: string, data: Partial<WorkSchedule>) =>
  apiClient.put<ApiResponse<WorkSchedule>>(`attendance/work-schedules/${id}`, data);

export const getShifts = (params: PaginationRequest, workScheduleId?: string) =>
  apiClient.get<ApiResponse<PagedResult<Shift>>>("attendance/shifts", {
    params: { ...params, workScheduleId },
  });

export const createShift = (data: Partial<Shift>) =>
  apiClient.post<ApiResponse<Shift>>("attendance/shifts", data);

export const updateShift = (id: string, data: Partial<Shift>) =>
  apiClient.put<ApiResponse<Shift>>(`attendance/shifts/${id}`, data);

export const getRosters = (params: PaginationRequest, filters?: { employeeId?: string; shiftId?: string; fromDate?: string; toDate?: string }) =>
  apiClient.get<ApiResponse<PagedResult<EmployeeRoster>>>("attendance/rosters", {
    params: { ...params, ...filters },
  });

export const createRoster = (data: { employeeId: string; shiftId: string; rosterDate: string; isOff: boolean; note?: string }) =>
  apiClient.post<ApiResponse<EmployeeRoster>>("attendance/rosters", data);

export const updateRoster = (id: string, data: { shiftId: string; isOff: boolean; note?: string }) =>
  apiClient.put<ApiResponse<EmployeeRoster>>(`attendance/rosters/${id}`, data);

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

export const getAllSalaryGrades = () =>
  apiClient.get<ApiResponse<SalaryGrade[]>>("payroll/salary-grades/all");

export const createSalaryGrade = (data: Partial<SalaryGrade>) =>
  apiClient.post<ApiResponse<SalaryGrade>>("payroll/salary-grades", data);

export const updateSalaryGrade = (id: string, data: Partial<SalaryGrade>) =>
  apiClient.put<ApiResponse<SalaryGrade>>(`payroll/salary-grades/${id}`, data);

export const deleteSalaryGrade = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`payroll/salary-grades/${id}`);

export const getSalaryComponents = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<SalaryComponent>>>("payroll/salary-components", { params });

export const createSalaryComponent = (data: Partial<SalaryComponent>) =>
  apiClient.post<ApiResponse<SalaryComponent>>("payroll/salary-components", data);

export const updateSalaryComponent = (id: string, data: Partial<SalaryComponent>) =>
  apiClient.put<ApiResponse<SalaryComponent>>(`payroll/salary-components/${id}`, data);

export const deleteSalaryComponent = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`payroll/salary-components/${id}`);

export const getPayrollCycles = (params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<PayrollCycle>>>("payroll/cycles", { params });

export const createPayrollCycle = (data: { year: number; month: number; title: string; startDate: string; endDate: string }) =>
  apiClient.post<ApiResponse<PayrollCycle>>("payroll/cycles", data);

export const updatePayrollCycle = (id: string, data: { status: string; remarks?: string }) =>
  apiClient.put<ApiResponse<PayrollCycle>>(`payroll/cycles/${id}`, data);

export const processPayrollCycle = (id: string) =>
  apiClient.post<ApiResponse<ProcessPayrollResult>>(`payroll/cycles/${id}/process`);

export const getPayrollEntries = (cycleId: string, params: PaginationRequest) =>
  apiClient.get<ApiResponse<PagedResult<PayrollEntry>>>(`payroll/cycles/${cycleId}/entries`, { params });

export const getPayslip = (cycleId: string, entryId: string) =>
  apiClient.get<ApiResponse<Payslip>>(`payroll/cycles/${cycleId}/entries/${entryId}/payslip`);

export const getSalaryStructures = (params: PaginationRequest, filters?: { employeeId?: string; currentOnly?: boolean }) =>
  apiClient.get<ApiResponse<PagedResult<SalaryStructure>>>("payroll/salary-structures", { params: { ...params, ...filters } });

export const createSalaryStructure = (data: SalaryStructureInput & { employeeId: string; effectiveDate: string }) =>
  apiClient.post<ApiResponse<SalaryStructure>>("payroll/salary-structures", data);

export const updateSalaryStructure = (id: string, data: SalaryStructureInput) =>
  apiClient.put<ApiResponse<SalaryStructure>>(`payroll/salary-structures/${id}`, data);

export const deletePayrollCycle = (id: string) =>
  apiClient.delete<ApiResponse<null>>(`payroll/cycles/${id}`);

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