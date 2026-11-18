import {
  MousePointerClick,
  Timer,
  StickyNote as StickyNoteIcon,
  ClipboardList,
  QrCode,
  Pipette,
  Binary,
  PictureInPicture2,
  Video,
  Image as ImageIcon,
  FileText,
  FolderSync,
  LucideIcon,
} from 'lucide-react';
import { Translations } from '../i18n/translations';

export type ToolCategory = 'productivity' | 'developer' | 'media' | 'system';

export interface ToolItem {
  id: string;
  nameKey: keyof Translations['nav'];
  descKey: keyof Translations['descriptions'];
  icon: LucideIcon;
  category: ToolCategory;
  badge?: string;
  gradient: string;
  isNew?: boolean;
}

export const TOOLS_CONFIG: ToolItem[] = [
  // Productivity
  {
    id: 'sticky-notes',
    nameKey: 'stickyNotes',
    descKey: 'stickyNotes',
    icon: StickyNoteIcon,
    category: 'productivity',
    gradient: 'from-amber-500 to-orange-500',
  },
  {
    id: 'clipboard-manager',
    nameKey: 'clipboardManager',
    descKey: 'clipboardManager',
    icon: ClipboardList,
    category: 'productivity',
    gradient: 'from-blue-500 to-cyan-500',
  },
  {
    id: 'smart-timer',
    nameKey: 'scheduleShutdown',
    descKey: 'scheduleShutdown',
    icon: Timer,
    category: 'productivity',
    badge: 'Smart Focus',
    gradient: 'from-rose-500 to-pink-500',
  },

  // Developer
  {
    id: 'text-devtools',
    nameKey: 'textDevTools',
    descKey: 'textDevTools',
    icon: Binary,
    category: 'developer',
    gradient: 'from-emerald-500 to-teal-500',
  },
  {
    id: 'qr-studio',
    nameKey: 'qrStudio',
    descKey: 'qrStudio',
    icon: QrCode,
    category: 'developer',
    gradient: 'from-violet-500 to-purple-500',
  },
  {
    id: 'color-picker',
    nameKey: 'colorPicker',
    descKey: 'colorPicker',
    icon: Pipette,
    category: 'developer',
    badge: 'EyeDropper API',
    gradient: 'from-fuchsia-500 to-pink-500',
  },

  // Media
  {
    id: 'pdf-toolkit',
    nameKey: 'pdfToolkit',
    descKey: 'pdfToolkit',
    icon: FileText,
    category: 'media',
    badge: '100% Offline',
    gradient: 'from-red-500 to-rose-600',
  },
  {
    id: 'image-studio',
    nameKey: 'imageStudio',
    descKey: 'imageStudio',
    icon: ImageIcon,
    category: 'media',
    gradient: 'from-sky-500 to-indigo-500',
  },
  {
    id: 'video-converter',
    nameKey: 'videoConverter',
    descKey: 'videoConverter',
    icon: Video,
    category: 'media',
    badge: 'WASM',
    gradient: 'from-purple-500 to-indigo-600',
  },

  // System & Files
  {
    id: 'bulk-renamer',
    nameKey: 'bulkRenamer',
    descKey: 'bulkRenamer',
    icon: FolderSync,
    category: 'system',
    badge: 'FS Access API',
    gradient: 'from-amber-600 to-yellow-500',
  },
  {
    id: 'auto-clicker',
    nameKey: 'autoClicker',
    descKey: 'autoClicker',
    icon: MousePointerClick,
    category: 'system',
    badge: 'DOM Macro',
    gradient: 'from-green-500 to-emerald-600',
  },
  {
    id: 'window-pinner',
    nameKey: 'windowPinner',
    descKey: 'windowPinner',
    icon: PictureInPicture2,
    category: 'system',
    badge: 'Picture-in-Pic',
    gradient: 'from-blue-600 to-indigo-600',
  },
];
