import React, { useState, useEffect } from 'react';
import { useI18n } from '../../i18n/LanguageContext';
import { useTheme } from '../../contexts/ThemeContext';
import { db, StickyNote } from '../../services/db';
import {
  StickyNote as StickyNoteIcon,
  Plus,
  Trash2,
  Pin,
  Search,
  ExternalLink,
} from 'lucide-react';

const NOTE_COLORS = [
  {
    name: 'Amber',
    bg: 'bg-amber-50 border-amber-200 text-amber-950 dark:bg-amber-950/60 dark:border-amber-600/40 dark:text-amber-100',
    dot: 'bg-amber-400',
  },
  {
    name: 'Emerald',
    bg: 'bg-emerald-50 border-emerald-200 text-emerald-950 dark:bg-emerald-950/60 dark:border-emerald-600/40 dark:text-emerald-100',
    dot: 'bg-emerald-400',
  },
  {
    name: 'Sky',
    bg: 'bg-sky-50 border-sky-200 text-sky-950 dark:bg-sky-950/60 dark:border-sky-600/40 dark:text-sky-100',
    dot: 'bg-sky-400',
  },
  {
    name: 'Purple',
    bg: 'bg-purple-50 border-purple-200 text-purple-950 dark:bg-purple-950/60 dark:border-purple-600/40 dark:text-purple-100',
    dot: 'bg-purple-400',
  },
  {
    name: 'Rose',
    bg: 'bg-rose-50 border-rose-200 text-rose-950 dark:bg-rose-950/60 dark:border-rose-600/40 dark:text-rose-100',
    dot: 'bg-rose-400',
  },
];

export const StickyNotesView: React.FC = () => {
  const { t } = useI18n();
  const { theme } = useTheme();
  const [notes, setNotes] = useState<StickyNote[]>([]);
  const [search, setSearch] = useState('');
  const [newTitle, setNewTitle] = useState('');
  const [newContent, setNewContent] = useState('');
  const [selectedColor, setSelectedColor] = useState(NOTE_COLORS[0].name);

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
        title: newTitle.trim() || 'Untitled Note',
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
      alert('Document Picture-in-Picture is supported in modern Chrome/Edge browsers.');
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
        <div style="font-family: sans-serif; padding: 16px; background: ${isDark ? '#0f172a' : '#fffbeb'}; color: ${isDark ? '#f8fafc' : '#451a03'}; height: 100%; box-sizing: border-box; display: flex; flex-direction: column;">
          <h2 style="font-size: 16px; font-weight: bold; margin: 0 0 8px 0; color: ${isDark ? '#fbbf24' : '#b45309'};">${note.title}</h2>
          <div style="font-size: 13px; line-height: 1.5; white-space: pre-wrap; flex: 1; overflow-y: auto;">${note.content}</div>
          <div style="font-size: 10px; color: #64748b; margin-top: 8px;">MyDeusTools Web Note • Always-on-Top</div>
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
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
          <StickyNoteIcon className="w-6 h-6 text-amber-500 dark:text-amber-400" />
          <span>{t.nav.stickyNotes}</span>
        </h2>
        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">{t.descriptions.stickyNotes}</p>
      </div>

      {/* Create New Note Card */}
      <div className="glass-card p-5 rounded-2xl space-y-4">
        <div className="flex items-center justify-between">
          <span className="text-xs font-semibold text-slate-700 dark:text-slate-300 flex items-center gap-1.5">
            <Plus className="w-4 h-4 text-amber-500 dark:text-amber-400" />
            <span>Create New Sticky Note</span>
          </span>
          <div className="flex items-center gap-1.5">
            {NOTE_COLORS.map((c) => (
              <button
                key={c.name}
                onClick={() => setSelectedColor(c.name)}
                className={`w-5 h-5 rounded-full ${c.dot} transition-all cursor-pointer ${
                  selectedColor === c.name
                    ? 'ring-2 ring-slate-900 dark:ring-white scale-110'
                    : 'opacity-60 hover:opacity-100'
                }`}
              />
            ))}
          </div>
        </div>

        <input
          type="text"
          value={newTitle}
          onChange={(e) => setNewTitle(e.target.value)}
          placeholder="Note Title..."
          className="w-full px-3.5 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs font-semibold text-slate-900 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-amber-500/50"
        />

        <textarea
          value={newContent}
          onChange={(e) => setNewContent(e.target.value)}
          placeholder="Write your note content here..."
          rows={3}
          className="w-full p-3.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs text-slate-900 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-amber-500/50"
        />

        <div className="flex justify-end">
          <button
            onClick={handleAddNote}
            className="px-5 py-2 bg-amber-600 hover:bg-amber-500 text-white rounded-xl text-xs font-semibold transition-all shadow-lg shadow-amber-600/20 cursor-pointer"
          >
            {t.common.save} Note
          </button>
        </div>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="w-4 h-4 absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search notes..."
          className="w-full pl-10 pr-4 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs text-slate-900 dark:text-slate-200 focus:outline-none"
        />
      </div>

      {/* Notes Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredNotes.map((note) => {
          const colorObj = NOTE_COLORS.find((c) => c.name === note.color) || NOTE_COLORS[0];
          return (
            <div
              key={note.id}
              className={`p-5 rounded-2xl border flex flex-col justify-between space-y-3 transition-all shadow-xs dark:shadow-none ${colorObj.bg}`}
            >
              <div className="space-y-2">
                <div className="flex items-center justify-between gap-2">
                  <h3 className="font-bold text-sm truncate flex-1">{note.title}</h3>
                  <div className="flex items-center gap-1 shrink-0">
                    <button
                      onClick={() => handleTogglePin(note)}
                      className={`p-1 rounded-md cursor-pointer ${
                        note.isPinned
                          ? 'text-amber-600 dark:text-amber-400'
                          : 'text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white'
                      }`}
                      title={note.isPinned ? 'Unpin' : 'Pin to top'}
                    >
                      <Pin className="w-3.5 h-3.5" />
                    </button>
                    <button
                      onClick={() => handlePopOutPip(note)}
                      className="p-1 text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white rounded-md cursor-pointer"
                      title="Float Always-on-Top (Picture-in-Picture)"
                    >
                      <ExternalLink className="w-3.5 h-3.5" />
                    </button>
                    <button
                      onClick={() => handleDelete(note.id)}
                      className="p-1 text-slate-500 hover:text-rose-500 dark:text-slate-400 dark:hover:text-rose-400 rounded-md cursor-pointer"
                    >
                      <Trash2 className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </div>

                <p className="text-xs whitespace-pre-wrap leading-relaxed opacity-90 line-clamp-6">
                  {note.content}
                </p>
              </div>

              <div className="text-[10px] opacity-60 pt-2 border-t border-black/10 dark:border-white/10 flex justify-between">
                <span>{new Date(note.updatedAt).toLocaleDateString()}</span>
                {note.isPinned && <span className="font-bold">PINNED</span>}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
