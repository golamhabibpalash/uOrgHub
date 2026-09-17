/** Label/value pair used across detail pages (mirrors VoucherDetail.tsx's local Field). */
export default function Field({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-xs text-gray-400">{label}</p>
      <p className="text-sm text-gray-800 mt-0.5">{value ?? "—"}</p>
    </div>
  );
}
