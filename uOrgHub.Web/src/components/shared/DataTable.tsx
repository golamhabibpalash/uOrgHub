interface Column<T> {
  key: keyof T | string;
  label: string;
  render?: (row: T) => React.ReactNode;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  onEdit?: (row: T) => void;
  onDelete?: (row: T) => void;
}

import { Pencil, Trash2 } from "lucide-react";

export default function DataTable<T extends { id: string }>({
  columns,
  data,
  loading,
  onEdit,
  onDelete,
}: DataTableProps<T>) {
  if (loading)
    return (
      <div className="flex items-center justify-center h-40 text-sm text-gray-400">
        Loading...
      </div>
    );

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="bg-gray-50">
            {columns.map((col) => (
              <th
                key={String(col.key)}
                className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200"
              >
                {col.label}
              </th>
            ))}
            {(onEdit || onDelete) && (
              <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 border-b border-gray-200">
                Actions
              </th>
            )}
          </tr>
        </thead>
        <tbody>
          {data.length === 0 ? (
            <tr>
              <td
                colSpan={columns.length + 1}
                className="text-center py-10 text-gray-400 text-sm"
              >
                No records found
              </td>
            </tr>
          ) : (
            data.map((row) => (
              <tr
                key={row.id}
                className="border-t border-gray-100 hover:bg-gray-50"
              >
                {columns.map((col) => (
                  <td
                    key={String(col.key)}
                    className="px-4 py-2.5 text-gray-700"
                  >
                    {col.render
                      ? col.render(row)
                      : String(row[col.key as keyof T] ?? "")}
                  </td>
                ))}
                {(onEdit || onDelete) && (
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-3">
                      {onEdit && (
                        <button
                          onClick={() => onEdit(row)}
                          className="text-gray-400 hover:text-primary-600"
                        >
                          <Pencil size={15} />
                        </button>
                      )}
                      {onDelete && (
                        <button
                          onClick={() => onDelete(row)}
                          className="text-gray-400 hover:text-red-500"
                        >
                          <Trash2 size={15} />
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
  );
}
