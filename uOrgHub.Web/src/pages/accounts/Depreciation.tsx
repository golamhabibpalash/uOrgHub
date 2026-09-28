import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Eye, Send, RotateCcw, ChevronDown, ChevronUp } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import {
  getDepreciationRuns,
  previewDepreciation,
  postDepreciationRun,
  reverseDepreciationRun,
  DepreciationLine,
  DepreciationPreview,
  DepreciationRun,
  DepreciationRunStatus,
} from "../../api/accounts";

const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
const selectClass = "text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary-500";

const statusColors: Record<DepreciationRunStatus, string> = {
  Posted: "bg-green-50 text-green-700",
  Reversed: "bg-gray-100 text-gray-500",
};

function errorMessage(err: unknown, fallback: string) {
  const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
  return axiosErr?.response?.data?.message ?? axiosErr?.response?.data?.errors?.[0] ?? fallback;
}

function LinesTable({ lines }: { lines: DepreciationLine[] }) {
  return (
    <table className="w-full text-xs">
      <thead>
        <tr className="text-gray-500 border-b border-gray-100">
          <th className="text-left py-1.5">Asset</th>
          <th className="text-left py-1.5">Category</th>
          <th className="text-right py-1.5">Months</th>
          <th className="text-right py-1.5">Cost</th>
          <th className="text-right py-1.5">Acc. Before</th>
          <th className="text-right py-1.5">Charge</th>
          <th className="text-right py-1.5">Book Value After</th>
        </tr>
      </thead>
      <tbody>
        {lines.map((l) => (
          <tr key={l.fixedAssetId} className="border-b border-gray-50">
            <td className="py-1"><span className="font-mono text-gray-500">{l.assetCode}</span> {l.assetName}</td>
            <td className="py-1 text-gray-500">{l.categoryName}</td>
            <td className="py-1 text-right" title={l.months > 1 ? "Catching up months that were not run earlier" : undefined}>
              {l.months > 1 ? <span className="text-amber-600">{l.months}</span> : l.months}
            </td>
            <td className="py-1 text-right tabular-nums">{fmt(l.purchaseCost)}</td>
            <td className="py-1 text-right tabular-nums text-gray-500">{fmt(l.accumulatedBefore)}</td>
            <td className="py-1 text-right tabular-nums font-medium">{fmt(l.amount)}</td>
            <td className="py-1 text-right tabular-nums">{fmt(l.bookValueAfter)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default function Depreciation() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "periodEndDate", defaultSortDescending: true });
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [notes, setNotes] = useState("");
  const [preview, setPreview] = useState<DepreciationPreview | null>(null);
  const [error, setError] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["depreciation-runs", ...dg.queryKey],
    queryFn: () => getDepreciationRuns(dg.queryParams),
  });
  const runs = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  // Runs unwind newest-first (the server enforces it); only the latest posted one offers Reverse.
  const latestPostedId = [...runs]
    .filter((r) => r.status === "Posted")
    .sort((a, b) => b.periodEndDate.localeCompare(a.periodEndDate) || b.createdAt.localeCompare(a.createdAt))[0]?.id;

  const previewMutation = useMutation({
    mutationFn: () => previewDepreciation(year, month),
    onMutate: () => { setError(""); setPreview(null); },
    onSuccess: (res) => setPreview(res.data.data ?? null),
    onError: (err) => setError(errorMessage(err, "Failed to preview depreciation.")),
  });

  const postMutation = useMutation({
    mutationFn: () => postDepreciationRun({ year, month, notes: notes || undefined }),
    onSuccess: () => {
      setPreview(null);
      setNotes("");
      qc.invalidateQueries({ queryKey: ["depreciation-runs"] });
      qc.invalidateQueries({ queryKey: ["fixed-assets"] });
      qc.invalidateQueries({ queryKey: ["fixed-assets-summary"] });
      qc.invalidateQueries({ queryKey: ["journal-entries"] });
    },
    onError: (err) => setError(errorMessage(err, "Failed to post depreciation.")),
  });

  const reverseMutation = useMutation({
    mutationFn: (id: string) => reverseDepreciationRun(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["depreciation-runs"] });
      qc.invalidateQueries({ queryKey: ["fixed-assets"] });
      qc.invalidateQueries({ queryKey: ["fixed-assets-summary"] });
      qc.invalidateQueries({ queryKey: ["journal-entries"] });
    },
    onError: (err) => setError(errorMessage(err, "Failed to reverse depreciation run.")),
  });

  function changePeriod(nextYear: number, nextMonth: number) {
    setYear(nextYear);
    setMonth(nextMonth);
    setPreview(null);
    setError("");
  }

  function confirmPost() {
    if (!preview) return;
    const label = `${MONTHS[month - 1]} ${year}`;
    if (window.confirm(`Post depreciation of ${fmt(preview.totalAmount)} for ${label} across ${preview.lines.length} asset(s)? This creates a posted journal entry.`))
      postMutation.mutate();
  }

  function confirmReverse(run: DepreciationRun) {
    if (window.confirm(`Reverse ${run.runNumber}? Its journal entry will be cancelled and each asset's accumulated depreciation restored.`))
      reverseMutation.mutate(run.id);
  }

  const years = Array.from({ length: 8 }, (_, i) => now.getFullYear() - 6 + i).filter((y) => y <= now.getFullYear());

  const columns: DataGridColumn<DepreciationRun>[] = [
    { key: "runNumber", label: "Run #", className: "font-mono text-xs text-gray-500" },
    { key: "periodEndDate", label: "Period", render: (row) => `${MONTHS[row.periodMonth - 1]} ${row.periodYear}` },
    { key: "assetCount", label: "Assets", sortable: false },
    { key: "totalAmount", label: "Depreciation", headerClassName: "text-right", className: "text-right tabular-nums font-medium", render: (row) => fmt(row.totalAmount) },
    { key: "journalEntryEntryNumber", label: "Journal Entry", sortable: false, className: "font-mono text-xs" },
    {
      key: "status",
      label: "Status",
      sortable: false,
      render: (row) => (
        <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[row.status]}`} title={row.reversedBy ? `Reversed by ${row.reversedBy}` : undefined}>
          {row.status}
        </span>
      ),
    },
    { key: "createdBy", label: "Posted By", sortable: false, render: (row) => <span className="text-xs text-gray-500">{row.createdBy} · {row.createdAt.split("T")[0]}</span> },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row) => (
        <div className="flex items-center gap-2">
          <button onClick={() => setExpandedId(expandedId === row.id ? null : row.id)} className="text-gray-400 hover:text-primary-600" title="View assets charged">
            {expandedId === row.id ? <ChevronUp size={15} /> : <ChevronDown size={15} />}
          </button>
          {row.id === latestPostedId && (
            <button onClick={() => confirmReverse(row)} disabled={reverseMutation.isPending} className="text-red-400 hover:text-red-600 disabled:opacity-50" title="Reverse this run">
              <RotateCcw size={14} />
            </button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="mb-4">
        <h2 className="text-base font-medium text-gray-900">Depreciation</h2>
        <p className="text-xs text-gray-400">Run monthly depreciation for every fixed asset. Each run posts one journal entry: Dr Depreciation Expense / Cr Accumulated Depreciation.</p>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-4 mb-4 space-y-3">
        <div className="flex flex-wrap items-end gap-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Month</label>
            <select className={selectClass} value={month} onChange={(e) => changePeriod(year, Number(e.target.value))}>
              {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
            </select>
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Year</label>
            <select className={selectClass} value={year} onChange={(e) => changePeriod(Number(e.target.value), month)}>
              {years.map((y) => <option key={y} value={y}>{y}</option>)}
            </select>
          </div>
          <button
            onClick={() => previewMutation.mutate()}
            disabled={previewMutation.isPending}
            className="flex items-center gap-2 text-sm border border-gray-200 rounded-lg px-4 py-2 hover:bg-gray-50 disabled:opacity-50"
          >
            <Eye size={14} /> {previewMutation.isPending ? "Calculating..." : "Preview"}
          </button>
          {preview && preview.lines.length > 0 && (
            <>
              <input
                className="text-sm border border-gray-200 rounded-lg px-3 py-2 flex-1 min-w-[200px] focus:outline-none focus:ring-1 focus:ring-primary-500"
                placeholder="Notes (optional)"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
              />
              <button
                onClick={confirmPost}
                disabled={postMutation.isPending}
                className="flex items-center gap-2 bg-primary-500 text-white text-sm px-4 py-2 rounded-lg hover:bg-primary-600 disabled:opacity-50"
              >
                <Send size={14} /> {postMutation.isPending ? "Posting..." : `Post ${fmt(preview.totalAmount)}`}
              </button>
            </>
          )}
        </div>

        {error && <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</div>}

        {preview && (
          preview.lines.length === 0 ? (
            <p className="text-sm text-gray-500">No depreciation is due for {MONTHS[month - 1]} {year}.</p>
          ) : (
            <div className="border border-gray-100 rounded-lg p-3">
              <div className="flex items-center justify-between mb-2">
                <p className="text-xs font-medium text-gray-700">Preview — {MONTHS[month - 1]} {year} · {preview.lines.length} asset(s)</p>
                <p className="text-sm font-medium tabular-nums">{fmt(preview.totalAmount)}</p>
              </div>
              <LinesTable lines={preview.lines} />
            </div>
          )
        )}
      </div>

      <DataGrid
        columns={columns}
        data={runs}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search runs..."
        page={dg.page}
        totalPages={totalPages}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={totalCount}
        emptyMessage="No depreciation has been posted yet"
        renderExpandedRow={(row) => <LinesTable lines={row.lines} />}
        expandedRowId={expandedId}
        actions={<ExportMenu baseUrl="/accounts/depreciation-runs" />}
      />
    </div>
  );
}
