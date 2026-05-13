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
} from "lucide-react";

const navItems = [
  { label: "HR & Payroll", path: "/hr", icon: Users },
  { label: "Accounts", path: "/accounts", icon: Receipt },
  { label: "Inventory", path: "/inventory", icon: Box },
  { label: "Procurement", path: "/procurement", icon: ShoppingCart },
  { label: "Projects", path: "/projects", icon: HardHat },
];

export default function Sidebar() {
  const user = { name: "Admin", initials: "AD" };

  return (
    <aside className="w-[220px] min-w-[220px] bg-sidebar flex flex-col h-screen">
      {/* Logo */}
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

      {/* Nav */}
      <nav className="flex-1 px-2 py-3 overflow-y-auto">
        <p className="text-slate-600 text-[10px] font-medium px-2 pb-1 tracking-widest">
          MODULES
        </p>
        {navItems.map(({ label, path, icon: Icon }) => (
          <NavLink
            key={path}
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

      {/* User */}
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
