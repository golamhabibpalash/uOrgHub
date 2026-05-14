import { useState, useEffect } from "react";
import { NavLink } from "react-router-dom";
import {
  Users,
  Receipt,
  Box,
  ShoppingCart,
  HardHat,
  Settings,
  LogOut,
  Building2,
  UserCircle,
  Briefcase,
  CalendarClock,
  Clock,
  Wallet,
  UserCheck,
  Target,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  BookOpen,
  Calendar,
  Layers,
  MapPin,
  Percent,
  Landmark,
  FileText,
  CreditCard,
  PiggyBank,
  FileSpreadsheet,
  ShoppingBag,
  Tag,
  Ruler,
  Package,
  Warehouse,
  ArrowDownToLine,
  ArrowUpFromLine,
} from "lucide-react";

const hrSubItems = [
  { label: "Dashboard", path: "/hr", icon: Users },
  { label: "Departments", path: "/hr/departments", icon: Building2 },
  { label: "Employees", path: "/hr/employees", icon: UserCircle },
  { label: "Designations", path: "/hr/designations", icon: Briefcase },
  { label: "Leave", path: "/hr/leave", icon: CalendarClock },
  { label: "Attendance", path: "/hr/attendance", icon: Clock },
  { label: "Payroll", path: "/hr/payroll", icon: Wallet },
  { label: "Recruitment", path: "/hr/recruitment", icon: UserCheck },
  { label: "Performance", path: "/hr/performance", icon: Target },
];

const accountsSubItems = [
  { label: "Dashboard", path: "/accounts", icon: Receipt },
  { label: "Account Groups", path: "/accounts/account-groups", icon: Layers },
  { label: "Fiscal Years", path: "/accounts/fiscal-years", icon: Calendar },
  { label: "Chart of Accounts", path: "/accounts/chart-of-accounts", icon: BookOpen },
  { label: "Journal Entries", path: "/accounts/journal-entries", icon: FileSpreadsheet },
  { label: "Cost Centers", path: "/accounts/cost-centers", icon: MapPin },
  { label: "Tax Rates", path: "/accounts/tax-rates", icon: Percent },
  { label: "Bank Accounts", path: "/accounts/bank-accounts", icon: Landmark },
  { label: "Customers", path: "/accounts/customers", icon: Users },
  { label: "Invoices", path: "/accounts/invoices", icon: FileText },
  { label: "Vendors", path: "/accounts/vendors", icon: ShoppingBag },
  { label: "Bills", path: "/accounts/bills", icon: FileText },
  { label: "Payments", path: "/accounts/payments", icon: CreditCard },
  { label: "Budgets", path: "/accounts/budgets", icon: PiggyBank },
];

const inventorySubItems = [
  { label: "Dashboard", path: "/inventory", icon: Box },
  { label: "Inventory Types", path: "/inventory/types", icon: Tag },
  { label: "Categories", path: "/inventory/categories", icon: Package },
  { label: "Units of Measure", path: "/inventory/units-of-measure", icon: Ruler },
  { label: "Attributes", path: "/inventory/attributes", icon: Tag },
  { label: "Items", path: "/inventory/items", icon: Package },
  { label: "Item Variants", path: "/inventory/item-variants", icon: Package },
  { label: "Warehouses", path: "/inventory/warehouses", icon: Warehouse },
  { label: "Stock Balances", path: "/inventory/stock-balances", icon: ArrowDownToLine },
  { label: "Stock Transactions", path: "/inventory/stock-transactions", icon: ArrowUpFromLine },
];

const procurementSubItems = [
  { label: "Dashboard", path: "/procurement", icon: ShoppingCart },
  { label: "Vendors", path: "/procurement/vendors", icon: Users },
  { label: "Purchase Requisitions", path: "/procurement/purchase-requisitions", icon: FileText },
  { label: "Request for Quotation", path: "/procurement/rfqs", icon: FileSpreadsheet },
  { label: "Vendor Quotations", path: "/procurement/quotations", icon: Tag },
  { label: "Purchase Orders", path: "/procurement/purchase-orders", icon: Package },
  { label: "Goods Received Notes", path: "/procurement/grns", icon: ArrowDownToLine },
];

const navItems = [
  { label: "HR & Payroll", path: "/hr", icon: Users, subItems: hrSubItems },
  { label: "Accounts", path: "/accounts", icon: Receipt, subItems: accountsSubItems },
  { label: "Inventory", path: "/inventory", icon: Box, subItems: inventorySubItems },
  { label: "Procurement", path: "/procurement", icon: ShoppingCart, subItems: procurementSubItems },
  { label: "Projects", path: "/projects", icon: HardHat },
];

const STORAGE_KEY = "uorghub-sidebar-state";

interface SidebarState {
  isCollapsed: boolean;
  expandedModules: Record<string, boolean>;
}

function getStoredState(): SidebarState {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      return JSON.parse(stored);
    }
  } catch (e) {
    console.error("Failed to parse sidebar state:", e);
  }
  return { isCollapsed: false, expandedModules: { "HR & Payroll": true } };
}

function storeState(state: SidebarState) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

export default function Sidebar() {
  const user = { name: "Admin", initials: "AD" };
  const [state, setState] = useState<SidebarState>(getStoredState);

  useEffect(() => {
    storeState(state);
  }, [state]);

  const toggleCollapse = () => {
    setState((prev) => ({ ...prev, isCollapsed: !prev.isCollapsed }));
  };

  const toggleModule = (label: string) => {
    setState((prev) => ({
      ...prev,
      expandedModules: {
        ...prev.expandedModules,
        [label]: !prev.expandedModules[label],
      },
    }));
  };

  const { isCollapsed, expandedModules } = state;

  return (
    <aside
      className={`${
        isCollapsed ? "w-16 min-w-16" : "w-60 min-w-60"
      } bg-sidebar flex flex-col h-screen transition-all duration-300 relative group`}
    >
      <div className={`px-4 py-5 border-b border-white/10 ${isCollapsed ? "flex justify-center" : ""}`}>
        {!isCollapsed && (
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center">
              <Building2 size={18} className="text-white" />
            </div>
            <div>
              <p className="text-slate-100 text-sm font-medium">uOrgHub</p>
              <p className="text-slate-500 text-xs">Civil ERP</p>
            </div>
          </div>
        )}
        {isCollapsed && (
          <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center">
            <Building2 size={18} className="text-white" />
          </div>
        )}
      </div>

      <button
        onClick={toggleCollapse}
        className="absolute -right-3 top-20 w-6 h-6 bg-primary-500 rounded-full flex items-center justify-center text-white shadow-md hover:bg-primary-600 transition-colors z-10"
        title={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        {isCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
      </button>

      <nav className="flex-1 px-2 py-3 overflow-y-auto overflow-x-hidden">
        {!isCollapsed && (
          <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 tracking-widest">
            MODULES
          </p>
        )}
        {isCollapsed && <div className="h-4" />}
        {navItems.map(({ label, path, icon: Icon, subItems }) => (
          <div key={path} className="mb-1">
            {subItems ? (
              <div>
                <div
                  onClick={() => toggleModule(label)}
                  className={`flex items-center gap-3 px-3 py-2 rounded-md text-sm text-slate-400 cursor-pointer hover:text-slate-200 hover:bg-white/5 ${
                    isCollapsed ? "justify-center" : ""
                  }`}
                >
                  {isCollapsed ? (
                    <div className="relative">
                      <Icon size={16} />
                      <div className="absolute left-full ml-2 top-0 bg-slate-800 text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 pointer-events-none z-50 transition-opacity">
                        {label}
                      </div>
                    </div>
                  ) : (
                    <>
                      <Icon size={16} />
                      <span className="flex-1">{label}</span>
                      <ChevronDown
                        size={14}
                        className={`transition-transform ${
                          expandedModules[label] ? "rotate-180" : ""
                        }`}
                      />
                    </>
                  )}
                </div>
                {expandedModules[label] && !isCollapsed && (
                  <div className="overflow-hidden transition-all">
                    {subItems.map(({ label: subLabel, path: subPath, icon: SubIcon }) => (
                      <NavLink
                        key={subPath}
                        to={subPath}
                        className={({ isActive }) =>
                          `flex items-center gap-3 pl-10 pr-3 py-1.5 rounded-md mb-0.5 text-xs transition-colors ${
                            isActive
                              ? "bg-primary-500 text-white font-medium"
                              : "text-slate-400 hover:text-slate-200 hover:bg-white/5"
                          }`
                        }
                      >
                        <SubIcon size={14} />
                        {subLabel}
                      </NavLink>
                    ))}
                  </div>
                )}
              </div>
            ) : (
              <NavLink
                to={path}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors ${
                    isCollapsed ? "justify-center" : ""
                  } ${
                    isActive
                      ? "bg-primary-500 text-white font-medium"
                      : "text-slate-400 hover:text-slate-200 hover:bg-white/5"
                  }`
                }
              >
                {isCollapsed ? (
                  <div className="relative">
                    <Icon size={16} />
                    <div className="absolute left-full ml-2 top-0 bg-slate-800 text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 pointer-events-none z-50 transition-opacity">
                      {label}
                    </div>
                  </div>
                ) : (
                  <>
                    <Icon size={16} />
                    {label}
                  </>
                )}
              </NavLink>
            )}
          </div>
        ))}

        {!isCollapsed && (
          <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 pt-4 tracking-widest">
            SYSTEM
          </p>
        )}
        {isCollapsed && <div className="h-4" />}
        <NavLink
          to="/settings"
          className={({ isActive }) =>
            `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors ${
              isCollapsed ? "justify-center" : ""
            } ${
              isActive
                ? "bg-primary-500 text-white font-medium"
                : "text-slate-400 hover:text-slate-200 hover:bg-white/5"
            }`
          }
        >
          {isCollapsed ? (
            <div className="relative">
              <Settings size={16} />
              <div className="absolute left-full ml-2 top-0 bg-slate-800 text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 pointer-events-none z-50 transition-opacity">
                Settings
              </div>
            </div>
          ) : (
            <>
              <Settings size={16} />
              Settings
            </>
          )}
        </NavLink>
      </nav>

      <div className={`px-3 py-3 border-t border-white/10 flex items-center gap-3 ${isCollapsed ? "justify-center" : ""}`}>
        <div className="w-8 h-8 rounded-full bg-primary-700 flex items-center justify-center text-white text-xs font-medium">
          {user.initials}
        </div>
        {!isCollapsed && (
          <>
            <div className="flex-1 min-w-0">
              <p className="text-slate-100 text-xs font-medium truncate">
                {user.name}
              </p>
              <p className="text-slate-500 text-xs">Administrator</p>
            </div>
            <LogOut
              size={16}
              className="text-slate-600 cursor-pointer hover:text-slate-400"
            />
          </>
        )}
        {isCollapsed && (
          <div className="relative">
            <div className="w-8 h-8 rounded-full bg-primary-700 flex items-center justify-center text-white text-xs font-medium cursor-pointer">
              {user.initials}
            </div>
            <div className="absolute left-full ml-2 top-0 bg-slate-800 text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 pointer-events-none z-50 transition-opacity">
              {user.name} (Administrator)
            </div>
          </div>
        )}
      </div>
    </aside>
  );
}