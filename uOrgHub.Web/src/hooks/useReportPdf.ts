import { useCallback, useState } from "react";
import apiClient from "../api/client";
import toast from "react-hot-toast";

interface ReportPdfParams {
  url: string;
  params?: object;
  filename?: string;
}

/** Downloads a server-generated report PDF as a blob. Mirrors useExport's blob-download pattern,
 * but points at a per-report `/pdf` endpoint instead of a generic `{baseUrl}/export`. */
export function useReportPdf() {
  const [isDownloading, setIsDownloading] = useState(false);

  const downloadPdf = useCallback(async ({ url, params = {}, filename }: ReportPdfParams) => {
    setIsDownloading(true);
    try {
      const response = await apiClient.get(url, { params, responseType: "blob" });

      const blob = new Blob([response.data], { type: "application/pdf" });
      const objectUrl = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = objectUrl;
      const disposition = response.headers["content-disposition"];
      const match = disposition?.match(/filename="?(.+?)"?$/);
      a.download = match?.[1] ?? filename ?? "report.pdf";
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(objectUrl);
    } catch {
      toast.error("PDF download failed. Please try again.");
    } finally {
      setIsDownloading(false);
    }
  }, []);

  return { downloadPdf, isDownloading };
}
