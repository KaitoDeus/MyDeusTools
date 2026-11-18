import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import JSZip from 'jszip';
import {
  FolderSync,
  UploadCloud,
  Download,
  FolderOpen,
  Sparkles,
  Check,
  AlertTriangle,
  RotateCcw,
} from 'lucide-react';

interface FileToRename {
  id: string;
  originalName: string;
  newName: string;
  file?: File;
  handle?: any;
}

export const BulkRenamerView: React.FC = () => {
  const { t } = useI18n();
  const [files, setFiles] = useState<FileToRename[]>([]);
  const [findText, setFindText] = useState('');
  const [replaceText, setReplaceText] = useState('');
  const [prefix, setPrefix] = useState('');
  const [suffix, setSuffix] = useState('');
  const [caseMode, setCaseMode] = useState<string>('none');
  const [isProcessing, setIsProcessing] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  // File Upload
  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const uploaded = Array.from(e.target.files || []);
    if (uploaded.length === 0) return;

    const list: FileToRename[] = uploaded.map((f) => ({
      id: Math.random().toString(36).substring(7),
      originalName: f.name,
      newName: f.name,
      file: f,
    }));
    setFiles(list);
    computeNewNames(list, findText, replaceText, prefix, suffix, caseMode);
  };

  // Modern File System Access API (Chromium)
  const handleOpenDirectory = async () => {
    if (!('showDirectoryPicker' in window)) {
      alert('Direct folder renaming is supported in Chrome/Edge via File System Access API. You can still upload files directly!');
      return;
    }
    try {
      // @ts-ignore
      const dirHandle = await window.showDirectoryPicker({ mode: 'readwrite' });
      const list: FileToRename[] = [];
      for await (const entry of dirHandle.values()) {
        if (entry.kind === 'file') {
          list.push({
            id: Math.random().toString(36).substring(7),
            originalName: entry.name,
            newName: entry.name,
            handle: entry,
          });
        }
      }
      setFiles(list);
      computeNewNames(list, findText, replaceText, prefix, suffix, caseMode);
    } catch {
      // User cancelled picker
    }
  };

  const computeNewNames = (
    currentFiles: FileToRename[],
    find: string,
    replace: string,
    pre: string,
    suf: string,
    mode: string
  ) => {
    const updated = currentFiles.map((item) => {
      let ext = '';
      let base = item.originalName;
      const dotIdx = item.originalName.lastIndexOf('.');
      if (dotIdx > 0) {
        ext = item.originalName.substring(dotIdx);
        base = item.originalName.substring(0, dotIdx);
      }

      // 1. Find & Replace
      if (find) {
        try {
          const regex = new RegExp(find, 'g');
          base = base.replace(regex, replace);
        } catch {
          base = base.split(find).join(replace);
        }
      }

      // 2. Case transformation
      if (mode === 'uppercase') base = base.toUpperCase();
      if (mode === 'lowercase') base = base.toLowerCase();
      if (mode === 'titlecase') base = base.charAt(0).toUpperCase() + base.slice(1);

      // 3. Prefix & Suffix
      base = pre + base + suf;

      return {
        ...item,
        newName: base + ext,
      };
    });
    setFiles(updated);
  };

  const applyRule = (find: string, replace: string, pre: string, suf: string, mode: string) => {
    setFindText(find);
    setReplaceText(replace);
    setPrefix(pre);
    setSuffix(suf);
    setCaseMode(mode);
    computeNewNames(files, find, replace, pre, suf, mode);
  };

  const handleDownloadRenamedZip = async () => {
    if (files.length === 0) return;
    setIsProcessing(true);
    try {
      const zip = new JSZip();
      for (const item of files) {
        if (item.file) {
          zip.file(item.newName, item.file);
        }
      }
      const zipBlob = await zip.generateAsync({ type: 'blob' });
      const url = URL.createObjectURL(zipBlob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `renamed_files_${Date.now()}.zip`;
      a.click();
      setStatusMessage('Renamed files successfully packaged and downloaded as ZIP!');
    } catch (err: any) {
      alert('Error creating ZIP: ' + err.message);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <FolderSync className="w-6 h-6 text-amber-500" />
          <span>{t.nav.bulkRenamer}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.bulkRenamer}</p>
      </div>

      {/* Directory & Upload Row */}
      <div className="flex flex-wrap items-center gap-3">
        <button
          onClick={handleOpenDirectory}
          className="px-4 py-2.5 bg-amber-600 hover:bg-amber-500 text-white rounded-xl text-xs font-semibold shadow-lg shadow-amber-600/20 transition-all flex items-center gap-2 cursor-pointer"
        >
          <FolderOpen className="w-4 h-4" />
          <span>Select Disk Folder (File System Access)</span>
        </button>

        <label className="px-4 py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-all flex items-center gap-2 cursor-pointer">
          <UploadCloud className="w-4 h-4 text-amber-400" />
          <span>Select Batch Files</span>
          <input type="file" multiple onChange={handleFileUpload} className="hidden" />
        </label>
      </div>

      {/* Rule Form */}
      <div className="glass-card p-5 rounded-2xl border border-slate-800 space-y-4">
        <h3 className="text-xs font-semibold text-slate-300 uppercase tracking-wider">
          Renaming Rules
        </h3>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="space-y-1.5">
            <label className="text-xs font-medium text-slate-400">Find Text / Regex</label>
            <input
              type="text"
              value={findText}
              onChange={(e) => applyRule(e.target.value, replaceText, prefix, suffix, caseMode)}
              placeholder="e.g. IMG_, copy"
              className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-medium text-slate-400">Replace With</label>
            <input
              type="text"
              value={replaceText}
              onChange={(e) => applyRule(findText, e.target.value, prefix, suffix, caseMode)}
              placeholder="e.g. Photo_"
              className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-medium text-slate-400">Add Prefix</label>
            <input
              type="text"
              value={prefix}
              onChange={(e) => applyRule(findText, replaceText, e.target.value, suffix, caseMode)}
              placeholder="e.g. 2026_"
              className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-medium text-slate-400">Case Transformation</label>
            <select
              value={caseMode}
              onChange={(e) => applyRule(findText, replaceText, prefix, suffix, e.target.value)}
              className="w-full p-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-medium text-slate-200 focus:outline-none"
            >
              <option value="none">No case change</option>
              <option value="uppercase">UPPERCASE</option>
              <option value="lowercase">lowercase</option>
              <option value="titlecase">Title Case</option>
            </select>
          </div>
        </div>
      </div>

      {statusMessage && (
        <div className="p-3 bg-emerald-500/10 border border-emerald-500/30 rounded-xl flex items-center gap-2 text-emerald-400 text-xs">
          <Check className="w-4 h-4 shrink-0" />
          <span>{statusMessage}</span>
        </div>
      )}

      {/* Live Preview Table */}
      {files.length > 0 && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-300">
              Live Preview ({files.length} files)
            </span>
            <button
              onClick={handleDownloadRenamedZip}
              disabled={isProcessing}
              className="px-4 py-2 bg-amber-600 hover:bg-amber-500 text-white rounded-xl text-xs font-bold uppercase tracking-wider shadow-lg shadow-amber-600/20 transition-all flex items-center gap-1.5 cursor-pointer"
            >
              <Download className="w-3.5 h-3.5" />
              <span>{isProcessing ? t.common.processing : 'Download Renamed (ZIP)'}</span>
            </button>
          </div>

          <div className="glass-card rounded-2xl border border-slate-800 overflow-hidden">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-900/90 text-slate-400 border-b border-slate-800">
                <tr>
                  <th className="p-3 font-semibold">Original File Name</th>
                  <th className="p-3 font-semibold">New Name Preview</th>
                  <th className="p-3 font-semibold text-right">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60 font-mono">
                {files.map((item) => {
                  const isChanged = item.originalName !== item.newName;
                  return (
                    <tr key={item.id} className="hover:bg-slate-800/30 transition-colors">
                      <td className="p-3 text-slate-400 truncate max-w-xs">{item.originalName}</td>
                      <td className="p-3 text-slate-200 font-semibold truncate max-w-xs">
                        {item.newName}
                      </td>
                      <td className="p-3 text-right">
                        {isChanged ? (
                          <span className="inline-block px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                            Renamed
                          </span>
                        ) : (
                          <span className="inline-block px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-800 text-slate-400">
                            Unchanged
                          </span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
