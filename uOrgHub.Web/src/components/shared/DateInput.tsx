import { useEffect, useRef, useState } from "react";
import { Calendar as CalendarIcon, ChevronLeft, ChevronRight } from "lucide-react";

type DateInputMode = "date" | "datetime";

interface DateInputProps {
  /**
   * ISO value — `yyyy-mm-dd` for `mode="date"`, `yyyy-mm-ddThh:mm` for `mode="datetime"`.
   * Same contract as native date/datetime-local inputs.
   */
  value?: string;
  /** Emits a change event shaped like a native input so `e.target.value` works unchanged. */
  onChange: (e: { target: { value: string; name?: string } }) => void;
  className?: string;
  placeholder?: string;
  disabled?: boolean;
  name?: string;
  title?: string;
  required?: boolean;
  mode?: DateInputMode;
}

const MONTHS = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

const DAY_NAMES = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];

function isoToDisplay(iso: string): string {
  if (!iso) return "";
  const [y, m, d] = iso.split("T")[0].split("-");
  if (!y || !m || !d) return "";
  return `${d}/${m}/${y}`;
}

function valueToDisplay(value: string, mode: DateInputMode): string {
  if (!value) return "";
  const [datePart, timePart] = value.split("T");
  const d = isoToDisplay(datePart);
  if (mode === "datetime" && timePart) return `${d} ${timePart.slice(0, 5)}`;
  return d;
}

function maskInput(raw: string, mode: DateInputMode): string {
  const digits = raw.replace(/\D/g, "").slice(0, mode === "datetime" ? 12 : 8);
  if (digits.length <= 2) return digits;
  if (digits.length <= 4) return `${digits.slice(0, 2)}/${digits.slice(2)}`;
  if (digits.length <= 8) return `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4)}`;
  const date = `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4, 8)}`;
  if (mode === "date") return date;
  if (digits.length <= 10) return `${date} ${digits.slice(8)}`;
  return `${date} ${digits.slice(8, 10)}:${digits.slice(10)}`;
}

function isValidInput(input: string, mode: DateInputMode): boolean {
  const m = input.match(/^(\d{2})\/(\d{2})\/(\d{4})/);
  if (!m) return false;
  const d = Number(m[1]);
  const mo = Number(m[2]);
  const y = Number(m[3]);
  if (mo < 1 || mo > 12) return false;
  if (d < 1 || d > new Date(y, mo, 0).getDate()) return false;
  if (mode === "datetime") {
    const t = input.slice(11);
    const tm = t.match(/^(\d{2}):(\d{2})$/);
    if (!tm) return false;
    const hh = Number(tm[1]);
    const mm = Number(tm[2]);
    return hh <= 23 && mm <= 59;
  }
  return true;
}

function displayToValue(input: string, mode: DateInputMode): string {
  const m = input.match(/^(\d{2})\/(\d{2})\/(\d{4})(?: (\d{2}):(\d{2}))?$/);
  if (!m) return "";
  const iso = `${m[3]}-${m[2]}-${m[1]}`;
  if (mode === "datetime" && m[4]) return `${iso}T${m[4]}:${m[5]}`;
  return iso;
}

function toIso(y: number, mo: number, d: number): string {
  return `${y}-${String(mo + 1).padStart(2, "0")}-${String(d).padStart(2, "0")}`;
}

export default function DateInput({
  value,
  onChange,
  className,
  placeholder = "dd/mm/yyyy",
  disabled = false,
  name,
  title,
  required,
  mode = "date",
}: DateInputProps) {
  const [display, setDisplay] = useState(() => valueToDisplay(value ?? "", mode));
  const [open, setOpen] = useState(false);
  const [viewDate, setViewDate] = useState(() => {
    const [y, m, d] = (value ?? "").split("T")[0].split("-").map(Number);
    if (y && m) return new Date(y, m - 1, d || 1);
    return new Date();
  });
  const focusedRef = useRef(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!focusedRef.current) setDisplay(valueToDisplay(value ?? "", mode));
  }, [value, mode]);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const commit = (v: string) => onChange({ target: { value: v, name: name ?? "" } });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const masked = maskInput(e.target.value, mode);
    setDisplay(masked);
    if (masked === "") {
      commit("");
    } else if (isValidInput(masked, mode)) {
      commit(displayToValue(masked, mode));
    }
  };

  const handleDayClick = (dateIso: string, day: Date) => {
    const existingTime =
      mode === "datetime" && value ? value.split("T")[1]?.slice(0, 5) ?? "00:00" : "";
    const out = mode === "datetime" ? `${dateIso}T${existingTime}` : dateIso;
    setDisplay(valueToDisplay(out, mode));
    setViewDate(day);
    commit(out);
    setOpen(false);
  };

  const year = viewDate.getFullYear();
  const month = viewDate.getMonth();
  const firstDay = new Date(year, month, 1).getDay();
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const selectedIso = (value ?? "").split("T")[0];
  const today = new Date();

  return (
    <div className="relative" ref={containerRef}>
      <input
        type="text"
        inputMode="numeric"
        name={name}
        title={title}
        required={required}
        placeholder={mode === "datetime" ? "dd/mm/yyyy hh:mm" : placeholder}
        value={display}
        disabled={disabled}
        onFocus={() => { focusedRef.current = true; }}
        onBlur={() => {
          focusedRef.current = false;
          setDisplay(valueToDisplay(value ?? "", mode));
        }}
        onChange={handleChange}
        onKeyDown={(e) => {
          if (e.key === "Escape" || e.key === "Tab") setOpen(false);
        }}
        className={className}
      />
      <button
        type="button"
        tabIndex={-1}
        disabled={disabled}
        onClick={() => setOpen((p) => !p)}
        className="absolute right-2 top-1/2 -translate-y-1/2 p-1 rounded hover:bg-gray-100 text-gray-400 disabled:opacity-50"
      >
        <CalendarIcon size={15} />
      </button>

      {open && (
        <div className="absolute z-50 mt-1 w-64 bg-white border border-gray-200 rounded-lg shadow-lg p-3">
          <div className="flex items-center justify-between mb-2">
            <button
              type="button"
              onClick={() => setViewDate(new Date(year, month - 1, 1))}
              className="p-1 rounded hover:bg-gray-100 text-gray-500"
            >
              <ChevronLeft size={14} />
            </button>
            <span className="text-sm font-medium text-gray-800">
              {MONTHS[month]} {year}
            </span>
            <button
              type="button"
              onClick={() => setViewDate(new Date(year, month + 1, 1))}
              className="p-1 rounded hover:bg-gray-100 text-gray-500"
            >
              <ChevronRight size={14} />
            </button>
          </div>

          <div className="grid grid-cols-7 gap-1 text-center text-[11px] font-medium text-gray-400 mb-1">
            {DAY_NAMES.map((d) => (
              <div key={d}>{d}</div>
            ))}
          </div>

          <div className="grid grid-cols-7 gap-1">
            {Array.from({ length: firstDay }).map((_, i) => (
              <div key={`empty-${i}`} />
            ))}
            {Array.from({ length: daysInMonth }).map((_, i) => {
              const d = i + 1;
              const iso = toIso(year, month, d);
              const isSelected = selectedIso === iso;
              const isToday =
                today.getFullYear() === year &&
                today.getMonth() === month &&
                today.getDate() === d;
              return (
                <button
                  key={d}
                  type="button"
                  onClick={() => handleDayClick(iso, new Date(year, month, d))}
                  className={`h-7 w-7 rounded-md text-xs transition-colors ${
                    isSelected
                      ? "bg-primary-500 text-white font-medium"
                      : "text-gray-700 hover:bg-gray-100"
                  } ${isToday && !isSelected ? "ring-1 ring-primary-300" : ""}`}
                >
                  {d}
                </button>
              );
            })}
          </div>

          <button
            type="button"
            onClick={() => { commit(""); setDisplay(""); setOpen(false); }}
            className="mt-2 w-full text-xs text-red-500 hover:bg-red-50 rounded-md py-1"
          >
            Clear
          </button>
        </div>
      )}
    </div>
  );
}
