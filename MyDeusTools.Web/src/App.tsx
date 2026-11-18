import React, { useState, useEffect, Suspense, lazy } from 'react';
import { useI18n } from './i18n/LanguageContext';
import { useTheme } from './contexts/ThemeContext';
import { TOOLS_CONFIG } from './data/toolsData';

// Lazy-loaded Views for ultra-fast initial bundle & Code Splitting
const DashboardView = lazy(() =>
  import('./features/dashboard/DashboardView').then((m) => ({ default: m.DashboardView }))
);
const TextDevToolsView = lazy(() =>
  import('./features/text-devtools/TextDevToolsView').then((m) => ({ default: m.TextDevToolsView }))
);
const QrStudioView = lazy(() =>
  import('./features/qr-studio/QrStudioView').then((m) => ({ default: m.QrStudioView }))
);
const ColorPickerView = lazy(() =>
  import('./features/color-picker/ColorPickerView').then((m) => ({ default: m.ColorPickerView }))
);
const PdfToolkitView = lazy(() =>
  import('./features/pdf-toolkit/PdfToolkitView').then((m) => ({ default: m.PdfToolkitView }))
);
const ImageStudioView = lazy(() =>
  import('./features/image-studio/ImageStudioView').then((m) => ({ default: m.ImageStudioView }))
);
const VideoConverterView = lazy(() =>
  import('./features/video-converter/VideoConverterView').then((m) => ({
    default: m.VideoConverterView,
  }))
);
const StickyNotesView = lazy(() =>
  import('./features/sticky-notes/StickyNotesView').then((m) => ({ default: m.StickyNotesView }))
);
const ClipboardHubView = lazy(() =>
  import('./features/clipboard-hub/ClipboardHubView').then((m) => ({ default: m.ClipboardHubView }))
);
const BulkRenamerView = lazy(() =>
  import('./features/bulk-renamer/BulkRenamerView').then((m) => ({ default: m.BulkRenamerView }))
);
const SmartTimerView = lazy(() =>
  import('./features/smart-timer/SmartTimerView').then((m) => ({ default: m.SmartTimerView }))
);
const AutoClickerView = lazy(() =>
  import('./features/auto-clicker/AutoClickerView').then((m) => ({ default: m.AutoClickerView }))
);
const WindowPipView = lazy(() =>
  import('./features/window-pip/WindowPipView').then((m) => ({ default: m.WindowPipView }))
);

import {
  LayoutDashboard,
  Menu,
  X,
  Languages,
  Moon,
  Sun,
  Globe,
  ChevronRight,
  ShieldCheck,
  Loader2,
} from 'lucide-react';

const ViewLoadingFallback = () => (
  <div className="flex flex-col items-center justify-center min-h-[50vh] space-y-3">
    <Loader2 className="w-8 h-8 text-indigo-500 animate-spin" />
    <span className="text-xs text-slate-500 dark:text-slate-400 font-medium">
      Loading Utility...
    </span>
  </div>
);

export const App: React.FC = () => {
  const { t, language, toggleLanguage } = useI18n();
  const { theme, toggleTheme } = useTheme();
  const [activeToolId, setActiveToolId] = useState<string>('dashboard');
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Keyboard shortcut Ctrl+K to jump to dashboard search
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        setActiveToolId('dashboard');
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  const renderActiveView = () => {
    switch (activeToolId) {
      case 'dashboard':
        return <DashboardView onSelectTool={(id) => setActiveToolId(id)} />;
      case 'text-devtools':
        return <TextDevToolsView />;
      case 'qr-studio':
        return <QrStudioView />;
      case 'color-picker':
        return <ColorPickerView />;
      case 'pdf-toolkit':
        return <PdfToolkitView />;
      case 'image-studio':
        return <ImageStudioView />;
      case 'video-converter':
        return <VideoConverterView />;
      case 'sticky-notes':
        return <StickyNotesView />;
      case 'clipboard-manager':
        return <ClipboardHubView />;
      case 'bulk-renamer':
        return <BulkRenamerView />;
      case 'smart-timer':
        return <SmartTimerView />;
      case 'auto-clicker':
        return <AutoClickerView />;
      case 'window-pinner':
        return <WindowPipView />;
      default:
        return <DashboardView onSelectTool={(id) => setActiveToolId(id)} />;
    }
  };

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100 transition-colors duration-200">
      {/* Sidebar Overlay for Mobile */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-slate-950/40 dark:bg-black/60 backdrop-blur-sm lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar Navigation */}
      <aside
        className={`fixed lg:static inset-y-0 left-0 z-50 w-72 flex flex-col justify-between glass-panel border-r border-slate-200/80 dark:border-slate-800/80 transition-transform duration-300 ease-in-out ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        }`}
      >
        <div className="flex flex-col h-full overflow-hidden">
          {/* Brand Header */}
          <div className="p-5 flex items-center justify-between border-b border-slate-200/80 dark:border-slate-800/80">
            <div
              className="flex items-center gap-3 cursor-pointer group"
              onClick={() => setActiveToolId('dashboard')}
            >
              <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-500 p-0.5 shadow-lg shadow-indigo-600/30 flex items-center justify-center">
                <img
                  src="/avatar.ico"
                  alt="MyDeusTools"
                  className="w-full h-full object-cover rounded-lg"
                  onError={(e) => {
                    (e.target as HTMLElement).style.display = 'none';
                  }}
                />
              </div>
              <div>
                <div className="font-extrabold text-sm tracking-tight text-slate-900 dark:text-white group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors flex items-center gap-1.5">
                  <span>MyDeusTools</span>
                  <span className="text-[10px] px-1.5 py-0.2 rounded bg-indigo-500/15 dark:bg-indigo-500/20 text-indigo-600 dark:text-indigo-400 font-mono font-bold">
                    WEB
                  </span>
                </div>
                <div className="text-[11px] text-slate-500 dark:text-slate-400 font-medium">
                  12 Native Web Tools
                </div>
              </div>
            </div>

            <button
              onClick={() => setSidebarOpen(false)}
              className="p-1 text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white lg:hidden cursor-pointer"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Navigation Items */}
          <div className="flex-1 overflow-y-auto p-3 space-y-1 scrollbar-thin">
            {/* Dashboard Home */}
            <button
              onClick={() => {
                setActiveToolId('dashboard');
                setSidebarOpen(false);
              }}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
                activeToolId === 'dashboard'
                  ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/25'
                  : 'text-slate-600 hover:text-slate-900 hover:bg-slate-200/60 dark:text-slate-400 dark:hover:text-slate-200 dark:hover:bg-slate-900/80'
              }`}
            >
              <LayoutDashboard className="w-4 h-4 shrink-0" />
              <span className="flex-1 text-left">{t.nav.dashboard}</span>
            </button>

            <div className="pt-3 pb-1 px-3 text-[10px] uppercase font-bold tracking-wider text-slate-400 dark:text-slate-500">
              {t.common.allTools} (12)
            </div>

            {TOOLS_CONFIG.map((tool) => {
              const Icon = tool.icon;
              const isActive = activeToolId === tool.id;
              return (
                <button
                  key={tool.id}
                  onClick={() => {
                    setActiveToolId(tool.id);
                    setSidebarOpen(false);
                  }}
                  className={`w-full flex items-center gap-3 px-3 py-2 rounded-xl text-xs font-medium transition-all cursor-pointer group ${
                    isActive
                      ? 'bg-indigo-50 text-indigo-600 font-semibold border border-indigo-200 dark:bg-slate-800 dark:text-indigo-400 dark:border-indigo-500/30'
                      : 'text-slate-600 hover:text-slate-900 hover:bg-slate-200/50 dark:text-slate-400 dark:hover:text-slate-200 dark:hover:bg-slate-900/60'
                  }`}
                >
                  <Icon
                    className={`w-4 h-4 shrink-0 transition-colors ${
                      isActive
                        ? 'text-indigo-600 dark:text-indigo-400'
                        : 'text-slate-400 group-hover:text-slate-700 dark:text-slate-500 dark:group-hover:text-slate-300'
                    }`}
                  />
                  <span className="flex-1 text-left truncate">{t.nav[tool.nameKey]}</span>
                  {tool.badge && (
                    <span className="text-[9px] px-1.5 py-0.2 rounded bg-slate-100 text-slate-500 border border-slate-200 dark:bg-slate-800 dark:text-slate-400 dark:border-slate-700/60">
                      {tool.badge}
                    </span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Sidebar Footer Info */}
          <div className="p-4 border-t border-slate-200/80 dark:border-slate-800/80 space-y-2">
            <div className="flex items-center gap-2 text-[11px] text-slate-600 dark:text-slate-400">
              <ShieldCheck className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400 shrink-0" />
              <span className="truncate">Client-Side Privacy 100%</span>
            </div>
            <div className="flex items-center justify-between text-[11px] text-slate-500">
              <span>v1.0.0 Pure Web</span>
              <a
                href="https://github.com/KaitoDeus/MyDeusTools"
                target="_blank"
                rel="noreferrer"
                className="hover:text-slate-900 dark:hover:text-slate-300 flex items-center gap-1"
              >
                <Globe className="w-3.5 h-3.5" />
                <span>GitHub</span>
              </a>
            </div>
          </div>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col h-full overflow-hidden">
        {/* Top Navbar */}
        <header className="h-16 border-b border-slate-200/80 dark:border-slate-800/80 glass-panel px-6 flex items-center justify-between shrink-0">
          <div className="flex items-center gap-3">
            <button
              onClick={() => setSidebarOpen(true)}
              className="p-2 text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white rounded-lg lg:hidden cursor-pointer"
            >
              <Menu className="w-5 h-5" />
            </button>
            <div className="text-xs text-slate-500 dark:text-slate-400 hidden sm:flex items-center gap-1.5 font-medium">
              <span
                className="hover:text-slate-900 dark:hover:text-slate-200 cursor-pointer"
                onClick={() => setActiveToolId('dashboard')}
              >
                MyDeusTools
              </span>
              <ChevronRight className="w-3.5 h-3.5 text-slate-400 dark:text-slate-600" />
              <span className="text-slate-900 dark:text-slate-200 font-semibold capitalize">
                {activeToolId === 'dashboard'
                  ? t.nav.dashboard
                  : t.nav[TOOLS_CONFIG.find((t) => t.id === activeToolId)?.nameKey || 'dashboard']}
              </span>
            </div>
          </div>

          {/* Quick Actions (Lang, Theme, Shortcuts) */}
          <div className="flex items-center gap-3">
            {/* Language Switcher */}
            <button
              onClick={toggleLanguage}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-white border border-slate-200 hover:border-slate-300 text-slate-700 dark:bg-slate-900 dark:border-slate-800 dark:hover:border-slate-700 text-xs font-semibold dark:text-slate-300 transition-all cursor-pointer shadow-xs dark:shadow-none"
              title="Toggle Language (Việt / English)"
            >
              <Languages className="w-3.5 h-3.5 text-indigo-600 dark:text-indigo-400" />
              <span>{language.toUpperCase()}</span>
            </button>

            {/* Theme Toggle */}
            <button
              onClick={toggleTheme}
              className="flex items-center gap-1.5 px-2.5 py-1.5 rounded-xl bg-white border border-slate-200 hover:border-slate-300 text-slate-700 dark:bg-slate-900 dark:border-slate-800 dark:hover:border-slate-700 dark:text-slate-300 transition-all cursor-pointer shadow-xs dark:shadow-none"
              title={theme === 'dark' ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
              aria-label="Toggle color theme"
            >
              {theme === 'dark' ? (
                <>
                  <Sun className="w-4 h-4 text-amber-400" />
                  <span className="text-xs font-semibold hidden sm:inline">Light</span>
                </>
              ) : (
                <>
                  <Moon className="w-4 h-4 text-indigo-600" />
                  <span className="text-xs font-semibold hidden sm:inline">Dark</span>
                </>
              )}
            </button>
          </div>
        </header>

        {/* Scrollable View Container */}
        <main className="flex-1 overflow-y-auto p-6 md:p-8">
          <div className="max-w-7xl mx-auto">
            <Suspense fallback={<ViewLoadingFallback />}>
              {renderActiveView()}
            </Suspense>
          </div>
        </main>
      </div>
    </div>
  );
};
