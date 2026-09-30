import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Eye, Send, RotateCcw, ChevronDown, ChevronUp, AlertTriangle } from "lucide-react";
import DataGrid, { DataGridColumn } from "../../components/shared/DataGrid";
import { useDataGrid } from "../../hooks/useDataGrid";
import ExportMenu from "../../components/shared/ExportMenu";
import DateInput from "../../components/shared/DateInput";
import {
  getHireChargeRuns,
  previewHireCharges,
  postHireChargeRun,
  reverseHireChargeRun,
  HireChargeLine,
  HireChargePreview,
  HireChargeRun,
  HireChargeRunStatus,
  hireRateUnitLabels,
} from "../../api/accounts";

const fmt = (v: number) => v.toLocaleString("en-BD", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const day = (iso?: string) => iso?.split("T")[0] ?? "";
const inputClass = "text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary-500";

const statusColors: Record<HireChargeRunStatus, string> = {
  Posted: "bg-green-50 text-green-700",
  Reversed: "bg-gray-100 text-gray-500",
};

function errorMessage(err: unknown, fallback: string) {
  const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
  return axiosErr?.response?.data?.message ?? axiosErr?.response?.data?.errors?.[0] ?? fallback;
}

function LinesTable({ lines }: { lines: HireChargeLine[] }) {
  return (
    <table className="w-full text-xs">
      <thead>
        <tr className="text-gray-500 border-b border-gray-100">
          <th className="text-left py-1.5">Asset</th>
          <th className="text-left py-1.5">Project</th>
          <th className="text-left py-1.5">Days on site</th>
          <th className="text-right py-1.5">Rate</th>
          <th className="text-right py-1.5">Charge</th>
        </tr>
      </thead>
      <tbody>
        {lines.map((l) => (
          <tr key={l.assetDeploymentId} className="border-b border-gray-50">
            <td className="py-1"><span className="font-mono text-gray-500">{l.assetCode}</span> {l.assetName}</td>
            <td className="py-1">{l.projectName}</td>
            <td className="py-1 text-gray-600">{day(l.fromDate)} → {day(l.toDate)} · <strong>{l.days}</strong> day{l.days === 1 ? "" : "s"}</td>
            <td className="py-1 text-right tabular-nums">{fmt(l.rate)} {hireRateUnitLabels[l.rateUnit]}</td>
            <td className="py-1 text-right tabular-nums font-medium">{fmt(l.amount)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default function HireCharges() {
  const qc = useQueryClient();
  const dg = useDataGrid({ defaultSortBy: "fromDate", defaultSortDescending: true });
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState(new Date().toISOString().split("T")[0]);
  const [notes, setNotes] = useState("");
  const [preview, setPreview] = useState<HireChargePreview | null>(null);
  const [error, setError] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["hire-charge-runs", ...dg.queryKey],
    queryFn: () => getHireChargeRuns(dg.queryParams),
  });
  const runs = data?.data?.data?.items ?? [];
  const totalPages = data?.data?.data?.totalPages ?? 1;
  const totalCount = data?.data?.data?.totalCount ?? 0;

  // Posted ranges can't overlap, so the natural next range starts the day after the latest one.
  const lastPostedTo = runs.filter((r) => r.status === "Posted").map((r) => day(r.toDate)).sort().at(-1);
  const suggestedFrom = lastPostedTo
    ? new Date(new Date(lastPostedTo).getTime() + 86_400_000).toISOString().split("T")[0]
    : "";

  function invalidate() {
    qc.invalidateQueries({ queryKey: ["hire-charge-runs"] });
    qc.invalidateQueries({ queryKey: ["asset-deployments"] });
    qc.invalidateQueries({ queryKey: ["journal-entries"] });
  }

  const previewMutation = useMutation({
    mutationFn: () => previewHireCharges(fromDate, toDate),
    onMutate: () => { setError(""); setPreview(null); },
    onSuccess: (res) => setPreview(res.data.data ?? null),
    onError: (err) => setError(errorMessage(err, "Failed to preview hire charges.")),
  });

  const postMutation = useMutation({
    mutationFn: () => postHireChargeRun({ fromDate, toDate, notes: notes || undefined }),
    onSuccess: () => { setPreview(null); setNotes(""); invalidate(); },
    onError: (err) => setError(errorMessage(err, "Failed to post hire charges.")),
  });

  const reverseMutation = useMutation({
    mutationFn: (id: string) => reverseHireChargeRun(id),
    onSuccess: invalidate,
    onError: (err) => setError(errorMessage(err, "Failed to reverse hire charge run.")),
  });

  function changeRange(from: string, to: string) {
    setFromDate(from);
    setToDate(to);
    setPreview(null);
    setError("");
  }

  function confirmPost() {
    if (!preview) return;
    if (window.confirm(`Post equipment hire of ${fmt(preview.totalAmount)} for ${fromDate} to ${toDate} across ${preview.lines.length} deployment(s)? Each project's cost center is charged.`))
      postMutation.mutate();
  }

  function confirmReverse(run: HireChargeRun) {
    if (window.confirm(`Reverse ${run.runNumber}? Its journal entry is cancelled and ${day(run.fromDate)} to ${day(run.toDate)} can be charged again.`))
      reverseMutation.mutate(run.id);
  }

  const columns: DataGridColumn<HireChargeRun>[] = [
    { key: "runNumber", label: "Run #", className: "font-mono text-xs text-gray-500" },
    { key: "fromDate", label: "Range", render: (row) => `${day(row.fromDate)} → ${day(row.toDate)}` },
    { key: "deploymentCount", label: "Deployments", sortable: false },
    { key: "totalAmount", label: "Hire", headerClassName: "text-right", className: "text-right tabular-nums font-medium", render: (row) => fmt(row.totalAmount) },
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
    { key: "createdBy", label: "Posted By", sortable: false, render: (row) => <span className="text-xs text-gray-500">{row.createdBy} · {day(row.createdAt)}</span> },
    {
      key: "actions",
      label: "Actions",
      sortable: false,
      render: (row) => (
        <div className="flex items-center gap-2">
          <button onClick={() => setExpandedId(expandedId === row.id ? null : row.id)} className="text-gray-400 hover:text-primary-600" title="View charges">
            {expandedId === row.id ? <ChevronUp size={15} /> : <ChevronDown size={15} />}
          </button>
          {row.status === "Posted" && (
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
        <h2 className="text-base font-medium text-gray-900">Equipment Hire</h2>
        <p className="text-xs text-gray-400">
          Charge projects for company machinery on a hire rate, for any date range. Each run posts Dr Equipment Hire (project cost center) / Cr Internal Equipment Recovery.
        </p>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-4 mb-4 space-y-3">
        <div className="flex flex-wrap items-end gap-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">From</label>
            <DateInput className={inputClass} value={fromDate} onChange={(e) => changeRange(e.target.value, toDate)} />
            {suggestedFrom && fromDate !== suggestedFrom && (
              <button type="button" onClick={() => changeRange(suggestedFrom, toDate)} className="text-[11px] text-primary-600 hover:underline mt-0.5">
                Continue from {suggestedFrom}
              </button>
            )}
          </div>
          <div>
            <label className="text-xs text-gray-500 mb-1 block">To</label>
            <DateInput className={inputClass} value={toDate} onChange={(e) => changeRange(fromDate, e.target.value)} />
          </div>
          <button
            onClick={() => previewMutation.mutate()}
            disabled={!fromDate || !toDate || previewMutation.isPending}
            className="flex items-center gap-2 text-sm border border-gray-200 rounded-lg px-4 py-2 hover:bg-gray-50 disabled:opacity-50"
          >
            <Eye size={14} /> {previewMutation.isPending ? "Calculating..." : "Preview"}
          </button>
          {preview && preview.lines.length > 0 && (
            <>
              <input className={`${inputClass} flex-1 min-w-[200px]`} placeholder="Notes (optional)" value={notes} onChange={(e) => setNotes(e.target.value)} />
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
            <p className="text-sm text-gray-500">No machine was on a project on a hire rate between {fromDate} and {toDate}.</p>
          ) : (
            <div className="border border-gray-100 rounded-lg p-3 space-y-2">
              {preview.warning && (
                <div className="flex items-start gap-2 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
                  <AlertTriangle size={14} className="mt-0.5 shrink-0" /> {preview.warning}
                </div>
              )}
              <div className="flex items-center justify-between">
                <p className="text-xs font-medium text-gray-700">Preview — {fromDate} to {toDate} · {preview.lines.length} deployment(s)</p>
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
        emptyMessage="No equipment hire has been charged yet"
        renderExpandedRow={(row) => <LinesTable lines={row.lines} />}
        expandedRowId={expandedId}
        actions={<ExportMenu baseUrl="/accounts/hire-charge-runs" />}
      />
    </div>
  );
}
