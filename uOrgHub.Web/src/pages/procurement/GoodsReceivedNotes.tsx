import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Search, CheckCircle } from "lucide-react";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import { getGRNs, createGRN, updateGRN, deleteGRN, confirmGRN, GoodsReceivedNote, GRNStatus } from "../../api/procurement";

export default function GoodsReceivedNotes() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<GoodsReceivedNote | null>(null);
  const [form, setForm] = useState({
    grnDate: new Date().toISOString().split("T")[0], poId: "", warehouseId: "", receivedById: "",
    notes: "", invoiceNumber: "", invoiceDate: "",
    items: [] as { poItemId: string; itemVariantId: string; orderedQuantity: number; receivedQuantity: number; rejectedQuantity: number; unitCost: number; notes: string }[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["grns", page, search, filterStatus],
    queryFn: () => getGRNs({ page, pageSize: 10, search },
      filterStatus ? filterStatus as GRNStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateGRN(editing.id, { ...form, items: form.items.map(i => ({ ...i })) })
      : createGRN({ ...form, items: form.items.map(i => ({ ...i })) }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["grns"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteGRN(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["grns"] }),
  });

  const confirmMutation = useMutation({
    mutationFn: (id: string) => confirmGRN(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["grns"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      grnDate: new Date().toISOString().split("T")[0], poId: "", warehouseId: "", receivedById: "",
      notes: "", invoiceNumber: "", invoiceDate: "",
      items: [],
    });
    setModal(true);
  }

  function openEdit(grn: GoodsReceivedNote) {
    setEditing(grn);
    setForm({
      grnDate: grn.grnDate.split("T")[0], poId: grn.poId, warehouseId: grn.warehouseId, receivedById: grn.receivedById,
      notes: grn.notes ?? "", invoiceNumber: grn.invoiceNumber ?? "", invoiceDate: grn.invoiceDate?.split("T")[0] ?? "",
      items: grn.items.map(i => ({ poItemId: i.poItemId, itemVariantId: i.itemVariantId, orderedQuantity: i.orderedQuantity, receivedQuantity: i.receivedQuantity, rejectedQuantity: i.rejectedQuantity, unitCost: i.unitCost, notes: i.notes ?? "" })),
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  function addItem() {
    setForm(f => ({ ...f, items: [...f.items, { poItemId: "", itemVariantId: "", orderedQuantity: 0, receivedQuantity: 0, rejectedQuantity: 0, unitCost: 0, notes: "" }] }));
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
    Draft: "bg-gray-50 text-gray-600", Confirmed: "bg-green-50 text-green-700", Cancelled: "bg-red-50 text-red-700",
  };

  const columns = [
    { key: "grnNumber", label: "GRN Number", render: (row: GoodsReceivedNote) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.grnNumber}</span> },
    { key: "grnDate", label: "GRN Date", render: (row: GoodsReceivedNote) => new Date(row.grnDate).toLocaleDateString() },
    { key: "poNumber", label: "PO Number" },
    { key: "warehouseName", label: "Warehouse" },
    { key: "receivedByName", label: "Received By" },
    { key: "invoiceNumber", label: "Invoice", render: (row: GoodsReceivedNote) => row.invoiceNumber || "—" },
    { key: "status", label: "Status", render: (row: GoodsReceivedNote) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    { key: "actions", label: "Actions", render: (row: GoodsReceivedNote) => (
      <div className="flex items-center gap-1">
        {row.status === "Draft" && (
          <>
            <button onClick={() => confirmMutation.mutate(row.id)} title="Confirm" className="p-1 text-green-600 hover:bg-green-50 rounded"><CheckCircle size={14} /></button>
            <button onClick={() => openEdit(row)} className="p-1 text-gray-600 hover:bg-gray-50 rounded">✏️</button>
          </>
        )}
        {row.status !== "Draft" && <span className="text-xs text-gray-400">—</span>}
      </div>
    )},
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Goods Received Notes</h2>
          <p className="text-xs text-gray-400">Manage GRNs</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Create GRN
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100 flex gap-3 flex-wrap">
          <div className="relative">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
            <input type="text" placeholder="Search GRNs..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }}
              className="pl-8 text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-48" />
          </div>
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5">
            <option value="">All Status</option>
            <option value="Draft">Draft</option>
            <option value="Confirmed">Confirmed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <DataTable columns={columns} data={items} loading={isLoading} onDelete={(row) => row.status === "Draft" && deleteMutation.mutate(row.id)} />
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title={editing ? "Edit GRN" : "Create GRN"} open={modal} onClose={closeModal} size="lg">
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">GRN Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.grnDate} onChange={(e) => setForm((f) => ({ ...f, grnDate: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">PO ID *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.poId} onChange={(e) => setForm((f) => ({ ...f, poId: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Warehouse ID *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.warehouseId} onChange={(e) => setForm((f) => ({ ...f, warehouseId: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Received By ID *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.receivedById} onChange={(e) => setForm((f) => ({ ...f, receivedById: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Invoice Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.invoiceNumber} onChange={(e) => setForm((f) => ({ ...f, invoiceNumber: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Invoice Date</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.invoiceDate} onChange={(e) => setForm((f) => ({ ...f, invoiceDate: e.target.value }))} /></div>
          </div>
          <div><label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} /></div>
          <div className="border-t pt-3">
            <div className="flex items-center justify-between mb-2"><label className="text-xs text-gray-500">Items</label>
              <button onClick={addItem} type="button" className="text-xs text-primary-500">+ Add Item</button></div>
            {form.items.map((item, idx) => (
              <div key={idx} className="grid grid-cols-7 gap-2 mb-2 items-end p-2 bg-gray-50 rounded-lg">
                <input placeholder="PO Item" className="border border-gray-200 rounded px-2 py-1 text-xs col-span-2" value={item.poItemId} onChange={(e) => updateItem(idx, "poItemId", e.target.value)} />
                <input type="number" placeholder="Ordered" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.orderedQuantity} onChange={(e) => updateItem(idx, "orderedQuantity", parseFloat(e.target.value))} />
                <input type="number" placeholder="Received" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.receivedQuantity} onChange={(e) => updateItem(idx, "receivedQuantity", parseFloat(e.target.value))} />
                <input type="number" placeholder="Rejected" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.rejectedQuantity} onChange={(e) => updateItem(idx, "rejectedQuantity", parseFloat(e.target.value))} />
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