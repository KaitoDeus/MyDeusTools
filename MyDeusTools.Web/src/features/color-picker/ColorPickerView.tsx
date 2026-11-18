import React, { useState, useEffect } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { db, SavedColor } from '../../services/db';
import { TuiPageHeader, TuiButton, TuiCard, TuiAlert } from '../../components/terminal/TerminalUI';

export const ColorPickerView: React.FC = () => {
  const { t } = useI18n();
  const [currentColor, setCurrentColor] = useState('#00FF41');
  const [copiedKey, setCopiedKey] = useState<string | null>(null);
  const [savedColors, setSavedColors] = useState<SavedColor[]>([]);

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

  const hexToRgb = (hex: string) => {
    let cleanHex = hex.replace('#', '');
    if (cleanHex.length === 3) {
      cleanHex = cleanHex
        .split('')
        .map((c) => c + c)
        .join('');
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

  const handleOpenEyeDropper = async () => {
    if (!('EyeDropper' in window)) {
      alert('The EyeDropper API is supported in Chromium browsers (Chrome, Edge, Opera).');
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
    } catch {
      // Cancelled
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
    <div className="space-y-8">
      <TuiPageHeader
        command="color-picker"
        title={t.nav.colorPicker}
        description={t.descriptions.colorPicker}
        badge="[EYEDROPPER_API]"
      />

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Main Picker (7 cols) */}
        <TuiCard title="COLOR_SAMPLER" className="lg:col-span-7 space-y-6">
          <TuiAlert type="info">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 w-full">
              <span>Pick any pixel on your entire screen directly via browser EyeDropper.</span>
              <TuiButton
                variant="primary"
                size="sm"
                prefix=">"
                onClick={handleOpenEyeDropper}
                className="shrink-0"
              >
                PICK_PIXEL
              </TuiButton>
            </div>
          </TuiAlert>

          <div className="flex items-center gap-4">
            <div
              className="w-24 h-24 border border-[var(--tui-border-medium)] shrink-0 relative cursor-pointer"
              style={{ backgroundColor: currentColor }}
            >
              <input
                type="color"
                value={currentColor}
                onChange={(e) => setCurrentColor(e.target.value)}
                className="opacity-0 absolute inset-0 w-full h-full cursor-pointer"
                title="Click to tune custom color"
              />
            </div>

            <div className="space-y-2 flex-1">
              <div className="flex items-center justify-between">
                <span className="text-[11px] uppercase tracking-[0.08em] text-[var(--tui-text-muted)]">
                  // SELECTED_HEX
                </span>
                <TuiButton
                  size="sm"
                  variant="secondary"
                  onClick={() => saveColorToHistory(currentColor)}
                >
                  [+ SAVE_PALETTE]
                </TuiButton>
              </div>
              <div className="text-[28px] font-bold text-[var(--tui-primary)] tracking-[0.04em] uppercase">
                {currentColor}
              </div>
              <p className="text-[11px] text-[var(--tui-text-dim)]">
                Click color square to manually adjust hue/brightness.
              </p>
            </div>
          </div>

          <div className="space-y-2 pt-2">
            {[
              { label: 'HEX', value: currentColor.toUpperCase() },
              { label: 'RGB', value: rgbString },
              { label: 'HSL', value: hslString },
            ].map((f) => (
              <div
                key={f.label}
                className="flex items-center justify-between px-3 py-2 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)] text-[13px]"
              >
                <span className="font-bold text-[var(--tui-secondary)] w-12">{f.label}</span>
                <span className="text-[var(--tui-text)] select-all">{f.value}</span>
                <TuiButton
                  size="sm"
                  variant="ghost"
                  onClick={() => copyToClipboard(f.value, f.label)}
                >
                  {copiedKey === f.label ? `[${t.common.copied.toUpperCase()}]` : `[${t.common.copy.toUpperCase()}]`}
                </TuiButton>
              </div>
            ))}
          </div>
        </TuiCard>

        {/* Palette History (5 cols) */}
        <TuiCard
          title={`PALETTE_HISTORY (${savedColors.length})`}
          rightMeta={
            savedColors.length > 0 && (
              <TuiButton
                size="sm"
                variant="destructive"
                onClick={async () => {
                  await db.colors.clear();
                  loadColors();
                }}
              >
                [{t.common.clear.toUpperCase()}]
              </TuiButton>
            )
          }
          className="lg:col-span-5 space-y-3"
        >
          <div className="max-h-[380px] overflow-y-auto">
            {savedColors.map((sc, idx) => (
              <div
                key={sc.id}
                className="h-[36px] flex items-center justify-between px-2 border-b border-dashed border-[var(--tui-border-subtle)] hover:bg-[var(--tui-surface-elevated)] text-[13px]"
              >
                <div
                  className="flex items-center gap-3 cursor-pointer"
                  onClick={() => setCurrentColor(sc.hex)}
                >
                  <span className="text-[11px] text-[var(--tui-text-dim)] w-[3ch] text-right">
                    {idx + 1}
                  </span>
                  <div
                    className="w-5 h-5 border border-[var(--tui-border-medium)] shrink-0"
                    style={{ backgroundColor: sc.hex }}
                  />
                  <span className="font-semibold text-[var(--tui-text)] uppercase">{sc.hex}</span>
                </div>

                <div className="flex items-center gap-1">
                  <TuiButton
                    size="sm"
                    variant="ghost"
                    onClick={() => copyToClipboard(sc.hex, `hist-${sc.id}`)}
                  >
                    {copiedKey === `hist-${sc.id}` ? '[OK]' : '[CPY]'}
                  </TuiButton>
                  <TuiButton
                    size="sm"
                    variant="destructive"
                    onClick={() => handleDeleteColor(sc.id)}
                  >
                    [X]
                  </TuiButton>
                </div>
              </div>
            ))}

            {savedColors.length === 0 && (
              <div className="text-center py-12 text-[var(--tui-text-dim)] text-[12px] space-y-1">
                <div>[EMPTY_PALETTE]</div>
                <div>Sample or pick a pixel color to store in IndexedDB.</div>
              </div>
            )}
          </div>
        </TuiCard>
      </div>
    </div>
  );
};
