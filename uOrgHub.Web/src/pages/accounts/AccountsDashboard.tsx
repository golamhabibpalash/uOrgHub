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

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Accounts</h1>
        <p className="text-sm text-gray-400">Financial management and accounting module</p>
      </div>

      <div>
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
    </div>
  );
}
