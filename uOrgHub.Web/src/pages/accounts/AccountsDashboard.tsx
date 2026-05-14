import { useNavigate } from "react-router-dom";
import {
  BookOpen,
  Calendar,
  Layers,
  MapPin,
  Percent,
  Landmark,
  Users,
  FileText,
  ShoppingBag,
  CreditCard,
  PiggyBank,
  FileSpreadsheet,
} from "lucide-react";
import StatCard from "../../components/shared/StatCard";

export default function AccountsDashboard() {
  const navigate = useNavigate();

  const modules = [
    { name: "Account Groups", path: "/accounts/account-groups", icon: Layers, color: "bg-slate-500" },
    { name: "Fiscal Years", path: "/accounts/fiscal-years", icon: Calendar, color: "bg-indigo-500" },
    { name: "Chart of Accounts", path: "/accounts/chart-of-accounts", icon: BookOpen, color: "bg-blue-500" },
    { name: "Journal Entries", path: "/accounts/journal-entries", icon: FileSpreadsheet, color: "bg-violet-500" },
    { name: "Cost Centers", path: "/accounts/cost-centers", icon: MapPin, color: "bg-teal-500" },
    { name: "Tax Rates", path: "/accounts/tax-rates", icon: Percent, color: "bg-orange-500" },
    { name: "Bank Accounts", path: "/accounts/bank-accounts", icon: Landmark, color: "bg-cyan-500" },
    { name: "Customers", path: "/accounts/customers", icon: Users, color: "bg-green-500" },
    { name: "Invoices", path: "/accounts/invoices", icon: FileText, color: "bg-emerald-500" },
    { name: "Vendors", path: "/accounts/vendors", icon: ShoppingBag, color: "bg-yellow-500" },
    { name: "Bills", path: "/accounts/bills", icon: FileText, color: "bg-red-500" },
    { name: "Payments", path: "/accounts/payments", icon: CreditCard, color: "bg-pink-500" },
    { name: "Budgets", path: "/accounts/budgets", icon: PiggyBank, color: "bg-purple-500" },
  ];

  const stats = [
    { label: "Active Bank Accounts", value: "4", sub: "" },
    { label: "Open Invoices", value: "18", sub: "+3" },
    { label: "Unpaid Bills", value: "12", sub: "-2" },
    { label: "Total Customers", value: "56", sub: "+5" },
    { label: "Total Vendors", value: "34", sub: "" },
    { label: "Draft Budgets", value: "2", sub: "" },
  ];

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Accounts</h1>
        <p className="text-sm text-gray-400">Financial management and accounting module</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
        {stats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} sub={stat.sub} />
        ))}
      </div>

      <div className="mb-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Accounting Modules</h2>
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
              <p className="text-xs text-gray-400 mt-1">Manage {mod.name.toLowerCase()}</p>
            </button>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">Recent Transactions</h3>
          <div className="space-y-3">
            {[
              { text: "Invoice INV-2026-001 created for ABC Corp", time: "2 hours ago", color: "text-green-600" },
              { text: "Payment received BDT 50,000 from XYZ Ltd", time: "4 hours ago", color: "text-blue-600" },
              { text: "Bill BL-2026-005 approved from Supplier Co", time: "5 hours ago", color: "text-orange-600" },
              { text: "Journal entry posted for May payroll", time: "1 day ago", color: "text-gray-600" },
              { text: "Budget Q2 2026 approved", time: "2 days ago", color: "text-purple-600" },
            ].map((item, idx) => (
              <div key={idx} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                <p className={`text-sm ${item.color}`}>{item.text}</p>
                <span className="text-xs text-gray-400 ml-4 whitespace-nowrap">{item.time}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-5">
          <h3 className="text-sm font-medium text-gray-900 mb-4">Financial Summary</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Total Receivables</span>
              <span className="text-sm font-medium text-green-600">BDT 12,50,000</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Total Payables</span>
              <span className="text-sm font-medium text-red-500">BDT 8,20,000</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Bank Balance</span>
              <span className="text-sm font-medium text-blue-600">BDT 45,30,000</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Overdue Invoices</span>
              <span className="text-sm font-medium text-orange-500">3</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">Overdue Bills</span>
              <span className="text-sm font-medium text-orange-500">2</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
