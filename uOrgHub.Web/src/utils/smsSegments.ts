/**
 * SMS segment counting, as handsets and gateways do it (3GPP TS 23.038 / 23.040).
 * Mirrors uOrgHub.Sms/Services/SmsSegmentCalculator.cs — keep the two in step; the server
 * recomputes on send, so this only drives the live counter on the compose page.
 *
 * All-GSM text: 160 septets in one SMS, 153 per part once split; extension characters
 * (€ [ ] { } ~ ^ | \ and form feed) cost two. Anything else makes the whole message UCS-2:
 * 70 code units, 67 per part; an emoji (surrogate pair) costs two. A two-unit character is never
 * cut across parts, so counting packs characters into parts instead of dividing.
 */
export type SmsEncoding = "Gsm7" | "Unicode";

export interface SmsSegmentInfo {
  encoding: SmsEncoding;
  characters: number;
  units: number;
  segments: number;
  perSegmentLimit: number;
  remainingInSegment: number;
}

const GSM_BASIC =
  "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?" +
  "¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";
const GSM_EXTENSION = "^{}\\[~]|€\f";
const BASIC = new Set(GSM_BASIC);
const EXTENSION = new Set(GSM_EXTENSION);

export const SMS_LIMITS = { gsmSingle: 160, gsmMulti: 153, unicodeSingle: 70, unicodeMulti: 67 } as const;

/** Per-character costs, iterating UTF-16 code units the way C# strings do. */
function costs(text: string): { encoding: SmsEncoding; list: number[] } {
  let gsm = true;
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    if (!BASIC.has(c) && !EXTENSION.has(c)) { gsm = false; break; }
  }
  const list: number[] = [];
  if (gsm) {
    for (let i = 0; i < text.length; i++) list.push(EXTENSION.has(text[i]) ? 2 : 1);
    return { encoding: "Gsm7", list };
  }
  for (let i = 0; i < text.length; i++) {
    const code = text.charCodeAt(i);
    const next = i + 1 < text.length ? text.charCodeAt(i + 1) : 0;
    if (code >= 0xd800 && code <= 0xdbff && next >= 0xdc00 && next <= 0xdfff) {
      list.push(2);
      i++;
    } else {
      list.push(1);
    }
  }
  return { encoding: "Unicode", list };
}

export function calculateSmsSegments(text: string): SmsSegmentInfo {
  const { encoding, list } = costs(text);
  const units = list.reduce((s, c) => s + c, 0);
  const [single, multi] = encoding === "Gsm7"
    ? [SMS_LIMITS.gsmSingle, SMS_LIMITS.gsmMulti]
    : [SMS_LIMITS.unicodeSingle, SMS_LIMITS.unicodeMulti];

  if (units === 0) return { encoding, characters: 0, units: 0, segments: 0, perSegmentLimit: single, remainingInSegment: single };
  if (units <= single) return { encoding, characters: list.length, units, segments: 1, perSegmentLimit: single, remainingInSegment: single - units };

  let segments = 1;
  let used = 0;
  for (const cost of list) {
    if (used + cost > multi) { segments++; used = 0; }
    used += cost;
  }
  return { encoding, characters: list.length, units, segments, perSegmentLimit: multi, remainingInSegment: multi - used };
}

/** Characters that force Unicode, for the "why is this Unicode?" hint. */
export function nonGsmCharacters(text: string): string[] {
  const found = new Set<string>();
  for (const ch of text) if (!BASIC.has(ch) && !EXTENSION.has(ch)) found.add(ch);
  return [...found];
}

/**
 * Recipient parsing — mirrors SmsPhoneNumber.cs. Returns normalised numbers (digits with
 * country code), de-duplicated, plus entries that are not valid mobile numbers.
 */
const BD_MOBILE = /^(?:\+?88|0088)?(01[3-9]\d{8})$/;
const INTERNATIONAL = /^\+([1-9]\d{7,14})$/;

export function normalizePhone(raw: string): string | null {
  const compact = raw.trim().replace(/[\s\-().]/g, "");
  const bd = BD_MOBILE.exec(compact);
  if (bd) return "88" + bd[1];
  const intl = INTERNATIONAL.exec(compact);
  return intl ? intl[1] : null;
}

export function parseRecipients(input: string): { valid: string[]; invalid: string[] } {
  const valid: string[] = [];
  const invalid: string[] = [];
  const seen = new Set<string>();
  for (const entry of input.split(/[,;\n\r]+/).map((s) => s.trim()).filter(Boolean)) {
    const n = normalizePhone(entry);
    if (!n) invalid.push(entry);
    else if (!seen.has(n)) { seen.add(n); valid.push(n); }
  }
  return { valid, invalid };
}

/** 8801712345678 → 01712-345678 for display; other numbers get a leading +. */
export function formatPhone(normalized: string): string {
  if (normalized.length === 13 && normalized.startsWith("8801")) {
    const local = normalized.slice(2);
    return `${local.slice(0, 5)}-${local.slice(5)}`;
  }
  return "+" + normalized;
}
