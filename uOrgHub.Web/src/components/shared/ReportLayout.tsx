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
    // A4 width less the 12mm @page margins: the print document is laid out off-screen at the
    // width it will have on paper, so tables marked data-page-totals can be measured into pages.
    iframe.style.width = "186mm";
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
            /* The page number lives in the page margin, where the browser fills in the real
               number for each sheet. (A position:fixed footer is painted identically on every
               page, so it can only ever show one number.) */
            @page {
              margin: 12mm;
              @bottom-center { content: "Page " counter(page) " of " counter(pages); font-size: 9px; color: #6b7280; }
            }

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

            /* This document only ever goes to the printer, so the print-only visibility rules
               apply outright rather than under @media print: the off-screen layout then matches
               the paper one, which is what tables marked data-page-totals are measured against. */
            .no-print { display: none !important; }
            .print\\:hidden { display: none !important; }
            .print\\:table { display: table !important; }
            .print\\:block { display: block !important; }

            /* Wide tables use the full page width instead of scrolling or being clipped. */
            .overflow-x-auto, .overflow-hidden, .overflow-y-auto { overflow: visible !important; }

            table { width: 100%; border-collapse: collapse; }
            thead { display: table-header-group; }   /* repeat column headers on every page */
            /* Totals print once, after the last row. As a footer group the browser repeats the
               row at the foot of every page — the grand total then reads as each page's total. */
            tfoot { display: table-row-group; }
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

              /* Black-ink printing: every report prints in pure black on white, no
                 matter what accent colours the screen version uses (status pills,
                 type badges, summary widgets, muted helper text). The !important
                 flags also override the print header's own inline gray styles. */
              body, body *, body *::before, body *::after {
                color: #000 !important;
                background: transparent !important;
                border-color: #000 !important;
                box-shadow: none !important;
                text-shadow: none !important;
              }
              a { text-decoration: none; }
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
              // Tables marked data-page-totals get a subtotal row at the foot of every printed
              // page. The browser never reports where it breaks pages, so the table is measured
              // here and split into one table per A4 page, each ending with a forced page break —
              // the breaks then fall exactly where the subtotals are. Markup contract:
              //   table[data-page-totals="Page total"]   label for the per-page row
              //   td[data-sum="key"][data-value="123.4"] a summed body cell
              //   tfoot row: td[data-total-label], td[data-sum="key"] — cloned as the page row's
              //   template; the tfoot itself stays as the grand total on the last page only.
              function paginateTotals() {
                var tables = document.querySelectorAll('table[data-page-totals]');
                if (!tables.length) return;

                // Pages are planned for A4 portrait, so the sheet is fixed to that size.
                var size = document.createElement('style');
                size.textContent = '@page { size: A4 portrait; }';
                document.head.appendChild(size);

                var probe = document.createElement('div');
                probe.style.cssText = 'position:absolute;visibility:hidden;width:1px;height:273mm;';
                document.body.appendChild(probe);
                var pageH = probe.getBoundingClientRect().height; // A4 height less 12mm margins
                document.body.removeChild(probe);
                var SAFETY = 12;

                var height = function (el) { return el.getBoundingClientRect().height; };
                var docTop = function (el) { return el.getBoundingClientRect().top + window.scrollY; };
                var fmt = function (n) { return n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); };

                Array.prototype.forEach.call(tables, function (table) {
                  var body = table.tBodies[0];
                  var thead = table.tHead, tfoot = table.tFoot;
                  var footRow = tfoot ? tfoot.rows[0] : null;
                  if (!body || !body.rows.length || !footRow) return;

                  var rows = Array.prototype.slice.call(body.rows);
                  var rowH = rows.map(height);
                  var theadH = thead ? height(thead) : 0;
                  var subtotalH = height(footRow);
                  var grandH = height(tfoot);
                  // Column widths are frozen so every page's table lines up with the others and
                  // rows wrap exactly as they were measured.
                  var widths = thead ? Array.prototype.map.call(thead.rows[0].cells, function (c) { return c.getBoundingClientRect().width; }) : [];

                  // Room left on the page the table starts on. If what precedes it already runs
                  // past the first page, the browser's breaks up there can't be predicted, so the
                  // section starts on a fresh page instead.
                  var section = table.closest('[data-print-section]') || table;
                  var top = docTop(table);
                  var cap;
                  if (top + theadH + rowH[0] + subtotalH + SAFETY > pageH) {
                    section.style.breakBefore = 'page';
                    cap = pageH - (top - docTop(section));
                  } else {
                    cap = pageH - top;
                  }

                  var chunks = [], cur = [], used = theadH;
                  rows.forEach(function (r, i) {
                    if (cur.length && used + rowH[i] + subtotalH + SAFETY > cap) {
                      chunks.push(cur); cur = []; used = theadH; cap = pageH;
                    }
                    cur.push(i); used += rowH[i];
                  });
                  // The last page also carries the grand total; rows that leave it no room move on.
                  var moved = [];
                  while (cur.length > 1 && used + subtotalH + grandH + SAFETY > cap) {
                    var j = cur.pop(); used -= rowH[j]; moved.unshift(j);
                  }
                  chunks.push(cur);
                  if (moved.length) chunks.push(moved);

                  var pageRow = function (idx) {
                    var tr = footRow.cloneNode(true);
                    Array.prototype.forEach.call(tr.cells, function (cell) {
                      var key = cell.getAttribute('data-sum');
                      if (cell.hasAttribute('data-total-label')) {
                        cell.textContent = table.getAttribute('data-page-totals') || 'Page total';
                      } else if (key) {
                        cell.textContent = fmt(idx.reduce(function (s, i) {
                          var c = rows[i].querySelector('[data-sum="' + key + '"]');
                          return s + (c ? parseFloat(c.getAttribute('data-value')) || 0 : 0);
                        }, 0));
                      } else {
                        cell.textContent = '';
                      }
                    });
                    return tr;
                  };

                  var parent = table.parentNode;
                  chunks.forEach(function (idx, n) {
                    var t = table.cloneNode(false);
                    t.removeAttribute('data-page-totals');
                    t.style.tableLayout = 'fixed';
                    if (thead) {
                      var h = thead.cloneNode(true);
                      Array.prototype.forEach.call(h.rows[0].cells, function (c, k) { c.style.width = widths[k] + 'px'; });
                      t.appendChild(h);
                    }
                    var b = document.createElement('tbody');
                    idx.forEach(function (i) { b.appendChild(rows[i]); });
                    b.appendChild(pageRow(idx));
                    t.appendChild(b);

                    // Breaks go on a block wrapper: forced breaks on a table box itself are not
                    // honoured by every engine.
                    var wrap = document.createElement('div');
                    if (n === chunks.length - 1) t.appendChild(tfoot);
                    else wrap.style.breakAfter = 'page';
                    wrap.appendChild(t);
                    parent.insertBefore(wrap, table);
                  });
                  parent.removeChild(table);
                });
              }

              var done = false;
              function go() {
                if (done) return; done = true;
                // Web fonts change row heights, so pages are planned only once they have loaded.
                var ready = document.fonts && document.fonts.ready ? document.fonts.ready : Promise.resolve();
                ready.then(function () {
                  try { paginateTotals(); } catch (e) { /* print without page totals rather than not at all */ }
                  window.focus(); window.print();
                });
              }
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