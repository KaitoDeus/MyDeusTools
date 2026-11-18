# MyDeusTools - Architecture & Technical Design

This document details the **Hybrid Web + Local Companion Agent Architecture**, the **TerminalUI Design System**, and the **Native Desktop WPF MVVM Architecture** across MyDeusTools.

---

## 1. High-Level Hybrid Ecosystem

MyDeusTools decouples pure client-side utilities from privileged Windows OS automation using a strictly loopback HTTP bridge:

```
+-----------------------------------------------------------------------+
|                    MYDEUSTOOLS.WEB (React 19 + Vite)                  |
|  - Lazy-loaded Dashboard + 12 Tools styled with TerminalUI (0px CSS)  |
|  - Dexie IndexedDB (MyDeusToolsWebDB: notes, snippets, colors)        |
|  - Client Web APIs: pdf-lib, qrcode, jszip, Canvas, EyeDropper, PiP   |
|  - AgentContext (5s /api/health poll) + agentClient.ts                |
+----------------------------------+------------------------------------+
                                   | HTTP JSON over Loopback ONLY
                                   | http://127.0.0.1:41950/ (+ CORS & PNA)
+----------------------------------v------------------------------------+
|              MYDEUSTOOLS.AGENT (.NET 8 Console Bridge)                |
|  - LocalHttpBridgeServer (System.Net.HttpListener)                    |
|  - GlobalHotkeyService (40ms GetAsyncKeyState: F6 toggle, F7 capture) |
|  - OsAutoClickEngine (IMouseInterop -> Win32 mouse_event/SetCursorPos)|
|  - OsSystemPowerService (shutdown /s /r /a & timeout && shutdown /h)  |
|  - OsWindowPinnerService (EnumWindows, SetWindowPos, WS_EX_LAYERED)   |
+-----------------------------------------------------------------------+

+-----------------------------------------------------------------------+
|               MYDEUSTOOLS.APP (Original Native WPF App)               |
|  - FluentWindow / Mica / WPF-UI 3.0.5 / CommunityToolkit.Mvvm         |
|  - 15 Singleton Domain Services + 13 Transient ViewModels             |
|  - 12 Navigation Pages + 4 Multi-Monitor Overlays                     |
|  - Local JSON Persistence in BaseDirectory\Data\                      |
+-----------------------------------------------------------------------+
```

---

## 2. Web Architecture (`MyDeusTools.Web`)

### A. Shell & Providers
- **Entry & Providers (`src/main.tsx`, `src/App.tsx`)**:
  - `ThemeProvider` (`src/contexts/ThemeContext.tsx`): Persists `'dark' | 'light'` in `localStorage['mydeustools_theme']` (defaults to `'dark'`), toggles `.dark` class and `colorScheme` on `<html>`.
  - `LanguageProvider` (`src/i18n/LanguageContext.tsx`): Persists `'vi' | 'en'` in `localStorage['mydeustools_lang']` (falls back to `navigator.language`). Compile-time type check `_viSymmetryCheck` in `src/i18n/translations.ts` guarantees 100% key symmetry between `en` and `vi`.
  - `AgentProvider` (`src/contexts/AgentContext.tsx`): Polls `/api/health` every `5000ms` via `agentClient` and broadcasts `{ isOnline, health, checking, refreshAgentStatus }` across all views.
- **Lazy Code Splitting (`src/App.tsx`)**: All 13 feature components (`DashboardView` + 12 utilities) are dynamically imported via `React.lazy` and rendered inside `<Suspense>`.
- **Tool Registry (`src/data/toolsData.ts`)**: Defines `TOOLS_CONFIG` mapping each tool ID (`01`..`12`), CLI slug (`cmd`), ASCII tag, category (`productivity | developer | media | system`), badge, and accent color.

### B. TerminalUI Design System (`src/index.css` & `src/components/terminal/TerminalUI.tsx`)
Enforces an authentic monospaced terminal aesthetic:
- **Zero-Radius & Zero-Shadow Geometry**: Global CSS rule sets `border-radius: 0px`, `box-shadow: none`, `transition-timing-function: steps(1, end)`, and `JetBrains Mono` monospace typography.
- **Dual Theme Semantic Tokens**:
  - `.dark`: `--tui-bg: #0d0d0d`, `--tui-surface: #141414`, `--tui-surface-elevated: #1a1a1a`, `--tui-primary: #00ff41` (phosphor green), `--tui-secondary: #ff6600` (amber), `--tui-tertiary: #00bfff` (cyan), `--tui-error: #ff0040`.
  - `:root` (Light / TTY Paper): `--tui-bg: #f4f4ee`, `--tui-surface: #ffffff`, `--tui-surface-elevated: #eaeae2`, `--tui-primary: #007a1f`, `--tui-secondary: #cc4d00`, `--tui-tertiary: #0077aa`, `--tui-error: #cc0033`.
- **CRT Scanlines**: Optional fixed overlay (`repeating-linear-gradient`) toggled in header and stored in `localStorage['mydeustools_crt']`.
- **Component Primitives (`TerminalUI.tsx`)**: `TuiButton`, `TuiCard` (with separate `className` and `bodyClassName` layout props so dashed headers never misalign), `TuiInput`, `TuiTextarea`, `TuiSelect`, `TuiChip`, `TuiPageHeader`, `TuiAlert`, and `TuiCheckbox`.

### C. Agent Gate Patterns in Web Views
1. **Strict OS Gate (`SmartTimerView`)**: Separated into independent tabs `pomodoro` and `shutdown`. All interactive controls inside Tab 2 (`shutdown`) enforce `disabled={!isOnline}` (power mode buttons, `hh/mm/ss` inputs, preset buttons, Schedule Shutdown button, Reset, Cancel OS Shutdown) and surface a `[LOCKED]` status badge + `[WARN]` alert when offline.
2. **Progressive Dual-Tab Unlock (`AutoClickerView` & `WindowPipView`)**: Both tools operate standalone in browser Tab 1 (`WEB_DOM_SANDBOX` / `WEB_DOCUMENT_PiP`) when offline and automatically switch to / unlock Tab 2 (`OS_NATIVE_AGENT` / `OS_WINDOWS_MANAGER`) when `isOnline === true`.

### D. Browser Storage (`src/services/db.ts`)
Uses **Dexie** IndexedDB database `MyDeusToolsWebDB` (schema v1):
- `notes`: `++id, title, isPinned, createdAt, updatedAt`
- `snippets`: `++id, title, tag, isPinned, usageCount, createdAt`
- `colors`: `++id, hex, label, createdAt`

---

## 3. Local Companion Agent Architecture (`MyDeusTools.Agent`)

Zero external NuGet dependencies (`net8.0-windows`, `win-x64`):
- **`LocalHttpBridgeServer.cs`**: Binds `HttpListener` strictly to loopback prefix `http://127.0.0.1:41950/`. Injects `Access-Control-Allow-Origin: *`, `Access-Control-Allow-Methods: GET, POST, OPTIONS`, `Access-Control-Allow-Headers: Content-Type, Access-Control-Request-Private-Network`, and responds with `Access-Control-Allow-Private-Network: true` so Chrome/Edge Private Network Access checks allow requests from hosted or local web origins.
- **`OsAutoClickEngine.cs` + `IMouseInterop.cs`**: Thread-safe (`lock (_sync)`) PeriodicTimer engine enforcing a `MinIntervalMs = 10` safety clamp, Left/Right/Middle buttons, Single/Double clicks (`mouse_event`), finite repeat counters or infinite loops, and recorded cursor waypoint cycling (`SetCursorPos`). Abstracted behind `IMouseInterop` for deterministic unit testing.
- **`GlobalHotkeyService.cs`**: Edge-triggered 40ms poll loop using `GetAsyncKeyState` for `VK_F6` (0x75) and `VK_F7` (0x76), ensuring reliable hotkey detection across fullscreen desktop apps and games.
- **`OsSystemPowerService.cs`**: Dispatches hidden `shutdown.exe` (`/s /f /t`, `/r /f /t`, `/a`) and background `cmd.exe /c timeout /t <n> /nobreak && shutdown /h` processes with child-tree cleanup on cancel.
- **`OsWindowPinnerService.cs`**: Uses `EnumWindows`, `GetWindowText`, `GetWindowLongPtr`/`SetWindowLongPtr` (`WS_EX_TOOLWINDOW`, `WS_EX_TOPMOST`, `WS_EX_LAYERED`), `SetWindowPos`, `SetLayeredWindowAttributes` (alpha clamped `20-100%`), `ShowWindow(SW_RESTORE)`, and `SetForegroundWindow`.

---

## 4. Native WPF Desktop Architecture (`MyDeusTools.App`)

Retains clean 3-layer **MVVM** (`Views -> ViewModels -> Services`):
- **Presentation**: `MainWindow` (`ui:FluentWindow`, Mica backdrop, `NavigationView`, `Hardcodet.NotifyIcon.Wpf`), 12 Pages under `Views/Pages/`, and 4 multi-monitor overlays under `Views/Windows/` (`RecordingOverlayWindow`, `StickyNoteWindow`, `QrSnippingOverlayWindow`, `ColorPickerOverlayWindow`).
- **ViewModels**: 13 ViewModels inheriting `ObservableObject` using `[ObservableProperty]` and `[RelayCommand]` (`CommunityToolkit.Mvvm`).
- **DI Container (`App.xaml.cs`)**: Registers 16 Singleton services (`IPageService`, `INavigationService`, `IAutoClickService`, `ISystemService`, `IStickyNoteService`, `IAutoStartService`, `IClipboardService`, `IQrCodeService`, `IColorPickerService`, `ITextUtilityService`, `ILanguageService`, `IWindowPinnerService`, `IVideoConverterService`, `IImageStudioService`, `IPdfToolkitService`, `IBulkRenamerService`), 13 Transient ViewModels, and 13 Transient Window/Page views.
- **Win32 Interop**: `mouse_event`, `SetCursorPos`, `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE`, `EnumWindows`, `SetWindowPos`, `SetLayeredWindowAttributes`, GDI `GetDC`/`GetPixel`/`CopyFromScreen`, and HKCU Registry `Run` key for autostart.
- **Desktop Persistence (`BaseDirectory\Data\`)**: Stores `notes.json`, `clipboard.json`, `colors.json`, and `settings.json` with `SemaphoreSlim` / atomic write guards.
