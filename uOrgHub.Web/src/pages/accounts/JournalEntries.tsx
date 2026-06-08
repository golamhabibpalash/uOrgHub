import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, XCircle, ChevronDown, ChevronUp } from "lucide-react";
import Pagination from "../../components/shared/Pagination";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useChartOfAccountsLookup } from "../../hooks/useEntityLookup";
import {
  getJournalEntries,
  createJournalEntry,
  postJournalEntry,
  cancelJournalEntry,
  JournalEntry,
  JournalEntryStatus,
  CreateJournalEntryLineDto,
} from "../../api/accounts";

const statusColors: Record<JournalEntryStatus, string> = {
  Draft: "bg-yellow-50 text-yellow-700",
  Posted: "bg-green-50 text-green-700",
  Cancelled: "bg-gray-100 text-gray-500",
};

export default function JournalEntries() {
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(false);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [form, setForm] = useState({
    entryDate: new Date().toISOString().split("T")[0],
    referenceNumber: "",
    description: "",
    lines: [
      { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: 1 },
      { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: 2 },
    ] as CreateJournalEntryLineDto[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["journal-entries", page, search],
    queryFn: () => getJournalEntries({ page, pageSize: 10, search }),
  });

  const { options: coaOptions } = useChartOfAccountsLookup();

  const entries = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const [saveError, setSaveError] = useState("");

  const createMutation = useMutation({
    mutationFn: () => createJournalEntry(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ["journal-entries"] }); closeModal(); },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg = axiosErr?.response?.data?.message
        ?? axiosErr?.response?.data?.errors?.[0]
        ?? "Failed to save journal entry.";
      setSaveError(msg);
    },
  });

  const postMutation = useMutation({
    mutationFn: (id: string) => postJournalEntry(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["journal-entries"] }),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => cancelJournalEntry(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["journal-entries"] }),
  });

  function openAdd() {
    setForm({
      entryDate: new Date().toISOString().split("T")[0],
      referenceNumber: "",
      description: "",
      lines: [
        { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: 1 },
        { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: 2 },
      ],
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setSaveError(""); }

  function addLine() {
    setForm((f) => ({
      ...f,
      lines: [...f.lines, { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: f.lines.length + 1 }],
    }));
  }

  function removeLine(idx: number) {
    setForm((f) => ({ ...f, lines: f.lines.filter((_, i) => i !== idx) }));
  }

  function updateLine(idx: number, field: keyof CreateJournalEntryLineDto, value: string | number) {
    setForm((f) => ({
      ...f,
      lines: f.lines.map((l, i) => i === idx ? { ...l, [field]: value } : l),
    }));
  }

  const totalDebit = form.lines.reduce((s, l) => s + (Number(l.debitAmount) || 0), 0);
  const totalCredit = form.lines.reduce((s, l) => s + (Number(l.creditAmount) || 0), 0);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.01;

  const columns = [
    { key: "entryNumber", label: "Entry #" },
    { key: "entryDate", label: "Date", render: (row: JournalEntry) => row.entryDate?.split("T")[0] ?? "" },
    { key: "description", label: "Description" },
    { key: "referenceNumber", label: "Reference" },
    {
      key: "status",
      label: "Status",
      render: (row: JournalEntry) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span>
      ),
    },
    {
      key: "totalDebit",
      label: "Debit",
      render: (row: JournalEntry) => row.totalDebit.toLocaleString("en-BD", { minimumFractionDigits: 2 }),
    },
    {
      key: "totalCredit",
      label: "Credit",
      render: (row: JournalEntry) => row.totalCredit.toLocaleString("en-BD", { minimumFractionDigits: 2 }),
    },
    {
      key: "actions",
      label: "Actions",
      render: (row: JournalEntry) => (
        <div className="flex items-center gap-2">
          <button
            onClick={() => setExpandedId(expandedId === row.id ? null : row.id)}
            className="text-gray-400 hover:text-primary-600"
            title="View lines"
          >
            {expandedId === row.id ? <ChevronUp size={15} /> : <ChevronDown size={15} />}
          </button>
          {row.status === "Draft" && (
            <>
              <button
                onClick={() => postMutation.mutate(row.id)}
                className="text-green-500 hover:text-green-700"
                title="Post"
              >
                <Send size={14} />
              </button>
              <button
                onClick={() => cancelMutation.mutate(row.id)}
                className="text-red-400 hover:text-red-600"
                title="Cancel"
              >
                <XCircle size={14} />
              </button>
            </>
          )}
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Journal Entries</h2>
          <p className="text-xs text-gray-400">Double-entry bookkeeping records</p>
        </div>
        <button onClick={openAdd} className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600">
          <Plus size={15} /> New Entry
        </button>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        <div className="px-4 py-3 border-b border-gray-100">
          <input
            type="text"
            placeholder="Search journal entries..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 w-64 focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center h-40 text-sm text-gray-400">Loading...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-50">
                  {columns.map((col) => (
                    <th key={col.key} className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">{col.label}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {entries.length === 0 ? (
                  <tr><td colSpan={columns.length} className="text-center py-10 text-gray-400">No records found</td></tr>
                ) : entries.map((entry) => (
                  <>
                    <tr key={entry.id} className="border-t border-gray-100 hover:bg-gray-50">
                      {columns.map((col) => (
                        <td key={col.key} className="px-4 py-2.5 text-gray-700">
                          {col.render ? col.render(entry) : String((entry as unknown as Record<string, unknown>)[col.key] ?? "")}
                        </td>
                      ))}
                    </tr>
                    {expandedId === entry.id && (
                      <tr key={`${entry.id}-lines`} className="bg-gray-50">
                        <td colSpan={columns.length} className="px-6 py-3">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="text-gray-500">
                                <th className="text-left pb-1">Account</th>
                                <th className="text-left pb-1">Description</th>
                                <th className="text-right pb-1">Debit</th>
                                <th className="text-right pb-1">Credit</th>
                              </tr>
                            </thead>
                            <tbody>
                              {entry.lines.map((line) => (
                                <tr key={line.id}>
                                  <td className="py-0.5">{line.accountName}</td>
                                  <td className="py-0.5 text-gray-500">{line.description}</td>
                                  <td className="py-0.5 text-right">{line.debitAmount > 0 ? line.debitAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 }) : ""}</td>
                                  <td className="py-0.5 text-right">{line.creditAmount > 0 ? line.creditAmount.toLocaleString("en-BD", { minimumFractionDigits: 2 }) : ""}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </td>
                      </tr>
                    )}
                  </>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
      </div>

      <Modal title="New Journal Entry" open={modal} onClose={closeModal}>
        <div className="space-y-3">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Entry Date *</label>
              <input type="date" className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.entryDate} onChange={(e) => setForm((f) => ({ ...f, entryDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Reference Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.referenceNumber} onChange={(e) => setForm((f) => ({ ...f, referenceNumber: e.target.value }))} />
            </div>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Description *</label>
            <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-xs text-gray-500">Entry Lines</label>
              <button onClick={addLine} className="text-xs text-primary-600 hover:underline">+ Add Line</button>
            </div>
            <div className="border border-gray-200 rounded-lg">
              <table className="w-full text-xs">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="text-left px-2 py-1.5 text-gray-500">Account</th>
                    <th className="text-left px-2 py-1.5 text-gray-500">Description</th>
                    <th className="text-right px-2 py-1.5 text-gray-500">Debit</th>
                    <th className="text-right px-2 py-1.5 text-gray-500">Credit</th>
                    <th className="px-2 py-1.5"></th>
                  </tr>
                </thead>
                <tbody>
                  {form.lines.map((line, idx) => (
                    <tr key={idx} className="border-t border-gray-100">
                      <td className="px-2 py-1">
                        <SearchableDropdown
                          options={coaOptions}
                          value={line.accountId}
                          onChange={(v) => updateLine(idx, "accountId", v ?? "")}
                          placeholder="Select"
                          searchPlaceholder="Search accounts..."
                        />
                      </td>
                      <td className="px-2 py-1">
                        <input className="w-full border border-gray-200 rounded px-1 py-1 text-xs focus:outline-none" value={line.description ?? ""} onChange={(e) => updateLine(idx, "description", e.target.value)} />
                      </td>
                      <td className="px-2 py-1">
                        <input type="number" className="w-24 border border-gray-200 rounded px-1 py-1 text-xs text-right focus:outline-none" value={line.debitAmount || ""} onChange={(e) => updateLine(idx, "debitAmount", parseFloat(e.target.value) || 0)} />
                      </td>
                      <td className="px-2 py-1">
                        <input type="number" className="w-24 border border-gray-200 rounded px-1 py-1 text-xs text-right focus:outline-none" value={line.creditAmount || ""} onChange={(e) => updateLine(idx, "creditAmount", parseFloat(e.target.value) || 0)} />
                      </td>
                      <td className="px-2 py-1">
                        {form.lines.length > 2 && (
                          <button onClick={() => removeLine(idx)} className="text-red-400 hover:text-red-600">×</button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50 border-t border-gray-200">
                  <tr>
                    <td colSpan={2} className="px-2 py-1.5 text-xs text-gray-500 font-medium">Totals</td>
                    <td className="px-2 py-1.5 text-xs text-right font-medium">{totalDebit.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                    <td className="px-2 py-1.5 text-xs text-right font-medium">{totalCredit.toLocaleString("en-BD", { minimumFractionDigits: 2 })}</td>
                    <td />
                  </tr>
                </tfoot>
              </table>
            </div>
            {!isBalanced && totalDebit > 0 && (
              <p className="text-xs text-red-500 mt-1">Debit and credit totals must be equal.</p>
            )}
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button
              onClick={() => createMutation.mutate()}
              disabled={createMutation.isPending || !isBalanced}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {createMutation.isPending ? "Saving..." : "Save Draft"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
