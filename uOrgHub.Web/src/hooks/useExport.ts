import { useCallback, useState } from "react";
import apiClient from "../api/client";
import toast from "react-hot-toast";

interface ExportParams {
  baseUrl: string;
  format?: "xlsx" | "csv";
  filters?: Record<string, unknown>;
}

export function useExport() {
  const [isExporting, setIsExporting] = useState(false);

  const exportData = useCallback(async ({ baseUrl, format = "xlsx", filters = {} }: ExportParams) => {
    setIsExporting(true);
    try {
      const response = await apiClient.get(`${baseUrl}/export`, {
        params: { format, ...filters },
        responseType: "blob",
      });

      const blob = new Blob([response.data]);
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      const disposition = response.headers["content-disposition"];
      const match = disposition?.match(/filename="?(.+?)"?$/);
      a.download = match?.[1] ?? `export.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {
      toast.error("Export failed. Please try again.");
    } finally {
      setIsExporting(false);
    }
  }, []);

  return { exportData, isExporting };
}
