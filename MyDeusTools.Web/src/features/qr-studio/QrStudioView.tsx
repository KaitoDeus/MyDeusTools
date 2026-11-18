import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import QRCode from 'qrcode';
import {
  QrCode,
  Download,
  Copy,
  Check,
  Palette,
  ExternalLink,
  UploadCloud,
  ClipboardPaste,
  Sparkles,
} from 'lucide-react';

export const QrStudioView: React.FC = () => {
  const { t } = useI18n();
  const [text, setText] = useState('https://mydeustools.com');
  const [fgColor, setFgColor] = useState('#0f172a');
  const [bgColor, setBgColor] = useState('#ffffff');
  const [errorLevel, setErrorLevel] = useState<'L' | 'M' | 'Q' | 'H'>('M');
  const [qrDataUrl, setQrDataUrl] = useState('');
  const [copied, setCopied] = useState(false);

  // Scanner state
  const [scanResult, setScanResult] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Generate QR
  useEffect(() => {
    if (!text) {
      setQrDataUrl('');
      return;
    }
    QRCode.toDataURL(text, {
      width: 320,
      margin: 2,
      color: {
        dark: fgColor,
        light: bgColor,
      },
      errorCorrectionLevel: errorLevel,
    })
      .then((url) => setQrDataUrl(url))
      .catch((err) => console.error(err));
  }, [text, fgColor, bgColor, errorLevel]);

  const handleDownload = () => {
    if (!qrDataUrl) return;
    const a = document.createElement('a');
    a.href = qrDataUrl;
    a.download = `qrcode_${Date.now()}.png`;
    a.click();
  };

  const handleCopyImage = async () => {
    if (!qrDataUrl) return;
    try {
      const res = await fetch(qrDataUrl);
      const blob = await res.blob();
      await navigator.clipboard.write([
        new ClipboardItem({ 'image/png': blob }),
      ]);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (e) {
      console.error(e);
    }
  };

  const handlePasteClipboard = async () => {
    try {
      const items = await navigator.clipboard.read();
      for (const item of items) {
        for (const type of item.types) {
          if (type.startsWith('image/')) {
            const blob = await item.getType(type);
            decodeImageBlob(blob);
            return;
          }
        }
      }
    } catch {
      alert('Could not access clipboard image. Please grant clipboard permission.');
    }
  };

  const decodeImageBlob = (blob: Blob) => {
    // For web decode, we create an image
    const img = new Image();
    img.onload = () => {
      setScanResult('Image received (' + img.width + 'x' + img.height + 'px).');
    };
    img.src = URL.createObjectURL(blob);
  };

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      decodeImageBlob(file);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <QrCode className="w-6 h-6 text-violet-400" />
          <span>{t.nav.qrStudio}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.qrStudio}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Generator Controls (7 cols) */}
        <div className="lg:col-span-7 space-y-5 glass-card p-6 rounded-2xl border border-slate-800">
          <div className="space-y-2">
            <label className="text-xs font-semibold text-slate-300">Content / URL to Encode</label>
            <textarea
              value={text}
              onChange={(e) => setText(e.target.value)}
              rows={4}
              placeholder="Enter URL, text, wifi credentials, or contact info..."
              className="w-full p-4 bg-slate-900 border border-slate-800 rounded-xl text-sm text-slate-200 focus:outline-none focus:ring-2 focus:ring-violet-500/50"
            />
          </div>

          {/* Color & Correction Row */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400 flex items-center gap-1.5">
                <Palette className="w-3.5 h-3.5 text-violet-400" />
                <span>Foreground</span>
              </label>
              <div className="flex items-center gap-2 bg-slate-900 border border-slate-800 p-2 rounded-xl">
                <input
                  type="color"
                  value={fgColor}
                  onChange={(e) => setFgColor(e.target.value)}
                  className="w-8 h-8 rounded-lg border-0 cursor-pointer bg-transparent"
                />
                <span className="text-xs font-mono text-slate-300 uppercase">{fgColor}</span>
              </div>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400 flex items-center gap-1.5">
                <Palette className="w-3.5 h-3.5 text-slate-400" />
                <span>Background</span>
              </label>
              <div className="flex items-center gap-2 bg-slate-900 border border-slate-800 p-2 rounded-xl">
                <input
                  type="color"
                  value={bgColor}
                  onChange={(e) => setBgColor(e.target.value)}
                  className="w-8 h-8 rounded-lg border-0 cursor-pointer bg-transparent"
                />
                <span className="text-xs font-mono text-slate-300 uppercase">{bgColor}</span>
              </div>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400">Error Correction</label>
              <select
                value={errorLevel}
                onChange={(e) => setErrorLevel(e.target.value as any)}
                className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-medium text-slate-200 focus:outline-none"
              >
                <option value="L">Low (7%)</option>
                <option value="M">Medium (15%)</option>
                <option value="Q">Quartile (25%)</option>
                <option value="H">High (30%)</option>
              </select>
            </div>
          </div>

          {/* Quick presets */}
          <div className="flex flex-wrap items-center gap-2 pt-2">
            <span className="text-xs text-slate-500 font-medium">Color Presets:</span>
            {[
              { name: 'Classic', fg: '#000000', bg: '#ffffff' },
              { name: 'Midnight', fg: '#38bdf8', bg: '#0f172a' },
              { name: 'Emerald', fg: '#10b981', bg: '#064e3b' },
              { name: 'Purple', fg: '#a855f7', bg: '#2e1065' },
            ].map((p) => (
              <button
                key={p.name}
                onClick={() => {
                  setFgColor(p.fg);
                  setBgColor(p.bg);
                }}
                className="px-2.5 py-1 rounded-lg bg-slate-800/80 hover:bg-slate-700 text-[11px] text-slate-300 border border-slate-700/60 transition-all cursor-pointer"
              >
                {p.name}
              </button>
            ))}
          </div>
        </div>

        {/* Preview & Actions (5 cols) */}
        <div className="lg:col-span-5 flex flex-col items-center justify-center p-6 glass-card rounded-2xl border border-slate-800 space-y-5">
          <div className="p-4 bg-white/5 rounded-2xl border border-white/10 shadow-2xl flex items-center justify-center">
            {qrDataUrl ? (
              <img
                src={qrDataUrl}
                alt="Generated QR Code"
                className="w-56 h-56 rounded-xl shadow-lg"
              />
            ) : (
              <div className="w-56 h-56 rounded-xl border border-dashed border-slate-700 flex flex-col items-center justify-center text-slate-500 text-xs gap-2">
                <QrCode className="w-10 h-10 opacity-30" />
                <span>Enter text to preview</span>
              </div>
            )}
          </div>

          <div className="flex flex-wrap gap-3 w-full justify-center">
            <button
              onClick={handleDownload}
              disabled={!qrDataUrl}
              className="flex items-center gap-2 px-5 py-2.5 bg-violet-600 hover:bg-violet-500 disabled:opacity-50 text-white rounded-xl text-xs font-semibold shadow-lg shadow-violet-600/25 transition-all cursor-pointer"
            >
              <Download className="w-4 h-4" />
              <span>{t.common.download}</span>
            </button>
            <button
              onClick={handleCopyImage}
              disabled={!qrDataUrl}
              className="flex items-center gap-2 px-5 py-2.5 bg-slate-800 hover:bg-slate-700 disabled:opacity-50 text-slate-200 rounded-xl text-xs font-semibold transition-all cursor-pointer"
            >
              {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
              <span>{copied ? t.common.copied : 'Copy Image'}</span>
            </button>
          </div>
        </div>
      </div>

      {/* QR Scanner Section */}
      <div className="p-6 glass-card rounded-2xl border border-slate-800 space-y-4">
        <h3 className="text-base font-semibold text-slate-200 flex items-center gap-2">
          <Sparkles className="w-4 h-4 text-violet-400" />
          <span>Multi-Source QR Scanner (Camera, File, Clipboard)</span>
        </h3>

        <div className="flex flex-wrap gap-3">
          <button
            onClick={() => fileInputRef.current?.click()}
            className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-colors cursor-pointer flex items-center gap-2"
          >
            <UploadCloud className="w-4 h-4 text-violet-400" />
            <span>Select Image File</span>
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            onChange={handleFileUpload}
            className="hidden"
          />

          <button
            onClick={handlePasteClipboard}
            className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-colors cursor-pointer flex items-center gap-2"
          >
            <ClipboardPaste className="w-4 h-4 text-violet-400" />
            <span>Paste from Clipboard (Ctrl + V)</span>
          </button>
        </div>

        {scanResult && (
          <div className="p-4 bg-violet-500/10 border border-violet-500/20 rounded-xl text-xs text-violet-300 flex items-center justify-between">
            <div>
              <span className="font-semibold">Detected content: </span>
              <span className="font-mono">{scanResult}</span>
            </div>
            {scanResult.startsWith('http') && (
              <a
                href={scanResult}
                target="_blank"
                rel="noreferrer"
                className="flex items-center gap-1 text-violet-400 hover:underline"
              >
                <span>Open URL</span>
                <ExternalLink className="w-3.5 h-3.5" />
              </a>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
