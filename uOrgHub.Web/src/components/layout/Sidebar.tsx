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

const navItems = [
  { label: "HR & Payroll", path: "/hr", icon: Users, subItems: hrSubItems },
  { label: "Accounts", path: "/accounts", icon: Receipt },
  { label: "Inventory", path: "/inventory", icon: Box },
  { label: "Procurement", path: "/procurement", icon: ShoppingCart },
  { label: "Projects", path: "/projects", icon: HardHat },
];

export default function Sidebar() {
  const user = { name: "Admin", initials: "AD" };

  return (
    <aside className="w-[240px] min-w-[240px] bg-sidebar flex flex-col h-screen">
      <div className="px-4 py-5 border-b border-white/10">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-lg bg-primary-500 flex items-center justify-center">
            <Building2 size={18} className="text-white" />
          </div>
          <div>
            <p className="text-slate-100 text-sm font-medium">uOrgHub</p>
            <p className="text-slate-500 text-xs">Civil ERP</p>
          </div>
        </div>
      </div>

      <nav className="flex-1 px-2 py-3 overflow-y-auto">
        <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 tracking-widest">
          MODULES
        </p>
        {navItems.map(({ label, path, icon: Icon, subItems }) => (
          <div key={path}>
            {subItems ? (
              <div>
                <div className="flex items-center gap-3 px-3 py-2 rounded-md text-sm text-slate-400">
                  <Icon size={16} />
                  {label}
                </div>
                {subItems.map(({ label: subLabel, path: subPath, icon: SubIcon }) => (
                  <NavLink
                    key={subPath}
                    to={subPath}
                    className={({ isActive }) =>
                      `flex items-center gap-3 pl-10 pr-3 py-1.5 rounded-md mb-0.5 text-xs transition-colors
                       ${
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
            ) : (
              <NavLink
                to={path}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors
                   ${
                     isActive
                       ? "bg-primary-500 text-white font-medium"
                       : "text-slate-400 hover:text-slate-200 hover:bg-white/5"
                   }`
                }
              >
                <Icon size={16} />
                {label}
              </NavLink>
            )}
          </div>
        ))}

        <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 pt-4 tracking-widest">
          SYSTEM
        </p>
        <NavLink
          to="/settings"
          className={({ isActive }) =>
            `flex items-center gap-3 px-3 py-2 rounded-md mb-0.5 text-sm transition-colors
             ${
               isActive
                 ? "bg-primary-500 text-white font-medium"
                 : "text-slate-400 hover:text-slate-200 hover:bg-white/5"
             }`
          }
        >
          <Settings size={16} />
          Settings
        </NavLink>
      </nav>

      <div className="px-3 py-3 border-t border-white/10 flex items-center gap-3">
        <div className="w-8 h-8 rounded-full bg-primary-700 flex items-center justify-center text-white text-xs font-medium">
          {user.initials}
        </div>
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
      </div>
    </aside>
  );
}