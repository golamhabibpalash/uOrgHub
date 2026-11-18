import { CheckCircle2, Clock, HelpCircle, Send, XCircle } from "lucide-react";
import type { SmsMessageStatus } from "../../api/sms";

/** Status words with an icon, so a status never reads by colour alone. */
export const SMS_STATUS_STYLE: Record<SmsMessageStatus, { label: string; className: string; Icon: typeof Clock }> = {
  Queued: { label: "Queued", className: "bg-gray-100 text-gray-700", Icon: Clock },
  Sending: { label: "Sending", className: "bg-gray-100 text-gray-700", Icon: Send },
  Submitted: { label: "Submitted", className: "bg-blue-50 text-blue-700", Icon: Send },
  Delivered: { label: "Delivered", className: "bg-green-50 text-green-700", Icon: CheckCircle2 },
  Failed: { label: "Failed", className: "bg-red-50 text-red-700", Icon: XCircle },
  Undetermined: { label: "Unknown", className: "bg-amber-50 text-amber-800", Icon: HelpCircle },
};

export const SMS_STATUSES = Object.keys(SMS_STATUS_STYLE) as SmsMessageStatus[];
export const smsStatusLabel = (s: SmsMessageStatus) => SMS_STATUS_STYLE[s].label;

export function formatMoney(value: number | null | undefined, currency = "BDT", digits = 2) {
  if (value === null || value === undefined) return "—";
  return `${value.toLocaleString("en-BD", { minimumFractionDigits: digits, maximumFractionDigits: Math.max(digits, 4) })} ${currency}`;
}

export const formatCount = (n: number) => n.toLocaleString("en-BD");

export function formatDateTime(iso?: string | null) {
  if (!iso) return "—";
  // The API sends UTC without a zone suffix (legacy timestamp mode); read it as UTC.
  const d = new Date(/[zZ]|[+-]\d\d:\d\d$/.test(iso) ? iso : iso + "Z");
  return d.toLocaleString("en-GB", { day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" });
}

/** Local calendar date as yyyy-mm-dd. */
export function localDate(d: Date) {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

/** Start of a local yyyy-mm-dd day as a UTC ISO string; `nextDay` gives the exclusive end. */
export function localDayToUtc(day: string, nextDay = false) {
  const d = new Date(`${day}T00:00:00`);
  if (nextDay) d.setDate(d.getDate() + 1);
  return d.toISOString();
}

/** One key per composed message: a resubmit of the same content is recognised as the same send. */
export function newIdempotencyKey() {
  if (typeof crypto !== "undefined" && "randomUUID" in crypto) return crypto.randomUUID();
  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 12)}`;
}
