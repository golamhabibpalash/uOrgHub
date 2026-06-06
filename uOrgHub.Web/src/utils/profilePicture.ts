export const DEFAULT_AVATAR = "/default-avatar.svg";

export function profilePictureUrl(path: string | null | undefined): string {
  if (!path) return DEFAULT_AVATAR;
  if (path.startsWith("http://") || path.startsWith("https://") || path.startsWith("data:")) return path;
  const clean = path.replace(/\\/g, "/")
    .replace(/^\/?(uploads\/)?/i, "");
  const ts = new Date().getTime();
  return `/uploads/${clean}?t=${ts}`;
}

export function profileThumbnailUrl(path: string | null | undefined): string {
  if (!path) return DEFAULT_AVATAR;
  if (path.startsWith("http://") || path.startsWith("https://") || path.startsWith("data:")) return path;
  const clean = path.replace(/\\/g, "/")
    .replace(/^\/?(uploads\/)?/i, "")
    .replace(/_display\./, "_thumb.");
  const ts = new Date().getTime();
  return `/uploads/${clean}?t=${ts}`;
}

export function getInitials(firstName?: string | null, lastName?: string | null): string {
  const first = (firstName ?? "").trim();
  const last = (lastName ?? "").trim();
  if (!first && !last) return "?";
  const a = first[0] ?? "";
  const b = last[0] ?? "";
  return (a + b).toUpperCase() || "?";
}
