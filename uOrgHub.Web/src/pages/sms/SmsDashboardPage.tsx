import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { AlertTriangle, RefreshCw, Send, Settings } from "lucide-react";
import {
  Bar, BarChart, CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis,
} from "recharts";
import {
  getSmsAnalytics, getSmsDashboard, syncSmsBalance,
  type SmsAnalyticsPoint, type SmsGranularity, type SmsTotals,
} from "../../api/sms";
import DateInput from "../../components/shared/DateInput";
import { useAuthStore } from "../../store/authStore";
import { formatCount, formatDateTime, formatMoney, localDate } from "./smsFormat";

/**
 * Chart colours: the reference palette's first three categorical slots, checked with the dataviz
 * validator against the white card (CVD-safe, adjacent ΔE ≥ 9). "Other" sits under 3:1
 * contrast, so the chart always has a legend, a tooltip and a table view.
 */
const SERIES = {
  successful: { label: "Successful", color: "#2a78d6" },
  failed: { label: "Failed", color: "#eb6834" },
  other: { label: "Pending / unknown", color: "#1baf7a" },
} as const;
const INK = { muted: "#6b7280", grid: "#e5e7eb", axis: "#d1d5db" };

type Preset = "7" | "30" | "90" | "custom";

function rangeFor(preset: Exclude<Preset, "custom">) {
  const to = new Date();
  const from = new Date();
  from.setDate(to.getDate() - (Number(preset) - 1));
  return { from: localDate(from), to: localDate(to) };
}

export default function SmsDashboardPage() {
  const { hasClaim, hasRole } = useAuthStore();
  const can = (c: string) => hasRole("Admin") || hasClaim(c);
  const qc = useQueryClient();

  const dashboard = useQuery({ queryKey: ["sms-dashboard"], queryFn: getSmsDashboard });
  const d = dashboard.data?.data?.data;
  const gateway = d?.activeGateway;

  // Pull a fresh balance on open (the server throttles to one vendor call every couple of minutes).
  const sync = useMutation({
    mutationFn: (force: boolean) => syncSmsBalance(force),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["sms-dashboard"] }),
  });
  const gatewayId = gateway?.id;
  const { mutate: syncNow } = sync;
  useEffect(() => {
    if (gatewayId) syncNow(false);
  }, [gatewayId, syncNow]);

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-base font-medium text-gray-900">SMS Dashboard</h2>
        <div className="flex gap-2">
          {can("Sms.Messages.Send") && (
            <Link to="/sms/compose" className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm rounded-lg bg-primary-600 text-white hover:bg-primary-700">
              <Send size={14} /> Compose
            </Link>
          )}
          {can("Sms.Gateways.View") && (
            <Link to="/sms/gateways" className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">
              <Settings size={14} /> Gateways
            </Link>
          )}
        </div>
      </div>

      {/* Active gateway */}
      <div className="bg-white border border-gray-200 rounded-xl p-4">
        {dashboard.isLoading ? (
          <p className="text-sm text-gray-500">Loading…</p>
        ) : !gateway ? (
          <div className="flex items-start gap-2 text-sm">
            <AlertTriangle size={16} className="text-amber-600 mt-0.5" />
            <div>
              <p className="font-medium text-gray-900">No active SMS gateway</p>
              <p className="text-gray-500">{d?.gatewayCount ? "Make one of the configured gateways active to start sending." : "Add a gateway to start sending SMS."}</p>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4 items-start">
            <div className="col-span-2 md:col-span-1">
              <p className="text-xs text-gray-500">Active gateway</p>
              <p className="text-sm font-medium text-gray-900 mt-0.5">{gateway.name}</p>
              <p className="text-xs text-gray-500">{gateway.providerName}{gateway.senderId ? ` · ${gateway.senderId}` : ""}</p>
            </div>
            <Metric
              label="Balance"
              value={gateway.capabilities?.balance === false ? "Not reported" : gateway.lastBalance != null ? formatMoney(gateway.lastBalance, gateway.balanceUnit ?? gateway.currency) : "—"}
              hint={gateway.lastBalanceError ?? undefined}
              hintTone="error"
            />
            <Metric label="Rate per SMS" value={gateway.effectiveRate != null ? formatMoney(gateway.effectiveRate, gateway.currency) : "Not set"}
              hint={gateway.lastProviderRate != null ? "From the provider" : gateway.ratePerSegment != null ? "Set on the gateway" : undefined} />
            <Metric label="Last balance sync" value={formatDateTime(gateway.lastBalanceSyncedAt)} />
            <div className="flex md:justify-end">
              {gateway.capabilities?.balance !== false && (
                <button
                  onClick={() => sync.mutate(true)}
                  disabled={sync.isPending}
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs rounded-lg border border-gray-200 hover:bg-gray-50 disabled:opacity-50"
                >
                  <RefreshCw size={12} className={sync.isPending ? "animate-spin" : ""} /> Refresh balance
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Totals */}
      {d && (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
          <Tile label="Total sent" value={formatCount(d.allTime.sent)} sub={`${formatCount(d.today.sent)} today`} />
          <Tile label="Successful" value={formatCount(d.allTime.successful)} sub={rate(d.allTime)} />
          <Tile label="Failed" value={formatCount(d.allTime.failed)} sub={d.allTime.undetermined ? `${formatCount(d.allTime.undetermined)} unknown` : `${formatCount(d.today.failed)} today`} />
          <Tile label="SMS parts used" value={formatCount(d.allTime.segments)} sub={`${formatCount(d.last30Days.segments)} in 30 days`} />
          <Tile label="Total cost" value={formatMoney(d.allTime.cost, gateway?.currency)} sub={`${formatMoney(d.last30Days.cost, gateway?.currency)} in 30 days`} />
          <Tile label="Pending" value={formatCount(d.allTime.pending)} sub="Waiting to send" />
        </div>
      )}

      {can("Sms.Analytics.View") && <SmsAnalyticsPanel />}
    </div>
  );
}

const rate = (t: SmsTotals) => (t.sent ? `${Math.round((t.successful / t.sent) * 100)}% of sent` : "—");

function Metric({ label, value, hint, hintTone }: { label: string; value: string; hint?: string; hintTone?: "error" }) {
  return (
    <div>
      <p className="text-xs text-gray-500">{label}</p>
      <p className="text-sm font-medium text-gray-900 mt-0.5 tabular-nums">{value}</p>
      {hint && <p className={`text-xs mt-0.5 ${hintTone === "error" ? "text-red-600" : "text-gray-500"}`}>{hint}</p>}
    </div>
  );
}

function Tile({ label, value, sub }: { label: string; value: string; sub?: string }) {
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4">
      <p className="text-xs text-gray-500 mb-1">{label}</p>
      <p className="text-xl font-medium text-gray-900 tabular-nums">{value}</p>
      {sub && <p className="text-xs text-gray-500 mt-1">{sub}</p>}
    </div>
  );
}

function periodLabel(period: string, g: SmsGranularity) {
  const d = new Date(`${period}T00:00:00`);
  if (g === "Month") return d.toLocaleDateString("en-GB", { month: "short", year: "numeric" });
  return d.toLocaleDateString("en-GB", { day: "2-digit", month: "short" });
}

function SmsAnalyticsPanel() {
  const [preset, setPreset] = useState<Preset>("30");
  const [custom, setCustom] = useState(rangeFor("30"));
  const [granularity, setGranularity] = useState<SmsGranularity>("Day");
  const [showTable, setShowTable] = useState(false);

  const range = preset === "custom" ? custom : rangeFor(preset);
  const invalid = range.from > range.to;

  const analytics = useQuery({
    queryKey: ["sms-analytics", range.from, range.to, granularity],
    queryFn: () => getSmsAnalytics(range.from, range.to, granularity),
    enabled: !invalid,
  });
  const a = analytics.data?.data?.data;

  const rows = useMemo(
    () => (a?.points ?? []).map((p: SmsAnalyticsPoint) => ({
      ...p,
      label: periodLabel(p.period, granularity),
      other: p.sent - p.successful - p.failed + p.pending,
    })),
    [a, granularity],
  );
  const currency = a?.currency ?? "BDT";

  const btn = (active: boolean) =>
    `px-3 py-1.5 text-xs ${active ? "bg-gray-900 text-white" : "bg-white text-gray-600 hover:bg-gray-50"}`;

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4 space-y-4">
      {/* Filters: one row above both charts */}
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-gray-900">Usage</p>
          <p className="text-xs text-gray-500">From sent-message records{a ? ` · ${formatCount(a.totals.sent)} sent, ${formatCount(a.totals.segments)} SMS parts, ${formatMoney(a.totals.cost, currency)}` : ""}</p>
        </div>
        <div className="flex flex-wrap items-end gap-2">
          <div className="inline-flex rounded-lg border border-gray-200 overflow-hidden" role="group" aria-label="Date range">
            {(["7", "30", "90"] as const).map((p) => (
              <button key={p} className={btn(preset === p)} aria-pressed={preset === p} onClick={() => setPreset(p)}>{p} days</button>
            ))}
            <button className={btn(preset === "custom")} aria-pressed={preset === "custom"} onClick={() => setPreset("custom")}>Custom</button>
          </div>
          {preset === "custom" && (
            <>
              <DateInput className="border border-gray-200 rounded-lg px-2 py-1.5 text-sm" value={custom.from} onChange={(e) => setCustom((c) => ({ ...c, from: e.target.value }))} />
              <DateInput className="border border-gray-200 rounded-lg px-2 py-1.5 text-sm" value={custom.to} onChange={(e) => setCustom((c) => ({ ...c, to: e.target.value }))} />
            </>
          )}
          <div className="inline-flex rounded-lg border border-gray-200 overflow-hidden" role="group" aria-label="Group by">
            {(["Day", "Week", "Month"] as const).map((g) => (
              <button key={g} className={btn(granularity === g)} aria-pressed={granularity === g} onClick={() => setGranularity(g)}>
                {g === "Day" ? "Daily" : g === "Week" ? "Weekly" : "Monthly"}
              </button>
            ))}
          </div>
          <button onClick={() => setShowTable((v) => !v)} className="px-3 py-1.5 text-xs border border-gray-200 rounded-lg hover:bg-gray-50">
            {showTable ? "Show charts" : "Show table"}
          </button>
        </div>
      </div>

      {invalid ? (
        <p className="text-sm text-red-600">Start date is after end date.</p>
      ) : analytics.isError ? (
        <p className="text-sm text-red-600">{(analytics.error as Error)?.message ?? "Could not load analytics."}</p>
      ) : analytics.isLoading ? (
        <p className="text-sm text-gray-500 py-10 text-center">Loading…</p>
      ) : showTable ? (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200 text-xs text-gray-500">
                <th className="text-left px-3 py-2 font-medium">Period</th>
                <th className="text-right px-3 py-2 font-medium">Sent</th>
                <th className="text-right px-3 py-2 font-medium">Successful</th>
                <th className="text-right px-3 py-2 font-medium">Failed</th>
                <th className="text-right px-3 py-2 font-medium">Pending / unknown</th>
                <th className="text-right px-3 py-2 font-medium">SMS parts</th>
                <th className="text-right px-3 py-2 font-medium">Cost ({currency})</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.period} className="border-b border-gray-100">
                  <td className="px-3 py-1.5">{r.label}</td>
                  <td className="px-3 py-1.5 text-right tabular-nums">{formatCount(r.sent)}</td>
                  <td className="px-3 py-1.5 text-right tabular-nums">{formatCount(r.successful)}</td>
                  <td className="px-3 py-1.5 text-right tabular-nums">{formatCount(r.failed)}</td>
                  <td className="px-3 py-1.5 text-right tabular-nums">{formatCount(r.other)}</td>
                  <td className="px-3 py-1.5 text-right tabular-nums">{formatCount(r.segments)}</td>
                  <td className="px-3 py-1.5 text-right tabular-nums">{r.cost.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
          <div>
            <div className="flex flex-wrap items-center justify-between gap-2 mb-2">
              <p className="text-xs font-medium text-gray-700">Messages by outcome</p>
              <Legend items={Object.values(SERIES)} />
            </div>
            <ResponsiveContainer width="100%" height={240}>
              <BarChart data={rows} margin={{ top: 4, right: 8, left: -12, bottom: 0 }}>
                <CartesianGrid vertical={false} stroke={INK.grid} />
                <XAxis dataKey="label" tick={{ fontSize: 10, fill: INK.muted }} tickLine={false} axisLine={{ stroke: INK.axis }} interval="preserveStartEnd" minTickGap={16} />
                <YAxis allowDecimals={false} tick={{ fontSize: 10, fill: INK.muted }} tickLine={false} axisLine={false} />
                <Tooltip content={<OutcomeTooltip />} cursor={{ fill: "rgba(0,0,0,0.04)" }} />
                {/* Stacked bottom→top; a 2px white stroke separates segments. */}
                <Bar dataKey="successful" name={SERIES.successful.label} stackId="o" fill={SERIES.successful.color} stroke="#fff" strokeWidth={2} maxBarSize={28} isAnimationActive={false} />
                <Bar dataKey="failed" name={SERIES.failed.label} stackId="o" fill={SERIES.failed.color} stroke="#fff" strokeWidth={2} maxBarSize={28} isAnimationActive={false} />
                <Bar dataKey="other" name={SERIES.other.label} stackId="o" fill={SERIES.other.color} stroke="#fff" strokeWidth={2} radius={[4, 4, 0, 0]} maxBarSize={28} isAnimationActive={false} />
              </BarChart>
            </ResponsiveContainer>
          </div>
          <div>
            <p className="text-xs font-medium text-gray-700 mb-2">Cost ({currency})</p>
            <ResponsiveContainer width="100%" height={240}>
              <LineChart data={rows} margin={{ top: 4, right: 8, left: -12, bottom: 0 }}>
                <CartesianGrid vertical={false} stroke={INK.grid} />
                <XAxis dataKey="label" tick={{ fontSize: 10, fill: INK.muted }} tickLine={false} axisLine={{ stroke: INK.axis }} interval="preserveStartEnd" minTickGap={16} />
                <YAxis tick={{ fontSize: 10, fill: INK.muted }} tickLine={false} axisLine={false} />
                <Tooltip content={<CostTooltip currency={currency} />} cursor={{ stroke: INK.axis }} />
                <Line dataKey="cost" name="Cost" stroke={SERIES.successful.color} strokeWidth={2}
                  dot={rows.length <= 31 ? { r: 3, strokeWidth: 0, fill: SERIES.successful.color } : false}
                  activeDot={{ r: 5, stroke: "#fff", strokeWidth: 2 }} isAnimationActive={false} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
    </div>
  );
}

function Legend({ items }: { items: { label: string; color: string }[] }) {
  return (
    <ul className="flex flex-wrap gap-3 text-xs text-gray-600">
      {items.map((i) => (
        <li key={i.label} className="inline-flex items-center gap-1.5">
          <span className="w-2.5 h-2.5 rounded-sm" style={{ background: i.color }} aria-hidden /> {i.label}
        </li>
      ))}
    </ul>
  );
}

interface TipProps<T> { active?: boolean; payload?: { payload: T }[] }
type Row = SmsAnalyticsPoint & { label: string; other: number };

function OutcomeTooltip({ active, payload }: TipProps<Row>) {
  const r = payload?.[0]?.payload;
  if (!active || !r) return null;
  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-sm px-3 py-2 text-xs space-y-0.5">
      <p className="font-medium text-gray-900">{r.label}</p>
      {([["successful", r.successful], ["failed", r.failed], ["other", r.other]] as const).map(([k, v]) => (
        <p key={k} className="flex items-center gap-1.5 text-gray-700">
          <span className="w-2 h-2 rounded-sm" style={{ background: SERIES[k].color }} aria-hidden />
          {SERIES[k].label}: <span className="tabular-nums font-medium">{formatCount(v)}</span>
        </p>
      ))}
      <p className="text-gray-500 pt-0.5">{formatCount(r.segments)} SMS parts</p>
    </div>
  );
}

function CostTooltip({ active, payload, currency }: TipProps<Row> & { currency: string }) {
  const r = payload?.[0]?.payload;
  if (!active || !r) return null;
  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-sm px-3 py-2 text-xs space-y-0.5">
      <p className="font-medium text-gray-900">{r.label}</p>
      <p className="text-gray-700">Cost: <span className="tabular-nums font-medium">{formatMoney(r.cost, currency)}</span></p>
      <p className="text-gray-500">{formatCount(r.segments)} SMS parts · {formatCount(r.sent)} sent</p>
    </div>
  );
}
