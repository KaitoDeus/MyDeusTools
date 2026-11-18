import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { PDFDocument } from 'pdf-lib';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiChip,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

type PdfTab = 'merge' | 'split';

interface UploadedPdf {
  id: string;
  name: string;
  size: number;
  file: File;
  pageCount?: number;
}

export const PdfToolkitView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.pdf;
  const [activeTab, setActiveTab] = useState<PdfTab>('merge');
  const [isProcessing, setIsProcessing] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  const [mergeFiles, setMergeFiles] = useState<UploadedPdf[]>([]);

  const handleMergeFilesUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    const newItems: UploadedPdf[] = [];
    for (const f of files) {
      try {
        const bytes = await f.arrayBuffer();
        const doc = await PDFDocument.load(bytes, { ignoreEncryption: true });
        newItems.push({
          id: Math.random().toString(36).substring(7),
          name: f.name,
          size: f.size,
          file: f,
          pageCount: doc.getPageCount(),
        });
      } catch {
        newItems.push({
          id: Math.random().toString(36).substring(7),
          name: f.name,
          size: f.size,
          file: f,
        });
      }
    }
    setMergeFiles((prev) => [...prev, ...newItems]);
  };

  const moveMergeItem = (idx: number, direction: 'up' | 'down') => {
    const targetIdx = direction === 'up' ? idx - 1 : idx + 1;
    if (targetIdx < 0 || targetIdx >= mergeFiles.length) return;
    const cloned = [...mergeFiles];
    const temp = cloned[idx];
    cloned[idx] = cloned[targetIdx];
    cloned[targetIdx] = temp;
    setMergeFiles(cloned);
  };

  const removeMergeItem = (id: string) => {
    setMergeFiles((prev) => prev.filter((item) => item.id !== id));
  };

  const executeMerge = async () => {
    if (mergeFiles.length < 2) {
      alert(tt.minFilesAlert);
      return;
    }
    setIsProcessing(true);
    setStatusMessage(null);

    try {
      const mergedPdf = await PDFDocument.create();

      for (const item of mergeFiles) {
        const bytes = await item.file.arrayBuffer();
        const doc = await PDFDocument.load(bytes);
        const copiedPages = await mergedPdf.copyPages(doc, doc.getPageIndices());
        copiedPages.forEach((page) => mergedPdf.addPage(page));
      }

      const mergedBytes = await mergedPdf.save();
      const blob = new Blob([mergedBytes as unknown as BlobPart], { type: 'application/pdf' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `merged_${Date.now()}.pdf`;
      a.click();
      setStatusMessage(tt.mergeSuccess);
    } catch (err: any) {
      alert(tt.mergeError + err.message);
    } finally {
      setIsProcessing(false);
    }
  };

  const [splitFile, setSplitFile] = useState<File | null>(null);
  const [splitPageCount, setSplitPageCount] = useState<number | null>(null);
  const [pageRange, setPageRange] = useState('1-2');

  const handleSplitFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setSplitFile(file);
    try {
      const bytes = await file.arrayBuffer();
      const doc = await PDFDocument.load(bytes);
      setSplitPageCount(doc.getPageCount());
      setPageRange(`1-${Math.min(doc.getPageCount(), 2)}`);
    } catch {
      setSplitPageCount(null);
    }
  };

  const executeSplit = async () => {
    if (!splitFile) return;
    setIsProcessing(true);
    setStatusMessage(null);

    try {
      const bytes = await splitFile.arrayBuffer();
      const doc = await PDFDocument.load(bytes);
      const total = doc.getPageCount();

      const pageIndices: number[] = [];
      const parts = pageRange.split(',').map((p) => p.trim());
      for (const part of parts) {
        if (part.includes('-')) {
          const [startStr, endStr] = part.split('-');
          const start = parseInt(startStr, 10);
          const end = parseInt(endStr, 10);
          for (let i = start; i <= end; i++) {
            if (i >= 1 && i <= total) pageIndices.push(i - 1);
          }
        } else {
          const p = parseInt(part, 10);
          if (p >= 1 && p <= total) pageIndices.push(p - 1);
        }
      }

      if (pageIndices.length === 0) {
        alert(tt.invalidRangeAlert);
        setIsProcessing(false);
        return;
      }

      const newPdf = await PDFDocument.create();
      const pages = await newPdf.copyPages(doc, pageIndices);
      pages.forEach((page) => newPdf.addPage(page));

      const newBytes = await newPdf.save();
      const blob = new Blob([newBytes as unknown as BlobPart], { type: 'application/pdf' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `split_${Date.now()}.pdf`;
      a.click();
      setStatusMessage(tt.splitSuccess);
    } catch (err: any) {
      alert(tt.splitError + err.message);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="pdf-toolkit"
        title={t.nav.pdfToolkit}
        description={t.descriptions.pdfToolkit}
        badge="[OFFLINE_PDF_LIB]"
        accent="secondary"
      />

      <div className="flex flex-wrap gap-2">
        <TuiChip active={activeTab === 'merge'} onClick={() => setActiveTab('merge')}>
          {tt.tabMerge}
        </TuiChip>
        <TuiChip active={activeTab === 'split'} onClick={() => setActiveTab('split')}>
          {tt.tabSplit}
        </TuiChip>
      </div>

      {statusMessage && <TuiAlert type="ok">{statusMessage}</TuiAlert>}

      {activeTab === 'merge' && (
        <div className="space-y-5">
          <div className="p-6 border border-dashed border-[var(--tui-border-medium)] hover:border-[var(--tui-primary)] bg-[var(--tui-surface)] flex flex-col items-center justify-center text-center space-y-3">
            <div className="text-[13px] font-bold text-[var(--tui-primary)]">{tt.loadTitle}</div>
            <p className="text-[12px] text-[var(--tui-text-muted)]">{tt.privacyHint}</p>
            <label className="inline-flex h-[36px] px-4 py-2 text-[13px] font-semibold uppercase border border-[var(--tui-primary)] text-[var(--tui-primary)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)] cursor-pointer items-center">
              <span>$ {t.common.browse.toUpperCase()}</span>
              <input
                type="file"
                multiple
                accept="application/pdf"
                onChange={handleMergeFilesUpload}
                className="hidden"
              />
            </label>
          </div>

          {mergeFiles.length > 0 && (
            <TuiCard
              title={`${tt.queueTitle} (${mergeFiles.length})`}
              rightMeta={
                <TuiButton size="sm" variant="destructive" onClick={() => setMergeFiles([])}>
                  [{t.common.clear.toUpperCase()}]
                </TuiButton>
              }
              bodyClassName="space-y-4"
            >
              <div>
                {mergeFiles.map((item, idx) => (
                  <div
                    key={item.id}
                    className="h-[40px] flex items-center justify-between px-2 border-b border-dashed border-[var(--tui-border-subtle)] hover:bg-[var(--tui-surface-elevated)] text-[13px]"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <span className="text-[11px] text-[var(--tui-text-dim)] w-[4ch] text-right">
                        {String(idx + 1).padStart(2, '0')}
                      </span>
                      <span className="font-semibold text-[var(--tui-text)] truncate">
                        {item.name}
                      </span>
                      <span className="text-[11px] text-[var(--tui-text-muted)] shrink-0">
                        [{(item.size / 1024).toFixed(1)}KB | {item.pageCount || '?'} {tt.pagesUnit}]
                      </span>
                    </div>

                    <div className="flex items-center gap-1 shrink-0">
                      <TuiButton
                        size="sm"
                        variant="ghost"
                        onClick={() => moveMergeItem(idx, 'up')}
                        disabled={idx === 0}
                      >
                        {tt.moveUp}
                      </TuiButton>
                      <TuiButton
                        size="sm"
                        variant="ghost"
                        onClick={() => moveMergeItem(idx, 'down')}
                        disabled={idx === mergeFiles.length - 1}
                      >
                        {tt.moveDown}
                      </TuiButton>
                      <TuiButton
                        size="sm"
                        variant="destructive"
                        onClick={() => removeMergeItem(item.id)}
                      >
                        [X]
                      </TuiButton>
                    </div>
                  </div>
                ))}
              </div>

              <TuiButton
                variant="primary"
                size="lg"
                prefix=">"
                onClick={executeMerge}
                disabled={isProcessing}
                className="w-full"
              >
                {isProcessing ? t.common.processing : tt.mergeAction}
              </TuiButton>
            </TuiCard>
          )}
        </div>
      )}

      {activeTab === 'split' && (
        <TuiCard title={tt.splitCardTitle} className="p-6" bodyClassName="space-y-5">
          <div className="space-y-1">
            <label className="text-[12px] font-medium text-[var(--tui-text-muted)]">
              // {tt.selectPdfLabel}
            </label>
            <input
              type="file"
              accept="application/pdf"
              onChange={handleSplitFileUpload}
              className="w-full text-[12px] text-[var(--tui-text-muted)] file:mr-4 file:py-2 file:px-4 file:border file:border-[var(--tui-primary)] file:bg-transparent file:text-[var(--tui-primary)] hover:file:bg-[var(--tui-primary)] hover:file:text-[var(--tui-primary-fg)] file:text-[12px] file:font-semibold file:uppercase cursor-pointer"
            />
          </div>

          {splitFile && (
            <div className="p-4 bg-[var(--tui-bg)] border border-[var(--tui-border-medium)] space-y-4">
              <div className="flex flex-wrap items-center justify-between text-[12px] text-[var(--tui-text-muted)]">
                <span>
                  {tt.targetLabel} <strong className="text-[var(--tui-text)]">{splitFile.name}</strong>
                </span>
                <span>
                  {tt.totalPagesLabel}{' '}
                  <strong className="text-[var(--tui-primary)]">{splitPageCount}</strong>
                </span>
              </div>

              <TuiInput
                label={tt.pageRangeLabel}
                prompt
                value={pageRange}
                onChange={(e) => setPageRange(e.target.value)}
              />

              <TuiButton
                variant="primary"
                prefix=">"
                onClick={executeSplit}
                disabled={isProcessing}
                className="w-full"
              >
                {isProcessing ? t.common.processing : tt.extractAction}
              </TuiButton>
            </div>
          )}
        </TuiCard>
      )}
    </div>
  );
};
