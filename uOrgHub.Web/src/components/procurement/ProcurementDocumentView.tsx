import { forwardRef } from "react";
import type { CompanyInfo, DocumentItem } from "../../api/procurement";

interface ProcurementDocumentViewProps {
  company: CompanyInfo;
  title: string;
  referenceLine: string;
  metaFields: { label: string; value: string }[];
  /** A record-specific line (e.g. an RFQ's own Title) shown between the meta grid and the body. */
  subtitle?: string;
  documentText: string;
  items: DocumentItem[];
  showPricing: boolean;
  totalLabel?: string;
  totalValue?: string;
  remarks?: string;
  /** Fixed boilerplate terms (not part of the editable application text) — numbered automatically. */
  termsAndConditions?: string[];
  signatures: { label: string; name?: string }[];
}

const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

/**
 * Entity-agnostic paper-equivalent of a procurement document (Purchase Requisition, RFQ, and any
 * future PO/GRN document) — mirrors the structure of the matching QuestPDF builder
 * (uOrgHub.Procurement/Reporting/Pdf/ProcurementDocumentPage.cs and its per-entity builders) so
 * screen, print, and PDF all read the same. Same Tailwind-table styling as VoucherPrintView, the
 * existing paper-equivalent precedent in this app.
 */
const ProcurementDocumentView = forwardRef<HTMLDivElement, ProcurementDocumentViewProps>(
  ({ company, title, referenceLine, metaFields, subtitle, documentText, items, showPricing, totalLabel, totalValue, remarks, termsAndConditions, signatures }, ref) => {
    const paragraphs = documentText.split(/\n\n+/).filter((p) => p.trim());

    return (
      <div ref={ref} className="bg-white text-gray-900" style={{ fontSize: "12px" }}>
        {/* Company letterhead */}
        <div className="text-center pb-3 border-b-4 border-gray-800">
          <h1 className="text-lg font-bold uppercase tracking-wide">{company.name}</h1>
          {company.tagLine && <p className="text-xs text-gray-500 mt-0.5">{company.tagLine}</p>}
          {(company.address || company.phone || company.email) && (
            <p className="text-[11px] text-gray-500 mt-0.5">
              {[company.address, company.phone && `Phone: ${company.phone}`, company.email && `Email: ${company.email}`]
                .filter(Boolean)
                .join("  |  ")}
            </p>
          )}
        </div>

        {/* Document title + reference */}
        <div className="text-center my-4">
          <h2 className="text-base font-bold tracking-widest uppercase">{title}</h2>
          <p className="text-xs text-gray-500 mt-1">{referenceLine}</p>
        </div>

        {/* Meta grid */}
        <table className="w-full mb-4 border-collapse border border-gray-300">
          <tbody>
            {Array.from({ length: Math.ceil(metaFields.length / 2) }).map((_, rowIdx) => {
              const a = metaFields[rowIdx * 2];
              const b = metaFields[rowIdx * 2 + 1];
              return (
                <tr key={rowIdx}>
                  <td className="border border-gray-300 bg-gray-50 px-2 py-1.5 w-28 text-[11px] text-gray-500 align-top">{a.label}</td>
                  <td className="border border-gray-300 px-2 py-1.5 w-1/3">{a.value}</td>
                  {b ? (
                    <>
                      <td className="border border-gray-300 bg-gray-50 px-2 py-1.5 w-28 text-[11px] text-gray-500 align-top">{b.label}</td>
                      <td className="border border-gray-300 px-2 py-1.5">{b.value}</td>
                    </>
                  ) : (
                    <td className="border border-gray-300" colSpan={2} />
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>

        {subtitle && <p className="font-semibold text-sm mb-3">{subtitle}</p>}

        {/* Application / letter body */}
        {paragraphs.length > 0 && (
          <div className="mb-4 space-y-2.5 text-justify leading-relaxed">
            {paragraphs.map((p, i) => (
              <p key={i}>{p.trim()}</p>
            ))}
          </div>
        )}

        {/* Items table */}
        <table className="w-full border-collapse border border-gray-800 mb-3 text-[11px]">
          <thead>
            <tr className="bg-gray-100">
              <th className="border border-gray-800 px-2 py-1.5 text-left font-semibold w-8">SL</th>
              <th className="border border-gray-800 px-2 py-1.5 text-left font-semibold">Item Description</th>
              <th className="border border-gray-800 px-2 py-1.5 text-left font-semibold w-16">Unit</th>
              <th className="border border-gray-800 px-2 py-1.5 text-right font-semibold w-20">Quantity</th>
              {showPricing && (
                <>
                  <th className="border border-gray-800 px-2 py-1.5 text-right font-semibold w-24">Est. Unit Cost</th>
                  <th className="border border-gray-800 px-2 py-1.5 text-right font-semibold w-24">Est. Total</th>
                </>
              )}
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.lineNo}>
                <td className="border border-gray-800 px-2 py-1.5">{item.lineNo}</td>
                <td className="border border-gray-800 px-2 py-1.5">
                  <div className="font-medium">{item.variantName}</div>
                  {item.description && <div className="text-gray-500">{item.description}</div>}
                  {item.notes && <div className="text-gray-500">{item.notes}</div>}
                </td>
                <td className="border border-gray-800 px-2 py-1.5">{item.uom ?? "-"}</td>
                <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">{fmt(item.quantity)}</td>
                {showPricing && (
                  <>
                    <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">{item.unitCost != null ? fmt(item.unitCost) : "-"}</td>
                    <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">{item.totalCost != null ? fmt(item.totalCost) : "-"}</td>
                  </>
                )}
              </tr>
            ))}
            {showPricing && totalValue != null && (
              <tr className="bg-gray-50 font-semibold">
                <td className="border border-gray-800 px-2 py-1.5 text-right" colSpan={4}>{totalLabel ?? "Total"}</td>
                <td className="border border-gray-800 px-2 py-1.5" />
                <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">{totalValue}</td>
              </tr>
            )}
          </tbody>
        </table>

        {remarks && (
          <p className="text-xs text-gray-600 mb-4">
            <span className="font-semibold">Remarks / Notes: </span>{remarks}
          </p>
        )}

        {termsAndConditions && termsAndConditions.length > 0 && (
          <div className="mb-4">
            <p className="text-sm font-semibold mb-1">Terms and Conditions</p>
            <ol className="text-xs text-gray-700 list-decimal list-inside space-y-0.5">
              {termsAndConditions.map((t, i) => (
                <li key={i}>{t}</li>
              ))}
            </ol>
          </div>
        )}

        {/* Signatures */}
        <table className="w-full mt-10 print-avoid-break">
          <tbody>
            <tr>
              {signatures.map((sig, i) => (
                <td key={i} className="w-1/2 pb-8 align-bottom">
                  <div className={`border-t border-gray-800 pt-1 ${i === 0 ? "mr-8" : "ml-8"}`}>
                    <span className="font-semibold">{sig.label}</span>
                    {sig.name && <span className="text-gray-600"> — {sig.name}</span>}
                  </div>
                </td>
              ))}
            </tr>
          </tbody>
        </table>
      </div>
    );
  }
);

ProcurementDocumentView.displayName = "ProcurementDocumentView";

export default ProcurementDocumentView;
