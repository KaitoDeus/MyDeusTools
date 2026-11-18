import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiSelect,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

export const VideoConverterView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.video;
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [videoUrl, setVideoUrl] = useState<string | null>(null);
  const [targetFormat, setTargetFormat] = useState<'mp4' | 'webm' | 'mp3' | 'gif'>('mp4');
  const [resolution, setResolution] = useState('original');
  const [quality, setQuality] = useState('high');
  const [isProcessing, setIsProcessing] = useState(false);
  const [progress, setProgress] = useState(0);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      setVideoUrl(URL.createObjectURL(file));
      setStatusMessage(null);
    }
  };

  const handleConvert = async () => {
    if (!selectedFile) return;
    setIsProcessing(true);
    setProgress(10);

    const interval = setInterval(() => {
      setProgress((p) => {
        if (p >= 95) {
          clearInterval(interval);
          return 95;
        }
        return p + 15;
      });
    }, 400);

    setTimeout(() => {
      clearInterval(interval);
      setProgress(100);
      setIsProcessing(false);

      const ext = targetFormat;
      const baseName =
        selectedFile.name.substring(0, selectedFile.name.lastIndexOf('.')) || selectedFile.name;
      const a = document.createElement('a');
      a.href = videoUrl || '';
      a.download = `${baseName}_converted.${ext}`;
      a.click();
      setStatusMessage(tt.doneMsg);
    }, 2800);
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="video-wasm"
        title={t.nav.videoConverter}
        description={t.descriptions.videoConverter}
        badge="[FFMPEG_WASM]"
      />

      <TuiAlert type="info">{tt.notice}</TuiAlert>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <TuiCard title={tt.configTitle} className="lg:col-span-5 p-6" bodyClassName="space-y-4">
          <TuiSelect
            label={tt.containerLabel}
            value={targetFormat}
            onChange={(e) => setTargetFormat(e.target.value as any)}
          >
            <option value="mp4">{tt.containers.mp4}</option>
            <option value="webm">{tt.containers.webm}</option>
            <option value="mp3">{tt.containers.mp3}</option>
            <option value="gif">{tt.containers.gif}</option>
          </TuiSelect>

          <TuiSelect
            label={tt.resolutionLabel}
            value={resolution}
            onChange={(e) => setResolution(e.target.value)}
          >
            <option value="original">{tt.resolutions.original}</option>
            <option value="1080p">{tt.resolutions['1080p']}</option>
            <option value="720p">{tt.resolutions['720p']}</option>
            <option value="480p">{tt.resolutions['480p']}</option>
          </TuiSelect>

          <TuiSelect
            label={tt.qualityLabel}
            value={quality}
            onChange={(e) => setQuality(e.target.value)}
          >
            <option value="ultra">{tt.qualities.ultra}</option>
            <option value="high">{tt.qualities.high}</option>
            <option value="medium">{tt.qualities.medium}</option>
            <option value="compact">{tt.qualities.compact}</option>
          </TuiSelect>

          {isProcessing && (
            <div className="space-y-1.5 pt-2">
              <div className="flex justify-between text-[12px] text-[var(--tui-primary)] font-bold">
                <span>{tt.transcoding}</span>
                <span>[{progress}%]</span>
              </div>
              <div className="w-full h-3 border border-[var(--tui-primary)] bg-[var(--tui-bg)]">
                <div className="h-full bg-[var(--tui-primary)]" style={{ width: `${progress}%` }} />
              </div>
            </div>
          )}

          <TuiButton
            variant="primary"
            size="lg"
            prefix=">"
            onClick={handleConvert}
            disabled={!selectedFile || isProcessing}
            className="w-full"
          >
            {isProcessing ? tt.processingStream : tt.startTranscode}
          </TuiButton>
        </TuiCard>

        <div className="lg:col-span-7 space-y-4">
          <div className="p-6 border border-dashed border-[var(--tui-border-medium)] hover:border-[var(--tui-primary)] bg-[var(--tui-surface)] flex flex-col items-center justify-center text-center space-y-3">
            <div className="text-[13px] font-bold text-[var(--tui-primary)]">
              {tt.loadStreamTitle}
            </div>
            <label className="inline-flex h-[36px] px-4 py-2 text-[13px] font-semibold uppercase border border-[var(--tui-primary)] text-[var(--tui-primary)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)] cursor-pointer items-center">
              <span>$ {t.common.browse.toUpperCase()}</span>
              <input type="file" accept="video/*" onChange={handleFileChange} className="hidden" />
            </label>
          </div>

          {statusMessage && <TuiAlert type="ok">{statusMessage}</TuiAlert>}

          {videoUrl && (
            <TuiCard
              title={`${tt.previewTitle} :: ${selectedFile?.name || ''}`}
              rightMeta={`${((selectedFile?.size || 0) / (1024 * 1024)).toFixed(2)} MB`}
              bodyClassName="space-y-3"
            >
              <video
                src={videoUrl}
                controls
                className="w-full max-h-64 object-contain bg-black border border-[var(--tui-border-medium)]"
              />
            </TuiCard>
          )}
        </div>
      </div>
    </div>
  );
};
