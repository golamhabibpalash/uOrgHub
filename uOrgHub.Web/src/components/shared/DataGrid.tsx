import { ArrowUpDown, ArrowUp, ArrowDown, Search, ChevronLeft, ChevronRight, List, Eye } from "lucide-react";

export interface DataGridColumn<T> {
  key: string;
  label: string;
  sortable?: boolean;
  render?: (row: T) => React.ReactNode;
  className?: string;
  headerClassName?: string;
  width?: string;
}

interface DataGridProps<T> {
  columns: DataGridColumn<T>[];
  data: T[];
  loading?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  onSort?: (column: string) => void;
  search?: string;
  onSearch?: (value: string) => void;
  searchPlaceholder?: string;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  pageSize: number;
  onPageSizeChange?: (size: number) => void;
  totalCount: number;
  onView?: (row: T) => void;
  onEdit?: (row: T) => void;
  onDelete?: (row: T) => void;
  actions?: React.ReactNode;
  emptyMessage?: string;
  /** Optional toolbar content rendered before search */
  toolbarPrefix?: React.ReactNode;
  /** Optional filter bar rendered below toolbar */
  filterBar?: React.ReactNode;
}

function SortIcon({ column, sortBy, sortDescending }: { column: string; sortBy?: string; sortDescending?: boolean }) {
  if (sortBy !== column) return <ArrowUpDown size={13} className="text-gray-300 group-hover:text-gray-400" />;
  return sortDescending ? <ArrowDown size={13} className="text-primary-500" /> : <ArrowUp size={13} className="text-primary-500" />;
}

export default function DataGrid<T extends { id: string }>({
  columns,
  data,
  loading,
  sortBy,
  sortDescending,
  onSort,
  search,
  onSearch,
  searchPlaceholder = "Search...",
  page,
  totalPages,
  onPageChange,
  pageSize,
  onPageSizeChange,
  totalCount,
  onView,
  onEdit,
  onDelete,
  actions,
  emptyMessage = "No records found",
  toolbarPrefix,
  filterBar,
}: DataGridProps<T>) {
  return (
    <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
      {/* Toolbar */}
      <div className="px-4 py-3 border-b border-gray-100 flex items-center justify-between gap-3">
        <div className="flex items-center gap-3 flex-1">
          {toolbarPrefix}
          {onSearch && (
            <div className="relative flex-1 max-w-xs">
              <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder={searchPlaceholder}
                value={search ?? ""}
                onChange={(e) => onSearch(e.target.value)}
                className="w-full text-sm border border-gray-200 rounded-lg pl-9 pr-3 py-1.5 focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
          )}
          {filterBar}
        </div>
        <div className="flex items-center gap-2">
          {actions}
        </div>
      </div>

      {/* Loading overlay / table */}
      <div className="relative">
        {loading && (
          <div className="absolute inset-0 bg-white/60 z-10 flex items-center justify-center">
            <div className="flex items-center gap-2 text-sm text-gray-400">
              <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
              Loading...
            </div>
          </div>
        )}
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                {columns.map((col) => (
                  <th
                    key={col.key}
                    className={`text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200 ${col.headerClassName ?? ""}`}
                    style={col.width ? { width: col.width } : undefined}
                  >
                    {col.sortable !== false && onSort ? (
                      <button
                        onClick={() => onSort(col.key)}
                        className="group inline-flex items-center gap-1.5 hover:text-gray-700"
                      >
                        {col.label}
                        <SortIcon column={col.key} sortBy={sortBy} sortDescending={sortDescending} />
                      </button>
                    ) : (
                      col.label
                    )}
                  </th>
                ))}
                {(onView || onEdit || onDelete) && (
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">
                    Actions
                  </th>
                )}
              </tr>
            </thead>
            <tbody>
              {data.length === 0 && !loading ? (
                <tr>
                  <td
                    colSpan={columns.length + (onView || onEdit || onDelete ? 1 : 0)}
                    className="text-center py-16 text-gray-400 text-sm"
                  >
                    <div className="flex flex-col items-center gap-2">
                      <List size={32} className="text-gray-200" />
                      {emptyMessage}
                    </div>
                  </td>
                </tr>
              ) : (
                data.map((row) => (
                  <tr
                    key={row.id}
                    className="border-t border-gray-100 hover:bg-gray-50 transition-colors"
                  >
                    {columns.map((col) => (
                      <td
                        key={col.key}
                        className={`px-4 py-2.5 text-gray-700 ${col.className ?? ""}`}
                      >
                        {col.render ? col.render(row) : String((row as Record<string, unknown>)[col.key] ?? "")}
                      </td>
                    ))}
                    {(onView || onEdit || onDelete) && (
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-3">
                          {onView && (
                            <button
                              onClick={() => onView(row)}
                              className="text-gray-400 hover:text-primary-600"
                              title="View details"
                            >
                              <Eye size={15} />
                            </button>
                          )}
                          {onEdit && (
                            <button
                              onClick={() => onEdit(row)}
                              className="text-gray-400 hover:text-primary-600"
                              title="Edit"
                            >
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/></svg>
                            </button>
                          )}
                          {onDelete && (
                            <button
                              onClick={() => onDelete(row)}
                              className="text-gray-400 hover:text-red-500"
                              title="Delete"
                            >
                              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
                            </button>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between px-4 py-3 border-t border-gray-100">
        <div className="flex items-center gap-2 text-xs text-gray-400">
          <span>{totalCount} record{totalCount !== 1 ? "s" : ""}</span>
          {onPageSizeChange && (
            <>
              <span className="text-gray-200">|</span>
              <span>Show</span>
              <select
                value={pageSize}
                onChange={(e) => onPageSizeChange(Number(e.target.value))}
                className="border border-gray-200 rounded px-1.5 py-0.5 text-xs focus:outline-none focus:ring-1 focus:ring-primary-500"
              >
                {[10, 25, 50, 100].map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </>
          )}
        </div>
        {totalPages > 1 && (
          <div className="flex items-center gap-2">
            <p className="text-xs text-gray-400">
              Page {page} of {totalPages}
            </p>
            <button
              disabled={page <= 1}
              onClick={() => onPageChange(page - 1)}
              className="p-1 border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50"
            >
              <ChevronLeft size={14} />
            </button>
            <button
              disabled={page >= totalPages}
              onClick={() => onPageChange(page + 1)}
              className="p-1 border border-gray-200 rounded-md disabled:opacity-40 hover:bg-gray-50"
            >
              <ChevronRight size={14} />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
