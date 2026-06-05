import { useState } from "react";
import { ChevronDown, ChevronRight, User } from "lucide-react";
import clsx from "clsx";
import type { OrganogramNode } from "../../api/hr";

interface Props {
  nodes: OrganogramNode[];
  expandedIds: Set<string>;
  toggleExpand: (id: string) => void;
  highlightedId?: string;
  depth?: number;
}

export default function OrganogramTree({ nodes, expandedIds, toggleExpand, highlightedId, depth = 0 }: Props) {
  if (!nodes.length) return null;

  return (
    <ul className={clsx("flex justify-center", depth > 0 && "pt-8")}>
      {nodes.map((node, i) => (
        <li key={node.id} className="relative flex flex-col items-center px-3">
          <div className="relative before:absolute before:top-0 before:left-1/2 before:w-0 before:h-4 before:border-l-2 before:border-gray-300">
            <div
              className={clsx(
                "relative bg-white border-2 rounded-xl px-4 py-3 min-w-[180px] text-center shadow-sm hover:shadow-md transition-shadow cursor-pointer",
                highlightedId === node.id
                  ? "border-primary-500 ring-2 ring-primary-200"
                  : "border-gray-200"
              )}
            >
              <div className="flex items-center gap-2 mb-1 justify-center">
                <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-gray-500">
                  <User size={16} />
                </div>
                <div className="text-left">
                  <div className="text-sm font-semibold text-gray-900 leading-tight">{node.fullName}</div>
                  <div className="text-xs text-gray-500">{node.employeeCode}</div>
                </div>
              </div>
              <div className="text-xs text-primary-600 font-medium truncate">{node.designationName}</div>
              <div className="text-xs text-gray-400 truncate">{node.departmentName}</div>
              {node.hasChildren && (
                <button
                  onClick={(e) => { e.stopPropagation(); toggleExpand(node.id); }}
                  className="absolute -bottom-3 left-1/2 -translate-x-1/2 w-6 h-6 rounded-full bg-white border border-gray-300 flex items-center justify-center hover:bg-gray-50"
                >
                  {expandedIds.has(node.id) ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                </button>
              )}
            </div>
          </div>
          {node.hasChildren && expandedIds.has(node.id) && (
            <div className="relative mt-8">
              <div className="absolute top-0 left-1/2 -translate-x-1/2 w-px h-8 bg-gray-300" />
              <OrganogramTree
                nodes={node.children}
                expandedIds={expandedIds}
                toggleExpand={toggleExpand}
                highlightedId={highlightedId}
                depth={depth + 1}
              />
            </div>
          )}
        </li>
      ))}
    </ul>
  );
}
