import React, { useState, useEffect } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { db, SavedColor } from '../../services/db';
import {
  Pipette,
  Copy,
  Check,
  Trash2,
  Bookmark,
  Sparkles,
  Layers,
} from 'lucide-react';

export const ColorPickerView: React.FC = () => {
  const { t } = useI18n();
  const [currentColor, setCurrentColor] = useState('#6366f1');
  const [copiedKey, setCopiedKey] = useState<string | null>(null);
  const [savedColors, setSavedColors] = useState<SavedColor[]>([]);
  const [isSupported] = useState(() => 'EyeDropper' in window);

  // Load colors from IndexedDB
  const loadColors = async () => {
    try {
      const all = await db.colors.orderBy('createdAt').reverse().toArray();
      setSavedColors(all);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadColors();
  }, []);

  const copyToClipboard = (text: string, key: string) => {
    navigator.clipboard.writeText(text);
    setCopiedKey(key);
    setTimeout(() => setCopiedKey(null), 2000);
  };

  // Convert HEX to RGB, HSL, HSV
  const hexToRgb = (hex: string) => {
    let cleanHex = hex.replace('#', '');
    if (cleanHex.length === 3) {
      cleanHex = cleanHex.split('').map((c) => c + c).join('');
    }
    const num = parseInt(cleanHex, 16);
    return {
      r: (num >> 16) & 255,
      g: (num >> 8) & 255,
      b: num & 255,
    };
  };

  const rgbToHsl = (r: number, g: number, b: number) => {
    r /= 255;
    g /= 255;
    b /= 255;
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h = 0,
      s = 0;
    const l = (max + min) / 2;

    if (max !== min) {
      const d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      switch (max) {
        case r:
          h = (g - b) / d + (g < b ? 6 : 0);
          break;
        case g:
          h = (b - r) / d + 2;
          break;
        case b:
          h = (r - g) / d + 4;
          break;
      }
      h /= 6;
    }
    return {
      h: Math.round(h * 360),
      s: Math.round(s * 100),
      l: Math.round(l * 100),
    };
  };

  const { r, g, b } = hexToRgb(currentColor);
  const { h, s, l } = rgbToHsl(r, g, b);

  const rgbString = `rgb(${r}, ${g}, ${b})`;
  const hslString = `hsl(${h}, ${s}%, ${l}%)`;

  // Trigger EyeDropper
  const handleOpenEyeDropper = async () => {
    if (!('EyeDropper' in window)) {
      alert('The EyeDropper API is currently supported in Chromium browsers (Chrome, Edge, Opera).');
      return;
    }
    try {
      // @ts-ignore
      const eyeDropper = new window.EyeDropper();
      const result = await eyeDropper.open();
      if (result?.sRGBHex) {
        setCurrentColor(result.sRGBHex);
        await saveColorToHistory(result.sRGBHex);
      }
    } catch (err) {
      // User cancelled picker
    }
  };

  const saveColorToHistory = async (hex: string) => {
    const { r, g, b } = hexToRgb(hex);
    const { h, s, l } = rgbToHsl(r, g, b);
    try {
      await db.colors.add({
        hex,
        rgb: `rgb(${r}, ${g}, ${b})`,
        hsl: `hsl(${h}, ${s}%, ${l}%)`,
        hsv: '',
        label: 'Color',
        createdAt: Date.now(),
      });
      loadColors();
    } catch (e) {
      console.error(e);
    }
  };

  const handleDeleteColor = async (id?: number) => {
    if (!id) return;
    await db.colors.delete(id);
    loadColors();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
          <Pipette className="w-6 h-6 text-fuchsia-600 dark:text-fuchsia-400" />
          <span>{t.nav.colorPicker}</span>
        </h2>
        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">{t.descriptions.colorPicker}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Main Picker & Formats (7 cols) */}
        <div className="lg:col-span-7 space-y-6 glass-card p-6 rounded-2xl">
          {/* Eyedropper Button Banner */}
          <div className="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 rounded-xl bg-gradient-to-r from-fuchsia-50 via-purple-50 to-indigo-50 dark:from-fuchsia-950/40 dark:via-purple-950/40 dark:to-indigo-950/40 border border-fuchsia-200 dark:border-fuchsia-500/20">
            <div className="space-y-1">
              <h3 className="text-sm font-semibold text-slate-900 dark:text-white flex items-center gap-1.5">
                <Sparkles className="w-4 h-4 text-fuchsia-600 dark:text-fuchsia-400" />
                <span>Screen Eyedropper Tool</span>
              </h3>
              <p className="text-xs text-slate-600 dark:text-slate-400">
                Pick any pixel on your screen directly from the browser window.
              </p>
            </div>
            <button
              onClick={handleOpenEyeDropper}
              className="px-5 py-2.5 bg-gradient-to-r from-fuchsia-600 to-pink-600 hover:from-fuchsia-500 hover:to-pink-500 text-white rounded-xl text-xs font-semibold shadow-lg shadow-fuchsia-600/25 transition-all cursor-pointer flex items-center gap-2 shrink-0"
            >
              <Pipette className="w-4 h-4" />
              <span>Pick Screen Color</span>
            </button>
          </div>

          {/* Color Display & Native HTML Picker */}
          <div className="flex items-center gap-4">
            <div
              className="w-24 h-24 rounded-2xl shadow-xl border border-slate-200 dark:border-white/20 shrink-0 relative overflow-hidden"
              style={{ backgroundColor: currentColor }}
            >
              <input
                type="color"
                value={currentColor}
                onChange={(e) => setCurrentColor(e.target.value)}
                className="opacity-0 absolute inset-0 w-full h-full cursor-pointer"
                title="Click to choose custom color"
              />
            </div>

            <div className="space-y-2 flex-1">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-slate-600 dark:text-slate-400">Selected Color</span>
                <button
                  onClick={() => saveColorToHistory(currentColor)}
                  className="text-xs text-fuchsia-600 dark:text-fuchsia-400 hover:underline flex items-center gap-1 cursor-pointer"
                >
                  <Bookmark className="w-3.5 h-3.5" />
                  <span>Save to Palette</span>
                </button>
              </div>
              <div className="text-2xl font-extrabold text-slate-900 dark:text-white font-mono tracking-wider uppercase">
                {currentColor}
              </div>
              <p className="text-xs text-slate-500">
                Click the square to fine-tune color shade and saturation.
              </p>
            </div>
          </div>

          {/* Color Formats Table */}
          <div className="space-y-2.5 pt-2">
            {[
              { label: 'HEX', value: currentColor.toUpperCase() },
              { label: 'RGB', value: rgbString },
              { label: 'HSL', value: hslString },
            ].map((f) => (
              <div
                key={f.label}
                className="flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs"
              >
                <span className="font-semibold text-slate-500 dark:text-slate-400 w-12">{f.label}</span>
                <span className="font-mono text-slate-900 dark:text-slate-200 select-all">{f.value}</span>
                <button
                  onClick={() => copyToClipboard(f.value, f.label)}
                  className="text-slate-500 hover:text-fuchsia-600 dark:text-slate-400 dark:hover:text-fuchsia-400 flex items-center gap-1 cursor-pointer"
                >
                  {copiedKey === f.label ? (
                    <Check className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                  ) : (
                    <Copy className="w-3.5 h-3.5" />
                  )}
                  <span>{copiedKey === f.label ? t.common.copied : t.common.copy}</span>
                </button>
              </div>
            ))}
          </div>
        </div>

        {/* Palette History (5 cols) */}
        <div className="lg:col-span-5 space-y-4 glass-card p-6 rounded-2xl">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-2">
              <Layers className="w-4 h-4 text-fuchsia-600 dark:text-fuchsia-400" />
              <span>Palette History ({savedColors.length})</span>
            </h3>
            {savedColors.length > 0 && (
              <button
                onClick={async () => {
                  await db.colors.clear();
                  loadColors();
                }}
                className="text-xs text-slate-500 hover:text-rose-500 dark:hover:text-rose-400 transition-colors cursor-pointer"
              >
                {t.common.clear}
              </button>
            )}
          </div>

          <div className="space-y-2 max-h-[360px] overflow-y-auto pr-1">
            {savedColors.map((sc) => (
              <div
                key={sc.id}
                className="flex items-center justify-between p-2.5 rounded-xl bg-slate-50 dark:bg-slate-900/60 border border-slate-200 dark:border-slate-800 hover:border-slate-300 dark:hover:border-slate-700 transition-all text-xs group"
              >
                <div
                  className="flex items-center gap-3 cursor-pointer"
                  onClick={() => setCurrentColor(sc.hex)}
                >
                  <div
                    className="w-7 h-7 rounded-lg border border-slate-200 dark:border-white/10 shrink-0"
                    style={{ backgroundColor: sc.hex }}
                  />
                  <div className="font-mono font-medium text-slate-800 dark:text-slate-300 uppercase">
                    {sc.hex}
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => copyToClipboard(sc.hex, `hist-${sc.id}`)}
                    className="p-1 text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white cursor-pointer"
                  >
                    {copiedKey === `hist-${sc.id}` ? (
                      <Check className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                    ) : (
                      <Copy className="w-3.5 h-3.5" />
                    )}
                  </button>
                  <button
                    onClick={() => handleDeleteColor(sc.id)}
                    className="p-1 text-slate-500 hover:text-rose-500 dark:hover:text-rose-400 cursor-pointer"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            ))}

            {savedColors.length === 0 && (
              <div className="text-center py-12 text-slate-500 text-xs space-y-1">
                <p>No colors saved yet.</p>
                <p className="text-[11px] text-slate-400 dark:text-slate-600">Sample or pick a color to build your palette.</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
