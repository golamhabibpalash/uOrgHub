import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, CheckCircle, XCircle, Trash2, FileText, FilePlus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useDataGrid } from "../../hooks/useDataGrid";
import { useDepartmentLookup, useEmployeeLookup, useWarehouseLookup, useItemVariantLookup } from "../../hooks/useEntityLookup";
import { getPurchaseRequisitions, createPurchaseRequisition, updatePurchaseRequisition, deletePurchaseRequisition, submitPR, approvePR, rejectPR, PurchaseRequisition, PRStatus } from "../../api/procurement";
import DateInput from "../../components/shared/DateInput";

export default function PurchaseRequisitions() {
  const navigate = useNavigate();
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

  const { options: departmentOptions, isLoading: departmentsLoading } = useDepartmentLookup();
  const { options: employeeOptions, isLoading: employeesLoading } = useEmployeeLookup();
  const { options: warehouseOptions, isLoading: warehousesLoading } = useWarehouseLookup();
  const { options: itemVariantOptions, isLoading: itemVariantsLoading } = useItemVariantLookup();

  const estimatedTotal = form.items.reduce((sum, i) => sum + i.requestedQuantity * i.estimatedUnitCost, 0);

  const isFormValid =
    !!form.departmentId &&
    !!form.requestedById &&
    !!form.requiredDate &&
    form.items.length > 0 &&
    form.items.every((i) => i.itemVariantId && i.warehouseId && i.requestedQuantity > 0);

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

  function updateItem<K extends keyof (typeof form.items)[number]>(index: number, field: K, value: (typeof form.items)[number][K]) {
    const newItems = [...form.items];
    newItems[index] = { ...newItems[index], [field]: value };
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
          {row.status === "Approved" && (
            <button onClick={() => navigate(`/procurement/rfqs?fromPR=${row.id}`)} title="Create RFQ" className="p-1 text-purple-600 hover:bg-purple-50 rounded"><FilePlus size={14} /></button>
          )}
          <button onClick={() => navigate(`/procurement/purchase-requisitions/${row.id}/document`)} title="Document" className="p-1 text-gray-600 hover:bg-gray-50 rounded"><FileText size={14} /></button>
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

      <Modal title={editing ? "Edit PR" : "Add Purchase Requisition"} open={modal} onClose={closeModal} size="3xl">
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">PR Date *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.prDate} onChange={(e) => setForm((f) => ({ ...f, prDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Required Date *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={form.requiredDate} onChange={(e) => setForm((f) => ({ ...f, requiredDate: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <SearchableDropdown
                label="Department"
                required
                options={departmentOptions}
                value={form.departmentId || undefined}
                onChange={(v) => setForm((f) => ({ ...f, departmentId: v ?? "" }))}
                loading={departmentsLoading}
                placeholder="Select department..."
                className="w-full"
              />
            </div>
            <div>
              <SearchableDropdown
                label="Requested By"
                required
                options={employeeOptions}
                value={form.requestedById || undefined}
                onChange={(v) => setForm((f) => ({ ...f, requestedById: v ?? "" }))}
                loading={employeesLoading}
                placeholder="Select employee..."
                className="w-full"
              />
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
              {form.items.map((item, idx) => {
                const lineTotal = item.requestedQuantity * item.estimatedUnitCost;
                return (
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
                    <div className="grid grid-cols-12 gap-2 items-end">
                      <div className="col-span-4">
                        <label className="text-[11px] text-gray-400 mb-0.5 block">Warehouse</label>
                        <SearchableDropdown
                          options={warehouseOptions}
                          value={item.warehouseId || undefined}
                          onChange={(v) => updateItem(idx, "warehouseId", v ?? "")}
                          loading={warehousesLoading}
                          placeholder="Select warehouse..."
                        />
                      </div>
                      <div className="col-span-2">
                        <label className="text-[11px] text-gray-400 mb-0.5 block">Qty</label>
                        <input type="number" min={0}
                          className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                          value={item.requestedQuantity || ""}
                          onChange={(e) => updateItem(idx, "requestedQuantity", parseFloat(e.target.value) || 0)} />
                      </div>
                      <div className="col-span-3">
                        <label className="text-[11px] text-gray-400 mb-0.5 block">Est. Unit Cost</label>
                        <input type="number" min={0}
                          className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                          value={item.estimatedUnitCost || ""}
                          onChange={(e) => updateItem(idx, "estimatedUnitCost", parseFloat(e.target.value) || 0)} />
                      </div>
                      <div className="col-span-3">
                        <label className="text-[11px] text-gray-400 mb-0.5 block">Notes</label>
                        <input
                          className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                          value={item.notes}
                          onChange={(e) => updateItem(idx, "notes", e.target.value)}
                          placeholder="Optional"
                        />
                      </div>
                    </div>
                    <div className="mt-2 text-right">
                      <span className="text-xs text-gray-500">Est. line total: </span>
                      <span className="text-sm font-semibold text-gray-900">{lineTotal.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
                    </div>
                  </div>
                );
              })}
            </div>

            {form.items.length > 0 && (
              <div className="flex justify-end items-center gap-2 mt-2 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg">
                <span className="text-sm text-gray-600 font-medium">Estimated Total</span>
                <span className="text-base font-bold text-gray-900">{estimatedTotal.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
              </div>
            )}
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending || !isFormValid}
              title={isFormValid ? undefined : "Department, Requested By, Required Date and at least one complete item line are required"}
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