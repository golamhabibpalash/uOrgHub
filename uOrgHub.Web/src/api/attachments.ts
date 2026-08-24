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

const MAX_SIZE_BYTES = 2 * 1024 * 1024;

/** Mirrors the backend whitelist in SecureFileStorageOptions — reject obvious ones before upload. */
export const ALLOWED_EXTENSIONS = [
  ".jpg", ".jpeg", ".png", ".gif", ".webp",
  ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt",
];

export function validateAttachmentFile(file: File): string | null {
  if (file.size <= 0) return "The selected file is empty.";
  if (file.size > MAX_SIZE_BYTES) return "File exceeds the maximum size of 2 MB.";
  const ext = `.${file.name.split(".").pop()?.toLowerCase() ?? ""}`;
  if (!ALLOWED_EXTENSIONS.includes(ext)) return `File type '${ext}' is not allowed.`;
  return null;
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

/** Loads a file as an object URL for inline image preview. Caller must revoke it. */
export async function getAttachmentPreviewUrl(attachment: Attachment): Promise<string> {
  const { data } = await apiClient.get(`/attachments/${attachment.id}/download`, {
    params: { inline: true },
    responseType: "blob",
  });
  return URL.createObjectURL(data);
}
