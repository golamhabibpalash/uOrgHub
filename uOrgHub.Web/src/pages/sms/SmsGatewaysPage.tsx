import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, KeyRound, Plus, Power, Star, Trash2, Wifi, XCircle } from "lucide-react";
import {
  activateSmsGateway, createSmsGateway, deleteSmsGateway, getSmsGateways, getSmsProviders,
  setSmsGatewayEnabled, testSmsGateway, updateSmsGateway,
  type SmsGateway, type SmsGatewayInput, type SmsGatewayTestResult, type SmsProviderDescriptor, type SmsProviderField,
} from "../../api/sms";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import Modal from "../../components/shared/Modal";
import { useAuthStore } from "../../store/authStore";
import { extractApiError } from "../../utils/apiError";
import { formatDateTime, formatMoney } from "./smsFormat";

const input = "w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary-500";

export default function SmsGatewaysPage() {
  const { hasClaim, hasRole } = useAuthStore();
  const canManage = hasRole("Admin") || hasClaim("Sms.Gateways.Manage");
  const canSwitch = hasRole("Admin") || hasClaim("Sms.Gateways.Switch");
  const qc = useQueryClient();

  const gateways = useQuery({ queryKey: ["sms-gateways"], queryFn: getSmsGateways });
  const providers = useQuery({ queryKey: ["sms-providers"], queryFn: getSmsProviders });
  const list = gateways.data?.data?.data ?? [];
  const providerList = providers.data?.data?.data ?? [];

  const [editing, setEditing] = useState<SmsGateway | "new" | null>(null);
  const [testing, setTesting] = useState<SmsGateway | null>(null);
  const [removing, setRemoving] = useState<SmsGateway | null>(null);

  const refresh = () => {
    qc.invalidateQueries({ queryKey: ["sms-gateways"] });
    qc.invalidateQueries({ queryKey: ["sms-dashboard"] });
    qc.invalidateQueries({ queryKey: ["sms-compose-info"] });
  };
  const activate = useMutation({ mutationFn: (id: string) => activateSmsGateway(id), onSuccess: refresh });
  const toggle = useMutation({ mutationFn: (g: SmsGateway) => setSmsGatewayEnabled(g.id, !g.isEnabled), onSuccess: refresh });
  const remove = useMutation({
    mutationFn: (id: string) => deleteSmsGateway(id),
    onSuccess: () => { setRemoving(null); refresh(); },
  });

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">SMS Gateways</h2>
          <p className="text-xs text-gray-500 mt-0.5">Every SMS goes through the active gateway. Switching vendor never needs a code change.</p>
        </div>
        {canManage && (
          <button onClick={() => setEditing("new")} className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm rounded-lg bg-primary-600 text-white hover:bg-primary-700">
            <Plus size={14} /> Add gateway
          </button>
        )}
      </div>

      {gateways.isLoading ? (
        <p className="text-sm text-gray-500">Loading…</p>
      ) : list.length === 0 ? (
        <div className="bg-white border border-gray-200 rounded-xl p-8 text-center text-sm text-gray-500">
          No gateways yet.{canManage ? " Add one to start sending — use “Log only (test)” to try things out without sending real SMS." : ""}
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {list.map((g) => (
            <div key={g.id} className={`bg-white border rounded-xl p-4 ${g.isDefault ? "border-primary-300 ring-1 ring-primary-100" : "border-gray-200"}`}>
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-gray-900">{g.name}</p>
                    {g.isDefault && <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs bg-primary-50 text-primary-700"><Star size={11} /> Active</span>}
                    {!g.isEnabled && <span className="px-2 py-0.5 rounded-full text-xs bg-gray-100 text-gray-600">Disabled</span>}
                  </div>
                  <p className="text-xs text-gray-500 mt-0.5">{g.providerName}{g.senderId ? ` · Sender ID ${g.senderId}` : ""}</p>
                </div>
              </div>

              <dl className="grid grid-cols-2 gap-x-4 gap-y-2 mt-3 text-xs">
                <Item label="Rate per SMS" value={g.effectiveRate != null ? formatMoney(g.effectiveRate, g.currency) : "Not set"} />
                <Item label="Balance" value={g.capabilities?.balance === false ? "Not reported" : g.lastBalance != null ? formatMoney(g.lastBalance, g.balanceUnit ?? g.currency) : "—"} />
                <Item label="Endpoint" value={g.effectiveBaseUrl ?? "—"} mono />
                <Item label="Credentials" value={g.credentials.length === 0 ? "None needed" : g.credentials.map((c) => `${c.key} ${c.isSet ? "✓" : "missing"}`).join(" · ")} />
                <Item
                  label="Last test"
                  value={g.lastTestedAt ? `${g.lastTestSucceeded ? "Passed" : "Failed"} · ${formatDateTime(g.lastTestedAt)}` : "Never"}
                  tone={g.lastTestedAt ? (g.lastTestSucceeded ? "ok" : "error") : undefined}
                  title={g.lastTestMessage}
                />
                <Item label="Limits" value={`${g.timeoutSeconds}s timeout · ${g.maxSendsPerSecond}/s`} />
              </dl>

              <div className="flex flex-wrap gap-2 mt-4">
                {canSwitch && !g.isDefault && g.isEnabled && (
                  <ActionButton onClick={() => activate.mutate(g.id)} disabled={activate.isPending} icon={<Star size={12} />}>Make active</ActionButton>
                )}
                {canManage && (
                  <>
                    <ActionButton onClick={() => setTesting(g)} icon={<Wifi size={12} />}>Test connection</ActionButton>
                    <ActionButton onClick={() => setEditing(g)} icon={<KeyRound size={12} />}>Edit</ActionButton>
                    {!g.isDefault && (
                      <>
                        <ActionButton onClick={() => toggle.mutate(g)} disabled={toggle.isPending} icon={<Power size={12} />}>{g.isEnabled ? "Disable" : "Enable"}</ActionButton>
                        <ActionButton onClick={() => setRemoving(g)} icon={<Trash2 size={12} />} danger>Remove</ActionButton>
                      </>
                    )}
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {editing && (
        <GatewayFormModal
          gateway={editing === "new" ? null : editing}
          providers={providerList}
          onClose={() => setEditing(null)}
          onSaved={() => { setEditing(null); refresh(); }}
        />
      )}
      {testing && <TestGatewayModal gateway={testing} onClose={() => { setTesting(null); refresh(); }} />}
      <ConfirmDialog
        open={!!removing}
        title="Remove gateway?"
        message={<>Remove <strong>{removing?.name}</strong>? Its stored credentials are deleted. Messages already sent keep their history.</>}
        confirmLabel="Remove"
        tone="danger"
        loading={remove.isPending}
        onConfirm={() => removing && remove.mutate(removing.id)}
        onCancel={() => setRemoving(null)}
      />
    </div>
  );
}

function Item({ label, value, mono, tone, title }: { label: string; value: string; mono?: boolean; tone?: "ok" | "error"; title?: string }) {
  const color = tone === "ok" ? "text-green-700" : tone === "error" ? "text-red-700" : "text-gray-900";
  return (
    <div title={title}>
      <dt className="text-gray-500">{label}</dt>
      <dd className={`${color} ${mono ? "font-mono break-all" : ""}`}>{value}</dd>
    </div>
  );
}

function ActionButton({ children, icon, onClick, disabled, danger }: { children: React.ReactNode; icon: React.ReactNode; onClick: () => void; disabled?: boolean; danger?: boolean }) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`inline-flex items-center gap-1 px-2.5 py-1.5 text-xs rounded-lg border hover:bg-gray-50 disabled:opacity-50 ${danger ? "border-red-200 text-red-700" : "border-gray-200 text-gray-700"}`}
    >
      {icon} {children}
    </button>
  );
}

function GatewayFormModal({ gateway, providers, onClose, onSaved }: {
  gateway: SmsGateway | null;
  providers: SmsProviderDescriptor[];
  onClose: () => void;
  onSaved: () => void;
}) {
  const [providerKey, setProviderKey] = useState(gateway?.providerKey ?? providers[0]?.key ?? "");
  const provider = useMemo(() => providers.find((p) => p.key === providerKey), [providers, providerKey]);
  const [form, setForm] = useState({
    name: gateway?.name ?? "",
    senderId: gateway?.senderId ?? "",
    baseUrl: gateway?.baseUrl ?? "",
    isEnabled: gateway?.isEnabled ?? true,
    ratePerSegment: gateway?.ratePerSegment?.toString() ?? "",
    currency: gateway?.currency ?? "BDT",
    timeoutSeconds: gateway?.timeoutSeconds ?? 30,
    maxSendsPerSecond: gateway?.maxSendsPerSecond ?? 5,
  });
  const [settings, setSettings] = useState<Record<string, string>>(gateway?.settings ?? {});
  // Secrets are write-only: the form starts blank and only sends what the admin typed.
  const [secrets, setSecrets] = useState<Record<string, string>>({});
  const [clear, setClear] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);

  const stored = new Set(gateway?.credentials.filter((c) => c.isSet).map((c) => c.key));

  const save = useMutation({
    mutationFn: () => {
      const body: SmsGatewayInput = {
        name: form.name.trim(),
        providerKey,
        senderId: form.senderId.trim() || undefined,
        baseUrl: form.baseUrl.trim() || undefined,
        settings,
        credentials: secrets,
        clearCredentials: clear,
        isEnabled: form.isEnabled,
        ratePerSegment: form.ratePerSegment === "" ? null : Number(form.ratePerSegment),
        currency: form.currency.trim() || "BDT",
        timeoutSeconds: Number(form.timeoutSeconds),
        maxSendsPerSecond: Number(form.maxSendsPerSecond),
      };
      return gateway ? updateSmsGateway(gateway.id, body) : createSmsGateway(body);
    },
    onSuccess: onSaved,
    onError: (e) => setError(extractApiError(e)),
  });

  const fieldInput = (f: SmsProviderField) => {
    if (f.isSecret) {
      const isStored = stored.has(f.key) && !clear.includes(f.key);
      return (
        <div>
          <input
            type="password"
            autoComplete="new-password"
            className={input}
            value={secrets[f.key] ?? ""}
            placeholder={isStored ? "Stored — leave blank to keep" : f.placeholder ?? ""}
            onChange={(e) => setSecrets((s) => ({ ...s, [f.key]: e.target.value }))}
          />
          {stored.has(f.key) && !f.required && (
            <label className="inline-flex items-center gap-1.5 text-xs text-gray-500 mt-1">
              <input type="checkbox" checked={clear.includes(f.key)}
                onChange={(e) => setClear((c) => (e.target.checked ? [...c, f.key] : c.filter((k) => k !== f.key)))} />
              Remove stored value
            </label>
          )}
        </div>
      );
    }
    const value = settings[f.key] ?? "";
    const set = (v: string) => setSettings((s) => ({ ...s, [f.key]: v }));
    if (f.kind === "select")
      return (
        <select className={input} value={value || f.defaultValue || ""} onChange={(e) => set(e.target.value)}>
          {(f.options ?? []).map((o) => <option key={o} value={o}>{o}</option>)}
        </select>
      );
    if (f.kind === "textarea")
      return <textarea className={`${input} font-mono`} rows={3} value={value} placeholder={f.placeholder} onChange={(e) => set(e.target.value)} />;
    return <input className={input} value={value} placeholder={f.placeholder ?? f.defaultValue ?? ""} onChange={(e) => set(e.target.value)} />;
  };

  return (
    <Modal title={gateway ? `Edit ${gateway.name}` : "Add SMS gateway"} open onClose={onClose} size="2xl">
      <form
        className="space-y-4"
        onSubmit={(e) => { e.preventDefault(); setError(null); save.mutate(); }}
      >
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Labeled label="Provider *">
            <select className={input} value={providerKey} disabled={!!gateway}
              onChange={(e) => { setProviderKey(e.target.value); setSettings({}); setSecrets({}); }}>
              {providers.map((p) => <option key={p.key} value={p.key}>{p.displayName}</option>)}
            </select>
          </Labeled>
          <Labeled label="Name *">
            <input className={input} required maxLength={100} value={form.name} placeholder="e.g. BulkSMSBD main"
              onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
          </Labeled>
        </div>
        {provider && <p className="text-xs text-gray-500 -mt-2">{provider.description}</p>}

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Labeled label={`Sender ID${provider?.capabilities.requiresSenderId ? " *" : ""}`} help="Approved masking / sender name.">
            <input className={input} maxLength={50} value={form.senderId} onChange={(e) => setForm((f) => ({ ...f, senderId: e.target.value }))} />
          </Labeled>
          <Labeled label="API base URL" help={provider?.defaultBaseUrl ? `Blank uses ${provider.defaultBaseUrl}` : undefined}>
            <input className={input} value={form.baseUrl} placeholder={provider?.defaultBaseUrl ?? ""} onChange={(e) => setForm((f) => ({ ...f, baseUrl: e.target.value }))} />
          </Labeled>
        </div>

        {provider && provider.fields.length > 0 && (
          <div className="border-t border-gray-100 pt-3 grid grid-cols-1 sm:grid-cols-2 gap-3">
            {provider.fields.map((f) => (
              <div key={f.key} className={f.kind === "textarea" || f.key.endsWith("_url") ? "sm:col-span-2" : ""}>
                <Labeled label={`${f.label}${f.required ? " *" : ""}${f.isSecret ? " 🔒" : ""}`} help={f.help}>
                  {fieldInput(f)}
                </Labeled>
              </div>
            ))}
          </div>
        )}

        <div className="border-t border-gray-100 pt-3 grid grid-cols-2 sm:grid-cols-4 gap-3">
          <Labeled label="Rate per SMS" help={provider?.capabilities.rate ? "Provider's rate is used when it reports one." : "Used to estimate cost."}>
            <input className={input} type="number" min={0} step="0.0001" value={form.ratePerSegment}
              onChange={(e) => setForm((f) => ({ ...f, ratePerSegment: e.target.value }))} />
          </Labeled>
          <Labeled label="Currency">
            <input className={input} maxLength={10} value={form.currency} onChange={(e) => setForm((f) => ({ ...f, currency: e.target.value }))} />
          </Labeled>
          <Labeled label="Timeout (s)">
            <input className={input} type="number" min={5} max={120} value={form.timeoutSeconds}
              onChange={(e) => setForm((f) => ({ ...f, timeoutSeconds: Number(e.target.value) }))} />
          </Labeled>
          <Labeled label="Max per second">
            <input className={input} type="number" min={1} max={100} value={form.maxSendsPerSecond}
              onChange={(e) => setForm((f) => ({ ...f, maxSendsPerSecond: Number(e.target.value) }))} />
          </Labeled>
        </div>

        <label className="inline-flex items-center gap-2 text-sm text-gray-700">
          <input type="checkbox" checked={form.isEnabled} disabled={gateway?.isDefault}
            onChange={(e) => setForm((f) => ({ ...f, isEnabled: e.target.checked }))} />
          Enabled {gateway?.isDefault && <span className="text-xs text-gray-500">(the active gateway stays enabled)</span>}
        </label>

        {error && <p role="alert" className="text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</p>}

        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onClose} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={save.isPending || !providerKey} className="px-4 py-2 text-sm rounded-lg bg-primary-600 text-white hover:bg-primary-700 disabled:opacity-50">
            {save.isPending ? "Saving…" : gateway ? "Save changes" : "Add gateway"}
          </button>
        </div>
      </form>
    </Modal>
  );
}

function Labeled({ label, help, children }: { label: string; help?: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="text-xs text-gray-500 mb-1 block">{label}</label>
      {children}
      {help && <p className="text-[11px] text-gray-400 mt-0.5">{help}</p>}
    </div>
  );
}

function TestGatewayModal({ gateway, onClose }: { gateway: SmsGateway; onClose: () => void }) {
  const [recipient, setRecipient] = useState("");
  const [result, setResult] = useState<SmsGatewayTestResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const test = useMutation({
    mutationFn: () => testSmsGateway(gateway.id, recipient.trim() || undefined),
    onSuccess: (res) => setResult(res.data.data ?? null),
    onError: (e) => setError(extractApiError(e)),
  });

  return (
    <Modal title={`Test ${gateway.name}`} open onClose={onClose} size="md">
      <div className="space-y-3 text-sm">
        <p className="text-gray-600">
          Checks the endpoint and credentials{gateway.capabilities?.balance ? " with a balance request — nothing is sent or charged" : ""}.
          Enter a number to also send a real test SMS.
        </p>
        <div>
          <label className="text-xs text-gray-500 mb-1 block">Test number (optional — sends one SMS)</label>
          <input className={input} value={recipient} placeholder="01712345678" onChange={(e) => setRecipient(e.target.value)} />
        </div>
        {result && (
          <div role="status" className={`flex items-start gap-2 rounded-lg border px-3 py-2 ${result.success ? "bg-green-50 border-green-200 text-green-800" : "bg-red-50 border-red-200 text-red-800"}`}>
            {result.success ? <CheckCircle2 size={16} className="mt-0.5 shrink-0" /> : <XCircle size={16} className="mt-0.5 shrink-0" />}
            <span>{result.message}</span>
          </div>
        )}
        {error && <p role="alert" className="text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</p>}
        <div className="flex justify-end gap-2">
          <button onClick={onClose} className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">Close</button>
          <button onClick={() => { setError(null); setResult(null); test.mutate(); }} disabled={test.isPending}
            className="px-4 py-2 text-sm rounded-lg bg-primary-600 text-white hover:bg-primary-700 disabled:opacity-50">
            {test.isPending ? "Testing…" : "Run test"}
          </button>
        </div>
      </div>
    </Modal>
  );
}
