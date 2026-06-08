import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ChevronDown, ChevronUp } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
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

export default function Payments() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState({
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
    allocations: [] as { invoiceId: string; billId: string; allocatedAmount: number }[],
  });

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
  const openInvoices = (invoicesData?.data?.data?.items ?? []).filter((inv) => ["Sent", "PartiallyPaid", "Overdue"].includes(inv.status));
  const openBills = (billsData?.data?.data?.items ?? []).filter((b) => ["Received", "PartiallyPaid", "Overdue"].includes(b.status));
  const [saveError, setSaveError] = useState("");

  const isCustomerPayment = ["CustomerPayment", "AdvanceFromCustomer"].includes(form.paymentType);
  const isVendorPayment = ["VendorPayment", "AdvanceToVendor"].includes(form.paymentType);

  const createMutation = useMutation({
    mutationFn: () => {
      const payload = {
        ...form,
        customerId: form.customerId || undefined,
        vendorId: form.vendorId || undefined,
        bankAccountId: form.bankAccountId || undefined,
        referenceNumber: form.referenceNumber || undefined,
        chequeNumber: form.chequeNumber || undefined,
        allocations: form.allocations
          .filter((a) => a.allocatedAmount > 0)
          .map((a) => ({ invoiceId: a.invoiceId || undefined, billId: a.billId || undefined, allocatedAmount: a.allocatedAmount })),
      };
      return createPayment(payload as Parameters<typeof createPayment>[0]);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["payments"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save payment.";
      setSaveError(msg);
    },
  });

  function openAdd() {
    setForm({ paymentNumber: "", paymentType: "CustomerPayment", paymentMethod: "BankTransfer", paymentDate: new Date().toISOString().split("T")[0], amount: 0, referenceNumber: "", chequeNumber: "", notes: "", customerId: "", vendorId: "", bankAccountId: "", fiscalYearId: "", allocations: [] });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setSaveError(""); }

  function addAllocation(type: "invoice" | "bill", id: string) {
    setForm((f) => ({
      ...f,
      allocations: [...f.allocations, { invoiceId: type === "invoice" ? id : "", billId: type === "bill" ? id : "", allocatedAmount: 0 }],
    }));
  }

  function updateAllocation(idx: number, amount: number) {
    setForm((f) => ({ ...f, allocations: f.allocations.map((a, i) => i === idx ? { ...a, allocatedAmount: amount } : a) }));
  }

  function removeAllocation(idx: number) {
    setForm((f) => ({ ...f, allocations: f.allocations.filter((_, i) => i !== idx) }));
  }

  const totalAllocated = form.allocations.reduce((s, a) => s + (a.allocatedAmount || 0), 0);
  const unallocated = (form.amount || 0) - totalAllocated;

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
                  {["Payment #", "Type", "Method", "Date", "Party", "Amount", "Reference", "Actions"].map((h) => (
                    <th key={h} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {payments.length === 0 ? (
                  <tr><td colSpan={8} className="text-center py-10 text-gray-400">No payments found</td></tr>
                ) : payments.map((pmt) => (
                  <>
                    <tr key={pmt.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5 font-medium text-primary-600">{pmt.paymentNumber}</td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${typeColors[pmt.paymentType]}`}>{pmt.paymentType}</span>
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${methodColors[pmt.paymentMethod]}`}>{pmt.paymentMethod}</span>
                      </td>
                      <td className="px-4 py-2.5">{pmt.paymentDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">{pmt.customerName ?? pmt.vendorName ?? "—"}</td>
                      <td className="px-4 py-2.5 font-medium">{pmt.amount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-gray-500">{pmt.referenceNumber ?? pmt.chequeNumber ?? "—"}</td>
                      <td className="px-4 py-2.5">
                        {pmt.allocations.length > 0 && (
                          <button onClick={() => setExpandedId(expandedId === pmt.id ? null : pmt.id)} className="text-gray-400 hover:text-primary-600">
                            {expandedId === pmt.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                          </button>
                        )}
                      </td>
                    </tr>
                    {expandedId === pmt.id && pmt.allocations.length > 0 && (
                      <tr key={`${pmt.id}-alloc`} className="bg-gray-50">
                        <td colSpan={8} className="px-6 py-3">
                          <p className="text-xs text-gray-500 mb-1 font-medium">Allocations</p>
                          <div className="space-y-1">
                            {pmt.allocations.map((a) => (
                              <div key={a.id} className="flex items-center gap-4 text-xs">
                                <span className="text-gray-500">{a.invoiceId ? `Invoice: ${a.invoiceId}` : `Bill: ${a.billId}`}</span>
                                <span className="font-medium">{a.allocatedAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
                              </div>
                            ))}
                          </div>
                        </td>
                      </tr>
                    )}
                  </>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title="Record Payment" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Number *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.paymentNumber} onChange={(e) => setForm((f) => ({ ...f, paymentNumber: e.target.value }))} placeholder="PMT-2026-001" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.paymentDate} onChange={(e) => setForm((f) => ({ ...f, paymentDate: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.paymentType} onChange={(e) => setForm((f) => ({ ...f, paymentType: e.target.value as PaymentType, customerId: "", vendorId: "", allocations: [] }))}>
                {PAYMENT_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payment Method *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.paymentMethod} onChange={(e) => setForm((f) => ({ ...f, paymentMethod: e.target.value as PaymentMethod }))}>
                {PAYMENT_METHODS.map((m) => <option key={m} value={m}>{m}</option>)}
              </select>
            </div>
          </div>

          {isCustomerPayment && (
            <div>
              <SearchableDropdown
                label="Customer *"
                options={customerOptions}
                value={form.customerId}
                onChange={(v) => setForm((f) => ({ ...f, customerId: v ?? "", allocations: [] }))}
                placeholder="Select customer"
                searchPlaceholder="Search customers..."
                required
              />
            </div>
          )}

          {isVendorPayment && (
            <div>
              <SearchableDropdown
                label="Vendor *"
                options={vendorOptions}
                value={form.vendorId}
                onChange={(v) => setForm((f) => ({ ...f, vendorId: v ?? "", allocations: [] }))}
                placeholder="Select vendor"
                searchPlaceholder="Search vendors..."
                required
              />
            </div>
          )}

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Amount *</label>
              <input type="number" min={0} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.amount || ""} onChange={(e) => setForm((f) => ({ ...f, amount: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <SearchableDropdown
                label="Bank Account"
                options={bankAccountOptions}
                value={form.bankAccountId}
                onChange={(v) => setForm((f) => ({ ...f, bankAccountId: v ?? "" }))}
                placeholder="None (Cash)"
                searchPlaceholder="Search bank accounts..."
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Reference Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.referenceNumber} onChange={(e) => setForm((f) => ({ ...f, referenceNumber: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Cheque Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.chequeNumber} onChange={(e) => setForm((f) => ({ ...f, chequeNumber: e.target.value }))} />
            </div>
          </div>

          <div>
            <SearchableDropdown
              label="Fiscal Year *"
              options={fiscalYearOptions}
              value={form.fiscalYearId}
              onChange={(v) => setForm((f) => ({ ...f, fiscalYearId: v ?? "" }))}
              placeholder="Select year"
              searchPlaceholder="Search fiscal years..."
              required
            />
          </div>

          {(isCustomerPayment && form.customerId || isVendorPayment && form.vendorId) && (
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-xs text-gray-500">Allocations <span className="text-gray-400">(unallocated: {unallocated.toLocaleString("en-BD", { minimumFractionDigits: 2 })})</span></label>
                <div className="flex gap-2">
                  {isCustomerPayment && openInvoices.filter((inv) => inv.customerId === form.customerId).map((inv) => (
                    <button key={inv.id} onClick={() => addAllocation("invoice", inv.id)} className="text-xs text-primary-600 hover:underline">+{inv.invoiceNumber}</button>
                  ))}
                  {isVendorPayment && openBills.filter((b) => b.vendorId === form.vendorId).map((b) => (
                    <button key={b.id} onClick={() => addAllocation("bill", b.id)} className="text-xs text-primary-600 hover:underline">+{b.billNumber}</button>
                  ))}
                </div>
              </div>
              {form.allocations.length > 0 && (
                <div className="border border-gray-200 rounded-lg overflow-hidden">
                  <table className="w-full text-xs">
                    <thead className="bg-gray-50"><tr>
                      <th className="text-left px-3 py-1.5 text-gray-500">Invoice/Bill</th>
                      <th className="text-right px-3 py-1.5 text-gray-500">Amount</th>
                      <th className="px-2 py-1.5"></th>
                    </tr></thead>
                    <tbody>
                      {form.allocations.map((a, idx) => {
                        const inv = openInvoices.find((i) => i.id === a.invoiceId);
                        const bill = openBills.find((b) => b.id === a.billId);
                        return (
                          <tr key={idx} className="border-t border-gray-100">
                            <td className="px-3 py-1">{inv?.invoiceNumber ?? bill?.billNumber ?? a.invoiceId ?? a.billId}</td>
                            <td className="px-3 py-1 text-right"><input type="number" min={0} className="w-24 border border-gray-200 rounded px-1 py-0.5 text-xs text-right" value={a.allocatedAmount || ""} onChange={(e) => updateAllocation(idx, parseFloat(e.target.value) || 0)} /></td>
                            <td className="px-2 py-1"><button onClick={() => removeAllocation(idx)} className="text-red-400 hover:text-red-600">×</button></td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          )}

          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => createMutation.mutate()} disabled={createMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {createMutation.isPending ? "Saving..." : "Record Payment"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
