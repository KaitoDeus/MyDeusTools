import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiTextarea,
  TuiChip,
  TuiAlert,
} from '../../components/terminal/TerminalUI';

type Tab = 'json' | 'base64' | 'hash' | 'case' | 'url';

export const TextDevToolsView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.textDev;
  const [activeTab, setActiveTab] = useState<Tab>('json');
  const [copiedKey, setCopiedKey] = useState<string | null>(null);

  const copyToClipboard = (text: string, key: string) => {
    navigator.clipboard.writeText(text);
    setCopiedKey(key);
    setTimeout(() => setCopiedKey(null), 2000);
  };

  // --- JSON State ---
  const [jsonInput, setJsonInput] = useState(
    '{\n  "app": "MyDeusTools Web",\n  "version": "1.0.0",\n  "features": ["json", "base64", "hash"]\n}'
  );
  const [jsonOutput, setJsonOutput] = useState('');
  const [jsonError, setJsonError] = useState<string | null>(null);

  const handleJsonFormat = () => {
    try {
      const parsed = JSON.parse(jsonInput);
      setJsonOutput(JSON.stringify(parsed, null, 2));
      setJsonError(null);
    } catch (err: any) {
      setJsonError(err.message);
    }
  };

  const handleJsonMinify = () => {
    try {
      const parsed = JSON.parse(jsonInput);
      setJsonOutput(JSON.stringify(parsed));
      setJsonError(null);
    } catch (err: any) {
      setJsonError(err.message);
    }
  };

  // --- Base64 State ---
  const [base64Input, setBase64Input] = useState('Hello, MyDeusTools Web!');
  const [base64Output, setBase64Output] = useState('');

  const handleBase64Encode = () => {
    try {
      const encoded = btoa(unescape(encodeURIComponent(base64Input)));
      setBase64Output(encoded);
    } catch (e: any) {
      setBase64Output(tt.encodeError + e.message);
    }
  };

  const handleBase64Decode = () => {
    try {
      const decoded = decodeURIComponent(escape(atob(base64Input)));
      setBase64Output(decoded);
    } catch {
      setBase64Output(tt.invalidBase64);
    }
  };

  const handleBase64FileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      setBase64Output(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  // --- Hash State ---
  const [hashInput, setHashInput] = useState('MyDeusTools Web 2026');
  const [hashes, setHashes] = useState<Record<string, string>>({});
  const [isHashing, setIsHashing] = useState(false);

  const computeHashes = async () => {
    setIsHashing(true);
    const enc = new TextEncoder();
    const data = enc.encode(hashInput);
    const results: Record<string, string> = {};

    try {
      const buf1 = await crypto.subtle.digest('SHA-1', data);
      results['SHA-1'] = Array.from(new Uint8Array(buf1))
        .map((b) => b.toString(16).padStart(2, '0'))
        .join('');

      const buf256 = await crypto.subtle.digest('SHA-256', data);
      results['SHA-256'] = Array.from(new Uint8Array(buf256))
        .map((b) => b.toString(16).padStart(2, '0'))
        .join('');

      const buf512 = await crypto.subtle.digest('SHA-512', data);
      results['SHA-512'] = Array.from(new Uint8Array(buf512))
        .map((b) => b.toString(16).padStart(2, '0'))
        .join('');
    } catch (e) {
      console.error(e);
    }

    setHashes(results);
    setIsHashing(false);
  };

  // --- Case Converter State ---
  const [caseInput, setCaseInput] = useState('user profile settings controller');

  const getCaseVariants = (text: string) => {
    const words = text
      .trim()
      .replace(/[-_]/g, ' ')
      .split(/\s+/)
      .filter(Boolean);

    if (words.length === 0) return {};

    const lowerWords = words.map((w) => w.toLowerCase());
    return {
      camelCase: lowerWords
        .map((w, i) => (i === 0 ? w : w.charAt(0).toUpperCase() + w.slice(1)))
        .join(''),
      PascalCase: lowerWords.map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(''),
      snake_case: lowerWords.join('_'),
      'kebab-case': lowerWords.join('-'),
      UPPERCASE: text.toUpperCase(),
      lowercase: text.toLowerCase(),
      'Title Case': lowerWords.map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(' '),
    };
  };

  // --- URL & HTML State ---
  const [urlInput, setUrlInput] = useState(
    'https://example.com/search?q=MyDeusTools Web&filter=active'
  );
  const [urlOutput, setUrlOutput] = useState('');

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="text-devtools"
        title={t.nav.textDevTools}
        description={t.descriptions.textDevTools}
        badge="[DEV]"
      />

      {/* Tabs */}
      <div className="flex flex-wrap gap-2">
        {(
          [
            { id: 'json', label: tt.tabs.json },
            { id: 'base64', label: tt.tabs.base64 },
            { id: 'hash', label: tt.tabs.hash },
            { id: 'case', label: tt.tabs.case },
            { id: 'url', label: tt.tabs.url },
          ] as const
        ).map((tab) => (
          <TuiChip
            key={tab.id}
            active={activeTab === tab.id}
            onClick={() => setActiveTab(tab.id as Tab)}
          >
            {tab.label}
          </TuiChip>
        ))}
      </div>

      {/* Tab 1: JSON Studio */}
      {activeTab === 'json' && (
        <div className="space-y-4">
          <div className="flex flex-wrap gap-3">
            <TuiButton variant="primary" prefix=">" onClick={handleJsonFormat}>
              {tt.formatJson}
            </TuiButton>
            <TuiButton variant="secondary" prefix="$" onClick={handleJsonMinify}>
              {tt.minifyJson}
            </TuiButton>
            <TuiButton
              variant="destructive"
              onClick={() => {
                setJsonInput('');
                setJsonOutput('');
                setJsonError(null);
              }}
            >
              [{t.common.clear.toUpperCase()}]
            </TuiButton>
          </div>

          {jsonError && (
            <TuiAlert type="err">
              {tt.syntaxError}: {jsonError}
            </TuiAlert>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <TuiTextarea
              label={tt.inputJsonLabel}
              value={jsonInput}
              onChange={(e) => setJsonInput(e.target.value)}
              rows={14}
              className="text-[var(--tui-primary)]"
            />
            <TuiTextarea
              label={tt.outputJsonLabel}
              readOnly
              value={jsonOutput}
              placeholder={tt.outputPlaceholder}
              rows={14}
              rightAction={
                jsonOutput && (
                  <TuiButton
                    size="sm"
                    variant="primary"
                    onClick={() => copyToClipboard(jsonOutput, 'json')}
                  >
                    {copiedKey === 'json'
                      ? `[${t.common.copied.toUpperCase()}]`
                      : `[${t.common.copy.toUpperCase()}]`}
                  </TuiButton>
                )
              }
            />
          </div>
        </div>
      )}

      {/* Tab 2: Base64 */}
      {activeTab === 'base64' && (
        <div className="space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div className="flex flex-wrap gap-2">
              <TuiButton variant="primary" prefix=">" onClick={handleBase64Encode}>
                {tt.encodeText}
              </TuiButton>
              <TuiButton variant="secondary" prefix="$" onClick={handleBase64Decode}>
                {tt.decodeBase64}
              </TuiButton>
            </div>

            <label className="inline-flex h-[36px] px-4 py-2 text-[13px] font-semibold uppercase border border-[var(--tui-secondary)] text-[var(--tui-secondary)] hover:bg-[var(--tui-secondary)] hover:text-[var(--tui-primary-fg)] cursor-pointer items-center gap-2">
              <span>{tt.fileToBase64}</span>
              <input type="file" onChange={handleBase64FileUpload} className="hidden" />
            </label>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <TuiTextarea
              label={tt.inputDataLabel}
              value={base64Input}
              onChange={(e) => setBase64Input(e.target.value)}
              rows={10}
            />
            <TuiTextarea
              label={tt.outputBase64Label}
              readOnly
              value={base64Output}
              placeholder={tt.encodedPlaceholder}
              rows={10}
              rightAction={
                base64Output && (
                  <TuiButton
                    size="sm"
                    variant="primary"
                    onClick={() => copyToClipboard(base64Output, 'b64')}
                  >
                    {copiedKey === 'b64'
                      ? `[${t.common.copied.toUpperCase()}]`
                      : `[${t.common.copy.toUpperCase()}]`}
                  </TuiButton>
                )
              }
            />
          </div>
        </div>
      )}

      {/* Tab 3: Crypto Hash */}
      {activeTab === 'hash' && (
        <div className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-3 items-end">
            <div className="flex-1 w-full">
              <TuiInput
                label={tt.textToHashLabel}
                prompt
                value={hashInput}
                onChange={(e) => setHashInput(e.target.value)}
              />
            </div>
            <TuiButton
              variant="primary"
              prefix=">"
              onClick={computeHashes}
              disabled={isHashing}
              className="shrink-0"
            >
              {isHashing ? tt.computing : tt.generateHashes}
            </TuiButton>
          </div>

          <div className="grid grid-cols-1 gap-3 pt-2">
            {['SHA-256', 'SHA-512', 'SHA-1'].map((algo) => {
              const val = hashes[algo] || tt.digestEmptyHint;
              return (
                <TuiCard
                  key={algo}
                  title={`${tt.digestPrefix} :: ${algo}`}
                  rightMeta={
                    hashes[algo] && (
                      <TuiButton
                        size="sm"
                        variant="ghost"
                        onClick={() => copyToClipboard(val, algo)}
                      >
                        {copiedKey === algo
                          ? `[${t.common.copied.toUpperCase()}]`
                          : `[${t.common.copy.toUpperCase()}]`}
                      </TuiButton>
                    )
                  }
                >
                  <div className="text-[13px] text-[var(--tui-text)] break-all select-all">{val}</div>
                </TuiCard>
              );
            })}
          </div>
        </div>
      )}

      {/* Tab 4: Case Converter */}
      {activeTab === 'case' && (
        <div className="space-y-4">
          <TuiTextarea
            label={tt.originalTextLabel}
            value={caseInput}
            onChange={(e) => setCaseInput(e.target.value)}
            rows={3}
          />

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {Object.entries(getCaseVariants(caseInput)).map(([variantName, transformed]) => (
              <TuiCard
                key={variantName}
                interactive
                onClick={() => copyToClipboard(transformed, variantName)}
                title={variantName}
                rightMeta={
                  copiedKey === variantName
                    ? `[${t.common.copied.toUpperCase()}]`
                    : tt.clickToCopy
                }
              >
                <div className="text-[13px] text-[var(--tui-text)] truncate">{transformed}</div>
              </TuiCard>
            ))}
          </div>
        </div>
      )}

      {/* Tab 5: URL & HTML */}
      {activeTab === 'url' && (
        <div className="space-y-4">
          <div className="flex flex-wrap gap-2">
            <TuiButton
              variant="primary"
              prefix=">"
              onClick={() => setUrlOutput(encodeURIComponent(urlInput))}
            >
              {tt.urlEncode}
            </TuiButton>
            <TuiButton
              variant="secondary"
              prefix="$"
              onClick={() => {
                try {
                  setUrlOutput(decodeURIComponent(urlInput));
                } catch {
                  setUrlOutput(tt.invalidUrl);
                }
              }}
            >
              {tt.urlDecode}
            </TuiButton>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <TuiTextarea
              label={tt.inputStringLabel}
              value={urlInput}
              onChange={(e) => setUrlInput(e.target.value)}
              rows={8}
            />
            <TuiTextarea
              label={tt.resultStringLabel}
              readOnly
              value={urlOutput}
              placeholder={tt.encodedPlaceholder}
              rows={8}
              rightAction={
                urlOutput && (
                  <TuiButton
                    size="sm"
                    variant="primary"
                    onClick={() => copyToClipboard(urlOutput, 'url')}
                  >
                    {copiedKey === 'url'
                      ? `[${t.common.copied.toUpperCase()}]`
                      : `[${t.common.copy.toUpperCase()}]`}
                  </TuiButton>
                )
              }
            />
          </div>
        </div>
      )}
    </div>
  );
};
