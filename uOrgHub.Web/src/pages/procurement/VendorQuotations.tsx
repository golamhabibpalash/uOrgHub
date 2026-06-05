import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getVendorQuotations, createVendorQuotation, updateVendorQuotation, deleteVendorQuotation, VendorQuotation, QuotationStatus } from "../../api/procurement";

export default function VendorQuotations() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "quotationDate" });
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<VendorQuotation | null>(null);
  const [form, setForm] = useState({
    rfqId: "", vendorId: "", quotationDate: new Date().toISOString().split("T")[0],
    validUntil: "", deliveryDays: 0, paymentTerms: "", notes: "",
    status: "Received" as QuotationStatus,
    items: [] as { rfqItemId: string; itemVariantId: string; quotedQuantity: number; unitPrice: number; notes: string }[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["vendor-quotations", dg.page, dg.search, dg.sortBy, dg.sortDescending, filterStatus],
    queryFn: () => getVendorQuotations(dg.queryParams,
      filterStatus ? filterStatus as QuotationStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateVendorQuotation(editing.id, { ...form, items: form.items.map(i => ({ ...i })) })
      : createVendorQuotation({ ...form, items: form.items.map(i => ({ ...i })) }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["vendor-quotations"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteVendorQuotation(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["vendor-quotations"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      rfqId: "", vendorId: "", quotationDate: new Date().toISOString().split("T")[0],
      validUntil: "", deliveryDays: 0, paymentTerms: "", notes: "",
      status: "Received", items: [],
    });
    setModal(true);
  }

  function openEdit(q: VendorQuotation) {
    setEditing(q);
    setForm({
      rfqId: q.rfqId, vendorId: q.vendorId,
      quotationDate: q.quotationDate.split("T")[0], validUntil: q.validUntil.split("T")[0],
      deliveryDays: q.deliveryDays, paymentTerms: q.paymentTerms ?? "", notes: q.notes ?? "",
      status: q.status,
      items: q.items.map(i => ({ rfqItemId: i.rfqItemId, itemVariantId: i.itemVariantId, quotedQuantity: i.quotedQuantity, unitPrice: i.unitPrice, notes: i.notes ?? "" })),
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  function addItem() {
    setForm(f => ({ ...f, items: [...f.items, { rfqItemId: "", itemVariantId: "", quotedQuantity: 0, unitPrice: 0, notes: "" }] }));
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
    Received: "bg-blue-50 text-blue-700", Evaluated: "bg-yellow-50 text-yellow-700",
    Accepted: "bg-green-50 text-green-700", Rejected: "bg-red-50 text-red-700",
  };

  const columns = [
    { key: "quotationNumber", label: "Quote #", render: (row: VendorQuotation) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.quotationNumber}</span> },
    { key: "rfqNumber", label: "RFQ #", render: (row: VendorQuotation) => row.rfqNumber || "—" },
    { key: "vendorName", label: "Vendor" },
    { key: "quotationDate", label: "Quote Date", render: (row: VendorQuotation) => new Date(row.quotationDate).toLocaleDateString() },
    { key: "validUntil", label: "Valid Until", render: (row: VendorQuotation) => new Date(row.validUntil).toLocaleDateString() },
    { key: "totalAmount", label: "Total", render: (row: VendorQuotation) => <span className="font-medium">${row.totalAmount.toLocaleString()}</span> },
    { key: "status", label: "Status", sortable: false, render: (row: VendorQuotation) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    { key: "actions", label: "", sortable: false, render: (row: VendorQuotation) => (
      <div className="flex gap-1">
        <button onClick={() => openEdit(row)} className="p-1 text-blue-600 hover:bg-blue-50 rounded">✏️</button>
        {row.status === "Received" && <button onClick={() => deleteMutation.mutate(row.id)} className="p-1 text-red-600 hover:bg-red-50 rounded">🗑️</button>}
      </div>
    )},
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Vendor Quotations</h2>
          <p className="text-xs text-gray-400">Manage vendor quotations</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Quotation
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No quotations found"
        toolbarPrefix={
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); dg.setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5">
            <option value="">All Status</option>
            <option value="Received">Received</option>
            <option value="Evaluated">Evaluated</option>
            <option value="Accepted">Accepted</option>
            <option value="Rejected">Rejected</option>
          </select>
        }
        actions={<ExportMenu baseUrl="vendorquotations" filters={{ search: dg.search || undefined, status: filterStatus || undefined }} />}
      />

      <Modal title={editing ? "Edit Quotation" : "Add Quotation"} open={modal} onClose={closeModal}>
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">RFQ ID *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.rfqId} onChange={(e) => setForm((f) => ({ ...f, rfqId: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Vendor ID *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.vendorId} onChange={(e) => setForm((f) => ({ ...f, vendorId: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Quote Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.quotationDate} onChange={(e) => setForm((f) => ({ ...f, quotationDate: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Valid Until *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.validUntil} onChange={(e) => setForm((f) => ({ ...f, validUntil: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">Delivery Days</label>
              <input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.deliveryDays} onChange={(e) => setForm((f) => ({ ...f, deliveryDays: parseInt(e.target.value) }))} /></div>
            {editing && <div><label className="text-xs text-gray-500 mb-1 block">Status</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.status} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value as QuotationStatus }))}>
                <option value="Received">Received</option><option value="Evaluated">Evaluated</option><option value="Accepted">Accepted</option><option value="Rejected">Rejected</option>
              </select></div>}
          </div>
          <div><label className="text-xs text-gray-500 mb-1 block">Payment Terms</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.paymentTerms} onChange={(e) => setForm((f) => ({ ...f, paymentTerms: e.target.value }))} /></div>
          <div><label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} /></div>
          <div className="border-t pt-3">
            <div className="flex items-center justify-between mb-2"><label className="text-xs text-gray-500">Items</label>
              <button onClick={addItem} type="button" className="text-xs text-primary-500">+ Add Item</button></div>
            {form.items.map((item, idx) => (
              <div key={idx} className="grid grid-cols-5 gap-2 mb-2 items-end p-2 bg-gray-50 rounded-lg">
                <input placeholder="RFQItem ID" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.rfqItemId} onChange={(e) => updateItem(idx, "rfqItemId", e.target.value)} />
                <input placeholder="Item ID" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.itemVariantId} onChange={(e) => updateItem(idx, "itemVariantId", e.target.value)} />
                <input type="number" placeholder="Qty" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.quotedQuantity} onChange={(e) => updateItem(idx, "quotedQuantity", parseFloat(e.target.value))} />
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