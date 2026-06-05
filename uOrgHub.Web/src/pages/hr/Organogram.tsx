import { useState, useRef, useCallback, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { ZoomIn, ZoomOut, RotateCcw, ChevronsDownUp, ChevronsUpDown, Building2, Briefcase } from "lucide-react";
import { getOrganogram, getAllDepartments, getAllDesignations } from "../../api/hr";
import OrganogramTree from "../../components/shared/OrganogramTree";

export default function Organogram() {
  const [deptFilter, setDeptFilter] = useState("");
  const [desigFilter, setDesigFilter] = useState("");
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [scale, setScale] = useState(1);
  const containerRef = useRef<HTMLDivElement>(null);

  const { data: orgData, isLoading } = useQuery({
    queryKey: ["organogram", deptFilter, desigFilter],
    queryFn: () => getOrganogram({ departmentId: deptFilter || undefined, designationId: desigFilter || undefined }),
  });

  const { data: deptData } = useQuery({
    queryKey: ["departments-all"],
    queryFn: () => getAllDepartments(),
  });

  const { data: desigData } = useQuery({
    queryKey: ["designations-all"],
    queryFn: () => getAllDesignations(),
  });

  const nodes = orgData?.data?.data ?? [];

  const expandAll = useCallback(() => {
    const collect = (items: typeof nodes): string[] =>
      items.flatMap((n) => n.hasChildren ? [n.id, ...collect(n.children)] : []);
    setExpandedIds(new Set(collect(nodes)));
  }, [nodes]);

  const collapseAll = useCallback(() => {
    setExpandedIds(new Set());
  }, []);

  useEffect(() => {
    if (nodes.length > 0) expandAll();
  }, [nodes.length, expandAll]);

  const zoomIn = () => setScale((s) => Math.min(s + 0.1, 2));
  const zoomOut = () => setScale((s) => Math.max(s - 0.1, 0.3));
  const resetZoom = () => setScale(1);

  return (
    <div className="flex flex-col h-[calc(100vh-4rem)]">
      <div className="bg-white border-b border-gray-200 px-6 py-3 flex items-center justify-between shrink-0">
        <h1 className="text-lg font-semibold text-gray-900">Organizational Chart</h1>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-sm">
            <Building2 size={15} className="text-gray-400" />
            <select
              value={deptFilter}
              onChange={(e) => setDeptFilter(e.target.value)}
              className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              <option value="">All Departments</option>
              {(deptData?.data?.data ?? []).map((d: { id: string; name: string }) => (
                <option key={d.id} value={d.id}>{d.name}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2 text-sm">
            <Briefcase size={15} className="text-gray-400" />
            <select
              value={desigFilter}
              onChange={(e) => setDesigFilter(e.target.value)}
              className="border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
            >
              <option value="">All Designations</option>
              {(desigData?.data?.data ?? []).map((d: { id: string; name: string }) => (
                <option key={d.id} value={d.id}>{d.name}</option>
              ))}
            </select>
          </div>
          <div className="w-px h-6 bg-gray-200" />
          <button onClick={collapseAll} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500" title="Collapse All">
            <ChevronsDownUp size={16} />
          </button>
          <button onClick={expandAll} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500" title="Expand All">
            <ChevronsUpDown size={16} />
          </button>
          <div className="w-px h-6 bg-gray-200" />
          <button onClick={zoomOut} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500" title="Zoom Out">
            <ZoomOut size={16} />
          </button>
          <span className="text-xs text-gray-500 w-10 text-center">{Math.round(scale * 100)}%</span>
          <button onClick={zoomIn} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500" title="Zoom In">
            <ZoomIn size={16} />
          </button>
          <button onClick={resetZoom} className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500" title="Reset Zoom">
            <RotateCcw size={16} />
          </button>
        </div>
      </div>

      <div ref={containerRef} className="flex-1 overflow-auto bg-gray-50">
        {isLoading ? (
          <div className="flex items-center justify-center h-full text-gray-400">Loading...</div>
        ) : nodes.length === 0 ? (
          <div className="flex items-center justify-center h-full text-gray-400">No employees found.</div>
        ) : (
          <div
            className="inline-flex p-8 min-w-full"
            style={{ transform: `scale(${scale})`, transformOrigin: "top center" }}
          >
            <OrganogramTree
              nodes={nodes}
              expandedIds={expandedIds}
              toggleExpand={(id) => setExpandedIds((prev) => {
                const next = new Set(prev);
                next.has(id) ? next.delete(id) : next.add(id);
                return next;
              })}
              highlightedId={undefined}
            />
          </div>
        )}
      </div>
    </div>
  );
}
