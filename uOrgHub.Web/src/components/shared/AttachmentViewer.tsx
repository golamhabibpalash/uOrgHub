import { useEffect } from "react";
import { createPortal } from "react-dom";
import { Download, ExternalLink, X } from "lucide-react";

interface AttachmentViewerProps {
  /** Object URL of the file (from a fetched blob or a local File); the caller owns revoking it. */
  url: string | null;
  fileName: string;
  contentType: string;
  onClose: () => void;
  onDownload?: () => void;
}

/**
 * Full-screen in-app viewer for images and PDFs, so users can read a receipt or bill scan without
 * downloading it. Portaled to <body> because the shared Modal applies a CSS transform, which would
 * otherwise trap this fixed overlay inside the parent dialog's box when opened from within one.
 */
export default function AttachmentViewer({ url, fileName, contentType, onClose, onDownload }: AttachmentViewerProps) {
  useEffect(() => {
    if (!url) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.stopPropagation();
        onClose();
      }
    };
    window.addEventListener("keydown", onKey, true);
    return () => window.removeEventListener("keydown", onKey, true);
  }, [url, onClose]);

  if (!url) return null;

  const isImage = contentType.startsWith("image/");
  const isPdf = contentType === "application/pdf";

  return createPortal(
    <div className="fixed inset-0 z-[60] flex flex-col bg-black/80" onClick={onClose}>
      <div
        className="flex items-center gap-3 px-4 py-3 text-white bg-black/40 shrink-0"
        onClick={(e) => e.stopPropagation()}
      >
        <p className="text-sm font-medium truncate flex-1" title={fileName}>{fileName}</p>
        <a
          href={url}
          target="_blank"
          rel="noreferrer"
          title="Open in new tab"
          className="p-1.5 rounded-md hover:bg-white/10"
        >
          <ExternalLink size={16} />
        </a>
        {onDownload && (
          <button onClick={onDownload} title="Download" className="p-1.5 rounded-md hover:bg-white/10">
            <Download size={16} />
          </button>
        )}
        <button onClick={onClose} title="Close (Esc)" className="p-1.5 rounded-md hover:bg-white/10">
          <X size={18} />
        </button>
      </div>
      <div className="flex-1 min-h-0 flex items-center justify-center p-4">
        {isImage ? (
          <img
            src={url}
            alt={fileName}
            onClick={(e) => e.stopPropagation()}
            className="max-w-full max-h-full object-contain rounded shadow-lg bg-white"
          />
        ) : isPdf ? (
          <iframe
            src={url}
            title={fileName}
            onClick={(e) => e.stopPropagation()}
            className="w-full h-full max-w-5xl rounded bg-white shadow-lg"
          />
        ) : (
          <p className="text-sm text-white/80">No preview available for this file type.</p>
        )}
      </div>
    </div>,
    document.body,
  );
}
