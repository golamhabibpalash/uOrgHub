import { useState, useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, CheckCircle, XCircle } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useDataGrid } from "../../hooks/useDataGrid";
import { useWarehouseLookup } from "../../hooks/useEntityLookup";
import {
  getStockTransactions, createStockTransaction, updateStockTransaction,
  deleteStockTransaction, confirmStockTransaction, cancelStockTransaction,
  getItemVariants,
  StockTransaction, StockTransactionType, StockTransactionStatus,
} from "../../api/inventory";

const TXN_TYPES: StockTransactionType[] = ["GoodsReceived", "GoodsIssued", "Transfer", "Adjustment", "Return"];

const txnTypeColors: Record<StockTransactionType, string> = {
  GoodsReceived: "bg-green-50 text-green-700",
  GoodsIssued: "bg-red-50 text-red-700",
  Transfer: "bg-blue-50 text-blue-700",
  Adjustment: "bg-orange-50 text-orange-700",
  Return: "bg-purple-50 text-purple-700",
};

const statusColors: Record<StockTransactionStatus, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Confirmed: "bg-green-50 text-green-700",
  Cancelled: "bg-red-50 text-red-600",
};

export default function StockTransactions() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "transactionDate" });
  const [filterStatus, setFilterStatus] = useState<StockTransactionStatus | "">("");
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<StockTransaction | null>(null);
  const [form, setForm] = useState({
    transactionDate: new Date().toISOString().split("T")[0],
    transactionType: "GoodsReceived" as StockTransactionType,
    itemVariantId: "", warehouseId: "", fromWarehouseId: "",
    quantity: 1, unitCost: 0, referenceNumber: "", notes: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["stock-transactions", dg.page, dg.search, dg.sortBy, dg.sortDescending, filterStatus],
    queryFn: () => getStockTransactions(dg.queryParams, undefined, undefined, filterStatus || undefined),
  });

  const { options: warehouseOptions, isLoading: warehousesLoading } = useWarehouseLookup();

  const { data: variantsData } = useQuery({
    queryKey: ["item-variants-all"],
    queryFn: () => getItemVariants({ page: 1, pageSize: 200 }),
  });

  const txns = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const allVariants = variantsData?.data?.data?.items ?? [];
  const variantOptions = useMemo(() => allVariants.map((v) => ({ value: v.id, label: `${v.sku} — ${v.variantName}` })), [allVariants]);

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload = {
        ...form,
        fromWarehouseId: form.fromWarehouseId || undefined,
        transactionDate: new Date(form.transactionDate).toISOString(),
      };
      return editing ? updateStockTransaction(editing.id, payload) : createStockTransaction(payload);
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["stock-transactions"] }); closeModal(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteStockTransaction(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["stock-transactions"] }),
  });

  const confirmMutation = useMutation({
    mutationFn: (id: string) => confirmStockTransaction(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["stock-transactions"] });
      qc.invalidateQueries({ queryKey: ["stock-balances"] });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => cancelStockTransaction(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["stock-transactions"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ transactionDate: new Date().toISOString().split("T")[0], transactionType: "GoodsReceived", itemVariantId: allVariants[0]?.id ?? "", warehouseId: "", fromWarehouseId: "", quantity: 1, unitCost: 0, referenceNumber: "", notes: "" });
    setModal(true);
  }

  function openEdit(t: StockTransaction) {
    if (t.status !== "Draft") return;
    setEditing(t);
    setForm({
      transactionDate: t.transactionDate.split("T")[0],
      transactionType: t.transactionType,
      itemVariantId: t.itemVariantId, warehouseId: t.warehouseId, fromWarehouseId: t.fromWarehouseId ?? "",
      quantity: t.quantity, unitCost: t.unitCost, referenceNumber: t.referenceNumber ?? "", notes: t.notes ?? "",
    });
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); }

  const columns = [
    { key: "transactionNumber", label: "TXN#", render: (row: StockTransaction) => <span className="font-mono text-xs bg-gray-100 px-2 py-0.5 rounded">{row.transactionNumber}</span> },
    {
      key: "transactionType", label: "Type", sortable: false,
      render: (row: StockTransaction) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${txnTypeColors[row.transactionType]}`}>
          {row.transactionTypeName}
        </span>
      ),
    },
    {
      key: "status", label: "Status", sortable: false,
      render: (row: StockTransaction) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.statusName}</span>
      ),
    },
    { key: "variantSKU", label: "SKU", render: (row: StockTransaction) => <span className="font-mono text-xs">{row.variantSKU}</span> },
    { key: "warehouseName", label: "Warehouse" },
    { key: "quantity", label: "Qty", render: (row: StockTransaction) => <span className="font-medium">{row.quantity}</span> },
    { key: "unitCost", label: "Unit Cost", render: (row: StockTransaction) => <span>${row.unitCost.toFixed(2)}</span> },
    { key: "totalCost", label: "Total", sortable: false, render: (row: StockTransaction) => <span className="font-medium">${(row.quantity * row.unitCost).toFixed(2)}</span> },
    {
      key: "transactionDate", label: "Date",
      render: (row: StockTransaction) => <span className="text-gray-500 text-xs">{new Date(row.transactionDate).toLocaleDateString()}</span>,
    },
    {
      key: "actions", label: "Actions", sortable: false,
      render: (row: StockTransaction) => (
        <div className="flex gap-2">
          {row.status === "Draft" && (
            <>
              <button
                onClick={() => openEdit(row)}
                className="text-gray-400 hover:text-primary-600"
                title="Edit"
              >
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/></svg>
              </button>
              <button
                onClick={() => deleteMutation.mutate(row.id)}
                className="text-gray-400 hover:text-red-500"
                title="Delete"
              >
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
              </button>
              <button
                onClick={() => confirmMutation.mutate(row.id)}
                disabled={confirmMutation.isPending}
                className="flex items-center gap-1 text-xs bg-green-50 text-green-700 px-2 py-1 rounded hover:bg-green-100"
              >
                <CheckCircle size={12} /> Confirm
              </button>
              <button
                onClick={() => cancelMutation.mutate(row.id)}
                disabled={cancelMutation.isPending}
                className="flex items-center gap-1 text-xs bg-red-50 text-red-600 px-2 py-1 rounded hover:bg-red-100"
              >
                <XCircle size={12} /> Cancel
              </button>
            </>
          )}
        </div>
      ),
    },
  ];

  const isTransfer = form.transactionType === "Transfer";

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Stock Transactions</h2>
          <p className="text-xs text-gray-400">Goods received, issued, transfers and adjustments</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Transaction
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={txns}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search TXN#, reference..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No transactions found"
        toolbarPrefix={
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value as StockTransactionStatus | ""); dg.setPage(1); }} className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500">
            <option value="">All Statuses</option>
            <option value="Draft">Draft</option>
            <option value="Confirmed">Confirmed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        }
        actions={<ExportMenu baseUrl="/stocktransactions" filters={{ search: dg.search || undefined, status: filterStatus || undefined }} />}
      />

      <Modal title={editing ? "Edit Transaction" : "New Stock Transaction"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Transaction Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.transactionType} onChange={(e) => setForm((f) => ({ ...f, transactionType: e.target.value as StockTransactionType }))} disabled={!!editing}>
                {TXN_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Transaction Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.transactionDate} onChange={(e) => setForm((f) => ({ ...f, transactionDate: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Item Variant *</label>
            <SearchableDropdown
              label="Item Variant"
              required
              options={variantOptions}
              value={form.itemVariantId || undefined}
              onChange={(v) => setForm((f) => ({ ...f, itemVariantId: v ?? "" }))}
              placeholder="Select variant..."
              className="w-full"
            />
          </div>
          <div className={isTransfer ? "grid grid-cols-2 gap-3" : ""}>
            {isTransfer && (
              <div>
                <label className="text-xs text-gray-500 mb-1 block">From Warehouse *</label>
                <SearchableDropdown
                  label="From Warehouse"
                  required
                  options={warehouseOptions}
                  value={form.fromWarehouseId || undefined}
                  onChange={(v) => setForm((f) => ({ ...f, fromWarehouseId: v ?? "" }))}
                  loading={warehousesLoading}
                  placeholder="Select source..."
                  className="w-full"
                />
              </div>
            )}
            <div className={!isTransfer ? "" : ""}>
              <label className="text-xs text-gray-500 mb-1 block">{isTransfer ? "To Warehouse *" : "Warehouse *"}</label>
              <SearchableDropdown
                label={isTransfer ? "To Warehouse" : "Warehouse"}
                required
                options={warehouseOptions}
                value={form.warehouseId || undefined}
                onChange={(v) => setForm((f) => ({ ...f, warehouseId: v ?? "" }))}
                loading={warehousesLoading}
                placeholder="Select warehouse..."
                className="w-full"
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Quantity *</label>
              <input type="number" min="0.001" step="0.001" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.quantity} onChange={(e) => setForm((f) => ({ ...f, quantity: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Unit Cost</label>
              <input type="number" min="0" step="0.01" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.unitCost} onChange={(e) => setForm((f) => ({ ...f, unitCost: parseFloat(e.target.value) || 0 }))} />
            </div>
          </div>
          {form.quantity > 0 && form.unitCost > 0 && (
            <p className="text-xs text-gray-500 text-right">Total: <span className="font-medium text-gray-800">${(form.quantity * form.unitCost).toFixed(2)}</span></p>
          )}
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Reference Number</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" placeholder="PO#, SO#, etc." value={form.referenceNumber} onChange={(e) => setForm((f) => ({ ...f, referenceNumber: e.target.value }))} />
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Notes</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save Draft"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
