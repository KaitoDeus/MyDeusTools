import React, { useState, useEffect, Suspense, lazy } from 'react';
import { useI18n } from './i18n/LanguageContext';
import { useTheme } from './contexts/ThemeContext';
import { TOOLS_CONFIG } from './data/toolsData';

// Lazy-loaded Views
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

const ViewLoadingFallback: React.FC = () => {
  const { t } = useI18n();
  return (
    <div className="flex flex-col items-center justify-center min-h-[50vh] space-y-2 text-[13px] text-[var(--tui-primary)]">
      <div>{t.common.loadingModule}</div>
      <div className="text-[11px] text-[var(--tui-text-muted)]">{t.common.loadingWorker}</div>
    </div>
  );
};

export const App: React.FC = () => {
  const { t, toggleLanguage } = useI18n();
  const { theme, toggleTheme } = useTheme();
  const [activeToolId, setActiveToolId] = useState<string>('dashboard');
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [crtEnabled, setCrtEnabled] = useState<boolean>(() => {
    const saved = localStorage.getItem('mydeustools_crt');
    return saved !== 'false';
  });

  const toggleCrt = () => {
    const next = !crtEnabled;
    setCrtEnabled(next);
    localStorage.setItem('mydeustools_crt', String(next));
  };

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

  const currentTool = TOOLS_CONFIG.find((tool) => tool.id === activeToolId);

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
    <div className="relative flex h-screen w-screen overflow-hidden bg-[var(--tui-bg)] text-[var(--tui-text)]">
      {/* Optional CRT Scanlines Overlay */}
      {crtEnabled && <div className="fixed inset-0 z-[100] tui-scanlines" aria-hidden="true" />}

      {/* Mobile Overlay Backdrop */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 lg:hidden"
          style={{ backgroundColor: 'var(--tui-overlay)' }}
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Left Terminal Navigation Panel */}
      <aside
        className={`fixed lg:static inset-y-0 left-0 z-50 w-72 flex flex-col justify-between bg-[var(--tui-surface)] border-r border-[var(--tui-border-subtle)] ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        }`}
      >
        <div className="flex flex-col h-full overflow-hidden">
          {/* ASCII Brand Box */}
          <div className="p-4 border-b border-[var(--tui-border-subtle)] flex items-start justify-between gap-2">
            <button
              type="button"
              onClick={() => {
                setActiveToolId('dashboard');
                setSidebarOpen(false);
              }}
              className="text-left group cursor-pointer focus:outline-none"
            >
              <div className="flex items-center gap-2">
                <div className="w-7 h-7 border border-[var(--tui-primary)] bg-[var(--tui-bg)] text-[var(--tui-primary)] flex items-center justify-center text-[12px] font-bold">
                  &gt;_
                </div>
                <div>
                  <div className="text-[14px] font-bold text-[var(--tui-primary)] tracking-[0.04em]">
                    MYDEUSTOOLS
                  </div>
                  <div className="text-[11px] text-[var(--tui-text-muted)]">{t.shell.subtitle}</div>
                </div>
              </div>
            </button>

            <button
              type="button"
              onClick={() => setSidebarOpen(false)}
              className="lg:hidden px-2 py-0.5 text-[11px] border border-[var(--tui-border-medium)] text-[var(--tui-text-muted)] hover:border-[var(--tui-primary)] hover:text-[var(--tui-primary)] cursor-pointer"
              aria-label={t.common.close}
            >
              [X]
            </button>
          </div>

          {/* Navigation Menu */}
          <nav className="flex-1 overflow-y-auto py-2">
            <button
              type="button"
              onClick={() => {
                setActiveToolId('dashboard');
                setSidebarOpen(false);
              }}
              className={`w-full h-[36px] px-4 flex items-center justify-between text-[13px] border-b border-dashed border-[var(--tui-border-subtle)] cursor-pointer ${
                activeToolId === 'dashboard'
                  ? 'bg-[var(--tui-primary)] text-[var(--tui-primary-fg)] font-bold'
                  : 'text-[var(--tui-text)] hover:bg-[var(--tui-surface-elevated)] hover:text-[var(--tui-primary)]'
              }`}
            >
              <span className="truncate">
                {activeToolId === 'dashboard' ? '> ' : '$ '}
                {t.nav.dashboard}
              </span>
              <span className="text-[11px] opacity-75">00</span>
            </button>

            <div className="px-4 pt-4 pb-1.5 text-[11px] font-medium uppercase tracking-[0.1em] text-[var(--tui-text-dim)]">
              // {t.common.allTools} (12)
            </div>

            {TOOLS_CONFIG.map((tool) => {
              const isActive = activeToolId === tool.id;
              return (
                <button
                  type="button"
                  key={tool.id}
                  onClick={() => {
                    setActiveToolId(tool.id);
                    setSidebarOpen(false);
                  }}
                  className={`w-full h-[34px] px-4 flex items-center justify-between gap-2 text-[13px] border-b border-dashed border-[var(--tui-border-subtle)] cursor-pointer ${
                    isActive
                      ? 'bg-[var(--tui-surface-elevated)] text-[var(--tui-primary)] font-bold border-l-2 border-l-[var(--tui-primary)]'
                      : 'text-[var(--tui-text-muted)] hover:bg-[var(--tui-surface-elevated)] hover:text-[var(--tui-text)]'
                  }`}
                >
                  <span className="flex items-center gap-1.5 truncate">
                    <span
                      className={isActive ? 'text-[var(--tui-primary)]' : 'text-[var(--tui-text-dim)]'}
                    >
                      {isActive ? '>' : ' '}
                    </span>
                    <span className="truncate">{t.nav[tool.nameKey]}</span>
                  </span>
                  <span
                    className={`text-[11px] shrink-0 ${
                      isActive ? 'text-[var(--tui-primary)]' : 'text-[var(--tui-text-dim)]'
                    }`}
                  >
                    {tool.index}
                  </span>
                </button>
              );
            })}
          </nav>

          {/* Terminal Footer Info */}
          <div className="p-4 border-t border-[var(--tui-border-subtle)] space-y-1.5 text-[11px]">
            <div className="flex items-center justify-between">
              <span className="text-[var(--tui-text-dim)]">{t.shell.privacyLabel}</span>
              <span className="text-[var(--tui-primary)] font-semibold">{t.shell.privacyValue}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-[var(--tui-text-dim)]">{t.shell.statusLabel}</span>
              <span className="text-[var(--tui-text-muted)]">{t.shell.statusValue}</span>
            </div>
            <div className="pt-1 border-t border-dashed border-[var(--tui-border-subtle)] flex items-center justify-between">
              <span className="text-[var(--tui-text-dim)]">{t.shell.sourceLabel}</span>
              <a
                href="https://github.com/KaitoDeus/MyDeusTools"
                target="_blank"
                rel="noreferrer"
                className="text-[var(--tui-tertiary)] hover:underline"
              >
                {t.shell.sourceRepo}
              </a>
            </div>
          </div>
        </div>
      </aside>

      {/* Main Terminal Viewport */}
      <div className="flex-1 flex flex-col h-full overflow-hidden">
        {/* Top Terminal Bar */}
        <header className="h-[48px] bg-[var(--tui-surface)] border-b border-[var(--tui-border-subtle)] px-4 md:px-6 flex items-center justify-between shrink-0 gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <button
              type="button"
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden px-2.5 py-1 text-[12px] border border-[var(--tui-primary)] text-[var(--tui-primary)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)] cursor-pointer shrink-0"
            >
              [{t.common.menu}]
            </button>

            <div className="text-[13px] truncate flex items-center">
              <span className="text-[var(--tui-primary)] font-bold">root@mydeustools</span>
              <span className="text-[var(--tui-text-muted)]">:</span>
              <button
                type="button"
                onClick={() => setActiveToolId('dashboard')}
                className="text-[var(--tui-tertiary)] hover:underline cursor-pointer"
              >
                ~
              </button>
              <span className="text-[var(--tui-text-muted)]">
                /{activeToolId === 'dashboard' ? 'dashboard' : `bin/${currentTool?.cmd || activeToolId}`}
              </span>
              <span className="text-[var(--tui-primary)] ml-1.5 mr-1 font-bold">$</span>
              <span className="tui-cursor" aria-hidden="true" />
            </div>
          </div>

          {/* Terminal Mode Switches */}
          <div className="flex items-center gap-2 shrink-0">
            <button
              type="button"
              onClick={toggleCrt}
              className={`hidden sm:inline-flex px-2.5 py-1 text-[11px] font-semibold border cursor-pointer ${
                crtEnabled
                  ? 'border-[var(--tui-primary)] text-[var(--tui-primary)] hover:bg-[var(--tui-primary)] hover:text-[var(--tui-primary-fg)]'
                  : 'border-[var(--tui-border-medium)] text-[var(--tui-text-muted)] hover:border-[var(--tui-text)]'
              }`}
              title={t.shell.crtTooltip}
            >
              [{crtEnabled ? t.shell.crtOn : t.shell.crtOff}]
            </button>

            <button
              type="button"
              onClick={toggleLanguage}
              className="px-2.5 py-1 text-[11px] font-semibold border border-[var(--tui-tertiary)] text-[var(--tui-tertiary)] hover:bg-[var(--tui-tertiary)] hover:text-[var(--tui-primary-fg)] cursor-pointer"
              title={t.shell.langTooltip}
            >
              [{t.shell.langLabel}]
            </button>

            <button
              type="button"
              onClick={toggleTheme}
              aria-label="Toggle color theme"
              className="px-2.5 py-1 text-[11px] font-semibold border border-[var(--tui-secondary)] text-[var(--tui-secondary)] hover:bg-[var(--tui-secondary)] hover:text-[var(--tui-primary-fg)] cursor-pointer"
              title={theme === 'dark' ? t.shell.themeToLight : t.shell.themeToDark}
            >
              [{theme === 'dark' ? t.shell.themeDark : t.shell.themeLight}]
            </button>
          </div>
        </header>

        {/* Scrollable Workspace */}
        <main className="flex-1 overflow-y-auto p-6 md:p-8">
          <div className="max-w-7xl mx-auto">
            <Suspense fallback={<ViewLoadingFallback />}>{renderActiveView()}</Suspense>
          </div>
        </main>
      </div>
    </div>
  );
};
