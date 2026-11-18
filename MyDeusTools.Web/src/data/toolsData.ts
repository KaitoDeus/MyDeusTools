import { Translations } from '../i18n/translations';

export type ToolCategory = 'productivity' | 'developer' | 'media' | 'system';

export interface ToolItem {
  id: string;
  index: string;
  cmd: string;
  asciiTag: string;
  nameKey: keyof Translations['nav'];
  descKey: keyof Translations['descriptions'];
  category: ToolCategory;
  badge?: string;
  accent: 'primary' | 'secondary' | 'tertiary';
}

export const TOOLS_CONFIG: ToolItem[] = [
  // Productivity
  {
    id: 'sticky-notes',
    index: '01',
    cmd: 'sticky-notes',
    asciiTag: '[NOTE]',
    nameKey: 'stickyNotes',
    descKey: 'stickyNotes',
    category: 'productivity',
    accent: 'secondary',
  },
  {
    id: 'clipboard-manager',
    index: '02',
    cmd: 'clip-vault',
    asciiTag: '[CLIP]',
    nameKey: 'clipboardManager',
    descKey: 'clipboardManager',
    category: 'productivity',
    accent: 'tertiary',
  },
  {
    id: 'smart-timer',
    index: '03',
    cmd: 'smart-timer',
    asciiTag: '[TIME]',
    nameKey: 'scheduleShutdown',
    descKey: 'scheduleShutdown',
    category: 'productivity',
    badge: 'FOCUS',
    accent: 'secondary',
  },

  // Developer
  {
    id: 'text-devtools',
    index: '04',
    cmd: 'text-devtools',
    asciiTag: '[DEV]',
    nameKey: 'textDevTools',
    descKey: 'textDevTools',
    category: 'developer',
    accent: 'primary',
  },
  {
    id: 'qr-studio',
    index: '05',
    cmd: 'qr-studio',
    asciiTag: '[QR]',
    nameKey: 'qrStudio',
    descKey: 'qrStudio',
    category: 'developer',
    accent: 'tertiary',
  },
  {
    id: 'color-picker',
    index: '06',
    cmd: 'color-picker',
    asciiTag: '[HEX]',
    nameKey: 'colorPicker',
    descKey: 'colorPicker',
    category: 'developer',
    badge: 'EYEDROP',
    accent: 'primary',
  },

  // Media
  {
    id: 'pdf-toolkit',
    index: '07',
    cmd: 'pdf-toolkit',
    asciiTag: '[PDF]',
    nameKey: 'pdfToolkit',
    descKey: 'pdfToolkit',
    category: 'media',
    badge: 'OFFLINE',
    accent: 'secondary',
  },
  {
    id: 'image-studio',
    index: '08',
    cmd: 'img-studio',
    asciiTag: '[IMG]',
    nameKey: 'imageStudio',
    descKey: 'imageStudio',
    category: 'media',
    accent: 'tertiary',
  },
  {
    id: 'video-converter',
    index: '09',
    cmd: 'video-wasm',
    asciiTag: '[VID]',
    nameKey: 'videoConverter',
    descKey: 'videoConverter',
    category: 'media',
    badge: 'WASM',
    accent: 'primary',
  },

  // System & Files
  {
    id: 'bulk-renamer',
    index: '10',
    cmd: 'bulk-rename',
    asciiTag: '[REN]',
    nameKey: 'bulkRenamer',
    descKey: 'bulkRenamer',
    category: 'system',
    badge: 'FS-API',
    accent: 'secondary',
  },
  {
    id: 'auto-clicker',
    index: '11',
    cmd: 'auto-click',
    asciiTag: '[CLK]',
    nameKey: 'autoClicker',
    descKey: 'autoClicker',
    category: 'system',
    badge: 'MACRO',
    accent: 'primary',
  },
  {
    id: 'window-pinner',
    index: '12',
    cmd: 'pip-float',
    asciiTag: '[PiP]',
    nameKey: 'windowPinner',
    descKey: 'windowPinner',
    category: 'system',
    badge: 'PiP-API',
    accent: 'tertiary',
  },
];
