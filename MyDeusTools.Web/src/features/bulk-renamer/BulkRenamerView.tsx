import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import JSZip from 'jszip';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiSelect,
  TuiChip,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

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
  const [suffix] = useState('');
  const [caseMode, setCaseMode] = useState<string>('none');
  const [isProcessing, setIsProcessing] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

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

  const handleOpenDirectory = async () => {
    if (!('showDirectoryPicker' in window)) {
      alert(
        'Direct folder renaming is supported in Chrome/Edge via File System Access API. You can still upload files directly!'
      );
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

      if (find) {
        try {
          const regex = new RegExp(find, 'g');
          base = base.replace(regex, replace);
        } catch {
          base = base.split(find).join(replace);
        }
      }

      if (mode === 'uppercase') base = base.toUpperCase();
      if (mode === 'lowercase') base = base.toLowerCase();
      if (mode === 'titlecase') base = base.charAt(0).toUpperCase() + base.slice(1);

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
      setStatusMessage('Renamed files packaged and downloaded as ZIP (EXIT_CODE 0).');
    } catch (err: any) {
      alert('Error creating ZIP: ' + err.message);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="bulk-rename"
        title={t.nav.bulkRenamer}
        description={t.descriptions.bulkRenamer}
        badge="[FS_ACCESS_API]"
        accent="secondary"
      />

      {/* Directory & Upload Row */}
      <div className="flex flex-wrap items-center gap-3">
        <TuiButton variant="amber" prefix=">" onClick={handleOpenDirectory}>
          MOUNT_DISK_DIRECTORY
        </TuiButton>

        <label className="inline-flex h-[36px] px-4 py-2 text-[13px] font-semibold uppercase border border-[var(--tui-primary)] text-[var(--tui-primary)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)] cursor-pointer items-center">
          <span>$ SELECT_BATCH_FILES</span>
          <input type="file" multiple onChange={handleFileUpload} className="hidden" />
        </label>
      </div>

      {/* Rule Form */}
      <TuiCard title="RENAMING_PIPELINE_RULES">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <TuiInput
            label="find_regex"
            value={findText}
            onChange={(e) => applyRule(e.target.value, replaceText, prefix, suffix, caseMode)}
            placeholder="e.g. IMG_, copy"
          />
          <TuiInput
            label="replace_with"
            value={replaceText}
            onChange={(e) => applyRule(findText, e.target.value, prefix, suffix, caseMode)}
            placeholder="e.g. Photo_"
          />
          <TuiInput
            label="add_prefix"
            value={prefix}
            onChange={(e) => applyRule(findText, replaceText, e.target.value, suffix, caseMode)}
            placeholder="e.g. 2026_"
          />
          <TuiSelect
            label="case_mode"
            value={caseMode}
            onChange={(e) => applyRule(findText, replaceText, prefix, suffix, e.target.value)}
          >
            <option value="none">UNCHANGED</option>
            <option value="uppercase">UPPERCASE</option>
            <option value="lowercase">lowercase</option>
            <option value="titlecase">Title Case</option>
          </TuiSelect>
        </div>
      </TuiCard>

      {statusMessage && <TuiAlert type="ok">{statusMessage}</TuiAlert>}

      {/* Live Preview Table */}
      {files.length > 0 && (
        <TuiCard
          title={`LIVE_DRY_RUN (${files.length} FILES)`}
          rightMeta={
            <TuiButton
              size="sm"
              variant="primary"
              prefix=">"
              onClick={handleDownloadRenamedZip}
              disabled={isProcessing}
            >
              {isProcessing ? t.common.processing : 'EXPORT_ZIP'}
            </TuiButton>
          }
          className="overflow-x-auto"
        >
          <table className="w-full text-left text-[12px]">
            <thead>
              <tr className="border-b border-[var(--tui-border-medium)] text-[var(--tui-text-muted)] uppercase text-[11px]">
                <th className="py-2 pr-3 w-[4ch]">#</th>
                <th className="py-2 pr-3">ORIGINAL_NAME</th>
                <th className="py-2 pr-3">TARGET_PREVIEW</th>
                <th className="py-2 text-right">STATUS</th>
              </tr>
            </thead>
            <tbody>
              {files.map((item, idx) => {
                const isChanged = item.originalName !== item.newName;
                return (
                  <tr
                    key={item.id}
                    className="h-[32px] border-b border-dashed border-[var(--tui-border-subtle)] hover:bg-[var(--tui-surface-elevated)]"
                  >
                    <td className="py-1.5 pr-3 text-[var(--tui-text-dim)]">
                      {String(idx + 1).padStart(2, '0')}
                    </td>
                    <td className="py-1.5 pr-3 text-[var(--tui-text-muted)] truncate max-w-xs">
                      {item.originalName}
                    </td>
                    <td className="py-1.5 pr-3 text-[var(--tui-text)] font-semibold truncate max-w-xs">
                      {isChanged ? `> ${item.newName}` : item.newName}
                    </td>
                    <td className="py-1.5 text-right">
                      {isChanged ? (
                        <TuiChip tone="primary">MODIFIED</TuiChip>
                      ) : (
                        <TuiChip tone="muted">NOOP</TuiChip>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </TuiCard>
      )}
    </div>
  );
};
