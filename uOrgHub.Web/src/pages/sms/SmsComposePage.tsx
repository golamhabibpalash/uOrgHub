import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { AlertTriangle, CheckCircle2, Info, Loader2, Send, Users } from "lucide-react";
import { getSmsComposeInfo, getSmsSendStatus, sendSms, type SendSmsResult } from "../../api/sms";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import { calculateSmsSegments, formatPhone, nonGsmCharacters, parseRecipients } from "../../utils/smsSegments";
import { extractApiError } from "../../utils/apiError";
import { formatCount, formatMoney, newIdempotencyKey } from "./smsFormat";

const MAX_MESSAGE = 1600;

export default function SmsComposePage() {
  const [recipientsText, setRecipientsText] = useState("");
  const [message, setMessage] = useState("");
  // A new key whenever the content changes: an identical resubmit (double click, retry after a
  // dropped connection) is deduplicated by the server; an edited message is a new send.
  const [idempotencyKey, setIdempotencyKey] = useState(newIdempotencyKey);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<SendSmsResult | null>(null);

  const info = useQuery({ queryKey: ["sms-compose-info"], queryFn: getSmsComposeInfo });
  const compose = info.data?.data?.data;

  const recipients = useMemo(() => parseRecipients(recipientsText), [recipientsText]);
  const segments = useMemo(() => calculateSmsSegments(message), [message]);
  const forcingUnicode = useMemo(() => (segments.encoding === "Unicode" ? nonGsmCharacters(message).slice(0, 8) : []), [message, segments.encoding]);

  const count = recipients.valid.length;
  const totalSegments = segments.segments * count;
  const rate = compose?.ratePerSegment;
  const currency = compose?.currency ?? "BDT";
  const isBulk = count > 1;
  const bulkBlocked = isBulk && compose && !compose.canSendBulk;
  const tooMany = compose ? count > compose.maxRecipients : false;

  const problems = [
    !compose?.hasActiveGateway && "No active SMS gateway — ask an administrator to set one up.",
    recipients.invalid.length > 0 && `${recipients.invalid.length} invalid number${recipients.invalid.length > 1 ? "s" : ""}: ${recipients.invalid.slice(0, 5).join(", ")}${recipients.invalid.length > 5 ? "…" : ""}`,
    bulkBlocked && "You can send to one recipient at a time. Sending to several needs the 'Send bulk SMS' permission.",
    tooMany && `At most ${compose?.maxRecipients} recipients per send.`,
    message.length > MAX_MESSAGE && `Message is longer than ${MAX_MESSAGE} characters.`,
  ].filter(Boolean) as string[];
  const canSend = !!compose?.hasActiveGateway && count > 0 && message.trim().length > 0 && problems.length === 0;

  const changeRecipients = (v: string) => { setRecipientsText(v); setIdempotencyKey(newIdempotencyKey()); setError(null); };
  const changeMessage = (v: string) => { setMessage(v); setIdempotencyKey(newIdempotencyKey()); setError(null); };

  const send = useMutation({
    mutationFn: () => sendSms({ recipients: recipients.valid, message, idempotencyKey }),
    onSuccess: (res) => {
      setResult(res.data.data ?? null);
      setRecipientsText("");
      setMessage("");
      setIdempotencyKey(newIdempotencyKey());
    },
    onError: (err) => setError(extractApiError(err)),
    onSettled: () => setConfirmOpen(false),
  });

  const submit = () => {
    if (!canSend || send.isPending) return;
    setError(null);
    if (isBulk) setConfirmOpen(true);
    else send.mutate();
  };

  return (
    <div className="max-w-5xl">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-base font-medium text-gray-900">Compose SMS</h2>
          <p className="text-xs text-gray-500 mt-0.5">
            {compose?.hasActiveGateway
              ? <>Sending via <span className="font-medium text-gray-700">{compose.gatewayName}</span> ({compose.providerName}){rate != null ? ` · ${formatMoney(rate, currency, 2)} per SMS` : ""}</>
              : info.isLoading ? "Loading gateway…" : "No active gateway"}
          </p>
        </div>
        <Link to="/sms/history" className="text-sm text-primary-600 hover:underline">View history →</Link>
      </div>

      {result && <SendProgress result={result} onDismiss={() => setResult(null)} />}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <div className="bg-white border border-gray-200 rounded-xl p-4">
            <label htmlFor="sms-recipients" className="text-sm font-medium text-gray-900 block mb-1">Recipients</label>
            <p className="text-xs text-gray-500 mb-2">
              Mobile numbers, separated by commas or new lines — e.g. 01712345678 or +8801712345678.
              {compose && !compose.canSendBulk && " You can send to one number at a time."}
            </p>
            <textarea
              id="sms-recipients"
              value={recipientsText}
              onChange={(e) => changeRecipients(e.target.value)}
              rows={compose?.canSendBulk ? 4 : 2}
              placeholder="01712345678"
              className="w-full text-sm font-mono border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
            <div className="flex flex-wrap items-center gap-2 mt-2 text-xs">
              <span className="inline-flex items-center gap-1 text-gray-600"><Users size={12} /> {count} valid recipient{count === 1 ? "" : "s"}</span>
              {recipients.valid.slice(0, 6).map((n) => (
                <span key={n} className="px-2 py-0.5 rounded-full bg-gray-100 text-gray-700 font-mono">{formatPhone(n)}</span>
              ))}
              {count > 6 && <span className="text-gray-400">+{count - 6} more</span>}
            </div>
          </div>

          <div className="bg-white border border-gray-200 rounded-xl p-4">
            <label htmlFor="sms-message" className="text-sm font-medium text-gray-900 block mb-2">Message</label>
            <textarea
              id="sms-message"
              value={message}
              onChange={(e) => changeMessage(e.target.value)}
              rows={7}
              placeholder="Type your message…"
              className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary-500"
            />
            <div className="flex flex-wrap items-center justify-between gap-2 mt-2 text-xs text-gray-500" aria-live="polite">
              <span>
                {segments.encoding === "Gsm7" ? "GSM-7" : "Unicode"} · {segments.characters} characters
                {segments.units !== segments.characters && ` (${segments.units} ${segments.encoding === "Gsm7" ? "septets" : "units"})`}
              </span>
              <span className="tabular-nums">
                {segments.segments} SMS · {segments.remainingInSegment} left in this part
                <span className="text-gray-400"> ({segments.perSegmentLimit}/part)</span>
              </span>
            </div>
            {forcingUnicode.length > 0 && (
              <p className="flex items-start gap-1.5 text-xs text-gray-500 mt-2">
                <Info size={12} className="mt-0.5 shrink-0" />
                Unicode because of: <span className="font-mono">{forcingUnicode.map((c) => (c === " " ? "␠" : c)).join(" ")}</span> — each SMS holds 70 characters instead of 160.
              </p>
            )}
          </div>
        </div>

        <div className="space-y-4">
          <div className="bg-white border border-gray-200 rounded-xl p-4">
            <p className="text-sm font-medium text-gray-900 mb-3">Estimate</p>
            <dl className="space-y-2 text-sm">
              <Row label="SMS per recipient" value={formatCount(segments.segments)} />
              <Row label="Recipients" value={formatCount(count)} />
              <Row label="Total SMS" value={formatCount(totalSegments)} strong />
              <Row label="Estimated cost" value={rate != null ? formatMoney(rate * totalSegments, currency) : "Rate not set"} strong />
            </dl>
          </div>

          {(problems.length > 0 || error) && (
            <div role="alert" className="bg-red-50 border border-red-200 rounded-xl p-3 space-y-1">
              {[...problems, ...(error ? [error] : [])].map((p) => (
                <p key={p} className="flex items-start gap-1.5 text-xs text-red-700"><AlertTriangle size={12} className="mt-0.5 shrink-0" /> {p}</p>
              ))}
            </div>
          )}

          <button
            onClick={submit}
            disabled={!canSend || send.isPending}
            className="w-full inline-flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-medium rounded-lg bg-primary-600 text-white hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {send.isPending ? <Loader2 size={16} className="animate-spin" /> : <Send size={16} />}
            {send.isPending ? "Sending…" : isBulk ? `Send to ${formatCount(count)} recipients` : "Send SMS"}
          </button>
        </div>
      </div>

      <ConfirmDialog
        open={confirmOpen}
        title="Send bulk SMS?"
        message={
          <div className="space-y-1 text-sm">
            <p>This sends <strong>{formatCount(totalSegments)} SMS</strong> to <strong>{formatCount(count)} recipients</strong> via {compose?.gatewayName}.</p>
            {rate != null && <p>Estimated cost: <strong>{formatMoney(rate * totalSegments, currency)}</strong>.</p>}
            <p className="text-gray-500">Sent messages can't be recalled.</p>
          </div>
        }
        confirmLabel="Send now"
        loading={send.isPending}
        onConfirm={() => send.mutate()}
        onCancel={() => setConfirmOpen(false)}
      />
    </div>
  );
}

function Row({ label, value, strong }: { label: string; value: string; strong?: boolean }) {
  return (
    <div className="flex items-center justify-between">
      <dt className="text-gray-500">{label}</dt>
      <dd className={`tabular-nums ${strong ? "font-medium text-gray-900" : "text-gray-700"}`}>{value}</dd>
    </div>
  );
}

/** Follows a send until every message has left the queue (polls for up to a minute). */
function SendProgress({ result, onDismiss }: { result: SendSmsResult; onDismiss: () => void }) {
  // Polling stops after a minute; the history page has the final word after that.
  const [timedOut, setTimedOut] = useState(false);
  useEffect(() => {
    const t = setTimeout(() => setTimedOut(true), 60_000);
    return () => clearTimeout(t);
  }, [result.requestId]);

  const status = useQuery({
    queryKey: ["sms-send-status", result.requestId],
    queryFn: () => getSmsSendStatus(result.requestId),
    refetchInterval: (q) => (q.state.data?.data?.data?.completed || timedOut ? false : 2000),
  });
  const s = status.data?.data?.data;
  const done = s?.completed;
  const ok = (s?.submitted ?? 0) + (s?.delivered ?? 0);

  return (
    <div role="status" className={`mb-4 rounded-xl border p-4 ${done && (s?.failed ?? 0) === 0 && (s?.undetermined ?? 0) === 0 ? "bg-green-50 border-green-200" : "bg-white border-gray-200"}`}>
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-start gap-2">
          {done ? <CheckCircle2 size={18} className="text-green-600 mt-0.5" /> : <Loader2 size={18} className="animate-spin text-gray-500 mt-0.5" />}
          <div className="text-sm">
            <p className="font-medium text-gray-900">
              {result.duplicate ? "Already sent — this is the earlier send" : done ? "Send finished" : "Sending…"}
            </p>
            <p className="text-gray-600 mt-0.5">
              {s
                ? `${ok} accepted · ${s.failed} failed${s.undetermined ? ` · ${s.undetermined} unknown` : ""}${s.queued ? ` · ${s.queued} waiting` : ""} of ${s.recipientCount}`
                : `${result.recipientCount} recipient${result.recipientCount === 1 ? "" : "s"} queued via ${result.gatewayName}`}
            </p>
            {s && s.failures.length > 0 && (
              <ul className="mt-1 text-xs text-red-700 list-disc ml-4">
                {s.failures.map((f) => <li key={f.recipient}>{formatPhone(f.recipient)}: {f.errorMessage ?? "Failed"}</li>)}
              </ul>
            )}
            {!done && timedOut && (
              <p className="text-xs text-gray-500 mt-1">Still in progress — check SMS History for the final result.</p>
            )}
          </div>
        </div>
        <button onClick={onDismiss} className="text-xs text-gray-500 hover:text-gray-700">Dismiss</button>
      </div>
    </div>
  );
}
