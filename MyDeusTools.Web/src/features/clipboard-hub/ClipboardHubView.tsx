import React, { useState, useEffect } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { db, ClipboardSnippet } from '../../services/db';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiTextarea,
  TuiChip,
} from '../../components/terminal/TerminalUI';

export const ClipboardHubView: React.FC = () => {
  const { t } = useI18n();
  const [snippets, setSnippets] = useState<ClipboardSnippet[]>([]);
  const [search, setSearch] = useState('');
  const [copiedId, setCopiedId] = useState<number | null>(null);
  const [newTitle, setNewTitle] = useState('');
  const [newContent, setNewContent] = useState('');
  const [newTag, setNewTag] = useState('CODE');

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
        title: newTitle.trim() || 'UNTITLED_SNIPPET',
        content: newContent.trim(),
        tag: newTag.trim().toUpperCase() || 'GENERAL',
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
        tag: 'CLIPBOARD',
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
    <div className="space-y-8">
      <TuiPageHeader
        command="clip-vault"
        title={t.nav.clipboardManager}
        description={t.descriptions.clipboardManager}
        badge="[CLIPBOARD_API]"
        accent="tertiary"
      />

      {/* Add Snippet Form */}
      <TuiCard title="ADD_SNIPPET_ENTRY" className="space-y-4">
        <div className="grid grid-cols-1 sm:grid-cols-12 gap-3">
          <div className="sm:col-span-8">
            <TuiInput
              label="snippet_title"
              prompt
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              placeholder="Snippet identifier..."
            />
          </div>
          <div className="sm:col-span-4">
            <TuiInput
              label="tag"
              value={newTag}
              onChange={(e) => setNewTag(e.target.value)}
              placeholder="CODE, SQL, AUTH..."
            />
          </div>
        </div>

        <TuiTextarea
          label="payload"
          value={newContent}
          onChange={(e) => setNewContent(e.target.value)}
          placeholder="Paste or write snippet payload here..."
          rows={3}
        />

        <div className="flex flex-wrap items-center justify-between gap-3">
          <TuiButton variant="secondary" prefix="$" onClick={handlePasteFromSystem}>
            PASTE_OS_CLIPBOARD
          </TuiButton>

          <TuiButton variant="primary" prefix=">" onClick={handleAdd}>
            SAVE_SNIPPET
          </TuiButton>
        </div>
      </TuiCard>

      {/* Search */}
      <TuiInput
        label="grep_snippets"
        prompt
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder="Search snippets by title, content, or tag..."
      />

      {/* Snippets List */}
      <div className="space-y-3">
        {filtered.map((s) => (
          <TuiCard
            key={s.id}
            className="space-y-3"
            title={`> ${s.title}`}
            rightMeta={
              <div className="flex items-center gap-2">
                <TuiChip tone="tertiary">#{s.tag}</TuiChip>
                {s.isPinned && <TuiChip tone="secondary">[PINNED]</TuiChip>}
              </div>
            }
          >
            <div className="p-3 bg-[var(--tui-bg)] border border-[var(--tui-border-subtle)] text-[13px] text-[var(--tui-text)] max-h-28 overflow-y-auto whitespace-pre-wrap select-all">
              {s.content}
            </div>

            <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-dashed border-[var(--tui-border-subtle)] text-[11px] text-[var(--tui-text-dim)]">
              <div className="flex items-center gap-4">
                <span>USES: {s.usageCount || 0}</span>
                <span>CREATED: {new Date(s.createdAt).toLocaleDateString()}</span>
              </div>

              <div className="flex items-center gap-2">
                <TuiButton size="sm" variant="primary" onClick={() => handleCopy(s)}>
                  {copiedId === s.id
                    ? `[${t.common.copied.toUpperCase()}]`
                    : `[${t.common.copy.toUpperCase()}]`}
                </TuiButton>
                <TuiButton
                  size="sm"
                  variant={s.isPinned ? 'amber' : 'ghost'}
                  onClick={() => handleTogglePin(s)}
                >
                  {s.isPinned ? '[UNPIN]' : '[PIN]'}
                </TuiButton>
                <TuiButton size="sm" variant="destructive" onClick={() => handleDelete(s.id)}>
                  [DEL]
                </TuiButton>
              </div>
            </div>
          </TuiCard>
        ))}

        {filtered.length === 0 && (
          <TuiCard className="text-center py-12 text-[var(--tui-text-dim)] text-[12px]">
            [EMPTY_VAULT] No snippets matched query.
          </TuiCard>
        )}
      </div>
    </div>
  );
};
