import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiSelect,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

interface SelectedImage {
  id: string;
  name: string;
  originalSize: number;
  file: File;
  previewUrl: string;
  width: number;
  height: number;
}

export const ImageStudioView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.image;
  const [images, setImages] = useState<SelectedImage[]>([]);
  const [quality, setQuality] = useState(80);
  const [format, setFormat] = useState<'image/webp' | 'image/jpeg' | 'image/png'>('image/webp');
  const [scale, setScale] = useState(100);
  const [isProcessing, setIsProcessing] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  const handleUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    files.forEach((f) => {
      const url = URL.createObjectURL(f);
      const img = new Image();
      img.onload = () => {
        setImages((prev) => [
          ...prev,
          {
            id: Math.random().toString(36).substring(7),
            name: f.name,
            originalSize: f.size,
            file: f,
            previewUrl: url,
            width: img.width,
            height: img.height,
          },
        ]);
      };
      img.src = url;
    });
  };

  const removeImage = (id: string) => {
    setImages((prev) => prev.filter((img) => img.id !== id));
  };

  const processAndDownload = async (imgItem: SelectedImage) => {
    const img = new Image();
    img.src = imgItem.previewUrl;
    await img.decode();

    const canvas = document.createElement('canvas');
    const targetW = Math.round((img.width * scale) / 100);
    const targetH = Math.round((img.height * scale) / 100);
    canvas.width = targetW;
    canvas.height = targetH;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.drawImage(img, 0, 0, targetW, targetH);

    canvas.toBlob(
      (blob) => {
        if (!blob) return;
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        const ext = format === 'image/webp' ? 'webp' : format === 'image/jpeg' ? 'jpg' : 'png';
        const baseName = imgItem.name.substring(0, imgItem.name.lastIndexOf('.')) || imgItem.name;
        a.href = url;
        a.download = `${baseName}_optimized.${ext}`;
        a.click();
      },
      format,
      quality / 100
    );
  };

  const handleProcessAll = async () => {
    if (images.length === 0) return;
    setIsProcessing(true);
    for (const img of images) {
      await processAndDownload(img);
    }
    setIsProcessing(false);
    setStatusMessage(tt.doneMsg);
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="img-studio"
        title={t.nav.imageStudio}
        description={t.descriptions.imageStudio}
        badge="[CANVAS_ENGINE]"
        accent="tertiary"
      />

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <TuiCard title={tt.paramsTitle} className="lg:col-span-4 p-6" bodyClassName="space-y-5">
          <TuiSelect
            label={tt.targetFormatLabel}
            value={format}
            onChange={(e) => setFormat(e.target.value as any)}
          >
            <option value="image/webp">{tt.formats.webp}</option>
            <option value="image/jpeg">{tt.formats.jpeg}</option>
            <option value="image/png">{tt.formats.png}</option>
          </TuiSelect>

          {format !== 'image/png' && (
            <div className="space-y-1.5">
              <div className="flex justify-between text-[12px]">
                <span className="text-[var(--tui-text-muted)]">// {tt.qualityLabel}</span>
                <span className="text-[var(--tui-primary)] font-bold">{quality}%</span>
              </div>
              <input
                type="range"
                min={10}
                max={100}
                value={quality}
                onChange={(e) => setQuality(Number(e.target.value))}
                className="w-full cursor-pointer"
              />
            </div>
          )}

          <div className="space-y-1.5">
            <div className="flex justify-between text-[12px]">
              <span className="text-[var(--tui-text-muted)]">// {tt.scaleLabel}</span>
              <span className="text-[var(--tui-tertiary)] font-bold">{scale}%</span>
            </div>
            <input
              type="range"
              min={20}
              max={100}
              step={5}
              value={scale}
              onChange={(e) => setScale(Number(e.target.value))}
              className="w-full cursor-pointer"
            />
          </div>

          <TuiAlert type="info">{tt.exifNotice}</TuiAlert>

          <TuiButton
            variant="primary"
            prefix=">"
            onClick={handleProcessAll}
            disabled={images.length === 0 || isProcessing}
            className="w-full"
          >
            {isProcessing ? t.common.processing : `${tt.processAll} (${images.length})`}
          </TuiButton>
        </TuiCard>

        <div className="lg:col-span-8 space-y-4">
          <div className="p-6 border border-dashed border-[var(--tui-border-medium)] hover:border-[var(--tui-tertiary)] bg-[var(--tui-surface)] flex flex-col items-center justify-center text-center space-y-3">
            <div className="text-[13px] font-bold text-[var(--tui-tertiary)]">
              {tt.loadBatchTitle}
            </div>
            <label className="inline-flex h-[36px] px-4 py-2 text-[13px] font-semibold uppercase border border-[var(--tui-tertiary)] text-[var(--tui-tertiary)] hover:bg-[var(--tui-tertiary)] hover:text-[var(--tui-primary-fg)] cursor-pointer items-center">
              <span>$ {t.common.browse.toUpperCase()}</span>
              <input
                type="file"
                multiple
                accept="image/*"
                onChange={handleUpload}
                className="hidden"
              />
            </label>
          </div>

          {statusMessage && <TuiAlert type="ok">{statusMessage}</TuiAlert>}

          {images.length > 0 && (
            <TuiCard title={`${tt.queueTitle} (${images.length})`}>
              <div className="max-h-[420px] overflow-y-auto">
                {images.map((img, idx) => (
                  <div
                    key={img.id}
                    className="flex items-center justify-between py-2 px-2 border-b border-dashed border-[var(--tui-border-subtle)] hover:bg-[var(--tui-surface-elevated)] text-[12px]"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <span className="text-[11px] text-[var(--tui-text-dim)] w-[3ch] text-right">
                        {idx + 1}
                      </span>
                      <img
                        src={img.previewUrl}
                        alt={img.name}
                        className="w-10 h-10 object-cover border border-[var(--tui-border-medium)] shrink-0"
                      />
                      <div className="min-w-0">
                        <div className="font-semibold text-[var(--tui-text)] truncate">
                          {img.name}
                        </div>
                        <div className="text-[11px] text-[var(--tui-text-muted)]">
                          {img.width}x{img.height}px | {(img.originalSize / 1024).toFixed(1)} KB
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-2 shrink-0">
                      <TuiButton
                        size="sm"
                        variant="secondary"
                        onClick={() => processAndDownload(img)}
                      >
                        {tt.getOne}
                      </TuiButton>
                      <TuiButton
                        size="sm"
                        variant="destructive"
                        onClick={() => removeImage(img.id)}
                      >
                        [X]
                      </TuiButton>
                    </div>
                  </div>
                ))}
              </div>
            </TuiCard>
          )}
        </div>
      </div>
    </div>
  );
};
