import { useRef, useState, useEffect, useCallback } from "react";
import { Printer, Download, FileDown, Columns3 } from "lucide-react";

interface ReportLayoutProps {
  title: string;
  subtitle?: string;
  filters?: React.ReactNode;
  children: React.ReactNode;
  loading?: boolean;
  onExportExcel?: () => void;
  onExportCsv?: () => void;
}

interface PrintColumn {
  key: string;
  label: string;
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
  const columnMenuRef = useRef<HTMLDivElement>(null);

  const [columnOptions, setColumnOptions] = useState<PrintColumn[]>([]);
  const [enabledColumns, setEnabledColumns] = useState<string[]>([]);
  const [columnMenuOpen, setColumnMenuOpen] = useState(false);
  const storageKey = `uorghub.print.columns.${title}`;

  const discoverColumns = useCallback((): PrintColumn[] => {
    const root = printRef.current;
    if (!root) return [];
    const found: PrintColumn[] = [];
    const seen = new Set<string>();
    root.querySelectorAll("th[data-col]").forEach((th) => {
      const key = th.getAttribute("data-col") ?? "";
      if (!key || seen.has(key)) return;
      const label = (th.textContent ?? key).replace(/\s+/g, " ").trim();
      if (!label) return;
      seen.add(key);
      found.push({ key, label });
    });
    return found;
  }, []);

  const loadSavedColumns = useCallback(
    (cols: PrintColumn[]): string[] | null => {
      try {
        const raw = localStorage.getItem(storageKey);
        const parsed = raw ? JSON.parse(raw) : null;
        if (Array.isArray(parsed)) {
          const valid = parsed.filter((k) => cols.some((c) => c.key === k));
          if (valid.length) return valid;
        }
      } catch {
        /* corrupted/missing storage is not fatal */
      }
      return null;
    },
    [storageKey]
  );

  const immediateEnabled = useCallback(
    (cols: PrintColumn[]): string[] => {
      const valid = enabledColumns.filter((k) => cols.some((c) => c.key === k));
      if (valid.length) return valid;
      return cols.filter((c) => c.key !== "actions").map((c) => c.key);
    },
    [enabledColumns]
  );

  const openColumnMenu = useCallback(() => {
    const cols = discoverColumns();
    setColumnOptions(cols);
    if (!cols.length) {
      setColumnMenuOpen(false);
      return;
    }
    setEnabledColumns((prev) => {
      if (prev.length) return prev;
      return loadSavedColumns(cols) ?? cols.filter((c) => c.key !== "actions").map((c) => c.key);
    });
    setColumnMenuOpen((open) => !open);
  }, [discoverColumns, loadSavedColumns]);

  const toggleColumn = useCallback(
    (key: string) => {
      setEnabledColumns((prev) => {
        const next = prev.includes(key) ? prev.filter((k) => k !== key) : [...prev, key];
        try {
          localStorage.setItem(storageKey, JSON.stringify(next));
        } catch {
          /* ignore */
        }
        return next;
      });
    },
    [storageKey]
  );

  useEffect(() => {
    if (!columnMenuOpen) return;
    const onDocumentClick = (e: MouseEvent) => {
      if (columnMenuRef.current && !columnMenuRef.current.contains(e.target as Node)) {
        setColumnMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", onDocumentClick);
    return () => document.removeEventListener("mousedown", onDocumentClick);
  }, [columnMenuOpen]);

  const handlePrint = useCallback(() => {
    const printWindow = window.open("", "_blank");
    if (!printWindow) return;
    const content = printRef.current?.innerHTML ?? "";
    const now = new Date().toLocaleString("en-BD");

    // The column menu configures which columns survive to paper; the print window only applies it.
    const cols = discoverColumns();
    const enabledPayload = JSON.stringify(immediateEnabled(cols));

    printWindow.document.write(`
      <!DOCTYPE html>
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
          <div id="print-content">${content}</div>
          <div class="print-footer">Page 1</div>
          <script>
            (function () {
              var enabled = ${enabledPayload};
              var active = {};
              enabled.forEach(function (k) { active[k] = true; });
              document.querySelectorAll('[data-col]').forEach(function (el) {
                var k = el.getAttribute('data-col');
                if (!active[k]) el.style.display = 'none';
              });
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
  }, [title, subtitle, discoverColumns, immediateEnabled]);

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
          <div className="relative" ref={columnMenuRef}>
            <button
              onClick={openColumnMenu}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600"
              title="Choose columns to print"
            >
              <Columns3 size={14} /> Columns
            </button>
            {columnMenuOpen && (
              <div className="absolute right-0 top-full mt-2 z-20 w-64 bg-white border border-gray-200 rounded-xl shadow-lg p-3">
                <div className="text-[11px] font-semibold text-gray-500 uppercase tracking-wide mb-2">
                  Columns to print
                </div>
                <div className="space-y-1 max-h-72 overflow-y-auto pr-1">
                  {columnOptions.map((col) => (
                    <label
                      key={col.key}
                      className="flex items-center gap-2 px-2 py-1 rounded-md text-sm text-gray-700 cursor-pointer hover:bg-gray-50"
                    >
                      <input
                        type="checkbox"
                        checked={enabledColumns.includes(col.key)}
                        onChange={() => toggleColumn(col.key)}
                        className="accent-primary-600"
                      />
                      {col.label}
                    </label>
                  ))}
                </div>
              </div>
            )}
          </div>
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