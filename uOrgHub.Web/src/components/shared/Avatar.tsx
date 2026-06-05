import { useState } from "react";
import { profilePictureUrl, getInitials, DEFAULT_AVATAR } from "../../utils/profilePicture";

interface AvatarProps {
  src?: string | null;
  name: string;
  size?: "xs" | "sm" | "md" | "lg" | "xl" | "2xl";
  className?: string;
  ring?: boolean;
}

const SIZE_CLASSES: Record<NonNullable<AvatarProps["size"]>, string> = {
  xs: "w-6 h-6 text-[10px]",
  sm: "w-8 h-8 text-xs",
  md: "w-10 h-10 text-sm",
  lg: "w-14 h-14 text-lg",
  xl: "w-20 h-20 text-2xl",
  "2xl": "w-28 h-28 text-3xl",
};

const RING_CLASSES = "ring-2 ring-white/20";

export default function Avatar({ src, name, size = "md", className = "", ring = false }: AvatarProps) {
  const [errored, setErrored] = useState(false);
  const url = src ? profilePictureUrl(src) : null;
  const showImg = url && !errored && url !== DEFAULT_AVATAR;

  return (
    <div
      className={`${SIZE_CLASSES[size]} rounded-full overflow-hidden shrink-0 flex items-center justify-center font-semibold text-white ${
        ring ? RING_CLASSES : ""
      } ${className}`}
      style={{
        background: showImg
          ? "transparent"
          : "linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #a855f7 100%)",
      }}
    >
      {showImg ? (
        <img
          src={url}
          alt={name}
          className="w-full h-full object-cover"
          onError={() => setErrored(true)}
          loading="lazy"
        />
      ) : (
        <span>{getInitials(name)}</span>
      )}
    </div>
  );
}
