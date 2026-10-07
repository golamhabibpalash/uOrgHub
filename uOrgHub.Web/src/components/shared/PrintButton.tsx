import { useState } from "react";
import { Printer } from "lucide-react";
import toast from "react-hot-toast";
import { openPrintWindow, buildPrintTable, PrintColumn } from "../../utils/print";

interface PrintButtonProps<T> {
  title: string;
  subtitle?: string;
  columns: PrintColumn<T>[];
  /** Fetches the full set of (filtered) rows to print — not just the current page. */
  fetchRows: () => Promise<T[]>;
  disabled?: boolean;
}

export default function PrintButton<T>({ title, subtitle, columns, fetchRows, disabled }: PrintButtonProps<T>) {
  const [isPrinting, setIsPrinting] = useState(false);

  const handlePrint = async () => {
    setIsPrinting(true);
    try {
      const rows = await fetchRows();
      openPrintWindow({ title, subtitle, bodyHtml: buildPrintTable(columns, rows) });
    } catch {
      toast.error("Failed to prepare print view. Please try again.");
    } finally {
      setIsPrinting(false);
    }
  };

  return (
    <button
      onClick={handlePrint}
      disabled={disabled || isPrinting}
      className="flex items-center gap-2 text-sm border border-gray-200 rounded-lg px-3 py-1.5 hover:bg-gray-50 disabled:opacity-50"
    >
      <Printer size={14} />
      {isPrinting ? "Preparing..." : "Print"}
    </button>
  );
}
