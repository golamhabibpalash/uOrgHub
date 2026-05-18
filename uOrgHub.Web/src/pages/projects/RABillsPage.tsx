import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowLeft, Trash2, Send, CheckCircle, DollarSign } from "lucide-react";
import Modal from "../../components/shared/Modal";
import ProjectNav from "../../components/projects/ProjectNav";
import {
  getRABills,
  createRABill,
  submitRABill,
  certifyRABill,
  markRABillPaid,
  deleteRABill,
  RABill,
  RABillItem,
} from "../../api/projects";

const STATUSES = ["Draft", "Submitted", "UnderReview", "Certified", "Paid", "Rejected"];

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Submitted: "bg-blue-50 text-blue-700",
  UnderReview: "bg-amber-50 text-amber-700",
  Certified: "bg-purple-50 text-purple-700",
  Paid: "bg-green-50 text-green-700",
  Rejected: "bg-red-50 text-red-700",
};

const emptyItem = (): Partial<RABillItem> => ({
  boqItemDescription: "",
  uom: "",
  previousQuantity: 0,
  currentQuantity: 0,
  rate: 0,
});

export default function RABillsPage() {
  const { id: projectId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const [page] = useState(1);
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [certifyModal, setCertifyModal] = useState(false);
  const [selected, setSelected] = useState<RABill | null>(null);
  const [form, setForm] = useState({
    billDate: new Date().toISOString().substring(0, 10),
    retentionPercent: 5,
    deductionAmount: 0,
    items: [emptyItem()],
  });
  const [certifyForm, setCertifyForm] = useState({ certifiedAmount: 0, certificationNotes: "" });

  const { data, isLoading } = useQuery({
    queryKey: ["rabills", projectId, filterStatus, page],
    queryFn: () => getRABills({ page, pageSize: 50 }, projectId, filterStatus || undefined),
    enabled: !!projectId,
  });

  const bills = data?.data?.data?.items ?? [];

  const createMutation = useMutation({
    mutationFn: () => createRABill({ ...form, projectId }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["rabills", projectId] }); closeModal(); },
  });

  const submitMutation = useMutation({
    mutationFn: (id: string) => submitRABill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rabills", projectId] }),
  });

  const certifyMutation = useMutation({
    mutationFn: () => certifyRABill(selected!.id, certifyForm),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["rabills", projectId] }); setCertifyModal(false); },
  });

  const markPaidMutation = useMutation({
    mutationFn: (id: string) => markRABillPaid(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rabills", projectId] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteRABill(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rabills", projectId] }),
  });

  function openCreate() {
    setForm({ billDate: new Date().toISOString().substring(0, 10), retentionPercent: 5, deductionAmount: 0, items: [emptyItem()] });
    setModal(true);
  }

  function openCertify(bill: RABill) {
    setSelected(bill);
    setCertifyForm({ certifiedAmount: bill.netAmount, certificationNotes: "" });
    setCertifyModal(true);
  }

  function closeModal() {
    setModal(false);
    setForm({ billDate: new Date().toISOString().substring(0, 10), retentionPercent: 5, deductionAmount: 0, items: [emptyItem()] });
  }

  function addItem() {
    setForm((f) => ({ ...f, items: [...f.items, emptyItem()] }));
  }

  function removeItem(idx: number) {
    setForm((f) => ({ ...f, items: f.items.filter((_, i) => i !== idx) }));
  }

  function updateItem(idx: number, field: string, value: string | number) {
    setForm((f) => ({
      ...f,
      items: f.items.map((item, i) => (i === idx ? { ...item, [field]: value } : item)),
    }));
  }

  const grossAmount = form.items.reduce((sum, item) => {
    const qty = (item.currentQuantity ?? 0);
    const rate = (item.rate ?? 0);
    return sum + qty * rate;
  }, 0);
  const retention = (grossAmount * form.retentionPercent) / 100;
  const net = grossAmount - form.deductionAmount - retention;

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
          <p className="text-xs text-gray-400">Manage progressive billing and payment certificates</p>
        </div>
        <button onClick={openCreate} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Create RA Bill
        </button>
      </div>

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
          bills.map((bill) => (
            <div key={bill.id} className="bg-white border border-gray-200 rounded-xl p-4">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-medium text-gray-900 text-sm">{bill.billNumber}</span>
                    <span className="text-xs text-gray-400">Bill #{bill.billSequence}</span>
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[bill.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {bill.status}
                    </span>
                  </div>
                  <p className="text-xs text-gray-400">{new Date(bill.billDate).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2">
                  {bill.status === "Draft" && (
                    <button onClick={() => { if (confirm("Submit this bill?")) submitMutation.mutate(bill.id); }}
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
                  {bill.status === "Certified" && (
                    <button onClick={() => { if (confirm("Mark this bill as paid?")) markPaidMutation.mutate(bill.id); }}
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

              <div className="grid grid-cols-5 gap-4 mt-3 pt-3 border-t border-gray-100">
                <div>
                  <p className="text-xs text-gray-400">Gross Amount</p>
                  <p className="text-sm font-medium text-gray-900">BDT {bill.grossAmount.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-400">Deductions</p>
                  <p className="text-sm text-gray-700">BDT {bill.deductionAmount.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-400">Retention ({bill.retentionPercent}%)</p>
                  <p className="text-sm text-gray-700">BDT {bill.retentionAmount.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-400">Net Amount</p>
                  <p className="text-sm font-semibold text-primary-600">BDT {bill.netAmount.toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-400">Cumulative Billed</p>
                  <p className="text-sm text-gray-700">BDT {bill.cumulativeBilledAmount.toLocaleString()}</p>
                </div>
              </div>

              {bill.items.length > 0 && (
                <div className="mt-3">
                  <table className="w-full text-xs">
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
                          <td className="py-1 text-gray-700">{item.boqItemDescription || "—"}</td>
                          <td className="py-1 text-gray-600">{item.uom}</td>
                          <td className="py-1 text-right text-gray-600">{item.previousQuantity.toLocaleString()}</td>
                          <td className="py-1 text-right text-gray-700">{item.currentQuantity.toLocaleString()}</td>
                          <td className="py-1 text-right text-gray-700">{item.rate.toLocaleString()}</td>
                          <td className="py-1 text-right font-medium text-gray-900">{item.amount.toLocaleString()}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {bill.certificationNotes && (
                <p className="text-xs text-gray-500 mt-2 pt-2 border-t border-gray-100">
                  Certification Notes: {bill.certificationNotes}
                </p>
              )}
            </div>
          ))
        )}
      </div>

      <Modal title="Create RA Bill" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Bill Date *</label>
              <input type="date" value={form.billDate} onChange={(e) => setForm((f) => ({ ...f, billDate: e.target.value }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Retention %</label>
              <input type="number" value={form.retentionPercent} onChange={(e) => setForm((f) => ({ ...f, retentionPercent: parseFloat(e.target.value) || 0 }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Deductions (BDT)</label>
              <input type="number" value={form.deductionAmount} onChange={(e) => setForm((f) => ({ ...f, deductionAmount: parseFloat(e.target.value) || 0 }))}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Bill Items *</label>
              <button onClick={addItem} className="text-xs text-primary-600 hover:text-primary-700 font-medium">+ Add Item</button>
            </div>
            <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
              {form.items.map((item, idx) => (
                <div key={idx} className="border border-gray-100 rounded-lg p-2 space-y-1">
                  <div className="flex gap-1 items-start">
                    <input value={item.boqItemDescription ?? ""} onChange={(e) => updateItem(idx, "boqItemDescription", e.target.value)}
                      placeholder="Item description" className="flex-1 border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none" />
                    <input value={item.uom ?? ""} onChange={(e) => updateItem(idx, "uom", e.target.value)}
                      placeholder="UOM" className="w-16 border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none" />
                    <button onClick={() => removeItem(idx)} className="text-gray-300 hover:text-red-500 mt-0.5">
                      <Trash2 size={12} />
                    </button>
                  </div>
                  <div className="flex gap-1">
                    <div className="flex-1">
                      <span className="text-[10px] text-gray-400">Prev Qty</span>
                      <input type="number" value={item.previousQuantity ?? 0}
                        onChange={(e) => updateItem(idx, "previousQuantity", parseFloat(e.target.value) || 0)}
                        className="w-full border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none" />
                    </div>
                    <div className="flex-1">
                      <span className="text-[10px] text-gray-400">Curr Qty</span>
                      <input type="number" value={item.currentQuantity ?? 0}
                        onChange={(e) => updateItem(idx, "currentQuantity", parseFloat(e.target.value) || 0)}
                        className="w-full border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none" />
                    </div>
                    <div className="flex-1">
                      <span className="text-[10px] text-gray-400">Rate</span>
                      <input type="number" value={item.rate ?? 0}
                        onChange={(e) => updateItem(idx, "rate", parseFloat(e.target.value) || 0)}
                        className="w-full border border-gray-200 rounded px-2 py-1 text-xs focus:outline-none" />
                    </div>
                    <div className="flex-1">
                      <span className="text-[10px] text-gray-400">Amount</span>
                      <p className="text-xs font-medium text-gray-900 px-2 py-1">
                        {((item.currentQuantity ?? 0) * (item.rate ?? 0)).toLocaleString()}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-gray-50 rounded-lg p-3 text-xs space-y-1">
            <div className="flex justify-between"><span className="text-gray-500">Gross Amount</span><span className="font-medium">BDT {grossAmount.toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Deductions</span><span>BDT {form.deductionAmount.toLocaleString()}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Retention ({form.retentionPercent}%)</span><span>BDT {retention.toFixed(2)}</span></div>
            <div className="flex justify-between border-t border-gray-200 pt-1 font-medium"><span>Net Amount</span><span className="text-primary-600">BDT {net.toFixed(2)}</span></div>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => createMutation.mutate()} disabled={createMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {createMutation.isPending ? "Creating..." : "Create RA Bill"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Certify RA Bill" open={certifyModal} onClose={() => setCertifyModal(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Certified Amount (BDT) *</label>
            <input type="number" value={certifyForm.certifiedAmount}
              onChange={(e) => setCertifyForm((f) => ({ ...f, certifiedAmount: parseFloat(e.target.value) || 0 }))}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Certification Notes</label>
            <textarea value={certifyForm.certificationNotes}
              onChange={(e) => setCertifyForm((f) => ({ ...f, certificationNotes: e.target.value }))} rows={3}
              className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => setCertifyModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => certifyMutation.mutate()} disabled={certifyMutation.isPending}
              className="px-4 py-2 text-sm bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50">
              {certifyMutation.isPending ? "Certifying..." : "Certify Bill"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
