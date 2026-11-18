import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { PDFDocument } from 'pdf-lib';
import {
  FileText,
  UploadCloud,
  Download,
  Trash2,
  MoveUp,
  MoveDown,
  Layers,
  Scissors,
  Image as ImageIcon,
  ShieldCheck,
  Check,
  AlertCircle,
} from 'lucide-react';

type PdfTab = 'merge' | 'split' | 'images' | 'metadata';

interface UploadedPdf {
  id: string;
  name: string;
  size: number;
  file: File;
  pageCount?: number;
}

export const PdfToolkitView: React.FC = () => {
  const { t } = useI18n();
  const [activeTab, setActiveTab] = useState<PdfTab>('merge');
  const [isProcessing, setIsProcessing] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  // --- Merge State ---
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
      alert('Please select at least 2 PDF files to merge.');
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
      setStatusMessage('PDFs successfully merged and downloaded!');
    } catch (err: any) {
      alert('Error merging PDFs: ' + err.message);
    } finally {
      setIsProcessing(false);
    }
  };

  // --- Split State ---
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

      // Parse range: e.g. "1-3, 5"
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
        alert('Invalid page range entered.');
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
      setStatusMessage('Split PDF successfully downloaded!');
    } catch (err: any) {
      alert('Error splitting PDF: ' + err.message);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <FileText className="w-6 h-6 text-rose-500" />
          <span>{t.nav.pdfToolkit}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.pdfToolkit}</p>
      </div>

      {/* Tabs */}
      <div className="flex flex-wrap gap-2 border-b border-slate-800 pb-3">
        {[
          { id: 'merge', label: 'Merge PDFs', icon: Layers },
          { id: 'split', label: 'Split Pages', icon: Scissors },
        ].map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as PdfTab)}
              className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
                activeTab === tab.id
                  ? 'bg-rose-600 text-white shadow-lg shadow-rose-600/20'
                  : 'bg-slate-900 text-slate-400 hover:text-slate-200 hover:bg-slate-800'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {statusMessage && (
        <div className="p-3 bg-emerald-500/10 border border-emerald-500/30 rounded-xl flex items-center gap-2 text-emerald-400 text-xs">
          <Check className="w-4 h-4 shrink-0" />
          <span>{statusMessage}</span>
        </div>
      )}

      {/* Tab 1: Merge */}
      {activeTab === 'merge' && (
        <div className="space-y-5">
          <div className="p-6 border-2 border-dashed border-slate-800 hover:border-rose-500/50 rounded-2xl flex flex-col items-center justify-center text-center space-y-3 glass-card transition-all">
            <UploadCloud className="w-10 h-10 text-rose-400 opacity-80" />
            <div className="space-y-1">
              <p className="text-sm font-semibold text-slate-200">
                Drag and drop PDF files here, or browse
              </p>
              <p className="text-xs text-slate-500">
                100% client-side memory processing. Files never leave your browser.
              </p>
            </div>
            <label className="px-5 py-2.5 bg-rose-600 hover:bg-rose-500 text-white rounded-xl text-xs font-semibold shadow-lg shadow-rose-600/20 transition-all cursor-pointer">
              <span>{t.common.browse}</span>
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
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-slate-400">
                  Files to Merge ({mergeFiles.length})
                </span>
                <button
                  onClick={() => setMergeFiles([])}
                  className="text-xs text-slate-500 hover:text-rose-400 cursor-pointer"
                >
                  {t.common.clear}
                </button>
              </div>

              <div className="space-y-2">
                {mergeFiles.map((item, idx) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between p-3 rounded-xl bg-slate-900 border border-slate-800 text-xs"
                  >
                    <div className="flex items-center gap-3">
                      <span className="w-6 h-6 rounded-lg bg-slate-800 text-slate-400 font-bold flex items-center justify-center text-[10px]">
                        {idx + 1}
                      </span>
                      <div>
                        <div className="font-medium text-slate-200">{item.name}</div>
                        <div className="text-[11px] text-slate-500">
                          {(item.size / 1024).toFixed(1)} KB • {item.pageCount || '?'} pages
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => moveMergeItem(idx, 'up')}
                        disabled={idx === 0}
                        className="p-1.5 text-slate-400 hover:text-white disabled:opacity-30 cursor-pointer"
                      >
                        <MoveUp className="w-3.5 h-3.5" />
                      </button>
                      <button
                        onClick={() => moveMergeItem(idx, 'down')}
                        disabled={idx === mergeFiles.length - 1}
                        className="p-1.5 text-slate-400 hover:text-white disabled:opacity-30 cursor-pointer"
                      >
                        <MoveDown className="w-3.5 h-3.5" />
                      </button>
                      <button
                        onClick={() => removeMergeItem(item.id)}
                        className="p-1.5 text-slate-500 hover:text-rose-400 cursor-pointer"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>

              <div className="pt-2">
                <button
                  onClick={executeMerge}
                  disabled={isProcessing}
                  className="w-full py-3 bg-gradient-to-r from-rose-600 to-pink-600 hover:from-rose-500 hover:to-pink-500 text-white rounded-xl text-xs font-bold uppercase tracking-wider shadow-lg shadow-rose-600/25 transition-all cursor-pointer flex items-center justify-center gap-2"
                >
                  <Download className="w-4 h-4" />
                  <span>{isProcessing ? t.common.processing : 'Merge & Download PDF'}</span>
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Tab 2: Split */}
      {activeTab === 'split' && (
        <div className="space-y-5 glass-card p-6 rounded-2xl border border-slate-800">
          <div className="space-y-2">
            <label className="text-xs font-semibold text-slate-300">Select PDF File</label>
            <input
              type="file"
              accept="application/pdf"
              onChange={handleSplitFileUpload}
              className="w-full text-xs text-slate-400 file:mr-4 file:py-2.5 file:px-4 file:rounded-xl file:border-0 file:text-xs file:font-semibold file:bg-rose-600 file:text-white hover:file:bg-rose-500 cursor-pointer"
            />
          </div>

          {splitFile && (
            <div className="p-4 bg-slate-900 rounded-xl space-y-4 border border-slate-800">
              <div className="flex items-center justify-between text-xs text-slate-400">
                <span>Selected: <strong className="text-slate-200">{splitFile.name}</strong></span>
                <span>Total Pages: <strong className="text-rose-400">{splitPageCount}</strong></span>
              </div>

              <div className="space-y-1.5">
                <label className="text-xs font-medium text-slate-300">Page Range (e.g. 1-3, 5)</label>
                <input
                  type="text"
                  value={pageRange}
                  onChange={(e) => setPageRange(e.target.value)}
                  className="w-full p-2.5 bg-slate-950 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none focus:ring-2 focus:ring-rose-500/50"
                />
              </div>

              <button
                onClick={executeSplit}
                disabled={isProcessing}
                className="w-full py-2.5 bg-rose-600 hover:bg-rose-500 text-white rounded-xl text-xs font-semibold transition-all cursor-pointer flex items-center justify-center gap-2"
              >
                <Download className="w-4 h-4" />
                <span>{isProcessing ? t.common.processing : 'Extract Pages & Download'}</span>
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
