import { useRef, useState } from "react";
import { File as FileIcon, FileImage, FileText, Loader2, UploadCloud } from "lucide-react";
import { ATTACHMENT_ACCEPT, MAX_ATTACHMENT_SIZE_BYTES, formatFileSize } from "../../api/attachments";

export function FileTypeIcon({ contentType }: { contentType: string }) {
  const base = "w-9 h-9 rounded-lg flex items-center justify-center shrink-0";
  if (contentType.startsWith("image/")) {
    return <span className={`${base} bg-blue-50 text-blue-500`}><FileImage size={16} /></span>;
  }
  if (contentType === "application/pdf") {
    return <span className={`${base} bg-red-50 text-red-500`}><FileText size={16} /></span>;
  }
  return <span className={`${base} bg-gray-100 text-gray-500`}><FileIcon size={16} /></span>;
}

interface AttachmentDropZoneProps {
  onFiles: (files: File[]) => void;
  busy?: boolean;
  disabled?: boolean;
}

/** Click-or-drop area for picking one or more files. Validation is left to the caller. */
export default function AttachmentDropZone({ onFiles, busy = false, disabled = false }: AttachmentDropZoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragOver, setDragOver] = useState(false);
  const inactive = busy || disabled;

  const emit = (list: FileList | null) => {
    const files = Array.from(list ?? []);
    if (files.length > 0) onFiles(files);
    // Reset so picking the same file again still fires onChange.
    if (inputRef.current) inputRef.current.value = "";
  };

  return (
    <div
      role="button"
      tabIndex={inactive ? -1 : 0}
      onClick={() => !inactive && inputRef.current?.click()}
      onKeyDown={(e) => {
        if (!inactive && (e.key === "Enter" || e.key === " ")) {
          e.preventDefault();
          inputRef.current?.click();
        }
      }}
      onDragOver={(e) => {
        e.preventDefault();
        if (!inactive) setDragOver(true);
      }}
      onDragLeave={() => setDragOver(false)}
      onDrop={(e) => {
        e.preventDefault();
        setDragOver(false);
        if (!inactive) emit(e.dataTransfer.files);
      }}
      className={`flex items-center gap-3 border-2 border-dashed rounded-lg px-4 py-3 transition-colors ${
        inactive
          ? "border-gray-200 bg-gray-50 cursor-not-allowed opacity-60"
          : dragOver
            ? "border-primary-500 bg-primary-50 cursor-copy"
            : "border-gray-200 hover:border-primary-400 hover:bg-gray-50 cursor-pointer"
      }`}
    >
      <input
        ref={inputRef}
        type="file"
        multiple
        hidden
        accept={ATTACHMENT_ACCEPT}
        onChange={(e) => emit(e.target.files)}
      />
      {busy ? (
        <Loader2 size={20} className="text-primary-500 animate-spin shrink-0" />
      ) : (
        <UploadCloud size={20} className="text-gray-400 shrink-0" />
      )}
      <div className="min-w-0">
        <p className="text-sm text-gray-700">
          {busy ? "Uploading…" : (
            <>
              <span className="text-primary-600 font-medium">Click to choose</span> or drag files here
            </>
          )}
        </p>
        <p className="text-xs text-gray-400">
          PDF, images (JPG, PNG, GIF, WEBP), Word, Excel, CSV or TXT — max {formatFileSize(MAX_ATTACHMENT_SIZE_BYTES)} each
        </p>
      </div>
    </div>
  );
}
