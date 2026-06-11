import { useRef, useCallback } from "react";
import { Printer, Download, FileDown } from "lucide-react";

interface ReportLayoutProps {
  title: string;
  subtitle?: string;
  filters?: React.ReactNode;
  children: React.ReactNode;
  loading?: boolean;
  onExportExcel?: () => void;
  onExportCsv?: () => void;
}

export default function ReportLayout({
  title,
  subtitle,
  filters,
  children,
  loading,
  onExportExcel,
  onExportCsv,
}: ReportLayoutProps) {
  const printRef = useRef<HTMLDivElement>(null);

  const handlePrint = useCallback(() => {
    const printWindow = window.open("", "_blank");
    if (!printWindow) return;
    const content = printRef.current?.innerHTML ?? "";
    const now = new Date().toLocaleString("en-BD");
    printWindow.document.write(`
      <html>
        <head>
          <title>${title}</title>
          <script src="https://cdn.tailwindcss.com"></script>
          <style>
            @page { margin: 15mm; }
            body { font-family: 'Inter', sans-serif; color: #1f2937; -webkit-print-color-adjust: exact; }
            .print-header { text-align: center; margin-bottom: 1.5rem; }
            .print-footer { position: fixed; bottom: 0; width: 100%; text-align: center; font-size: 10px; color: #9ca3af; border-top: 1px solid #e5e7eb; padding-top: 4px; }
            @media print { .no-print { display: none !important; } }
          </style>
        </head>
        <body>
          <div class="print-header">
            <h1 style="font-size:18px; font-weight:600; margin:0;">${title}</h1>
            ${subtitle ? `<p style="font-size:12px; color:#6b7280; margin:4px 0 0;">${subtitle}</p>` : ""}
            <p style="font-size:10px; color:#9ca3af; margin:4px 0;">Printed: ${now}</p>
          </div>
          ${content}
          <div class="print-footer">Page 1</div>
          <script>window.print();</script>
        </body>
      </html>
    `);
    printWindow.document.close();
  }, [title, subtitle]);

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-4 no-print">
        <div>
          <h2 className="text-base font-medium text-gray-900">{title}</h2>
          {subtitle && <p className="text-xs text-gray-400">{subtitle}</p>}
        </div>
        <div className="flex items-center gap-2">
          {onExportExcel && (
            <button onClick={onExportExcel} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600">
              <FileDown size={14} /> Excel
            </button>
          )}
          {onExportCsv && (
            <button onClick={onExportCsv} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600">
              <Download size={14} /> CSV
            </button>
          )}
          <button onClick={handlePrint} className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600">
            <Printer size={14} /> Print
          </button>
        </div>
      </div>

      {/* Filters */}
      {filters && <div className="mb-4 no-print">{filters}</div>}

      {/* Content */}
      <div ref={printRef}>
        {loading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-2 border-primary-500 border-t-transparent" />
          </div>
        ) : (
          children
        )}
      </div>
    </div>
  );
}
