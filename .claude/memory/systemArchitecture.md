# System Architecture - MyDeusTools

This document outlines the three subsystems across MyDeusTools: **Web TerminalUI (`MyDeusTools.Web`)**, **Windows Local Companion Agent (`MyDeusTools.Agent`)**, and **Native Desktop WPF (`MyDeusTools.App`)**.

---

## 1. Hybrid Web + Companion Agent Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                MYDEUSTOOLS.WEB (React 19 + Vite 8)          │
│  - Providers: ThemeProvider, LanguageProvider, AgentProvider│
│  - Shell: App.tsx (Sidebar, Top TTY Bar, Lazy Suspense)     │
│  - TerminalUI.tsx: TuiButton, TuiCard, TuiInput, TuiAlert   │
│  - IndexedDB: MyDeusToolsWebDB (notes, snippets, colors)    │
└──────────────────────────────▲──────────────────────────────┘
                               │ Loopback HTTP JSON (Cors + PNA)
                               │ http://127.0.0.1:41950/
┌──────────────────────────────▼──────────────────────────────┐
│            MYDEUSTOOLS.AGENT (.NET 8 Win32 Bridge)          │
│  - LocalHttpBridgeServer (12 GET/POST REST endpoints)       │
│  - GlobalHotkeyService (VK_F6 toggle, VK_F7 capture)        │
│  - OsAutoClickEngine (IMouseInterop -> mouse_event/Cursor)  │
│  - OsSystemPowerService (shutdown.exe /s /r /h /a)          │
│  - OsWindowPinnerService (EnumWindows/TOPMOST/WS_EX_LAYERED)│
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Web Frontend Subsystems (`MyDeusTools.Web/src/`)

- **Contexts**:
  - `ThemeContext.tsx`: Toggles `.dark` class on `<html>` and `colorScheme`; persisted in `localStorage['mydeustools_theme']`.
  - `LanguageContext.tsx`: Manages `'vi' | 'en'`; persisted in `localStorage['mydeustools_lang']`.
  - `AgentContext.tsx`: Polls `GET /api/health` every `5000ms` and exports `{ isOnline, health, checking, refreshAgentStatus }`.
- **Views (`features/*/`)**:
  - 13 `React.lazy` views loaded cleanly via `Suspense`.
  - Dual-mode OS-aware views: `SmartTimerView` (strictly separates `pomodoro` and Agent-gated `shutdown` tabs), `AutoClickerView` (`web` DOM sandbox vs `os` Agent tab), and `WindowPipView` (`web` Document PiP vs `os` Windows Manager tab).
- **Styling (`index.css`)**:
  - Semantic CSS variables (`--tui-bg`, `--tui-surface`, `--tui-surface-elevated`, `--tui-primary`, `--tui-secondary`, `--tui-tertiary`, `--tui-error`, `--tui-border-*`).
  - Global TerminalUI enforcement: `JetBrains Mono`, `0px` border radius, zero box shadows, `steps(1, end)` transitions, optional fixed `.tui-scanlines` CRT overlay (`localStorage['mydeustools_crt']`).

---

## 3. Local Companion Agent (`MyDeusTools.Agent/`)

Zero external NuGet packages (`net8.0-windows`, `win-x64`, `PublishSingleFile=true`, `SelfContained=true`):
- **HTTP Bridge (`LocalHttpBridgeServer.cs`)**: `HttpListener` on `http://127.0.0.1:41950/`, handling `OPTIONS` preflight, CORS (`*`), and `Access-Control-Allow-Private-Network: true`.
- **REST Endpoints**:
  - `GET /api/health`
  - `GET /api/autoclick/status`, `POST /api/autoclick/start`, `POST /api/autoclick/stop`, `POST /api/autoclick/capture`, `POST /api/autoclick/points/clear`
  - `POST /api/system/schedule`, `POST /api/system/cancel`
  - `GET /api/windows`, `POST /api/windows/pin`, `POST /api/windows/opacity`, `POST /api/windows/focus`

---

## 4. Native WPF MVVM Architecture (`MyDeusTools.App/`)

Retains standard 3-layer separation configured in `App.xaml.cs` via `Microsoft.Extensions.DependencyInjection`:
- **16 Singleton Services**: `IPageService`, `INavigationService`, `IAutoClickService`, `ISystemService`, `IStickyNoteService`, `IAutoStartService`, `IClipboardService`, `IQrCodeService`, `IColorPickerService`, `ITextUtilityService`, `ILanguageService`, `IWindowPinnerService`, `IVideoConverterService`, `IImageStudioService`, `IPdfToolkitService`, `IBulkRenamerService`.
- **13 Transient ViewModels**: `MainWindowViewModel` + 12 utility ViewModels.
- **13 Transient Views + 4 Overlay Windows**: `MainWindow`, 12 `Views/Pages/*Page.xaml`, and `RecordingOverlayWindow`, `StickyNoteWindow`, `QrSnippingOverlayWindow`, `ColorPickerOverlayWindow`.
- **File Persistence (`BaseDirectory\Data\`)**: `notes.json`, `clipboard.json`, `colors.json`, `settings.json`.
