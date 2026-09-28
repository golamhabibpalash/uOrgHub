import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ChevronDown, ChevronUp, Lock } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import DateInput from "../../components/shared/DateInput";
import {
  useAssetCategoryLookup,
  useVendorLookup,
  useApprovedBillLookup,
  useCostCenterLookup,
} from "../../hooks/useEntityLookup";
import {
  getFixedAssets,
  getFixedAssetSummary,
  getFixedAssetDepreciationHistory,
  createFixedAsset,
  updateFixedAsset,
  deleteFixedAsset,
  FixedAsset,
  FixedAssetPayload,
  FixedAssetStatus,
  DepreciationMethod,
  depreciationMethodLabels,
  fixedAssetStatusLabels,
} from "../../api/accounts";

const inputClass = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500 disabled:bg-gray-50 disabled:text-gray-500";
const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const today = () => new Date().toISOString().split("T")[0];

const statusColors: Record<FixedAssetStatus, string> = {
  Active: "bg-green-50 text-green-700",
  Idle: "bg-gray-100 text-gray-600",
  UnderMaintenance: "bg-yellow-50 text-yellow-700",
};

const emptyForm = {
  name: "",
  description: "",
  categoryId: "",
  manufacturer: "",
  model: "",
  serialNumber: "",
  chassisNumber: "",
  engineNumber: "",
  registrationNumber: "",
  purchaseDate: today(),
  depreciationStartDate: "",
  purchaseCost: 0,
  salvageValue: "",
  usefulLifeYears: "",
  depreciationMethod: "" as DepreciationMethod | "",
  openingAccumulatedDepreciation: 0,
  openingDepreciatedUpTo: "",
  vendorId: "",
  billId: "",
  costCenterId: "",
  location: "",
  status: "Active" as FixedAssetStatus,
  notes: "",
};

function DepreciationHistory({ asset }: { asset: FixedAsset }) {
  const { data, isLoading } = useQuery({
    queryKey: ["fixed-asset-history", asset.id],
    queryFn: () => getFixedAssetDepreciationHistory(asset.id),
  });
  const rows = data?.data?.data ?? [];

  return (
    <div className="grid grid-cols-3 gap-6 text-xs">
      <div className="space-y-1">
        <p className="font-medium text-gray-700 mb-1">Identification</p>
        <p><span className="text-gray-400">Make / Model:</span> {[asset.manufacturer, asset.model].filter(Boolean).join(" ") || "—"}</p>
        <p><span className="text-gray-400">Serial:</span> {asset.serialNumber || "—"}</p>
        <p><span className="text-gray-400">Chassis:</span> {asset.chassisNumber || "—"}</p>
        <p><span className="text-gray-400">Engine:</span> {asset.engineNumber || "—"}</p>
        <p><span className="text-gray-400">Registration:</span> {asset.registrationNumber || "—"}</p>
      </div>
      <div className="space-y-1">
        <p className="font-medium text-gray-700 mb-1">Depreciation</p>
        <p><span className="text-gray-400">Method:</span> {depreciationMethodLabels[asset.depreciationMethod]}, {(asset.usefulLifeMonths / 12).toLocaleString("en-BD", { maximumFractionDigits: 1 })} yrs</p>
        <p><span className="text-gray-400">Salvage:</span> {fmt(asset.salvageValue)}</p>
        <p><span className="text-gray-400">Starts:</span> {asset.depreciationStartDate.split("T")[0]}</p>
        <p><span className="text-gray-400">Depreciated up to:</span> {asset.lastDepreciationDate?.split("T")[0] ?? "not yet"}</p>
        {asset.openingAccumulatedDepreciation > 0 && (
          <p><span className="text-gray-400">Opening (pre go-live):</span> {fmt(asset.openingAccumulatedDepreciation)}</p>
        )}
        <p><span className="text-gray-400">Bill:</span> {asset.billBillNumber ?? "—"} · <span className="text-gray-400">Vendor:</span> {asset.vendorName ?? "—"}</p>
      </div>
      <div>
        <p className="font-medium text-gray-700 mb-1">Posted runs</p>
        {isLoading ? (
          <p className="text-gray-400">Loading...</p>
        ) : rows.length === 0 ? (
          <p className="text-gray-400">No depreciation posted yet.</p>
        ) : (
          <table className="w-full">
            <tbody>
              {rows.map((r) => (
                <tr key={r.depreciationRunId} className={r.status === "Reversed" ? "text-gray-300 line-through" : ""}>
                  <td className="py-0.5 font-mono">{r.runNumber}</td>
                  <td className="py-0.5">{r.periodEndDate.split("T")[0]}</td>
                  <td className="py-0.5 text-right tabular-nums">{fmt(r.amount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default function FixedAssets() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "assetCode", searchDebounceMs: 300 });
  const [categoryFilter, setCategoryFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<FixedAsset | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [saveError, setSaveError] = useState("");

  const { options: categoryOptions, categories } = useAssetCategoryLookup();
  const { options: vendorOptions } = useVendorLookup();
  const { options: billOptions } = useApprovedBillLookup();
  const { options: costCenterOptions } = useCostCenterLookup();

  const { data, isLoading } = useQuery({
    queryKey: ["fixed-assets", ...dg.queryKey, categoryFilter, statusFilter],
    queryFn: () => getFixedAssets(dg.queryParams, categoryFilter || undefined, statusFilter || undefined),
    placeholderData: (prev) => prev,
  });
  const { data: summaryData } = useQuery({ queryKey: ["fixed-assets-summary"], queryFn: getFixedAssetSummary });

  const assets = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const summary = summaryData?.data?.data;

  const locked = editing?.hasPostedDepreciation ?? false;
  const selectedCategory = categories.find((c) => c.id === form.categoryId);

  function invalidate() {
    qc.invalidateQueries({ queryKey: ["fixed-assets"] });
    qc.invalidateQueries({ queryKey: ["fixed-assets-summary"] });
  }

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload: FixedAssetPayload = {
        name: form.name,
        description: form.description || undefined,
        categoryId: form.categoryId,
        manufacturer: form.manufacturer || undefined,
        model: form.model || undefined,
        serialNumber: form.serialNumber || undefined,
        chassisNumber: form.chassisNumber || undefined,
        engineNumber: form.engineNumber || undefined,
        registrationNumber: form.registrationNumber || undefined,
        purchaseDate: form.purchaseDate,
        depreciationStartDate: form.depreciationStartDate || undefined,
        purchaseCost: form.purchaseCost,
        // Left blank on create, these fall back to the category defaults server-side.
        salvageValue: form.salvageValue === "" ? undefined : Number(form.salvageValue),
        usefulLifeMonths: form.usefulLifeYears === "" ? undefined : Math.round(Number(form.usefulLifeYears) * 12),
        depreciationMethod: form.depreciationMethod || undefined,
        vendorId: form.vendorId || undefined,
        billId: form.billId || undefined,
        costCenterId: form.costCenterId || undefined,
        location: form.location || undefined,
        status: form.status,
        notes: form.notes || undefined,
      };
      if (editing) return updateFixedAsset(editing.id, payload);
      return createFixedAsset({
        ...payload,
        openingAccumulatedDepreciation: form.openingAccumulatedDepreciation || 0,
        openingDepreciatedUpTo: form.openingDepreciatedUpTo ? `${form.openingDepreciatedUpTo}-01` : undefined,
      });
    },
    onSuccess: () => { invalidate(); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      setSaveError(axiosErr?.response?.data?.message ?? axiosErr?.response?.data?.errors?.[0] ?? "Failed to save fixed asset.");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteFixedAsset(id),
    onSuccess: invalidate,
  });

  function openAdd() {
    setEditing(null);
    setForm({ ...emptyForm, purchaseDate: today() });
    setSaveError("");
    setModal(true);
  }

  function openEdit(a: FixedAsset) {
    setEditing(a);
    setForm({
      name: a.name,
      description: a.description ?? "",
      categoryId: a.categoryId,
      manufacturer: a.manufacturer ?? "",
      model: a.model ?? "",
      serialNumber: a.serialNumber ?? "",
      chassisNumber: a.chassisNumber ?? "",
      engineNumber: a.engineNumber ?? "",
      registrationNumber: a.registrationNumber ?? "",
      purchaseDate: a.purchaseDate.split("T")[0],
      depreciationStartDate: a.depreciationStartDate.split("T")[0],
      purchaseCost: a.purchaseCost,
      salvageValue: String(a.salvageValue),
      usefulLifeYears: String(a.usefulLifeMonths / 12),
      depreciationMethod: a.depreciationMethod,
      openingAccumulatedDepreciation: a.openingAccumulatedDepreciation,
      openingDepreciatedUpTo: "",
      vendorId: a.vendorId ?? "",
      billId: a.billId ?? "",
      costCenterId: a.costCenterId ?? "",
      location: a.location ?? "",
      status: a.status,
      notes: a.notes ?? "",
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const set = <K extends keyof typeof form>(key: K, value: (typeof form)[K]) => setForm((f) => ({ ...f, [key]: value }));

  const columns: DataGridColumn<FixedAsset>[] = [
    { key: "assetCode", label: "Code", className: "font-mono text-xs text-gray-500" },
    {
      key: "name",
      label: "Asset",
      render: (row) => (
        <div>
          <div className="text-gray-900">{row.name}</div>
          <div className="text-xs text-gray-400">{[row.manufacturer, row.model, row.registrationNumber ?? row.serialNumber].filter(Boolean).join(" · ")}</div>
        </div>
      ),
    },
    { key: "categoryName", label: "Category" },
    { key: "purchaseDate", label: "Purchased", render: (row) => row.purchaseDate.split("T")[0] },
    { key: "purchaseCost", label: "Cost", headerClassName: "text-right", className: "text-right tabular-nums", render: (row) => fmt(row.purchaseCost) },
    {
      key: "accumulatedDepreciation",
      label: "Acc. Depreciation",
      sortable: false,
      headerClassName: "text-right",
      className: "text-right tabular-nums text-gray-500",
      render: (row) => fmt(row.accumulatedDepreciation),
    },
    { key: "bookValue", label: "Book Value", headerClassName: "text-right", className: "text-right tabular-nums font-medium", render: (row) => fmt(row.bookValue) },
    { key: "location", label: "Location", sortable: false },
    {
      key: "status",
      label: "Status",
      render: (row) => <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{fixedAssetStatusLabels[row.status]}</span>,
    },
    {
      key: "details",
      label: "",
      sortable: false,
      render: (row) => (
        <button onClick={() => setExpandedId(expandedId === row.id ? null : row.id)} className="text-gray-400 hover:text-primary-600" title="Details & depreciation history">
          {expandedId === row.id ? <ChevronUp size={15} /> : <ChevronDown size={15} />}
        </button>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Fixed Assets</h2>
          <p className="text-xs text-gray-400">Register of machinery, vehicles and equipment — tracked individually and depreciated monthly</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Register Asset
        </button>
      </div>

      {summary && (
        <div className="grid grid-cols-4 gap-3 mb-4">
          {[
            { label: "Assets", value: summary.assetCount.toLocaleString("en-BD") },
            { label: "Total Cost", value: fmt(summary.totalCost) },
            { label: "Accumulated Depreciation", value: fmt(summary.totalAccumulatedDepreciation) },
            { label: "Net Book Value", value: fmt(summary.totalBookValue) },
          ].map((s) => (
            <div key={s.label} className="bg-white border border-gray-200 rounded-xl px-4 py-3">
              <p className="text-xs text-gray-400">{s.label}</p>
              <p className="text-lg font-medium text-gray-900 tabular-nums">{s.value}</p>
            </div>
          ))}
        </div>
      )}

      <DataGrid
        columns={columns}
        data={assets}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search code, name, serial, chassis, registration..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No fixed assets registered"
        renderExpandedRow={(row) => <DepreciationHistory asset={row} />}
        expandedRowId={expandedId}
        filterBar={
          <div className="flex items-center gap-2">
            <SearchableDropdown
              options={categoryOptions}
              value={categoryFilter}
              onChange={(v) => { setCategoryFilter(v ?? ""); dg.resetPage(); }}
              placeholder="All categories"
              searchPlaceholder="Search categories..."
              className="w-56"
            />
            <select
              value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); dg.resetPage(); }}
              className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              <option value="">All statuses</option>
              {(Object.keys(fixedAssetStatusLabels) as FixedAssetStatus[]).map((s) => (
                <option key={s} value={s}>{fixedAssetStatusLabels[s]}</option>
              ))}
            </select>
          </div>
        }
        actions={<ExportMenu baseUrl="/accounts/fixed-assets" />}
      />

      <Modal title={editing ? `Edit ${editing.assetCode}` : "Register Fixed Asset"} open={modal} onClose={closeModal} size="4xl">
        <div className="space-y-4">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{saveError}</div>
          )}

          <section className="space-y-3">
            <div className="grid grid-cols-3 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Asset Code</label>
                <div className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-500">
                  {editing ? editing.assetCode : "Auto-generated on save"}
                </div>
              </div>
              <div className="col-span-2">
                <label className="text-xs text-gray-500 mb-1 block">Name *</label>
                <input className={inputClass} placeholder="e.g. Excavator CAT 320D" value={form.name} onChange={(e) => set("name", e.target.value)} />
              </div>
            </div>
            <div className="grid grid-cols-3 gap-3">
              <SearchableDropdown
                label="Category *"
                options={categoryOptions}
                value={form.categoryId}
                onChange={(v) => set("categoryId", v ?? "")}
                placeholder="Select category"
                searchPlaceholder="Search categories..."
                disabled={locked}
                required
              />
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Status</label>
                <select className={inputClass} value={form.status} onChange={(e) => set("status", e.target.value as FixedAssetStatus)}>
                  {(Object.keys(fixedAssetStatusLabels) as FixedAssetStatus[]).map((s) => (
                    <option key={s} value={s}>{fixedAssetStatusLabels[s]}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Location</label>
                <input className={inputClass} placeholder="e.g. CFEL Warehouse 1" value={form.location} onChange={(e) => set("location", e.target.value)} />
              </div>
            </div>
          </section>

          <section className="border-t border-gray-100 pt-3">
            <p className="text-xs font-medium text-gray-600 mb-2">Identification</p>
            <div className="grid grid-cols-3 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Manufacturer</label>
                <input className={inputClass} value={form.manufacturer} onChange={(e) => set("manufacturer", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Model</label>
                <input className={inputClass} value={form.model} onChange={(e) => set("model", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Serial No.</label>
                <input className={inputClass} value={form.serialNumber} onChange={(e) => set("serialNumber", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Chassis No.</label>
                <input className={inputClass} value={form.chassisNumber} onChange={(e) => set("chassisNumber", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Engine No.</label>
                <input className={inputClass} value={form.engineNumber} onChange={(e) => set("engineNumber", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Registration No.</label>
                <input className={inputClass} value={form.registrationNumber} onChange={(e) => set("registrationNumber", e.target.value)} />
              </div>
            </div>
          </section>

          <section className="border-t border-gray-100 pt-3">
            <div className="flex items-center gap-2 mb-2">
              <p className="text-xs font-medium text-gray-600">Cost & depreciation</p>
              {locked && (
                <span className="inline-flex items-center gap-1 text-xs text-amber-600">
                  <Lock size={11} /> Locked — depreciation has been posted. Reverse the runs to change these.
                </span>
              )}
            </div>
            <div className="grid grid-cols-3 gap-3">
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Purchase Date *</label>
                <DateInput className={inputClass} value={form.purchaseDate} onChange={(e) => set("purchaseDate", e.target.value)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Depreciation Starts</label>
                <DateInput className={inputClass} value={form.depreciationStartDate} disabled={locked} onChange={(e) => set("depreciationStartDate", e.target.value)} />
                {!editing && <p className="text-[11px] text-gray-400 mt-0.5">Blank = purchase date</p>}
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Purchase Cost *</label>
                <input type="number" min={0} className={inputClass} disabled={locked} value={form.purchaseCost || ""} onChange={(e) => set("purchaseCost", parseFloat(e.target.value) || 0)} />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Useful Life (years)</label>
                <input
                  type="number" min={0.1} step={0.5} className={inputClass} disabled={locked}
                  placeholder={selectedCategory ? `${selectedCategory.usefulLifeMonths / 12} (category default)` : ""}
                  value={form.usefulLifeYears}
                  onChange={(e) => set("usefulLifeYears", e.target.value)}
                />
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Method</label>
                <select className={inputClass} disabled={locked} value={form.depreciationMethod} onChange={(e) => set("depreciationMethod", e.target.value as DepreciationMethod | "")}>
                  {!editing && <option value="">{selectedCategory ? `${depreciationMethodLabels[selectedCategory.depreciationMethod]} (category default)` : "Category default"}</option>}
                  {(Object.keys(depreciationMethodLabels) as DepreciationMethod[]).map((m) => (
                    <option key={m} value={m}>{depreciationMethodLabels[m]}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-xs text-gray-500 mb-1 block">Salvage Value</label>
                <input
                  type="number" min={0} className={inputClass} disabled={locked}
                  placeholder={selectedCategory && form.purchaseCost ? `${fmt(form.purchaseCost * selectedCategory.salvageValuePercent / 100)} (${selectedCategory.salvageValuePercent}%)` : ""}
                  value={form.salvageValue}
                  onChange={(e) => set("salvageValue", e.target.value)}
                />
              </div>
            </div>

            {!editing && (
              <div className="grid grid-cols-3 gap-3 mt-3 bg-gray-50 rounded-lg p-3">
                <p className="text-[11px] text-gray-500 col-span-3">
                  Already owned before using this system? Enter the depreciation charged so far and the month it covers — the first run will only charge the months after that.
                </p>
                <div>
                  <label className="text-xs text-gray-500 mb-1 block">Opening Accumulated Depreciation</label>
                  <input type="number" min={0} className={inputClass} value={form.openingAccumulatedDepreciation || ""} onChange={(e) => set("openingAccumulatedDepreciation", parseFloat(e.target.value) || 0)} />
                </div>
                <div>
                  <label className="text-xs text-gray-500 mb-1 block">Depreciated Up To (month)</label>
                  <input type="month" className={inputClass} value={form.openingDepreciatedUpTo} onChange={(e) => set("openingDepreciatedUpTo", e.target.value)} />
                </div>
              </div>
            )}
          </section>

          <section className="border-t border-gray-100 pt-3">
            <p className="text-xs font-medium text-gray-600 mb-2">Purchase & accounting links</p>
            <div className="grid grid-cols-3 gap-3">
              <SearchableDropdown
                label="Vendor Bill"
                options={billOptions}
                value={form.billId}
                onChange={(v) => set("billId", v ?? "")}
                placeholder="None"
                searchPlaceholder="Search approved bills..."
              />
              <SearchableDropdown
                label="Vendor"
                options={vendorOptions}
                value={form.vendorId}
                onChange={(v) => set("vendorId", v ?? "")}
                placeholder={form.billId ? "From the bill" : "None"}
                searchPlaceholder="Search vendors..."
              />
              <SearchableDropdown
                label="Cost Center (depreciation charged to)"
                options={costCenterOptions}
                value={form.costCenterId}
                onChange={(v) => set("costCenterId", v ?? "")}
                placeholder="None (company level)"
                searchPlaceholder="Search cost centers..."
              />
            </div>
          </section>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Description</label>
              <textarea rows={2} className={inputClass} value={form.description} onChange={(e) => set("description", e.target.value)} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Notes</label>
              <textarea rows={2} className={inputClass} value={form.notes} onChange={(e) => set("notes", e.target.value)} />
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : editing ? "Update Asset" : "Register Asset"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
