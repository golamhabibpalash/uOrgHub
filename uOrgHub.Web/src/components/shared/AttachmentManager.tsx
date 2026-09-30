import { useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { Download, Eye, Paperclip, Trash2 } from "lucide-react";
import ConfirmDialog from "./ConfirmDialog";
import AttachmentViewer from "./AttachmentViewer";
import AttachmentDropZone, { FileTypeIcon } from "./AttachmentDropZone";
import {
  deleteAttachment,
  downloadAttachment,
  formatFileSize,
  getAttachmentPreviewUrl,
  getAttachments,
  isPreviewable,
  uploadAttachment,
  validateAttachmentFile,
  type Attachment,
} from "../../api/attachments";
import { formatDate } from "../../utils/format";
import { extractApiError } from "../../utils/apiError";

interface AttachmentManagerProps {
  /** Registered target type, e.g. "Voucher" — must match a backend attachment target. */
  entityType: string;
  entityId: string;
  /** Whether upload/delete controls are shown; the parent gates this on claims. */
  canEdit?: boolean;
  /** Drop the card chrome when embedding inside another panel or dialog. */
  bare?: boolean;
}

/**
 * Reusable attach-files panel for any record that accepts attachments. Lists, previews (images and
 * PDFs, in-app), downloads and removes files through the shared /attachments endpoints; the backend
 * enforces the real permission checks against the owning record's claims.
 */
export default function AttachmentManager({ entityType, entityId, canEdit = false, bare = false }: AttachmentManagerProps) {
  const qc = useQueryClient();
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
    // Sequential so one rejected file doesn't abort the rest; report each failure by name.
    mutationFn: async (files: File[]) => {
      let uploaded = 0;
      for (const file of files) {
        try {
          await uploadAttachment(file, entityType, entityId);
          uploaded++;
        } catch (err) {
          toast.error(`${file.name}: ${extractApiError(err)}`);
        }
      }
      return uploaded;
    },
    onSuccess: (uploaded) => {
      invalidate();
      if (uploaded > 0) toast.success(uploaded === 1 ? "Attachment uploaded." : `${uploaded} attachments uploaded.`);
    },
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

  const handleFiles = (files: File[]) => {
    const valid = files.filter((f) => {
      const error = validateAttachmentFile(f);
      if (error) toast.error(error);
      return !error;
    });
    if (valid.length > 0) uploadMutation.mutate(valid);
  };

  const openPreview = async (attachment: Attachment) => {
    try {
      const url = await getAttachmentPreviewUrl(attachment);
      setPreviewing({ attachment, url });
    } catch (err) {
      toast.error(extractApiError(err));
    }
  };

  const closePreview = useCallback(() => {
    setPreviewing((p) => {
      if (p) URL.revokeObjectURL(p.url);
      return null;
    });
  }, []);

  const download = (a: Attachment) => downloadAttachment(a).catch((err) => toast.error(extractApiError(err)));

  return (
    <div className={bare ? "" : "bg-white border border-gray-200 rounded-xl p-5"}>
      <h3 className="flex items-center gap-2 text-sm font-medium text-gray-900 mb-3">
        <Paperclip size={15} className="text-gray-400" />
        Attachments
        {!isLoading && attachments.length > 0 && (
          <span className="text-xs font-normal text-gray-400">({attachments.length})</span>
        )}
      </h3>

      {canEdit && (
        <div className="mb-3">
          <AttachmentDropZone onFiles={handleFiles} busy={uploadMutation.isPending} />
        </div>
      )}

      {isLoading ? (
        <p className="text-sm text-gray-400 py-2">Loading…</p>
      ) : attachments.length === 0 ? (
        <p className="text-sm text-gray-400 py-2">No files attached.</p>
      ) : (
        <ul className="divide-y divide-gray-100 border border-gray-100 rounded-lg">
          {attachments.map((a) => (
            <li key={a.id} className="px-3 py-2.5 flex items-center gap-3">
              <FileTypeIcon contentType={a.contentType} />
              <div className="min-w-0 flex-1">
                {isPreviewable(a.contentType) ? (
                  <button
                    type="button"
                    onClick={() => openPreview(a)}
                    className="block max-w-full text-sm text-gray-800 truncate hover:text-primary-600 hover:underline text-left"
                    title={`View ${a.fileName}`}
                  >
                    {a.fileName}
                  </button>
                ) : (
                  <p className="text-sm text-gray-800 truncate" title={a.fileName}>{a.fileName}</p>
                )}
                <p className="text-xs text-gray-400">
                  {formatFileSize(a.fileSizeBytes)} • {a.uploadedBy || "—"} • {formatDate(a.uploadedAt)}
                  {a.description ? ` • ${a.description}` : ""}
                </p>
              </div>
              <div className="flex items-center gap-1 shrink-0">
                {isPreviewable(a.contentType) && (
                  <button
                    type="button"
                    onClick={() => openPreview(a)}
                    title="View"
                    className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
                  >
                    <Eye size={14} />
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => download(a)}
                  title="Download"
                  className="p-1.5 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors"
                >
                  <Download size={14} />
                </button>
                {canEdit && (
                  <button
                    type="button"
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

      <AttachmentViewer
        url={previewing?.url ?? null}
        fileName={previewing?.attachment.fileName ?? ""}
        contentType={previewing?.attachment.contentType ?? ""}
        onClose={closePreview}
        onDownload={previewing ? () => download(previewing.attachment) : undefined}
      />

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
