import React, { useState, useEffect } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useTheme } from '../../contexts/ThemeContext';
import { db, StickyNote } from '../../services/db';
import {
  TuiPageHeader,
  TuiButton,
  TuiCard,
  TuiInput,
  TuiTextarea,
  TuiChip,
} from '../../components/terminal/TerminalUI';

const NOTE_ACCENTS = [
  {
    name: 'GREEN' as const,
    border: 'border-[var(--tui-primary)]',
    header: 'text-[var(--tui-primary)]',
  },
  {
    name: 'AMBER' as const,
    border: 'border-[var(--tui-secondary)]',
    header: 'text-[var(--tui-secondary)]',
  },
  {
    name: 'CYAN' as const,
    border: 'border-[var(--tui-tertiary)]',
    header: 'text-[var(--tui-tertiary)]',
  },
  {
    name: 'RED' as const,
    border: 'border-[var(--tui-error)]',
    header: 'text-[var(--tui-error)]',
  },
];

export const StickyNotesView: React.FC = () => {
  const { t } = useI18n();
  const tt = t.tools.notes;
  const { theme } = useTheme();
  const [notes, setNotes] = useState<StickyNote[]>([]);
  const [search, setSearch] = useState('');
  const [newTitle, setNewTitle] = useState('');
  const [newContent, setNewContent] = useState('');
  const [selectedColor, setSelectedColor] = useState<'GREEN' | 'AMBER' | 'CYAN' | 'RED'>('GREEN');

  const loadNotes = async () => {
    try {
      const all = await db.notes.orderBy('updatedAt').reverse().toArray();
      all.sort((a, b) => (b.isPinned ? 1 : 0) - (a.isPinned ? 1 : 0));
      setNotes(all);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadNotes();
  }, []);

  const handleAddNote = async () => {
    if (!newTitle.trim() && !newContent.trim()) return;
    try {
      await db.notes.add({
        title: newTitle.trim() || 'UNTITLED_NOTE',
        content: newContent.trim(),
        color: selectedColor,
        isPinned: false,
        createdAt: Date.now(),
        updatedAt: Date.now(),
      });
      setNewTitle('');
      setNewContent('');
      loadNotes();
    } catch (e) {
      console.error(e);
    }
  };

  const handleTogglePin = async (note: StickyNote) => {
    if (!note.id) return;
    await db.notes.update(note.id, { isPinned: !note.isPinned, updatedAt: Date.now() });
    loadNotes();
  };

  const handleDelete = async (id?: number) => {
    if (!id) return;
    await db.notes.delete(id);
    loadNotes();
  };

  const handlePopOutPip = async (note: StickyNote) => {
    if (!('documentPictureInPicture' in window)) {
      alert(tt.pipUnsupported);
      return;
    }
    try {
      // @ts-ignore
      const pipWindow = await window.documentPictureInPicture.requestWindow({
        width: 320,
        height: 380,
      });

      const isDark = theme === 'dark';
      pipWindow.document.body.innerHTML = `
        <div style="font-family: 'JetBrains Mono', monospace; padding: 16px; background: ${isDark ? '#0d0d0d' : '#f4f4ee'}; color: ${isDark ? '#e2e2e2' : '#141414'}; border: 1px solid ${isDark ? '#00ff41' : '#007a1f'}; height: 100%; box-sizing: border-box; display: flex; flex-direction: column;">
          <div style="font-size: 14px; font-weight: bold; margin: 0 0 8px 0; color: ${isDark ? '#00ff41' : '#007a1f'}; border-bottom: 1px dashed #444444; padding-bottom: 6px;">> ${note.title}</div>
          <div style="font-size: 13px; line-height: 1.5; white-space: pre-wrap; flex: 1; overflow-y: auto;">${note.content}</div>
          <div style="font-size: 11px; color: #8b8b8b; margin-top: 8px; border-top: 1px dashed #222222; padding-top: 6px;">${tt.pipFooter}</div>
        </div>
      `;
    } catch (e) {
      console.error(e);
    }
  };

  const filteredNotes = notes.filter(
    (n) =>
      n.title.toLowerCase().includes(search.toLowerCase()) ||
      n.content.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="space-y-8">
      <TuiPageHeader
        command="sticky-notes"
        title={t.nav.stickyNotes}
        description={t.descriptions.stickyNotes}
        badge="[INDEXEDDB]"
        accent="secondary"
      />

      <TuiCard
        title={tt.composerTitle}
        rightMeta={
          <div className="flex flex-wrap items-center gap-1.5">
            {NOTE_ACCENTS.map((c) => (
              <TuiChip
                key={c.name}
                active={selectedColor === c.name}
                onClick={() => setSelectedColor(c.name)}
              >
                {tt.colors[c.name]}
              </TuiChip>
            ))}
          </div>
        }
        className="p-5"
        bodyClassName="space-y-4"
      >
        <TuiInput
          label={tt.titleLabel}
          prompt
          value={newTitle}
          onChange={(e) => setNewTitle(e.target.value)}
          placeholder={tt.titlePlaceholder}
        />

        <TuiTextarea
          label={tt.bodyLabel}
          value={newContent}
          onChange={(e) => setNewContent(e.target.value)}
          placeholder={tt.bodyPlaceholder}
          rows={3}
        />

        <div className="flex justify-end">
          <TuiButton variant="primary" prefix=">" onClick={handleAddNote}>
            {tt.saveBtn}
          </TuiButton>
        </div>
      </TuiCard>

      <TuiInput
        label={tt.grepLabel}
        prompt
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder={tt.grepPlaceholder}
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredNotes.map((note) => {
          const accent = NOTE_ACCENTS.find((c) => c.name === note.color) || NOTE_ACCENTS[0];
          return (
            <div
              key={note.id}
              className={`bg-[var(--tui-surface)] border ${accent.border} p-4 flex flex-col justify-between space-y-3`}
            >
              <div className="space-y-2">
                <div className="flex items-center justify-between gap-2 pb-2 border-b border-dashed border-[var(--tui-border-subtle)]">
                  <h3 className={`font-bold text-[14px] truncate flex-1 ${accent.header}`}>
                    &gt; {note.title}
                  </h3>
                  <div className="flex items-center gap-1 shrink-0">
                    <TuiButton
                      size="sm"
                      variant={note.isPinned ? 'amber' : 'ghost'}
                      onClick={() => handleTogglePin(note)}
                    >
                      {note.isPinned ? tt.unpin : tt.pin}
                    </TuiButton>
                    <TuiButton
                      size="sm"
                      variant="secondary"
                      onClick={() => handlePopOutPip(note)}
                    >
                      {tt.pip}
                    </TuiButton>
                    <TuiButton
                      size="sm"
                      variant="destructive"
                      onClick={() => handleDelete(note.id)}
                    >
                      [X]
                    </TuiButton>
                  </div>
                </div>

                <p className="text-[13px] whitespace-pre-wrap leading-[1.6] text-[var(--tui-text)] line-clamp-6">
                  {note.content}
                </p>
              </div>

              <div className="text-[11px] text-[var(--tui-text-dim)] pt-2 border-t border-dashed border-[var(--tui-border-subtle)] flex justify-between">
                <span>
                  {tt.datePrefix} {new Date(note.updatedAt).toLocaleDateString()}
                </span>
                {note.isPinned && (
                  <span className="text-[var(--tui-secondary)] font-bold">{tt.pinnedTag}</span>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {filteredNotes.length === 0 && (
        <TuiCard bodyClassName="text-center py-8 text-[12px] text-[var(--tui-text-dim)]">
          {tt.emptyState}
        </TuiCard>
      )}
    </div>
  );
};
