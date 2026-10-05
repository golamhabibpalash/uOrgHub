import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { Plus, ArrowLeft, Trash2, Send, CheckCircle, DollarSign, FileText, AlertCircle } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import ContractAccountPanel from "../../components/projects/ContractAccountPanel";
import { money, paymentStateBadge } from "../../components/projects/contractAccount";
import {
  getRABills,
  createRABill,
  submitRABill,
  certifyRABill,
  raiseRABillInvoice,
  markRABillPaid,
  deleteRABill,
  getContractAccount,
  RABill,
} from "../../api/projects";
import { useChartOfAccountsLookup } from "../../hooks/useEntityLookup";
import { useAuthStore } from "../../store/authStore";
import { extractApiError } from "../../utils/apiError";
import { formatDate } from "../../utils/format";
import DateInput from "../../components/shared/DateInput";

const STATUSES = ["Draft", "Submitted", "UnderReview", "Certified", "Paid", "Rejected"];

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Submitted: "bg-blue-50 text-blue-700",
  UnderReview: "bg-amber-50 text-amber-700",
  Certified: "bg-purple-50 text-purple-700",
  Paid: "bg-green-50 text-green-700",
  Rejected: "bg-red-50 text-red-700",
};

const inputCls = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";
const today = () => new Date().toISOString().substring(0, 10);

interface ItemForm {
  description: string;
  unitOfMeasure: string;
  previousQuantity: number;
  currentQuantity: number;
  rate: number;
}
const emptyItem = (): ItemForm => ({ description: "", unitOfMeasure: "", previousQuantity: 0, currentQuantity: 0, rate: 0 });

const emptyForm = () => ({
  title: "",
  billDate: today(),
  periodFrom: "",
  periodTo: today(),
  retentionPercent: 5,
  notes: "",
  items: [emptyItem()],
});

export default function RABillsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const userId = useAuthStore((s) => s.user?.id);
  const { hasClaim, hasRole } = useAuthStore();
  const canInvoice = hasRole("Admin") || hasClaim("Accounts.Invoices.Create");
  const { options: incomeOptions } = useChartOfAccountsLookup("Income");

  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState("");

  const [certifying, setCertifying] = useState<RABill | null>(null);
  const [certifyForm, setCertifyForm] = useState({
    grossAmount: 0,
    deductionAmount: 0,
    certifiedDate: today(),
    raiseInvoice: true,
    revenueAccountId: "",
    dueDate: "",
  });
  const [certifyError, setCertifyError] = useState("");

  const [invoicing, setInvoicing] = useState<RABill | null>(null);
  const [invoiceForm, setInvoiceForm] = useState({ revenueAccountId: "", dueDate: "" });

  const { data, isLoading } = useQuery({
    queryKey: ["rabills", projectId, filterStatus],
    queryFn: () => getRABills({ page: 1, pageSize: 100 }, projectId, filterStatus || undefined),
    enabled: !!projectId,
  });
  const { data: caData } = useQuery({
    queryKey: ["contract-account", projectId],
    queryFn: () => getContractAccount(projectId!),
    enabled: !!projectId,
  });
  const contract = caData?.data?.data;
  const bills = data?.data?.data?.items ?? [];

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ["rabills", projectId] });
    qc.invalidateQueries({ queryKey: ["contract-account", projectId] });
    qc.invalidateQueries({ queryKey: ["invoices"] });
  };

  const createMutation = useMutation({
    mutationFn: () =>
      createRABill({
        projectId: projectId!,
        title: form.title.trim(),
        billDate: form.billDate,
        periodFrom: form.periodFrom,
        periodTo: form.periodTo,
        submittedById: userId!,
        retentionPercent: form.retentionPercent,
        notes: form.notes || undefined,
        items: form.items
          .filter((i) => i.description.trim())
          .map((i, idx) => ({ ...i, description: i.description.trim(), unitOfMeasure: i.unitOfMeasure || undefined, sequence: idx + 1 })),
      }),
    onSuccess: () => { refresh(); setModal(false); },
    onError: (err) => setFormError(extractApiError(err)),
  });

  const submitMutation = useMutation({
    mutationFn: (id: string) => submitRABill(id),
    onSuccess: refresh,
    onError: (err) => toast.error(extractApiError(err)),
  });

  // Certify, then (optionally) raise the invoice. If invoicing fails the bill stays certified and
  // the "Raise invoice" button on it lets the user retry once the problem is fixed.
  const certifyMutation = useMutation({
    mutationFn: async () => {
      const certified = await certifyRABill(certifying!.id, {
        certifiedById: userId!,
        certifiedDate: certifyForm.certifiedDate,
        grossAmount: certifyForm.grossAmount,
        deductionAmount: certifyForm.deductionAmount,
      });
      if (certified.data.data?.warning) toast(certified.data.data.warning, { icon: "⚠️" });
      if (certifyForm.raiseInvoice && canInvoice) {
        try {
          const invoiced = await raiseRABillInvoice(certifying!.id, {
            revenueAccountId: certifyForm.revenueAccountId,
            dueDate: certifyForm.dueDate || undefined,
          });
          toast.success(`Certified and invoiced as ${invoiced.data.data?.invoiceNumber}.`);
        } catch (err) {
          toast.error(`Certified, but the invoice was not raised: ${extractApiError(err)}`);
        }
      }
    },
    onSuccess: () => { refresh(); setCertifying(null); },
    onError: (err) => setCertifyError(extractApiError(err)),
  });

  const raiseMutation = useMutation({
    mutationFn: () => raiseRABillInvoice(invoicing!.id, { revenueAccountId: invoiceForm.revenueAccountId, dueDate: invoiceForm.dueDate || undefined }),
    onSuccess: (res) => { refresh(); setInvoicing(null); toast.success(`Invoice ${res.data.data?.invoiceNumber} raised and posted.`); },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const markPaidMutation = useMutation({
    mutationFn: (id: string) => markRABillPaid(id),
    onSuccess: refresh,
    onError: (err) => toast.error(extractApiError(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteRABill(id),
    onSuccess: refresh,
    onError: (err) => toast.error(extractApiError(err)),
  });

  function openCreate() {
    setForm(emptyForm());
    setFormError("");
    setModal(true);
  }

  function submitCreate() {
    if (!form.title.trim()) return setFormError("Enter a title for the bill.");
    if (!form.periodFrom || !form.periodTo || form.periodTo <= form.periodFrom)
      return setFormError("Choose the work period; its end must be after its start.");
    if (!form.items.some((i) => i.description.trim() && i.currentQuantity > 0))
      return setFormError("Add at least one item with a description and a current quantity.");
    setFormError("");
    createMutation.mutate();
  }

  function openCertify(bill: RABill) {
    setCertifying(bill);
    setCertifyError("");
    setCertifyForm({
      grossAmount: bill.grossAmount,
      deductionAmount: bill.deductionAmount,
      certifiedDate: today(),
      raiseInvoice: canInvoice && Boolean(contract?.customerId),
      revenueAccountId: contract?.defaultRevenueAccountId ?? "",
      dueDate: "",
    });
  }

  const updateItem = (idx: number, patch: Partial<ItemForm>) =>
    setForm((f) => ({ ...f, items: f.items.map((it, i) => (i === idx ? { ...it, ...patch } : it)) }));

  const grossAmount = form.items.reduce((s, i) => s + i.currentQuantity * i.rate, 0);
  const retention = (grossAmount * form.retentionPercent) / 100;

  const certNet = certifying
    ? (certifyForm.grossAmount - certifyForm.deductionAmount) * (1 - certifying.retentionPercent / 100)
    : 0;
  const certRetention = certifying ? (certifyForm.grossAmount - certifyForm.deductionAmount) * (certifying.retentionPercent / 100) : 0;

  return (
    <div>
      <div className="mb-4">
        <Link to={`/projects/${projectId}`} className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700">
          <ArrowLeft size={16} /> Back to Project
        </Link>
      </div>

      <ProjectNav />

      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">RA Bills (Running Account Bills)</h2>
          <p className="text-xs text-gray-400">Bill the client for work done, invoice it, and track what's been received</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Create RA Bill
        </button>
      </div>

      {projectId && <ContractAccountPanel projectId={projectId} />}

      <div className="flex gap-3 mb-4">
        <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}
          className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500">
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>

      <div className="space-y-3">
        {isLoading ? (
          <div className="text-center py-10 text-gray-400">Loading...</div>
        ) : bills.length === 0 ? (
          <div className="text-center py-10 text-gray-400 bg-white border border-gray-200 rounded-xl">No RA Bills found</div>
        ) : (
          bills.map((bill) => {
            const badge = paymentStateBadge[bill.paymentState];
            const invoicedLive = bill.invoiceNumber && bill.paymentState !== "InvoiceVoid";
            return (
              <div key={bill.id} className="bg-white border border-gray-200 rounded-xl p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <div className="flex flex-wrap items-center gap-2 mb-1">
                      <span className="font-medium text-gray-900 text-sm">{bill.billNumber}</span>
                      <span className="text-xs text-gray-400">Bill #{bill.billSequence}</span>
                      <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[bill.status] ?? "bg-gray-100 text-gray-600"}`}>
                        {bill.status}
                      </span>
                      {(bill.status === "Certified" || bill.status === "Paid") && badge && (
                        <span className={`text-xs px-2 py-0.5 rounded-full ${badge.cls}`}>{badge.label}</span>
                      )}
                    </div>
                    <p className="text-sm text-gray-700">{bill.title}</p>
                    <p className="text-xs text-gray-400">
                      {formatDate(bill.billDate)} · work {formatDate(bill.periodFrom)} – {formatDate(bill.periodTo)}
                    </p>
                  </div>
                  <div className="flex flex-wrap items-center gap-2">
                    {bill.status === "Draft" && (
                      <button onClick={() => { if (confirm("Submit this bill for certification?")) submitMutation.mutate(bill.id); }}
                        disabled={submitMutation.isPending}
                        className="flex items-center gap-1 px-3 py-1.5 text-sm border border-blue-200 text-blue-700 rounded-lg hover:bg-blue-50">
                        <Send size={13} /> Submit
                      </button>
                    )}
                    {(bill.status === "Submitted" || bill.status === "UnderReview") && (
                      <button onClick={() => openCertify(bill)}
                        className="flex items-center gap-1 px-3 py-1.5 text-sm border border-purple-200 text-purple-700 rounded-lg hover:bg-purple-50">
                        <CheckCircle size={13} /> Certify
                      </button>
                    )}
                    {bill.status === "Certified" && !invoicedLive && canInvoice && (
                      <button
                        onClick={() => { setInvoiceForm({ revenueAccountId: contract?.defaultRevenueAccountId ?? "", dueDate: "" }); setInvoicing(bill); }}
                        className="flex items-center gap-1 px-3 py-1.5 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600">
                        <FileText size={13} /> Raise invoice
                      </button>
                    )}
                    {/* Only for bills settled outside AR; an invoiced bill is paid by recording payments. */}
                    {bill.status === "Certified" && !invoicedLive && (
                      <button onClick={() => { if (confirm("Mark this bill as paid without an invoice?")) markPaidMutation.mutate(bill.id); }}
                        className="flex items-center gap-1 px-3 py-1.5 text-sm border border-green-200 text-green-700 rounded-lg hover:bg-green-50">
                        <DollarSign size={13} /> Mark Paid
                      </button>
                    )}
                    {bill.status === "Draft" && (
                      <button onClick={() => { if (confirm("Delete this bill?")) deleteMutation.mutate(bill.id); }}
                        className="text-gray-400 hover:text-red-600"><Trash2 size={14} /></button>
                    )}
                  </div>
                </div>

                <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mt-3 pt-3 border-t border-gray-100">
                  <div>
                    <p className="text-xs text-gray-400">Gross Amount</p>
                    <p className="text-sm font-medium text-gray-900 tabular-nums">{money(bill.grossAmount)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-400">Deductions</p>
                    <p className="text-sm text-gray-700 tabular-nums">{money(bill.deductionAmount)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-400">Retention ({bill.retentionPercent}%)</p>
                    <p className="text-sm text-gray-700 tabular-nums">{money(bill.retentionAmount)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-400">Net Amount</p>
                    <p className="text-sm font-semibold text-primary-600 tabular-nums">{money(bill.netAmount)}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-400">Cumulative Billed</p>
                    <p className="text-sm text-gray-700 tabular-nums">{money(bill.cumulativeBilledAmount)}</p>
                  </div>
                </div>

                {bill.invoiceNumber && (
                  <div className="flex flex-wrap items-center gap-x-6 gap-y-1 mt-3 pt-3 border-t border-gray-100 text-xs">
                    <span className="text-gray-500">
                      Invoice <Link to="/accounts/invoices" className="font-mono text-primary-600 hover:underline">{bill.invoiceNumber}</Link>
                      {bill.paymentState === "InvoiceVoid" && " (void)"}
                    </span>
                    <span className="text-gray-500">Invoiced <b className="text-gray-800 tabular-nums">{money(bill.invoiceTotal)}</b></span>
                    <span className="text-gray-500">Received <b className="text-green-700 tabular-nums">{money(bill.invoicePaid)}</b></span>
                    <span className="text-gray-500">Balance <b className="text-red-600 tabular-nums">{money(bill.invoiceBalance)}</b></span>
                    {bill.paymentState !== "Paid" && bill.paymentState !== "InvoiceVoid" && (
                      <Link to="/accounts/payments" className="text-primary-600 hover:underline">Record payment →</Link>
                    )}
                  </div>
                )}

                {bill.items.length > 0 && (
                  <table className="w-full text-xs mt-3">
                    <thead>
                      <tr className="text-gray-400">
                        <th className="text-left pb-1">Description</th>
                        <th className="text-left pb-1">UOM</th>
                        <th className="text-right pb-1">Prev Qty</th>
                        <th className="text-right pb-1">Curr Qty</th>
                        <th className="text-right pb-1">Rate</th>
                        <th className="text-right pb-1">Amount</th>
                      </tr>
                    </thead>
                    <tbody>
                      {bill.items.map((item) => (
                        <tr key={item.id} className="border-t border-gray-50">
                          <td className="py-1 text-gray-700">{item.description || "—"}</td>
                          <td className="py-1 text-gray-600">{item.unitOfMeasure}</td>
                          <td className="py-1 text-right text-gray-600 tabular-nums">{item.previousQuantity.toLocaleString()}</td>
                          <td className="py-1 text-right text-gray-700 tabular-nums">{item.currentQuantity.toLocaleString()}</td>
                          <td className="py-1 text-right text-gray-700 tabular-nums">{money(item.rate)}</td>
                          <td className="py-1 text-right font-medium text-gray-900 tabular-nums">{money(item.amount)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}

                {bill.notes && <p className="text-xs text-gray-500 mt-2 pt-2 border-t border-gray-100">Notes: {bill.notes}</p>}
              </div>
            );
          })
        )}
      </div>

      {/* Create */}
      <Modal title="Create RA Bill" open={modal} onClose={() => setModal(false)} size="3xl">
        <div className="space-y-3">
          {formError && (
            <div className="flex gap-2 text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              <AlertCircle size={16} className="shrink-0 mt-0.5" /> {formError}
            </div>
          )}
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))} className={inputCls}
              placeholder="e.g. Foundation & ground floor slab" maxLength={300} />
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Bill Date *</label>
              <DateInput value={form.billDate} onChange={(e) => setForm((f) => ({ ...f, billDate: e.target.value }))} className={inputCls} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Work From *</label>
              <DateInput value={form.periodFrom} onChange={(e) => setForm((f) => ({ ...f, periodFrom: e.target.value }))} className={inputCls} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Work To *</label>
              <DateInput value={form.periodTo} onChange={(e) => setForm((f) => ({ ...f, periodTo: e.target.value }))} className={inputCls} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Retention %</label>
              <input type="number" min={0} max={100} value={form.retentionPercent}
                onChange={(e) => setForm((f) => ({ ...f, retentionPercent: parseFloat(e.target.value) || 0 }))} className={inputCls} />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Work done *</label>
              <button onClick={() => setForm((f) => ({ ...f, items: [...f.items, emptyItem()] }))} className="text-xs text-primary-600 hover:text-primary-700 font-medium">+ Add Item</button>
            </div>
            <div className="space-y-2 max-h-72 overflow-y-auto pr-1">
              {form.items.map((item, idx) => (
                <div key={idx} className="grid grid-cols-12 gap-2 items-end border border-gray-100 rounded-lg p-2">
                  <div className="col-span-12 md:col-span-5">
                    <span className="text-[10px] text-gray-400">Description</span>
                    <input value={item.description} onChange={(e) => updateItem(idx, { description: e.target.value })}
                      placeholder="Item of work" className="w-full border border-gray-200 rounded px-2 py-1 text-sm focus:outline-none" />
                  </div>
                  <div className="col-span-3 md:col-span-1">
                    <span className="text-[10px] text-gray-400">UOM</span>
                    <input value={item.unitOfMeasure} onChange={(e) => updateItem(idx, { unitOfMeasure: e.target.value })}
                      className="w-full border border-gray-200 rounded px-2 py-1 text-sm focus:outline-none" />
                  </div>
                  {(["previousQuantity", "currentQuantity", "rate"] as const).map((k) => (
                    <div key={k} className="col-span-3 md:col-span-2">
                      <span className="text-[10px] text-gray-400">{{ previousQuantity: "Prev Qty", currentQuantity: "This Qty", rate: "Rate" }[k]}</span>
                      <input type="number" min={0} step="any" value={item[k] || ""}
                        onChange={(e) => updateItem(idx, { [k]: parseFloat(e.target.value) || 0 })}
                        className="w-full border border-gray-200 rounded px-2 py-1 text-sm text-right focus:outline-none" />
                    </div>
                  ))}
                  <div className="col-span-12 md:col-span-12 flex justify-between text-xs">
                    <span className="text-gray-500">Amount <b className="text-gray-900 tabular-nums">{money(item.currentQuantity * item.rate)}</b></span>
                    {form.items.length > 1 && (
                      <button onClick={() => setForm((f) => ({ ...f, items: f.items.filter((_, i) => i !== idx) }))} className="text-gray-400 hover:text-red-500">
                        <Trash2 size={12} />
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <input value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} className={inputCls} maxLength={500} />
          </div>

          <div className="bg-gray-50 rounded-lg p-3 text-xs space-y-1">
            <div className="flex justify-between"><span className="text-gray-500">Gross Amount</span><span className="font-medium tabular-nums">{money(grossAmount)}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Retention ({form.retentionPercent}%)</span><span className="tabular-nums">{money(retention)}</span></div>
            <div className="flex justify-between border-t border-gray-200 pt-1 font-medium"><span>Net (before deductions)</span><span className="text-primary-600 tabular-nums">{money(grossAmount - retention)}</span></div>
            <p className="text-[11px] text-gray-400">Deductions are entered when the bill is certified.</p>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={submitCreate} disabled={createMutation.isPending || !userId}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {createMutation.isPending ? "Creating..." : "Create RA Bill"}
            </button>
          </div>
        </div>
      </Modal>

      {/* Certify (+ invoice) */}
      <Modal title={`Certify ${certifying?.billNumber ?? ""}`} open={certifying !== null} onClose={() => setCertifying(null)} size="lg">
        <div className="space-y-3">
          {certifyError && (
            <div className="text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{certifyError}</div>
          )}
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Certified gross *</label>
              <input type="number" min={0} step="0.01" value={certifyForm.grossAmount || ""}
                onChange={(e) => setCertifyForm((f) => ({ ...f, grossAmount: parseFloat(e.target.value) || 0 }))} className={`${inputCls} text-right`} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Deductions</label>
              <input type="number" min={0} step="0.01" value={certifyForm.deductionAmount || ""}
                onChange={(e) => setCertifyForm((f) => ({ ...f, deductionAmount: parseFloat(e.target.value) || 0 }))} className={`${inputCls} text-right`} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Certified on</label>
              <DateInput value={certifyForm.certifiedDate} onChange={(e) => setCertifyForm((f) => ({ ...f, certifiedDate: e.target.value }))} className={inputCls} />
            </div>
          </div>
          <div className="bg-gray-50 rounded-lg p-3 text-xs space-y-1">
            <div className="flex justify-between"><span className="text-gray-500">Retention ({certifying?.retentionPercent}%)</span><span className="tabular-nums">{money(certRetention)}</span></div>
            <div className="flex justify-between font-medium"><span>Net payable by client</span><span className="text-primary-600 tabular-nums">{money(certNet)}</span></div>
          </div>

          {canInvoice && (
            <div className={`border rounded-xl p-3 space-y-3 ${certifyForm.raiseInvoice ? "border-primary-200 bg-primary-50/40" : "border-gray-200"}`}>
              <label className="flex items-start gap-2 text-sm cursor-pointer">
                <input type="checkbox" className="mt-0.5 h-4 w-4 accent-primary-500" checked={certifyForm.raiseInvoice}
                  disabled={!contract?.customerId}
                  onChange={(e) => setCertifyForm((f) => ({ ...f, raiseInvoice: e.target.checked }))} />
                <span>
                  <span className="font-medium text-gray-900">Raise AR invoice now</span>
                  <span className="block text-xs text-gray-500">
                    {contract?.customerId
                      ? `Posts an invoice for the net ${money(certNet)} to ${contract.customerName} on this project.`
                      : "Link the client to an Accounts customer first (Projects → Clients)."}
                  </span>
                </span>
              </label>
              {certifyForm.raiseInvoice && (
                <div className="grid grid-cols-2 gap-3">
                  <SearchableDropdown label="Revenue account *" options={incomeOptions} value={certifyForm.revenueAccountId}
                    onChange={(v) => setCertifyForm((f) => ({ ...f, revenueAccountId: v ?? "" }))}
                    placeholder="e.g. Construction Revenue" searchPlaceholder="Search accounts..." />
                  <div>
                    <label className="text-xs text-gray-500 mb-1 block">Due date</label>
                    <DateInput value={certifyForm.dueDate} onChange={(e) => setCertifyForm((f) => ({ ...f, dueDate: e.target.value }))} className={inputCls} />
                    <p className="text-[11px] text-gray-400 mt-0.5">Blank = customer's payment terms</p>
                  </div>
                </div>
              )}
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setCertifying(null)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => certifyMutation.mutate()}
              disabled={certifyMutation.isPending || !userId || !(certifyForm.grossAmount > 0)
                || (certifyForm.raiseInvoice && canInvoice && !certifyForm.revenueAccountId)}
              className="px-4 py-2 text-sm bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50">
              {certifyMutation.isPending ? "Saving..." : certifyForm.raiseInvoice && canInvoice ? "Certify & invoice" : "Certify bill"}
            </button>
          </div>
        </div>
      </Modal>

      {/* Raise invoice for an already-certified bill */}
      <Modal title={`Raise invoice — ${invoicing?.billNumber ?? ""}`} open={invoicing !== null} onClose={() => setInvoicing(null)} size="md">
        <div className="space-y-3">
          <p className="text-sm text-gray-600">
            Posts an invoice for the net <b>{money(invoicing?.netAmount ?? 0)}</b> to {contract?.customerName ?? "the client's customer"}.
          </p>
          <SearchableDropdown label="Revenue account *" options={incomeOptions} value={invoiceForm.revenueAccountId}
            onChange={(v) => setInvoiceForm((f) => ({ ...f, revenueAccountId: v ?? "" }))}
            placeholder="e.g. Construction Revenue" searchPlaceholder="Search accounts..." />
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Due date</label>
            <DateInput value={invoiceForm.dueDate} onChange={(e) => setInvoiceForm((f) => ({ ...f, dueDate: e.target.value }))} className={inputCls} />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setInvoicing(null)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => raiseMutation.mutate()} disabled={!invoiceForm.revenueAccountId || raiseMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {raiseMutation.isPending ? "Raising..." : "Raise invoice"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
