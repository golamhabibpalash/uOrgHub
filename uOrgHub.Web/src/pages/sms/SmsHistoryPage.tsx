import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import DataGrid, { type DataGridColumn } from "../../components/shared/DataGrid";
import DateInput from "../../components/shared/DateInput";
import Modal from "../../components/shared/Modal";
import { useDataGrid } from "../../hooks/useDataGrid";
import { useAuthStore } from "../../store/authStore";
import {
  getSmsGateways, getSmsMessage, getSmsMessages,
  type SmsHistoryFilters, type SmsMessage, type SmsMessageStatus,
} from "../../api/sms";
import { formatPhone } from "../../utils/smsSegments";
import SmsStatusBadge from "./SmsStatusBadge";
import { SMS_STATUSES, formatDateTime, formatMoney, localDayToUtc, smsStatusLabel } from "./smsFormat";

const selectClass = "text-sm border border-gray-200 rounded-lg px-2 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500";

export default function SmsHistoryPage() {
  const { hasClaim, hasRole } = useAuthStore();
  const canSeeDetails = hasRole("Admin") || hasClaim("Sms.History.ViewDetails");
  const canSeeGateways = hasRole("Admin") || hasClaim("Sms.Gateways.View");

  const dg = useDataGrid({ defaultSortBy: "createdAt", defaultSortDescending: true, defaultPageSize: 25 });
  const [fromDay, setFromDay] = useState("");
  const [toDay, setToDay] = useState("");
  const [gatewayId, setGatewayId] = useState("");
  const [status, setStatus] = useState<SmsMessageStatus | "">("");
  const [detailId, setDetailId] = useState<string | null>(null);

  const filters: SmsHistoryFilters = {
    from: fromDay ? localDayToUtc(fromDay) : undefined,
    to: toDay ? localDayToUtc(toDay, true) : undefined,
    gatewayId: gatewayId || undefined,
    status: status || undefined,
  };
  const invalidRange = !!fromDay && !!toDay && fromDay > toDay;

  const { data, isLoading } = useQuery({
    queryKey: ["sms-messages", ...dg.queryKey, filters.from, filters.to, filters.gatewayId, filters.status],
    queryFn: () => getSmsMessages(dg.queryParams, filters),
    enabled: !invalidRange,
  });
  const gateways = useQuery({ queryKey: ["sms-gateways"], queryFn: getSmsGateways, enabled: canSeeGateways });

  const page = data?.data?.data;
  const items = page?.items ?? [];

  const resetPage = () => dg.setPage(1);
  const clearFilters = () => { setFromDay(""); setToDay(""); setGatewayId(""); setStatus(""); resetPage(); };
  const filtersActive = !!(fromDay || toDay || gatewayId || status);

  const columns: DataGridColumn<SmsMessage>[] = [
    { key: "createdAt", label: "Sent", width: "150px", render: (r) => <span className="text-xs whitespace-nowrap">{formatDateTime(r.submittedAt ?? r.createdAt)}</span> },
    { key: "recipient", label: "Recipient", render: (r) => <span className="font-mono text-xs whitespace-nowrap">{formatPhone(r.recipient)}</span> },
    { key: "messagePreview", label: "Message", render: (r) => <span className="text-sm line-clamp-2 max-w-xs" title={r.messagePreview}>{r.messagePreview}</span> },
    { key: "gatewayName", label: "Gateway", render: (r) => <span className="text-xs">{r.gatewayName}</span> },
    { key: "segments", label: "SMS", className: "text-right tabular-nums", headerClassName: "text-right" },
    {
      key: "status", label: "Status",
      render: (r) => (
        <div>
          <SmsStatusBadge status={r.status} />
          {r.errorMessage && (r.status === "Failed" || r.status === "Undetermined") && (
            <p className="text-[11px] text-red-600 mt-0.5 line-clamp-1 max-w-[220px]" title={r.errorMessage}>{r.errorMessage}</p>
          )}
        </div>
      ),
    },
    {
      key: "cost", label: "Cost", className: "text-right tabular-nums whitespace-nowrap", headerClassName: "text-right",
      render: (r) => <span title={r.costIsActual ? "Charged by the provider" : "Estimated from the rate"}>{r.cost == null ? "—" : `${r.cost.toFixed(2)}${r.costIsActual ? "" : "*"}`}</span>,
    },
    { key: "providerMessageId", label: "Reference", render: (r) => <span className="font-mono text-xs text-gray-500">{r.providerMessageId ?? "—"}</span> },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">SMS History</h2>
          <p className="text-xs text-gray-500 mt-0.5">Every message sent through this company's gateways. * cost estimated from the gateway rate.</p>
        </div>
      </div>

      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        sortBy={dg.sortBy}
        sortDescending={dg.sortDescending}
        onSort={dg.handleSort}
        search={dg.search}
        onSearch={dg.setSearch}
        searchPlaceholder="Search number, message or reference…"
        page={dg.page}
        totalPages={page?.totalPages ?? 1}
        onPageChange={dg.setPage}
        pageSize={dg.pageSize}
        onPageSizeChange={dg.setPageSize}
        totalCount={page?.totalCount ?? 0}
        onView={canSeeDetails ? (r) => setDetailId(r.id) : undefined}
        emptyMessage={filtersActive || dg.search ? "No messages match these filters" : "No SMS sent yet"}
        filterBar={
          <div className="flex flex-wrap items-end gap-2">
            <div>
              <label className="text-xs text-gray-500 block mb-1">From</label>
              <DateInput className="border border-gray-200 rounded-lg px-2 py-1.5 text-sm" value={fromDay} onChange={(e) => { setFromDay(e.target.value); resetPage(); }} />
            </div>
            <div>
              <label className="text-xs text-gray-500 block mb-1">To</label>
              <DateInput className="border border-gray-200 rounded-lg px-2 py-1.5 text-sm" value={toDay} onChange={(e) => { setToDay(e.target.value); resetPage(); }} />
            </div>
            {canSeeGateways && (
              <div>
                <label className="text-xs text-gray-500 block mb-1">Gateway</label>
                <select className={selectClass} value={gatewayId} onChange={(e) => { setGatewayId(e.target.value); resetPage(); }}>
                  <option value="">All gateways</option>
                  {(gateways.data?.data?.data ?? []).map((g) => <option key={g.id} value={g.id}>{g.name}</option>)}
                </select>
              </div>
            )}
            <div>
              <label className="text-xs text-gray-500 block mb-1">Status</label>
              <select className={selectClass} value={status} onChange={(e) => { setStatus(e.target.value as SmsMessageStatus | ""); resetPage(); }}>
                <option value="">All statuses</option>
                {SMS_STATUSES.map((s) => <option key={s} value={s}>{smsStatusLabel(s)}</option>)}
              </select>
            </div>
            {filtersActive && (
              <button onClick={clearFilters} className="px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600">Clear filters</button>
            )}
            {invalidRange && <p className="text-xs text-red-600 self-center">"From" is after "To".</p>}
          </div>
        }
      />

      <SmsMessageDetailModal id={detailId} onClose={() => setDetailId(null)} />
    </div>
  );
}

function SmsMessageDetailModal({ id, onClose }: { id: string | null; onClose: () => void }) {
  const { data, isLoading } = useQuery({
    queryKey: ["sms-message", id],
    queryFn: () => getSmsMessage(id!),
    enabled: !!id,
  });
  const m = data?.data?.data;

  return (
    <Modal title="SMS details" open={!!id} onClose={onClose} size="2xl">
      {isLoading || !m ? (
        <p className="text-sm text-gray-500 py-6 text-center">Loading…</p>
      ) : (
        <div className="space-y-4 text-sm">
          <div className="flex items-center justify-between">
            <span className="font-mono">{formatPhone(m.recipient)}</span>
            <SmsStatusBadge status={m.status} />
          </div>
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-3 whitespace-pre-wrap break-words">{m.messageText}</div>
          <dl className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-2">
            <Field label="Gateway" value={`${m.gatewayName} (${m.providerKey})`} />
            <Field label="Reference / transaction ID" value={m.providerMessageId ?? "—"} mono />
            <Field label="Encoding · SMS parts" value={`${m.encoding === "Gsm7" ? "GSM-7" : "Unicode"} · ${m.segments}`} />
            <Field label="Cost" value={`${formatMoney(m.cost)}${m.cost != null ? (m.costIsActual ? " (charged)" : " (estimated)") : ""}`} />
            <Field label="Queued" value={formatDateTime(m.createdAt)} />
            <Field label="Submitted" value={formatDateTime(m.submittedAt)} />
            <Field label="Delivered" value={formatDateTime(m.deliveredAt)} />
            <Field label="Failed" value={formatDateTime(m.failedAt)} />
            <Field label="Provider status" value={m.providerStatus ?? "—"} />
            <Field label="Attempts" value={String(m.attempts)} />
            <Field label="Sent by" value={m.createdBy || "—"} />
            <Field label="Source" value={m.source ? `${m.source}${m.sourceReference ? ` · ${m.sourceReference}` : ""}` : "—"} />
          </dl>
          {(m.errorMessage || m.errorCode) && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-red-800">
              <p className="font-medium">Error {m.errorCode ? `(${m.errorCode})` : ""}</p>
              <p className="mt-0.5">{m.errorMessage}</p>
            </div>
          )}
          {m.providerResponse && (
            <div>
              <p className="text-xs text-gray-500 mb-1">Gateway response</p>
              <pre className="bg-gray-50 border border-gray-200 rounded-lg p-3 text-xs overflow-x-auto whitespace-pre-wrap break-all">{m.providerResponse}</pre>
            </div>
          )}
        </div>
      )}
    </Modal>
  );
}

function Field({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div>
      <dt className="text-xs text-gray-500">{label}</dt>
      <dd className={`text-gray-900 ${mono ? "font-mono text-xs" : ""}`}>{value}</dd>
    </div>
  );
}
