import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, XCircle, ChevronDown, ChevronUp, Pencil } from "lucide-react";
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
  getInvoiceById,
  createInvoice,
  updateInvoice,
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
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState({
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
  const taxRateOptions = useMemo(
    () => taxRates.map((t) => ({ value: t.id, label: t.code })),
    [taxRates],
  );
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

  const updateMutation = useMutation({
    mutationFn: () => {
      if (!editingId) throw new Error("No invoice selected for edit");
      const payload = {
        invoiceDate: form.invoiceDate,
        dueDate: form.dueDate,
        notes: form.notes || undefined,
        costCenterId: form.costCenterId || undefined,
        lines: form.lines.map((l, i) => ({
          description: l.description,
          quantity: l.quantity,
          unitPrice: l.unitPrice,
          discountPercent: l.discountPercent,
          lineOrder: i + 1,
          taxRateId: l.taxRateId || undefined,
          revenueAccountId: l.revenueAccountId,
          costCenterId: l.costCenterId || undefined,
        })),
      };
      return updateInvoice(editingId, payload as Parameters<typeof updateInvoice>[1]);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["invoices"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to update invoice.";
      setSaveError(msg);
    },
  });

  function openAdd() {
    setEditingId(null);
    setForm({
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

  async function openEdit(id: string) {
    setEditingId(id);
    setSaveError("");
    try {
      const res = await getInvoiceById(id);
      const inv = res.data.data;
      if (!inv) { setSaveError("Invoice not found."); return; }
      setForm({
        customerId: inv.customerId,
        fiscalYearId: inv.fiscalYearId,
        invoiceDate: inv.invoiceDate.split("T")[0],
        dueDate: inv.dueDate.split("T")[0],
        notes: inv.notes ?? "",
        costCenterId: inv.costCenterId ?? "",
        lines: inv.lines.length > 0
          ? inv.lines.map((l) => ({
              description: l.description,
              quantity: l.quantity,
              unitPrice: l.unitPrice,
              discountPercent: l.discountPercent,
              lineOrder: l.lineOrder,
              taxRateId: l.taxRateId ?? "",
              revenueAccountId: l.revenueAccountId,
              costCenterId: l.costCenterId ?? "",
            }))
          : [{ description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: 1, taxRateId: "", revenueAccountId: "", costCenterId: "" }],
      });
      setModal(true);
    } catch {
      setSaveError("Failed to load invoice data.");
    }
  }

  function closeModal() { setModal(false); setSaveError(""); setEditingId(null); }

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
                          <button onClick={() => setExpandedId(expandedId === inv.id ? null : inv.id)} className="text-gray-400 hover:text-primary-600" title="View Details">
                            {expandedId === inv.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                          </button>
                          {inv.status === "Draft" && (
                            <>
                              <button onClick={() => openEdit(inv.id)} className="text-gray-500 hover:text-primary-600" title="Edit Invoice"><Pencil size={13} /></button>
                              <button onClick={() => postMutation.mutate(inv.id)} className="text-blue-500 hover:text-blue-700" title="Post Invoice"><Send size={13} /></button>
                            </>
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

      <Modal title={editingId ? "Edit Invoice" : "New Invoice"} open={modal} onClose={closeModal} size="4xl">
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Invoice Number</label>
              <div className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-500">Auto-generated on save</div>
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
              <label className="text-xs font-medium text-gray-600">Line Items</label>
              <button onClick={addLine} className="text-xs text-primary-600 hover:underline flex items-center gap-1">+ Add Line</button>
            </div>
            <div className="space-y-2">
              {form.lines.map((line, idx) => (
                <div key={idx} className="border border-gray-200 rounded-lg p-3 bg-white">
                  <div className="flex items-start gap-2 mb-2">
                    <div className="flex-1">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Description</label>
                      <input
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={line.description}
                        onChange={(e) => updateLine(idx, "description", e.target.value)}
                        placeholder="Item description"
                      />
                    </div>
                    {form.lines.length > 1 && (
                      <button onClick={() => removeLine(idx)} className="mt-5 text-red-400 hover:text-red-600 p-1 rounded hover:bg-red-50 shrink-0" title="Remove line">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/></svg>
                      </button>
                    )}
                  </div>
                  <div className="grid grid-cols-12 gap-2 items-end">
                    <div className="col-span-2">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Qty</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={line.quantity}
                        onChange={(e) => updateLine(idx, "quantity", parseFloat(e.target.value) || 0)}
                      />
                    </div>
                    <div className="col-span-2">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Unit Price</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={line.unitPrice || ""}
                        onChange={(e) => updateLine(idx, "unitPrice", parseFloat(e.target.value) || 0)}
                      />
                    </div>
                    <div className="col-span-2">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Disc %</label>
                      <input type="number" min={0} max={100}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={line.discountPercent || ""}
                        onChange={(e) => updateLine(idx, "discountPercent", parseFloat(e.target.value) || 0)}
                      />
                    </div>
                    <div className="col-span-3">
                      <SearchableDropdown
                        options={taxRateOptions}
                        value={line.taxRateId}
                        onChange={(v) => updateLine(idx, "taxRateId", v ?? "")}
                        placeholder="Tax rate"
                        searchPlaceholder="Search tax rates..."
                        buttonClassName="py-1.5 text-sm"
                      />
                    </div>
                    <div className="col-span-3">
                      <SearchableDropdown
                        options={coaOptions}
                        value={line.revenueAccountId}
                        onChange={(v) => updateLine(idx, "revenueAccountId", v ?? "")}
                        placeholder="Revenue account"
                        searchPlaceholder="Search accounts..."
                        buttonClassName="py-1.5 text-sm"
                      />
                    </div>
                  </div>
                  <div className="mt-2 text-right">
                    <span className="text-xs text-gray-500">Line total: </span>
                    <span className="text-sm font-semibold text-gray-900">{lineSubtotal(line).toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
                  </div>
                </div>
              ))}
            </div>
            <div className="flex justify-end items-center gap-2 mt-2 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg">
              <span className="text-sm text-gray-600 font-medium">Subtotal</span>
              <span className="text-base font-bold text-gray-900">{totalAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
            </div>
          </div>

          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            {editingId ? (
              <button onClick={() => updateMutation.mutate()} disabled={updateMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
                {updateMutation.isPending ? "Saving..." : "Update Invoice"}
              </button>
            ) : (
              <button onClick={() => createMutation.mutate()} disabled={createMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
                {createMutation.isPending ? "Saving..." : "Create Invoice"}
              </button>
            )}
          </div>
        </div>
      </Modal>
    </div>
  );
}
