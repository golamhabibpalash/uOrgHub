import { useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import {
  Download,
  Eye,
  File as FileIcon,
  FileImage,
  FileText,
  Loader2,
  Paperclip,
  Trash2,
  Upload,
} from "lucide-react";
import Modal from "./Modal";
import ConfirmDialog from "./ConfirmDialog";
import {
  deleteAttachment,
  downloadAttachment,
  getAttachmentPreviewUrl,
  getAttachments,
  uploadAttachment,
  validateAttachmentFile,
  type Attachment,
} from "../../api/attachments";
import { formatDate } from "../../utils/format";
import { extractApiError } from "../../utils/apiError";

const MAX_SIZE_BYTES = 2 * 1024 * 1024;

function formatFileSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function fileIcon(contentType: string) {
  const cls = "w-8 h-8 rounded-lg flex items-center justify-center shrink-0 bg-gray-100 text-gray-500";
  if (contentType.startsWith("image/")) {
    return (
      <span className={cls}>
        <FileImage size={16} />
      </span>
    );
  }
  if (contentType === "application/pdf") {
    return (
      <span className={`${cls} bg-red-50 text-red-500`}>
        <FileText size={16} />
      </span>
    );
  }
  return (
    <span className={cls}>
      <FileIcon size={16} />
    </span>
  );
}

interface AttachmentManagerProps {
  /** Registered target type, e.g. "Voucher" — must match a backend attachment target. */
  entityType: string;
  entityId: string;
  /** Whether upload/delete controls are shown; the parent gates this on claims. */
  canEdit?: boolean;
}

/**
 * Reusable attach-files panel for any record that accepts attachments. Lists, previews, downloads
 * and removes files through the shared /attachments endpoints; the backend enforces the real
 * permission checks against the owning record's claims.
 */
export default function AttachmentManager({ entityType, entityId, canEdit = false }: AttachmentManagerProps) {
  const qc = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [previewing, setPreviewing] = useState<{ attachment: Attachment; url: string } | null>(null);
  const [deleting, setDeleting] = useState<Attachment | null>(null);

  const queryKey = ["attachments", entityType, entityId];

  const { data: attachments = [], isLoading } = useQuery({
    queryKey,
    queryFn: () => getAttachments(entityType, entityId),
    enabled: Boolean(entityId),
  });

  const invalidate = () => qc.invalidateQueries({ queryKey });

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => uploadAttachment(file, entityType, entityId),
    onSuccess: () => {
      invalidate();
      toast.success("Attachment uploaded.");
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => {
      await deleteAttachment(id);
    },
    onSuccess: () => {
      invalidate();
      setDeleting(null);
    },
    onError: (err) => toast.error(extractApiError(err)),
  });

  const handleFileSelected = (file: File | undefined) => {
    if (!file) return;
    const error = validateAttachmentFile(file);
    if (error) {
      toast.error(error);
      return;
    }
    uploadMutation.mutate(file);
    // Reset so picking the same file again still fires onChange.
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const openPreview = async (attachment: Attachment) => {
    try {
      const url = await getAttachmentPreviewUrl(attachment);
      setPreviewing({ attachment, url });
    } catch (err) {
      toast.error(extractApiError(err));
    }
  };

  const closePreview = () => {
    if (previewing) URL.revokeObjectURL(previewing.url);
    setPreviewing(null);
  };

  return (
    <div className="bg-white border border-gray-200 rounded-xl p-5">
      <div className="flex items-center justify-between mb-3">
        <h3 className="flex items-center gap-2 text-sm font-medium text-gray-900">
          <Paperclip size={15} className="text-gray-400" />
          Attachments
          {!isLoading && attachments.length > 0 && (
            <span className="text-xs font-normal text-gray-400">({attachments.length})</span>
          )}
        </h3>
        {canEdit && (
          <>
            <input
              ref={fileInputRef}
              type="file"
              hidden
              accept={["image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf",
                ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt"].join(",")}
              onChange={(e) => handleFileSelected(e.target.files?.[0])}
            />
            <button
              onClick={() => fileInputRef.current?.click()}
              disabled={uploadMutation.isPending}
              className="flex items-center gap-1.5 text-xs text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:border-primary-500 hover:text-primary-600 transition-colors disabled:opacity-50"
            >
              {uploadMutation.isPending ? (
                <Loader2 size={13} className="animate-spin" />
              ) : (
                <Upload size={13} />
              )}
              Attach file
            </button>
          </>
        )}
      </div>

      <p className="text-xs text-gray-400 -mt-2 mb-3">
        Images or documents up to {formatFileSize(MAX_SIZE_BYTES)} each.
      </p>

      {isLoading ? (
        <p className="text-sm text-gray-400 py-2">Loading…</p>
      ) : attachments.length === 0 ? (
        <p className="text-sm text-gray-400 py-2">No files attached.</p>
      ) : (
        <ul className="divide-y divide-gray-100">
          {attachments.map((a) => (
            <li key={a.id} className="py-2.5 flex items-center gap-3">
              {fileIcon(a.contentType)}
              <div className="min-w-0 flex-1">
                <p className="text-sm text-gray-800 truncate">{a.fileName}</p>
                <p className="text-xs text-gray-400">
                  {formatFileSize(a.fileSizeBytes)} • {a.uploadedBy || "—"} • {formatDate(a.uploadedAt)}
                  {a.description ? ` • ${a.description}` : ""}
                </p>
              </div>
              <div className="flex items-center gap-1 shrink-0">
                {a.contentType.startsWith("image/") && (
                  <button
                    onClick={() => openPreview(a)}
                    title="Preview"
                    className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
                  >
                    <Eye size={14} />
                  </button>
                )}
                <button
                  onClick={() => downloadAttachment(a).catch((err) => toast.error(extractApiError(err)))}
                  title="Download"
                  className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
                >
                  <Download size={14} />
                </button>
                {canEdit && (
                  <button
                    onClick={() => setDeleting(a)}
                    title="Delete"
                    className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors"
                  >
                    <Trash2 size={14} />
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}

      <Modal title={previewing?.attachment.fileName ?? ""} open={previewing !== null} onClose={closePreview}>
        {previewing && (
          <img src={previewing.url} alt={previewing.attachment.fileName} className="max-w-full max-h-[70vh] mx-auto" />
        )}
      </Modal>

      <ConfirmDialog
        open={deleting !== null}
        title="Delete attachment"
        message={`Delete "${deleting?.fileName}"? This cannot be undone.`}
        confirmLabel="Delete"
        tone="danger"
        loading={deleteMutation.isPending}
        onCancel={() => setDeleting(null)}
        onConfirm={() => deleting && deleteMutation.mutate(deleting.id)}
      />
    </div>
  );
}
