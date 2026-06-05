import { useState, useRef, useCallback, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  ZoomIn,
  ZoomOut,
  RotateCcw,
  ChevronsDownUp,
  ChevronsUpDown,
  Building2,
  Briefcase,
  Search,
  Maximize2,
  Minimize2,
  X,
  User,
} from "lucide-react";
import clsx from "clsx";
import { getOrganogram, getAllDepartments, getAllDesignations, getEmployeeById } from "../../api/hr";
import OrganogramTree from "../../components/shared/OrganogramTree";
import Modal from "../../components/shared/Modal";
import Avatar from "../../components/shared/Avatar";

import type { OrganogramNode, Employee } from "../../api/hr";

export default function Organogram() {
  const [deptFilter, setDeptFilter] = useState("");
  const [desigFilter, setDesigFilter] = useState("");
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [scale, setScale] = useState(1);
  const [search, setSearch] = useState("");
  const [highlightedId, setHighlightedId] = useState<string | undefined>();
  const [fullScreen, setFullScreen] = useState(false);
  const [selectedNode, setSelectedNode] = useState<OrganogramNode | null>(null);
  const [detailData, setDetailData] = useState<Employee | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const { data: orgData, isLoading } = useQuery({
    queryKey: ["organogram", deptFilter, desigFilter],
    queryFn: () => getOrganogram({
      departmentId: deptFilter || undefined,
      designationId: desigFilter || undefined,
    }),
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

  // Flatten all nodes for search
  const flattenNodes = useCallback((items: typeof nodes): OrganogramNode[] => {
    return items.flatMap((n) => [n, ...flattenNodes(n.children)]);
  }, []);

  const allNodes = flattenNodes(nodes);

  // Search logic: find matching node IDs and highlight the first match
  const searchResults = search.trim()
    ? allNodes.filter(
        (n) =>
          n.fullName.toLowerCase().includes(search.toLowerCase()) ||
          n.employeeCode.toLowerCase().includes(search.toLowerCase())
      )
    : [];

  const handleSearchSelect = (id: string) => {
    setHighlightedId(id);
    // Expand all ancestors to reveal the highlighted node
    const expandAncestors = (items: typeof nodes, targetId: string, ancestors: string[]): string[] => {
      for (const item of items) {
        if (item.id === targetId) return ancestors;
        const found = expandAncestors(item.children, targetId, [...ancestors, item.id]);
        if (found.length > 0) return found;
      }
      return [];
    };
    const path = expandAncestors(nodes, id, []);
    setExpandedIds((prev) => {
      const next = new Set(prev);
      path.forEach((pid) => next.add(pid));
      next.add(id);
      return next;
    });
    setSearch("");
  };

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

  const toggleFullScreen = () => {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
      setFullScreen(true);
    } else {
      document.exitFullscreen();
      setFullScreen(false);
    }
  };

  useEffect(() => {
    const handler = () => setFullScreen(!!document.fullscreenElement);
    document.addEventListener("fullscreenchange", handler);
    return () => document.removeEventListener("fullscreenchange", handler);
  }, []);

  // Handle node click — load employee detail
  const handleNodeClick = async (node: OrganogramNode) => {
    setSelectedNode(node);
    setDetailData(null);
    setDetailLoading(true);
    try {
      const res = await getEmployeeById(node.id);
      setDetailData(res.data.data);
    } catch {
      setDetailData(null);
    } finally {
      setDetailLoading(false);
    }
  };

  return (
    <div className={clsx("flex flex-col", fullScreen ? "h-screen" : "h-[calc(100vh-4rem)]")}>
      {/* Toolbar */}
      <div className="bg-white border-b border-gray-200 px-6 py-3 flex items-center justify-between shrink-0 gap-4">
        <h1 className="text-lg font-semibold text-gray-900 shrink-0">Organizational Chart</h1>

        {/* Search */}
        <div className="relative flex-1 max-w-sm">
          <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Search by name or employee ID..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-3 py-1.5 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-1 focus:ring-primary-500"
          />
          {search && searchResults.length > 0 && (
            <div className="absolute top-full mt-1 left-0 right-0 bg-white border border-gray-200 rounded-lg shadow-lg z-50 max-h-60 overflow-y-auto">
              {searchResults.slice(0, 10).map((n) => (
                <button
                  key={n.id}
                  onClick={() => handleSearchSelect(n.id)}
                  className="w-full text-left px-3 py-2 text-sm hover:bg-gray-50 flex items-center gap-2"
                >
                  <User size={14} className="text-gray-400 shrink-0" />
                  <div className="min-w-0">
                    <div className="font-medium text-gray-900 truncate">{n.fullName}</div>
                    <div className="text-xs text-gray-500">{n.employeeCode} &middot; {n.designationName}</div>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-sm">
            <Building2 size={15} className="text-gray-400 shrink-0" />
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
            <Briefcase size={15} className="text-gray-400 shrink-0" />
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

          {highlightedId && (
            <button
              onClick={() => setHighlightedId(undefined)}
              className="text-xs text-primary-600 hover:text-primary-700 flex items-center gap-1"
            >
              <X size={14} />
              Clear highlight
            </button>
          )}

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
          <div className="w-px h-6 bg-gray-200" />
          <button
            onClick={toggleFullScreen}
            className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500"
            title={fullScreen ? "Exit Full Screen" : "Full Screen"}
          >
            {fullScreen ? <Minimize2 size={16} /> : <Maximize2 size={16} />}
          </button>
        </div>
      </div>

      {/* Tree area */}
      <div ref={containerRef} className="flex-1 overflow-auto bg-gray-50">
        {isLoading ? (
          <div className="flex items-center justify-center h-full text-gray-400 text-sm">Loading organizational chart...</div>
        ) : nodes.length === 0 ? (
          <div className="flex items-center justify-center h-full text-gray-400 text-sm">No employees found.</div>
        ) : (
          <div
            className="inline-flex p-8 min-w-full"
            style={{ transform: `scale(${scale})`, transformOrigin: "top center" }}
          >
            <OrganogramTree
              nodes={nodes}
              expandedIds={expandedIds}
              toggleExpand={(id) =>
                setExpandedIds((prev) => {
                  const next = new Set(prev);
                  next.has(id) ? next.delete(id) : next.add(id);
                  return next;
                })
              }
              highlightedId={highlightedId}
              onNodeClick={handleNodeClick}
            />
          </div>
        )}
      </div>

      {/* Employee detail modal */}
      <Modal
        title="Employee Details"
        open={!!selectedNode}
        onClose={() => setSelectedNode(null)}
      >
        {detailLoading ? (
          <div className="py-8 text-center text-sm text-gray-400">Loading details...</div>
        ) : detailData ? (
          <div className="space-y-4">
            {/* Header */}
            <div className="flex items-center gap-4">
              <div className="shrink-0">
                <Avatar
                  src={detailData.profilePicturePath}
                  firstName={detailData.firstName}
                  lastName={detailData.lastName}
                  size="xl"
                />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">
                  {detailData.firstName} {detailData.lastName}
                </h3>
                <p className="text-sm text-gray-500">{detailData.employeeCode}</p>
                <div className="flex items-center gap-2 mt-1">
                  <span className={clsx(
                    "inline-block text-xs px-2 py-0.5 rounded-full font-medium",
                    detailData.status === "Active"
                      ? "bg-green-50 text-green-700 ring-1 ring-green-200"
                      : detailData.status === "Inactive"
                      ? "bg-gray-50 text-gray-500 ring-1 ring-gray-200"
                      : "bg-red-50 text-red-600 ring-1 ring-red-200"
                  )}>
                    {detailData.status}
                  </span>
                </div>
              </div>
            </div>

            <div className="border-t border-gray-100" />

            {/* Details grid */}
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <span className="text-gray-400 block">Designation</span>
                <span className="text-gray-900 font-medium">{detailData.designationName}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Department</span>
                <span className="text-gray-900 font-medium">{detailData.departmentName}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Email</span>
                <span className="text-gray-900">{detailData.email}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Phone</span>
                <span className="text-gray-900">{detailData.phone || "—"}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Manager</span>
                <span className="text-gray-900">{detailData.managerName || "—"}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Joining Date</span>
                <span className="text-gray-900">{detailData.joiningDate?.split("T")[0] || "—"}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Employment Type</span>
                <span className="text-gray-900">{detailData.employmentType}</span>
              </div>
              <div>
                <span className="text-gray-400 block">Basic Salary</span>
                <span className="text-gray-900">{detailData.basicSalary?.toLocaleString() || "—"}</span>
              </div>
            </div>
          </div>
        ) : (
          <div className="py-8 text-center text-sm text-gray-400">Failed to load employee details.</div>
        )}
      </Modal>
    </div>
  );
}
