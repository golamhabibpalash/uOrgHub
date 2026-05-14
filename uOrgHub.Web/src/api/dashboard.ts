import apiClient from './client';
import type { ApiResponse } from '../types/api';

export interface PendingApproval {
  id: string;
  type: 'PR' | 'DPR' | 'Leave' | 'Expense' | 'GRN' | 'BOQ';
  title: string;
  module: string;
  amount?: number;
  requestedBy: string;
  requestedAt: string;
  urgency: 'low' | 'medium' | 'high';
  link: string;
}

export interface LowStockAlert {
  id: string;
  itemName: string;
  variantName: string;
  currentStock: number;
  reorderLevel: number;
  unit: string;
  stockPercent: number;
  warehouseName: string;
}

export interface RecentActivity {
  id: string;
  title: string;
  module: string;
  timestamp: string;
  type: 'success' | 'info' | 'warning';
}

export interface ProjectProgress {
  id: string;
  projectCode: string;
  projectName: string;
  completionPercent: number;
  status: string;
  daysRemaining: number;
  contractValue: number;
}

export interface MonthlyExpenseData {
  month: string;
  budget: number;
  actual: number;
}

export interface BudgetUtilization {
  type: string;
  allocated: number;
  spent: number;
  percent: number;
}

export interface UpcomingDeadline {
  id: string;
  deadlineType: string;
  reference: string;
  title: string;
  dueDate: string;
  daysLeft: number;
  status: string;
  link: string;
}

export interface DashboardStats {
  activeProjects: number;
  totalProjectValue: number;
  totalEmployees: number;
  newEmployeesThisMonth: number;
  inventoryItems: number;
  lowStockCount: number;
  openPOs: number;
  openPOValue: number;
  monthlyPayroll: number;
  payrollDueInDays: number;
  pendingApprovalsCount: number;
  pendingApprovalDetails: PendingApproval[];
  lowStockAlerts: LowStockAlert[];
  recentActivities: RecentActivity[];
  projectProgress: ProjectProgress[];
  monthlyExpenseVsBudget: MonthlyExpenseData[];
  budgetUtilization: BudgetUtilization[];
  upcomingDeadlines: UpcomingDeadline[];
}

export const getDashboardStats = () =>
  apiClient.get<ApiResponse<DashboardStats>>('/dashboard/stats');

export const getPendingApprovals = () =>
  apiClient.get<ApiResponse<PendingApproval[]>>('/dashboard/pending-approvals');

export const getLowStockAlerts = () =>
  apiClient.get<ApiResponse<LowStockAlert[]>>('/dashboard/low-stock');

export const getRecentActivities = () =>
  apiClient.get<ApiResponse<RecentActivity[]>>('/dashboard/activities');
