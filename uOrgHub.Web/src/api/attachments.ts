import apiClient from "./client";

export interface Attachment {
  id: string;
  entityType: string;
  entityId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  description?: string | null;
  uploadedBy: string;
  uploadedAt: string;
}

/** Mirrors SecureFileStorageOptions.MaxFileSizeBytes on the backend. */
export const MAX_ATTACHMENT_SIZE_BYTES = 2 * 1024 * 1024;

/** Mirrors the backend whitelist in SecureFileStorageOptions — reject obvious ones before upload. */
export const ALLOWED_EXTENSIONS = [
  ".jpg", ".jpeg", ".png", ".gif", ".webp",
  ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt",
];

/** Value for a file input's `accept` attribute. */
export const ATTACHMENT_ACCEPT = ALLOWED_EXTENSIONS.join(",");

export function formatFileSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function validateAttachmentFile(file: File): string | null {
  if (file.size <= 0) return `"${file.name}" is empty.`;
  if (file.size > MAX_ATTACHMENT_SIZE_BYTES)
    return `"${file.name}" is ${formatFileSize(file.size)} — the maximum is ${formatFileSize(MAX_ATTACHMENT_SIZE_BYTES)}.`;
  const ext = `.${file.name.split(".").pop()?.toLowerCase() ?? ""}`;
  if (!ALLOWED_EXTENSIONS.includes(ext)) return `"${file.name}": file type '${ext}' is not allowed.`;
  return null;
}

/** Images and PDFs render in the in-app viewer; everything else is download-only. */
export function isPreviewable(contentType: string) {
  return contentType.startsWith("image/") || contentType === "application/pdf";
}

/**
 * Content type for a local (not yet uploaded) file. Browsers sometimes leave File.type empty, so
 * fall back to the extension — same mapping the backend uses when it stores the file.
 */
export function contentTypeForFile(file: File): string {
  if (file.type) return file.type;
  const ext = file.name.split(".").pop()?.toLowerCase();
  switch (ext) {
    case "jpg":
    case "jpeg": return "image/jpeg";
    case "png": return "image/png";
    case "gif": return "image/gif";
    case "webp": return "image/webp";
    case "pdf": return "application/pdf";
    default: return "application/octet-stream";
  }
}

export async function getAttachments(entityType: string, entityId: string): Promise<Attachment[]> {
  const { data } = await apiClient.get("/attachments", { params: { entityType, entityId } });
  return data.data;
}

export async function uploadAttachment(
  file: File,
  entityType: string,
  entityId: string,
  description?: string
): Promise<Attachment> {
  const form = new FormData();
  form.append("file", file);
  form.append("EntityType", entityType);
  form.append("EntityId", entityId);
  if (description) form.append("Description", description);

  const { data } = await apiClient.post("/attachments", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data.data;
}

export const deleteAttachment = (id: string) => apiClient.delete(`/attachments/${id}`);

/** Streams the file through the authorized endpoint and triggers a browser download. */
export async function downloadAttachment(attachment: Attachment) {
  const { data } = await apiClient.get(`/attachments/${attachment.id}/download`, {
    responseType: "blob",
  });
  const url = URL.createObjectURL(data);
  const link = document.createElement("a");
  link.href = url;
  link.download = attachment.fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

/** Loads a file as an object URL for in-app image/PDF preview. Caller must revoke it. */
export async function getAttachmentPreviewUrl(attachment: Attachment): Promise<string> {
  const { data } = await apiClient.get<Blob>(`/attachments/${attachment.id}/download`, {
    params: { inline: true },
    responseType: "blob",
  });
  // Re-wrap with the recorded type so the browser's PDF viewer kicks in even if a proxy
  // rewrote the response's Content-Type.
  return URL.createObjectURL(new Blob([data], { type: attachment.contentType }));
}
