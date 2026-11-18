import React, { useState } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import {
  Code,
  FileCode,
  Hash,
  Type,
  Link2,
  Copy,
  Check,
  RotateCcw,
  Sparkles,
  AlertCircle,
  FileUp,
} from 'lucide-react';

type Tab = 'json' | 'base64' | 'hash' | 'case' | 'url';

export const TextDevToolsView: React.FC = () => {
  const { t } = useI18n();
  const [activeTab, setActiveTab] = useState<Tab>('json');
  const [copiedKey, setCopiedKey] = useState<string | null>(null);

  const copyToClipboard = (text: string, key: string) => {
    navigator.clipboard.writeText(text);
    setCopiedKey(key);
    setTimeout(() => setCopiedKey(null), 2000);
  };

  // --- JSON State ---
  const [jsonInput, setJsonInput] = useState('{\n  "app": "MyDeusTools Web",\n  "version": "1.0.0",\n  "features": ["json", "base64", "hash"]\n}');
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
  const [base64FileMode, setBase64FileMode] = useState(false);

  const handleBase64Encode = () => {
    try {
      const encoded = btoa(unescape(encodeURIComponent(base64Input)));
      setBase64Output(encoded);
    } catch (e: any) {
      setBase64Output('Error encoding: ' + e.message);
    }
  };

  const handleBase64Decode = () => {
    try {
      const decoded = decodeURIComponent(escape(atob(base64Input)));
      setBase64Output(decoded);
    } catch (e: any) {
      setBase64Output('Invalid Base64 string');
    }
  };

  const handleBase64FileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      const res = reader.result as string;
      setBase64Output(res);
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
      // SHA-1
      const buf1 = await crypto.subtle.digest('SHA-1', data);
      results['SHA-1'] = Array.from(new Uint8Array(buf1))
        .map((b) => b.toString(16).padStart(2, '0'))
        .join('');

      // SHA-256
      const buf256 = await crypto.subtle.digest('SHA-256', data);
      results['SHA-256'] = Array.from(new Uint8Array(buf256))
        .map((b) => b.toString(16).padStart(2, '0'))
        .join('');

      // SHA-512
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
      'camelCase': lowerWords.map((w, i) => (i === 0 ? w : w.charAt(0).toUpperCase() + w.slice(1))).join(''),
      'PascalCase': lowerWords.map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(''),
      'snake_case': lowerWords.join('_'),
      'kebab-case': lowerWords.join('-'),
      'UPPERCASE': text.toUpperCase(),
      'lowercase': text.toLowerCase(),
      'Title Case': lowerWords.map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(' '),
    };
  };

  // --- URL & HTML State ---
  const [urlInput, setUrlInput] = useState('https://example.com/search?q=MyDeusTools Web&filter=active');
  const [urlOutput, setUrlOutput] = useState('');

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-white flex items-center gap-2">
          <Code className="w-6 h-6 text-emerald-400" />
          <span>{t.nav.textDevTools}</span>
        </h2>
        <p className="text-sm text-slate-400 mt-1">{t.descriptions.textDevTools}</p>
      </div>

      {/* Tabs */}
      <div className="flex flex-wrap gap-2 border-b border-slate-800 pb-3">
        {[
          { id: 'json', label: 'JSON Studio', icon: FileCode },
          { id: 'base64', label: 'Base64', icon: Sparkles },
          { id: 'hash', label: 'Crypto Hash', icon: Hash },
          { id: 'case', label: 'Case Converter', icon: Type },
          { id: 'url', label: 'URL & Entities', icon: Link2 },
        ].map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as Tab)}
              className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
                activeTab === tab.id
                  ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-600/20'
                  : 'bg-slate-900 text-slate-400 hover:text-slate-200 hover:bg-slate-800'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {/* Tab 1: JSON Studio */}
      {activeTab === 'json' && (
        <div className="space-y-4">
          <div className="flex flex-wrap gap-3">
            <button
              onClick={handleJsonFormat}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl text-xs font-semibold transition-colors cursor-pointer flex items-center gap-2"
            >
              <Sparkles className="w-4 h-4" />
              Format (2 Spaces)
            </button>
            <button
              onClick={handleJsonMinify}
              className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-colors cursor-pointer"
            >
              Minify JSON
            </button>
            <button
              onClick={() => {
                setJsonInput('');
                setJsonOutput('');
                setJsonError(null);
              }}
              className="px-4 py-2 bg-slate-900 hover:bg-slate-800 text-slate-400 rounded-xl text-xs font-semibold transition-colors cursor-pointer flex items-center gap-1.5"
            >
              <RotateCcw className="w-3.5 h-3.5" />
              {t.common.clear}
            </button>
          </div>

          {jsonError && (
            <div className="p-3 bg-rose-500/10 border border-rose-500/30 rounded-xl flex items-center gap-2 text-rose-400 text-xs">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>Syntax Error: {jsonError}</span>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-xs font-medium text-slate-400">Input JSON</label>
              <textarea
                value={jsonInput}
                onChange={(e) => setJsonInput(e.target.value)}
                rows={14}
                className="w-full p-4 bg-slate-900/90 border border-slate-800 rounded-xl text-xs font-mono text-emerald-300 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 resize-y"
              />
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="text-xs font-medium text-slate-400">Formatted Output</label>
                {jsonOutput && (
                  <button
                    onClick={() => copyToClipboard(jsonOutput, 'json')}
                    className="text-xs text-emerald-400 hover:underline flex items-center gap-1 cursor-pointer"
                  >
                    {copiedKey === 'json' ? <Check className="w-3 h-3" /> : <Copy className="w-3 h-3" />}
                    <span>{copiedKey === 'json' ? t.common.copied : t.common.copy}</span>
                  </button>
                )}
              </div>
              <textarea
                readOnly
                value={jsonOutput}
                placeholder="Result will appear here..."
                rows={14}
                className="w-full p-4 bg-slate-950 border border-slate-800/80 rounded-xl text-xs font-mono text-slate-200 focus:outline-none resize-y"
              />
            </div>
          </div>
        </div>
      )}

      {/* Tab 2: Base64 */}
      {activeTab === 'base64' && (
        <div className="space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div className="flex gap-2">
              <button
                onClick={handleBase64Encode}
                className="px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl text-xs font-semibold transition-colors cursor-pointer"
              >
                Encode Text
              </button>
              <button
                onClick={handleBase64Decode}
                className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-colors cursor-pointer"
              >
                Decode Base64
              </button>
            </div>

            <label className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-colors cursor-pointer flex items-center gap-2">
              <FileUp className="w-4 h-4 text-emerald-400" />
              <span>File to Base64</span>
              <input type="file" onChange={handleBase64FileUpload} className="hidden" />
            </label>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-xs font-medium text-slate-400">Input String / Data</label>
              <textarea
                value={base64Input}
                onChange={(e) => setBase64Input(e.target.value)}
                rows={10}
                className="w-full p-4 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 resize-y"
              />
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="text-xs font-medium text-slate-400">Base64 Result</label>
                {base64Output && (
                  <button
                    onClick={() => copyToClipboard(base64Output, 'b64')}
                    className="text-xs text-emerald-400 hover:underline flex items-center gap-1 cursor-pointer"
                  >
                    {copiedKey === 'b64' ? <Check className="w-3 h-3" /> : <Copy className="w-3 h-3" />}
                    <span>{copiedKey === 'b64' ? t.common.copied : t.common.copy}</span>
                  </button>
                )}
              </div>
              <textarea
                readOnly
                value={base64Output}
                placeholder="Encoded/Decoded result..."
                rows={10}
                className="w-full p-4 bg-slate-950 border border-slate-800/80 rounded-xl text-xs font-mono text-slate-300 focus:outline-none resize-y"
              />
            </div>
          </div>
        </div>
      )}

      {/* Tab 3: Crypto Hash */}
      {activeTab === 'hash' && (
        <div className="space-y-4">
          <div className="space-y-2">
            <label className="text-xs font-medium text-slate-400">Text to Hash</label>
            <div className="flex gap-3">
              <input
                type="text"
                value={hashInput}
                onChange={(e) => setHashInput(e.target.value)}
                className="flex-1 px-4 py-2.5 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500/50"
              />
              <button
                onClick={computeHashes}
                disabled={isHashing}
                className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl text-xs font-semibold transition-colors cursor-pointer shrink-0"
              >
                {isHashing ? 'Computing...' : 'Generate Hashes'}
              </button>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-3 pt-2">
            {['SHA-256', 'SHA-512', 'SHA-1'].map((algo) => {
              const val = hashes[algo] || 'Click "Generate Hashes" to compute';
              return (
                <div key={algo} className="p-4 bg-slate-900/60 border border-slate-800/80 rounded-xl space-y-1.5">
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-bold text-emerald-400">{algo}</span>
                    {hashes[algo] && (
                      <button
                        onClick={() => copyToClipboard(val, algo)}
                        className="text-xs text-slate-400 hover:text-white flex items-center gap-1 cursor-pointer"
                      >
                        {copiedKey === algo ? <Check className="w-3 h-3 text-emerald-400" /> : <Copy className="w-3 h-3" />}
                        <span>{copiedKey === algo ? t.common.copied : t.common.copy}</span>
                      </button>
                    )}
                  </div>
                  <div className="text-xs font-mono text-slate-300 break-all select-all">{val}</div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Tab 4: Case Converter */}
      {activeTab === 'case' && (
        <div className="space-y-4">
          <div className="space-y-2">
            <label className="text-xs font-medium text-slate-400">Original Text</label>
            <textarea
              value={caseInput}
              onChange={(e) => setCaseInput(e.target.value)}
              rows={3}
              className="w-full p-4 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500/50"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {Object.entries(getCaseVariants(caseInput)).map(([variantName, transformed]) => (
              <div
                key={variantName}
                onClick={() => copyToClipboard(transformed, variantName)}
                className="p-3.5 bg-slate-900/60 border border-slate-800 hover:border-emerald-500/40 rounded-xl cursor-pointer group transition-all"
              >
                <div className="flex items-center justify-between text-[11px] text-slate-400 mb-1">
                  <span className="font-semibold text-slate-300">{variantName}</span>
                  <span className="text-emerald-400 opacity-0 group-hover:opacity-100 transition-opacity">
                    {copiedKey === variantName ? t.common.copied : t.common.copy}
                  </span>
                </div>
                <div className="text-xs font-mono text-slate-200 truncate">{transformed}</div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Tab 5: URL & HTML */}
      {activeTab === 'url' && (
        <div className="space-y-4">
          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => setUrlOutput(encodeURIComponent(urlInput))}
              className="px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl text-xs font-semibold transition-colors cursor-pointer"
            >
              URL Encode
            </button>
            <button
              onClick={() => {
                try {
                  setUrlOutput(decodeURIComponent(urlInput));
                } catch {
                  setUrlOutput('Invalid encoded URL');
                }
              }}
              className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-xs font-semibold transition-colors cursor-pointer"
            >
              URL Decode
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-xs font-medium text-slate-400">Input String</label>
              <textarea
                value={urlInput}
                onChange={(e) => setUrlInput(e.target.value)}
                rows={8}
                className="w-full p-4 bg-slate-900 border border-slate-800 rounded-xl text-xs font-mono text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 resize-y"
              />
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="text-xs font-medium text-slate-400">Result</label>
                {urlOutput && (
                  <button
                    onClick={() => copyToClipboard(urlOutput, 'url')}
                    className="text-xs text-emerald-400 hover:underline flex items-center gap-1 cursor-pointer"
                  >
                    {copiedKey === 'url' ? <Check className="w-3 h-3" /> : <Copy className="w-3 h-3" />}
                    <span>{copiedKey === 'url' ? t.common.copied : t.common.copy}</span>
                  </button>
                )}
              </div>
              <textarea
                readOnly
                value={urlOutput}
                placeholder="Encoded/Decoded result..."
                rows={8}
                className="w-full p-4 bg-slate-950 border border-slate-800/80 rounded-xl text-xs font-mono text-slate-300 focus:outline-none resize-y"
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
