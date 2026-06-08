import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, XCircle, ChevronDown, ChevronUp } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import {
  useCustomerLookup,
  useFiscalYearLookup,
  useCostCenterLookup,
  useChartOfAccountsLookup,
} from "../../hooks/useEntityLookup";
import {
  getInvoices,
  createInvoice,
  postInvoice,
  voidInvoice,
  getTaxRates,
  InvoiceStatus,
} from "../../api/accounts";

const statusColors: Record<InvoiceStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Sent: "bg-blue-50 text-blue-700",
  PartiallyPaid: "bg-yellow-50 text-yellow-700",
  Paid: "bg-green-50 text-green-700",
  Overdue: "bg-red-50 text-red-700",
  Cancelled: "bg-gray-100 text-gray-400",
  Void: "bg-red-100 text-red-500",
};

export default function Invoices() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [form, setForm] = useState({
    invoiceNumber: "",
    customerId: "",
    fiscalYearId: "",
    invoiceDate: new Date().toISOString().split("T")[0],
    dueDate: "",
    notes: "",
    costCenterId: "",
    lines: [{ description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: 1, taxRateId: "", revenueAccountId: "", costCenterId: "" }],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["invoices", page, search, statusFilter],
    queryFn: () => getInvoices({ page, pageSize: 10, search }, undefined, statusFilter || undefined),
  });

  const { data: taxRatesData } = useQuery({ queryKey: ["tax-rates", 1, ""], queryFn: () => getTaxRates({ page: 1, pageSize: 100 }) });
  const { options: customerOptions } = useCustomerLookup();
  const { options: fiscalYearOptions } = useFiscalYearLookup();
  const { options: costCenterOptions } = useCostCenterLookup();
  const { options: coaOptions } = useChartOfAccountsLookup();

  const invoices = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const taxRates = taxRatesData?.data?.data?.items ?? [];
  const [saveError, setSaveError] = useState("");

  const createMutation = useMutation({
    mutationFn: () => {
      const payload = {
        ...form,
        costCenterId: form.costCenterId || undefined,
        lines: form.lines.map((l, i) => ({
          ...l,
          lineOrder: i + 1,
          taxRateId: l.taxRateId || undefined,
          costCenterId: l.costCenterId || undefined,
        })),
      };
      return createInvoice(payload as Parameters<typeof createInvoice>[0]);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["invoices"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save invoice.";
      setSaveError(msg);
    },
  });

  const postMutation = useMutation({
    mutationFn: (id: string) => postInvoice(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["invoices"] }),
  });

  const voidMutation = useMutation({
    mutationFn: (id: string) => voidInvoice(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["invoices"] }),
  });

  function openAdd() {
    setForm({
      invoiceNumber: "",
      customerId: "",
      fiscalYearId: "",
      invoiceDate: new Date().toISOString().split("T")[0],
      dueDate: "",
      notes: "",
      costCenterId: "",
      lines: [{ description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: 1, taxRateId: "", revenueAccountId: "", costCenterId: "" }],
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setSaveError(""); }

  function addLine() {
    setForm((f) => ({
      ...f,
      lines: [...f.lines, { description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: f.lines.length + 1, taxRateId: "", revenueAccountId: "", costCenterId: "" }],
    }));
  }

  function removeLine(idx: number) {
    setForm((f) => ({ ...f, lines: f.lines.filter((_, i) => i !== idx) }));
  }

  function updateLine(idx: number, field: string, value: string | number) {
    setForm((f) => ({ ...f, lines: f.lines.map((l, i) => i === idx ? { ...l, [field]: value } : l) }));
  }

  const lineSubtotal = (l: typeof form.lines[0]) => l.quantity * l.unitPrice * (1 - l.discountPercent / 100);
  const totalAmount = form.lines.reduce((s, l) => s + lineSubtotal(l), 0);

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Invoices</h2>
          <p className="text-xs text-gray-400">Manage customer invoices (AR)</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Invoice
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center gap-3">
          <input
            type="text"
            placeholder="Search invoices..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-52 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
          >
            <option value="">All Statuses</option>
            {(["Draft", "Sent", "PartiallyPaid", "Paid", "Overdue", "Cancelled", "Void"] as InvoiceStatus[]).map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center h-40 text-sm text-gray-400">Loading...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-50">
                  {["Invoice #", "Customer", "Date", "Due Date", "Status", "Total", "Paid", "Balance Due", "Actions"].map((h) => (
                    <th key={h} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {invoices.length === 0 ? (
                  <tr><td colSpan={9} className="text-center py-10 text-gray-400">No invoices found</td></tr>
                ) : invoices.map((inv) => (
                  <>
                    <tr key={inv.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5 font-medium text-primary-600">{inv.invoiceNumber}</td>
                      <td className="px-4 py-2.5">{inv.customerName}</td>
                      <td className="px-4 py-2.5">{inv.invoiceDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">{inv.dueDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[inv.status]}`}>{inv.status}</span>
                      </td>
                      <td className="px-4 py-2.5">{inv.totalAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-green-700">{inv.paidAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 font-medium text-red-600">{(inv.totalAmount - inv.paidAmount).toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <button onClick={() => setExpandedId(expandedId === inv.id ? null : inv.id)} className="text-gray-400 hover:text-primary-600">
                            {expandedId === inv.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                          </button>
                          {inv.status === "Draft" && (
                            <button onClick={() => postMutation.mutate(inv.id)} className="text-blue-500 hover:text-blue-700" title="Post Invoice"><Send size={13} /></button>
                          )}
                          {(inv.status === "Draft" || inv.status === "Sent") && (
                            <button onClick={() => voidMutation.mutate(inv.id)} className="text-red-400 hover:text-red-600" title="Void Invoice"><XCircle size={13} /></button>
                          )}
                        </div>
                      </td>
                    </tr>
                    {expandedId === inv.id && (
                      <tr key={`${inv.id}-lines`} className="bg-gray-50">
                        <td colSpan={9} className="px-6 py-3">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="text-gray-500">
                                <th className="text-left pb-1">Description</th>
                                <th className="text-right pb-1">Qty</th>
                                <th className="text-right pb-1">Unit Price</th>
                                <th className="text-right pb-1">Disc%</th>
                                <th className="text-right pb-1">Tax</th>
                                <th className="text-right pb-1">Line Total</th>
                              </tr>
                            </thead>
                            <tbody>
                              {inv.lines.map((line) => (
                                <tr key={line.id}>
                                  <td className="py-0.5">{line.description}</td>
                                  <td className="py-0.5 text-right">{line.quantity}</td>
                                  <td className="py-0.5 text-right">{line.unitPrice.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                                  <td className="py-0.5 text-right">{line.discountPercent}%</td>
                                  <td className="py-0.5 text-right">{line.taxAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                                  <td className="py-0.5 text-right font-medium">{line.lineTotal.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
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

      <Modal title="New Invoice" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Invoice Number *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.invoiceNumber} onChange={(e) => setForm((f) => ({ ...f, invoiceNumber: e.target.value }))} placeholder="INV-2026-001" />
            </div>
            <div>
              <SearchableDropdown
                label="Customer *"
                options={customerOptions}
                value={form.customerId}
                onChange={(v) => setForm((f) => ({ ...f, customerId: v ?? "" }))}
                placeholder="Select customer"
                searchPlaceholder="Search customers..."
                required
              />
            </div>
          </div>
          <div className="grid grid-cols-3 gap-3">
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
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Invoice Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.invoiceDate} onChange={(e) => setForm((f) => ({ ...f, invoiceDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Due Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.dueDate} onChange={(e) => setForm((f) => ({ ...f, dueDate: e.target.value }))} />
            </div>
          </div>
          <div>
            <SearchableDropdown
              label="Cost Center"
              options={costCenterOptions}
              value={form.costCenterId}
              onChange={(v) => setForm((f) => ({ ...f, costCenterId: v ?? "" }))}
              placeholder="None"
              searchPlaceholder="Search cost centers..."
            />
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Line Items</label>
              <button onClick={addLine} className="text-xs text-primary-600 hover:underline">+ Add Line</button>
            </div>
            <div className="border border-gray-200 rounded-lg">
              <table className="w-full text-xs">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="text-left px-2 py-1.5 text-gray-500">Description</th>
                    <th className="text-right px-2 py-1.5 text-gray-500">Qty</th>
                    <th className="text-right px-2 py-1.5 text-gray-500">Price</th>
                    <th className="text-right px-2 py-1.5 text-gray-500">Disc%</th>
                    <th className="text-left px-2 py-1.5 text-gray-500">Tax</th>
                    <th className="text-left px-2 py-1.5 text-gray-500">Account</th>
                    <th className="text-right px-2 py-1.5 text-gray-500">Total</th>
                    <th className="px-2 py-1.5"></th>
                  </tr>
                </thead>
                <tbody>
                  {form.lines.map((line, idx) => (
                    <tr key={idx} className="border-t border-gray-100">
                      <td className="px-2 py-1">
                        <input className="w-28 border border-gray-200 rounded px-1 py-1 text-xs focus:outline-none" value={line.description} onChange={(e) => updateLine(idx, "description", e.target.value)} />
                      </td>
                      <td className="px-2 py-1">
                        <input type="number" min={0} className="w-12 border border-gray-200 rounded px-1 py-1 text-xs text-right focus:outline-none" value={line.quantity} onChange={(e) => updateLine(idx, "quantity", parseFloat(e.target.value) || 0)} />
                      </td>
                      <td className="px-2 py-1">
                        <input type="number" min={0} className="w-20 border border-gray-200 rounded px-1 py-1 text-xs text-right focus:outline-none" value={line.unitPrice || ""} onChange={(e) => updateLine(idx, "unitPrice", parseFloat(e.target.value) || 0)} />
                      </td>
                      <td className="px-2 py-1">
                        <input type="number" min={0} max={100} className="w-12 border border-gray-200 rounded px-1 py-1 text-xs text-right focus:outline-none" value={line.discountPercent || ""} onChange={(e) => updateLine(idx, "discountPercent", parseFloat(e.target.value) || 0)} />
                      </td>
                      <td className="px-2 py-1">
                        <select className="w-20 border border-gray-200 rounded px-1 py-1 text-xs focus:outline-none" value={line.taxRateId} onChange={(e) => updateLine(idx, "taxRateId", e.target.value)}>
                          <option value="">None</option>
                          {taxRates.map((t) => <option key={t.id} value={t.id}>{t.code}</option>)}
                        </select>
                      </td>
                      <td className="px-2 py-1">
                        <SearchableDropdown
                          options={coaOptions}
                          value={line.revenueAccountId}
                          onChange={(v) => updateLine(idx, "revenueAccountId", v ?? "")}
                          placeholder="Select"
                          searchPlaceholder="Search accounts..."
                        />
                      </td>
                      <td className="px-2 py-1 text-right font-medium">{lineSubtotal(line).toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-2 py-1">
                        {form.lines.length > 1 && <button onClick={() => removeLine(idx)} className="text-red-400 hover:text-red-600">×</button>}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50 border-t border-gray-200">
                  <tr>
                    <td colSpan={6} className="px-2 py-1.5 text-xs text-gray-500 font-medium text-right">Subtotal</td>
                    <td className="px-2 py-1.5 text-xs font-medium text-right">{totalAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                    <td />
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>

          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => createMutation.mutate()} disabled={createMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {createMutation.isPending ? "Saving..." : "Create Invoice"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
