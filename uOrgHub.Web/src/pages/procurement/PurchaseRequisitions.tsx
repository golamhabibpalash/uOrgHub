import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, CheckCircle, XCircle } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import { useDataGrid } from "../../hooks/useDataGrid";
import { getPurchaseRequisitions, createPurchaseRequisition, updatePurchaseRequisition, deletePurchaseRequisition, submitPR, approvePR, rejectPR, PurchaseRequisition, PRStatus } from "../../api/procurement";

export default function PurchaseRequisitions() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "prDate" });
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<PurchaseRequisition | null>(null);
  const [rejectModal, setRejectModal] = useState(false);
  const [rejectId, setRejectId] = useState<string>("");
  const [rejectReason, setRejectReason] = useState("");
  const [form, setForm] = useState({
    prDate: new Date().toISOString().split("T")[0],
    requiredDate: "",
    departmentId: "",
    requestedById: "",
    purpose: "",
    notes: "",
    items: [] as { itemVariantId: string; warehouseId: string; requestedQuantity: number; estimatedUnitCost: number; notes: string }[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["purchase-requisitions", ...dg.queryKey, filterStatus],
    queryFn: () => getPurchaseRequisitions(dg.queryParams,
      filterStatus ? filterStatus as PRStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing
      ? updatePurchaseRequisition(editing.id, { ...form, items: form.items.map(i => ({ ...i })) })
      : createPurchaseRequisition({ ...form, items: form.items.map(i => ({ ...i })) }),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["purchase-requisitions"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deletePurchaseRequisition(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-requisitions"] }),
  });

  const submitMutation = useMutation({
    mutationFn: (id: string) => submitPR(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-requisitions"] }),
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => approvePR(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["purchase-requisitions"] }),
  });

  const rejectMutation = useMutation({
    mutationFn: () => rejectPR(rejectId, rejectReason),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["purchase-requisitions"] }); setRejectModal(false); setRejectReason(""); },
  });

  function openAdd() {
    setEditing(null);
    setForm({
      prDate: new Date().toISOString().split("T")[0],
      requiredDate: "",
      departmentId: "",
      requestedById: "",
      purpose: "",
      notes: "",
      items: [],
    });
    setModal(true);
  }

  function openEdit(pr: PurchaseRequisition) {
    setEditing(pr);
    setForm({
      prDate: pr.prDate.split("T")[0],
      requiredDate: pr.requiredDate.split("T")[0],
      departmentId: pr.departmentId,
      requestedById: pr.requestedById,
      purpose: pr.purpose ?? "",
      notes: pr.notes ?? "",
      items: pr.items.map(i => ({
        itemVariantId: i.itemVariantId,
        warehouseId: i.warehouseId,
        requestedQuantity: i.requestedQuantity,
        estimatedUnitCost: i.estimatedUnitCost,
        notes: i.notes ?? "",
      })),
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  function addItem() {
    setForm(f => ({ ...f, items: [...f.items, { itemVariantId: "", warehouseId: "", requestedQuantity: 0, estimatedUnitCost: 0, notes: "" }] }));
  }

  function updateItem(index: number, field: string, value: any) {
    const newItems = [...form.items];
    (newItems[index] as any)[field] = value;
    setForm(f => ({ ...f, items: newItems }));
  }

  function removeItem(index: number) {
    setForm(f => ({ ...f, items: f.items.filter((_, i) => i !== index) }));
  }

  const statusColors: Record<string, string> = {
    Draft: "bg-gray-50 text-gray-600",
    Submitted: "bg-blue-50 text-blue-700",
    Approved: "bg-green-50 text-green-700",
    Rejected: "bg-red-50 text-red-700",
    Converted: "bg-purple-50 text-purple-700",
  };

  const columns = [
    { key: "prNumber", label: "PR Number", render: (row: PurchaseRequisition) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.prNumber}</span> },
    { key: "prDate", label: "PR Date", render: (row: PurchaseRequisition) => new Date(row.prDate).toLocaleDateString() },
    { key: "requiredDate", label: "Required", render: (row: PurchaseRequisition) => new Date(row.requiredDate).toLocaleDateString() },
    { key: "departmentName", label: "Department" },
    { key: "requestedByName", label: "Requested By" },
    { key: "status", label: "Status", sortable: false, render: (row: PurchaseRequisition) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    {
      key: "actions", label: "Actions", sortable: false, render: (row: PurchaseRequisition) => (
        <div className="flex items-center gap-1">
          {row.status === "Draft" && (
            <>
              <button onClick={() => submitMutation.mutate(row.id)} title="Submit" className="p-1 text-blue-600 hover:bg-blue-50 rounded"><Send size={14} /></button>
              <button onClick={() => openEdit(row)} title="Edit" className="p-1 text-gray-600 hover:bg-gray-50 rounded">✏️</button>
              <button onClick={() => deleteMutation.mutate(row.id)} className="p-1 text-red-600 hover:bg-red-50 rounded">🗑️</button>
            </>
          )}
          {row.status === "Submitted" && (
            <>
              <button onClick={() => approveMutation.mutate(row.id)} title="Approve" className="p-1 text-green-600 hover:bg-green-50 rounded"><CheckCircle size={14} /></button>
              <button onClick={() => { setRejectId(row.id); setRejectModal(true); }} title="Reject" className="p-1 text-red-600 hover:bg-red-50 rounded"><XCircle size={14} /></button>
            </>
          )}
          {row.status !== "Draft" && <span className="text-xs text-gray-400">—</span>}
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Purchase Requisitions</h2>
          <p className="text-xs text-gray-400">Manage purchase requisitions</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add PR
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
        searchPlaceholder="Search PRs..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No purchase requisitions found"
        toolbarPrefix={
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); dg.setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Status</option>
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Converted">Converted</option>
          </select>
        }
        actions={<ExportMenu baseUrl="purchaserequisitions" filters={{ search: dg.search || undefined, status: filterStatus || undefined }} />}
      />

      <Modal title={editing ? "Edit PR" : "Add Purchase Requisition"} open={modal} onClose={closeModal}>
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">PR Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.prDate} onChange={(e) => setForm((f) => ({ ...f, prDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Required Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.requiredDate} onChange={(e) => setForm((f) => ({ ...f, requiredDate: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Purpose</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.purpose} onChange={(e) => setForm((f) => ({ ...f, purpose: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Notes</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
            </div>
          </div>

          <div className="border-t pt-3 mt-3">
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Items</label>
              <button onClick={addItem} type="button" className="text-xs text-primary-500 hover:text-primary-600">+ Add Item</button>
            </div>
            {form.items.map((item, idx) => (
              <div key={idx} className="grid grid-cols-5 gap-2 mb-2 items-end p-2 bg-gray-50 rounded-lg">
                <input placeholder="Item ID" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.itemVariantId} onChange={(e) => updateItem(idx, "itemVariantId", e.target.value)} />
                <input placeholder="Warehouse ID" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.warehouseId} onChange={(e) => updateItem(idx, "warehouseId", e.target.value)} />
                <input type="number" placeholder="Qty" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.requestedQuantity} onChange={(e) => updateItem(idx, "requestedQuantity", parseFloat(e.target.value))} />
                <input type="number" placeholder="Est. Cost" className="border border-gray-200 rounded px-2 py-1 text-xs" value={item.estimatedUnitCost} onChange={(e) => updateItem(idx, "estimatedUnitCost", parseFloat(e.target.value))} />
                <button onClick={() => removeItem(idx)} className="text-red-500 hover:bg-red-50 p-1 rounded">✕</button>
              </div>
            ))}
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>

      <Modal title="Reject PR" open={rejectModal} onClose={() => setRejectModal(false)}>
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Reason for rejection</label>
            <textarea rows={3} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              value={rejectReason} onChange={(e) => setRejectReason(e.target.value)} placeholder="Enter rejection reason..." />
          </div>
          <div className="flex justify-end gap-2">
            <button onClick={() => setRejectModal(false)} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => rejectMutation.mutate()} disabled={rejectMutation.isPending || !rejectReason}
              className="px-4 py-2 text-sm bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:opacity-50">
              {rejectMutation.isPending ? "Rejecting..." : "Reject"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}