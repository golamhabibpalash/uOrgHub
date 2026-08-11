export function formatBDT(amount: number): string {
  if (amount >= 10_000_000) return `৳ ${(amount / 10_000_000).toFixed(1)} Cr`;
  if (amount >= 100_000) return `৳ ${(amount / 100_000).toFixed(1)} L`;
  if (amount >= 1_000) return `৳ ${(amount / 1_000).toFixed(1)}K`;
  return `৳ ${amount.toLocaleString()}`;
}

/** Exact Taka amount with thousands separators — for vouchers, invoices and anything printed. */
export function formatTaka(amount: number): string {
  return `৳ ${amount.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

const ONES = [
  '', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten',
  'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen',
];

const TENS = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

function belowThousandInWords(n: number): string {
  if (n < 20) return ONES[n];
  if (n < 100) {
    const unit = n % 10;
    return unit ? `${TENS[Math.floor(n / 10)]} ${ONES[unit]}` : TENS[Math.floor(n / 10)];
  }
  const remainder = n % 100;
  const hundreds = `${ONES[Math.floor(n / 100)]} Hundred`;
  return remainder ? `${hundreds} ${belowThousandInWords(remainder)}` : hundreds;
}

/** Spells a whole number using the Bangladeshi/South Asian scale (crore, lakh, thousand). */
function wholeNumberInWords(n: number): string {
  if (n === 0) return 'Zero';

  const crore = Math.floor(n / 10_000_000);
  const lakh = Math.floor((n % 10_000_000) / 100_000);
  const thousand = Math.floor((n % 100_000) / 1_000);
  const rest = n % 1_000;

  const parts: string[] = [];
  // Beyond 999 crore the crore count itself needs the same scale (e.g. "One Thousand Crore").
  if (crore) parts.push(`${crore > 999 ? wholeNumberInWords(crore) : belowThousandInWords(crore)} Crore`);
  if (lakh) parts.push(`${belowThousandInWords(lakh)} Lakh`);
  if (thousand) parts.push(`${belowThousandInWords(thousand)} Thousand`);
  if (rest) parts.push(belowThousandInWords(rest));

  return parts.join(' ');
}

/**
 * Renders an amount the way it is written on a physical voucher, e.g.
 * `amountInWords(40000)` → "Taka Forty Thousand Only".
 */
export function amountInWords(amount: number, currency = 'Taka'): string {
  const safe = Math.max(0, Math.round(amount * 100) / 100);
  const whole = Math.floor(safe);
  const paisa = Math.round((safe - whole) * 100);

  const words = `${currency} ${wholeNumberInWords(whole)}`;
  return paisa > 0
    ? `${words} and ${belowThousandInWords(paisa)} Paisa Only`
    : `${words} Only`;
}

export function formatDate(date: string): string {
  return new Date(date).toLocaleDateString('en-BD', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

export function timeAgo(date: string): string {
  const diff = Date.now() - new Date(date).getTime();
  const mins = Math.floor(diff / 60_000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}
