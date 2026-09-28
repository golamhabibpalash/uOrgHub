import { useState, useRef, useEffect } from "react";
import { Building2, Check, ChevronDown } from "lucide-react";
import { useMutation } from "@tanstack/react-query";
import { switchCompany } from "../../api/auth";
import { useAuthStore } from "../../store/authStore";

// Sister-concern isolation (SISTER_CONCERN_PLAN.md). Hidden entirely for the common case (a
// single-company install, or a user who only belongs to one) — there is nothing to switch
// between, so showing a one-item dropdown would just be clutter.
export default function CompanySwitcher() {
  const user = useAuthStore((s) => s.user);
  const setAuth = useAuthStore((s) => s.setAuth);
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  const switchMutation = useMutation({
    mutationFn: (companyId: string) => switchCompany(companyId),
    onSuccess: (result) => {
      if (result.user) setAuth(result.accessToken, result.refreshToken, result.user);
      setOpen(false);
    },
  });

  useEffect(() => {
    function onClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  if (!user || !user.companies || user.companies.length < 2) return null;

  return (
    <div className="relative" ref={ref}>
      <button
        onClick={() => setOpen((o) => !o)}
        className="flex items-center gap-2 px-3 py-1.5 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 max-w-[180px]"
        title="Switch company"
      >
        <Building2 size={14} className="text-gray-400 shrink-0" />
        <span className="truncate text-gray-700">{user.activeCompanyName ?? "Select company"}</span>
        <ChevronDown size={13} className="text-gray-400 shrink-0" />
      </button>

      {open && (
        <div className="absolute right-0 mt-1 w-56 bg-white border border-gray-200 rounded-lg shadow-lg py-1 z-50">
          <div className="px-3 py-1.5 text-[11px] uppercase tracking-wide text-gray-400">Switch company</div>
          {user.companies.map((c) => (
            <button
              key={c.id}
              disabled={switchMutation.isPending}
              onClick={() => {
                if (c.id !== user.activeCompanyId) switchMutation.mutate(c.id);
                else setOpen(false);
              }}
              className="w-full flex items-center justify-between gap-2 px-3 py-2 text-sm hover:bg-gray-50 disabled:opacity-50 text-left"
            >
              <span className="truncate text-gray-700">{c.name}</span>
              {c.id === user.activeCompanyId && <Check size={14} className="text-primary-500 shrink-0" />}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
