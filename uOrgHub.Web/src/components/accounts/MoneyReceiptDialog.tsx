import { useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Printer } from "lucide-react";
import Modal from "../shared/Modal";
import MoneyReceiptPrintView, { type MoneyReceiptOptions } from "./MoneyReceiptPrintView";
import { getPaymentReceipt } from "../../api/accounts";
import { getMyCompany } from "../../api/company";
import { printDocument } from "../../utils/printDocument";
import { extractApiError } from "../../utils/apiError";

const OPTIONS_KEY = "money-receipt-options";
const DEFAULT_OPTIONS: MoneyReceiptOptions = { billBreakdown: false, wordsAndSignatures: true, officeCopy: true };

// The last choice is a per-user convenience; storage can be unavailable (private mode, blocked).
function loadOptions(): MoneyReceiptOptions {
  try {
    const raw = localStorage.getItem(OPTIONS_KEY);
    return raw ? { ...DEFAULT_OPTIONS, ...JSON.parse(raw) } : DEFAULT_OPTIONS;
  } catch {
    return DEFAULT_OPTIONS;
  }
}
function saveOptions(options: MoneyReceiptOptions) {
  try {
    localStorage.setItem(OPTIONS_KEY, JSON.stringify(options));
  } catch {
    /* ignore */
  }
}

interface MoneyReceiptDialogProps {
  paymentId: string | null;
  onClose: () => void;
}

/** Choose what the receipt shows, preview it, print it. */
export default function MoneyReceiptDialog({ paymentId, onClose }: MoneyReceiptDialogProps) {
  const printRef = useRef<HTMLDivElement>(null);
  const [options, setOptions] = useState<MoneyReceiptOptions>(loadOptions);

  const { data, isLoading, error } = useQuery({
    queryKey: ["payment-receipt", paymentId],
    queryFn: () => getPaymentReceipt(paymentId!),
    enabled: Boolean(paymentId),
  });
  const { data: company } = useQuery({ queryKey: ["my-company"], queryFn: getMyCompany, staleTime: 300000 });

  const receipt = data?.data?.data;

  const toggle = (key: keyof MoneyReceiptOptions) =>
    setOptions((o) => {
      const next = { ...o, [key]: !o[key] };
      saveOptions(next);
      return next;
    });

  return (
    <Modal title={receipt ? `Money Receipt ${receipt.receiptNumber}` : "Money Receipt"} open={paymentId !== null} onClose={onClose} size="5xl">
      <div className="space-y-4">
        <div className="flex flex-wrap items-center gap-x-6 gap-y-2 bg-gray-50 border border-gray-200 rounded-lg px-4 py-3">
          <span className="text-xs font-medium text-gray-500">Include:</span>
          <label className="flex items-center gap-2 text-sm cursor-pointer">
            <input type="checkbox" className="h-4 w-4 accent-primary-500" checked={options.billBreakdown} onChange={() => toggle("billBreakdown")} />
            Bill-wise breakdown
          </label>
          <label className="flex items-center gap-2 text-sm cursor-pointer">
            <input type="checkbox" className="h-4 w-4 accent-primary-500" checked={options.wordsAndSignatures} onChange={() => toggle("wordsAndSignatures")} />
            Amount in words + signatures
          </label>
          <label className="flex items-center gap-2 text-sm cursor-pointer">
            <input type="checkbox" className="h-4 w-4 accent-primary-500" checked={options.officeCopy} onChange={() => toggle("officeCopy")} />
            Office copy (counterfoil)
          </label>
          <button
            onClick={() => printRef.current && receipt && printDocument(`Money Receipt ${receipt.receiptNumber}`, printRef.current)}
            disabled={!receipt}
            className="ml-auto flex items-center gap-1.5 px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
          >
            <Printer size={14} /> Print
          </button>
        </div>

        {isLoading ? (
          <p className="text-sm text-gray-400 py-8 text-center">Loading receipt…</p>
        ) : error ? (
          <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{extractApiError(error)}</p>
        ) : receipt ? (
          <div className="border border-gray-200 rounded-xl p-6 overflow-x-auto">
            <MoneyReceiptPrintView
              ref={printRef}
              receipt={receipt}
              options={options}
              companyName={company?.name}
              companyAddress={company?.address}
              companyPhone={company?.phone}
              companyLogoUrl={company?.logoUrl}
            />
          </div>
        ) : null}
      </div>
    </Modal>
  );
}
