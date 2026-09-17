import { useRef, useState, useEffect, useCallback } from "react";
import { useQuery } from "@tanstack/react-query";
import { Printer, Download, FileDown, FileText, Columns3 } from "lucide-react";
import { getMyCompany } from "../../api/company";

const escapeHtml = (value: string) =>
  value.replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[c] as string);

interface ReportLayoutProps {
  title: string;
  subtitle?: string;
  filters?: React.ReactNode;
  children: React.ReactNode;
  loading?: boolean;
  onExportExcel?: () => void;
  onExportCsv?: () => void;
  onExportPdf?: () => void;
  exportingPdf?: boolean;
  /** A page-specific action (e.g. "Add Vendor") rendered before the Print/Columns buttons, so a
   * plain list page can get this layout's print/column-picker without losing its create button. */
  headerActions?: React.ReactNode;
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
  onExportPdf,
  exportingPdf,
  headerActions,
}: ReportLayoutProps) {
  const printRef = useRef<HTMLDivElement>(null);
  const columnMenuRef = useRef<HTMLDivElement>(null);

  // Same query key/staleTime as VoucherDetail.tsx's printed-document header, so the two share
  // one cached fetch instead of each hitting /company/mine separately.
  const { data: company } = useQuery({ queryKey: ["my-company"], queryFn: getMyCompany, staleTime: 300000 });

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
    const content = printRef.current?.innerHTML ?? "";
    const now = new Date().toLocaleString("en-BD");

    // The column menu configures which columns survive to paper; the print document only applies it.
    const cols = discoverColumns();
    const enabledPayload = JSON.stringify(immediateEnabled(cols));

    // Printed via a detached, off-screen iframe rather than window.open(), so printing never
    // spawns a visible new tab/window — the print dialog attaches to this same page.
    const iframe = document.createElement("iframe");
    iframe.style.position = "fixed";
    iframe.style.top = "-100000px";
    iframe.style.left = "0";
    iframe.style.width = "0";
    iframe.style.height = "0";
    iframe.style.border = "none";
    iframe.setAttribute("aria-hidden", "true");
    document.body.appendChild(iframe);

    let cleaned = false;
    const cleanup = () => {
      if (cleaned) return;
      cleaned = true;
      iframe.remove();
    };

    const printDoc = iframe.contentWindow?.document;
    if (!printDoc) {
      cleanup();
      return;
    }

    const printWin = iframe.contentWindow;
    printWin?.addEventListener("afterprint", cleanup);
    // Fallback in case afterprint never fires (some browser/OS print-dialog combinations).
    setTimeout(cleanup, 60000);

    // Reuse the app's own already-computed styles instead of fetching Tailwind fresh from a CDN:
    // a <script src="cdn.tailwindcss.com"> has to JIT-generate every utility class at print time,
    // and on a large report that can still be mid-generation when window.print() fires — utility
    // classes (especially colours) simply hadn't been created yet, printing washed-out/default
    // colours. Cloning the live page's own <link>/<style> tags is the exact CSS already rendering
    // correctly on screen, so there's nothing left to race.
    const appStyleNodes = Array.from(document.querySelectorAll('head link[rel="stylesheet"], head style'));
    const appStylesHtml = appStyleNodes
      .map((node) =>
        node.tagName === "LINK"
          ? `<link rel="stylesheet" href="${(node as HTMLLinkElement).href}">`
          : `<style>${node.textContent ?? ""}</style>`
      )
      .join("\n");
    // Belt-and-braces fallback for the unexpected case the live page has no stylesheet nodes to clone.
    const stylesTag = appStylesHtml || `<script src="https://cdn.tailwindcss.com"></script>`;

    const companyName = company?.name ? escapeHtml(company.name) : "";
    const companyAddress = company?.address ? escapeHtml(company.address) : "";

    printDoc.open();
    printDoc.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <title>${escapeHtml(title)}</title>
          ${stylesTag}
          <style>
            @page { margin: 12mm; }

            /* Forces backgrounds/colours to print at full strength regardless of the browser's
               "print backgrounds" default (usually off) or any ink-saving heuristic. */
            * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }

            /* Tailwind's text and spacing utilities are rem-based, so shrinking the root shrinks
               the whole report proportionally — a dense ledger then fits the page without having
               to override individual utility classes. */
            html { font-size: 11px; }
            body {
              font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
              color: #1f2937; line-height: 1.35;
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
              /* Defensive fallback in case the CDN path above ends up used (no app stylesheet
                 found to clone): a page can still render a compact interactive table for the
                 screen and a full unpaged twin for print. */
              .print\\:hidden { display: none !important; }
              .print\\:table { display: table !important; }
              .print\\:block { display: block !important; }

              /* The report pages lean on Tailwind's lighter grays/mid-tone accents for on-screen
                 visual hierarchy (text-gray-500 alone appears 150+ times across these pages) — on
                 paper that same weight reads as faint, low-contrast text. Reprint every shade that
                 shows up in the reports at a deep, unambiguous ink color; darker shades (700+) are
                 already print-legible and are left alone. */
              .text-gray-300, .text-gray-400, .text-gray-500 { color: #1f2937 !important; }
              .text-gray-600 { color: #111827 !important; }
              .border-gray-100, .border-gray-200 { border-color: #9ca3af !important; }

              .text-red-500, .text-red-600 { color: #7f1d1d !important; }
              .text-green-600 { color: #14532d !important; }
              .text-blue-600, .text-blue-700 { color: #1e3a8a !important; }
              .text-purple-600, .text-purple-700 { color: #581c87 !important; }
              .text-orange-600, .text-orange-700 { color: #7c2d12 !important; }
              .text-yellow-600, .text-yellow-700 { color: #713f12 !important; }
              .text-indigo-600 { color: #312e81 !important; }
              .text-amber-600 { color: #78350f !important; }
              .text-emerald-600 { color: #064e3b !important; }
              .border-red-100, .border-red-200 { border-color: #f87171 !important; }
              .border-green-100 { border-color: #4ade80 !important; }
            }
          </style>
        </head>
        <body>
          <div class="print-header">
            ${companyName ? `<h1 style="font-size:18px; font-weight:700; margin:0; text-transform:uppercase; letter-spacing:0.02em;">${companyName}</h1>` : ""}
            ${companyAddress ? `<p style="font-size:10px; color:#6b7280; margin:2px 0 0;">${companyAddress}</p>` : ""}
            <h2 style="font-size:15px; font-weight:600; margin:${companyName ? "10px" : "0"} 0 0;">${escapeHtml(title)}</h2>
            ${subtitle ? `<p style="font-size:11px; color:#6b7280; margin:4px 0 0;">${escapeHtml(subtitle)}</p>` : ""}
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
              // A totals-row label cell (e.g. "Totals" under colSpan={4}) names the data-col keys
              // it stands in for via data-col-span, so its span shrinks to match however many of
              // those columns are still visible — otherwise a table with one hidden in that group
              // shifts every total to its right out from under the wrong header.
              document.querySelectorAll('[data-col-span]').forEach(function (el) {
                var keys = (el.getAttribute('data-col-span') || '').split(',').map(function (s) { return s.trim(); }).filter(Boolean);
                var visible = keys.filter(function (k) { return active[k]; }).length;
                el.colSpan = Math.max(visible, 1);
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
    printDoc.close();
  }, [title, subtitle, discoverColumns, immediateEnabled, company]);

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-4 no-print">
        <div>
          <h2 className="text-base font-medium text-gray-900">{title}</h2>
          {subtitle && <p className="text-xs text-gray-400">{subtitle}</p>}
        </div>
        <div className="flex items-center gap-2">
          {headerActions}
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
          {onExportPdf && (
            <button
              onClick={onExportPdf}
              disabled={exportingPdf}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600 disabled:opacity-50"
            >
              <FileText size={14} /> {exportingPdf ? "Preparing..." : "Download PDF"}
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