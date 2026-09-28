import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useChartOfAccountsLookup } from "../../hooks/useEntityLookup";
import {
  getAssetCategories,
  createAssetCategory,
  updateAssetCategory,
  deleteAssetCategory,
  AssetCategory,
  DepreciationMethod,
  depreciationMethodLabels,
} from "../../api/accounts";

const inputClass = "w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500";

const emptyForm = {
  name: "",
  description: "",
  depreciationMethod: "StraightLine" as DepreciationMethod,
  usefulLifeYears: 10,
  salvageValuePercent: 5,
  assetAccountId: "",
  accumulatedDepreciationAccountId: "",
  depreciationExpenseAccountId: "",
  isActive: true,
};

export default function AssetCategories() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<AssetCategory | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [saveError, setSaveError] = useState("");

  // Accumulated depreciation is a contra-asset, so both balance-sheet pickers draw from Asset accounts.
  const { options: assetAccountOptions } = useChartOfAccountsLookup("Asset");
  const { options: expenseAccountOptions } = useChartOfAccountsLookup("Expense");

  const { data, isLoading } = useQuery({
    queryKey: ["asset-categories", ...dg.queryKey],
    queryFn: () => getAssetCategories(dg.queryParams),
  });

  const categories = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload = {
        name: form.name,
        description: form.description || undefined,
        depreciationMethod: form.depreciationMethod,
        usefulLifeMonths: Math.round(form.usefulLifeYears * 12),
        salvageValuePercent: form.salvageValuePercent,
        assetAccountId: form.assetAccountId,
        accumulatedDepreciationAccountId: form.accumulatedDepreciationAccountId,
        depreciationExpenseAccountId: form.depreciationExpenseAccountId,
        isActive: form.isActive,
      };
      return editing ? updateAssetCategory(editing.id, payload) : createAssetCategory(payload);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["asset-categories"] });
      qc.invalidateQueries({ queryKey: ["asset-categories-all"] });
      closeModal();
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      setSaveError(axiosErr?.response?.data?.message ?? axiosErr?.response?.data?.errors?.[0] ?? "Failed to save asset category.");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteAssetCategory(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["asset-categories"] });
      qc.invalidateQueries({ queryKey: ["asset-categories-all"] });
    },
  });

  function openAdd() {
    setEditing(null);
    setForm(emptyForm);
    setSaveError("");
    setModal(true);
  }

  function openEdit(c: AssetCategory) {
    setEditing(c);
    setForm({
      name: c.name,
      description: c.description ?? "",
      depreciationMethod: c.depreciationMethod,
      usefulLifeYears: c.usefulLifeMonths / 12,
      salvageValuePercent: c.salvageValuePercent,
      assetAccountId: c.assetAccountId,
      accumulatedDepreciationAccountId: c.accumulatedDepreciationAccountId,
      depreciationExpenseAccountId: c.depreciationExpenseAccountId,
      isActive: c.isActive,
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns: DataGridColumn<AssetCategory>[] = [
    { key: "code", label: "Code", className: "font-mono text-xs text-gray-500" },
    { key: "name", label: "Name" },
    { key: "depreciationMethod", label: "Method", sortable: false, render: (row) => depreciationMethodLabels[row.depreciationMethod] },
    {
      key: "usefulLifeMonths",
      label: "Useful Life",
      render: (row) => `${(row.usefulLifeMonths / 12).toLocaleString("en-BD", { maximumFractionDigits: 1 })} yrs`,
    },
    { key: "salvageValuePercent", label: "Salvage", sortable: false, render: (row) => `${row.salvageValuePercent}%` },
    { key: "assetAccountName", label: "Asset Account", sortable: false },
    { key: "depreciationExpenseAccountName", label: "Expense Account", sortable: false },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
          {row.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Asset Categories</h2>
          <p className="text-xs text-gray-400">Depreciation defaults and GL accounts for each class of fixed asset</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Category
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={categories}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search categories..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No asset categories yet — add one (e.g. Heavy Machinery) before registering assets"
      />

      <Modal title={editing ? "Edit Asset Category" : "Add Asset Category"} open={modal} onClose={closeModal} size="2xl">
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{saveError}</div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Code</label>
              <div className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-500">
                {editing ? editing.code : "Auto-generated on save"}
              </div>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Name *</label>
              <input className={inputClass} placeholder="e.g. Heavy Machinery" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
          </div>

          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Depreciation Method *</label>
              <select className={inputClass} value={form.depreciationMethod} onChange={(e) => setForm((f) => ({ ...f, depreciationMethod: e.target.value as DepreciationMethod }))}>
                {(Object.keys(depreciationMethodLabels) as DepreciationMethod[]).map((m) => (
                  <option key={m} value={m}>{depreciationMethodLabels[m]}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Useful Life (years) *</label>
              <input type="number" min={0.1} step={0.5} className={inputClass} value={form.usefulLifeYears || ""} onChange={(e) => setForm((f) => ({ ...f, usefulLifeYears: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Salvage Value (% of cost)</label>
              <input type="number" min={0} max={100} className={inputClass} value={form.salvageValuePercent} onChange={(e) => setForm((f) => ({ ...f, salvageValuePercent: parseFloat(e.target.value) || 0 }))} />
            </div>
          </div>

          <div className="border-t border-gray-100 pt-3 space-y-3">
            <p className="text-xs text-gray-400">
              Buy the asset on a vendor bill with its line on the <strong>asset account</strong>. Each monthly depreciation run then posts
              Dr <strong>depreciation expense</strong> / Cr <strong>accumulated depreciation</strong>.
              {editing && " Accounts are locked once any asset in this category has posted depreciation."}
            </p>
            <SearchableDropdown
              label="Asset Account * (e.g. Plant & Machinery)"
              options={assetAccountOptions}
              value={form.assetAccountId}
              onChange={(v) => setForm((f) => ({ ...f, assetAccountId: v ?? "" }))}
              placeholder="Select asset account"
              searchPlaceholder="Search accounts..."
              required
            />
            <SearchableDropdown
              label="Accumulated Depreciation Account * (contra-asset)"
              options={assetAccountOptions}
              value={form.accumulatedDepreciationAccountId}
              onChange={(v) => setForm((f) => ({ ...f, accumulatedDepreciationAccountId: v ?? "" }))}
              placeholder="Select accumulated depreciation account"
              searchPlaceholder="Search accounts..."
              required
            />
            <SearchableDropdown
              label="Depreciation Expense Account *"
              options={expenseAccountOptions}
              value={form.depreciationExpenseAccountId}
              onChange={(v) => setForm((f) => ({ ...f, depreciationExpenseAccountId: v ?? "" }))}
              placeholder="Select expense account"
              searchPlaceholder="Search accounts..."
              required
            />
          </div>

          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className={inputClass} value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>
          {editing && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="assetCategoryActive" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} />
              <label htmlFor="assetCategoryActive" className="text-xs text-gray-600">Active (inactive categories can't take new assets)</label>
            </div>
          )}
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {saveMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
