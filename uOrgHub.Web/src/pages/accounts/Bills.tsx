import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, CheckCircle, XCircle, ChevronDown, ChevronUp } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import {
  getBills,
  createBill,
  approveBill,
  voidBill,
  getVendors,
  getFiscalYears,
  getTaxRates,
  getChartOfAccounts,
  getCostCenters,
  Bill,
  BillStatus,
} from "../../api/accounts";

const statusColors: Record<BillStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Received: "bg-blue-50 text-blue-700",
  PartiallyPaid: "bg-yellow-50 text-yellow-700",
  Paid: "bg-green-50 text-green-700",
  Overdue: "bg-red-50 text-red-700",
  Cancelled: "bg-gray-100 text-gray-400",
  Void: "bg-red-100 text-red-500",
};

export default function Bills() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [modal, setModal] = useState(false);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [form, setForm] = useState({
    billNumber: "",
    vendorBillNumber: "",
    vendorId: "",
    fiscalYearId: "",
    billDate: new Date().toISOString().split("T")[0],
    dueDate: "",
    notes: "",
    costCenterId: "",
    lines: [{ description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: 1, taxRateId: "", expenseAccountId: "", costCenterId: "" }],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["bills", page, search, statusFilter],
    queryFn: () => getBills({ page, pageSize: 10, search }, undefined, statusFilter || undefined),
  });

  const { data: vendorsData } = useQuery({ queryKey: ["vendors", 1, ""], queryFn: () => getVendors({ page: 1, pageSize: 200 }) });
  const { data: fiscalYearsData } = useQuery({ queryKey: ["fiscal-years", 1, ""], queryFn: () => getFiscalYears({ page: 1, pageSize: 50 }) });
  const { data: taxRatesData } = useQuery({ queryKey: ["tax-rates", 1, ""], queryFn: () => getTaxRates({ page: 1, pageSize: 100 }) });
  const { data: accountsData } = useQuery({ queryKey: ["chart-of-accounts", 1, ""], queryFn: () => getChartOfAccounts({ page: 1, pageSize: 200 }) });
  const { data: costCentersData } = useQuery({ queryKey: ["cost-centers", 1, ""], queryFn: () => getCostCenters({ page: 1, pageSize: 100 }) });

  const bills = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const vendors = vendorsData?.data?.data?.items ?? [];
  const fiscalYears = fiscalYearsData?.data?.data?.items ?? [];
  const taxRates = taxRatesData?.data?.data?.items ?? [];
  const coaAccounts = accountsData?.data?.data?.items ?? [];
  const costCenters = costCentersData?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => {
      const payload = {
        ...form,
        vendorBillNumber: form.vendorBillNumber || undefined,
        costCenterId: form.costCenterId || undefined,
        lines: form.lines.map((l, i) => ({ ...l, lineOrder: i + 1, taxRateId: l.taxRateId || undefined, costCenterId: l.costCenterId || undefined })),
      };
      return createBill(payload);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["bills"] }); closeModal(); },
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => approveBill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["bills"] }),
  });

  const voidMutation = useMutation({
    mutationFn: (id: string) => voidBill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["bills"] }),
  });

  function openAdd() {
    const currentFY = fiscalYears.find((fy) => fy.isCurrent);
    setForm({ billNumber: "", vendorBillNumber: "", vendorId: vendors[0]?.id ?? "", fiscalYearId: currentFY?.id ?? fiscalYears[0]?.id ?? "", billDate: new Date().toISOString().split("T")[0], dueDate: "", notes: "", costCenterId: "", lines: [{ description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: 1, taxRateId: "", expenseAccountId: coaAccounts[0]?.id ?? "", costCenterId: "" }] });
    setModal(true);
  }

  function closeModal() { setModal(false); }

  function addLine() {
    setForm((f) => ({ ...f, lines: [...f.lines, { description: "", quantity: 1, unitPrice: 0, discountPercent: 0, lineOrder: f.lines.length + 1, taxRateId: "", expenseAccountId: coaAccounts[0]?.id ?? "", costCenterId: "" }] }));
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
          <h2 className="text-base font-medium text-gray-900">Bills</h2>
          <p className="text-xs text-gray-400">Manage vendor bills (AP)</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Bill
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex items-center gap-3">
          <input
            type="text"
            placeholder="Search bills..."
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
            {(["Draft", "Received", "PartiallyPaid", "Paid", "Overdue", "Cancelled", "Void"] as BillStatus[]).map((s) => (
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
                  {["Bill #", "Vendor", "Vendor Bill #", "Date", "Due Date", "Status", "Total", "Paid", "Balance Due", "Actions"].map((h) => (
                    <th key={h} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {bills.length === 0 ? (
                  <tr><td colSpan={10} className="text-center py-10 text-gray-400">No bills found</td></tr>
                ) : bills.map((bill) => (
                  <>
                    <tr key={bill.id} className="border-t border-gray-100 hover:bg-gray-50">
                      <td className="px-4 py-2.5 font-medium text-primary-600">{bill.billNumber}</td>
                      <td className="px-4 py-2.5">{bill.vendorName}</td>
                      <td className="px-4 py-2.5 text-gray-500">{bill.vendorBillNumber ?? "—"}</td>
                      <td className="px-4 py-2.5">{bill.billDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">{bill.dueDate?.split("T")[0]}</td>
                      <td className="px-4 py-2.5">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[bill.status]}`}>{bill.status}</span>
                      </td>
                      <td className="px-4 py-2.5">{bill.totalAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-green-700">{bill.paidAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 font-medium text-red-600">{(bill.totalAmount - bill.paidAmount).toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <button onClick={() => setExpandedId(expandedId === bill.id ? null : bill.id)} className="text-gray-400 hover:text-primary-600">
                            {expandedId === bill.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                          </button>
                          {bill.status === "Draft" && (
                            <button onClick={() => approveMutation.mutate(bill.id)} className="text-green-500 hover:text-green-700" title="Approve"><CheckCircle size={13} /></button>
                          )}
                          {(bill.status === "Draft" || bill.status === "Received") && (
                            <button onClick={() => voidMutation.mutate(bill.id)} className="text-red-400 hover:text-red-600" title="Void"><XCircle size={13} /></button>
                          )}
                        </div>
                      </td>
                    </tr>
                    {expandedId === bill.id && (
                      <tr key={`${bill.id}-lines`} className="bg-gray-50">
                        <td colSpan={10} className="px-6 py-3">
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
                              {bill.lines.map((line) => (
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

      <Modal title="New Bill" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Bill Number *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.billNumber} onChange={(e) => setForm((f) => ({ ...f, billNumber: e.target.value }))} placeholder="BL-2026-001" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Vendor Bill Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.vendorBillNumber} onChange={(e) => setForm((f) => ({ ...f, vendorBillNumber: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Vendor *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.vendorId} onChange={(e) => setForm((f) => ({ ...f, vendorId: e.target.value }))}>
                <option value="">Select vendor</option>
                {vendors.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Fiscal Year *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.fiscalYearId} onChange={(e) => setForm((f) => ({ ...f, fiscalYearId: e.target.value }))}>
                <option value="">Select year</option>
                {fiscalYears.map((fy) => <option key={fy.id} value={fy.id}>{fy.name}</option>)}
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Bill Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.billDate} onChange={(e) => setForm((f) => ({ ...f, billDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Due Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.dueDate} onChange={(e) => setForm((f) => ({ ...f, dueDate: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Cost Center</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.costCenterId} onChange={(e) => setForm((f) => ({ ...f, costCenterId: e.target.value }))}>
              <option value="">None</option>
              {costCenters.map((cc) => <option key={cc.id} value={cc.id}>{cc.name}</option>)}
            </select>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Line Items</label>
              <button onClick={addLine} className="text-xs text-primary-600 hover:underline">+ Add Line</button>
            </div>
            <div className="border border-gray-200 rounded-lg overflow-hidden">
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
                      <td className="px-2 py-1"><input className="w-28 border border-gray-200 rounded px-1 py-1 text-xs" value={line.description} onChange={(e) => updateLine(idx, "description", e.target.value)} /></td>
                      <td className="px-2 py-1"><input type="number" min={0} className="w-12 border border-gray-200 rounded px-1 py-1 text-xs text-right" value={line.quantity} onChange={(e) => updateLine(idx, "quantity", parseFloat(e.target.value) || 0)} /></td>
                      <td className="px-2 py-1"><input type="number" min={0} className="w-20 border border-gray-200 rounded px-1 py-1 text-xs text-right" value={line.unitPrice || ""} onChange={(e) => updateLine(idx, "unitPrice", parseFloat(e.target.value) || 0)} /></td>
                      <td className="px-2 py-1"><input type="number" min={0} max={100} className="w-12 border border-gray-200 rounded px-1 py-1 text-xs text-right" value={line.discountPercent || ""} onChange={(e) => updateLine(idx, "discountPercent", parseFloat(e.target.value) || 0)} /></td>
                      <td className="px-2 py-1"><select className="w-20 border border-gray-200 rounded px-1 py-1 text-xs" value={line.taxRateId} onChange={(e) => updateLine(idx, "taxRateId", e.target.value)}><option value="">None</option>{taxRates.map((t) => <option key={t.id} value={t.id}>{t.code}</option>)}</select></td>
                      <td className="px-2 py-1"><select className="w-24 border border-gray-200 rounded px-1 py-1 text-xs" value={line.expenseAccountId} onChange={(e) => updateLine(idx, "expenseAccountId", e.target.value)}><option value="">Select</option>{coaAccounts.map((a) => <option key={a.id} value={a.id}>{a.accountCode}</option>)}</select></td>
                      <td className="px-2 py-1 text-right font-medium">{lineSubtotal(line).toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                      <td className="px-2 py-1">{form.lines.length > 1 && <button onClick={() => removeLine(idx)} className="text-red-400 hover:text-red-600">×</button>}</td>
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
              {createMutation.isPending ? "Saving..." : "Create Bill"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
