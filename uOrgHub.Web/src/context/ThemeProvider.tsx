import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react';
import { getTheme, updateTheme, resetTheme, PALETTES, type ThemeSettings, type PaletteKey } from '../api/theme';
import { generateShades, hexToRgbString } from '../utils/color';
import { useAuthStore } from '../store/authStore';

const FALLBACK_THEME: ThemeSettings = {
  id: '',
  themeName: 'corporate-blue',
  primaryColor: '#0ea5e9',
  sidebarBg: '#1e293b',
  sidebarText: '#cbd5e1',
  sidebarActiveBg: '#0ea5e9',
  sidebarActiveText: '#ffffff',
  isDarkMode: false,
};

interface ThemeContextValue {
  theme: ThemeSettings | null;
  isLoading: boolean;
  applyPalette: (key: PaletteKey) => void;
  saveTheme: (settings: Partial<ThemeSettings>) => Promise<void>;
  resetToDefault: () => Promise<void>;
}

const ThemeContext = createContext<ThemeContextValue>({
  theme: null,
  isLoading: true,
  applyPalette: () => {},
  saveTheme: async () => {},
  resetToDefault: async () => {},
});

export function useThemeCtx() { return useContext(ThemeContext); }

function setCSSVars(settings: ThemeSettings) {
  const root = document.documentElement;

  root.style.setProperty('--sidebar-bg', settings.sidebarBg);
  root.style.setProperty('--sidebar-text', settings.sidebarText);
  root.style.setProperty('--sidebar-active-bg', settings.sidebarActiveBg);
  root.style.setProperty('--sidebar-active-text', settings.sidebarActiveText);

  const primaryShades = generateShades(settings.primaryColor);
  (Object.entries(primaryShades) as [string, string][]).forEach(([k, v]) => {
    root.style.setProperty(`--color-primary-${k}`, v);
    root.style.setProperty(`--color-primary-${k}-rgb`, hexToRgbString(v));
  });

  const activeShades = generateShades(settings.sidebarActiveBg);
  (Object.entries(activeShades) as [string, string][]).forEach(([k, v]) => {
    root.style.setProperty(`--color-accent-${k}`, v);
  });
}

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<ThemeSettings | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  const load = useCallback(async () => {
    try {
      const t = await getTheme();
      setTheme(t);
      setCSSVars(t);
    } catch {
      setTheme(FALLBACK_THEME);
      setCSSVars(FALLBACK_THEME);
    }
    setIsLoading(false);
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      setTheme(FALLBACK_THEME);
      setCSSVars(FALLBACK_THEME);
      setIsLoading(false);
      return;
    }
    load();
  }, [isAuthenticated, load]);

  const applyPalette = useCallback((key: PaletteKey) => {
    const p = PALETTES[key];
    const merged: ThemeSettings = {
      id: theme?.id ?? '', themeName: key,
      primaryColor: p.primaryColor, sidebarBg: p.sidebarBg,
      sidebarText: p.sidebarText, sidebarActiveBg: p.sidebarActiveBg,
      sidebarActiveText: p.sidebarActiveText, isDarkMode: theme?.isDarkMode ?? false,
    };
    setTheme(merged);
    setCSSVars(merged);
  }, [theme]);

  const saveTheme = useCallback(async (settings: Partial<ThemeSettings>) => {
    const merged = { ...theme!, ...settings };
    const saved = await updateTheme(merged);
    setTheme(saved);
    setCSSVars(saved);
  }, [theme]);

  const resetToDefault = useCallback(async () => {
    const saved = await resetTheme();
    setTheme(saved);
    setCSSVars(saved);
  }, []);

  return (
    <ThemeContext.Provider value={{ theme, isLoading, applyPalette, saveTheme, resetToDefault }}>
      {children}
    </ThemeContext.Provider>
  );
}
