import React, { useState, useEffect } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { db, ClipboardSnippet } from '../../services/db';
import {
  ClipboardList,
  Copy,
  Check,
  Plus,
  Trash2,
  Pin,
  Search,
  Tag,
  ClipboardPaste,
} from 'lucide-react';

export const ClipboardHubView: React.FC = () => {
  const { t } = useI18n();
  const [snippets, setSnippets] = useState<ClipboardSnippet[]>([]);
  const [search, setSearch] = useState('');
  const [copiedId, setCopiedId] = useState<number | null>(null);
  const [newTitle, setNewTitle] = useState('');
  const [newContent, setNewContent] = useState('');
  const [newTag, setNewTag] = useState('Code');

  const loadSnippets = async () => {
    try {
      const all = await db.snippets.orderBy('createdAt').reverse().toArray();
      all.sort((a, b) => (b.isPinned ? 1 : 0) - (a.isPinned ? 1 : 0));
      setSnippets(all);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadSnippets();
  }, []);

  const handleCopy = async (snippet: ClipboardSnippet) => {
    if (!snippet.id) return;
    await navigator.clipboard.writeText(snippet.content);
    await db.snippets.update(snippet.id, { usageCount: (snippet.usageCount || 0) + 1 });
    setCopiedId(snippet.id);
    setTimeout(() => setCopiedId(null), 2000);
    loadSnippets();
  };

  const handleAdd = async () => {
    if (!newContent.trim()) return;
    try {
      await db.snippets.add({
        title: newTitle.trim() || 'Untitled Snippet',
        content: newContent.trim(),
        tag: newTag.trim() || 'General',
        isPinned: false,
        usageCount: 0,
        createdAt: Date.now(),
      });
      setNewTitle('');
      setNewContent('');
      loadSnippets();
    } catch (e) {
      console.error(e);
    }
  };

  const handlePasteFromSystem = async () => {
    try {
      const text = await navigator.clipboard.readText();
      if (!text) return;
      await db.snippets.add({
        title: text.substring(0, 30) + '...',
        content: text,
        tag: 'Clipboard',
        isPinned: false,
        usageCount: 0,
        createdAt: Date.now(),
      });
      loadSnippets();
    } catch {
      alert('Please allow clipboard read permission.');
    }
  };

  const handleDelete = async (id?: number) => {
    if (!id) return;
    await db.snippets.delete(id);
    loadSnippets();
  };

  const handleTogglePin = async (s: ClipboardSnippet) => {
    if (!s.id) return;
    await db.snippets.update(s.id, { isPinned: !s.isPinned });
    loadSnippets();
  };

  const filtered = snippets.filter(
    (s) =>
      s.title.toLowerCase().includes(search.toLowerCase()) ||
      s.content.toLowerCase().includes(search.toLowerCase()) ||
      s.tag.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
          <ClipboardList className="w-6 h-6 text-blue-600 dark:text-blue-400" />
          <span>{t.nav.clipboardManager}</span>
        </h2>
        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
          {t.descriptions.clipboardManager}
        </p>
      </div>

      {/* Add New Snippet Form */}
      <div className="glass-card p-5 rounded-2xl space-y-4">
        <div className="flex flex-col sm:flex-row gap-3">
          <input
            type="text"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            placeholder="Snippet Title..."
            className="flex-1 px-3.5 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs font-semibold text-slate-900 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/50"
          />
          <input
            type="text"
            value={newTag}
            onChange={(e) => setNewTag(e.target.value)}
            placeholder="Tag (e.g. Code, SQL, Email)..."
            className="w-full sm:w-48 px-3.5 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs text-slate-900 dark:text-slate-200 focus:outline-none"
          />
        </div>

        <textarea
          value={newContent}
          onChange={(e) => setNewContent(e.target.value)}
          placeholder="Paste or write snippet text here..."
          rows={3}
          className="w-full p-3.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs font-mono text-slate-900 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/50"
        />

        <div className="flex items-center justify-between">
          <button
            onClick={handlePasteFromSystem}
            className="px-3.5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 border border-slate-200 dark:border-transparent dark:bg-slate-800 dark:hover:bg-slate-700 dark:text-slate-300 rounded-xl text-xs font-medium transition-colors cursor-pointer flex items-center gap-1.5"
          >
            <ClipboardPaste className="w-3.5 h-3.5 text-blue-600 dark:text-blue-400" />
            <span>Paste from OS Clipboard</span>
          </button>

          <button
            onClick={handleAdd}
            className="px-5 py-2 bg-blue-600 hover:bg-blue-500 text-white rounded-xl text-xs font-semibold shadow-lg shadow-blue-600/20 cursor-pointer flex items-center gap-1.5"
          >
            <Plus className="w-3.5 h-3.5" />
            <span>Save Snippet</span>
          </button>
        </div>
      </div>

      {/* Search Bar */}
      <div className="relative">
        <Search className="w-4 h-4 absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search snippets by title, content, or tag..."
          className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs text-slate-900 dark:text-slate-200 focus:outline-none"
        />
      </div>

      {/* Snippets List */}
      <div className="space-y-3">
        {filtered.map((s) => (
          <div
            key={s.id}
            className="p-4 rounded-xl glass-card hover:border-slate-300 dark:hover:border-slate-700 transition-all space-y-2 group"
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="font-semibold text-xs text-slate-900 dark:text-slate-200">
                  {s.title}
                </span>
                <span className="text-[10px] px-2 py-0.5 rounded-md bg-blue-500/10 text-blue-600 dark:text-blue-400 border border-blue-500/20 flex items-center gap-1">
                  <Tag className="w-2.5 h-2.5" />
                  <span>{s.tag}</span>
                </span>
                {s.isPinned && (
                  <span className="text-[10px] text-amber-600 dark:text-amber-400 font-bold">
                    PINNED
                  </span>
                )}
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={() => handleCopy(s)}
                  className="px-3 py-1 bg-blue-500/10 hover:bg-blue-600 text-blue-600 dark:bg-blue-600/20 dark:text-blue-400 hover:text-white rounded-lg text-xs font-medium transition-colors flex items-center gap-1 cursor-pointer"
                >
                  {copiedId === s.id ? (
                    <Check className="w-3.5 h-3.5 text-emerald-500 dark:text-emerald-400" />
                  ) : (
                    <Copy className="w-3.5 h-3.5" />
                  )}
                  <span>{copiedId === s.id ? t.common.copied : t.common.copy}</span>
                </button>

                <button
                  onClick={() => handleTogglePin(s)}
                  className={`p-1.5 rounded-lg cursor-pointer ${
                    s.isPinned
                      ? 'text-amber-600 dark:text-amber-400'
                      : 'text-slate-400 hover:text-slate-700 dark:text-slate-500 dark:hover:text-slate-300'
                  }`}
                >
                  <Pin className="w-3.5 h-3.5" />
                </button>

                <button
                  onClick={() => handleDelete(s.id)}
                  className="p-1.5 text-slate-400 hover:text-rose-500 dark:text-slate-500 dark:hover:text-rose-400 rounded-lg cursor-pointer"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </div>
            </div>

            <div className="p-3 bg-slate-100 dark:bg-slate-950 rounded-lg border border-slate-200 dark:border-slate-800/80 font-mono text-xs text-slate-800 dark:text-slate-300 max-h-24 overflow-y-auto whitespace-pre-wrap select-all">
              {s.content}
            </div>

            <div className="text-[10px] text-slate-500 flex justify-between">
              <span>Used {s.usageCount || 0} times</span>
              <span>{new Date(s.createdAt).toLocaleDateString()}</span>
            </div>
          </div>
        ))}

        {filtered.length === 0 && (
          <div className="text-center py-12 text-slate-500 text-xs">
            No snippets found. Add your frequently used text templates and code blocks!
          </div>
        )}
      </div>
    </div>
  );
};
