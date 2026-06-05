import { useRef, useState } from "react";
import { Trash2, Upload, X } from "lucide-react";
import Avatar from "./Avatar";
import { profilePictureUrl } from "../../utils/profilePicture";

interface ProfilePictureUploaderProps {
  currentPath?: string | null;
  name: string;
  uploading: boolean;
  deleting: boolean;
  onUpload: (file: File) => void;
  onDelete: () => void;
  size?: "lg" | "xl" | "2xl";
  hint?: string;
}

const ACCEPT = "image/jpeg,image/jpg,image/png,image/webp";
const MAX_BYTES = 5 * 1024 * 1024;

export default function ProfilePictureUploader({
  currentPath,
  name,
  uploading,
  deleting,
  onUpload,
  onDelete,
  size = "2xl",
  hint,
}: ProfilePictureUploaderProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [error, setError] = useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const handlePick = () => {
    setError(null);
    fileInputRef.current?.click();
  };

  const handleFile = (file: File | null | undefined) => {
    if (!file) return;
    setError(null);
    if (!ACCEPT.split(",").includes(file.type)) {
      setError("Please choose a JPG, PNG, or WEBP image.");
      return;
    }
    if (file.size > MAX_BYTES) {
      setError("Image is too large. Maximum size is 5 MB.");
      return;
    }
    const objectUrl = URL.createObjectURL(file);
    setPreviewUrl(objectUrl);
    onUpload(file);
  };

  const reset = () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    setPreviewUrl(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const displaySrc = previewUrl ?? (currentPath ? profilePictureUrl(currentPath) : undefined);

  return (
    <div className="flex items-start gap-4">
      <div className="relative">
        <Avatar src={displaySrc ?? undefined} name={name} size={size} ring />
        {(uploading || deleting) && (
          <div className="absolute inset-0 rounded-full bg-black/40 flex items-center justify-center">
            <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
          </div>
        )}
      </div>

      <div className="flex-1 min-w-0">
        <input
          ref={fileInputRef}
          type="file"
          accept={ACCEPT}
          className="hidden"
          onChange={(e) => handleFile(e.target.files?.[0])}
        />

        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={handlePick}
            disabled={uploading || deleting}
            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg bg-primary-500 text-white hover:bg-primary-600 disabled:opacity-50 transition-colors"
          >
            <Upload size={13} />
            {currentPath ? "Replace" : "Upload Photo"}
          </button>

          {currentPath && !previewUrl && (
            <button
              type="button"
              onClick={onDelete}
              disabled={uploading || deleting}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border border-red-200 text-red-600 hover:bg-red-50 disabled:opacity-50 transition-colors"
            >
              <Trash2 size={13} />
              Remove
            </button>
          )}

          {previewUrl && (
            <button
              type="button"
              onClick={reset}
              disabled={uploading || deleting}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-50 transition-colors"
            >
              <X size={13} />
              Cancel
            </button>
          )}
        </div>

        <p className="text-[11px] text-gray-400 mt-2 leading-relaxed">
          {hint ?? "JPG, PNG, or WEBP. Maximum 5 MB. Recommended: square image, at least 200×200 px."}
        </p>

        {error && (
          <p className="text-[11px] text-red-600 mt-1.5 bg-red-50 border border-red-200 rounded px-2 py-1 inline-block">
            {error}
          </p>
        )}
      </div>
    </div>
  );
}
