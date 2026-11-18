import Dexie, { Table } from 'dexie';

export interface StickyNote {
  id?: number;
  title: string;
  content: string;
  color: string;
  isPinned: boolean;
  createdAt: number;
  updatedAt: number;
}

export interface ClipboardSnippet {
  id?: number;
  title: string;
  content: string;
  tag: string;
  isPinned: boolean;
  usageCount: number;
  createdAt: number;
}

export interface SavedColor {
  id?: number;
  hex: string;
  rgb: string;
  hsl: string;
  hsv: string;
  label: string;
  createdAt: number;
}

export class MyDeusToolsDatabase extends Dexie {
  notes!: Table<StickyNote, number>;
  snippets!: Table<ClipboardSnippet, number>;
  colors!: Table<SavedColor, number>;

  constructor() {
    super('MyDeusToolsWebDB');
    this.version(1).stores({
      notes: '++id, title, isPinned, createdAt, updatedAt',
      snippets: '++id, title, tag, isPinned, usageCount, createdAt',
      colors: '++id, hex, label, createdAt',
    });
  }
}

export const db = new MyDeusToolsDatabase();
