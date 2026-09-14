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
            @page { margin: 12mm; }

            /* Tailwind's text and spacing utilities are rem-based, so shrinking the root shrinks
               the whole report proportionally — a dense ledger then fits the page without having
               to override individual utility classes. */
            html { font-size: 11px; }
            body {
              font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
              color: #1f2937; line-height: 1.35;
              -webkit-print-color-adjust: exact; print-color-adjust: exact;
            }

            .print-header { text-align: center; margin-bottom: 16px; }
            .print-footer {
              position: fixed; bottom: 0; width: 100%; text-align: center;
              font-size: 9px; color: #9ca3af; border-top: 1px solid #e5e7eb; padding-top: 4px;
            }

            /* Wide tables use the full page width instead of scrolling or being clipped. */
            .overflow-x-auto, .overflow-hidden, .overflow-y-auto { overflow: visible !important; }

            table { width: 100%; border-collapse: collapse; }
            thead { display: table-header-group; }   /* repeat column headers on every page */
            tfoot { display: table-footer-group; }
            tr { page-break-inside: avoid; }
            th, td { vertical-align: top; }

            /* Clamped cells (e.g. narration) wrap on paper rather than losing text. */
            .truncate { overflow: visible !important; white-space: normal !important; text-overflow: clip !important; }
            .max-w-xs, .max-w-sm, .max-w-md { max-width: none !important; }

            /* Keep a section heading with the content that follows it. */
            h1, h2, h3, h4, p.font-medium { page-break-after: avoid; }

            @media print {
              .no-print { display: none !important; }
              a { color: inherit; text-decoration: none; }
              /* Guaranteed regardless of the Tailwind CDN's timing: a page can render a
                 compact interactive table for the screen and a full unpaged twin for print. */
              .print\\:hidden { display: none !important; }
              .print\\:table { display: table !important; }
              .print\\:block { display: block !important; }
            }
          </style>
        </head>
        <body>
          <div class="print-header">
            <h1 style="font-size:16px; font-weight:600; margin:0;">${title}</h1>
            ${subtitle ? `<p style="font-size:11px; color:#6b7280; margin:4px 0 0;">${subtitle}</p>` : ""}
            <p style="font-size:9px; color:#9ca3af; margin:4px 0;">Printed: ${now}</p>
          </div>
          ${content}
          <div class="print-footer">Page 1</div>
          <script>
            // Wait for the Tailwind CDN to finish generating utilities before printing —
            // firing too early prints an unstyled (oversized) page.
            (function () {
              var done = false;
              function go() { if (done) return; done = true; window.focus(); window.print(); }
              window.addEventListener('load', function () { setTimeout(go, 350); });
              if (document.readyState === 'complete') setTimeout(go, 350);
            })();
          </script>
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

      {/* Content — kept mounted even while loading, so a report with its own search/filter inputs
          (e.g. a DataGrid) doesn't get torn down and rebuilt on every refetch, which would drop
          focus and cursor position out from under whoever is typing. */}
      <div ref={printRef} className="relative">
        {children}
        {loading && (
          <div className="absolute inset-0 bg-white/60 flex items-center justify-center">
            <div className="animate-spin rounded-full h-8 w-8 border-2 border-primary-500 border-t-transparent" />
          </div>
        )}
      </div>
    </div>
  );
}
