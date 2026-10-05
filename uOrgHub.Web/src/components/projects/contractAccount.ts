/** Shared formatting for the contract-account / RA-bill views. */

export const money = (n: number) => n.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export const paymentStateBadge: Record<string, { label: string; cls: string }> = {
  NotInvoiced: { label: "Not invoiced", cls: "bg-gray-100 text-gray-600" },
  Unpaid: { label: "Unpaid", cls: "bg-red-50 text-red-700" },
  PartiallyPaid: { label: "Partially paid", cls: "bg-amber-50 text-amber-700" },
  Paid: { label: "Paid", cls: "bg-green-50 text-green-700" },
  InvoiceVoid: { label: "Invoice void", cls: "bg-gray-100 text-gray-500" },
};
