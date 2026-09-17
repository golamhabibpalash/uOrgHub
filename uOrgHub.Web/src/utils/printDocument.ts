const escapeHtml = (value: string) =>
  value.replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[c] as string);

/**
 * Prints a single DOM node as a standalone document — same proven technique as
 * ReportLayout.handlePrint (detached off-screen iframe, clones the live page's own stylesheets
 * instead of racing a fresh Tailwind CDN load, forces full-strength print colors, deepens muted
 * text/border shades for paper legibility), but without ReportLayout's report-specific concerns
 * (auto company header, column picker) — the node itself already renders its own letterhead.
 * Reusable for any single-record "print this document" need (Procurement documents today; PO/GRN
 * or similar tomorrow).
 */
export function printDocument(title: string, node: HTMLElement) {
  const content = node.innerHTML;

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
  setTimeout(cleanup, 60000);

  const appStyleNodes = Array.from(document.querySelectorAll('head link[rel="stylesheet"], head style'));
  const appStylesHtml = appStyleNodes
    .map((n) =>
      n.tagName === "LINK"
        ? `<link rel="stylesheet" href="${(n as HTMLLinkElement).href}">`
        : `<style>${n.textContent ?? ""}</style>`
    )
    .join("\n");
  const stylesTag = appStylesHtml || `<script src="https://cdn.tailwindcss.com"></script>`;

  printDoc.open();
  printDoc.write(`
    <!DOCTYPE html>
    <html>
      <head>
        <title>${escapeHtml(title)}</title>
        ${stylesTag}
        <style>
          @page { margin: 14mm; }
          * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }
          html { font-size: 11px; }
          body {
            font-family: 'Inter', ui-sans-serif, system-ui, sans-serif;
            color: #1f2937; line-height: 1.35;
          }

          table { width: 100%; border-collapse: collapse; }
          thead { display: table-header-group; }
          tr { page-break-inside: avoid; }
          th, td { vertical-align: top; }

          /* Keeps a signature block (or any marked section) from splitting across a page break. */
          .print-avoid-break { page-break-inside: avoid; }

          @media print {
            .no-print { display: none !important; }
            a { color: inherit; text-decoration: none; }

            /* Same deepened-color set as ReportLayout, for visual consistency app-wide. */
            .text-gray-300, .text-gray-400, .text-gray-500 { color: #1f2937 !important; }
            .text-gray-600 { color: #111827 !important; }
            .border-gray-100, .border-gray-200 { border-color: #9ca3af !important; }
            .text-red-500, .text-red-600 { color: #7f1d1d !important; }
            .text-green-600 { color: #14532d !important; }
            .text-blue-600, .text-blue-700 { color: #1e3a8a !important; }
            .text-purple-600, .text-purple-700 { color: #581c87 !important; }
            .text-orange-600, .text-orange-700 { color: #7c2d12 !important; }
          }
        </style>
      </head>
      <body>
        <div id="print-content">${content}</div>
        <script>
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
  printDoc.close();
}
