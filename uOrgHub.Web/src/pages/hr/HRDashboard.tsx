import { useNavigate } from "react-router-dom";
import {
  Users,
  Building2,
  Briefcase,
  CalendarClock,
  Clock,
  Wallet,
  UserCheck,
  Target,
} from "lucide-react";
import StatCard from "../../components/shared/StatCard";

export default function HRDashboard() {
  const navigate = useNavigate();
  const modules = [
    { name: "Departments", path: "/hr/departments", icon: Building2, color: "bg-blue-500" },
    { name: "Employees", path: "/hr/employees", icon: Users, color: "bg-green-500" },
    { name: "Designations", path: "/hr/designations", icon: Briefcase, color: "bg-purple-500" },
    { name: "Leave", path: "/hr/leave", icon: CalendarClock, color: "bg-yellow-500" },
    { name: "Attendance", path: "/hr/attendance", icon: Clock, color: "bg-orange-500" },
    { name: "Payroll", path: "/hr/payroll", icon: Wallet, color: "bg-cyan-500" },
    { name: "Recruitment", path: "/hr/recruitment", icon: UserCheck, color: "bg-pink-500" },
    { name: "Performance", path: "/hr/performance", icon: Target, color: "bg-indigo-500" },
  ];

  const stats = [
    { label: "Total Employees", value: "245", sub: "+12%" },
    { label: "Departments", value: "8", sub: "+2" },
    { label: "Open Positions", value: "15", sub: "-3" },
    { label: "Pending Leave", value: "23", sub: "+5" },
    { label: "Active Payroll Cycles", value: "2", sub: "" },
    { label: "Active Training", value: "5", sub: "" },
  ];

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">HR & Payroll</h1>
        <p className="text-sm text-gray-400">Human resources management module</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
        {stats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} sub={stat.sub} />
        ))}
      </div>

      <div className="mb-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">HR Modules</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {modules.map((mod) => (
            <button
              key={mod.path}
              onClick={() => navigate(mod.path)}
              className="bg-white border border-gray-200 rounded-xl p-4 text-left hover:border-primary-500 hover:shadow-md transition-all group"
            >
              <div className={`w-10 h-10 ${mod.color} rounded-lg flex items-center justify-center mb-3`}>
                <mod.icon size={20} className="text-white" />
              </div>
              <h3 className="text-sm font-medium text-gray-900 group-hover:text-primary-600">
                {mod.name}
              </h3>
              <p className="text-xs text-gray-400 mt-1">
                Manage {mod.name.toLowerCase()}
              </p>
            </button>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">Recent Activities</h3>
          <div className="space-y-3">
            {[
              { text: "John Doe added to Engineering department", time: "2 hours ago" },
              { text: "Leave request approved for Jane Smith", time: "4 hours ago" },
              { text: "New payroll cycle created for May 2024", time: "5 hours ago" },
              { text: "Interview scheduled for Mark Wilson", time: "6 hours ago" },
              { text: "Performance review completed for Sarah", time: "1 day ago" },
            ].map((item, idx) => (
              <div key={idx} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                <p className="text-sm text-gray-700">{item.text}</p>
                <span className="text-xs text-gray-400">{item.time}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">Quick Stats</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Employees this month</span>
              <span className="text-sm font-medium text-gray-900">+12</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Leave requests pending</span>
              <span className="text-sm font-medium text-gray-900">23</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Active job postings</span>
              <span className="text-sm font-medium text-gray-900">8</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Attendance rate (this month)</span>
              <span className="text-sm font-medium text-green-600">94.5%</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Open positions</span>
              <span className="text-sm font-medium text-gray-900">15</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}