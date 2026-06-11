import { useQuery } from "@tanstack/react-query";
import { BarChart3, BookOpen, Scale, DollarSign, FileText, CalendarDays, Layers, BookType, PieChart, Clock, AlertTriangle } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { getDashboardSummary } from "../../../api/accounts";

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
  const { data: summary } = useQuery({
    queryKey: ["reports-dashboard-summary"],
    queryFn: getDashboardSummary,
  });

  const s = summary?.data?.data;
  const widgets = s ? [
    { label: "Total Assets", value: s.totalAssets, color: "text-blue-600" },
    { label: "Total Liabilities", value: s.totalLiabilities, color: "text-red-600" },
    { label: "Total Equity", value: s.totalEquity, color: "text-purple-600" },
    { label: "P&L", value: s.currentProfitLoss, color: s.currentProfitLoss >= 0 ? "text-green-600" : "text-red-600" },
    { label: "Journal Entries", value: s.totalJournalEntries, color: "text-gray-600" },
    { label: "Recent (30d)", value: s.recentTransactions, color: "text-gray-600" },
  ] : [];

  const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

  return (
    <div>
      <div className="mb-6">
        <h2 className="text-base font-medium text-gray-900">Accounting Reports</h2>
        <p className="text-xs text-gray-400">Financial reports, statements, and summaries</p>
      </div>

      {widgets.length > 0 && (
        <div className="grid grid-cols-3 lg:grid-cols-6 gap-3 mb-6">
          {widgets.map((w) => (
            <div key={w.label} className="bg-white border border-gray-200 rounded-lg px-4 py-3">
              <p className="text-xs text-gray-500 mb-1">{w.label}</p>
              <p className={`text-sm font-semibold ${w.color}`}>{fmt(w.value)}</p>
            </div>
          ))}
        </div>
      )}

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
