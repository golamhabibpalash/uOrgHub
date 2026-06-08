import { useState, useRef, useEffect, useCallback, useMemo } from "react";
import { ChevronDown, X, Search, Loader2 } from "lucide-react";

export interface SelectOption {
  value: string;
  label: string;
  searchText?: string;
}

interface SearchableDropdownProps {
  options?: SelectOption[];
  value?: string;
  onChange: (value: string | undefined) => void;
  placeholder?: string;
  searchPlaceholder?: string;
  loading?: boolean;
  disabled?: boolean;
  clearable?: boolean;
  creatable?: boolean;
  onCreate?: (label: string) => void;
  noResultsMessage?: string;
  className?: string;
  label?: string;
  error?: string;
  required?: boolean;
  searchFields?: string[];
}

export default function SearchableDropdown({
  options = [],
  value,
  onChange,
  placeholder = "Select...",
  searchPlaceholder = "Search...",
  loading = false,
  disabled = false,
  clearable = true,
  creatable = false,
  onCreate,
  noResultsMessage = "No results found",
  className = "",
  label,
  error,
  required = false,
}: SearchableDropdownProps) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [highlightedIndex, setHighlightedIndex] = useState(-1);
  const containerRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  const selectedOption = useMemo(
    () => options.find((o) => o.value === value),
    [options, value],
  );

  const filtered = useMemo(() => {
    if (!search) return options;
    const q = search.toLowerCase();
    return options.filter((o) => {
      if (o.searchText) return o.searchText.toLowerCase().includes(q);
      return o.label.toLowerCase().includes(q);
    });
  }, [options, search]);

  const showCreate = creatable && search.trim() && !filtered.some((o) => o.label.toLowerCase() === search.trim().toLowerCase());

  const close = useCallback(() => {
    setOpen(false);
    setSearch("");
    setHighlightedIndex(-1);
  }, []);

  useEffect(() => {
    if (open && searchInputRef.current) {
      searchInputRef.current.focus();
    }
  }, [open]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        close();
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [close]);

  const handleSelect = useCallback(
    (optionValue: string | undefined) => {
      onChange(optionValue);
      close();
    },
    [onChange, close],
  );

  const handleClear = useCallback(
    (e: React.MouseEvent) => {
      e.stopPropagation();
      onChange(undefined);
    },
    [onChange],
  );

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (!open) {
        if (e.key === "Enter" || e.key === "ArrowDown") {
          setOpen(true);
          e.preventDefault();
        }
        return;
      }

      const items = showCreate ? [...filtered, { value: "__create__", label: search }] : filtered;

      switch (e.key) {
        case "ArrowDown":
          e.preventDefault();
          setHighlightedIndex((i) => (i < items.length - 1 ? i + 1 : 0));
          break;
        case "ArrowUp":
          e.preventDefault();
          setHighlightedIndex((i) => (i > 0 ? i - 1 : items.length - 1));
          break;
        case "Enter":
          e.preventDefault();
          if (highlightedIndex >= 0 && highlightedIndex < items.length) {
            const item = items[highlightedIndex];
            if (item.value === "__create__") {
              onCreate?.(search);
            } else {
              handleSelect(item.value);
            }
          }
          break;
        case "Escape":
          e.preventDefault();
          close();
          break;
        case "Tab":
          close();
          break;
      }
    },
    [open, filtered, showCreate, search, highlightedIndex, handleSelect, onCreate, close],
  );

  useEffect(() => {
    if (listRef.current && highlightedIndex >= 0) {
      const el = listRef.current.children[highlightedIndex] as HTMLElement;
      el?.scrollIntoView({ block: "nearest" });
    }
  }, [highlightedIndex]);

  return (
    <div className={className} ref={containerRef}>
      {label && (
        <label className="text-xs text-gray-500 mb-1 block">
          {label}
          {required && <span className="text-red-500 ml-0.5">*</span>}
        </label>
      )}
      <div className="relative">
        <button
          type="button"
          onClick={() => { if (!disabled) setOpen((p) => !p); }}
          disabled={disabled}
          className={`w-full flex items-center gap-2 border rounded-lg px-3 py-2 text-sm text-left transition-colors ${
            disabled ? "bg-gray-50 cursor-not-allowed opacity-60" : "bg-white cursor-pointer hover:border-gray-400"
          } ${
            error ? "border-red-400" : "border-gray-200"
          } ${!selectedOption ? "text-gray-400" : "text-gray-900"}`}
        >
          <Search size={14} className="shrink-0 text-gray-400" />
          <span className="flex-1 truncate">
            {selectedOption ? selectedOption.label : placeholder}
          </span>
          {loading ? (
            <Loader2 size={14} className="shrink-0 animate-spin text-gray-400" />
          ) : clearable && selectedOption ? (
            <button
              type="button"
              onClick={handleClear}
              className="shrink-0 p-0.5 rounded hover:bg-gray-100"
              tabIndex={-1}
            >
              <X size={14} className="text-gray-400" />
            </button>
          ) : (
            <ChevronDown size={14} className={`shrink-0 text-gray-400 transition-transform ${open ? "rotate-180" : ""}`} />
          )}
        </button>

        {open && (
          <div className="absolute z-50 mt-1 w-full bg-white border border-gray-200 rounded-lg shadow-lg">
            <div className="p-2 border-b border-gray-100">
              <input
                ref={searchInputRef}
                type="text"
                value={search}
                onChange={(e) => { setSearch(e.target.value); setHighlightedIndex(0); }}
                onKeyDown={handleKeyDown}
                placeholder={searchPlaceholder}
                className="w-full border border-gray-200 rounded-md px-3 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
            </div>
            <div
              ref={listRef}
              className="max-h-60 overflow-y-auto"
            >
              {loading && options.length === 0 && (
                <div className="flex items-center justify-center gap-2 px-3 py-6 text-sm text-gray-400">
                  <Loader2 size={14} className="animate-spin" />
                  Loading...
                </div>
              )}
              {!loading && filtered.length === 0 && !showCreate && (
                <div className="px-3 py-6 text-sm text-gray-400 text-center">
                  {noResultsMessage}
                </div>
              )}
              {filtered.map((option, i) => (
                <button
                  key={option.value}
                  type="button"
                  onMouseEnter={() => setHighlightedIndex(i)}
                  onClick={() => handleSelect(option.value)}
                  className={`w-full text-left px-3 py-2 text-sm transition-colors ${
                    highlightedIndex === i ? "bg-primary-50 text-primary-700" : "text-gray-700 hover:bg-gray-50"
                  } ${option.value === value ? "font-medium" : ""}`}
                >
                  {option.label}
                </button>
              ))}
              {showCreate && (
                <button
                  type="button"
                  onMouseEnter={() => setHighlightedIndex(filtered.length)}
                  onClick={() => { onCreate?.(search); close(); }}
                  className={`w-full text-left px-3 py-2 text-sm transition-colors border-t border-dashed border-gray-200 ${
                    highlightedIndex === filtered.length ? "bg-primary-50 text-primary-700" : "text-gray-500 hover:bg-gray-50"
                  }`}
                >
                  + Add &quot;{search}&quot;
                </button>
              )}
            </div>
          </div>
        )}
      </div>
      {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
    </div>
  );
}
