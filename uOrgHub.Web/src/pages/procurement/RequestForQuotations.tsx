import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, FileText } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useDataGrid } from "../../hooks/useDataGrid";
import { useApprovedPRLookup, useItemVariantLookup, withCurrentOption } from "../../hooks/useEntityLookup";
import { getRFQs, createRFQ, updateRFQ, deleteRFQ, getPurchaseRequisitionById, RequestForQuotation, RFQStatus } from "../../api/procurement";
import DateInput from "../../components/shared/DateInput";

export default function RequestForQuotations() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
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

  const { options: prOptionsRaw, isLoading: prsLoading } = useApprovedPRLookup();
  const prOptions = withCurrentOption(prOptionsRaw, editing?.prId, editing?.prNumber);
  const { options: itemVariantOptions, isLoading: itemVariantsLoading } = useItemVariantLookup();

  const isFormValid =
    !!form.closingDate &&
    !!form.title &&
    form.items.length > 0 &&
    form.items.every((i) => i.itemVariantId && i.requestedQuantity > 0);

  const { data, isLoading } = useQuery({
    queryKey: ["rfqs", ...dg.queryKey, filterStatus],
    queryFn: () => getRFQs(dg.queryParams,
      filterStatus ? filterStatus as RFQStatus : undefined),
  });

  const items = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => {
      // prId is an optional reference — send undefined (omitted), never an empty string, since
      // the backend's nullable Guid can't parse "".
      const payload = { ...form, prId: form.prId || undefined, items: form.items.map(i => ({ ...i })) };
      return editing ? updateRFQ(editing.id, payload) : createRFQ(payload);
    },
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

  // "Create RFQ" from a Purchase Requisition row (?fromPR=<id>) — pre-fills the Add form with
  // that PR's reference and its own items instead of opening a blank one. The param is stripped
  // once consumed so a refresh/back-navigation doesn't re-trigger the prefill.
  useEffect(() => {
    const prId = searchParams.get("fromPR");
    if (!prId) return;
    setSearchParams((prev) => { const next = new URLSearchParams(prev); next.delete("fromPR"); return next; }, { replace: true });

    getPurchaseRequisitionById(prId).then((res) => {
      const pr = res.data.data;
      if (!pr) return;
      setEditing(null);
      setForm({
        rfqDate: new Date().toISOString().split("T")[0],
        closingDate: "",
        prId: pr.id,
        title: `RFQ for ${pr.prNumber}`,
        description: pr.purpose ?? "",
        notes: "",
        status: "Draft",
        items: pr.items.map((i) => ({ itemVariantId: i.itemVariantId, requestedQuantity: i.requestedQuantity, notes: i.notes ?? "" })),
      });
      setModal(true);
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  function closeModal() { setModal(false); setEditing(null); }

  function addItem() {
    setForm(f => ({ ...f, items: [...f.items, { itemVariantId: "", requestedQuantity: 0, notes: "" }] }));
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
          <button onClick={() => navigate(`/procurement/rfqs/${row.id}/document`)} title="Document" className="p-1 text-gray-600 hover:bg-gray-50 rounded"><FileText size={14} /></button>
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

      <Modal title={editing ? "Edit RFQ" : "Create RFQ"} open={modal} onClose={closeModal} size="3xl">
        <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">RFQ Date *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
                value={form.rfqDate} onChange={(e) => setForm((f) => ({ ...f, rfqDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Closing Date *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm"
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
                  <div className="grid grid-cols-12 gap-2 items-end">
                    <div className="col-span-3">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Qty</label>
                      <input type="number" min={0}
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm text-right focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={item.requestedQuantity || ""}
                        onChange={(e) => updateItem(idx, "requestedQuantity", parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="col-span-9">
                      <label className="text-[11px] text-gray-400 mb-0.5 block">Notes</label>
                      <input
                        className="w-full border border-gray-200 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                        value={item.notes}
                        onChange={(e) => updateItem(idx, "notes", e.target.value)}
                        placeholder="Optional"
                      />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending || !isFormValid}
              title={isFormValid ? undefined : "Closing Date, Title and at least one complete item line are required"}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}