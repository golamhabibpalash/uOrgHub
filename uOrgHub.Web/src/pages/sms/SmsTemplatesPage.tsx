import { useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { RotateCcw, Save } from "lucide-react";
import { getSmsTemplates, resetSmsTemplate, updateSmsTemplate, type SmsTemplate } from "../../api/sms";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import { useAuthStore } from "../../store/authStore";
import { extractApiError } from "../../utils/apiError";
import { calculateSmsSegments } from "../../utils/smsSegments";
import { formatDateTime } from "./smsFormat";

export default function SmsTemplatesPage() {
  const { hasClaim, hasRole } = useAuthStore();
  const canManage = hasRole("Admin") || hasClaim("Sms.Templates.Manage");
  const templates = useQuery({ queryKey: ["sms-templates"], queryFn: getSmsTemplates });
  const list = templates.data?.data?.data ?? [];

  return (
    <div className="max-w-5xl">
      <div className="mb-4">
        <h2 className="text-base font-medium text-gray-900">SMS Templates</h2>
        <p className="text-xs text-gray-500 mt-0.5">
          The text sent to a customer, vendor or client after a payment or voucher. Placeholders are filled in when the SMS is prepared, and the user can still adjust the message before sending.
        </p>
      </div>
      {templates.isLoading ? (
        <p className="text-sm text-gray-500">Loading…</p>
      ) : (
        <div className="space-y-4">
          {list.map((t) => <TemplateEditor key={`${t.eventKey}-${t.updatedAt ?? "default"}`} template={t} canManage={canManage} />)}
        </div>
      )}
    </div>
  );
}

function render(body: string, t: SmsTemplate) {
  return t.placeholders.reduce((text, p) => text.replace(new RegExp(`\\{${p.key}\\}`, "gi"), p.sample), body).trim();
}

function TemplateEditor({ template, canManage }: { template: SmsTemplate; canManage: boolean }) {
  const qc = useQueryClient();
  const [body, setBody] = useState(template.body);
  const [error, setError] = useState<string | null>(null);
  const [confirmReset, setConfirmReset] = useState(false);
  const ref = useRef<HTMLTextAreaElement>(null);
  const dirty = body !== template.body;
  const sample = render(body, template);
  const seg = calculateSmsSegments(sample);

  const onSaved = () => qc.invalidateQueries({ queryKey: ["sms-templates"] });
  const save = useMutation({
    mutationFn: () => updateSmsTemplate(template.eventKey, body),
    onSuccess: onSaved,
    onError: (e) => setError(extractApiError(e)),
  });
  const reset = useMutation({
    mutationFn: () => resetSmsTemplate(template.eventKey),
    onSuccess: () => { setConfirmReset(false); onSaved(); },
    onError: (e) => setError(extractApiError(e)),
  });

  /** Inserts {key} at the cursor, keeping focus in the editor. */
  const insert = (key: string) => {
    const el = ref.current;
    const token = `{${key}}`;
    if (!el) { setBody((b) => b + token); return; }
    const start = el.selectionStart ?? body.length;
    const end = el.selectionEnd ?? body.length;
    const next = body.slice(0, start) + token + body.slice(end);
    setBody(next);
    requestAnimationFrame(() => { el.focus(); el.setSelectionRange(start + token.length, start + token.length); });
  };

  return (
    <section className="bg-white border border-gray-200 rounded-xl p-4" aria-labelledby={`tpl-${template.eventKey}`}>
      <div className="flex flex-wrap items-start justify-between gap-2 mb-3">
        <div>
          <h3 id={`tpl-${template.eventKey}`} className="text-sm font-medium text-gray-900">
            {template.label}
            <span className={`ml-2 px-2 py-0.5 rounded-full text-xs font-normal ${template.isCustom ? "bg-primary-50 text-primary-700" : "bg-gray-100 text-gray-600"}`}>
              {template.isCustom ? "Customised" : "Default"}
            </span>
          </h3>
          <p className="text-xs text-gray-500 mt-0.5">{template.description}</p>
        </div>
        {template.isCustom && template.updatedAt && (
          <p className="text-xs text-gray-400">Edited {formatDateTime(template.updatedAt)}{template.updatedBy ? ` by ${template.updatedBy}` : ""}</p>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div className="space-y-2">
          <label htmlFor={`body-${template.eventKey}`} className="text-xs text-gray-500 block">Message</label>
          <textarea
            id={`body-${template.eventKey}`}
            ref={ref}
            value={body}
            readOnly={!canManage}
            onChange={(e) => { setBody(e.target.value); setError(null); }}
            rows={5}
            className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 font-mono focus:outline-none focus:ring-1 focus:ring-primary-500 read-only:bg-gray-50"
          />
          {canManage && (
            <div>
              <p className="text-xs text-gray-500 mb-1">Insert placeholder</p>
              <div className="flex flex-wrap gap-1.5">
                {template.placeholders.map((p) => (
                  <button key={p.key} type="button" onClick={() => insert(p.key)} title={p.description}
                    className="px-2 py-0.5 text-xs font-mono rounded-md border border-gray-200 bg-gray-50 hover:bg-gray-100 text-gray-700">
                    {`{${p.key}}`}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
        <div className="space-y-2">
          <p className="text-xs text-gray-500">Preview with sample values</p>
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-3 text-sm whitespace-pre-wrap break-words min-h-[7.5rem]">{sample || <span className="text-gray-400">Empty</span>}</div>
          <p className="text-xs text-gray-500 tabular-nums" aria-live="polite">
            {seg.encoding === "Gsm7" ? "GSM-7" : "Unicode"} · {seg.characters} characters · {seg.segments} SMS
            <span className="text-gray-400"> (actual length depends on the real names and amounts)</span>
          </p>
        </div>
      </div>

      {error && <p role="alert" className="mt-3 text-sm text-red-700 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</p>}

      {canManage && (
        <div className="flex flex-wrap justify-end gap-2 mt-3">
          {dirty && (
            <button type="button" onClick={() => setBody(template.body)} className="px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">
              Discard changes
            </button>
          )}
          {template.isCustom && !dirty && (
            <button type="button" onClick={() => setConfirmReset(true)} className="inline-flex items-center gap-1 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50">
              <RotateCcw size={13} /> Reset to default
            </button>
          )}
          <button type="button" onClick={() => { setError(null); save.mutate(); }} disabled={!dirty || save.isPending || body.trim() === ""}
            className="inline-flex items-center gap-1 px-3 py-1.5 text-sm rounded-lg bg-primary-600 text-white hover:bg-primary-700 disabled:opacity-50">
            <Save size={13} /> {save.isPending ? "Saving…" : "Save"}
          </button>
        </div>
      )}

      <ConfirmDialog
        open={confirmReset}
        title="Reset to default?"
        message={<>The built-in text will be used again:<br /><span className="font-mono text-xs">{template.defaultBody}</span></>}
        confirmLabel="Reset"
        loading={reset.isPending}
        onConfirm={() => reset.mutate()}
        onCancel={() => setConfirmReset(false)}
      />
    </section>
  );
}
