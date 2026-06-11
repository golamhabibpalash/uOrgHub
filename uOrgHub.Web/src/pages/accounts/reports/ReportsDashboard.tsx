import { BarChart3, BookOpen, Scale, DollarSign, FileText, CalendarDays, Layers, BookType, PieChart, Clock, AlertTriangle } from "lucide-react";
import { useNavigate } from "react-router-dom";

const reportCards = [
  { label: "Trial Balance", desc: "Account-wise debit/credit balances", icon: Scale, color: "bg-blue-50 text-blue-600", path: "trial-balance" },
  { label: "Income Statement", desc: "Revenue, expenses & net profit/loss", icon: DollarSign, color: "bg-green-50 text-green-600", path: "income-statement" },
  { label: "Balance Sheet", desc: "Assets, liabilities & equity position", icon: PieChart, color: "bg-purple-50 text-purple-600", path: "balance-sheet" },
  { label: "General Ledger", desc: "Account-wise ledger summary", icon: BookOpen, color: "bg-indigo-50 text-indigo-600", path: "general-ledger" },
  { label: "Chart of Accounts", desc: "Full account listing with details", icon: BookType, color: "bg-amber-50 text-amber-600", path: "chart-of-accounts" },
  { label: "Journal Entry Report", desc: "Journal entries with filters", icon: FileText, color: "bg-rose-50 text-rose-600", path: "journal-entries" },
  { label: "Account Ledger", desc: "Single account transaction history", icon: Layers, color: "bg-cyan-50 text-cyan-600", path: "account-ledger" },
  { label: "Day Book", desc: "Daily transaction register", icon: CalendarDays, color: "bg-orange-50 text-orange-600", path: "day-book" },
  { label: "Account Group Summary", desc: "Group-wise balance summaries", icon: BarChart3, color: "bg-teal-50 text-teal-600", path: "account-group-summary" },
  { label: "AR Aging", desc: "Outstanding customer invoices aging", icon: Clock, color: "bg-violet-50 text-violet-600", path: "ar-aging" },
  { label: "AP Aging", desc: "Outstanding vendor bills aging", icon: AlertTriangle, color: "bg-pink-50 text-pink-600", path: "ap-aging" },
];

export default function ReportsDashboard() {
  const navigate = useNavigate();

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-base font-medium text-gray-900">Accounting Reports</h2>
        <p className="text-xs text-gray-400">Financial reports, statements, and summaries</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {reportCards.map((card) => (
          <button
            key={card.path}
            onClick={() => navigate(card.path)}
            className="flex items-start gap-4 bg-white border border-gray-200 rounded-xl px-5 py-4 text-left hover:border-primary-300 hover:shadow-sm transition-all"
          >
            <div className={`w-10 h-10 rounded-lg flex items-center justify-center shrink-0 ${card.color}`}>
              <card.icon size={18} />
            </div>
            <div>
              <p className="text-sm font-medium text-gray-900">{card.label}</p>
              <p className="text-xs text-gray-400 mt-0.5">{card.desc}</p>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
