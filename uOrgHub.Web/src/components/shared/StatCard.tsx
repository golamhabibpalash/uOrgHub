interface StatCardProps {
  label: string;
  value: string | number;
  sub?: string;
  subColor?: string;
}

export default function StatCard({
  label,
  value,
  sub,
  subColor = "text-gray-400",
}: StatCardProps) {
  return (
    <div className="bg-white border border-gray-200 rounded-xl p-4">
      <p className="text-xs text-gray-500 mb-1">{label}</p>
      <p className="text-2xl font-medium text-gray-900">{value}</p>
      {sub && <p className={`text-xs mt-1 ${subColor}`}>{sub}</p>}
    </div>
  );
}
