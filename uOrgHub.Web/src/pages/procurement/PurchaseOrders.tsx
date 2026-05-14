import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Search, Send, CheckCircle, XCircle } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import { getPurchaseOrders, createPurchaseOrder, updatePurchaseOrder, deletePurchaseOrder, sendPO, confirmPO, cancelPO, PurchaseOrder, POStatus } from "../../api/procurement";

export default function PurchaseOrders() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<PurchaseOrder | null>(null);
  const [form, setForm] = useState({
    poDate: new Date().toISOString().split("T")[0], expectedDeliveryDate: "",
    vendorId: "", quotationId: "", prId: "", paymentTerms: "", deliveryAddress: "", notes: "",
    items: [] as { itemVariantId: string; orderedQuantity: number; unitPrice: number; taxPercent: number; discountPercent: number; notes: string }[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["purchase-orders", page, search, filterStatus],
    queryFn: () => getPurchaseOrders({ page, pageSize: 10, search },
      filterStatus ? filterStatus as POStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updatePurchaseOrder(editing.id, { ...form, items: form.items.map(i => ({ ...i })) })
      : createPurchaseOrder({ ...form, items: form.items.map(i => ({ ...i })) }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["purchase-orders"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deletePurchaseOrder(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-orders"] }),
  });

  const sendMutation = useMutation({ mutationFn: (id: string) => sendPO(id), onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-orders"] }) });
  const confirmMutation = useMutation({ mutationFn: (id: string) => confirmPO(id), onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-orders"] }) });
  const cancelMutation = useMutation({ mutationFn: (id: string) => cancelPO(id), onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-orders"] }) });

  function openAdd() {
    setEditing(null);
    setForm({
      poDate: new Date().toISOString().split("T")[0], expectedDeliveryDate: "",
      vendorId: "", quotationId: "", prId: "", paymentTerms: "", deliveryAddress: "", notes: "",
      items: [],
    });
    setModal(true);
  }

  function openEdit(po: PurchaseOrder) {
    setEditing(po);
    setForm({
      poDate: po.poDate.split("T")[0], expectedDeliveryDate: po.expectedDeliveryDate.split("T")[0],
      vendorId: po.vendorId, quotationId: po.quotationId ?? "", prId: po.prId ?? "",
      paymentTerms: po.paymentTerms ?? "", deliveryAddress: po.deliveryAddress ?? "", notes: po.notes ?? "",
      items: po.items.map(i => ({ itemVariantId: i.itemVariantId, orderedQuantity: i.orderedQuantity, unitPrice: i.unitPrice, taxPercent: i.taxPercent, discountPercent: i.discountPercent, notes: i.notes ?? "" })),
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  function addItem() {
    setForm(f => ({ ...f, items: [...f.items, { itemVariantId: "", orderedQuantity: 0, unitPrice: 0, taxPercent: 0, discountPercent: 0, notes: "" }] }));
  }

  function updateItem(idx: number, field: string, value: any) {
    const newItems = [...form.items];
    (newItems[idx] as any)[field] = value;
    setForm(f => ({ ...f, items: newItems }));
  }

  function removeItem(idx: number) {
    setForm(f => ({ ...f, items: f.items.filter((_, i) => i !== idx) }));
  }

  const statusColors: Record<string, string> = {
    Draft: "bg-gray-50 text-gray-600", Sent: "bg-blue-50 text-blue-700",
    Confirmed: "bg-green-50 text-green-700", PartiallyReceived: "bg-yellow-50 text-yellow-700",
    FullyReceived: "bg-green-100 text-green-800", Cancelled: "bg-red-50 text-red-700",
  };

  const columns = [
    { key: "poNumber", label: "PO Number", render: (row: PurchaseOrder) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.poNumber}</span> },
    { key: "poDate", label: "PO Date", render: (row: PurchaseOrder) => new Date(row.poDate).toLocaleDateString() },
    { key: "vendorName", label: "Vendor" },
    { key: "prNumber", label: "PR Ref", render: (row: PurchaseOrder) => row.prNumber || "—" },
    { key: "totalAmount", label: "Total", render: (row: PurchaseOrder) => <span className="font-medium">${row.totalAmount.toLocaleString()}</span> },
    { key: "status", label: "Status", render: (row: PurchaseOrder) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    { key: "actions", label: "Actions", render: (row: PurchaseOrder) => (
      <div className="flex items-center gap-1">
        {row.status === "Draft" && (
          <>
            <button onClick={() => sendMutation.mutate(row.id)} title="Send" className="p-1 text-blue-600 hover:bg-blue-50 rounded"><Send size={14} /></button>
            <button onClick={() => openEdit(row)} className="p-1 text-gray-600 hover:bg-gray-50 rounded">✏️</button>
          </>
        )}
        {row.status === "Sent" && (
          <>
            <button onClick={() => confirmMutation.mutate(row.id)} title="Confirm" className="p-1 text-green-600 hover:bg-green-50 rounded"><CheckCircle size={14} /></button>
            <button onClick={() => cancelMutation.mutate(row.id)} title="Cancel" className="p-1 text-red-600 hover:bg-red-50 rounded"><XCircle size={14} /></button>
          </>
        )}
        {row.status !== "Draft" && row.status !== "Sent" && <span className="text-xs text-gray-400">—</span>}
      </div>
    )},
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Purchase Orders</h2>
          <p className="text-xs text-gray-400">Manage purchase orders</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Create PO
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3 flex-wrap">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Search POs..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="pl-8 text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-48" />
          </div>
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5">
            <option value="">All Status</option>
            <option value="Draft">Draft</option>
            <option value="Sent">Sent</option>
            <option value="Confirmed">Confirmed</option>
            <option value="PartiallyReceived">Partially Received</option>
            <option value="FullyReceived">Fully Received</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <DataTable columns={columns} data={items} loading={isLoading} onDelete={(row) => row.status === "Draft" && deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit PO" : "Create Purchase Order"} open={modal} onClose={closeModal}>
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">PO Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.poDate} onChange={(e) => setForm((f) => ({ ...f, poDate: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Expected Delivery *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.expectedDeliveryDate} onChange={(e) => setForm((f) => ({ ...f, expectedDeliveryDate: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Vendor ID *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.vendorId} onChange={(e) => setForm((f) => ({ ...f, vendorId: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Quotation Ref</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.quotationId} onChange={(e) => setForm((f) => ({ ...f, quotationId: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">PR Reference</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.prId} onChange={(e) => setForm((f) => ({ ...f, prId: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Payment Terms</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.paymentTerms} onChange={(e) => setForm((f) => ({ ...f, paymentTerms: e.target.value }))} /></div>
          </div>
          <div><label className="text-xs text-gray-500 mb-1 block">Delivery Address</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.deliveryAddress} onChange={(e) => setForm((f) => ({ ...f, deliveryAddress: e.target.value }))} /></div>
          <div><label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} /></div>
          <div className="border-t pt-3">
            <div className="flex items-center justify-between mb-2"><label className="text-xs text-gray-500">Items</label>
              <button onClick={addItem} type="button" className="text-xs text-primary-500">+ Add Item</button></div>
            {form.items.map((item, idx) => (
              <div key={idx} className="grid grid-cols-6 gap-2 mb-2 items-end p-2 bg-gray-50 rounded-lg">
                <input placeholder="Item ID" className="border border-gray-200 rounded px-2 py-1 text-xs col-span-2" value={item.itemVariantId} onChange={(e) => updateItem(idx, "itemVariantId", e.target.value)} />
                <input type="number" placeholder="Qty" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.orderedQuantity} onChange={(e) => updateItem(idx, "orderedQuantity", parseFloat(e.target.value))} />
                <input type="number" placeholder="Price" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.unitPrice} onChange={(e) => updateItem(idx, "unitPrice", parseFloat(e.target.value))} />
                <button onClick={() => removeItem(idx)} className="text-red-500">✕</button>
              </div>))}
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}