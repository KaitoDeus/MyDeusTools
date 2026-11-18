# MyDeusTools - AI Developer & Architecture Guide

## Project Overview
**MyDeusTools** is a 12-utility productivity and developer suite evolving across a **Hybrid Web + Windows Companion Architecture** alongside the original **Native WPF Desktop Application**:

1. **`MyDeusTools.Web/`**: Pure client-side Web & PWA application built with **React 19**, **Vite 8**, **TypeScript 7**, **Tailwind CSS v4**, and **Dexie (IndexedDB)** using a strict **TerminalUI** design system (`#0D0D0D`/`#F4F4EE`, `JetBrains Mono`, `0px` corners, zero shadows/emojis/gradients, optional CRT scanlines).
2. **`MyDeusTools.Agent/`**: Optional zero-dependency **.NET 8** loopback HTTP bridge (`http://127.0.0.1:41950/`) exposing Win32 mouse synthesis + global `F6`/`F7` hotkeys (`OsAutoClickEngine`, `GlobalHotkeyService`), Windows power scheduling (`OsSystemPowerService`), and desktop window pinning/opacity (`OsWindowPinnerService`).
3. **`MyDeusTools.App/`**: Native Windows 10/11 desktop app built with **C# .NET 8 (WPF)**, **WPF-UI 3.0.5** (Fluent Design & Mica), **CommunityToolkit.Mvvm 8.4.2**, and `Microsoft.Extensions.DependencyInjection`.
4. **`MyDeusTools.Tests/`**: Automated **xUnit + Moq + Xunit.StaFact** test suite (**282/282 tests passing** covering both `MyDeusTools.App` and `MyDeusTools.Agent`).

---

## Core Utilities (12 Tools)

1. **Sticky Notes (`01` `sticky-notes`)**:
   - Web: Kanban note composer/board with semantic accents (`GREEN`, `AMBER`, `CYAN`, `RED`), search, pin-to-top, Dexie IndexedDB persistence, and `Document Picture-in-Picture` pop-out.
   - Desktop: Borderless draggable `StickyNoteWindow` + `Data/notes.json`.
2. **Clipboard Manager (`02` `clip-vault`)**:
   - Web: Reusable snippet vault with tags, usage counter, search, pin-to-top, OS clipboard paste/copy (`navigator.clipboard`), IndexedDB persistence.
   - Desktop: Real-time Win32 `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE` listener + `Data/clipboard.json`.
3. **Smart Timer & Shutdown (`03` `smart-timer`)**:
   - Web (**2 separate tabs**):
     - `01:POMODORO_TIMER`: Always available in browser (`00:25:00` default, Screen Wake Lock, alarm sound, webhook, `70m -> 1h 10m` auto-normalization).
     - `02:SCHEDULE_SHUTDOWN`: Strictly **enabled only when `MyDeusTools.Agent` is online** (`disabled={!isOnline}` on all controls); shows `[LOCKED]` chip and `[WARN]` banner when offline; supports Windows Shutdown (`/s`), Restart (`/r`), Hibernate (`/h`), Cancel (`/a`), and auto-normalization (`00:30:00` default).
   - Desktop: `ShutdownViewModel` + `SystemService` with live countdown & `70m -> 1h 10m` normalization.
4. **Text & Dev Tools (`04` `text-devtools`)**:
   - Web: 5 tabs &mdash; JSON Studio (format/minify/validate), Base64 (text & file upload), Web Crypto Hashes (`SHA-1`, `SHA-256`, `SHA-512`), 7 Case modes + metrics, URL Encode/Decode.
   - Desktop: Adds MD5, HTML Entities, binary Base64 decoding to file, and file hashing.
5. **QR Code Studio (`05` `qr-studio`)**:
   - Web: High-contrast QR generation via `qrcode` (presets, L/M/Q/H ECC, PNG export, clipboard copy) + file/clipboard image input reader.
   - Desktop: `QRCoder` generation + `ZXing.Net` decoding via file, clipboard, or fullscreen `QrSnippingOverlayWindow`.
6. **Color Picker & Eyedropper (`06` `color-picker`)**:
   - Web: Native browser `EyeDropper` API, manual `<input type="color">`, HEX/RGB/HSL conversion & copy, IndexedDB palette history.
   - Desktop: Fullscreen `ColorPickerOverlayWindow` with GDI `11x11` pixel magnifier HUD, HSV support, `Data/colors.json`.
7. **PDF Toolkit (`07` `pdf-toolkit`)**:
   - Web: Client-side Merge (with reorder) and Split (`1-3, 5` range) via `pdf-lib`.
   - Desktop: `PDFsharp` 6.1.1 Merge, Split (range/single/N-page chunks), Images-to-PDF, Password protection & metadata editor.
8. **Image Studio (`08` `img-studio`)**:
   - Web: HTML5 Canvas batch re-encoding (`WebP`, `JPEG`, `PNG`), quality (`10-100%`), percentage scaling (`25-100%`), automatic EXIF/GPS stripping.
   - Desktop: Batch optimizer + multi-resolution `.ico` generator (`16px`&ndash;`256px`) & aspect-ratio bounding box resize.
9. **Video Converter (`09` `video-wasm`)**:
   - Web: Container (`MP4`, `WEBM`, `MP3`, `GIF`), resolution, and CRF quality preset workflow UI.
   - Desktop: FFmpeg CLI wrapper with audio extraction, GPU hwaccel options, trimming, and progress parsing.
10. **Bulk File & Folder Renamer (`10` `bulk-rename`)**:
    - Web: Direct disk directory renaming via Chromium `File System Access API` (`showDirectoryPicker`) + uploaded batch ZIP export (`jszip`), Regex find/replace, prefix, case modes, live dry-run preview.
    - Desktop: Regex capture groups, insert/remove/trim, 8 case modes, zero-padded numbering, date tags, conflict detection, 1-click Undo.
11. **Auto Clicker Pro (`11` `auto-click`)**:
    - Web (**2 tabs**): `01:WEB_DOM_SANDBOX` (browser DOM interval loop >= 10ms, CPS readout, test target, draggable bookmarklet) + `02:OS_NATIVE_AGENT` (unrestricted Win32 mouse simulation via `MyDeusTools.Agent`, Left/Right/Middle, Single/Double, global `F6`/`F7` hotkeys, waypoint capture/replay). Defaults to `os` tab when Agent is online.
    - Desktop: `AutoClickService` + `NHotkey.Wpf` + `RecordingOverlayWindow`.
12. **Window Pinner & PiP (`12` `pip-float`)**:
    - Web (**2 tabs**): `01:WEB_DOCUMENT_PiP` (Digital Clock & Scratchpad via Document Picture-in-Picture API) + `02:OS_WINDOWS_MANAGER` (enumerate desktop windows, pin `HWND_TOPMOST`, opacity `40-100%`, focus window via Agent). Defaults to `os` tab when Agent is online.
    - Desktop: `WindowPinnerService` (`EnumWindows`, `SetWindowPos`, `WS_EX_LAYERED`, 20%&ndash;100% slider).

---

## Tech Stack & Dependencies

### Web Application (`MyDeusTools.Web/package.json`)
- **Core**: `react` `^19.3.1`, `react-dom` `^19.3.1`, `typescript` `~7.0.2`, `vite` `^8.3.6`, `@vitejs/plugin-react` `^6.3.0`
- **Styling**: `tailwindcss` `^4.3.5`, `@tailwindcss/vite` `^4.3.5`, `clsx` `^2.1.1`, `tailwind-merge` `^3.7.0`
- **Storage & Tooling**: `dexie` `^4.4.7`, `pdf-lib` `^1.17.1`, `qrcode` `^1.5.4`, `jszip` `^3.10.2`

### .NET Projects (`MyDeusTools.sln`)
- **Local Companion Agent (`MyDeusTools.Agent`)**: `net8.0-windows`, `win-x64`, `PublishSingleFile=true`, `SelfContained=true`, zero external NuGet packages.
- **Desktop App (`MyDeusTools.App`)**: `net8.0-windows`, `WPF-UI 3.0.5`, `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.DependencyInjection 10.0.7`, `NHotkey.Wpf 4.0.0`, `Hardcodet.NotifyIcon.Wpf 2.0.1`, `QRCoder 1.8.0`, `ZXing.Net 0.16.11`, `PDFsharp 6.1.1`.
- **Tests (`MyDeusTools.Tests`)**: `xunit 2.9.2`, `Moq 4.20.72`, `Xunit.StaFact 0.3.18`, `coverlet.collector 6.0.2` &mdash; **282/282 passing (100%)**.

---

## Repository Structure

```
d:\.MyDeusTools\MyDeusTools\
├── .agents/skills/               # Antigravity AI Skills (mirrored in .claude/skills/)
├── .claude/                      # Claude Memory Bank, Rules & Skills
│   ├── MEMORY.md                 # Memory Bank Master Index
│   ├── memory/                   # projectBrief, systemArchitecture, technicalPatterns, progress, activeContext, history
│   ├── rules/                    # web-terminalui.md, wpf-standards.md, windows-api.md, localization-standards.md
│   └── skills/                   # Mirrored AI Skills
├── docs/                         # Official Technical Documentation
│   ├── README.md                 # Master Documentation Index
│   ├── FEATURES.md               # 12 Utilities Manual (Web, Agent & WPF)
│   ├── ARCHITECTURE.md           # Hybrid Web + Agent + WPF MVVM Architecture
│   ├── DEVELOPER_GUIDE.md        # Setup, Build, 282-Test Suite & Publishing
│   └── API_REFERENCE.md          # Agent REST API, Web Storage & C# Service Interfaces
├── MyDeusTools.Web/              # Pure Web TerminalUI Application (Vite + React 19 + TS)
│   ├── src/components/terminal/  # TerminalUI.tsx primitives (TuiButton, TuiCard, TuiInput, etc.)
│   ├── src/contexts/             # ThemeContext.tsx (Dark/Light), AgentContext.tsx (5s polling)
│   ├── src/data/toolsData.ts     # Registry & metadata for the 12 tools
│   ├── src/features/             # 13 lazy-loaded views (dashboard + 12 tools)
│   ├── src/i18n/                 # LanguageContext.tsx & translations.ts (_viSymmetryCheck)
│   └── src/services/             # agentClient.ts (127.0.0.1:41950) & db.ts (Dexie IndexedDB)
├── MyDeusTools.Agent/            # Windows Local Companion Agent (.NET 8 HttpListener + Win32)
│   ├── Program.cs                # Startup wiring, hotkey loop, HTTP server
│   └── Services/                 # LocalHttpBridgeServer, OsAutoClickEngine, GlobalHotkeyService, IMouseInterop, OsSystemPowerService, OsWindowPinnerService
├── MyDeusTools.App/              # Original Native WPF Desktop App (.NET 8 + WPF-UI 3.0)
├── MyDeusTools.Tests/            # 282 Automated Unit & Integration Tests (WPF + Agent)
├── MyDeusTools.Website/          # Static Product Website (GitHub Pages)
├── package.json                  # Root monorepo-style scripts (web:dev/build/preview, agent:dev/publish)
├── MyDeusTools.sln               # .NET Solution (App, Agent, Tests, Website)
├── CLAUDE.md                     # AI Assistant & Developer Guide (this file)
└── README.md                     # Repository Front-Page Readme
```

---

## Development Protocols

1. **Read-First**: Consult `.claude/memory/activeContext.md`, `progress.md`, and `systemArchitecture.md` before modifying service boundaries or Web/Agent contracts.
2. **TerminalUI Strictness**: In `MyDeusTools.Web`, adhere to `.claude/rules/web-terminalui.md` (`0px` corners, monospace font, `--tui-*` tokens, `TuiCard` `bodyClassName` separation, `disabled={!isOnline}` OS gates).
3. **100% Bilingual Symmetry**:
   - Web: Every key in `MyDeusTools.Web/src/i18n/translations.ts` must exist in both `en` and `vi` (enforced at compile time by `_viSymmetryCheck`).
   - WPF: Keep `Strings.vi-VN.xaml` and `Strings.en-US.xaml` symmetric (enforced by `LanguageServiceTests`).
4. **Verification Before Commit**:
   - Run `npm run web:build` (`tsc && vite build`) &mdash; must succeed cleanly.
   - Run `dotnet test` &mdash; must maintain **282/282 passed** and 0 errors.
   - Use Conventional Commits with `Co-authored-by: Claude <81847+claude@users.noreply.github.com>`.
