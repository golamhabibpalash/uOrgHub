import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getChartOfAccounts,
  getAccountGroups,
  createChartOfAccount,
  updateChartOfAccount,
  deleteChartOfAccount,
  ChartOfAccount,
  AccountGroupType,
} from "../../api/accounts";

const typeColors: Record<AccountGroupType, string> = {
  Asset: "bg-blue-50 text-blue-700",
  Liability: "bg-red-50 text-red-700",
  Equity: "bg-purple-50 text-purple-700",
  Income: "bg-green-50 text-green-700",
  Expense: "bg-orange-50 text-orange-700",
};

export default function ChartOfAccounts() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "accountName" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<ChartOfAccount | null>(null);
  const [form, setForm] = useState({
    accountCode: "",
    accountName: "",
    accountGroupId: "",
    accountType: "Asset" as AccountGroupType,
    openingBalance: 0,
    description: "",
    allowDirectEntry: true,
    isActive: true,
  });

  const { data, isLoading } = useQuery({
    queryKey: ["chart-of-accounts", dg.page, dg.search, dg.sortBy, dg.sortDescending],
    queryFn: () => getChartOfAccounts(dg.queryParams),
  });

  const { data: groupsData } = useQuery({
    queryKey: ["account-groups", 1, ""],
    queryFn: () => getAccountGroups({ page: 1, pageSize: 100 }),
  });

  const accounts = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const groups = groupsData?.data?.data?.items ?? [];
  const [saveError, setSaveError] = useState("");

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateChartOfAccount(editing.id, form) : createChartOfAccount(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["chart-of-accounts"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save account.";
      setSaveError(msg);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteChartOfAccount(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["chart-of-accounts"] }),
  });

  function openAdd() {
    setEditing(null);
    setForm({ accountCode: "", accountName: "", accountGroupId: groups[0]?.id ?? "", accountType: "Asset", openingBalance: 0, description: "", allowDirectEntry: true, isActive: true });
    setSaveError("");
    setModal(true);
  }

  function openEdit(acc: ChartOfAccount) {
    setEditing(acc);
    setForm({
      accountCode: acc.accountCode,
      accountName: acc.accountName,
      accountGroupId: acc.accountGroupId,
      accountType: acc.accountType,
      openingBalance: acc.openingBalance,
      description: acc.description ?? "",
      allowDirectEntry: acc.allowDirectEntry,
      isActive: acc.isActive,
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  const columns = [
    { key: "accountCode", label: "Code" },
    { key: "accountName", label: "Account Name" },
    { key: "accountGroupName", label: "Group" },
    {
      key: "accountType",
      label: "Type",
      render: (row: ChartOfAccount) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${typeColors[row.accountType]}`}>{row.accountType}</span>
      ),
    },
    {
      key: "currentBalance",
      label: "Balance",
      render: (row: ChartOfAccount) => (
        <span className="font-medium">{row.currentBalance.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</span>
      ),
    },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row: ChartOfAccount) => (
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
          <h2 className="text-base font-medium text-gray-900">Chart of Accounts</h2>
          <p className="text-xs text-gray-400">Manage the general ledger accounts</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Account
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={accounts}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search accounts..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        onDelete={(row) => deleteMutation.mutate(row.id)}
        emptyMessage="No accounts found"
        actions={<ExportMenu baseUrl="/accounts/chart-of-accounts" filters={{ search: dg.search || undefined }} />}
      />

      <Modal title={editing ? "Edit Account" : "Add Account"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Account Code *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.accountCode} onChange={(e) => setForm((f) => ({ ...f, accountCode: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Account Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.accountName} onChange={(e) => setForm((f) => ({ ...f, accountName: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Account Group *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.accountGroupId} onChange={(e) => setForm((f) => ({ ...f, accountGroupId: e.target.value }))}>
                <option value="">Select group</option>
                {groups.map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Account Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.accountType} onChange={(e) => setForm((f) => ({ ...f, accountType: e.target.value as AccountGroupType }))}>
                {(["Asset", "Liability", "Equity", "Income", "Expense"] as AccountGroupType[]).map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
          </div>
          {!editing && (
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Opening Balance</label>
              <input type="number" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.openingBalance} onChange={(e) => setForm((f) => ({ ...f, openingBalance: parseFloat(e.target.value) || 0 }))} />
            </div>
          )}
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description</label>
            <textarea rows={2} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <input type="checkbox" id="allowDirect" checked={form.allowDirectEntry} onChange={(e) => setForm((f) => ({ ...f, allowDirectEntry: e.target.checked }))} />
              <label htmlFor="allowDirect" className="text-xs text-gray-600">Allow direct entry</label>
            </div>
            {editing && (
              <div className="flex items-center gap-2">
                <input type="checkbox" id="isActive" checked={form.isActive} onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))} />
                <label htmlFor="isActive" className="text-xs text-gray-600">Active</label>
              </div>
            )}
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
