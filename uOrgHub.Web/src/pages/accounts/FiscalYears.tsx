import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getFiscalYears,
  createFiscalYear,
  updateFiscalYear,
  deleteFiscalYear,
  FiscalYear,
  FiscalYearStatus,
} from "../../api/accounts";

const statusColors: Record<FiscalYearStatus, string> = {
  Active: "bg-green-50 text-green-700",
  Closed: "bg-gray-100 text-gray-600",
  Pending: "bg-yellow-50 text-yellow-700",
};

export default function FiscalYears() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "name" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<FiscalYear | null>(null);
  const [form, setForm] = useState({
    name: "",
    startDate: "",
    endDate: "",
    status: "Pending" as FiscalYearStatus,
    isCurrent: false,
  });
  const [saveError, setSaveError] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["fiscal-years", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getFiscalYears(dg.queryParams),
  });

  const fiscalYears = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateFiscalYear(editing.id, form) : createFiscalYear(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["fiscal-years"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save fiscal year.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteFiscalYear(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["fiscal-years"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ name: "", startDate: "", endDate: "", status: "Pending", isCurrent: false });
    setSaveError("");
    setModal(true);
  }

  function openEdit(fy: FiscalYear) {
    setEditing(fy);
    setForm({
      name: fy.name,
      startDate: fy.startDate?.split("T")[0] ?? "",
      endDate: fy.endDate?.split("T")[0] ?? "",
      status: fy.status,
      isCurrent: fy.isCurrent,
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns = [
    { key: "name", label: "Fiscal Year" },
    { key: "startDate", label: "Start Date", render: (row: FiscalYear) => row.startDate?.split("T")[0] ?? "" },
    { key: "endDate", label: "End Date", render: (row: FiscalYear) => row.endDate?.split("T")[0] ?? "" },
    {
      key: "status",
      label: "Status",
      sortable: false,
      render: (row: FiscalYear) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span>
      ),
    },
    {
      key: "isCurrent",
      label: "Current",
      sortable: false,
      render: (row: FiscalYear) => row.isCurrent ? (
        <span className="text-xs px-2 py-0.5 rounded-full bg-blue-50 text-blue-700">Current</span>
      ) : null,
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Fiscal Years</h2>
          <p className="text-xs text-gray-400">Manage financial year periods</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Fiscal Year
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={fiscalYears}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search fiscal years..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No fiscal years found"
        actions={<ExportMenu baseUrl="/accounts/fiscal-years" filters={{ search: dg.search || undefined }} />}
      />

      <Modal title={editing ? "Edit Fiscal Year" : "Add Fiscal Year"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Name *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} placeholder="e.g. FY 2025-2026" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Start Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.startDate} onChange={(e) => setForm((f) => ({ ...f, startDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">End Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.endDate} onChange={(e) => setForm((f) => ({ ...f, endDate: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Status</label>
            <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.status} onChange={(e) => setForm((f) => ({ ...f, status: e.target.value as FiscalYearStatus }))}>
              <option value="Pending">Pending</option>
              <option value="Active">Active</option>
              <option value="Closed">Closed</option>
            </select>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isCurrent" checked={form.isCurrent} onChange={(e) => setForm((f) => ({ ...f, isCurrent: e.target.checked }))} />
            <label htmlFor="isCurrent" className="text-xs text-gray-600">Set as current fiscal year</label>
          </div>
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
