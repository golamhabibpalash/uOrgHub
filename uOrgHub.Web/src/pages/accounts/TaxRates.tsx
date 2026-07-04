import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, RefreshCw } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getTaxRates,
  createTaxRate,
  updateTaxRate,
  deleteTaxRate,
  getNextTaxRateCode,
  TaxRate,
  TaxType,
} from "../../api/accounts";

const TAX_TYPES: TaxType[] = ["VAT", "WithholdingTax", "CustomsDuty", "ExciseDuty", "SalesTax"];

const taxTypeColors: Record<TaxType, string> = {
  VAT: "bg-blue-50 text-blue-700",
  WithholdingTax: "bg-orange-50 text-orange-700",
  CustomsDuty: "bg-purple-50 text-purple-700",
  ExciseDuty: "bg-red-50 text-red-700",
  SalesTax: "bg-green-50 text-green-700",
};

export default function TaxRates() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<TaxRate | null>(null);
  const [form, setForm] = useState({
    name: "",
    taxType: "VAT" as TaxType,
    rate: 0,
    description: "",
    isActive: true,
  });
  const [generatedCode, setGeneratedCode] = useState("");
  const [rateDisplay, setRateDisplay] = useState("0");

  const { data, isLoading } = useQuery({
    queryKey: ["tax-rates", ...dg.queryKey],
    queryFn: () => getTaxRates(dg.queryParams),
  });

  const codeQuery = useQuery({
    queryKey: ["tax-rate-code", form.taxType],
    queryFn: () => getNextTaxRateCode(form.taxType).then((r) => r.data.data ?? ""),
    enabled: modal && !editing,
  });

  const taxRates = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const [saveError, setSaveError] = useState("");

  const saveMutation = useMutation({
    mutationFn: () => {
      if (editing) return updateTaxRate(editing.id, form);
      const code = codeQuery.data ?? generatedCode;
      return createTaxRate({ ...form, customCode: code });
    },
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["tax-rates"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save tax rate.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteTaxRate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tax-rates"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", taxType: "VAT", rate: 0, description: "", isActive: true });
    setGeneratedCode("");
    setRateDisplay("0");
    setSaveError("");
    setModal(true);
  }

  function openEdit(tr: TaxRate) {
    setEditing(tr);
    setForm({ name: tr.name, taxType: tr.taxType, rate: tr.rate, description: tr.description ?? "", isActive: tr.isActive });
    setGeneratedCode(tr.code);
    setRateDisplay(String(tr.rate));
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns = [
    { key: "code", label: "Code" },
    { key: "name", label: "Tax Name" },
    {
      key: "taxType",
      label: "Type",
      render: (row: TaxRate) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${taxTypeColors[row.taxType]}`}>{row.taxType}</span>
      ),
    },
    {
      key: "rate",
      label: "Rate (%)",
      render: (row: TaxRate) => <span className="font-medium">{row.rate.toFixed(2)}%</span>,
    },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row: TaxRate) => (
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
          <h2 className="text-base font-medium text-gray-900">Tax Rates</h2>
          <p className="text-xs text-gray-400">Configure VAT, WHT and other tax rates</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Tax Rate
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={taxRates}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search tax rates..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No tax rates found"
        actions={<ExportMenu baseUrl="/accounts/tax-rates" filters={{ search: dg.search || undefined }} />}
      />

      <Modal title={editing ? "Edit Tax Rate" : "Add Tax Rate"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Code</label>
              <div className="flex gap-2">
                <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-500" value={codeQuery.data ?? generatedCode} disabled readOnly />
                {!editing && (
                  <button type="button" onClick={() => codeQuery.refetch()} className="px-2 py-2 border border-gray-200 rounded-lg hover:bg-gray-50" title="Generate new code">
                    <RefreshCw size={15} />
                  </button>
                )}
              </div>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Tax Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.taxType} onChange={(e) => setForm((f) => ({ ...f, taxType: e.target.value as TaxType }))} disabled={!!editing}>
                {TAX_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Rate (%) *</label>
              <input
                type="number"
                min={0} max={100} step={0.01}
                className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                value={rateDisplay}
                onFocus={() => { if (form.rate === 0) setRateDisplay(""); }}
                onBlur={() => { if (rateDisplay === "") { setRateDisplay("0"); setForm((f) => ({ ...f, rate: 0 })); } }}
                onChange={(e) => {
                  const raw = e.target.value;
                  setRateDisplay(raw);
                  const parsed = parseFloat(raw);
                  if (!isNaN(parsed)) setForm((f) => ({ ...f, rate: parsed }));
                }}
              />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>
          {editing && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} />
              <label htmlFor="isActive" className="text-xs text-gray-600">Active</label>
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
