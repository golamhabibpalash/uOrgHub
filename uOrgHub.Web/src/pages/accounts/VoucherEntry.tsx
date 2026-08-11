import { useNavigate } from "react-router-dom";
import { ArrowDownLeft, ArrowLeftRight, ArrowUpRight, List } from "lucide-react";
import { VoucherType } from "../../api/accounts";
import { voucherThemes } from "../../components/accounts/voucherTheme";

const icons: Record<VoucherType, typeof ArrowUpRight> = {
  Debit: ArrowUpRight,
  Credit: ArrowDownLeft,
  Contra: ArrowLeftRight,
};

export default function VoucherEntry() {
  const navigate = useNavigate();
  const types: VoucherType[] = ["Debit", "Credit", "Contra"];

  return (
    <div className="p-6">
      <div className="mb-6 flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Voucher Entry</h1>
          <p className="text-sm text-gray-400">
            Choose the kind of voucher you are preparing. Saving it records the accounting transaction for you.
          </p>
        </div>
        <button
          onClick={() => navigate("/accounts/vouchers")}
          className="flex items-center gap-2 text-sm border border-gray-200 rounded-lg px-3 py-1.5 hover:bg-gray-50 text-gray-600"
        >
          <List size={14} /> All Vouchers
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-5 max-w-4xl">
        {types.map((type) => {
          const theme = voucherThemes[type];
          const Icon = icons[type];
          return (
            <button
              key={type}
              onClick={() => navigate(`/accounts/vouchers/new?type=${type}`)}
              className={`group relative overflow-hidden bg-white border border-gray-200 rounded-xl p-6 pl-7 text-left transition-all hover:shadow-md hover:ring-4 ${theme.hover}`}
            >
              <span className={`absolute left-0 top-0 bottom-0 w-1.5 ${theme.bar}`} />

              <div className="flex items-start justify-between mb-4">
                <div className={`w-11 h-11 ${theme.solid} rounded-lg flex items-center justify-center`}>
                  <Icon size={22} className="text-white" />
                </div>
                <span className={`text-2xl font-bold tracking-wider px-2.5 py-0.5 rounded-lg border ${theme.chip}`}>
                  {theme.code}
                </span>
              </div>

              <h3 className="text-base font-semibold text-gray-900">{theme.label}</h3>
              <p className="text-sm text-gray-500 mt-0.5">{theme.meaning}</p>

              <p className="text-xs text-gray-400 mt-3 pt-3 border-t border-gray-100">{theme.effect}</p>
            </button>
          );
        })}
      </div>

      <p className="text-xs text-gray-400 mt-6 max-w-4xl">
        Every voucher you save is recorded as a balanced accounting transaction — the system prepares the
        journal entry for you, so the debit and credit sides always match. To move an amount between two
        non-cash heads, or to record anything needing more than one debit and one credit, use{" "}
        <button onClick={() => navigate("/accounts/journal-entries")} className="underline hover:text-gray-600">
          Journal Entries
        </button>{" "}
        instead.
      </p>
    </div>
  );
}
