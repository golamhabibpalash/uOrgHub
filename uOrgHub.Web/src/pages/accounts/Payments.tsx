import { Fragment, useState } from "react";
import { Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { Plus, ChevronDown, ChevronUp, FileCheck2, AlertCircle } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import AttachmentManager from "../../components/shared/AttachmentManager";
import PendingAttachments from "../../components/shared/PendingAttachments";
import {
  useCustomerLookup,
  useVendorLookup,
  useFiscalYearLookup,
  useBankAccountLookup,
} from "../../hooks/useEntityLookup";
import {
  getPayments,
  createPayment,
  getInvoices,
  getBills,
  PaymentType,
  PaymentMethod,
} from "../../api/accounts";
import { uploadAttachment } from "../../api/attachments";
import { useAuthStore } from "../../store/authStore";
import { extractApiError } from "../../utils/apiError";
import DateInput from "../../components/shared/DateInput";

const ATTACHMENT_ENTITY = "Payment";

const PAYMENT_TYPES: PaymentType[] = ["CustomerPayment", "VendorPayment", "AdvanceToVendor", "AdvanceFromCustomer", "Refund"];
const PAYMENT_METHODS: PaymentMethod[] = ["Cash", "BankTransfer", "Cheque", "CreditCard", "DebitCard", "MobileBanking", "OnlineTransfer"];

const typeColors: Record<PaymentType, string> = {
  CustomerPayment: "bg-green-50 text-green-700",
  VendorPayment: "bg-red-50 text-red-700",
  AdvanceToVendor: "bg-orange-50 text-orange-700",
  AdvanceFromCustomer: "bg-blue-50 text-blue-700",
  Refund: "bg-purple-50 text-purple-700",
};

const methodColors: Record<PaymentMethod, string> = {
  Cash: "bg-yellow-50 text-yellow-700",
  BankTransfer: "bg-blue-50 text-blue-700",
  Cheque: "bg-gray-100 text-gray-600",
  CreditCard: "bg-indigo-50 text-indigo-700",
  DebitCard: "bg-cyan-50 text-cyan-700",
  MobileBanking: "bg-teal-50 text-teal-700",
  OnlineTransfer: "bg-violet-50 text-violet-700",
};

const money = (n: number) => n.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const inputCls = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";

const emptyForm = () => ({
  paymentNumber: "",
  paymentType: "CustomerPayment" as PaymentType,
  paymentMethod: "BankTransfer" as PaymentMethod,
  paymentDate: new Date().toISOString().split("T")[0],
  amount: 0,
  referenceNumber: "",
  chequeNumber: "",
  notes: "",
  customerId: "",
  vendorId: "",
  bankAccountId: "",
  fiscalYearId: "",
  createVoucher: false,
  allocations: [] as { invoiceId: string; billId: string; allocatedAmount: number }[],
});

export default function Payments() {
  const qc = useQueryClient();
  const { hasClaim, hasRole } = useAuthStore();
  // Mirrors the "Payment" attachment target: Edit, or Create so whoever records it can attach the receipt.
  const canEditAttachments = hasRole("Admin") || hasClaim("Accounts.Payments.Edit") || hasClaim("Accounts.Payments.Create");

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [pendingFiles, setPendingFiles] = useState<File[]>([]);
  const [saveError, setSaveError] = useState("");
  const [formErrors, setFormErrors] = useState<string[]>([]);

  const { data, isLoading } = useQuery({
    queryKey: ["payments", page, search],
    queryFn: () => getPayments({ page, pageSize: 10, search }),
  });

  const { options: customerOptions } = useCustomerLookup();
  const { options: vendorOptions } = useVendorLookup();
  const { options: fiscalYearOptions } = useFiscalYearLookup();
  const { options: bankAccountOptions } = useBankAccountLookup();
  const { data: invoicesData } = useQuery({ queryKey: ["invoices", 1, "", ""], queryFn: () => getInvoices({ page: 1, pageSize: 200 }) });
  const { data: billsData } = useQuery({ queryKey: ["bills", 1, "", ""], queryFn: () => getBills({ page: 1, pageSize: 200 }) });

  const payments = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const allInvoices = invoicesData?.data?.data?.items ?? [];
  const allBills = billsData?.data?.data?.items ?? [];
  const openInvoices = allInvoices.filter((inv) => ["Sent", "PartiallyPaid", "Overdue"].includes(inv.status));
  const openBills = allBills.filter((b) => ["Received", "PartiallyPaid", "Overdue"].includes(b.status));

  const isCustomerPayment = ["CustomerPayment", "AdvanceFromCustomer"].includes(form.paymentType);
  const isVendorPayment = ["VendorPayment", "AdvanceToVendor"].includes(form.paymentType);
  const hasParty = isCustomerPayment || isVendorPayment;
  const voucherKind = isCustomerPayment ? "Credit (money received)" : "Debit (money paid out)";

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await createPayment({
        ...form,
        customerId: form.customerId || undefined,
        vendorId: form.vendorId || undefined,
        bankAccountId: form.bankAccountId || undefined,
        referenceNumber: form.referenceNumber || undefined,
        chequeNumber: form.chequeNumber || undefined,
        notes: form.notes || undefined,
        createVoucher: hasParty && form.createVoucher,
        allocations: form.allocations
          .filter((a) => a.allocatedAmount > 0)
          .map((a) => ({ invoiceId: a.invoiceId || undefined, billId: a.billId || undefined, allocatedAmount: a.allocatedAmount })),
      });
      const payment = res.data.data;

      // The payment is recorded; attach queued files to it. A failed file doesn't undo the
      // payment — say which one so it can be re-attached from the payment's row.
      if (payment?.id) {
        for (const file of pendingFiles) {
          try {
            await uploadAttachment(file, ATTACHMENT_ENTITY, payment.id);
          } catch (err) {
            toast.error(`${file.name} was not attached: ${extractApiError(err)}`);
          }
        }
      }
      return payment;
    },
    onSuccess: (payment) => {
      qc.invalidateQueries({ queryKey: ["payments"] });
      if (payment?.voucherNumber) {
        qc.invalidateQueries({ queryKey: ["vouchers"] });
        toast.success(`Voucher ${payment.voucherNumber} created and posted.`);
      }
      if (payment?.id) qc.invalidateQueries({ queryKey: ["attachments", ATTACHMENT_ENTITY, payment.id] });
      closeModal(true);
    },
    onError: (err: unknown) => setSaveError(extractApiError(err) || "Failed to save payment."),
  });

  function openAdd() {
    setForm(emptyForm());
    setPendingFiles([]);
    setSaveError("");
    setFormErrors([]);
    setModal(true);
  }

  function closeModal(force = false) {
    if (createMutation.isPending && !force) return;
    setModal(false);
    setSaveError("");
    setFormErrors([]);
    setPendingFiles([]);
  }

  function validate(): string[] {
    const errors: string[] = [];
    if (!form.paymentDate) errors.push("Payment date is required.");
    if (!(form.amount > 0)) errors.push("Amount must be greater than 0.");
    if (!form.fiscalYearId) errors.push("Select a fiscal year.");
    if (isCustomerPayment && !form.customerId) errors.push("Select a customer.");
    if (isVendorPayment && !form.vendorId) errors.push("Select a vendor.");
    if (totalAllocated > form.amount) errors.push("Allocated total cannot exceed the payment amount.");
    if (hasParty && form.createVoucher && !form.bankAccountId)
      errors.push("Select the bank/cash account to create a voucher.");
    return errors;
  }

  function submit() {
    setSaveError("");
    const errors = validate();
    setFormErrors(errors);
    if (errors.length === 0) createMutation.mutate();
  }

  function addAllocation(type: "invoice" | "bill", id: string) {
    setForm((f) => {
      if (f.allocations.some((a) => (type === "invoice" ? a.invoiceId : a.billId) === id)) return f;
      return {
        ...f,
        allocations: [...f.allocations, { invoiceId: type === "invoice" ? id : "", billId: type === "bill" ? id : "", allocatedAmount: 0 }],
      };
    });
  }

  function updateAllocation(idx: number, amount: number) {
    setForm((f) => ({ ...f, allocations: f.allocations.map((a, i) => i === idx ? { ...a, allocatedAmount: amount } : a) }));
  }

  function removeAllocation(idx: number) {
    setForm((f) => ({ ...f, allocations: f.allocations.filter((_, i) => i !== idx) }));
  }

  const totalAllocated = form.allocations.reduce((s, a) => s + (a.allocatedAmount || 0), 0);
  const unallocated = (form.amount || 0) - totalAllocated;

  const allocationLabel = (a: { invoiceId?: string | null; billId?: string | null }) => {
    if (a.invoiceId) return `Invoice ${allInvoices.find((i) => i.id === a.invoiceId)?.invoiceNumber ?? a.invoiceId}`;
    return `Bill ${allBills.find((b) => b.id === a.billId)?.billNumber ?? a.billId}`;
  };

  const errorsToShow = [...(saveError ? [saveError] : []), ...formErrors];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Payments</h2>
          <p className="text-xs text-gray-400">Record customer and vendor payments</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Record Payment
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search payments..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center h-40 text-sm text-gray-400">Loading...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-50">
                  {["Payment #", "Type", "Method", "Date", "Party", "Amount", "Reference", "Voucher", ""].map((h, i) => (
                    <th key={h || i} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {payments.length === 0 ? (
                  <tr><td colSpan={9} className="text-center py-10 text-gray-400">No payments found</td></tr>
                ) : payments.map((pmt) => (
                  <Fragment key={pmt.id}>
                    <tr className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5 font-medium text-primary-600">{pmt.paymentNumber}</td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${typeColors[pmt.paymentType]}`}>{pmt.paymentType}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${methodColors[pmt.paymentMethod]}`}>{pmt.paymentMethod}</span>
                      </td>
                      <td className="px-4 py-2.5">{pmt.paymentDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">{pmt.customerName ?? pmt.vendorName ?? "—"}</td>
                      <td className="px-4 py-2.5 font-medium tabular-nums">{money(pmt.amount)}</td>
                      <td className="px-4 py-2.5 text-gray-500">{pmt.referenceNumber ?? pmt.chequeNumber ?? "—"}</td>
                      <td className="px-4 py-2.5">
                        {pmt.voucherId ? (
                          <Link to={`/accounts/vouchers/${pmt.voucherId}`} className="inline-flex items-center gap-1 text-xs text-primary-600 hover:underline">
                            <FileCheck2 size={13} /> {pmt.voucherNumber}
                          </Link>
                        ) : (
                          <span className="text-gray-300">—</span>
                        )}
                      </td>
                      <td className="px-4 py-2.5">
                        <button
                          onClick={() => setExpandedId(expandedId === pmt.id ? null : pmt.id)}
                          className="text-gray-400 hover:text-primary-600"
                          title="View details & attachments"
                        >
                          {expandedId === pmt.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                        </button>
                      </td>
                    </tr>
                    {expandedId === pmt.id && (
                      <tr className="bg-gray-50">
                        <td colSpan={9} className="px-6 py-4">
                          <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
                            <div className="lg:col-span-3 bg-white border border-gray-200 rounded-xl p-4 space-y-4">
                              <div>
                                <h3 className="text-sm font-medium text-gray-900 mb-2">Allocations</h3>
                                {pmt.allocations.length === 0 ? (
                                  <p className="text-xs text-gray-400">Not allocated to any invoice or bill.</p>
                                ) : (
                                  <ul className="divide-y divide-gray-50">
                                    {pmt.allocations.map((a) => (
                                      <li key={a.id} className="flex justify-between py-1.5 text-xs">
                                        <span className="text-gray-600">{allocationLabel(a)}</span>
                                        <span className="font-medium tabular-nums">{money(a.allocatedAmount)}</span>
                                      </li>
                                    ))}
                                  </ul>
                                )}
                              </div>
                              {pmt.notes && (
                                <div>
                                  <h3 className="text-sm font-medium text-gray-900 mb-1">Notes</h3>
                                  <p className="text-xs text-gray-600 whitespace-pre-line">{pmt.notes}</p>
                                </div>
                              )}
                              <div className="text-xs text-gray-500">
                                {pmt.voucherId ? (
                                  <>Voucher <Link to={`/accounts/vouchers/${pmt.voucherId}`} className="text-primary-600 hover:underline">{pmt.voucherNumber}</Link> posted with this payment.</>
                                ) : pmt.journalEntryId ? (
                                  "Posted to the ledger. No voucher was created."
                                ) : (
                                  "Not posted to the ledger (no bank account or party account)."
                                )}
                              </div>
                            </div>
                            <div className="lg:col-span-2">
                              <AttachmentManager entityType={ATTACHMENT_ENTITY} entityId={pmt.id} canEdit={canEditAttachments} />
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title="Record Payment" open={modal} onClose={() => closeModal()} size="3xl">
        <div className="space-y-4">
          {errorsToShow.length > 0 && (
            <div className="flex gap-2 text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              <AlertCircle size={16} className="shrink-0 mt-0.5" />
              <ul className="space-y-0.5">{errorsToShow.map((e) => <li key={e}>{e}</li>)}</ul>
            </div>
          )}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Number</label>
              <input className={inputCls} value={form.paymentNumber} onChange={(e) => setForm((f) => ({ ...f, paymentNumber: e.target.value }))} placeholder="Auto-generated if blank" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Date *</label>
              <DateInput className={inputCls} value={form.paymentDate} onChange={(e) => setForm((f) => ({ ...f, paymentDate: e.target.value }))} />
            </div>
            <SearchableDropdown
              label="Fiscal Year *"
              options={fiscalYearOptions}
              value={form.fiscalYearId}
              onChange={(v) => setForm((f) => ({ ...f, fiscalYearId: v ?? "" }))}
              placeholder="Select year"
              searchPlaceholder="Search fiscal years..."
            />
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Type *</label>
              <select className={inputCls} value={form.paymentType} onChange={(e) => setForm((f) => ({ ...f, paymentType: e.target.value as PaymentType, customerId: "", vendorId: "", allocations: [] }))}>
                {PAYMENT_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Method *</label>
              <select className={inputCls} value={form.paymentMethod} onChange={(e) => setForm((f) => ({ ...f, paymentMethod: e.target.value as PaymentMethod }))}>
                {PAYMENT_METHODS.map((m) => <option key={m} value={m}>{m}</option>)}
              </select>
            </div>
            {isCustomerPayment && (
              <SearchableDropdown
                label="Customer *"
                options={customerOptions}
                value={form.customerId}
                onChange={(v) => setForm((f) => ({ ...f, customerId: v ?? "", allocations: [] }))}
                placeholder="Select customer"
                searchPlaceholder="Search customers..."
              />
            )}
            {isVendorPayment && (
              <SearchableDropdown
                label="Vendor *"
                options={vendorOptions}
                value={form.vendorId}
                onChange={(v) => setForm((f) => ({ ...f, vendorId: v ?? "", allocations: [] }))}
                placeholder="Select vendor"
                searchPlaceholder="Search vendors..."
              />
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Amount *</label>
              <input type="number" min={0} step="0.01" className={`${inputCls} text-right tabular-nums`} value={form.amount || ""} onChange={(e) => setForm((f) => ({ ...f, amount: parseFloat(e.target.value) || 0 }))} />
            </div>
            <SearchableDropdown
              label={hasParty && form.createVoucher ? "Bank Account *" : "Bank Account"}
              options={bankAccountOptions}
              value={form.bankAccountId}
              onChange={(v) => setForm((f) => ({ ...f, bankAccountId: v ?? "" }))}
              placeholder="None (Cash)"
              searchPlaceholder="Search bank accounts..."
            />
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Reference #</label>
                <input className={inputCls} value={form.referenceNumber} onChange={(e) => setForm((f) => ({ ...f, referenceNumber: e.target.value }))} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Cheque #</label>
                <input className={inputCls} value={form.chequeNumber} onChange={(e) => setForm((f) => ({ ...f, chequeNumber: e.target.value }))} />
              </div>
            </div>
          </div>

          {(isCustomerPayment && form.customerId || isVendorPayment && form.vendorId) && (
            <div className="border border-gray-200 rounded-xl p-4">
              <div className="flex flex-wrap items-center justify-between gap-2 mb-2">
                <h3 className="text-sm font-medium text-gray-900">
                  Allocations <span className="text-xs font-normal text-gray-400">(unallocated: {money(unallocated)})</span>
                </h3>
                <div className="flex flex-wrap gap-1.5">
                  {isCustomerPayment && openInvoices.filter((inv) => inv.customerId === form.customerId).map((inv) => (
                    <button key={inv.id} onClick={() => addAllocation("invoice", inv.id)} className="text-xs text-primary-600 border border-primary-200 rounded-full px-2 py-0.5 hover:bg-primary-50">+ {inv.invoiceNumber}</button>
                  ))}
                  {isVendorPayment && openBills.filter((b) => b.vendorId === form.vendorId).map((b) => (
                    <button key={b.id} onClick={() => addAllocation("bill", b.id)} className="text-xs text-primary-600 border border-primary-200 rounded-full px-2 py-0.5 hover:bg-primary-50">+ {b.billNumber}</button>
                  ))}
                </div>
              </div>
              {form.allocations.length === 0 ? (
                <p className="text-xs text-gray-400">Pick an open {isCustomerPayment ? "invoice" : "bill"} above to apply this payment to it.</p>
              ) : (
                <ul className="divide-y divide-gray-100">
                  {form.allocations.map((a, idx) => (
                    <li key={a.invoiceId || a.billId} className="flex items-center gap-3 py-1.5 text-sm">
                      <span className="flex-1 text-gray-700">{allocationLabel(a)}</span>
                      <input type="number" min={0} step="0.01" className="w-32 border border-gray-200 rounded-lg px-2 py-1 text-sm text-right tabular-nums" value={a.allocatedAmount || ""} onChange={(e) => updateAllocation(idx, parseFloat(e.target.value) || 0)} />
                      <button onClick={() => removeAllocation(idx)} className="text-red-400 hover:text-red-600 px-1" title="Remove">×</button>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )}

          <label
            className={`flex items-start gap-3 border rounded-xl p-4 transition-colors ${
              !hasParty
                ? "border-gray-200 bg-gray-50 opacity-60 cursor-not-allowed"
                : form.createVoucher
                  ? "border-primary-300 bg-primary-50/50 cursor-pointer"
                  : "border-gray-200 hover:border-gray-300 cursor-pointer"
            }`}
          >
            <input
              type="checkbox"
              className="mt-0.5 h-4 w-4 accent-primary-500"
              checked={hasParty && form.createVoucher}
              disabled={!hasParty}
              onChange={(e) => setForm((f) => ({ ...f, createVoucher: e.target.checked }))}
            />
            <span className="text-sm">
              <span className="font-medium text-gray-900">Create voucher</span>
              <span className="block text-xs text-gray-500 mt-0.5">
                {hasParty ? (
                  <>Generates a posted <b>{voucherKind}</b> voucher for this payment, using the same journal entry — nothing is booked twice. Needs a bank account and a party with a {isCustomerPayment ? "receivable" : "payable"} account.</>
                ) : (
                  "Vouchers can only be created for customer or vendor payments."
                )}
              </span>
            </span>
          </label>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Notes</label>
              <textarea rows={4} className={inputCls} value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
            </div>
            <PendingAttachments files={pendingFiles} onChange={setPendingFiles} disabled={createMutation.isPending} />
          </div>

          <div className="flex justify-end gap-2 pt-3 border-t border-gray-100">
            <button onClick={() => closeModal()} disabled={createMutation.isPending} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50">Cancel</button>
            <button onClick={submit} disabled={createMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {createMutation.isPending ? "Saving..." : hasParty && form.createVoucher ? "Record Payment & Voucher" : "Record Payment"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
