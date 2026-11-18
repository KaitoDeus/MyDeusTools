# MyDeusTools Web (TerminalUI Edition)

> **Privacy-First TerminalUI Web & PWA Utility Suite**  
> Built with **React 19**, **Vite 8**, **TypeScript**, **Tailwind CSS v4**, and **Dexie (IndexedDB)** &mdash; optionally paired with **MyDeusTools.Agent** (.NET 8) for Windows OS automation.

---

## Overview

**MyDeusTools Web** delivers 12 utility tools in a sharp, zero-radius monospaced **TerminalUI** workspace (`JetBrains Mono`, `#0D0D0D` Dark / `#F4F4EE` Light themes, optional CRT scanline overlay, and instant Vietnamese/English switching).

- **100% Client-Side Privacy**: Notes, snippets, color palettes, JSON/Base64/SHA transforms, QR generation, image compression, and PDF merging/splitting run strictly in browser memory and IndexedDB. Zero server uploads.
- **Optional Local Companion Agent (`http://127.0.0.1:41950/`)**: When running on Windows alongside `MyDeusTools.Agent`, the web app unlocks OS-level capabilities that browsers sandbox by default: global mouse/hotkey auto-clicking (`F6`/`F7`), Windows Shutdown/Restart/Hibernate scheduling, and external window Always-on-Top/opacity controls.
- **Code-Split & Fast**: All 13 views (Dashboard + 12 tools) are lazily loaded via `React.lazy`.

---

## The 12 Web Utilities

| Cmd (`$ ./<cmd>`) | Tool | Capabilities |
|---|---|---|
| `00` | **Dashboard (`Ctrl+K`)** | Search utilities by name/command/description, filter by category (`Productivity`, `Developer`, `Media`, `System`), view privacy/system status. |
| `01` `sticky-notes` | **Sticky Notes** | Kanban note cards with semantic colors (`GREEN`, `AMBER`, `CYAN`, `RED`), search, pin-to-top, IndexedDB auto-save, and Document PiP pop-out. |
| `02` `clip-vault` | **Clipboard Snippets** | Reusable snippet vault with tags, search, pin-to-top, usage counter, and 1-click OS clipboard paste/copy (`navigator.clipboard`). |
| `03` `smart-timer` | **Smart Timer & Shutdown** | **Tab 1 (`01:POMODORO_TIMER`)**: Pure browser countdown with `70m -> 1h 10m` auto-normalization, Screen Wake Lock API, alarm beep, and optional webhook. **Tab 2 (`02:SCHEDULE_SHUTDOWN`)**: Strictly **enabled only when Local Companion Agent is online**; schedules Windows Shutdown, Restart, or Hibernate. |
| `04` `text-devtools` | **Text & Dev Tools** | 5 tabs: JSON Studio (format/minify/validate), Base64 (text & file upload), Web Crypto hashing (`SHA-1`, `SHA-256`, `SHA-512`), 7 case conversions + metrics, and URL Encode/Decode. |
| `05` `qr-studio` | **QR Code Studio** | Customizable foreground/background colors, quick theme presets (`HACKER`, `CLASSIC`, `CYAN`, `AMBER`), L/M/Q/H error levels, PNG download, clipboard copy, and image/clipboard reader. |
| `06` `color-picker` | **Color Eyedropper** | Native screen pixel sampler via `EyeDropper API`, manual color input, live HEX/RGB/HSL converters with 1-click copy, and IndexedDB palette history. |
| `07` `pdf-toolkit` | **PDF Toolkit** | Offline client-side PDF Merge with reordering and Split by custom page ranges (`1-3, 5`) powered by `pdf-lib`. |
| `08` `img-studio` | **Image Studio** | Batch image optimization and format conversion (`WebP`, `JPEG`, `PNG`) via HTML5 Canvas, quality tuning (`10-100%`), percentage scaling (`25-100%`), and automatic EXIF/GPS stripping. |
| `09` `video-wasm` | **Video Converter (WASM)** | Target container (`MP4`, `WEBM`, `MP3`, `GIF`), resolution, and CRF quality workflow UI in the client workspace. |
| `10` `bulk-rename` | **Bulk File Renamer** | Direct disk directory renaming via `File System Access API` (`showDirectoryPicker`) in Chromium browsers, or uploaded batch renaming exported as `.zip` (`JSZip`), with Regex, prefix, case modes, and live dry-run preview. |
| `11` `auto-click` | **Auto Clicker Pro** | **Tab 1 (`01:WEB_DOM_SANDBOX`)**: DOM click loop, CPS rate, target test sandbox, and portable Bookmarklet for any web page. **Tab 2 (`02:OS_NATIVE_AGENT`)**: Real Windows/Game auto-clicker via Agent with global `F6`/`F7` hotkeys and waypoint capture/replay. |
| `12` `pip-float` | **Window Pinner & PiP** | **Tab 1 (`01:WEB_DOCUMENT_PiP`)**: Always-on-top Digital Clock and Scratchpad floating widgets (`Document Picture-in-Picture API`). **Tab 2 (`02:OS_WINDOWS_MANAGER`)**: Enumerate OS windows, pin `HWND_TOPMOST`, adjust opacity (`40-100%`), and bring windows to front via Agent. |

---

## Quick Start

### Prerequisites
- Node.js `v20+` and `npm`
- Optional (for Windows OS features): [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Development Server
```bash
# From repository root:
npm --prefix MyDeusTools.Web install   # first time only
npm run web:dev                        # serves on http://localhost:3000

# Or inside MyDeusTools.Web/:
npm install
npm run dev
```

### Start Optional Windows Local Companion Agent
From either the repository root or `MyDeusTools.Web/`:
```bash
npm run agent:dev
```
This launches `MyDeusTools.Agent` on `http://127.0.0.1:41950/`. The Web top bar switches from `[AGENT:OFFLINE]` to `[AGENT:ONLINE]` within 5 seconds (or immediately on clicking the badge).

### Production Build & Preview
```bash
# Type-check (tsc) and build optimized bundle into dist/
npm run build

# Preview production bundle locally (default port 4173)
npm run preview
```

For full architecture, REST endpoints, and developer guides, see [`../docs/README.md`](../docs/README.md).
