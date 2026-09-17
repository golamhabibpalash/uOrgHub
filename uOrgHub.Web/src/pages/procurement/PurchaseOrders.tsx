import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, CheckCircle, XCircle, Trash2, Eye } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useDataGrid } from "../../hooks/useDataGrid";
import { useProcurementVendorLookup, useApprovedPRLookup, useAcceptedQuotationLookup, useItemVariantLookup, withCurrentOption } from "../../hooks/useEntityLookup";
import { getPurchaseOrders, createPurchaseOrder, updatePurchaseOrder, deletePurchaseOrder, sendPO, confirmPO, cancelPO, PurchaseOrder, POStatus } from "../../api/procurement";
import DateInput from "../../components/shared/DateInput";

export default function PurchaseOrders() {
  const navigate = useNavigate();
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "poDate" });
  const [filterStatus, setFilterStatus] = useState("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<PurchaseOrder | null>(null);
  const [form, setForm] = useState({
    poDate: new Date().toISOString().split("T")[0], expectedDeliveryDate: "",
    vendorId: "", quotationId: "", prId: "", paymentTerms: "", deliveryAddress: "", notes: "",
    items: [] as { itemVariantId: string; orderedQuantity: number; unitPrice: number; taxPercent: number; discountPercent: number; notes: string }[],
  });

  const { options: vendorOptionsRaw, isLoading: vendorsLoading } = useProcurementVendorLookup();
  const { options: prOptionsRaw, isLoading: prsLoading } = useApprovedPRLookup();
  const { options: quotationOptionsRaw, isLoading: quotationsLoading } = useAcceptedQuotationLookup();
  const { options: itemVariantOptions, isLoading: itemVariantsLoading } = useItemVariantLookup();
  const vendorOptions = withCurrentOption(vendorOptionsRaw, editing?.vendorId, editing?.vendorName);
  const prOptions = withCurrentOption(prOptionsRaw, editing?.prId, editing?.prNumber);
  const quotationOptions = withCurrentOption(quotationOptionsRaw, editing?.quotationId, editing?.quotationNumber);

  const lineTotal = (i: (typeof form.items)[number]) =>
    i.orderedQuantity * i.unitPrice * (1 + i.taxPercent / 100 - i.discountPercent / 100);
  const poTotal = form.items.reduce((sum, i) => sum + lineTotal(i), 0);

  const isFormValid =
    !!form.vendorId &&
    !!form.expectedDeliveryDate &&
    form.items.length > 0 &&
    form.items.every((i) => i.itemVariantId && i.orderedQuantity > 0);

  const { data, isLoading } = useQuery({
    queryKey: ["purchase-orders", ...dg.queryKey, filterStatus],
    queryFn: () => getPurchaseOrders(dg.queryParams,
      filterStatus ? filterStatus as POStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => {
      // quotationId/prId are optional references — send undefined (omitted), never an empty
      // string, since the backend's nullable Guid can't parse "" (this was the PR page's bug).
      const payload = { ...form, quotationId: form.quotationId || undefined, prId: form.prId || undefined, items: form.items.map(i => ({ ...i })) };
      return editing ? updatePurchaseOrder(editing.id, payload) : createPurchaseOrder(payload);
    },
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

  function updateItem<K extends keyof (typeof form.items)[number]>(idx: number, field: K, value: (typeof form.items)[number][K]) {
    const newItems = [...form.items];
    newItems[idx] = { ...newItems[idx], [field]: value };
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
    { key: "status", label: "Status", sortable: false, render: (row: PurchaseOrder) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span> },
    { key: "actions", label: "Actions", sortable: false, render: (row: PurchaseOrder) => (
      <div className="flex items-center gap-1">
        <button onClick={() => navigate(`/procurement/purchase-orders/${row.id}`)} title="View details" className="p-1 text-gray-400 hover:text-primary-600 hover:bg-gray-50 rounded"><Eye size={14} /></button>
        {row.status === "Draft" && (
          <>
            <button onClick={() => sendMutation.mutate(row.id)} title="Send" className="p-1 text-blue-600 hover:bg-blue-50 rounded"><Send size={14} /></button>
            <button onClick={() => openEdit(row)} className="p-1 text-gray-600 hover:bg-gray-50 rounded">✏️</button>
            <button onClick={() => deleteMutation.mutate(row.id)} className="p-1 text-red-600 hover:bg-red-50 rounded">🗑️</button>
          </>
        )}
        {row.status === "Sent" && (
          <>
            <button onClick={() => confirmMutation.mutate(row.id)} title="Confirm" className="p-1 text-green-600 hover:bg-green-50 rounded"><CheckCircle size={14} /></button>
            <button onClick={() => cancelMutation.mutate(row.id)} title="Cancel" className="p-1 text-red-600 hover:bg-red-50 rounded"><XCircle size={14} /></button>
          </>
        )}
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

      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search POs..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No purchase orders found"
        toolbarPrefix={
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); dg.setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5">
            <option value="">All Status</option>
            <option value="Draft">Draft</option>
            <option value="Sent">Sent</option>
            <option value="Confirmed">Confirmed</option>
            <option value="PartiallyReceived">Partially Received</option>
            <option value="FullyReceived">Fully Received</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        }
        actions={<ExportMenu baseUrl="purchaseorders" filters={{ search: dg.search || undefined, status: filterStatus || undefined }} />}
      />

      <Modal title={editing ? "Edit PO" : "Create Purchase Order"} open={modal} onClose={closeModal} size="3xl">
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div><label className="text-xs text-gray-500 mb-1 block">PO Date *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.poDate} onChange={(e) => setForm((f) => ({ ...f, poDate: e.target.value }))} /></div>
            <div><label className="text-xs text-gray-500 mb-1 block">Expected Delivery *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.expectedDeliveryDate} onChange={(e) => setForm((f) => ({ ...f, expectedDeliveryDate: e.target.value }))} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <SearchableDropdown
                label="Vendor" required
                options={vendorOptions}
                value={form.vendorId || undefined}
                onChange={(v) => setForm((f) => ({ ...f, vendorId: v ?? "" }))}
                loading={vendorsLoading}
                placeholder="Select vendor..."
                className="w-full"
              />
            </div>
            <div>
              <SearchableDropdown
                label="Quotation Ref (optional)"
                options={quotationOptions}
                value={form.quotationId || undefined}
                onChange={(v) => setForm((f) => ({ ...f, quotationId: v ?? "" }))}
                loading={quotationsLoading}
                placeholder="Select accepted quotation..."
                clearable
                className="w-full"
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <SearchableDropdown
                label="PR Reference (optional)"
                options={prOptions}
                value={form.prId || undefined}
                onChange={(v) => setForm((f) => ({ ...f, prId: v ?? "" }))}
                loading={prsLoading}
                placeholder="Select approved PR..."
                clearable
                className="w-full"
              />
            </div>
            <div><label className="text-xs text-gray-500 mb-1 block">Payment Terms</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.paymentTerms} onChange={(e) => setForm((f) => ({ ...f, paymentTerms: e.target.value }))} /></div>
          </div>
          <div><label className="text-xs text-gray-500 mb-1 block">Delivery Address</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.deliveryAddress} onChange={(e) => setForm((f) => ({ ...f, deliveryAddress: e.target.value }))} /></div>
          <div><label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} /></div>
          <div className="border-t pt-3 mt-3">
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs font-medium text-gray-600">Items</label>
              <button onClick={addItem} type="button" className="text-xs text-primary-600 hover:underline flex items-center gap-1">
                <Plus size={12} /> Add Item
              </button>
            </div>

            {form.items.length === 0 && (
              <div className="text-center py-6 border border-dashed border-gray-200 rounded-lg text-xs text-gray-400">
                No items added yet — click "+ Add Item" to add the first line.
              </div>
            )}

            <div className="space-y-2">
              {form.items.map((item, idx) => (
                <div key={idx} className="border border-gray-200 rounded-lg p-3 bg-white">
                  <div className="flex items-start gap-2 mb-2">
                    <div className="flex-1">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Item</label>
                      <SearchableDropdown
                        options={itemVariantOptions}
                        value={item.itemVariantId || undefined}
                        onChange={(v) => updateItem(idx, "itemVariantId", v ?? "")}
                        loading={itemVariantsLoading}
                        placeholder="Select item..."
                      />
                    </div>
                    <button onClick={() => removeItem(idx)} className="mt-5 text-red-400 hover:text-red-600 p-1 rounded hover:bg-red-50 shrink-0" title="Remove line">
                      <Trash2 size={14} />
                    </button>
                  </div>
                  <div className="grid grid-cols-12 gap-2 items-end mb-2">
                    <div className="col-span-3">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Qty</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={item.orderedQuantity || ""}
                        onChange={(e) => updateItem(idx, "orderedQuantity", parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="col-span-3">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Unit Price</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={item.unitPrice || ""}
                        onChange={(e) => updateItem(idx, "unitPrice", parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="col-span-3">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Tax %</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={item.taxPercent || ""}
                        onChange={(e) => updateItem(idx, "taxPercent", parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="col-span-3">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Discount %</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={item.discountPercent || ""}
                        onChange={(e) => updateItem(idx, "discountPercent", parseFloat(e.target.value) || 0)} />
                    </div>
                  </div>
                  <div>
                    <label className="text-[11px] text-gray-400 mb-0.5 block">Notes</label>
                    <input
                      className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                      value={item.notes}
                      onChange={(e) => updateItem(idx, "notes", e.target.value)}
                      placeholder="Optional"
                    />
                  </div>
                  <div className="mt-2 text-right">
                    <span className="text-xs text-gray-500">Line total: </span>
                    <span className="text-sm font-semibold text-gray-900">{lineTotal(item).toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
                  </div>
                </div>
              ))}
            </div>

            {form.items.length > 0 && (
              <div className="flex justify-end items-center gap-2 mt-2 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg">
                <span className="text-sm text-gray-600 font-medium">Total</span>
                <span className="text-base font-bold text-gray-900">{poTotal.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
              </div>
            )}
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending || !isFormValid}
              title={isFormValid ? undefined : "Vendor, Expected Delivery and at least one complete item line are required"}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}