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
  Palette,
} from "lucide-react";
import { useAuthStore } from "../../store/authStore";
import { getMenuItems, type MenuItemDto } from "../../api/auth";

const iconMap: Record<string, React.ComponentType<{ size?: number; className?: string }>> = {
  LayoutDashboard, Users, Receipt, Box, ShoppingCart, HardHat,
  Building2, UserCircle, Briefcase, CalendarClock, Clock, Wallet,
  UserCheck, Target, ShieldCheck, ScrollText, Palette, Settings,
  BookOpen, Calendar, Layers, MapPin, Percent, Landmark, FileText,
  CreditCard, PiggyBank, FileSpreadsheet, ShoppingBag, Tag, Ruler,
  Package, Warehouse, ArrowDownToLine, ArrowUpFromLine,
};

interface NavItem {
  label: string;
  path?: string;
  icon: React.ComponentType<{ size?: number; className?: string }>;
  subItems?: { label: string; path: string; icon: React.ComponentType<{ size?: number; className?: string }> }[];
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

function dtoToNavItem(dto: MenuItemDto): NavItem {
  return {
    label: dto.label,
    path: dto.path,
    icon: dto.icon ? iconMap[dto.icon] ?? (() => null) : (() => null),
    subItems: dto.children?.filter(c => c.path).map(c => ({
      label: c.label,
      path: c.path!,
      icon: c.icon ? iconMap[c.icon] ?? (() => null) : (() => null),
    })),
  };
}

export default function Sidebar() {
  const location = useLocation();
  const authUser = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const displayName = authUser ? `${authUser.firstName} ${authUser.lastName}`.trim() : "Admin";
  const initials = displayName.split(" ").map((n) => n[0]).join("").toUpperCase().slice(0, 2);
  const roleLabel = authUser?.roles?.[0] ?? "User";

  const [menuItems, setMenuItems] = useState<MenuItemDto[]>([]);
  const [loaded, setLoaded] = useState(false);
  const [policy] = useState<NavPolicy>(defaultPolicy);
  const [isCollapsed, setIsCollapsed] = useState(() => loadPersisted().isCollapsed);
  const [toggleParity, setToggleParity] = useState<Record<string, number>>(() => loadPersisted().toggleParity);
  const prevPathRef = useRef(location.pathname);

  useEffect(() => {
    getMenuItems().then(setMenuItems).catch(() => setMenuItems([])).finally(() => setLoaded(true));
  }, []);

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
    (label: string) => {
      setToggleParity((prev) => {
        const next: Record<string, number> = { ...prev };
        next[label] = (prev[label] ?? 0) + 1;

        if (policy.autoCollapseSiblings) {
          const siblings: NavItem[] = [];
          for (const item of menuItems) {
            const nav = dtoToNavItem(item);
            if (nav.subItems && nav.label !== label) {
              siblings.push(nav);
            }
          }
          siblings.forEach((item) => {
            if (item.subItems) {
              next[item.label] = isRouteActiveFor(item.subItems) ? 1 : 0;
            }
          });
        }

        return next;
      });
    },
    [policy.autoCollapseSiblings, isRouteActiveFor, menuItems]
  );

  const isParentActive = useCallback(
    (subItems: { path: string }[]) => isRouteActiveFor(subItems),
    [isRouteActiveFor]
  );

  const toggleCollapse = () => setIsCollapsed((prev) => !prev);

  const renderNavItem = (dto: MenuItemDto) => {
    const nav = dtoToNavItem(dto);
    const { label, path, icon: Icon, subItems } = nav;

    if (!subItems || subItems.length === 0) {
      if (!path) return null;
      return (
        <div key={dto.key} className="mb-1">
          <NavLink
            to={path}
            end
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors ${
                isCollapsed ? "justify-center" : ""
              } ${
                isActive
                  ? "bg-primary-500 text-white font-medium"
                  : "sidebar-text-muted sidebar-hover"
              }`
            }
          >
            {isCollapsed ? (
              <div className="relative">
                <Icon size={16} />
                <div className="absolute left-full ml-2 top-0 bg-sidebar text-white text-xs px-2 py-1 rounded whitespace-nowrap opacity-0 group-hover:opacity-100 pointer-events-none z-50 transition-opacity">
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
        </div>
      );
    }

    return (
      <div key={dto.key} className="mb-1">
        <div>
          <button
            onClick={() => toggleModule(label)}
            className={`w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm transition-colors cursor-pointer text-left ${
              isCollapsed ? "justify-center" : ""
            } ${
              isParentActive(subItems)
                ? "bg-primary-500/15 text-primary-300 font-medium"
                : "sidebar-text-muted sidebar-hover"
            }`}
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
                  end={subPath === path}
                  className={({ isActive }) =>
                    `flex items-center gap-3 pl-10 pr-3 py-1.5 rounded-md mb-0.5 text-xs transition-colors ${
                      isActive
                        ? "bg-primary-500 text-white font-medium"
                        : "sidebar-text-muted sidebar-hover"
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
      </div>
    );
  };

  const renderSection = (key: string, label: string | null) => {
    const items = menuItems.filter(m => (m.section ?? "main") === key);
    if (items.length === 0) return null;

    return (
      <div key={key}>
        {label ? (
          !isCollapsed && (
            <p className="sidebar-text-dim text-[10px] font-medium px-2 pb-1 pt-4 tracking-widest">{label}</p>
          )
        ) : (
          !isCollapsed && <div className="h-3" />
        )}
        {isCollapsed && label !== null && <div className="h-2" />}
        {items.map(renderNavItem)}
      </div>
    );
  };

  if (!loaded) {
    return (
      <aside className={`${isCollapsed ? "w-16 min-w-16" : "w-60 min-w-60"} bg-sidebar flex flex-col h-screen transition-all duration-300 relative`} />
    );
  }

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
            <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center overflow-hidden">
              <img src="/logo.png" alt="uOrgHub" className="h-6 w-6 object-contain" />
            </div>
            <div>
              <p className="sidebar-text text-sm font-medium">uOrgHub</p>
              <p className="sidebar-text-dim text-xs">Civil ERP</p>
            </div>
          </div>
        ) : (
          <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center overflow-hidden">
            <img src="/logo.png" alt="uOrgHub" className="h-5 w-5 object-contain" />
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
        {renderSection("main", "MODULES")}
        {renderSection("admin", "ADMIN")}
        {renderSection("profile", null)}
        {renderSection("system", "SYSTEM")}
      </nav>

      <div className={`px-3 py-3 border-t border-white/10 flex items-center gap-3 ${isCollapsed ? "justify-center" : ""}`}>
        <div className="w-8 h-8 rounded-full bg-primary-700 flex items-center justify-center text-white text-xs font-medium flex-shrink-0">
          {initials}
        </div>
        {!isCollapsed && (
          <>
            <div className="flex-1 min-w-0">
              <p className="sidebar-text text-xs font-medium truncate">{displayName}</p>
              <p className="sidebar-text-dim text-xs truncate">{roleLabel}</p>
            </div>
            <LogOut
              size={16}
              className="sidebar-text-dim cursor-pointer sidebar-hover flex-shrink-0 rounded p-0.5"
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
