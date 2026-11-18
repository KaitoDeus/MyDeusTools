import React, { useState, useEffect, useRef } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import QRCode from 'qrcode';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiTextarea,
  TuiSelect,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

export const QrStudioView: React.FC = () => {
  const { t } = useI18n();
  const [text, setText] = useState('https://mydeustools.com');
  const [fgColor, setFgColor] = useState('#00FF41');
  const [bgColor, setBgColor] = useState('#0D0D0D');
  const [errorLevel, setErrorLevel] = useState<'L' | 'M' | 'Q' | 'H'>('M');
  const [qrDataUrl, setQrDataUrl] = useState('');
  const [copied, setCopied] = useState(false);

  // Scanner state
  const [scanResult, setScanResult] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

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
      await navigator.clipboard.write([new ClipboardItem({ 'image/png': blob })]);
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
    <div className="space-y-8">
      <TuiPageHeader
        command="qr-studio"
        title={t.nav.qrStudio}
        description={t.descriptions.qrStudio}
        badge="[QR]"
        accent="tertiary"
      />

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Generator Controls (7 cols) */}
        <TuiCard title="QR_GENERATOR_CONFIG" className="lg:col-span-7 space-y-5">
          <TuiTextarea
            label="content_or_url"
            value={text}
            onChange={(e) => setText(e.target.value)}
            rows={4}
            placeholder="Enter URL, text, wifi credentials, or contact info..."
          />

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="space-y-1">
              <label className="text-[12px] font-medium text-[var(--tui-text-muted)]">
                // fg_color
              </label>
              <div className="flex items-center gap-2 bg-[var(--tui-bg)] border border-[var(--tui-border-medium)] p-1.5 h-[36px]">
                <input
                  type="color"
                  value={fgColor}
                  onChange={(e) => setFgColor(e.target.value)}
                  className="w-6 h-6 border-0 cursor-pointer bg-transparent"
                />
                <span className="text-[12px] text-[var(--tui-text)] uppercase">{fgColor}</span>
              </div>
            </div>

            <div className="space-y-1">
              <label className="text-[12px] font-medium text-[var(--tui-text-muted)]">
                // bg_color
              </label>
              <div className="flex items-center gap-2 bg-[var(--tui-bg)] border border-[var(--tui-border-medium)] p-1.5 h-[36px]">
                <input
                  type="color"
                  value={bgColor}
                  onChange={(e) => setBgColor(e.target.value)}
                  className="w-6 h-6 border-0 cursor-pointer bg-transparent"
                />
                <span className="text-[12px] text-[var(--tui-text)] uppercase">{bgColor}</span>
              </div>
            </div>

            <TuiSelect
              label="error_correction"
              value={errorLevel}
              onChange={(e) => setErrorLevel(e.target.value as any)}
            >
              <option value="L">LOW (7%)</option>
              <option value="M">MEDIUM (15%)</option>
              <option value="Q">QUARTILE (25%)</option>
              <option value="H">HIGH (30%)</option>
            </TuiSelect>
          </div>

          <div className="flex flex-wrap items-center gap-2 pt-2 border-t border-dashed border-[var(--tui-border-subtle)]">
            <span className="text-[11px] text-[var(--tui-text-dim)] uppercase">PRESETS:</span>
            {[
              { name: 'HACKER', fg: '#00FF41', bg: '#0D0D0D' },
              { name: 'CLASSIC', fg: '#000000', bg: '#ffffff' },
              { name: 'CYAN', fg: '#00BFFF', bg: '#0D0D0D' },
              { name: 'AMBER', fg: '#FF6600', bg: '#0D0D0D' },
            ].map((p) => (
              <TuiButton
                key={p.name}
                size="sm"
                variant="ghost"
                onClick={() => {
                  setFgColor(p.fg);
                  setBgColor(p.bg);
                }}
              >
                [{p.name}]
              </TuiButton>
            ))}
          </div>
        </TuiCard>

        {/* Preview & Actions (5 cols) */}
        <TuiCard
          title="OUTPUT_MATRIX"
          elevated
          className="lg:col-span-5 flex flex-col items-center justify-center space-y-5"
        >
          <div className="p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-medium)] flex items-center justify-center">
            {qrDataUrl ? (
              <img src={qrDataUrl} alt="Generated QR Code" className="w-56 h-56" />
            ) : (
              <div className="w-56 h-56 border border-dashed border-[var(--tui-border-medium)] flex flex-col items-center justify-center text-[var(--tui-text-dim)] text-[12px]">
                <span>[NO_INPUT]</span>
              </div>
            )}
          </div>

          <div className="flex flex-wrap gap-3 w-full justify-center">
            <TuiButton
              variant="primary"
              prefix=">"
              onClick={handleDownload}
              disabled={!qrDataUrl}
            >
              {t.common.download}
            </TuiButton>
            <TuiButton variant="secondary" onClick={handleCopyImage} disabled={!qrDataUrl}>
              {copied ? `[${t.common.copied.toUpperCase()}]` : '[COPY_IMAGE]'}
            </TuiButton>
          </div>
        </TuiCard>
      </div>

      {/* QR Scanner Section */}
      <TuiCard title="MULTI_SOURCE_SCANNER (FILE / CLIPBOARD)" className="space-y-4">
        <div className="flex flex-wrap gap-3">
          <TuiButton
            variant="secondary"
            prefix="$"
            onClick={() => fileInputRef.current?.click()}
          >
            SELECT_IMAGE_FILE
          </TuiButton>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            onChange={handleFileUpload}
            className="hidden"
          />

          <TuiButton variant="amber" prefix="$" onClick={handlePasteClipboard}>
            PASTE_CLIPBOARD (CTRL+V)
          </TuiButton>
        </div>

        {scanResult && (
          <TuiAlert type="info">
            <div className="flex items-center justify-between w-full gap-4">
              <span>DETECTED: {scanResult}</span>
              {scanResult.startsWith('http') && (
                <a
                  href={scanResult}
                  target="_blank"
                  rel="noreferrer"
                  className="text-[var(--tui-primary)] font-bold hover:underline"
                >
                  [OPEN_URL]
                </a>
              )}
            </div>
          </TuiAlert>
        )}
      </TuiCard>
    </div>
  );
};
