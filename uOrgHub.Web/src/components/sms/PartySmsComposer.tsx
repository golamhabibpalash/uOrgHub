import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, RotateCcw, User } from "lucide-react";
import type { PartySmsPreview } from "../../api/accounts";
import { getSmsComposeInfo } from "../../api/sms";
import { calculateSmsSegments, formatPhone } from "../../utils/smsSegments";

interface Props {
  preview?: PartySmsPreview;
  loading?: boolean;
  message: string;
  onChange: (message: string) => void;
  disabled?: boolean;
}

/**
 * The SMS to a payment's or voucher's party: who it goes to (the number on their record — not
 * editable by design), the template text to tweak, and how many SMS it will take.
 */
export default function PartySmsComposer({ preview, loading, message, onChange, disabled }: Props) {
  const info = useQuery({ queryKey: ["sms-compose-info"], queryFn: getSmsComposeInfo, staleTime: 60000 });
  const compose = info.data?.data?.data;
  const seg = calculateSmsSegments(message);

  if (loading || !preview) return <p className="text-sm text-gray-500 py-3">Preparing message…</p>;

  if (!preview.canSend)
    return (
      <div role="alert" className="flex items-start gap-2 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">
        <AlertTriangle size={16} className="mt-0.5 shrink-0" />
        <span>{preview.reason ?? "This SMS can't be sent."}</span>
      </div>
    );

  return (
    <div className="space-y-2">
      <div className="flex items-center gap-2 text-sm">
        <User size={14} className="text-gray-400" aria-hidden />
        <span className="text-gray-500">To</span>
        <span className="font-medium text-gray-900">{preview.partyName}</span>
        <span className="font-mono text-gray-600">{preview.phone ? formatPhone(preview.phone) : ""}</span>
      </div>
      <textarea
        aria-label="SMS message"
        value={message}
        disabled={disabled}
        onChange={(e) => onChange(e.target.value)}
        rows={4}
        className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-primary-500 disabled:bg-gray-50"
      />
      <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-gray-500" aria-live="polite">
        <span className="tabular-nums">
          {seg.encoding === "Gsm7" ? "GSM-7" : "Unicode"} · {seg.characters} chars · {seg.segments} SMS
          {compose?.ratePerSegment != null && seg.segments > 0 && ` · ≈ ${(compose.ratePerSegment * seg.segments).toFixed(2)} ${compose.currency}`}
        </span>
        {message !== preview.message && (
          <button type="button" onClick={() => onChange(preview.message)} disabled={disabled}
            className="inline-flex items-center gap-1 text-primary-600 hover:underline">
            <RotateCcw size={11} /> Use template text
          </button>
        )}
      </div>
      {compose && !compose.hasActiveGateway && (
        <p className="text-xs text-red-600">No active SMS gateway — the SMS can't be sent until an administrator sets one up.</p>
      )}
    </div>
  );
}
