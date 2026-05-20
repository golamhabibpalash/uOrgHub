import { useState, useEffect, useCallback, useRef } from "react";
import { NavLink, useLocation } from "react-router-dom";
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
  LayoutDashboard,
  ShieldCheck,
  ScrollText,
} from "lucide-react";
import { useAuthStore } from "../../store/authStore";

interface NavItem {
  label: string;
  path: string;
  icon: React.ComponentType<{ size?: number }>;
  subItems?: { label: string; path: string; icon: React.ComponentType<{ size?: number }> }[];
}

interface NavPolicy {
  autoCollapseSiblings: boolean;
  defaultExpanded: string[];
  persistState: boolean;
}

const defaultPolicy: NavPolicy = {
  autoCollapseSiblings: true,
  defaultExpanded: [],
  persistState: true,
};

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

const projectsSubItems = [
  { label: "All Projects", path: "/projects", icon: HardHat },
  { label: "Clients", path: "/projects/clients", icon: Users },
];

const navItems: NavItem[] = [
  { label: "Dashboard", path: "/dashboard", icon: LayoutDashboard },
  { label: "HR & Payroll", path: "/hr", icon: Users, subItems: hrSubItems },
  { label: "Accounts", path: "/accounts", icon: Receipt, subItems: accountsSubItems },
  { label: "Inventory", path: "/inventory", icon: Box, subItems: inventorySubItems },
  { label: "Procurement", path: "/procurement", icon: ShoppingCart, subItems: procurementSubItems },
  { label: "Projects", path: "/projects", icon: HardHat, subItems: projectsSubItems },
];

const SIDEBAR_KEY = "uorghub-sidebar-state";

interface PersistedState {
  isCollapsed: boolean;
  toggleParity: Record<string, number>;
}

function loadPersisted(): PersistedState {
  try {
    const raw = localStorage.getItem(SIDEBAR_KEY);
    if (raw) {
      const parsed = JSON.parse(raw);
      return {
        isCollapsed: !!parsed.isCollapsed,
        toggleParity: typeof parsed.toggleParity === "object" && parsed.toggleParity !== null ? parsed.toggleParity : {},
      };
    }
  } catch { /* ignore */ }
  return { isCollapsed: false, toggleParity: {} };
}

export default function Sidebar() {
  const location = useLocation();
  const authUser = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const hasClaim = useAuthStore((s) => s.hasClaim);
  const hasRole = useAuthStore((s) => s.hasRole);
  const displayName = authUser ? `${authUser.firstName} ${authUser.lastName}`.trim() : "Admin";
  const initials = displayName.split(" ").map((n) => n[0]).join("").toUpperCase().slice(0, 2);
  const roleLabel = authUser?.roles?.[0] ?? "User";
  const canManageUsers = hasClaim("Users.View") || hasRole("Admin");

  const [policy] = useState<NavPolicy>(defaultPolicy);
  const [isCollapsed, setIsCollapsed] = useState(() => loadPersisted().isCollapsed);
  const [toggleParity, setToggleParity] = useState<Record<string, number>>(() => loadPersisted().toggleParity);
  const prevPathRef = useRef(location.pathname);

  useEffect(() => {
    if (policy.persistState) {
      localStorage.setItem(SIDEBAR_KEY, JSON.stringify({ isCollapsed, toggleParity }));
    }
  }, [isCollapsed, toggleParity, policy.persistState]);

  useEffect(() => {
    if (prevPathRef.current !== location.pathname) {
      prevPathRef.current = location.pathname;
      setToggleParity({});
    }
  }, [location.pathname]);

  const isRouteActiveFor = useCallback(
    (subItems: { path: string }[]) =>
      subItems.some(
        (item) =>
          location.pathname === item.path ||
          location.pathname.startsWith(item.path + "/")
      ),
    [location.pathname]
  );

  const isOpen = useCallback(
    (label: string, subItems: { path: string }[]) => {
      const parity = toggleParity[label] ?? (policy.defaultExpanded.includes(label) ? 1 : 0);
      return isRouteActiveFor(subItems) ? parity % 2 === 0 : parity % 2 === 1;
    },
    [toggleParity, isRouteActiveFor, policy.defaultExpanded]
  );

  const toggleModule = useCallback(
    (label: string, subItems: { path: string }[]) => {
      setToggleParity((prev) => {
        const next: Record<string, number> = { ...prev };
        next[label] = (prev[label] ?? 0) + 1;

        if (policy.autoCollapseSiblings) {
          navItems.forEach((item) => {
            if (item.subItems && item.label !== label) {
              next[item.label] = isRouteActiveFor(item.subItems) ? 1 : 0;
            }
          });
        }

        return next;
      });
    },
    [policy.autoCollapseSiblings, isRouteActiveFor]
  );

  const toggleCollapse = () => setIsCollapsed((prev) => !prev);

  return (
    <aside
      className={`${
        isCollapsed ? "w-16 min-w-16" : "w-60 min-w-60"
      } bg-sidebar flex flex-col h-screen transition-all duration-300 relative group`}
      role="navigation"
      aria-label="Main navigation"
    >
      <div className={`px-4 py-5 border-b border-white/10 ${isCollapsed ? "flex justify-center" : ""}`}>
        {!isCollapsed ? (
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center">
              <Building2 size={18} className="text-white" />
            </div>
            <div>
              <p className="text-slate-100 text-sm font-medium">uOrgHub</p>
              <p className="text-slate-500 text-xs">Civil ERP</p>
            </div>
          </div>
        ) : (
          <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center">
            <Building2 size={18} className="text-white" />
          </div>
        )}
      </div>

      <button
        onClick={toggleCollapse}
        className="absolute -right-3 top-20 w-6 h-6 bg-primary-500 rounded-full flex items-center justify-center text-white shadow-md hover:bg-primary-600 transition-colors z-10"
        title={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        {isCollapsed ? <ChevronRight size={14} /> : <ChevronLeft size={14} />}
      </button>

      <nav className="flex-1 px-2 py-3 overflow-y-auto overflow-x-hidden">
        {!isCollapsed && (
          <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 tracking-widest">MODULES</p>
        )}
        {isCollapsed && <div className="h-4" />}

        {navItems.map(({ label, path, icon: Icon, subItems }) => (
          <div key={path} className="mb-1">
            {subItems ? (
              <div>
                <button
                  onClick={() => toggleModule(label, subItems)}
                  className={`w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm transition-colors cursor-pointer text-left ${
                    isCollapsed ? "justify-center" : ""
                  } ${
                    isOpen(label, subItems) ? "text-slate-200" : "text-slate-400"
                  } hover:text-slate-200 hover:bg-white/5`}
                  aria-expanded={isOpen(label, subItems)}
                  aria-label={`${label} module`}
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
                      <span className="flex-1 truncate">{label}</span>
                      <ChevronDown
                        size={14}
                        className={`shrink-0 transition-transform duration-200 ${
                          isOpen(label, subItems) ? "rotate-180" : ""
                        }`}
                      />
                    </>
                  )}
                </button>
                <div
                  className={`overflow-hidden transition-all duration-200 ease-in-out ${
                    isOpen(label, subItems) && !isCollapsed ? "max-h-[800px] opacity-100" : "max-h-0 opacity-0"
                  }`}
                >
                  <div className="pt-1">
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
                        <span className="truncate">{subLabel}</span>
                      </NavLink>
                    ))}
                  </div>
                </div>
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
                    <span className="truncate">{label}</span>
                  </>
                )}
              </NavLink>
            )}
          </div>
        ))}

        {canManageUsers && (
          <>
            {!isCollapsed && (
              <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 pt-4 tracking-widest">ADMIN</p>
            )}
            {isCollapsed && <div className="h-2" />}
            {[
              { label: "Users", path: "/admin/users", icon: Users },
              { label: "Roles", path: "/admin/roles", icon: ShieldCheck },
              { label: "Access Logs", path: "/admin/access-logs", icon: ScrollText },
              { label: "Company", path: "/admin/company", icon: Building2 },
            ].map(({ label, path, icon: Icon }) => (
              <NavLink key={path} to={path}
                className={({ isActive }) => `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors ${isCollapsed ? "justify-center" : ""} ${isActive ? "bg-primary-500 text-white font-medium" : "text-slate-400 hover:text-slate-200 hover:bg-white/5"}`}>
                {isCollapsed ? <Icon size={16} /> : <><Icon size={16} /><span className="truncate">{label}</span></>}
              </NavLink>
            ))}
          </>
        )}

        <NavLink to="/profile"
          className={({ isActive }) => `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors ${isCollapsed ? "justify-center" : ""} ${isActive ? "bg-primary-500 text-white font-medium" : "text-slate-400 hover:text-slate-200 hover:bg-white/5"}`}>
          {isCollapsed ? <UserCircle size={16} /> : <><UserCircle size={16} /><span className="truncate">My Profile</span></>}
        </NavLink>

        {!isCollapsed && (
          <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 pt-4 tracking-widest">SYSTEM</p>
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
              <span className="truncate">Settings</span>
            </>
          )}
        </NavLink>
      </nav>

      <div className={`px-3 py-3 border-t border-white/10 flex items-center gap-3 ${isCollapsed ? "justify-center" : ""}`}>
        <div className="w-8 h-8 rounded-full bg-primary-700 flex items-center justify-center text-white text-xs font-medium flex-shrink-0">
          {initials}
        </div>
        {!isCollapsed && (
          <>
            <div className="flex-1 min-w-0">
              <p className="text-slate-100 text-xs font-medium truncate">{displayName}</p>
              <p className="text-slate-500 text-xs truncate">{roleLabel}</p>
            </div>
            <LogOut
              size={16}
              className="text-slate-600 cursor-pointer hover:text-slate-400 flex-shrink-0"
              onClick={logout}
            />
          </>
        )}
        {isCollapsed && (
          <div className="relative">
            <div className="w-8 h-8 rounded-full bg-primary-700 flex items-center justify-center text-white text-xs font-medium cursor-pointer" onClick={logout}>
              {initials}
            </div>
            <div className="absolute left-full ml-2 top-0 bg-slate-800 text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 pointer-events-none z-50 transition-opacity">
              {displayName} ({roleLabel})
            </div>
          </div>
        )}
      </div>
    </aside>
  );
}
