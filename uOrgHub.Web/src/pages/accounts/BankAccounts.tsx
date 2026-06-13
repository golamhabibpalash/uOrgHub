import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, ArrowRight, ArrowLeftRight } from "lucide-react";
import DataGrid from "../../components/shared/DataGrid";
import DataTable from "../../components/shared/DataTable";
import Pagination from "../../components/shared/Pagination";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import ExportMenu from "../../components/shared/ExportMenu";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useChartOfAccountsLookup } from "../../hooks/useEntityLookup";
import {
  getBankAccounts,
  createBankAccount,
  updateBankAccount,
  getBankTransactions,
  createBankTransaction,
  BankAccount,
  BankTransaction,
  BankTransactionType,
} from "../../api/accounts";

const TXN_TYPES: BankTransactionType[] = ["Deposit", "Withdrawal", "Transfer", "Fee", "Interest", "ChequeDeposit", "ChequeIssue"];

const txnTypeColors: Record<BankTransactionType, string> = {
  Deposit: "bg-green-50 text-green-700",
  Withdrawal: "bg-red-50 text-red-700",
  Transfer: "bg-blue-50 text-blue-700",
  Fee: "bg-orange-50 text-orange-700",
  Interest: "bg-teal-50 text-teal-700",
  ChequeDeposit: "bg-emerald-50 text-emerald-700",
  ChequeIssue: "bg-rose-50 text-rose-700",
};

export default function BankAccounts() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "accountName" });
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState<BankAccount | null>(null);
  const [selectedAccount, setSelectedAccount] = useState<BankAccount | null>(null);
  const [txnModal, setTxnModal] = useState(false);
  const [txnPage, setTxnPage] = useState(1);

  const [form, setForm] = useState({
    accountNumber: "",
    accountName: "",
    bankName: "",
    branchName: "",
    routingNumber: "",
    currency: "BDT",
    openingBalance: 0,
    chartOfAccountId: "",
    isActive: true,
  });

  const [txnForm, setTxnForm] = useState({
    transactionType: "Deposit" as BankTransactionType,
    transactionDate: new Date().toISOString().split("T")[0],
    amount: 0,
    description: "",
    referenceNumber: "",
    chequeNumber: "",
    payee: "",
  });

  const { data, isLoading } = useQuery({
    queryKey: ["bank-accounts", ...dg.queryKey],
    queryFn: () => getBankAccounts(dg.queryParams),
  });

  const { options: coaOptions } = useChartOfAccountsLookup("Asset");

  const { data: txnData, isLoading: txnLoading } = useQuery({
    queryKey: ["bank-transactions", selectedAccount?.id, txnPage],
    queryFn: () => getBankTransactions(selectedAccount!.id, { page: txnPage, pageSize: 10 }),
    enabled: !!selectedAccount,
  });

  const bankAccounts = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const transactions = txnData?.data?.data?.items ?? [];
  const txnTotalPages = txnData?.data?.data?.totalPages ?? 1;
  const [saveError, setSaveError] = useState("");

  const saveMutation = useMutation({
    mutationFn: () => editing ? updateBankAccount(editing.id, form) : createBankAccount(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["bank-accounts"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save bank account.";
      setSaveError(msg);
    },
  });

  const txnMutation = useMutation({
    mutationFn: () => createBankTransaction(selectedAccount!.id, txnForm),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["bank-transactions", selectedAccount?.id] });
      qc.invalidateQueries({ queryKey: ["bank-accounts"] });
      setTxnModal(false);
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save transaction.";
      setSaveError(msg);
    },
  });

  function openAdd() {
    setEditing(null);
    setForm({ accountNumber: "", accountName: "", bankName: "", branchName: "", routingNumber: "", currency: "BDT", openingBalance: 0, chartOfAccountId: coaOptions[0]?.value ?? "", isActive: true });
    setSaveError("");
    setModal(true);
  }

  function openEdit(ba: BankAccount) {
    setEditing(ba);
    setForm({ accountNumber: ba.accountNumber, accountName: ba.accountName, bankName: ba.bankName, branchName: ba.branchName ?? "", routingNumber: ba.routingNumber ?? "", currency: ba.currency, openingBalance: ba.openingBalance, chartOfAccountId: ba.chartOfAccountId, isActive: ba.isActive });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditing(null); setSaveError(""); }

  function openTxnModal() {
    setTxnForm({ transactionType: "Deposit", transactionDate: new Date().toISOString().split("T")[0], amount: 0, description: "", referenceNumber: "", chequeNumber: "", payee: "" });
    setSaveError("");
    setTxnModal(true);
  }

  const columns = [
    { key: "accountNumber", label: "Account No." },
    { key: "accountName", label: "Account Name" },
    { key: "bankName", label: "Bank" },
    { key: "branchName", label: "Branch" },
    { key: "currency", label: "Currency" },
    {
      key: "currentBalance",
      label: "Balance",
      render: (row: BankAccount) => (
        <span className={`font-medium ${row.currentBalance >= 0 ? "text-green-700" : "text-red-600"}`}>
          {row.currentBalance.toLocaleString("en-BD", { minimumFractionDigits: 2 })}
        </span>
      ),
    },
    {
      key: "isActive",
      label: "Status",
      sortable: false,
      render: (row: BankAccount) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isActive ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
          {row.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
    {
      key: "txn",
      label: "Transactions",
      sortable: false,
      render: (row: BankAccount) => (
        <button
          onClick={() => setSelectedAccount(row)}
          className="flex items-center gap-1 text-xs text-primary-600 hover:underline"
        >
          <ArrowRight size={12} /> View
        </button>
      ),
    },
  ];

  const txnColumns = [
    { key: "transactionDate", label: "Date", render: (row: BankTransaction) => row.transactionDate?.split("T")[0] ?? "" },
    {
      key: "transactionType",
      label: "Type",
      render: (row: BankTransaction) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${txnTypeColors[row.transactionType]}`}>{row.transactionType}</span>
      ),
    },
    { key: "description", label: "Description" },
    { key: "referenceNumber", label: "Reference" },
    { key: "payee", label: "Payee" },
    {
      key: "amount",
      label: "Amount",
      render: (row: BankTransaction) => (
        <span className={`font-medium ${["Deposit", "ChequeDeposit", "Interest"].includes(row.transactionType) ? "text-green-700" : "text-red-600"}`}>
          {row.amount.toLocaleString("en-BD", { minimumFractionDigits: 2 })}
        </span>
      ),
    },
    {
      key: "isReconciled",
      label: "Reconciled",
      render: (row: BankTransaction) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${row.isReconciled ? "bg-green-50 text-green-700" : "bg-gray-100 text-gray-500"}`}>
          {row.isReconciled ? "Yes" : "No"}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Bank Accounts</h2>
          <p className="text-xs text-gray-400">Manage company bank accounts and transactions</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> Add Bank Account
        </button>
      </div>

      <DataGrid
        columns={columns}
        data={bankAccounts}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search bank accounts..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        onEdit={openEdit}
        emptyMessage="No bank accounts found"
        actions={<ExportMenu baseUrl="/accounts/bank-accounts" filters={{ search: dg.search || undefined }} />}
      />

      {selectedAccount && (
        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <ArrowLeftRight size={15} className="text-primary-600" />
              <span className="text-sm font-medium text-gray-900">{selectedAccount.accountName} — Transactions</span>
              <span className="text-xs text-gray-400 ml-2">Balance: {selectedAccount.currentBalance.toLocaleString("en-BD", { minimumFractionDigits: 2 })} {selectedAccount.currency}</span>
            </div>
            <div className="flex items-center gap-2">
              <button onClick={openTxnModal} className="flex items-center gap-1 bg-primary-500 text-white text-xs px-3 py-1.5 rounded-lg hover:bg-primary-600">
                <Plus size={12} /> Add Transaction
              </button>
              <ExportMenu baseUrl={`/accounts/bank-accounts/${selectedAccount.id}/transactions`} filters={{}} />
              <button onClick={() => setSelectedAccount(null)} className="text-xs text-gray-400 hover:text-gray-600 px-2 py-1">Close</button>
            </div>
          </div>
          <DataTable columns={txnColumns} data={transactions} loading={txnLoading} />
          <Pagination page={txnPage} totalPages={txnTotalPages} onPageChange={setTxnPage} />
        </div>
      )}

      <Modal title={editing ? "Edit Bank Account" : "Add Bank Account"} open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Account Number *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.accountNumber} onChange={(e) => setForm((f) => ({ ...f, accountNumber: e.target.value }))} disabled={!!editing} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Account Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.accountName} onChange={(e) => setForm((f) => ({ ...f, accountName: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Bank Name *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.bankName} onChange={(e) => setForm((f) => ({ ...f, bankName: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Branch Name</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.branchName} onChange={(e) => setForm((f) => ({ ...f, branchName: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Routing Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.routingNumber} onChange={(e) => setForm((f) => ({ ...f, routingNumber: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Currency</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.currency} onChange={(e) => setForm((f) => ({ ...f, currency: e.target.value }))}>
                <option value="BDT">BDT</option>
                <option value="USD">USD</option>
                <option value="EUR">EUR</option>
                <option value="GBP">GBP</option>
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
            <SearchableDropdown
              label="Linked GL Account *"
              options={coaOptions}
              value={form.chartOfAccountId}
              onChange={(v) => setForm((f) => ({ ...f, chartOfAccountId: v ?? "" }))}
              placeholder="Select account"
              searchPlaceholder="Search accounts..."
              required
            />
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

      <Modal title={`New Transaction — ${selectedAccount?.accountName}`} open={txnModal} onClose={() => { setSaveError(""); setTxnModal(false); }}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Transaction Type *</label>
              <select className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.transactionType} onChange={(e) => setTxnForm((f) => ({ ...f, transactionType: e.target.value as BankTransactionType }))}>
                {TXN_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.transactionDate} onChange={(e) => setTxnForm((f) => ({ ...f, transactionDate: e.target.value }))} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Amount *</label>
              <input type="number" min={0} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.amount || ""} onChange={(e) => setTxnForm((f) => ({ ...f, amount: parseFloat(e.target.value) || 0 }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Reference Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.referenceNumber} onChange={(e) => setTxnForm((f) => ({ ...f, referenceNumber: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.description} onChange={(e) => setTxnForm((f) => ({ ...f, description: e.target.value }))} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Cheque Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.chequeNumber} onChange={(e) => setTxnForm((f) => ({ ...f, chequeNumber: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Payee</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={txnForm.payee} onChange={(e) => setTxnForm((f) => ({ ...f, payee: e.target.value }))} />
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <button onClick={() => { setSaveError(""); setTxnModal(false); }} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button onClick={() => txnMutation.mutate()} disabled={txnMutation.isPending} className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50">
              {txnMutation.isPending ? "Saving..." : "Save"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
