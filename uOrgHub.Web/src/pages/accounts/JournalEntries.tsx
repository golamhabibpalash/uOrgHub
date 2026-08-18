import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Plus, Send, XCircle, ChevronDown, ChevronUp, Pencil, Check, AlertCircle, Trash2, Lock } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import Modal from "../../components/shared/Modal";
import SearchableDropdown from "../../components/shared/SearchableDropdown";
import { useChartOfAccountsLookup, useCostCenterLookup } from "../../hooks/useEntityLookup";
import {
  getJournalEntries,
  createJournalEntry,
  updateJournalEntry,
  postJournalEntry,
  cancelJournalEntry,
  deleteJournalEntry,
  JournalEntry,
  JournalEntryStatus,
  CreateJournalEntryLineDto,
} from "../../api/accounts";
import DateInput from "../../components/shared/DateInput";

const statusColors: Record<JournalEntryStatus, string> = {
  Draft: "bg-yellow-50 text-yellow-700",
  Posted: "bg-green-50 text-green-700",
  Cancelled: "bg-gray-100 text-gray-500",
};

const selectClass =
  "text-sm border border-gray-200 rounded-lg px-2.5 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500 text-gray-600";

const fmtAmount = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2 });

export default function JournalEntries() {
  const qc = useQueryClient();
  // Newest first: a journal is read from the most recent entry backwards. The debounce keeps a
  // burst of typing to one request instead of one per keystroke.
  const dg = useDataGrid({
    defaultSortBy: "entryDate",
    defaultSortDescending: true,
    searchDebounceMs: 300,
  });
  const [modal, setModal] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [form, setForm] = useState({
    entryDate: new Date().toISOString().split("T")[0],
    referenceNumber: "",
    description: "",
    lines: [
      { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: 1, costCenterId: "" },
      { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: 2, costCenterId: "" },
    ] as CreateJournalEntryLineDto[],
  });

  const { data, isLoading } = useQuery({
    queryKey: ["journal-entries", ...dg.queryKey],
    queryFn: () => getJournalEntries(dg.queryParams),
    // Keeps the previous page on screen while the next one loads, so paging and sorting don't
    // blank the table out from under the reader.
    placeholderData: (prev) => prev,
  });

  const { options: coaOptions } = useChartOfAccountsLookup();
  const { options: costCenterOptions } = useCostCenterLookup();

  const entries = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;
  const [saveError, setSaveError] = useState("");

  const saveMutation = useMutation({
    mutationFn: () => {
      if (editingId) {
        return updateJournalEntry(editingId, {
          id: editingId,
          entryDate: form.entryDate,
          referenceNumber: form.referenceNumber || undefined,
          description: form.description,
          lines: form.lines.map((l, i) => ({ ...l, lineOrder: i + 1, costCenterId: l.costCenterId || undefined })),
        });
      }
      return createJournalEntry({
        ...form,
        lines: form.lines.map((l, i) => ({ ...l, lineOrder: i + 1, costCenterId: l.costCenterId || undefined })),
      });
    },
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

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteJournalEntry(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["journal-entries"] }),
  });

  function openAdd() {
    setEditingId(null);
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

  function openEdit(entry: JournalEntry) {
    setEditingId(entry.id);
    setForm({
      entryDate: entry.entryDate?.split("T")[0] ?? "",
      referenceNumber: entry.referenceNumber ?? "",
      description: entry.description,
      lines: entry.lines.map((l) => ({
        accountId: l.accountId,
        description: l.description ?? "",
        debitAmount: l.debitAmount,
        creditAmount: l.creditAmount,
        lineOrder: l.lineOrder,
        costCenterId: l.costCenterId ?? "",
      })),
    });
    setSaveError("");
    setModal(true);
  }

  function closeModal() { setModal(false); setEditingId(null); setSaveError(""); }

  function addLine() {
    setForm((f) => ({
      ...f,
      lines: [...f.lines, { accountId: "", description: "", debitAmount: 0, creditAmount: 0, lineOrder: f.lines.length + 1, costCenterId: "" }],
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

  const columns: DataGridColumn<JournalEntry>[] = [
    { key: "entryNumber", label: "Entry #", className: "font-mono text-xs text-gray-500" },
    { key: "entryDate", label: "Date", render: (row) => row.entryDate?.split("T")[0] ?? "" },
    { key: "description", label: "Description" },
    { key: "referenceNumber", label: "Reference" },
    {
      key: "sourceDocumentNumber",
      label: "Source",
      render: (row) =>
        row.isSystemGenerated ? (
          <span className="text-xs text-gray-600">
            {row.sourceDocumentType}{" "}
            <span className="font-mono text-gray-500">{row.sourceDocumentNumber}</span>
          </span>
        ) : (
          <span className="text-xs text-gray-300">Manual</span>
        ),
    },
    {
      key: "status",
      label: "Status",
      render: (row) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`}>{row.status}</span>
      ),
    },
    {
      key: "totalDebit",
      label: "Debit",
      headerClassName: "text-right",
      className: "text-right tabular-nums",
      render: (row) => fmtAmount(row.totalDebit),
    },
    {
      key: "totalCredit",
      label: "Credit",
      headerClassName: "text-right",
      className: "text-right tabular-nums",
      render: (row) => fmtAmount(row.totalCredit),
    },
    {
      key: "actions",
      label: "Actions",
      // Not a column on the entry — there is nothing for the server to order by.
      sortable: false,
      render: (row) => (
        <div className="flex items-center gap-2">
          <button
            onClick={() => setExpandedId(expandedId === row.id ? null : row.id)}
            className="text-gray-400 hover:text-primary-600"
            title="View lines"
          >
            {expandedId === row.id ? <ChevronUp size={15} /> : <ChevronDown size={15} />}
          </button>
          {/* A generated entry is driven by its source document's workflow. Showing a lock rather
              than dead buttons says why the actions are missing — the server rejects them anyway,
              so offering them would only produce a click and an error. */}
          {row.isSystemGenerated ? (
            <span
              className="inline-flex items-center gap-1 text-gray-300"
              title={`Managed by ${row.sourceDocumentType} ${row.sourceDocumentNumber} (${row.sourceDocumentStatus}). Post or reverse it from there.`}
            >
              <Lock size={13} />
            </span>
          ) : (
            <>
              {row.status === "Draft" && (
                <>
                  <button
                    onClick={() => openEdit(row)}
                    className="text-gray-400 hover:text-primary-600"
                    title="Edit"
                  >
                    <Pencil size={14} />
                  </button>
                  <button
                    onClick={() => postMutation.mutate(row.id)}
                    className="text-green-500 hover:text-green-700"
                    title="Post"
                  >
                    <Send size={14} />
                  </button>
                  <button
                    onClick={() => {
                      if (window.confirm("Delete this draft journal entry?")) deleteMutation.mutate(row.id);
                    }}
                    className="text-red-400 hover:text-red-600"
                    title="Delete"
                  >
                    <Trash2 size={14} />
                  </button>
                </>
              )}
              {row.status === "Posted" && (
                <button
                  onClick={() => {
                    if (window.confirm("Cancel this posted entry? This reverses all balance changes.")) cancelMutation.mutate(row.id);
                  }}
                  className="text-red-400 hover:text-red-600"
                  title="Cancel"
                >
                  <XCircle size={14} />
                </button>
              )}
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

      <DataGrid
        columns={columns}
        data={entries}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search entry no, description, reference..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No journal entries found"
        expandedRowId={expandedId}
        renderExpandedRow={(entry) => (
          <table className="w-full text-xs">
            <thead>
              <tr className="text-gray-500">
                <th className="text-left pb-1">Account</th>
                <th className="text-left pb-1">Description</th>
                <th className="text-left pb-1">Cost Center</th>
                <th className="text-right pb-1">Debit</th>
                <th className="text-right pb-1">Credit</th>
              </tr>
            </thead>
            <tbody>
              {entry.lines.map((line) => (
                <tr key={line.id}>
                  <td className="py-0.5">{line.accountName}</td>
                  <td className="py-0.5 text-gray-500">{line.description}</td>
                  <td className="py-0.5 text-gray-500">{line.costCenterName}</td>
                  <td className="py-0.5 text-right tabular-nums">{line.debitAmount > 0 ? fmtAmount(line.debitAmount) : ""}</td>
                  <td className="py-0.5 text-right tabular-nums">{line.creditAmount > 0 ? fmtAmount(line.creditAmount) : ""}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        toolbarPrefix={
          <select
            className={selectClass}
            value={dg.filters.Status ?? ""}
            onChange={(e) => dg.setFilter("Status", e.target.value)}
          >
            <option value="">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Posted">Posted</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        }
      />

      <Modal title={editingId ? "Edit Journal Entry" : "New Journal Entry"} open={modal} onClose={closeModal} size="5xl">
        <div className="space-y-4">
          {saveError && (
            <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {saveError}
            </div>
          )}

          {/* Header row */}
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Entry Date *</label>
              <DateInput className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.entryDate} onChange={(e) => setForm((f) => ({ ...f, entryDate: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Reference Number</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.referenceNumber} onChange={(e) => setForm((f) => ({ ...f, referenceNumber: e.target.value }))} />
            </div>
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Description *</label>
              <input className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500" value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
            </div>
          </div>

          {/* Journal lines */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <p className="text-xs font-medium text-gray-600">Journal Lines</p>
              <button onClick={addLine} className="flex items-center gap-1 text-xs text-primary-600 hover:text-primary-700 font-medium">
                <Plus size={12} /> Add Line
              </button>
            </div>

            <div className="rounded-lg border border-gray-200">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 border-b border-gray-200">
                  <tr>
                    <th className="w-10 px-3 py-2.5 text-center text-xs font-medium text-gray-500">#</th>
                    <th className="w-[30%] px-3 py-2.5 text-left text-xs font-medium text-gray-500">Account</th>
                    <th className="px-3 py-2.5 text-left text-xs font-medium text-gray-500">Narration</th>
                    <th className="w-44 px-3 py-2.5 text-left text-xs font-medium text-gray-500">Cost Center</th>
                    <th className="w-36 px-3 py-2.5 text-right text-xs font-medium text-gray-500">Debit</th>
                    <th className="w-36 px-3 py-2.5 text-right text-xs font-medium text-gray-500">Credit</th>
                    <th className="w-8 px-2 py-2.5"></th>
                  </tr>
                </thead>
                <tbody>
                  {form.lines.map((line, idx) => (
                    <tr key={idx} className="border-t border-gray-100 hover:bg-gray-50/50">
                      <td className="px-3 py-2 text-center text-xs text-gray-400 font-medium">{idx + 1}</td>
                      <td className="px-2 py-2">
                        <SearchableDropdown
                          options={coaOptions}
                          value={line.accountId}
                          onChange={(v) => updateLine(idx, "accountId", v ?? "")}
                          placeholder="Select account"
                          searchPlaceholder="Search by code or name..."
                        />
                      </td>
                      <td className="px-2 py-2">
                        <input
                          className="w-full border border-gray-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
                          value={line.description ?? ""}
                          onChange={(e) => updateLine(idx, "description", e.target.value)}
                          placeholder="Narration…"
                        />
                      </td>
                      <td className="px-2 py-2">
                        <SearchableDropdown
                          options={costCenterOptions}
                          value={line.costCenterId ?? ""}
                          onChange={(v) => updateLine(idx, "costCenterId", v ?? "")}
                          placeholder="None"
                          searchPlaceholder="Search cost center…"
                        />
                      </td>
                      <td className="px-2 py-2">
                        <input
                          type="number"
                          min="0"
                          className="w-full border border-gray-200 rounded-lg px-2.5 py-1.5 text-sm text-right tabular-nums focus:outline-none focus:ring-1 focus:ring-primary-500"
                          value={line.debitAmount || ""}
                          onChange={(e) => updateLine(idx, "debitAmount", parseFloat(e.target.value) || 0)}
                          placeholder="0.00"
                        />
                      </td>
                      <td className="px-2 py-2">
                        <input
                          type="number"
                          min="0"
                          className="w-full border border-gray-200 rounded-lg px-2.5 py-1.5 text-sm text-right tabular-nums focus:outline-none focus:ring-1 focus:ring-primary-500"
                          value={line.creditAmount || ""}
                          onChange={(e) => updateLine(idx, "creditAmount", parseFloat(e.target.value) || 0)}
                          placeholder="0.00"
                        />
                      </td>
                      <td className="px-2 py-2 text-center">
                        {form.lines.length > 2 && (
                          <button
                            onClick={() => removeLine(idx)}
                            className="w-6 h-6 flex items-center justify-center text-gray-300 hover:text-red-500 hover:bg-red-50 rounded text-lg leading-none"
                          >
                            ×
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50 border-t-2 border-gray-200">
                  <tr>
                    <td colSpan={4} className="px-3 py-2.5 text-xs font-semibold text-gray-600">Totals</td>
                    <td className="px-2 py-2.5 text-sm font-semibold text-right tabular-nums text-gray-800">
                      {totalDebit.toLocaleString("en-BD", { minimumFractionDigits: 2 })}
                    </td>
                    <td className="px-2 py-2.5 text-sm font-semibold text-right tabular-nums text-gray-800">
                      {totalCredit.toLocaleString("en-BD", { minimumFractionDigits: 2 })}
                    </td>
                    <td />
                  </tr>
                </tfoot>
              </table>
            </div>

            {/* Balance status badge */}
            <div className="mt-2 flex justify-end">
              {(totalDebit > 0 || totalCredit > 0) && (
                isBalanced ? (
                  <span className="inline-flex items-center gap-1.5 text-xs font-medium text-green-700 bg-green-50 border border-green-200 px-3 py-1 rounded-full">
                    <Check size={11} /> Balanced
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1.5 text-xs font-medium text-red-600 bg-red-50 border border-red-200 px-3 py-1 rounded-full">
                    <AlertCircle size={11} /> Difference: {Math.abs(totalDebit - totalCredit).toLocaleString("en-BD", { minimumFractionDigits: 2 })}
                  </span>
                )
              )}
            </div>
          </div>

          {/* Footer */}
          <div className="flex justify-end gap-2 pt-3 border-t border-gray-100">
            <button onClick={closeModal} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
            <button
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending || !isBalanced}
              className="px-4 py-2 text-sm bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:opacity-50"
            >
              {saveMutation.isPending ? "Saving…" : editingId ? "Update Draft" : "Save Draft"}
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
