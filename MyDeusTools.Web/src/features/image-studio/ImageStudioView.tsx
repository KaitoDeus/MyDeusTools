import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  Image as ImageIcon,
  UploadCloud,
  Download,
  Trash2,
  Sliders,
  Sparkles,
  ShieldCheck,
  Check,
} from 'lucide-react';

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
    setStatusMessage('All images processed and downloaded!');
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <ImageIcon className="w-6 h-6 text-sky-400" />
          <span>{t.nav.imageStudio}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.imageStudio}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Settings Panel (4 cols) */}
        <div className="lg:col-span-4 space-y-5 glass-card p-6 rounded-2xl border border-slate-800">
          <h3 className="text-sm font-semibold text-slate-200 flex items-center gap-2">
            <Sliders className="w-4 h-4 text-sky-400" />
            <span>Compression & Format</span>
          </h3>

          <div className="space-y-4">
            {/* Format */}
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-400">Target Format</label>
              <select
                value={format}
                onChange={(e) => setFormat(e.target.value as any)}
                className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-medium text-slate-200 focus:outline-none"
              >
                <option value="image/webp">WebP (Modern, Smallest size)</option>
                <option value="image/jpeg">JPEG (Universal photo)</option>
                <option value="image/png">PNG (Lossless & Alpha)</option>
              </select>
            </div>

            {/* Quality Slider */}
            {format !== 'image/png' && (
              <div className="space-y-1.5">
                <div className="flex justify-between text-xs">
                  <span className="text-slate-400">Quality</span>
                  <span className="text-sky-400 font-mono font-semibold">{quality}%</span>
                </div>
                <input
                  type="range"
                  min={10}
                  max={100}
                  value={quality}
                  onChange={(e) => setQuality(Number(e.target.value))}
                  className="w-full accent-sky-500 cursor-pointer"
                />
              </div>
            )}

            {/* Scale Slider */}
            <div className="space-y-1.5">
              <div className="flex justify-between text-xs">
                <span className="text-slate-400">Scale Resolution</span>
                <span className="text-sky-400 font-mono font-semibold">{scale}%</span>
              </div>
              <input
                type="range"
                min={20}
                max={100}
                step={5}
                value={scale}
                onChange={(e) => setScale(Number(e.target.value))}
                className="w-full accent-sky-500 cursor-pointer"
              />
            </div>

            <div className="p-3 bg-slate-900/60 rounded-xl border border-slate-800 text-[11px] text-slate-400 flex items-center gap-2">
              <ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" />
              <span>Canvas re-encoding automatically strips 100% EXIF and GPS metadata.</span>
            </div>

            <button
              onClick={handleProcessAll}
              disabled={images.length === 0 || isProcessing}
              className="w-full py-3 bg-sky-600 hover:bg-sky-500 disabled:opacity-40 text-white rounded-xl text-xs font-bold uppercase tracking-wider shadow-lg shadow-sky-600/20 transition-all cursor-pointer flex items-center justify-center gap-2"
            >
              <Download className="w-4 h-4" />
              <span>{isProcessing ? t.common.processing : `Process All (${images.length})`}</span>
            </button>
          </div>
        </div>

        {/* Upload & Image List (8 cols) */}
        <div className="lg:col-span-8 space-y-4">
          <div className="p-6 border-2 border-dashed border-slate-800 hover:border-sky-500/50 rounded-2xl flex flex-col items-center justify-center text-center space-y-2 glass-card transition-all">
            <UploadCloud className="w-8 h-8 text-sky-400 opacity-80" />
            <p className="text-xs font-semibold text-slate-300">
              Drag & drop images here, or browse from computer
            </p>
            <label className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-all cursor-pointer">
              <span>{t.common.browse}</span>
              <input
                type="file"
                multiple
                accept="image/*"
                onChange={handleUpload}
                className="hidden"
              />
            </label>
          </div>

          {statusMessage && (
            <div className="p-3 bg-emerald-500/10 border border-emerald-500/30 rounded-xl flex items-center gap-2 text-emerald-400 text-xs">
              <Check className="w-4 h-4 shrink-0" />
              <span>{statusMessage}</span>
            </div>
          )}

          {images.length > 0 && (
            <div className="space-y-2 max-h-[420px] overflow-y-auto pr-1">
              {images.map((img) => (
                <div
                  key={img.id}
                  className="flex items-center justify-between p-3 rounded-xl bg-slate-900 border border-slate-800 text-xs"
                >
                  <div className="flex items-center gap-3">
                    <img
                      src={img.previewUrl}
                      alt={img.name}
                      className="w-10 h-10 object-cover rounded-lg border border-slate-700"
                    />
                    <div>
                      <div className="font-medium text-slate-200 truncate max-w-[200px] sm:max-w-xs">
                        {img.name}
                      </div>
                      <div className="text-[11px] text-slate-500">
                        {img.width}x{img.height}px • {(img.originalSize / 1024).toFixed(1)} KB
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => processAndDownload(img)}
                      className="px-2.5 py-1.5 bg-slate-800 hover:bg-slate-700 text-sky-400 rounded-lg text-xs font-medium cursor-pointer"
                    >
                      Download
                    </button>
                    <button
                      onClick={() => removeImage(img.id)}
                      className="p-1.5 text-slate-500 hover:text-rose-400 cursor-pointer"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
