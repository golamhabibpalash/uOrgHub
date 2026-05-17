import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import { getDashboardStats } from '../api/dashboard';
import type { DashboardStats } from '../api/dashboard';

const now = new Date().toISOString();
const h = (offset: number) => new Date(Date.now() - offset * 3_600_000).toISOString();

export const MOCK_STATS: DashboardStats = {
  activeProjects: 8,
  totalProjectValue: 45_000_000,
  totalEmployees: 124,
  newEmployeesThisMonth: 3,
  inventoryItems: 342,
  lowStockCount: 7,
  openPOs: 12,
  openPOValue: 8_500_000,
  monthlyPayroll: 1_850_000,
  payrollDueInDays: 5,
  pendingApprovalsCount: 9,
  pendingApprovalDetails: [
    { id: '1', type: 'PR', title: 'Purchase Requisition – Cement (50 bags)', module: 'Procurement', amount: 125000, requestedBy: 'Rafiq Ahmed', requestedAt: h(2), urgency: 'high', link: '/procurement/purchase-requisitions' },
    { id: '2', type: 'Leave', title: 'Annual Leave – Sadia Islam (5 days)', module: 'HR', requestedBy: 'Sadia Islam', requestedAt: h(5), urgency: 'medium', link: '/hr/leave' },
    { id: '3', type: 'GRN', title: 'GRN-0042 – Steel Rods received', module: 'Inventory', amount: 280000, requestedBy: 'Karim Store', requestedAt: h(8), urgency: 'medium', link: '/procurement/grns' },
    { id: '4', type: 'DPR', title: 'DPR #107 – Site A progress overdue', module: 'Projects', requestedBy: 'Site Engineer', requestedAt: h(24), urgency: 'high', link: '/projects' },
    { id: '5', type: 'Expense', title: 'Travel expense – Chittagong visit', module: 'Accounts', amount: 18500, requestedBy: 'Nasrin Akter', requestedAt: h(30), urgency: 'low', link: '/accounts/journal-entries' },
  ],
  lowStockAlerts: [
    { id: '1', itemName: 'Portland Cement', variantName: '50kg Bag', currentStock: 45, reorderLevel: 200, unit: 'bags', stockPercent: 22, warehouseName: 'Main Warehouse' },
    { id: '2', itemName: 'MS Rod', variantName: '12mm', currentStock: 800, reorderLevel: 5000, unit: 'kg', stockPercent: 16, warehouseName: 'Site Store A' },
    { id: '3', itemName: 'Sand', variantName: 'Fine', currentStock: 3, reorderLevel: 20, unit: 'tons', stockPercent: 15, warehouseName: 'Site Store B' },
    { id: '4', itemName: 'Paint', variantName: 'White Emulsion', currentStock: 12, reorderLevel: 40, unit: 'ltrs', stockPercent: 30, warehouseName: 'Main Warehouse' },
    { id: '5', itemName: 'PVC Pipe', variantName: '2 inch', currentStock: 30, reorderLevel: 100, unit: 'pcs', stockPercent: 30, warehouseName: 'Site Store A' },
  ],
  recentActivities: [
    { id: '1', title: 'PO-0089 approved by Finance', module: 'Procurement', timestamp: h(0.5), type: 'success' },
    { id: '2', title: 'New employee Tariq Hasan onboarded', module: 'HR', timestamp: h(1), type: 'info' },
    { id: '3', title: 'Low stock alert – Portland Cement', module: 'Inventory', timestamp: h(2), type: 'warning' },
    { id: '4', title: 'Invoice INV-2045 issued to Client', module: 'Accounts', timestamp: h(3), type: 'success' },
    { id: '5', title: 'DPR #106 submitted for Site B', module: 'Projects', timestamp: h(5), type: 'info' },
    { id: '6', title: 'GRN-0041 items partially received', module: 'Procurement', timestamp: h(7), type: 'warning' },
    { id: '7', title: 'Payroll processed for April 2026', module: 'HR', timestamp: h(26), type: 'success' },
  ],
  projectProgress: [
    { id: '1', projectCode: 'PRJ-001', projectName: 'Bashundhara Residential', completionPercent: 72, status: 'On Track', daysRemaining: 45, contractValue: 12_000_000 },
    { id: '2', projectCode: 'PRJ-002', projectName: 'Gulshan Commercial', completionPercent: 35, status: 'Delayed', daysRemaining: 120, contractValue: 18_500_000 },
    { id: '3', projectCode: 'PRJ-003', projectName: 'Mirpur Road Bridge', completionPercent: 58, status: 'On Track', daysRemaining: 30, contractValue: 9_200_000 },
    { id: '4', projectCode: 'PRJ-004', projectName: 'Dhanmondi Office Tower', completionPercent: 90, status: 'Ahead', daysRemaining: 12, contractValue: 22_000_000 },
  ],
  monthlyExpenseVsBudget: [
    { month: 'Nov', budget: 3_200_000, actual: 2_850_000 },
    { month: 'Dec', budget: 3_500_000, actual: 3_700_000 },
    { month: 'Jan', budget: 3_000_000, actual: 2_950_000 },
    { month: 'Feb', budget: 3_200_000, actual: 3_100_000 },
    { month: 'Mar', budget: 3_800_000, actual: 4_100_000 },
    { month: 'Apr', budget: 3_500_000, actual: 3_350_000 },
  ],
  budgetUtilization: [
    { type: 'Materials', allocated: 15_000_000, spent: 11_200_000, percent: 75 },
    { type: 'Labor', allocated: 8_000_000, spent: 5_600_000, percent: 70 },
    { type: 'Equipment', allocated: 4_000_000, spent: 3_600_000, percent: 90 },
    { type: 'Overhead', allocated: 2_000_000, spent: 850_000, percent: 43 },
  ],
  upcomingDeadlines: [
    { id: '1', deadlineType: 'Project Milestone', reference: 'PRJ-004', title: 'Final handover – Dhanmondi Tower', dueDate: new Date(Date.now() + 2 * 86400000).toISOString(), daysLeft: 2, status: 'In Progress', link: '/projects' },
    { id: '2', deadlineType: 'Purchase Order', reference: 'PO-0088', title: 'Cement delivery expected', dueDate: new Date(Date.now() + 4 * 86400000).toISOString(), daysLeft: 4, status: 'Pending Delivery', link: '/procurement/purchase-orders' },
    { id: '3', deadlineType: 'Payroll', reference: 'PAY-MAY26', title: 'Monthly payroll due', dueDate: new Date(Date.now() + 5 * 86400000).toISOString(), daysLeft: 5, status: 'Pending', link: '/hr/payroll' },
    { id: '4', deadlineType: 'PR Closing', reference: 'PR-0037', title: 'Quotation submission deadline', dueDate: new Date(Date.now() + 7 * 86400000).toISOString(), daysLeft: 7, status: 'Open', link: '/procurement/purchase-requisitions' },
    { id: '5', deadlineType: 'BOQ Approval', reference: 'BOQ-015', title: 'Site B BOQ final approval', dueDate: new Date(Date.now() + 10 * 86400000).toISOString(), daysLeft: 10, status: 'Under Review', link: '/projects' },
  ],
};

export type UserRole =
  | 'Admin'
  | 'ProjectManager'
  | 'SiteEngineer'
  | 'Accountant'
  | 'StoreKeeper'
  | 'HRManager'
  | 'ProcurementOfficer';

export type ChartType = 'projectProgress' | 'expenseTrend' | 'stockMovement' | 'payrollTrend' | 'poValueTrend';
export type RightChartType = 'expenseVsBudget' | 'budgetUtilization' | 'stockByCategory';

export interface ModuleAccess {
  id: string;
  label: string;
  path: string;
  color: string;
  iconName: string;
}

export interface DashboardVisibility {
  showProjects: boolean;
  showHR: boolean;
  showInventory: boolean;
  showProcurement: boolean;
  showAccounts: boolean;
  showPendingApprovals: boolean;
  showLowStock: boolean;
  showLeaveRequests: boolean;
  showJournalEntries: boolean;
  showMyTasks: boolean;
  visibleModules: ModuleAccess[];
  leftChartType: ChartType;
  rightChartType: RightChartType;
}

const MODULES: Record<string, ModuleAccess> = {
  projects: { id: 'projects', label: 'Projects', path: '/projects', color: 'bg-blue-100 text-blue-600', iconName: 'HardHat' },
  hr: { id: 'hr', label: 'HR & Payroll', path: '/hr', color: 'bg-green-100 text-green-600', iconName: 'Users' },
  inventory: { id: 'inventory', label: 'Inventory', path: '/inventory', color: 'bg-amber-100 text-amber-600', iconName: 'Box' },
  procurement: { id: 'procurement', label: 'Procurement', path: '/procurement', color: 'bg-purple-100 text-purple-600', iconName: 'ShoppingCart' },
  accounts: { id: 'accounts', label: 'Accounts', path: '/accounts', color: 'bg-red-100 text-red-600', iconName: 'Receipt' },
};

function getVisibility(role: string): DashboardVisibility {
  switch (role) {
    case 'ProjectManager':
      return {
        showProjects: true, showHR: false, showInventory: true, showProcurement: true, showAccounts: false,
        showPendingApprovals: true, showLowStock: true, showLeaveRequests: false, showJournalEntries: false, showMyTasks: true,
        visibleModules: [MODULES.projects, MODULES.procurement, MODULES.inventory],
        leftChartType: 'projectProgress', rightChartType: 'budgetUtilization',
      };
    case 'SiteEngineer':
      return {
        showProjects: true, showHR: false, showInventory: true, showProcurement: false, showAccounts: false,
        showPendingApprovals: true, showLowStock: true, showLeaveRequests: false, showJournalEntries: false, showMyTasks: true,
        visibleModules: [MODULES.projects, MODULES.inventory],
        leftChartType: 'projectProgress', rightChartType: 'budgetUtilization',
      };
    case 'Accountant':
      return {
        showProjects: false, showHR: false, showInventory: false, showProcurement: true, showAccounts: true,
        showPendingApprovals: true, showLowStock: false, showLeaveRequests: false, showJournalEntries: true, showMyTasks: false,
        visibleModules: [MODULES.accounts, MODULES.procurement, MODULES.hr],
        leftChartType: 'expenseTrend', rightChartType: 'expenseVsBudget',
      };
    case 'StoreKeeper':
      return {
        showProjects: false, showHR: false, showInventory: true, showProcurement: true, showAccounts: false,
        showPendingApprovals: true, showLowStock: true, showLeaveRequests: false, showJournalEntries: false, showMyTasks: false,
        visibleModules: [MODULES.inventory, MODULES.procurement],
        leftChartType: 'stockMovement', rightChartType: 'stockByCategory',
      };
    case 'HRManager':
      return {
        showProjects: false, showHR: true, showInventory: false, showProcurement: false, showAccounts: true,
        showPendingApprovals: true, showLowStock: false, showLeaveRequests: true, showJournalEntries: false, showMyTasks: false,
        visibleModules: [MODULES.hr, MODULES.accounts],
        leftChartType: 'payrollTrend', rightChartType: 'expenseVsBudget',
      };
    case 'ProcurementOfficer':
      return {
        showProjects: false, showHR: false, showInventory: true, showProcurement: true, showAccounts: true,
        showPendingApprovals: true, showLowStock: true, showLeaveRequests: false, showJournalEntries: false, showMyTasks: false,
        visibleModules: [MODULES.procurement, MODULES.inventory, MODULES.accounts],
        leftChartType: 'poValueTrend', rightChartType: 'expenseVsBudget',
      };
    default: // Admin
      return {
        showProjects: true, showHR: true, showInventory: true, showProcurement: true, showAccounts: true,
        showPendingApprovals: true, showLowStock: true, showLeaveRequests: false, showJournalEntries: false, showMyTasks: false,
        visibleModules: [MODULES.projects, MODULES.hr, MODULES.inventory, MODULES.procurement, MODULES.accounts],
        leftChartType: 'projectProgress', rightChartType: 'expenseVsBudget',
      };
  }
}

export function useDashboard() {
  const user = useAuthStore((s) => s.user);
  const role = (user?.roles?.[0] ?? localStorage.getItem('userRole') ?? 'Admin') as UserRole;

  const query = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: async () => {
      try {
        const res = await getDashboardStats();
        return res.data.data ?? MOCK_STATS;
      } catch {
        return MOCK_STATS;
      }
    },
    placeholderData: MOCK_STATS,
    refetchInterval: 300_000,
    retry: false,
  });

  const stats = query.data ?? MOCK_STATS;
  const visibility = getVisibility(role);

  return {
    stats,
    role,
    visibility,
    isLoading: query.isLoading,
    isError: query.isError,
    refetch: query.refetch,
    lastUpdated: query.dataUpdatedAt,
  };
}

export const _ = now; // suppress unused warning
