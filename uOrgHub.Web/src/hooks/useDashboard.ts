import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import { getDashboardStats } from '../api/dashboard';

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

function getVisibility(role: UserRole, claims: string[]): DashboardVisibility {
  const has = (prefix: string) => claims.some(c => c.startsWith(prefix));
  const isAdmin = role === 'Admin' || claims.includes('Admin.Company.View');

  if (isAdmin) {
    return {
      showProjects: true, showHR: true, showInventory: true, showProcurement: true, showAccounts: true,
      showPendingApprovals: true, showLowStock: true, showLeaveRequests: false, showJournalEntries: false, showMyTasks: false,
      visibleModules: [MODULES.projects, MODULES.hr, MODULES.inventory, MODULES.procurement, MODULES.accounts],
      leftChartType: 'projectProgress', rightChartType: 'expenseVsBudget',
    };
  }

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
    default: {
      const modules: ModuleAccess[] = [];
      let showProjects = false, showHR = false, showInventory = false, showProcurement = false, showAccounts = false;

      if (has('Projects.')) { showProjects = true; modules.push(MODULES.projects); }
      if (has('HR.')) { showHR = true; modules.push(MODULES.hr); }
      if (has('Inventory.')) { showInventory = true; modules.push(MODULES.inventory); }
      if (has('Procurement.')) { showProcurement = true; modules.push(MODULES.procurement); }
      if (has('Accounts.')) { showAccounts = true; modules.push(MODULES.accounts); }

      if (modules.length === 0) {
        showProjects = true; showHR = true; showInventory = true; showProcurement = true; showAccounts = true;
        modules.push(MODULES.projects, MODULES.hr, MODULES.inventory, MODULES.procurement, MODULES.accounts);
      }

      const hasInv = showInventory || showProcurement;

      return {
        showProjects, showHR, showInventory, showProcurement, showAccounts,
        showPendingApprovals: true, showLowStock: hasInv,
        showLeaveRequests: showHR, showJournalEntries: showAccounts, showMyTasks: showProjects,
        visibleModules: modules,
        leftChartType: showProjects ? 'projectProgress' : showAccounts ? 'expenseTrend' : 'stockMovement',
        rightChartType: showAccounts ? 'expenseVsBudget' : showProjects ? 'budgetUtilization' : 'stockByCategory',
      };
    }
  }
}

export function useDashboard() {
  const user = useAuthStore((s) => s.user);
  const claims = user?.claims ?? [];
  const roles = user?.roles ?? [];

  const role: UserRole =
    roles.includes('Admin') ? 'Admin' :
    roles.includes('HRManager') ? 'HRManager' :
    roles.includes('Accountant') ? 'Accountant' :
    roles.includes('InventoryManager') ? 'StoreKeeper' :
    roles.includes('ProcurementOfficer') ? 'ProcurementOfficer' :
    roles.includes('ProjectManager') ? 'ProjectManager' :
    'Admin';

  const query = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: async () => {
      const res = await getDashboardStats();
      return res.data.data!;
    },
    refetchInterval: 300_000,
    retry: 2,
    staleTime: 60_000,
  });

  const stats = query.data;
  const visibility = getVisibility(role, claims);

  return {
    stats,
    role,
    visibility,
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    refetch: query.refetch,
    lastUpdated: query.dataUpdatedAt,
  };
}
