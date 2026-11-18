# Operation & History Log - MyDeusTools

Chronological record of milestones, feature integrations, refactorings, and architectural evolutions of **MyDeusTools**.

---

## Chronological Milestones

### Milestone 1: Inception & Core Foundation (v0.1.0)
- Initialized solution with `.NET 8` (WPF) and Lepo `WPF-UI 3.0` Fluent Design.
- Established Dependency Injection via `Microsoft.Extensions.DependencyInjection`.
- Implemented **Auto Clicker Pro**, **Schedule Shutdown**, **Sticky Notes**, and **System Tray** (`Hardcodet.NotifyIcon.Wpf`).

### Milestone 2: Clipboard & Visual Utilities (v0.2.0 - v0.4.0)
- Integrated **Clipboard Manager** (`AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE`), **QR Code Studio** (`QRCoder` + `ZXing.Net` + snipping overlay), **Color Picker & Eyedropper** (11x11 pixel magnifier HUD), and **Text & Dev Tools**.

### Milestone 3: Bilingual Localization & Window Controls (v0.5.0 - v0.7.0)
- Implemented runtime **Bilingual Localization (EN & VIE)** via `{DynamicResource}` and `Data/settings.json`.
- Implemented **Window Pinner & Transparency** (`HWND_TOPMOST`, `WS_EX_LAYERED` opacity).

### Milestone 4: Product Landing Page & Website (v0.8.0)
- Built static product site in `MyDeusTools.Website/` with GitHub Pages deployment (`.github/workflows/deploy-pages.yml`) and MIT License.

### Milestone 5: Multimedia & Graphics Suite (v0.9.0 - v0.9.5)
- Implemented **Video Converter & Transcoder** (FFmpeg orchestration) and **Image Studio & Optimizer** (batch compression, multi-layer `.ico`, EXIF strip), surpassing 200 unit tests.

### Milestone 6: Productivity Suite Expansion (v1.0.0)
- Implemented **PDF Toolkit** (`PDFsharp 6.1.1`) and **Bulk File & Folder Renamer** (Regex, case modes, numbering, conflict detection, Undo).
- Added **Automatic Time Normalization** (`70m -> 1h 10m`) in Schedule Shutdown (`3178766`).

### Milestone 7: Documentation & Skills Standardization
- Created `docs/`, `.agents/skills/`, `.claude/memory/`, and `.claude/rules/` around the WPF app (`8705cc8`, 274 tests).

### Milestone 8: Pure Web App Transformation (`MyDeusTools.Web`)
- (`c5f34ee`, `9d74be3`) Built pure client-side Web/PWA architecture using **Vite 8, React 19, TypeScript, Tailwind CSS v4, Dexie (IndexedDB), `pdf-lib`, `qrcode`, `jszip`**, and modern Web APIs (`EyeDropper`, `File System Access`, `Document PiP`, `Screen Wake Lock`, `Web Crypto`, `Canvas`).
- Fixed full Dark/Light runtime switching via Tailwind CSS v4 `@custom-variant dark` and CSS variables.

### Milestone 9: TerminalUI Redesign & Full Bilingual Parity
- (`51146ae`, `461774c`) Redesigned all 13 Web views using crisp **TerminalUI** aesthetics (`#0D0D0D`/`#F4F4EE`, `JetBrains Mono`, `0px` corners, CRT scanline toggle, dashed `TuiCard` headers).
- Refactored `TuiCard` with `bodyClassName`, resolved full EN/VI translation gaps, and enforced key parity at compile time via `_viSymmetryCheck`.

### Milestone 10: Windows Local Companion Agent & 2-Tab Agent-Gated Shutdown
- (`e4f15ea`, `789581e`) Added **`MyDeusTools.Agent`** (`.NET 8` zero-dependency loopback HTTP bridge on `http://127.0.0.1:41950/`) enabling OS-wide Win32 Auto-Clicking + global **`F6`/`F7`** hotkeys, Windows Shutdown/Restart/Hibernate, and desktop Window Pinning/Opacity from `MyDeusTools.Web`, plus 8 `OsAutoClickEngineTests` (raising suite to **282/282 tests**).
- (`bc96ba9`, `56dad09`) Split `SmartTimerView` into independent **`01:POMODORO_TIMER`** (browser-only) and **`02:SCHEDULE_SHUTDOWN`** (strictly enabled only when `MyDeusTools.Agent` is online via `disabled={!isOnline}` with `[LOCKED]` status indicators).
- Synchronized all project documentation, AI Memory Bank, Rules, and Skills to match the Hybrid Web + Agent + Desktop ecosystem.
