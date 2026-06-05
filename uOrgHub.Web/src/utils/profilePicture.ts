export const DEFAULT_AVATAR = "/default-avatar.svg";

export function profilePictureUrl(path?: string | null): string {
  if (!path) return DEFAULT_AVATAR;
  if (path.startsWith("http://") || path.startsWith("https://")) return path;
  if (path.startsWith("/")) return path;
  return `/uploads/${path.replace(/^\/+/, "")}`;
}

export function getInitials(name: string): string {
  if (!name) return "U";
  return name
    .trim()
    .split(/\s+/)
    .map((p) => p[0])
    .filter(Boolean)
    .join("")
    .toUpperCase()
    .slice(0, 2);
}
