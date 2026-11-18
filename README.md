# MyDeusTools

<p align="center">
  <img src="MyDeusTools.App/Resources/avatar.ico" width="80" height="80" alt="MyDeusTools Icon" />
</p>

<p align="center">
  <strong>All-in-One Utility Suite &mdash; TerminalUI Web App, Windows Local Companion Agent &amp; Native WPF Desktop App</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Web-React%2019%20%7C%20Vite%208%20%7C%20TS-00FF41?labelColor=0D0D0D" alt="Web Stack" />
  <img src="https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Agent-Loopback%20127.0.0.1%3A41950-FF6600?labelColor=0D0D0D" alt="Local Agent" />
  <img src="https://img.shields.io/badge/Tests-282%20Passed%20(100%25)-brightgreen" alt="Tests" />
  <img src="https://img.shields.io/badge/Platform-Web%20%2B%20Windows%20x64-00BFFF?labelColor=0D0D0D" alt="Platform" />
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License" />
</p>

<p align="center">
  <a href="MyDeusTools.Web/README.md"><strong>Web App Quick Start &rarr;</strong></a>
  &nbsp;|&nbsp;
  <a href="docs/README.md"><strong>Technical Documentation &rarr;</strong></a>
  &nbsp;|&nbsp;
  <a href="https://kaitodeus.github.io/MyDeusTools/"><strong>Project Website &rarr;</strong></a>
</p>

---

## Overview

**MyDeusTools** consolidates **12 essential productivity, developer, media, and system utilities** into a unified ecosystem built around a privacy-first **Hybrid Web + Local Agent Architecture**:

1. **`MyDeusTools.Web` (TerminalUI Pure Web & PWA)**: Client-side utility suite built with **React 19**, **Vite 8**, **TypeScript**, and **Tailwind CSS v4** styled in a strict high-contrast **TerminalUI** design system (`JetBrains Mono`, `0px` crisp borders, Light/Dark themes, optional CRT scanline toggle, and IndexedDB persistence via Dexie). Zero data ever leaves your machine.
2. **`MyDeusTools.Agent` (Optional Windows Local Companion Agent)**: Lightweight, zero-dependency **.NET 8** loopback HTTP bridge (`http://127.0.0.1:41950/`) that safely unlocks native Windows APIs from inside the browser &mdash; OS-wide mouse auto-clicking + global `F6`/`F7` hotkeys, Windows Shutdown/Restart/Hibernate scheduling, and desktop window Always-on-Top pinning/opacity.
3. **`MyDeusTools.App` (Native WPF Desktop Super-App)**: The original Windows 10/11 desktop application built with **C# .NET 8**, **WPF-UI 3.0** (Fluent Design & Mica backdrop), and **CommunityToolkit.Mvvm**, packable as a self-contained single-file executable.

---

## Key Utilities (12 Tools)

| # | Utility | Web (`MyDeusTools.Web`) | Windows OS Capabilities (`Agent` / `MyDeusTools.App`) |
|---|---|---|---|
| `01` | **Sticky Notes** | Kanban note board, semantic color tags, pin-to-top, IndexedDB auto-save, Document PiP pop-out. | Borderless floating desktop windows (`StickyNoteWindow`) & `Data/notes.json`. |
| `02` | **Clipboard Manager** | Snippet vault with tags, search, pin-to-top, usage counter, OS paste (`Clipboard API`), IndexedDB. | Real-time Win32 `AddClipboardFormatListener` hook (`WM_CLIPBOARDUPDATE`). |
| `03` | **Smart Timer & Shutdown** | **2 separate tabs**: `01:POMODORO_TIMER` (always available in browser, Screen Wake Lock, webhook, `70m -> 1h 10m` auto-normalization) & `02:SCHEDULE_SHUTDOWN` (strictly **enabled only when Agent is online**). | `MyDeusTools.Agent` (`POST /api/system/schedule` & `/cancel`) + `MyDeusTools.App` trigger Windows Shutdown (`/s`), Restart (`/r`), Hibernate (`/h`), and Abort (`/a`). |
| `04` | **Text & Dev Tools** | JSON Studio (format/minify/validate), Base64 (text & files), Web Crypto SHA-1/256/512, 7 case modes, URL Encoder. | Adds MD5 hashing, HTML Entity encoding, and local file hashing (`ITextUtilityService`). |
| `05` | **QR Code Studio** | Customizable foreground/background colors, presets, L/M/Q/H ECC, PNG export & clipboard copy (`qrcode`), image metadata reader. | HD generation (`QRCoder`) + multi-source decoding (`ZXing.Net`), including fullscreen snipping (`QrSnippingOverlayWindow`). |
| `06` | **Color Picker & Eyedropper** | Native browser `EyeDropper` API pixel sampler, HEX/RGB/HSL conversion, instant copy, IndexedDB palette history. | Fullscreen GDI `BitBlt`/`GetPixel` 11x11 magnifier HUD overlay & HSL/HSV formats. |
| `07` | **PDF Toolkit** | 100% client-side Merge & page-range Split (`1-3, 5`) via `pdf-lib`. | Full offline `PDFsharp` 6.1.1 suite: Merge, Split (range/single/N-page chunks), Images-to-PDF, Password encryption & metadata. |
| `08` | **Image Studio** | Canvas batch compressor/converter (`WebP`, `JPEG`, `PNG`), quality slider, percentage scale, automatic EXIF/GPS strip. | Batch optimizer + multi-resolution Windows `.ico` builder (`16px`&ndash;`256px`) & aspect-fit resizing. |
| `09` | **Video Converter** | Client configuration workflow for MP4/WebM/MP3/GIF containers, resolutions, and CRF quality levels. | Real FFmpeg CLI wrapper with GPU acceleration hooks, trimming, stream copy, and live progress parsing. |
| `10` | **Bulk File Renamer** | Direct disk directory access via `File System Access API` (`showDirectoryPicker`) + uploaded batch ZIP export (`JSZip`), Regex & case modes. | Regex capture groups (`$1`, `$2`), sequential zero-padded numbering, date/timestamp tags, live conflict checks, and 1-click Undo. |
| `11` | **Auto Clicker Pro** | **2 tabs**: `01:WEB_DOM_SANDBOX` (browser DOM interval loop, CPS meter, target sandbox, portable Bookmarklet) & `02:OS_NATIVE_AGENT` (unrestricted Win32 mouse loop). | `MyDeusTools.Agent` (`mouse_event`, `SetCursorPos`, min 10ms clamp, global `F6` Start/Stop & `F7` cursor waypoint capture); `MyDeusTools.App` (`NHotkey.Wpf` + `RecordingOverlayWindow`). |
| `12` | **Window Pinner & PiP** | **2 tabs**: `01:WEB_DOCUMENT_PiP` (always-on-top Digital Clock & Scratchpad via Document Picture-in-Picture API) & `02:OS_WINDOWS_MANAGER`. | `MyDeusTools.Agent` (`GET/POST /api/windows/*`) & `MyDeusTools.App` enumerate desktop windows, pin `HWND_TOPMOST`, adjust opacity (`20%`&ndash;`100%` via `WS_EX_LAYERED`), and focus windows. |

Cross-cutting capabilities:
- **Instant Bilingual Switching (`vi` / `en`)** with compile-time TypeScript key symmetry (`translations.ts`) in Web and runtime `DynamicResource` switching (`Strings.*.xaml`) in WPF.
- **TerminalUI Dark/Light & CRT Modes** in Web; Windows 11 **Mica Dark/Light**, Minimize-to-Tray, and CurrentUser Startup Registry in WPF.

---

## Documentation

Comprehensive guides and API specifications live in [`docs/`](docs/):

- [**docs/README.md**](docs/README.md): Master documentation index and repository overview.
- [**docs/FEATURES.md**](docs/FEATURES.md): Detailed manual for all 12 utilities across Web, Local Agent, and Desktop WPF.
- [**docs/ARCHITECTURE.md**](docs/ARCHITECTURE.md): Hybrid Web + Agent architecture, TerminalUI tokens, WPF MVVM layers, Win32 interop, and persistence.
- [**docs/API_REFERENCE.md**](docs/API_REFERENCE.md): Agent REST API (`127.0.0.1:41950`), `agentClient` / Dexie APIs, and C# WPF Service contracts.
- [**docs/DEVELOPER_GUIDE.md**](docs/DEVELOPER_GUIDE.md): Prerequisites, dev/build commands, 282-test suite, publishing, and contribution rules.

---

## Technology Stack

- **Web App (`MyDeusTools.Web`)**: React 19, Vite 8, TypeScript 7, Tailwind CSS v4 (`@tailwindcss/vite`), Dexie (IndexedDB), `pdf-lib`, `qrcode`, `jszip`, Web APIs (`EyeDropper`, `File System Access`, `Screen Wake Lock`, `Document Picture-in-Picture`, `Web Crypto`, `Canvas`).
- **Local Companion Agent (`MyDeusTools.Agent`)**: C# 12, `.NET 8.0` (`net8.0-windows`, `win-x64`), zero external NuGet dependencies, `System.Net.HttpListener` (`127.0.0.1:41950`), Win32 `user32.dll` (`mouse_event`, `SetCursorPos`, `GetAsyncKeyState`, `EnumWindows`, `SetWindowPos`, `SetLayeredWindowAttributes`).
- **Desktop WPF App (`MyDeusTools.App`)**: C# 12, `.NET 8.0` (`net8.0-windows`), `WPF-UI 3.0.5`, `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.DependencyInjection 10.0.7`, `NHotkey.Wpf 4.0.0`, `Hardcodet.NotifyIcon.Wpf 2.0.1`, `QRCoder 1.8.0`, `ZXing.Net 0.16.11`, `PDFsharp 6.1.1`.
- **Testing (`MyDeusTools.Tests`)**: `xUnit 2.9.2`, `Moq 4.20.72`, `Xunit.StaFact 0.3.18` (**282/282 tests passing, 100% pass rate**).

---

## Getting Started

### Prerequisites
- **For Web App**: Node.js `v20+` and `npm`.
- **For Local Companion Agent & WPF Desktop App**: Windows 10/11 (x64) and [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### 1. Run Web App + Local Companion Agent (Recommended Hybrid Mode)
```bash
# Clone repository
git clone https://github.com/KaitoDeus/MyDeusTools.git
cd MyDeusTools

# Install Web dependencies (first time only)
npm --prefix MyDeusTools.Web install

# Terminal 1: Start Web App dev server (http://localhost:3000)
npm run web:dev

# Terminal 2: Start Windows Local Companion Agent (http://127.0.0.1:41950)
npm run agent:dev
```

### 2. Run Native WPF Desktop App
```bash
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

### 3. Build & Test Verification
```bash
# Type-check + build Web App production bundle (MyDeusTools.Web/dist)
npm run web:build

# Run full .NET test suite (WPF App + Local Agent: 282 tests)
dotnet test
```

### 4. Package Standalone Executables (Windows x64)
```bash
# Standalone Local Companion Agent -> publish/agent/MyDeusTools.Agent.exe
npm run agent:publish

# Standalone Native WPF App -> MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

---

## Contributors

Developed and maintained through Human-AI Pair Programming (see [CONTRIBUTORS.md](CONTRIBUTORS.md)):

- **[KaitoDeus](https://github.com/KaitoDeus)** &mdash; Project Creator, Lead Software Architect & Developer
- **[Claude](https://claude.ai)** &mdash; AI Architecture Consultant, Pair Programmer & Automated Testing Partner

---

## License

This project is open-sourced under the [MIT License](LICENSE).
