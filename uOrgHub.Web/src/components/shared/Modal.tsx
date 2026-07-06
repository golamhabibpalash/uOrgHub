import { X } from "lucide-react";
import { useState, useRef, useEffect } from "react";

interface ModalProps {
  title: string;
  open: boolean;
  onClose: () => void;
  children: React.ReactNode;
  size?: "sm" | "md" | "lg" | "xl" | "2xl" | "3xl" | "4xl" | "5xl";
  draggable?: boolean;
}

const sizeClasses: Record<string, string> = {
  sm: "max-w-sm",
  md: "max-w-md",
  lg: "max-w-lg",
  xl: "max-w-xl",
  "2xl": "max-w-2xl",
  "3xl": "max-w-3xl",
  "4xl": "max-w-4xl",
  "5xl": "max-w-5xl",
};

export default function Modal({ title, open, onClose, children, size = "lg", draggable = true }: ModalProps) {
  const [offset, setOffset] = useState({ x: 0, y: 0 });
  const drag = useRef({ isDragging: false, startX: 0, startY: 0, offsetX: 0, offsetY: 0 });

  useEffect(() => { if (open) setOffset({ x: 0, y: 0 }); }, [open]);

  useEffect(() => {
    if (!draggable) return;
    function onMouseMove(e: MouseEvent) {
      if (!drag.current.isDragging) return;
      setOffset({
        x: drag.current.offsetX + (e.clientX - drag.current.startX),
        y: drag.current.offsetY + (e.clientY - drag.current.startY),
      });
    }
    function onMouseUp() {
      if (!drag.current.isDragging) return;
      drag.current.isDragging = false;
      document.body.style.userSelect = "";
      document.body.style.cursor = "";
    }
    window.addEventListener("mousemove", onMouseMove);
    window.addEventListener("mouseup", onMouseUp);
    return () => {
      window.removeEventListener("mousemove", onMouseMove);
      window.removeEventListener("mouseup", onMouseUp);
    };
  }, [draggable]);

  useEffect(() => () => { document.body.style.userSelect = ""; document.body.style.cursor = ""; }, []);

  if (!open) return null;

  function handleMouseDown(e: React.MouseEvent) {
    if (!draggable || e.button !== 0) return;
    drag.current = { isDragging: true, startX: e.clientX, startY: e.clientY, offsetX: offset.x, offsetY: offset.y };
    document.body.style.userSelect = "none";
    document.body.style.cursor = "grabbing";
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div
        className={`bg-white rounded-xl shadow-lg w-full mx-4 flex flex-col max-h-[85vh] ${sizeClasses[size]}`}
        style={draggable ? { transform: `translate(${offset.x}px, ${offset.y}px)` } : undefined}
      >
        <div
          className={`flex items-center justify-between px-5 py-4 border-b border-gray-100 shrink-0 ${draggable ? "cursor-grab active:cursor-grabbing select-none" : ""}`}
          onMouseDown={handleMouseDown}
        >
          <h2 className="text-sm font-medium text-gray-900">{title}</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600"><X size={16} /></button>
        </div>
        <div className="p-5 overflow-y-auto">{children}</div>
      </div>
    </div>
  );
}
