interface SkeletonCardProps {
  height?: string;
  width?: string;
  rounded?: string;
  className?: string;
}

export default function SkeletonCard({
  height = 'h-20',
  width = 'w-full',
  rounded = 'rounded-xl',
  className = '',
}: SkeletonCardProps) {
  return (
    <div className={`animate-pulse bg-gray-200 ${height} ${width} ${rounded} ${className}`} />
  );
}
