import { useState } from "react";
import { DEFAULT_AVATAR, getInitials, profilePictureUrl } from "../../utils/profilePicture";

type AvatarSize = "xs" | "sm" | "md" | "lg" | "xl";

const SIZE_CLASSES: Record<AvatarSize, string> = {
  xs: "w-6 h-6 text-[10px]",
  sm: "w-8 h-8 text-xs",
  md: "w-10 h-10 text-sm",
  lg: "w-14 h-14 text-base",
  xl: "w-24 h-24 text-2xl",
};

interface AvatarProps {
  src?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  size?: AvatarSize;
  className?: string;
}

export default function Avatar({
  src,
  firstName,
  lastName,
  size = "md",
  className = "",
}: AvatarProps) {
  const [errored, setErrored] = useState(false);
  const url = !errored ? profilePictureUrl(src ?? null) : DEFAULT_AVATAR;

  return (
    <div
      className={`${SIZE_CLASSES[size]} ${className} relative inline-flex shrink-0 items-center justify-center overflow-hidden rounded-full bg-gradient-to-br from-indigo-500 to-sky-500 text-white font-semibold`}
    >
      <img
        src={url}
        alt={firstName ? `${firstName} ${lastName ?? ""}`.trim() : "User"}
        className="h-full w-full object-cover"
        onError={() => setErrored(true)}
      />
    </div>
  );
}

export { DEFAULT_AVATAR, getInitials, profilePictureUrl };
