import { useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
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
import { getHRDashboard, type HRDashboardData } from "../../api/hr";

export default function HRDashboard() {
  const navigate = useNavigate();
  const [data, setData] = useState<HRDashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getHRDashboard()
      .then(setData)
      .catch(() => setData(null))
      .finally(() => setLoading(false));
  }, []);

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
    { label: "Total Employees", value: data?.totalEmployees ?? 0, sub: `+${data?.newEmployeesThisMonth ?? 0} this month` },
    { label: "Departments", value: data?.totalDepartments ?? 0, sub: "" },
    { label: "Open Positions", value: data?.openPositions ?? 0, sub: `${data?.activeJobPostings ?? 0} active postings` },
    { label: "Pending Leave", value: data?.pendingLeaveRequests ?? 0, sub: `${data?.leaveRequestsThisMonth ?? 0} this month` },
    { label: "Active Payroll Cycles", value: data?.activePayrollCycles ?? 0, sub: "" },
    { label: "Active Training", value: data?.activeTrainings ?? 0, sub: "" },
  ];

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">HR & Payroll</h1>
        <p className="text-sm text-gray-400">Human resources management module</p>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="bg-white border border-gray-200 rounded-xl p-5 animate-pulse">
              <div className="h-3 bg-gray-200 rounded w-24 mb-3" />
              <div className="h-6 bg-gray-200 rounded w-16 mb-2" />
              <div className="h-3 bg-gray-200 rounded w-20" />
            </div>
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
          {stats.map((stat) => (
            <StatCard key={stat.label} label={stat.label} value={stat.value} sub={stat.sub} />
          ))}
        </div>
      )}

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
          {data?.recentActivities && data.recentActivities.length > 0 ? (
            <div className="space-y-3">
              {data.recentActivities.map((item, idx) => (
                <div key={idx} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                  <p className="text-sm text-gray-700">{item.description}</p>
                  <span className="text-xs text-gray-400">{item.timestamp}</span>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-sm text-gray-400">No recent activities</p>
          )}
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">Quick Stats</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Employees this month</span>
              <span className="text-sm font-medium text-gray-900">+{data?.newEmployeesThisMonth ?? 0}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Leave requests pending</span>
              <span className="text-sm font-medium text-gray-900">{data?.pendingLeaveRequests ?? 0}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Active job postings</span>
              <span className="text-sm font-medium text-gray-900">{data?.activeJobPostings ?? 0}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Attendance rate (this month)</span>
              <span className={`text-sm font-medium ${(data?.attendanceRate ?? 0) >= 80 ? "text-green-600" : "text-yellow-600"}`}>
                {data?.attendanceRate ?? 0}%
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Open positions</span>
              <span className="text-sm font-medium text-gray-900">{data?.openPositions ?? 0}</span>
            </div>
          </div>

          {data?.employeesPerDepartment && data.employeesPerDepartment.length > 0 && (
            <>
              <h4 className="text-xs font-medium text-gray-500 mt-5 mb-3 uppercase tracking-wider">Employees by Department</h4>
              <div className="space-y-2">
                {data.employeesPerDepartment.slice(0, 6).map((dept) => (
                  <div key={dept.departmentName} className="flex items-center justify-between">
                    <span className="text-xs text-gray-600">{dept.departmentName}</span>
                    <div className="flex items-center gap-2">
                      <div className="w-24 h-1.5 bg-gray-100 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-primary-500 rounded-full"
                          style={{ width: `${Math.min((dept.employeeCount / (data.totalEmployees || 1)) * 100, 100)}%` }}
                        />
                      </div>
                      <span className="text-xs font-medium text-gray-900 w-6 text-right">{dept.employeeCount}</span>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
