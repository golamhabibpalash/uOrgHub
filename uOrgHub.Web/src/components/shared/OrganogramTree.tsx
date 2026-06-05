import { ChevronDown, ChevronUp, User } from "lucide-react";
import clsx from "clsx";
import type { OrganogramNode } from "../../api/hr";

interface Props {
  nodes: OrganogramNode[];
  expandedIds: Set<string>;
  toggleExpand: (id: string) => void;
  highlightedId?: string;
  onNodeClick?: (node: OrganogramNode) => void;
  depth?: number;
}

const statusColors: Record<string, string> = {
  Active: "bg-green-50 text-green-700 ring-1 ring-green-200",
  Inactive: "bg-gray-50 text-gray-500 ring-1 ring-gray-200",
  Terminated: "bg-red-50 text-red-600 ring-1 ring-red-200",
};

export default function OrganogramTree({
  nodes,
  expandedIds,
  toggleExpand,
  highlightedId,
  onNodeClick,
  depth = 0,
}: Props) {
  if (!nodes.length) return null;

  return (
    <div
      style={{
        display: "flex",
        alignItems: "flex-start",
        justifyContent: "center",
        gap: "1.5rem",
      }}
    >
      {nodes.map((node) => (
        <div
          key={node.id}
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            position: "relative",
          }}
        >
          {/* Vertical connector line going up to previous level's horizontal bar */}
          {depth > 0 && (
            <div
              style={{
                width: 0,
                height: "1.5rem",
                borderLeft: "2px solid #d1d5db",
                marginBottom: "0.25rem",
              }}
            />
          )}

          {/* Node card */}
          <div
            onClick={() => onNodeClick?.(node)}
            className={clsx(
              "relative bg-white border-2 rounded-xl px-4 py-3 min-w-[200px] text-center shadow-sm hover:shadow-md transition-all cursor-pointer group",
              highlightedId === node.id
                ? "border-primary-500 ring-2 ring-primary-200 shadow-lg"
                : "border-gray-200 hover:border-primary-300"
            )}
          >
            {/* Avatar + name row */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "0.75rem",
                marginBottom: "0.5rem",
              }}
            >
              <div
                style={{
                  width: "2.5rem",
                  height: "2.5rem",
                  borderRadius: "9999px",
                  backgroundColor: "#f3f4f6",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  color: "#9ca3af",
                  overflow: "hidden",
                  flexShrink: 0,
                }}
              >
                {node.profilePicturePath ? (
                  <img
                    src={node.profilePicturePath}
                    alt=""
                    style={{ width: "100%", height: "100%", objectFit: "cover" }}
                  />
                ) : (
                  <User size={18} />
                )}
              </div>
              <div style={{ textAlign: "left", minWidth: 0 }}>
                <div
                  style={{
                    fontSize: "0.875rem",
                    fontWeight: 600,
                    color: "#111827",
                    lineHeight: 1.25,
                    maxWidth: "140px",
                    overflow: "hidden",
                    textOverflow: "ellipsis",
                    whiteSpace: "nowrap",
                  }}
                >
                  {node.fullName}
                </div>
                <div style={{ fontSize: "0.75rem", color: "#9ca3af" }}>
                  {node.employeeCode}
                </div>
              </div>
            </div>

            {/* Designation */}
            <div
              style={{
                fontSize: "0.75rem",
                color: "#7c3aed",
                fontWeight: 500,
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
              }}
            >
              {node.designationName}
            </div>

            {/* Department */}
            <div
              style={{
                fontSize: "0.75rem",
                color: "#9ca3af",
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
              }}
            >
              {node.departmentName}
            </div>

            {/* Status badge */}
            <div style={{ marginTop: "0.375rem" }}>
              <span
                className={clsx(
                  "inline-block text-[10px] px-1.5 py-0.5 rounded-full font-medium",
                  statusColors[node.status] || "bg-gray-100 text-gray-500"
                )}
              >
                {node.status}
              </span>
            </div>

            {/* Expand/collapse toggle button */}
            {node.hasChildren && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  toggleExpand(node.id);
                }}
                style={{
                  position: "absolute",
                  bottom: "-0.75rem",
                  left: "50%",
                  transform: "translateX(-50%)",
                  width: "1.5rem",
                  height: "1.5rem",
                  borderRadius: "9999px",
                  backgroundColor: "white",
                  border: "1px solid #d1d5db",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  cursor: "pointer",
                  boxShadow: "0 1px 2px rgba(0,0,0,0.05)",
                  zIndex: 10,
                }}
              >
                {expandedIds.has(node.id) ? (
                  <ChevronUp size={14} />
                ) : (
                  <ChevronDown size={14} />
                )}
              </button>
            )}
          </div>

          {/* Children section */}
          {node.hasChildren && expandedIds.has(node.id) && (
            <div style={{ position: "relative", marginTop: "1.5rem" }}>
              {/* Vertical line going down from card center */}
              <div
                style={{
                  position: "absolute",
                  top: 0,
                  left: "50%",
                  width: 0,
                  height: "1.5rem",
                  borderLeft: "2px solid #d1d5db",
                }}
              />

              {/* Children row with horizontal connector bar at top */}
              <div style={{ position: "relative", marginTop: "1.5rem" }}>
                {/* Horizontal line spanning all children */}
                <div
                  style={{
                    position: "absolute",
                    top: 0,
                    left: 0,
                    right: 0,
                    height: 0,
                    borderTop: "2px solid #d1d5db",
                  }}
                />

                <OrganogramTree
                  nodes={node.children}
                  expandedIds={expandedIds}
                  toggleExpand={toggleExpand}
                  highlightedId={highlightedId}
                  onNodeClick={onNodeClick}
                  depth={depth + 1}
                />
              </div>
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
