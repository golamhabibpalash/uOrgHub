import { forwardRef } from "react";
import { MapPin, Phone } from "lucide-react";
import type { PaymentReceipt } from "../../api/accounts";
import { amountInWords, formatDate } from "../../utils/format";

export interface MoneyReceiptOptions {
  /** Table of the bills/invoices this receipt settled, with paid-before and balance-after (client copy). */
  billBreakdown: boolean;
  /** Amount in words plus the "Deposited By" / "Authorised By" signature lines. */
  wordsAndSignatures: boolean;
  /** Office counterfoil printed beside the client copy, as on the pre-printed MR book. */
  officeCopy: boolean;
}

interface MoneyReceiptPrintViewProps {
  receipt: PaymentReceipt;
  options: MoneyReceiptOptions;
  companyName?: string;
  companyAddress?: string;
  companyPhone?: string;
  companyLogoUrl?: string;
}

const methodLabel: Record<string, string> = {
  Cash: "Cash",
  BankTransfer: "Bank Transfer",
  Cheque: "Cheque",
  CreditCard: "Card",
  DebitCard: "Card",
  MobileBanking: "Mobile Banking",
  OnlineTransfer: "Online Transfer (TT)",
};

const taka = (n: number) => n.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** A labelled fill-in line, like the pre-printed form: label, then a ruled blank holding the value. */
function Field({ label, value, grow = true }: { label: string; value?: React.ReactNode; grow?: boolean }) {
  return (
    <span className={`flex items-end gap-1.5 ${grow ? "flex-1" : ""}`}>
      <span className="whitespace-nowrap">{label}</span>
      <span className="flex-1 border-b border-gray-700 px-1 leading-tight min-h-[1.3em] min-w-[3rem] font-medium">
        {value ?? ""}
      </span>
    </span>
  );
}

function Row({ children }: { children: React.ReactNode }) {
  return <div className="flex items-end gap-3 py-[5px]">{children}</div>;
}

interface CopyProps extends MoneyReceiptPrintViewProps {
  copyLabel?: string;
  compact?: boolean;
}

/** One copy of the receipt — the office counterfoil and the client copy share this exact layout. */
function ReceiptCopy({ receipt, options, companyName, companyAddress, companyPhone, companyLogoUrl, copyLabel, compact }: CopyProps) {
  const projects = [...new Set(receipt.lines.map((l) => l.projectName).filter(Boolean))].join(", ");
  const onAccountOf =
    receipt.lines.length > 0
      ? receipt.lines.map((l) => l.sourceReference ?? l.documentNumber).join(", ")
      : receipt.paymentType === "AdvanceFromCustomer"
        ? "Advance"
        : receipt.notes ?? "";
  const instrument = [methodLabel[receipt.paymentMethod] ?? receipt.paymentMethod, receipt.chequeNumber ?? receipt.referenceNumber]
    .filter(Boolean)
    .join(" · ");
  const isInstrument = receipt.paymentMethod !== "Cash";

  return (
    <div className={`flex flex-col ${compact ? "text-[10px]" : "text-[12px]"}`}>
      {/* Letterhead */}
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-2">
          {companyLogoUrl && <img src={companyLogoUrl} alt="" className={compact ? "h-8 w-auto" : "h-12 w-auto"} />}
          <div>
            <h1 className={`font-bold tracking-wide text-primary-800 ${compact ? "text-base" : "text-2xl"}`}>{companyName ?? "Company"}</h1>
            <div className={`flex flex-wrap gap-x-4 gap-y-0.5 text-gray-700 ${compact ? "text-[8px]" : "text-[10px]"}`}>
              {companyPhone && (
                <span className="flex items-center gap-1"><Phone size={compact ? 8 : 10} /> {companyPhone}</span>
              )}
              {companyAddress && (
                <span className="flex items-center gap-1"><MapPin size={compact ? 8 : 10} /> {companyAddress}</span>
              )}
            </div>
          </div>
        </div>
        <div className="text-right whitespace-nowrap">
          {copyLabel && <p className="font-bold">{copyLabel}</p>}
          <p><span className="font-bold">MR No.:</span> <span className="font-mono">{receipt.receiptNumber}</span></p>
          <p><span className="font-bold">Date :</span> {formatDate(receipt.receiptDate)}</p>
        </div>
      </div>

      {/* Body — the lines of the pre-printed form */}
      <div className="mt-3">
        <Row><Field label="Received with thanks from Mr./Mrs./Ms." value={receipt.partyName} /></Row>
        <Row>
          <Field label="an amount of TK." value={taka(receipt.amount)} grow={false} />
          {options.wordsAndSignatures && <Field label="In Words Taka" value={amountInWords(receipt.amount, "").trim()} />}
        </Row>
        <Row>
          <Field label="On Account of" value={onAccountOf} />
          <Field label="Against the Flat no." grow={false} />
          <Field label="Project" value={projects} />
        </Row>
        <Row><Field label="By Cash/Cheque/Bank Draft/Po/TT No." value={instrument} /></Row>
        <Row>
          <Field label="Dated" value={isInstrument ? formatDate(receipt.receiptDate) : ""} />
          <Field label="of" />
          <span className="whitespace-nowrap">Bank</span>
        </Row>
        <Row>
          <Field label="" />
          <span className="whitespace-nowrap">Branch</span>
        </Row>
        <p className="mt-2 font-bold">
          *Note: <span className="font-normal">This receipt is valid only upon {isInstrument ? "realisation of the instrument" : ".........."}</span>
        </p>
      </div>

      {options.billBreakdown && !compact && receipt.lines.length > 0 && (
        <table className="w-full border-collapse mt-3 text-[10px]">
          <thead>
            <tr className="bg-gray-100">
              {["Bill / Invoice", "Bill Amount", "Paid Before", "This Receipt", "Balance Due"].map((h, i) => (
                <th key={h} className={`border border-gray-700 px-1.5 py-1 font-semibold ${i ? "text-right" : "text-left"}`}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {receipt.lines.map((l) => (
              <tr key={l.documentId}>
                <td className="border border-gray-700 px-1.5 py-1">
                  {l.sourceReference ? `${l.sourceReference} · ` : ""}{l.documentNumber}
                </td>
                <td className="border border-gray-700 px-1.5 py-1 text-right tabular-nums">{taka(l.documentTotal)}</td>
                <td className="border border-gray-700 px-1.5 py-1 text-right tabular-nums">{taka(l.paidBefore)}</td>
                <td className="border border-gray-700 px-1.5 py-1 text-right tabular-nums font-semibold">{taka(l.thisReceipt)}</td>
                <td className="border border-gray-700 px-1.5 py-1 text-right tabular-nums">{taka(l.balanceAfter)}</td>
              </tr>
            ))}
            {receipt.unallocatedAmount > 0 && (
              <tr>
                <td className="border border-gray-700 px-1.5 py-1 italic" colSpan={3}>Advance / on account</td>
                <td className="border border-gray-700 px-1.5 py-1 text-right tabular-nums font-semibold">{taka(receipt.unallocatedAmount)}</td>
                <td className="border border-gray-700 px-1.5 py-1" />
              </tr>
            )}
          </tbody>
        </table>
      )}

      {options.wordsAndSignatures && (
        <div className={`flex justify-between items-end ${compact ? "mt-8" : "mt-12"}`}>
          <div className="text-center">
            <div className="border-t border-gray-700 pt-0.5 px-4">Deposited By</div>
            <div>(Client/Representative)</div>
          </div>
          <div className="text-center">
            <div className="border-t border-gray-700 pt-0.5 px-6">Authorised By</div>
          </div>
        </div>
      )}
    </div>
  );
}

/**
 * Money receipt laid out like the company's pre-printed MR book: an office counterfoil on the left
 * and the client copy on the right, on one landscape page. Lines the system can't know (flat no.,
 * the client's bank and branch) print blank for hand-filling, exactly as on the paper form.
 */
const MoneyReceiptPrintView = forwardRef<HTMLDivElement, MoneyReceiptPrintViewProps>((props, ref) => (
  <div ref={ref} className="bg-white text-gray-900">
    <style>{"@page { size: A4 landscape; margin: 10mm; }"}</style>
    <div className="flex items-stretch">
      {props.options.officeCopy && (
        <>
          <div className="w-[36%] pr-4">
            <ReceiptCopy {...props} compact />
          </div>
          <div className="border-l-2 border-dashed border-gray-400 mx-1" aria-hidden />
        </>
      )}
      <div className={props.options.officeCopy ? "flex-1 pl-4" : "flex-1"}>
        <ReceiptCopy {...props} copyLabel="Client Copy" />
      </div>
    </div>
  </div>
));

MoneyReceiptPrintView.displayName = "MoneyReceiptPrintView";
export default MoneyReceiptPrintView;
