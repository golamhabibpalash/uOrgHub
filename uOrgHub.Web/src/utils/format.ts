export function formatBDT(amount: number): string {
  if (amount >= 10_000_000) return `৳ ${(amount / 10_000_000).toFixed(1)} Cr`;
  if (amount >= 100_000) return `৳ ${(amount / 100_000).toFixed(1)} L`;
  if (amount >= 1_000) return `৳ ${(amount / 1_000).toFixed(1)}K`;
  return `৳ ${amount.toLocaleString()}`;
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
