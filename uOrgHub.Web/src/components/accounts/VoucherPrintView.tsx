import { forwardRef } from "react";
import { Voucher } from "../../api/accounts";
import { voucherThemes } from "./voucherTheme";
import { amountInWords, formatTaka } from "../../utils/format";

interface VoucherPrintViewProps {
  voucher: Voucher;
  companyName?: string;
  companyAddress?: string;
}

/**
 * Paper-equivalent of the office Debit/Credit Voucher. The signature block keeps the
 * three approval lines blank for wet signature, matching the pre-printed forms.
 */
const VoucherPrintView = forwardRef<HTMLDivElement, VoucherPrintViewProps>(
  ({ voucher, companyName, companyAddress }, ref) => {
    const theme = voucherThemes[voucher.voucherType];
    // A Contra voucher has no counterparty — money stays inside the organisation — so the field
    // carries the transfer's reference rather than a person's name.
    const nameLabel = { Debit: "Paid To", Credit: "Received From", Contra: "Reference" }[
      voucher.voucherType
    ];

    return (
      <div ref={ref} className="bg-white text-gray-900" style={{ fontSize: "12px" }}>
        {/* Company header — the rule under it carries the voucher-type colour */}
        <div className={`text-center pb-3 border-b-4 ${theme.printBorder}`}>
          <h1 className="text-lg font-bold uppercase tracking-wide">{companyName ?? "Company"}</h1>
          {companyAddress && <p className="text-xs text-gray-500 mt-0.5">{companyAddress}</p>}
        </div>

        {/* Voucher title */}
        <div className="text-center my-4">
          <span
            className={`inline-block border-2 px-6 py-1 font-bold tracking-widest uppercase ${theme.printBorder} ${theme.printHeader} ${theme.printText}`}
          >
            {theme.label}
          </span>
          <p className={`text-xs mt-1 font-semibold ${theme.printText}`}>
            ({theme.code}) — {theme.meaning}
          </p>
        </div>

        {/* Voucher no + date */}
        <table className="w-full mb-3">
          <tbody>
            <tr>
              <td className="py-1 w-1/2">
                <span className="font-semibold">Voucher No: </span>
                <span className="border-b border-dotted border-gray-500 px-1">{voucher.voucherNumber}</span>
              </td>
              <td className="py-1 w-1/2 text-right">
                <span className="font-semibold">Date: </span>
                <span className="border-b border-dotted border-gray-500 px-1">
                  {voucher.voucherDate?.split("T")[0]}
                </span>
              </td>
            </tr>
          </tbody>
        </table>

        {/* Name / Section */}
        <table className="w-full mb-3">
          <tbody>
            <tr>
              <td className="py-1 w-1/2">
                <span className="font-semibold">{nameLabel}: </span>
                <span className="border-b border-dotted border-gray-500 px-1">{voucher.name ?? ""}</span>
              </td>
              <td className="py-1 w-1/2 text-right">
                <span className="font-semibold">Section: </span>
                <span className="border-b border-dotted border-gray-500 px-1">{voucher.section ?? ""}</span>
              </td>
            </tr>
            <tr>
              <td className="py-1" colSpan={2}>
                <span className="font-semibold">{voucher.projectId ? "Project" : "Charged To"}: </span>
                <span className="border-b border-dotted border-gray-500 px-1">
                  {voucher.costCenterName ?? ""}
                </span>
              </td>
            </tr>
          </tbody>
        </table>

        {/* Accounts + amount */}
        <table className="w-full border-collapse border border-gray-800 mb-3">
          <thead>
            <tr className={theme.printHeader}>
              <th className="border border-gray-800 px-2 py-1.5 text-left font-semibold">Particulars</th>
              <th className="border border-gray-800 px-2 py-1.5 text-left font-semibold w-40">Account</th>
              <th className="border border-gray-800 px-2 py-1.5 text-right font-semibold w-32">Debit</th>
              <th className="border border-gray-800 px-2 py-1.5 text-right font-semibold w-32">Credit</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td className="border border-gray-800 px-2 py-1.5">{voucher.description}</td>
              <td className="border border-gray-800 px-2 py-1.5">{voucher.debitAccountName}</td>
              <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">
                {formatTaka(voucher.amount)}
              </td>
              <td className="border border-gray-800 px-2 py-1.5" />
            </tr>
            <tr>
              <td className="border border-gray-800 px-2 py-1.5" />
              <td className="border border-gray-800 px-2 py-1.5">{voucher.creditAccountName}</td>
              <td className="border border-gray-800 px-2 py-1.5" />
              <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">
                {formatTaka(voucher.amount)}
              </td>
            </tr>
            <tr className="bg-gray-50 font-semibold">
              <td className="border border-gray-800 px-2 py-1.5" colSpan={2}>
                Total
              </td>
              <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">
                {formatTaka(voucher.amount)}
              </td>
              <td className="border border-gray-800 px-2 py-1.5 text-right tabular-nums">
                {formatTaka(voucher.amount)}
              </td>
            </tr>
          </tbody>
        </table>

        {/* Amount in words */}
        <div className="mb-5 border border-gray-800 px-2 py-1.5">
          <span className="font-semibold">Amount in Words: </span>
          {amountInWords(voucher.amount)}
        </div>

        {/* Reference to the accounting record */}
        {voucher.journalEntryNumber && (
          <p className="text-xs text-gray-500 mb-6">
            Journal Entry: {voucher.journalEntryNumber}
            {voucher.fiscalYearName ? ` · Fiscal Year: ${voucher.fiscalYearName}` : ""}
          </p>
        )}

        {/* Signatures — the three lower lines stay blank for wet signature */}
        <table className="w-full mt-10">
          <tbody>
            <tr>
              <td className="w-1/2 pb-8 align-bottom">
                <div className="border-t border-gray-800 pt-1 mr-8">
                  <span className="font-semibold">Prepared By</span>
                  {voucher.preparedBy && <span className="text-gray-600"> — {voucher.preparedBy}</span>}
                </div>
              </td>
              <td className="w-1/2 pb-8 align-bottom">
                <div className="border-t border-gray-800 pt-1 ml-8">
                  <span className="font-semibold">Received By</span>
                  {voucher.receivedBy && <span className="text-gray-600"> — {voucher.receivedBy}</span>}
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <table className="w-full mt-10">
          <tbody>
            <tr>
              <td className="w-1/3 align-bottom">
                <div className="border-t border-gray-800 pt-1 mr-6 text-center font-semibold">
                  Accounts Officer
                </div>
              </td>
              <td className="w-1/3 align-bottom">
                <div className="border-t border-gray-800 pt-1 mx-3 text-center font-semibold">
                  Director (Accounts)
                </div>
              </td>
              <td className="w-1/3 align-bottom">
                <div className="border-t border-gray-800 pt-1 ml-6 text-center font-semibold">
                  Managing Director
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    );
  },
);

VoucherPrintView.displayName = "VoucherPrintView";

export default VoucherPrintView;
