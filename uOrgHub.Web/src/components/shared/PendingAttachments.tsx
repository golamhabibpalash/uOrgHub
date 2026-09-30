import { useCallback, useEffect, useState } from "react";
import toast from "react-hot-toast";
import { Eye, Paperclip, X } from "lucide-react";
import AttachmentDropZone, { FileTypeIcon } from "./AttachmentDropZone";
import AttachmentViewer from "./AttachmentViewer";
import {
  contentTypeForFile,
  formatFileSize,
  isPreviewable,
  validateAttachmentFile,
} from "../../api/attachments";

const MAX_FILES = 10;

interface PendingAttachmentsProps {
  files: File[];
  onChange: (files: File[]) => void;
  disabled?: boolean;
}

/**
 * Attachment picker for a record that doesn't exist yet: files are validated (type + size) and held
 * in memory, previewable locally, and the parent uploads them once the record has been saved and
 * has an id. Use AttachmentManager for records that already exist.
 */
export default function PendingAttachments({ files, onChange, disabled = false }: PendingAttachmentsProps) {
  const [preview, setPreview] = useState<{ file: File; url: string } | null>(null);

  const closePreview = useCallback(() => {
    setPreview((p) => {
      if (p) URL.revokeObjectURL(p.url);
      return null;
    });
  }, []);

  // Don't leak the object URL if the form unmounts while a preview is open.
  useEffect(() => () => closePreview(), [closePreview]);

  const add = (picked: File[]) => {
    const next = [...files];
    for (const file of picked) {
      const error = validateAttachmentFile(file);
      if (error) {
        toast.error(error);
        continue;
      }
      if (next.some((f) => f.name === file.name && f.size === file.size)) continue;
      if (next.length >= MAX_FILES) {
        toast.error(`You can attach up to ${MAX_FILES} files.`);
        break;
      }
      next.push(file);
    }
    onChange(next);
  };

  return (
    <div>
      <h3 className="flex items-center gap-2 text-sm font-medium text-gray-900 mb-3">
        <Paperclip size={15} className="text-gray-400" />
        Attachments
        {files.length > 0 && <span className="text-xs font-normal text-gray-400">({files.length})</span>}
      </h3>
      <AttachmentDropZone onFiles={add} disabled={disabled} />
      {files.length > 0 && (
        <ul className="mt-3 divide-y divide-gray-100 border border-gray-100 rounded-lg">
          {files.map((file, idx) => {
            const type = contentTypeForFile(file);
            return (
              <li key={`${file.name}-${file.size}`} className="px-3 py-2 flex items-center gap-3">
                <FileTypeIcon contentType={type} />
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-gray-800 truncate" title={file.name}>{file.name}</p>
                  <p className="text-xs text-gray-400">{formatFileSize(file.size)} • uploads when saved</p>
                </div>
                {isPreviewable(type) && (
                  <button
                    type="button"
                    title="View"
                    onClick={() => setPreview({ file, url: URL.createObjectURL(file) })}
                    className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-md"
                  >
                    <Eye size={14} />
                  </button>
                )}
                <button
                  type="button"
                  title="Remove"
                  disabled={disabled}
                  onClick={() => onChange(files.filter((_, i) => i !== idx))}
                  className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-md disabled:opacity-50"
                >
                  <X size={14} />
                </button>
              </li>
            );
          })}
        </ul>
      )}

      <AttachmentViewer
        url={preview?.url ?? null}
        fileName={preview?.file.name ?? ""}
        contentType={preview ? contentTypeForFile(preview.file) : ""}
        onClose={closePreview}
      />
    </div>
  );
}
