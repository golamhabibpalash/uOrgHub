import { useState } from "react";

interface TruncatedTextProps {
  text?: string | null;
  /** Max characters shown before truncating. Default 50. */
  limit?: number;
  className?: string;
}

/**
 * Renders `text` in full when short. When it exceeds `limit` characters it shows
 * a clipped version followed by a "Read more" toggle. Hovering the toggle reveals
 * the complete text as a native tooltip; clicking it expands/collapses inline.
 */
export default function TruncatedText({ text, limit = 50, className = "" }: TruncatedTextProps) {
  const [expanded, setExpanded] = useState(false);
  const value = text ?? "";

  if (!value) return <span className={`text-gray-400 ${className}`}>—</span>;

  if (value.length <= limit) {
    return <span className={`text-sm text-gray-700 ${className}`}>{value}</span>;
  }

  return (
    <span className={`text-sm text-gray-700 ${className}`}>
      {expanded ? value : `${value.slice(0, limit).trimEnd()}… `}
      <button
        type="button"
        title={value}
        onClick={() => setExpanded((v) => !v)}
        className="text-primary-600 hover:text-primary-700 hover:underline text-xs font-medium whitespace-nowrap"
      >
        {expanded ? "Show less" : "Read more"}
      </button>
    </span>
  );
}
