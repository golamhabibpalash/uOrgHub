import { useState, useRef, useEffect } from "react";
import { Download } from "lucide-react";
import { useExport } from "../../hooks/useExport";

interface ExportMenuProps {
  baseUrl: string;
  filters?: Record<string, unknown>;
}

export default function ExportMenu({ baseUrl, filters = {} }: ExportMenuProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const { exportData, isExporting } = useExport();

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleExport = (format: "xlsx" | "csv") => {
    setOpen(false);
    exportData({ baseUrl, format, filters });
  };

  return (
    <div className="relative" ref={ref}>
      <button
        onClick={() => setOpen(!open)}
        disabled={isExporting}
        className="flex items-center gap-2 text-sm border border-gray-200 rounded-lg px-3 py-1.5 hover:bg-gray-50 disabled:opacity-50"
      >
        <Download size={14} />
        {isExporting ? "Exporting..." : "Export"}
      </button>
      {open && (
        <div className="absolute right-0 mt-1 w-36 bg-white border border-gray-200 rounded-lg shadow-lg z-10">
          <button
            onClick={() => handleExport("xlsx")}
            className="w-full text-left px-3 py-2 text-sm hover:bg-gray-50 rounded-t-lg"
          >
            Excel (.xlsx)
          </button>
          <button
            onClick={() => handleExport("csv")}
            className="w-full text-left px-3 py-2 text-sm hover:bg-gray-50 rounded-b-lg"
          >
            CSV
          </button>
        </div>
      )}
    </div>
  );
}
