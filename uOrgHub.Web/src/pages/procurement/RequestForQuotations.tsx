import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getRFQs, createRFQ, updateRFQ, deleteRFQ, RequestForQuotation, RFQStatus } from "../../api/procurement";

export default function RequestForQuotations() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "rfqDate" });
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<RequestForQuotation | null>(null);
  const [form, setForm] = useState({
    rfqDate: new Date().toISOString().split("T")[0],
    closingDate: "",
    prId: "",
    title: "",
    description: "",
    notes: "",
    status: "Draft" as RFQStatus,
    items: [] as { itemVariantId: string; requestedQuantity: number; notes: string }[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["rfqs", ...dg.queryKey, filterStatus],
    queryFn: () => getRFQs(dg.queryParams,
      filterStatus ? filterStatus as RFQStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updateRFQ(editing.id, { ...form, items: form.items.map(i => ({ ...i })) })
      : createRFQ({ ...form, items: form.items.map(i => ({ ...i })) }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["rfqs"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteRFQ(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["rfqs"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({
      rfqDate: new Date().toISOString().split("T")[0],
      closingDate: "",
      prId: "",
      title: "",
      description: "",
      notes: "",
      status: "Draft",
      items: [],
    });
    setModal(true);
  }

  function openEdit(rfq: RequestForQuotation) {
    setEditing(rfq);
    setForm({
      rfqDate: rfq.rfqDate.split("T")[0],
      closingDate: rfq.closingDate.split("T")[0],
      prId: rfq.prId ?? "",
      title: rfq.title,
      description: rfq.description ?? "",
      notes: rfq.notes ?? "",
      status: rfq.status,
      items: rfq.items.map(i => ({ itemVariantId: i.itemVariantId, requestedQuantity: i.requestedQuantity, notes: i.notes ?? "" })),
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  function addItem() {
    setForm(f => ({ ...f, items: [...f.items, { itemVariantId: "", requestedQuantity: 0, notes: "" }] }));
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
    Draft: "bg-gray-50 text-gray-600",
    Sent: "bg-blue-50 text-blue-700",
    Closed: "bg-green-50 text-green-700",
    Cancelled: "bg-red-50 text-red-700",
  };

  const columns = [
    { key: "rfqNumber", label: "RFQ Number", render: (row: RequestForQuotation) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.rfqNumber}</span> },
    { key: "rfqDate", label: "RFQ Date", render: (row: RequestForQuotation) => new Date(row.rfqDate).toLocaleDateString() },
    { key: "closingDate", label: "Closing Date", render: (row: RequestForQuotation) => new Date(row.closingDate).toLocaleDateString() },
    { key: "title", label: "Title" },
    { key: "prNumber", label: "PR Ref", render: (row: RequestForQuotation) => row.prNumber || "—" },
    { key: "status", label: "Status", sortable: false, render: (row: RequestForQuotation) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    {
      key: "actions", label: "", sortable: false, render: (row: RequestForQuotation) => (
        <div className="flex gap-1">
          <button onClick={() => openEdit(row)} className="p-1 text-blue-600 hover:bg-blue-50 rounded">✏️</button>
          {row.status === "Draft" && <button onClick={() => deleteMutation.mutate(row.id)} className="p-1 text-red-600 hover:bg-red-50 rounded">🗑️</button>}
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Request for Quotations</h2>
          <p className="text-xs text-gray-400">Manage RFQs</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Create RFQ
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
        searchPlaceholder="Search RFQs..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No RFQs found"
        toolbarPrefix={
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); dg.setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5">
            <option value="">All Status</option>
            <option value="Draft">Draft</option>
            <option value="Sent">Sent</option>
            <option value="Closed">Closed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        }
        actions={<ExportMenu baseUrl="rfqs" filters={{ search: dg.search || undefined, status: filterStatus || undefined }} />}
      />

      <Modal title={editing ? "Edit RFQ" : "Create RFQ"} open={modal} onClose={closeModal}>
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">RFQ Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
                value={form.rfqDate} onChange={(e) => setForm((f) => ({ ...f, rfqDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Closing Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
                value={form.closingDate} onChange={(e) => setForm((f) => ({ ...f, closingDate: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Title *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
              value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">PR Reference</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
                value={form.prId} onChange={(e) => setForm((f) => ({ ...f, prId: e.target.value }))} />
            </div>
            {editing && (
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Status</label>
                <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
                  value={form.status} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value as RFQStatus }))}>
                  <option value="Draft">Draft</option>
                  <option value="Sent">Sent</option>
                  <option value="Closed">Closed</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
            )}
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
              value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>
          <div className="border-t pt-3">
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Items</label>
              <button onClick={addItem} type="button" className="text-xs text-primary-500">+ Add Item</button>
            </div>
            {form.items.map((item, idx) => (
              <div key={idx} className="grid grid-cols-4 gap-2 mb-2 items-end p-2 bg-gray-50 rounded-lg">
                <input placeholder="Item Variant ID" className="border border-gray-200 rounded px-2 py-1 text-xs"
                  value={item.itemVariantId} onChange={(e) => updateItem(idx, "itemVariantId", e.target.value)} />
                <input type="number" placeholder="Qty" className="border border-gray-200 rounded px-2 py-1 text-xs"
                  value={item.requestedQuantity} onChange={(e) => updateItem(idx, "requestedQuantity", parseFloat(e.target.value))} />
                <input placeholder="Notes" className="border border-gray-200 rounded px-2 py-1 text-xs"
                  value={item.notes} onChange={(e) => updateItem(idx, "notes", e.target.value)} />
                <button onClick={() => removeItem(idx)} className="text-red-500">✕</button>
              </div>
            ))}
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}