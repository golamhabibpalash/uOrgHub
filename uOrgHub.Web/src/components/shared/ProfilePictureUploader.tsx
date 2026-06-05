import { useRef, useState } from "react";
import { Camera, Trash2, Upload, X } from "lucide-react";
import Avatar from "./Avatar";
import { getInitials } from "../../utils/profilePicture";

const MAX_BYTES = 5 * 1024 * 1024;
const ALLOWED_TYPES = ["image/jpeg", "image/jpg", "image/png", "image/webp"];

interface ProfilePictureUploaderProps {
  currentPath?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  onUpload: (file: File) => Promise<string | void>;
  onDelete: () => Promise<void>;
  size?: "sm" | "md" | "lg" | "xl";
  disabled?: boolean;
}

export default function ProfilePictureUploader({
  currentPath,
  firstName,
  lastName,
  onUpload,
  onDelete,
  size = "xl",
  disabled = false,
}: ProfilePictureUploaderProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [dragOver, setDragOver] = useState(false);

  const validate = (file: File): string | null => {
    if (!ALLOWED_TYPES.includes(file.type))
      return "Unsupported file type. Allowed: JPEG, PNG, WEBP.";
    if (file.size > MAX_BYTES)
      return `File is too large. Maximum allowed size is ${MAX_BYTES / (1024 * 1024)} MB.`;
    return null;
  };

  const handleFile = async (file: File) => {
    setError(null);
    const v = validate(file);
    if (v) {
      setError(v);
      return;
    }
    setPreview(URL.createObjectURL(file));
    setUploading(true);
    try {
      await onUpload(file);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Upload failed.");
      setPreview(null);
    } finally {
      setUploading(false);
    }
  };

  const handleInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) void handleFile(file);
    e.target.value = "";
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setDragOver(false);
    if (disabled || uploading || deleting) return;
    const file = e.dataTransfer.files?.[0];
    if (file) void handleFile(file);
  };

  const handleDelete = async () => {
    if (disabled || deleting) return;
    setError(null);
    setDeleting(true);
    try {
      await onDelete();
      setPreview(null);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Delete failed.");
    } finally {
      setDeleting(false);
    }
  };

  const initials = getInitials(firstName, lastName);

  return (
    <div className="flex flex-col items-center gap-3">
      <div
        className={`group relative inline-block ${dragOver ? "ring-4 ring-indigo-300" : ""} rounded-full transition`}
        onDragOver={(e) => {
          e.preventDefault();
          if (!disabled) setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
      >
        {preview ? (
          <img
            src={preview}
            alt="Preview"
            className="rounded-full object-cover"
            style={{ width: 160, height: 160 }}
          />
        ) : currentPath ? (
          <Avatar
            src={currentPath}
            firstName={firstName}
            lastName={lastName}
            size={size}
            className="!w-40 !h-40 !text-3xl"
          />
        ) : (
          <div className="flex h-40 w-40 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-sky-500 text-4xl font-semibold text-white">
            {initials}
          </div>
        )}

        {uploading && (
          <div className="absolute inset-0 flex items-center justify-center rounded-full bg-black/50">
            <Upload className="h-6 w-6 animate-pulse text-white" />
          </div>
        )}

        {preview && !uploading && (
          <button
            type="button"
            onClick={() => setPreview(null)}
            className="absolute right-1 top-1 rounded-full bg-black/60 p-1 text-white hover:bg-black/80"
            title="Cancel preview"
          >
            <X className="h-3 w-3" />
          </button>
        )}
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <input
        ref={fileInputRef}
        type="file"
        accept={ALLOWED_TYPES.join(",")}
        onChange={handleInput}
        className="hidden"
        disabled={disabled || uploading || deleting}
      />

      <div className="flex flex-wrap items-center justify-center gap-2">
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled || uploading || deleting}
          className="inline-flex items-center gap-2 rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
        >
          <Camera className="h-4 w-4" />
          {currentPath ? "Change photo" : "Upload photo"}
        </button>

        {currentPath && !preview && (
          <button
            type="button"
            onClick={handleDelete}
            disabled={disabled || uploading || deleting}
            className="inline-flex items-center gap-2 rounded-md border border-red-200 bg-white px-3 py-1.5 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
          >
            <Trash2 className="h-4 w-4" />
            Remove
          </button>
        )}
      </div>

      <p className="text-xs text-gray-500">JPEG, PNG, WEBP · up to 5 MB · min 100×100px</p>
    </div>
  );
}
