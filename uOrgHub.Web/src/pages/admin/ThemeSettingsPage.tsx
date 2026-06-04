import { useState } from 'react';
import toast from 'react-hot-toast';
import { useThemeCtx } from '../../context/ThemeProvider';
import { PALETTES, type PaletteKey, type ThemeSettings } from '../../api/theme';
import { hexToRgbString, generateShades } from '../../utils/color';

const PALETTE_KEYS = Object.keys(PALETTES) as PaletteKey[];

function applyDraftPreview(draft: Partial<ThemeSettings>) {
  const root = document.documentElement;
  if (draft.primaryColor) {
    const shades = generateShades(draft.primaryColor);
    (Object.entries(shades) as [string, string][]).forEach(([k, v]) => {
      root.style.setProperty(`--color-primary-${k}`, v);
      root.style.setProperty(`--color-primary-${k}-rgb`, hexToRgbString(v));
    });
  }
  if (draft.sidebarBg) root.style.setProperty('--sidebar-bg', draft.sidebarBg);
  if (draft.sidebarText) root.style.setProperty('--sidebar-text', draft.sidebarText);
  if (draft.sidebarActiveBg) root.style.setProperty('--sidebar-active-bg', draft.sidebarActiveBg);
  if (draft.sidebarActiveText) root.style.setProperty('--sidebar-active-text', draft.sidebarActiveText);
}

export default function ThemeSettingsPage() {
  const { theme, isLoading, applyPalette, saveTheme, resetToDefault } = useThemeCtx();
  const [draft, setDraft] = useState<Partial<ThemeSettings> | null>(null);

  if (isLoading || !theme) {
    return <div className="p-6 text-gray-400 text-sm">Loading theme settings...</div>;
  }

  const current = draft ?? theme;

  const field = (key: keyof ThemeSettings) =>
    (current as any)[key] as string;

  return (
    <div className="p-6 max-w-4xl mx-auto space-y-8">
      <div>
        <h1 className="text-xl font-semibold text-gray-900">Theme Settings</h1>
        <p className="text-sm text-gray-500 mt-1">Customize the look and feel of the application.</p>
      </div>

      {/* Predefined Palettes */}
      <section>
        <h2 className="text-sm font-medium text-gray-800 mb-3">Predefined Palettes</h2>
        <div className="grid grid-cols-3 md:grid-cols-6 gap-3">
          {PALETTE_KEYS.map((key) => {
            const p = PALETTES[key];
            const active = current.themeName === key;
            return (
              <button
                key={key}
                onClick={() => { applyPalette(key); setDraft(null); }}
                className={`relative rounded-xl border-2 p-3 text-left transition-all hover:shadow-md ${
                  active ? 'border-primary-500 ring-2 ring-primary-200' : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                <div className="flex gap-1 mb-2">
                  {[p.primaryColor, p.sidebarBg, p.sidebarActiveBg].map((c, i) => (
                    <div key={i} className="w-5 h-5 rounded-full border border-white/30" style={{ backgroundColor: c }} />
                  ))}
                </div>
                <p className="text-xs font-medium text-gray-800">{p.label}</p>
                {active && <span className="text-[10px] text-primary-600 font-medium">Active</span>}
              </button>
            );
          })}
        </div>
      </section>

      {/* Custom Colors */}
      <section>
        <h2 className="text-sm font-medium text-gray-800 mb-3">Custom Colors</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {([
            { label: 'Primary Color', key: 'primaryColor' as const },
            { label: 'Sidebar Background', key: 'sidebarBg' as const },
            { label: 'Sidebar Text', key: 'sidebarText' as const },
            { label: 'Sidebar Active Background', key: 'sidebarActiveBg' as const },
            { label: 'Sidebar Active Text', key: 'sidebarActiveText' as const },
          ]).map(({ label, key }) => (
            <div key={key}>
              <label className="text-xs text-gray-500 mb-1 block">{label}</label>
              <div className="flex items-center gap-2">
                <input
                  type="color"
                  value={field(key)}
                  onChange={(e) => {
                    const newVal = e.target.value;
                    setDraft((prev) => {
                      const next = { ...(prev ?? theme), [key]: newVal };
                      applyDraftPreview(next);
                      return next;
                    });
                  }}
                  className="w-9 h-9 rounded-lg border border-gray-200 cursor-pointer p-0.5"
                />
                <input
                  type="text"
                  value={field(key)}
                  onChange={(e) => setDraft((prev) => ({ ...(prev ?? theme), [key]: e.target.value }))}
                  className="flex-1 border border-gray-200 rounded-lg px-3 py-1.5 text-xs font-mono focus:outline-none focus:ring-1 focus:ring-primary-500"
                />
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Actions */}
      <div className="flex items-center gap-3 pt-4 border-t border-gray-200">
        <button
          onClick={async () => {
            try {
              await saveTheme(current);
              toast.success('Theme saved');
            } catch { toast.error('Failed to save theme'); }
            setDraft(null);
          }}
          className="px-4 py-2 bg-primary-500 text-white text-sm rounded-lg hover:bg-primary-600 transition-colors"
        >
          Save Changes
        </button>
        <button
          onClick={async () => {
            try {
              await resetToDefault();
              toast.success('Theme reset to defaults');
            } catch { toast.error('Failed to reset theme'); }
            setDraft(null);
          }}
          className="px-4 py-2 border border-gray-200 text-gray-600 text-sm rounded-lg hover:bg-gray-50 transition-colors"
        >
          Reset to Default
        </button>
        {draft && (
          <button
            onClick={() => {
              setDraft(null);
              applyPalette(theme.themeName as PaletteKey);
            }}
            className="px-4 py-2 text-gray-500 text-sm hover:text-gray-700 transition-colors"
          >
            Cancel
          </button>
        )}
        <div className="flex items-center gap-2 ml-auto">
          <span className="text-xs text-gray-400">{theme.themeName}</span>
          <div className="w-5 h-5 rounded-full border border-gray-200" style={{ backgroundColor: theme.primaryColor }} />
        </div>
      </div>
    </div>
  );
}
