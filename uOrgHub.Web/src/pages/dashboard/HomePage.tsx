import { useNavigate } from 'react-router-dom';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, ComposedChart, Line, PieChart, Pie, Cell,
} from 'recharts';
import {
  HardHat, Users, Box, ShoppingCart, Receipt,
  AlertTriangle, CheckCircle2, Clock, TrendingUp,
  RefreshCw, ChevronRight, Plus,
  LayoutDashboard, Wallet, FileText, Package,
} from 'lucide-react';
import { useDashboard } from '../../hooks/useDashboard';
import { formatBDT, timeAgo, formatDate } from '../../utils/format';
import SkeletonCard from '../../components/shared/SkeletonCard';
import type { PendingApproval, LowStockAlert, RecentActivity, ProjectProgress, MonthlyExpenseData, BudgetUtilization } from '../../api/dashboard';
import type { UserRole, ModuleAccess } from '../../hooks/useDashboard';

// ── helpers ─────────────────────────────────────────────────────────────────

function getGreeting(): string {
  const h = new Date().getHours();
  if (h < 12) return 'Good morning';
  if (h < 17) return 'Good afternoon';
  return 'Good evening';
}

function todayLabel(): string {
  return new Date().toLocaleDateString('en-BD', {
    weekday: 'long', day: '2-digit', month: 'long', year: 'numeric',
  });
}

const APPROVAL_BADGE: Record<string, string> = {
  PR: 'bg-amber-100 text-amber-700',
  DPR: 'bg-red-100 text-red-700',
  Leave: 'bg-purple-100 text-purple-700',
  Expense: 'bg-orange-100 text-orange-700',
  GRN: 'bg-blue-100 text-blue-700',
  BOQ: 'bg-green-100 text-green-700',
};

const URGENCY_DOT: Record<string, string> = {
  high: 'bg-red-500',
  medium: 'bg-amber-400',
  low: 'bg-green-400',
};

const PIE_COLORS = ['#0ea5e9', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'];

// Custom recharts tooltip
interface TooltipProps { active?: boolean; payload?: { name: string; value: number; color: string }[]; label?: string }
function BDTTooltip({ active, payload, label }: TooltipProps) {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-md p-3 text-xs">
      <p className="font-medium text-gray-700 mb-1">{label}</p>
      {payload.map((p) => (
        <p key={p.name} style={{ color: p.color }}>{p.name}: {formatBDT(p.value)}</p>
      ))}
    </div>
  );
}

// ── Section 1: Welcome Header ────────────────────────────────────────────────

interface WelcomeSectionProps {
  firstName: string;
  role: string;
  pendingCount: number;
  lowStockCount: number;
  isProjectManager: boolean;
}

function WelcomeSection({ firstName, role, pendingCount, lowStockCount, isProjectManager }: WelcomeSectionProps) {
  const navigate = useNavigate();
  return (
    <div className="flex items-start justify-between flex-wrap gap-4 mb-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900">
          {getGreeting()}, {firstName} 👋
        </h1>
        <p className="text-sm text-gray-500 mt-0.5">
          {todayLabel()} · <span className="font-medium text-primary-600">{role}</span> · uOrgHub ERP
        </p>
      </div>
      <div className="flex items-center gap-2 flex-wrap">
        {pendingCount > 0 && (
          <button
            onClick={() => navigate('/procurement/purchase-requisitions')}
            className="flex items-center gap-1.5 bg-amber-50 border border-amber-200 text-amber-700 text-xs font-medium px-3 py-1.5 rounded-full hover:bg-amber-100 transition-colors"
          >
            <AlertTriangle size={13} />
            {pendingCount} pending approvals
          </button>
        )}
        {lowStockCount > 0 && (
          <button
            onClick={() => navigate('/inventory/stock-balances')}
            className="flex items-center gap-1.5 bg-red-50 border border-red-200 text-red-600 text-xs font-medium px-3 py-1.5 rounded-full hover:bg-red-100 transition-colors"
          >
            <span className="w-2 h-2 rounded-full bg-red-500 inline-block" />
            {lowStockCount} low stock alerts
          </button>
        )}
        {isProjectManager && (
          <button
            onClick={() => navigate('/projects')}
            className="flex items-center gap-1.5 bg-red-50 border border-red-200 text-red-600 text-xs font-medium px-3 py-1.5 rounded-full hover:bg-red-100 transition-colors"
          >
            <FileText size={13} />
            2 overdue DPRs
          </button>
        )}
      </div>
    </div>
  );
}

// ── Section 2: KPI Cards ─────────────────────────────────────────────────────

interface KpiCard {
  label: string;
  value: string;
  sub: string;
  subColor: string;
  iconBg: string;
  iconColor: string;
  Icon: React.ComponentType<{ size?: number; className?: string }>;
  link: string;
}

function buildKpiCards(
  role: UserRole,
  stats: { activeProjects: number; totalProjectValue: number; totalEmployees: number; newEmployeesThisMonth: number; inventoryItems: number; lowStockCount: number; openPOs: number; openPOValue: number; monthlyPayroll: number; payrollDueInDays: number },
): KpiCard[] {
  switch (role) {
    case 'ProjectManager':
      return [
        { label: 'Active Projects', value: String(stats.activeProjects), sub: formatBDT(stats.totalProjectValue) + ' total value', subColor: 'text-blue-600', iconBg: 'bg-blue-50', iconColor: 'text-blue-600', Icon: HardHat, link: '/projects' },
        { label: 'Budget Used', value: '68%', sub: 'of allocated budget', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: TrendingUp, link: '/projects' },
        { label: 'Pending DPRs', value: '4', sub: '2 overdue', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: FileText, link: '/projects' },
        { label: 'Material Requests', value: '7', sub: '3 awaiting approval', subColor: 'text-purple-600', iconBg: 'bg-purple-50', iconColor: 'text-purple-600', Icon: Package, link: '/procurement/purchase-requisitions' },
        { label: 'Team Members', value: String(stats.totalEmployees), sub: `${stats.newEmployeesThisMonth} new this month`, subColor: 'text-green-600', iconBg: 'bg-green-50', iconColor: 'text-green-600', Icon: Users, link: '/hr/employees' },
      ];
    case 'SiteEngineer':
      return [
        { label: 'My Projects', value: '3', sub: '1 ahead of schedule', subColor: 'text-green-600', iconBg: 'bg-blue-50', iconColor: 'text-blue-600', Icon: HardHat, link: '/projects' },
        { label: "Today's DPR", value: 'Pending', sub: 'Submit before 5 PM', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: FileText, link: '/projects' },
        { label: 'Material Requests', value: '2', sub: '1 approved', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Package, link: '/procurement/purchase-requisitions' },
        { label: 'Pending Issues', value: '5', sub: '1 critical', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: AlertTriangle, link: '/projects' },
        { label: 'Days to Deadline', value: '12', sub: 'Dhanmondi Tower', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Clock, link: '/projects' },
      ];
    case 'Accountant':
      return [
        { label: 'Total Receivables', value: formatBDT(3_200_000), sub: '12 open invoices', subColor: 'text-green-600', iconBg: 'bg-green-50', iconColor: 'text-green-600', Icon: TrendingUp, link: '/accounts/invoices' },
        { label: 'Total Payables', value: formatBDT(1_850_000), sub: '8 pending bills', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: Wallet, link: '/accounts/bills' },
        { label: 'This Month Expenses', value: formatBDT(3_350_000), sub: '96% of budget', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Receipt, link: '/accounts/journal-entries' },
        { label: 'Journal Entries', value: '24', sub: 'Today', subColor: 'text-blue-600', iconBg: 'bg-blue-50', iconColor: 'text-blue-600', Icon: FileText, link: '/accounts/journal-entries' },
        { label: 'Cash Balance', value: formatBDT(12_500_000), sub: 'Across all accounts', subColor: 'text-green-600', iconBg: 'bg-green-50', iconColor: 'text-green-600', Icon: Wallet, link: '/accounts/bank-accounts' },
      ];
    case 'StoreKeeper':
      return [
        { label: 'Total Items', value: String(stats.inventoryItems), sub: 'In all warehouses', subColor: 'text-blue-600', iconBg: 'bg-blue-50', iconColor: 'text-blue-600', Icon: Box, link: '/inventory/items' },
        { label: 'Low Stock Alerts', value: String(stats.lowStockCount), sub: '3 critical', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: AlertTriangle, link: '/inventory/stock-balances' },
        { label: 'Pending GRNs', value: '5', sub: '2 overdue', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Package, link: '/procurement/grns' },
        { label: "Today's Transactions", value: '18', sub: '10 in · 8 out', subColor: 'text-green-600', iconBg: 'bg-green-50', iconColor: 'text-green-600', Icon: TrendingUp, link: '/inventory/stock-transactions' },
        { label: 'Warehouses', value: '3', sub: 'Active locations', subColor: 'text-purple-600', iconBg: 'bg-purple-50', iconColor: 'text-purple-600', Icon: Box, link: '/inventory/warehouses' },
      ];
    case 'HRManager':
      return [
        { label: 'Total Employees', value: String(stats.totalEmployees), sub: `${stats.newEmployeesThisMonth} new this month`, subColor: 'text-green-600', iconBg: 'bg-green-50', iconColor: 'text-green-600', Icon: Users, link: '/hr/employees' },
        { label: 'On Leave Today', value: '8', sub: '3 approved today', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Clock, link: '/hr/leave' },
        { label: 'Pending Leaves', value: '6', sub: 'Awaiting approval', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: AlertTriangle, link: '/hr/leave' },
        { label: 'Payroll Status', value: 'Pending', sub: `Due in ${stats.payrollDueInDays} days`, subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Wallet, link: '/hr/payroll' },
        { label: 'New Joiners', value: String(stats.newEmployeesThisMonth), sub: 'This month', subColor: 'text-blue-600', iconBg: 'bg-blue-50', iconColor: 'text-blue-600', Icon: Users, link: '/hr/employees' },
      ];
    case 'ProcurementOfficer':
      return [
        { label: 'Open PRs', value: '9', sub: '3 urgent', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: FileText, link: '/procurement/purchase-requisitions' },
        { label: 'Open RFQs', value: '4', sub: '2 closing today', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: FileText, link: '/procurement/rfqs' },
        { label: 'Open POs', value: String(stats.openPOs), sub: formatBDT(stats.openPOValue) + ' value', subColor: 'text-purple-600', iconBg: 'bg-purple-50', iconColor: 'text-purple-600', Icon: ShoppingCart, link: '/procurement/purchase-orders' },
        { label: 'Pending GRNs', value: '5', sub: '2 overdue', subColor: 'text-amber-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Package, link: '/procurement/grns' },
        { label: 'This Month Spend', value: formatBDT(4_200_000), sub: '88% of budget', subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: Wallet, link: '/accounts/budgets' },
      ];
    default: // Admin
      return [
        { label: 'Active Projects', value: String(stats.activeProjects), sub: formatBDT(stats.totalProjectValue) + ' total value', subColor: 'text-blue-600', iconBg: 'bg-blue-50', iconColor: 'text-blue-600', Icon: HardHat, link: '/projects' },
        { label: 'Total Employees', value: String(stats.totalEmployees), sub: `+${stats.newEmployeesThisMonth} this month`, subColor: 'text-green-600', iconBg: 'bg-green-50', iconColor: 'text-green-600', Icon: Users, link: '/hr' },
        { label: 'Inventory Items', value: String(stats.inventoryItems), sub: `${stats.lowStockCount} low stock`, subColor: 'text-red-600', iconBg: 'bg-amber-50', iconColor: 'text-amber-600', Icon: Box, link: '/inventory' },
        { label: 'Open POs', value: String(stats.openPOs), sub: formatBDT(stats.openPOValue) + ' value', subColor: 'text-purple-600', iconBg: 'bg-purple-50', iconColor: 'text-purple-600', Icon: ShoppingCart, link: '/procurement/purchase-orders' },
        { label: 'Monthly Payroll', value: formatBDT(stats.monthlyPayroll), sub: `Due in ${stats.payrollDueInDays} days`, subColor: 'text-red-600', iconBg: 'bg-red-50', iconColor: 'text-red-600', Icon: Wallet, link: '/hr/payroll' },
      ];
  }
}

function KpiSection({ role, stats, isLoading }: { role: UserRole; stats: Parameters<typeof buildKpiCards>[1]; isLoading: boolean }) {
  const navigate = useNavigate();
  const cards = buildKpiCards(role, stats);

  if (isLoading) {
    return (
      <div className="grid grid-cols-5 gap-4 mb-6">
        {[...Array(5)].map((_, i) => <SkeletonCard key={i} height="h-24" />)}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4 mb-6">
      {cards.map((card) => (
        <button
          key={card.label}
          onClick={() => navigate(card.link)}
          className="bg-white border border-gray-200 rounded-xl p-4 text-left hover:border-primary-300 hover:shadow-sm transition-all group"
        >
          <div className={`w-9 h-9 rounded-lg ${card.iconBg} flex items-center justify-center mb-3`}>
            <card.Icon size={18} className={card.iconColor} />
          </div>
          <p className="text-xs text-gray-500 mb-0.5">{card.label}</p>
          <p className="text-2xl font-medium text-gray-900 leading-tight">{card.value}</p>
          <p className={`text-xs mt-1 ${card.subColor}`}>{card.sub}</p>
        </button>
      ))}
    </div>
  );
}

// ── Section 3: Charts ────────────────────────────────────────────────────────

function leftChartTitle(type: string): string {
  switch (type) {
    case 'expenseTrend': return 'Monthly Expense Trend';
    case 'stockMovement': return 'Stock Movement';
    case 'payrollTrend': return 'Payroll Trend';
    case 'poValueTrend': return 'PO Value Trend';
    default: return 'Project Progress Overview';
  }
}

function LeftChartCard({
  type, projectProgress, monthlyExpense, isLoading,
}: { type: string; projectProgress: ProjectProgress[]; monthlyExpense: MonthlyExpenseData[]; isLoading: boolean }) {
  if (isLoading) return <SkeletonCard height="h-64" />;

  const isProjectChart = type === 'projectProgress';

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-medium text-gray-800">{leftChartTitle(type)}</h3>
        <div className="flex items-center gap-3 text-xs text-gray-500">
          {isProjectChart ? (
            <><span className="flex items-center gap-1"><span className="w-2 h-2 rounded-sm bg-primary-500 inline-block" />Completion %</span></>
          ) : (
            <><span className="flex items-center gap-1"><span className="w-2 h-2 rounded-sm bg-primary-500 inline-block" />Actual</span></>
          )}
        </div>
      </div>
      <ResponsiveContainer width="100%" height={180}>
        {isProjectChart ? (
          <BarChart data={projectProgress} margin={{ top: 0, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
            <XAxis dataKey="projectCode" tick={{ fontSize: 10 }} />
            <YAxis tick={{ fontSize: 10 }} domain={[0, 100]} unit="%" />
            <Tooltip content={<SimplePercentTooltip />} />
            <Bar dataKey="completionPercent" name="Completion" fill="#0ea5e9" radius={[4, 4, 0, 0]} animationDuration={800} />
          </BarChart>
        ) : (
          <ComposedChart data={monthlyExpense} margin={{ top: 0, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
            <XAxis dataKey="month" tick={{ fontSize: 10 }} />
            <YAxis tick={{ fontSize: 10 }} tickFormatter={(v) => `${(v / 100000).toFixed(0)}L`} />
            <Tooltip content={<BDTTooltip />} />
            <Bar dataKey="actual" name="Actual" fill="#0ea5e9" radius={[4, 4, 0, 0]} animationDuration={800} />
          </ComposedChart>
        )}
      </ResponsiveContainer>
    </div>
  );
}

function SimplePercentTooltip({ active, payload, label }: TooltipProps & { payload?: { name: string; value: number; color: string }[] }) {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-md p-3 text-xs">
      <p className="font-medium text-gray-700 mb-1">{label}</p>
      {payload.map((p) => (
        <p key={p.name} style={{ color: p.color }}>{p.name}: {p.value}%</p>
      ))}
    </div>
  );
}

function RightChartCard({
  type, monthlyExpense, budgetUtilization, isLoading,
}: { type: string; monthlyExpense: MonthlyExpenseData[]; budgetUtilization: BudgetUtilization[]; isLoading: boolean }) {
  if (isLoading) return <SkeletonCard height="h-64" />;

  if (type === 'expenseVsBudget' || type === 'expenseTrend') {
    return (
      <div className="bg-white border border-gray-200 rounded-xl p-4">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-sm font-medium text-gray-800">Monthly Expense vs Budget</h3>
          <div className="flex items-center gap-3 text-xs text-gray-500">
            <span className="flex items-center gap-1"><span className="w-2 h-2 rounded-sm bg-gray-300 inline-block" />Budget</span>
            <span className="flex items-center gap-1"><span className="w-2 h-2 rounded-sm bg-primary-500 inline-block" />Actual</span>
          </div>
        </div>
        <ResponsiveContainer width="100%" height={180}>
          <ComposedChart data={monthlyExpense} margin={{ top: 0, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
            <XAxis dataKey="month" tick={{ fontSize: 10 }} />
            <YAxis tick={{ fontSize: 10 }} tickFormatter={(v) => `${(v / 100000).toFixed(0)}L`} />
            <Tooltip content={<BDTTooltip />} />
            <Bar dataKey="budget" name="Budget" fill="#e5e7eb" radius={[4, 4, 0, 0]} animationDuration={800} />
            <Line dataKey="actual" name="Actual" stroke="#0ea5e9" strokeWidth={2} dot={{ r: 3 }} animationDuration={800} />
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    );
  }

  if (type === 'budgetUtilization') {
    return (
      <div className="bg-white border border-gray-200 rounded-xl p-4">
        <h3 className="text-sm font-medium text-gray-800 mb-4">Project Budget Utilization</h3>
        <div className="space-y-3">
          {budgetUtilization.map((b) => (
            <div key={b.type}>
              <div className="flex justify-between text-xs text-gray-600 mb-1">
                <span>{b.type}</span>
                <span>{b.percent}% · {formatBDT(b.spent)} / {formatBDT(b.allocated)}</span>
              </div>
              <div className="w-full bg-gray-100 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${b.percent >= 90 ? 'bg-red-500' : b.percent >= 70 ? 'bg-amber-400' : 'bg-primary-500'}`}
                  style={{ width: `${b.percent}%` }}
                />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  // stockByCategory donut
  const donutData = [
    { name: 'Materials', value: 45 },
    { name: 'Tools', value: 20 },
    { name: 'Electrical', value: 18 },
    { name: 'Plumbing', value: 10 },
    { name: 'Other', value: 7 },
  ];
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4">
      <h3 className="text-sm font-medium text-gray-800 mb-2">Stock Levels by Category</h3>
      <div className="flex items-center gap-4">
        <ResponsiveContainer width="50%" height={180}>
          <PieChart>
            <Pie data={donutData} cx="50%" cy="50%" innerRadius={45} outerRadius={70} dataKey="value" animationDuration={800}>
              {donutData.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
            </Pie>
            <Tooltip formatter={(v) => `${v}%`} />
          </PieChart>
        </ResponsiveContainer>
        <div className="space-y-1.5 text-xs">
          {donutData.map((d, i) => (
            <div key={d.name} className="flex items-center gap-2">
              <span className="w-2.5 h-2.5 rounded-sm" style={{ backgroundColor: PIE_COLORS[i] }} />
              <span className="text-gray-600">{d.name}</span>
              <span className="font-medium text-gray-800 ml-auto">{d.value}%</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function ChartsSection({ leftType, rightType, projectProgress, monthlyExpense, budgetUtilization, isLoading }: {
  leftType: string; rightType: string;
  projectProgress: ProjectProgress[]; monthlyExpense: MonthlyExpenseData[]; budgetUtilization: BudgetUtilization[];
  isLoading: boolean;
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
      <LeftChartCard type={leftType} projectProgress={projectProgress} monthlyExpense={monthlyExpense} isLoading={isLoading} />
      <RightChartCard type={rightType} monthlyExpense={monthlyExpense} budgetUtilization={budgetUtilization} isLoading={isLoading} />
    </div>
  );
}

// ── Section 4 col 1: Pending Approvals ───────────────────────────────────────

function PendingApprovalsList({ items, isLoading }: { items: PendingApproval[]; isLoading: boolean }) {
  const navigate = useNavigate();
  const urgentCount = items.filter((i) => i.urgency === 'high').length;

  if (isLoading) {
    return (
      <div className="bg-white border border-gray-200 rounded-xl p-4 space-y-3">
        <SkeletonCard height="h-5" width="w-40" rounded="rounded" />
        {[...Array(4)].map((_, i) => <SkeletonCard key={i} height="h-12" />)}
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col h-full">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-medium text-gray-800">Pending Approvals</h3>
        {urgentCount > 0 && (
          <span className="bg-red-100 text-red-700 text-xs font-medium px-2 py-0.5 rounded-full">
            {urgentCount} urgent
          </span>
        )}
      </div>

      {items.length === 0 ? (
        <div className="flex flex-col items-center justify-center flex-1 py-8 text-gray-400">
          <CheckCircle2 size={32} className="mb-2 text-green-400" />
          <p className="text-sm">No pending approvals</p>
        </div>
      ) : (
        <div className="space-y-2 flex-1">
          {items.slice(0, 5).map((item) => (
            <button
              key={item.id}
              onClick={() => navigate(item.link)}
              className="w-full text-left flex items-start gap-2 p-2 rounded-lg hover:bg-gray-50 transition-colors group"
            >
              <span className={`mt-1.5 w-2 h-2 rounded-full flex-shrink-0 ${URGENCY_DOT[item.urgency]}`} />
              <div className="flex-1 min-w-0">
                <p className="text-xs font-medium text-gray-800 truncate">{item.title}</p>
                <div className="flex items-center gap-2 mt-0.5">
                  <span className={`text-[10px] font-medium px-1.5 py-0.5 rounded ${APPROVAL_BADGE[item.type]}`}>{item.type}</span>
                  {item.amount && <span className="text-[10px] text-gray-500">{formatBDT(item.amount)}</span>}
                  <span className="text-[10px] text-gray-400 ml-auto">{timeAgo(item.requestedAt)}</span>
                </div>
              </div>
              <ChevronRight size={14} className="text-gray-300 group-hover:text-gray-500 flex-shrink-0 mt-1" />
            </button>
          ))}
        </div>
      )}

      <button onClick={() => navigate('/procurement/purchase-requisitions')} className="mt-3 text-xs text-primary-600 hover:text-primary-700 font-medium flex items-center gap-1">
        View all <ChevronRight size={12} />
      </button>
    </div>
  );
}

// ── Section 4 col 2: Low Stock / role-based ───────────────────────────────────

function LowStockList({ items, isLoading }: { items: LowStockAlert[]; isLoading: boolean }) {
  const navigate = useNavigate();
  const criticalCount = items.filter((i) => i.stockPercent < 20).length;

  if (isLoading) {
    return (
      <div className="bg-white border border-gray-200 rounded-xl p-4 space-y-3">
        <SkeletonCard height="h-5" width="w-40" rounded="rounded" />
        {[...Array(4)].map((_, i) => <SkeletonCard key={i} height="h-14" />)}
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col h-full">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-medium text-gray-800">Low Stock Alerts</h3>
        {criticalCount > 0 && (
          <span className="bg-red-100 text-red-700 text-xs font-medium px-2 py-0.5 rounded-full">
            {criticalCount} critical
          </span>
        )}
      </div>

      {items.length === 0 ? (
        <div className="flex flex-col items-center justify-center flex-1 py-8 text-gray-400">
          <CheckCircle2 size={32} className="mb-2 text-green-400" />
          <p className="text-sm">All stock levels healthy</p>
        </div>
      ) : (
        <div className="space-y-3 flex-1">
          {items.slice(0, 4).map((item) => {
            const barColor = item.stockPercent < 20 ? 'bg-red-500' : item.stockPercent < 50 ? 'bg-amber-400' : 'bg-green-400';
            return (
              <div key={item.id} className="group">
                <button
                  onClick={() => navigate('/inventory/items')}
                  className="w-full text-left"
                >
                  <div className="flex justify-between items-start mb-1">
                    <div>
                      <p className="text-xs font-medium text-gray-800 truncate">{item.itemName}</p>
                      <p className="text-[10px] text-gray-500">{item.variantName} · {item.warehouseName}</p>
                    </div>
                    <span className={`text-[10px] font-medium ${item.stockPercent < 20 ? 'text-red-600' : 'text-amber-600'}`}>
                      {item.currentStock} / {item.reorderLevel} {item.unit}
                    </span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-1.5 mb-1.5">
                    <div className={`h-1.5 rounded-full ${barColor}`} style={{ width: `${Math.min(item.stockPercent, 100)}%` }} />
                  </div>
                </button>
                <button
                  onClick={() => navigate('/procurement/purchase-requisitions')}
                  className="flex items-center gap-1 text-[10px] text-primary-600 hover:text-primary-700 font-medium"
                >
                  <Plus size={10} /> Create PR
                </button>
              </div>
            );
          })}
        </div>
      )}

      <button onClick={() => navigate('/inventory/stock-balances')} className="mt-3 text-xs text-primary-600 hover:text-primary-700 font-medium flex items-center gap-1">
        View all <ChevronRight size={12} />
      </button>
    </div>
  );
}

function LeaveRequestsList({ isLoading }: { isLoading: boolean }) {
  const navigate = useNavigate();
  const mockLeaves = [
    { id: '1', name: 'Sadia Islam', type: 'Annual', days: 5, from: '2026-05-18', status: 'Pending' },
    { id: '2', name: 'Tariq Hasan', type: 'Sick', days: 2, from: '2026-05-15', status: 'Pending' },
    { id: '3', name: 'Nasrin Akter', type: 'Casual', days: 1, from: '2026-05-14', status: 'Approved' },
  ];
  if (isLoading) return <SkeletonCard height="h-64" />;
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col h-full">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-medium text-gray-800">Leave Requests</h3>
        <span className="bg-amber-100 text-amber-700 text-xs font-medium px-2 py-0.5 rounded-full">2 pending</span>
      </div>
      <div className="space-y-2 flex-1">
        {mockLeaves.map((l) => (
          <button key={l.id} onClick={() => navigate('/hr/leave')} className="w-full text-left flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50">
            <div className="w-7 h-7 rounded-full bg-primary-100 text-primary-700 flex items-center justify-center text-xs font-medium flex-shrink-0">
              {l.name.charAt(0)}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-gray-800 truncate">{l.name}</p>
              <p className="text-[10px] text-gray-500">{l.type} · {l.days} day{l.days > 1 ? 's' : ''} from {l.from}</p>
            </div>
            <span className={`text-[10px] font-medium px-1.5 py-0.5 rounded ${l.status === 'Approved' ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'}`}>
              {l.status}
            </span>
          </button>
        ))}
      </div>
      <button onClick={() => navigate('/hr/leave')} className="mt-3 text-xs text-primary-600 hover:text-primary-700 font-medium flex items-center gap-1">
        View all <ChevronRight size={12} />
      </button>
    </div>
  );
}

function JournalEntriesList({ isLoading }: { isLoading: boolean }) {
  const navigate = useNavigate();
  const entries = [
    { id: '1', ref: 'JE-2045', desc: 'Office supplies purchase', debit: 15000, credit: 15000, date: '2026-05-14' },
    { id: '2', ref: 'JE-2044', desc: 'Vendor payment – ACI Ltd', debit: 280000, credit: 280000, date: '2026-05-14' },
    { id: '3', ref: 'JE-2043', desc: 'Invoice receipt – Client A', debit: 450000, credit: 450000, date: '2026-05-13' },
  ];
  if (isLoading) return <SkeletonCard height="h-64" />;
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col h-full">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-medium text-gray-800">Recent Journal Entries</h3>
        <span className="bg-blue-100 text-blue-700 text-xs font-medium px-2 py-0.5 rounded-full">Today 24</span>
      </div>
      <div className="space-y-2 flex-1">
        {entries.map((e) => (
          <button key={e.id} onClick={() => navigate('/accounts/journal-entries')} className="w-full text-left flex items-start gap-2 p-2 rounded-lg hover:bg-gray-50">
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-gray-800">{e.ref}</p>
              <p className="text-[10px] text-gray-500 truncate">{e.desc}</p>
            </div>
            <div className="text-right flex-shrink-0">
              <p className="text-xs text-green-600 font-medium">{formatBDT(e.debit)}</p>
              <p className="text-[10px] text-gray-400">{e.date}</p>
            </div>
          </button>
        ))}
      </div>
      <button onClick={() => navigate('/accounts/journal-entries')} className="mt-3 text-xs text-primary-600 hover:text-primary-700 font-medium flex items-center gap-1">
        View all <ChevronRight size={12} />
      </button>
    </div>
  );
}

function MyTasksList({ isLoading }: { isLoading: boolean }) {
  const tasks = [
    { id: '1', title: 'Submit DPR for Site A', due: 'Today 5 PM', priority: 'high' },
    { id: '2', title: 'Review BOQ for PRJ-003', due: 'Tomorrow', priority: 'medium' },
    { id: '3', title: 'Approve material request #MR-45', due: 'May 16', priority: 'low' },
    { id: '4', title: 'Site inspection report upload', due: 'May 17', priority: 'medium' },
  ];
  if (isLoading) return <SkeletonCard height="h-64" />;
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col h-full">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-sm font-medium text-gray-800">My Tasks Today</h3>
        <span className="bg-red-100 text-red-700 text-xs font-medium px-2 py-0.5 rounded-full">1 urgent</span>
      </div>
      <div className="space-y-2 flex-1">
        {tasks.map((t) => (
          <div key={t.id} className="flex items-center gap-2 p-2 rounded-lg hover:bg-gray-50">
            <span className={`w-2 h-2 rounded-full flex-shrink-0 ${URGENCY_DOT[t.priority as keyof typeof URGENCY_DOT]}`} />
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-gray-800 truncate">{t.title}</p>
              <p className="text-[10px] text-gray-500">{t.due}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

// ── Section 4 col 3: Recent Activity ─────────────────────────────────────────

const ACTIVITY_DOT: Record<string, string> = {
  success: 'bg-green-500',
  info: 'bg-blue-500',
  warning: 'bg-amber-400',
};

function groupActivities(activities: RecentActivity[]): { label: string; items: RecentActivity[] }[] {
  const today: RecentActivity[] = [];
  const yesterday: RecentActivity[] = [];
  const older: RecentActivity[] = [];
  const now = Date.now();
  activities.forEach((a) => {
    const diff = now - new Date(a.timestamp).getTime();
    const hours = diff / 3_600_000;
    if (hours < 24) today.push(a);
    else if (hours < 48) yesterday.push(a);
    else older.push(a);
  });
  const groups: { label: string; items: RecentActivity[] }[] = [];
  if (today.length) groups.push({ label: 'Today', items: today });
  if (yesterday.length) groups.push({ label: 'Yesterday', items: yesterday });
  if (older.length) groups.push({ label: 'Earlier', items: older });
  return groups;
}

function RecentActivityList({ items, isLoading }: { items: RecentActivity[]; isLoading: boolean }) {
  if (isLoading) {
    return (
      <div className="bg-white border border-gray-200 rounded-xl p-4 space-y-3">
        <SkeletonCard height="h-5" width="w-32" rounded="rounded" />
        {[...Array(5)].map((_, i) => <SkeletonCard key={i} height="h-10" />)}
      </div>
    );
  }

  const groups = groupActivities(items);

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 flex flex-col h-full">
      <h3 className="text-sm font-medium text-gray-800 mb-3">Recent Activity</h3>
      <div className="flex-1 space-y-4 overflow-y-auto">
        {groups.map((group) => (
          <div key={group.label}>
            <p className="text-[10px] font-semibold text-gray-400 uppercase tracking-wider mb-2">{group.label}</p>
            <div className="space-y-2">
              {group.items.map((item) => (
                <div key={item.id} className="flex items-start gap-2.5">
                  <span className={`mt-1.5 w-1.5 h-1.5 rounded-full flex-shrink-0 ${ACTIVITY_DOT[item.type]}`} />
                  <div className="flex-1 min-w-0">
                    <p className="text-xs text-gray-700 leading-snug">{item.title}</p>
                    <div className="flex items-center gap-2 mt-0.5">
                      <span className="text-[10px] bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">{item.module}</span>
                      <span className="text-[10px] text-gray-400">{timeAgo(item.timestamp)}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function ThreeColumnSection({
  approvals, lowStockItems, activities, visibility, isLoading,
}: {
  approvals: PendingApproval[];
  lowStockItems: LowStockAlert[];
  activities: RecentActivity[];
  visibility: { showLowStock: boolean; showLeaveRequests: boolean; showJournalEntries: boolean; showMyTasks: boolean };
  isLoading: boolean;
}) {
  const mid = visibility.showLowStock
    ? <LowStockList items={lowStockItems} isLoading={isLoading} />
    : visibility.showLeaveRequests
    ? <LeaveRequestsList isLoading={isLoading} />
    : visibility.showJournalEntries
    ? <JournalEntriesList isLoading={isLoading} />
    : visibility.showMyTasks
    ? <MyTasksList isLoading={isLoading} />
    : <LowStockList items={lowStockItems} isLoading={isLoading} />;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
      <PendingApprovalsList items={approvals} isLoading={isLoading} />
      {mid}
      <RecentActivityList items={activities} isLoading={isLoading} />
    </div>
  );
}

// ── Section 5: Quick Access ───────────────────────────────────────────────────

const MODULE_ICON_MAP: Record<string, React.ComponentType<{ size?: number; className?: string }>> = {
  HardHat, Users, Box, ShoppingCart, Receipt,
};

function QuickAccessSection({ modules }: { modules: ModuleAccess[] }) {
  const navigate = useNavigate();
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 mb-6">
      <h3 className="text-sm font-medium text-gray-800 mb-4">Quick Access</h3>
      <div className="flex flex-wrap gap-4">
        {modules.map((mod) => {
          const Icon = MODULE_ICON_MAP[mod.iconName] ?? LayoutDashboard;
          return (
            <button
              key={mod.id}
              onClick={() => navigate(mod.path)}
              className="flex flex-col items-center gap-2 p-3 rounded-xl border border-gray-100 hover:border-primary-300 hover:bg-primary-50 transition-all group min-w-[80px]"
            >
              <div className={`w-10 h-10 rounded-xl ${mod.color} flex items-center justify-center group-hover:scale-105 transition-transform`}>
                <Icon size={20} />
              </div>
              <span className="text-xs text-gray-600 font-medium">{mod.label}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}

// ── Section 6: Upcoming Deadlines ────────────────────────────────────────────

function DeadlinesSection({ stats, isLoading }: { stats: { upcomingDeadlines: { id: string; deadlineType: string; reference: string; title: string; dueDate: string; daysLeft: number; status: string; link: string }[] }; isLoading: boolean }) {
  const navigate = useNavigate();
  if (isLoading) return <SkeletonCard height="h-48" />;

  const deadlines = stats.upcomingDeadlines ?? [];
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-medium text-gray-800">Upcoming Deadlines</h3>
        <button className="text-xs text-primary-600 hover:text-primary-700 font-medium flex items-center gap-1">
          View all <ChevronRight size={12} />
        </button>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-xs">
          <thead>
            <tr className="text-left text-gray-500 border-b border-gray-100">
              <th className="pb-2 pr-4 font-medium">Type</th>
              <th className="pb-2 pr-4 font-medium">Reference</th>
              <th className="pb-2 pr-4 font-medium">Title</th>
              <th className="pb-2 pr-4 font-medium">Due Date</th>
              <th className="pb-2 pr-4 font-medium">Days Left</th>
              <th className="pb-2 pr-4 font-medium">Status</th>
              <th className="pb-2 font-medium">Action</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {deadlines.slice(0, 5).map((d) => {
              const dayColor = d.daysLeft < 3 ? 'text-red-600 font-semibold' : d.daysLeft < 7 ? 'text-amber-600 font-medium' : 'text-green-600';
              return (
                <tr key={d.id} className="hover:bg-gray-50">
                  <td className="py-2.5 pr-4 text-gray-600">{d.deadlineType}</td>
                  <td className="py-2.5 pr-4 font-medium text-gray-800">{d.reference}</td>
                  <td className="py-2.5 pr-4 text-gray-700 max-w-[200px] truncate">{d.title}</td>
                  <td className="py-2.5 pr-4 text-gray-600">{formatDate(d.dueDate)}</td>
                  <td className={`py-2.5 pr-4 ${dayColor}`}>{d.daysLeft}d</td>
                  <td className="py-2.5 pr-4">
                    <span className="bg-gray-100 text-gray-600 px-2 py-0.5 rounded text-[10px] font-medium">{d.status}</span>
                  </td>
                  <td className="py-2.5">
                    <button onClick={() => navigate(d.link)} className="text-primary-600 hover:text-primary-700 font-medium">
                      View
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
        {deadlines.length === 0 && (
          <p className="text-center text-gray-400 py-8 text-xs">No upcoming deadlines</p>
        )}
      </div>
    </div>
  );
}

// ── Main HomePage ────────────────────────────────────────────────────────────

const EMPTY_STATS: DashboardStats = {
  activeProjects: 0, totalProjectValue: 0, projectProgress: [],
  totalEmployees: 0, newEmployeesThisMonth: 0, employeesByDepartment: [],
  inventoryItems: 0, lowStockCount: 0, lowStockAlerts: [],
  openPOs: 0, openPOValue: 0,
  monthlyPayroll: 0, payrollDueInDays: 0,
  pendingApprovalsCount: 0, pendingApprovalDetails: [],
  recentActivities: [],
  monthlyExpenseVsBudget: [],
  budgetUtilization: [],
  upcomingDeadlines: [],
};

export default function HomePage() {
  const { stats: liveStats, role, visibility, isLoading, isError, refetch, lastUpdated } = useDashboard();
  const user = { firstName: 'Admin' };

  // Try to get firstName from localStorage
  try {
    const stored = localStorage.getItem('user');
    if (stored) {
      const parsed = JSON.parse(stored);
      if (parsed.firstName) user.firstName = parsed.firstName;
    }
  } catch {
    // ignore
  }

  const stats = liveStats ?? EMPTY_STATS;

  if (isError && !liveStats) {
    return (
      <div className="flex flex-col items-center justify-center py-24">
        <AlertTriangle size={48} className="text-red-400 mb-4" />
        <h2 className="text-lg font-medium text-gray-800 mb-2">Failed to load dashboard</h2>
        <p className="text-sm text-gray-500 mb-6">Could not fetch dashboard data. Please try again.</p>
        <button
          onClick={() => refetch()}
          className="flex items-center gap-2 bg-primary-600 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-700 transition-colors"
        >
          <RefreshCw size={14} /> Retry
        </button>
      </div>
    );
  }

  const updatedMinsAgo = lastUpdated
    ? Math.floor((Date.now() - lastUpdated) / 60_000)
    : null;


  return (
    <div>
      {/* Header: greeting + last updated */}
      <div className="flex items-start justify-between mb-2">
        <div className="flex-1">
          <WelcomeSection
            firstName={user.firstName}
            role={role}
            pendingCount={stats.pendingApprovalsCount}
            lowStockCount={stats.lowStockCount}
            isProjectManager={role === 'ProjectManager'}
          />
        </div>
        <div className="flex items-center gap-2 mt-1">
          {updatedMinsAgo !== null && !isLoading && (
            <span className="text-xs text-gray-400">
              Updated {updatedMinsAgo === 0 ? 'just now' : `${updatedMinsAgo} min ago`}
            </span>
          )}
          {isLoading && <span className="text-xs text-gray-400">Refreshing...</span>}
          <button
            onClick={() => refetch()}
            disabled={isLoading}
            className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-gray-600 transition-colors disabled:opacity-50"
            title="Refresh dashboard"
          >
            <RefreshCw size={14} className={isLoading ? 'animate-spin' : ''} />
          </button>
        </div>
      </div>

      {/* Section 2: KPI Cards */}
      <KpiSection
        role={role}
        stats={{
          activeProjects: stats.activeProjects,
          totalProjectValue: stats.totalProjectValue,
          totalEmployees: stats.totalEmployees,
          newEmployeesThisMonth: stats.newEmployeesThisMonth,
          inventoryItems: stats.inventoryItems,
          lowStockCount: stats.lowStockCount,
          openPOs: stats.openPOs,
          openPOValue: stats.openPOValue,
          monthlyPayroll: stats.monthlyPayroll,
          payrollDueInDays: stats.payrollDueInDays,
        }}
        isLoading={isLoading}
      />

      {/* Section 3: Charts */}
      <ChartsSection
        leftType={visibility.leftChartType}
        rightType={visibility.rightChartType}
        projectProgress={stats.projectProgress}
        monthlyExpense={stats.monthlyExpenseVsBudget}
        budgetUtilization={stats.budgetUtilization}
        isLoading={isLoading}
      />

      {/* Section 4: 3-column cards */}
      <ThreeColumnSection
        approvals={stats.pendingApprovalDetails}
        lowStockItems={stats.lowStockAlerts}
        activities={stats.recentActivities}
        visibility={visibility}
        isLoading={isLoading}
      />

      {/* Section 5: Quick Access */}
      <QuickAccessSection modules={visibility.visibleModules} />

      {/* Section 6: Deadlines */}
      <DeadlinesSection stats={stats} isLoading={isLoading} />
    </div>
  );
}
