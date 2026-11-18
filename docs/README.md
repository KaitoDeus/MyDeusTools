# MyDeusTools Documentation

Welcome to the official technical documentation for **MyDeusTools**, an all-in-one **12-utility ecosystem** spanning a **TerminalUI Pure Web App (`MyDeusTools.Web`)**, an optional **Windows Local Companion Agent (`MyDeusTools.Agent`)**, and the original **Native WPF Desktop App (`MyDeusTools.App`)**.

---

## Documentation Index

| Document | Scope |
|---|---|
| [**FEATURES.md**](FEATURES.md) | Exhaustive guide to all 12 utilities across Web, Local Companion Agent, and Native WPF modes. |
| [**ARCHITECTURE.md**](ARCHITECTURE.md) | Hybrid Web + Agent loopback bridge, TerminalUI tokens, WPF MVVM layers, Win32 P/Invoke, and persistence models. |
| [**API_REFERENCE.md**](API_REFERENCE.md) | Local Agent REST endpoints (`127.0.0.1:41950`), `agentClient` / Dexie APIs, TerminalUI components, and C# Service interfaces. |
| [**DEVELOPER_GUIDE.md**](DEVELOPER_GUIDE.md) | Prerequisites, npm & dotnet dev/build commands, 282-test verification, single-file packaging, and coding rules. |

---

## Ecosystem Overview

- **Web App (`MyDeusTools.Web`)**:
  - **Stack**: React `19.3`, Vite `8.3`, TypeScript `7.0`, Tailwind CSS `v4.3`, Dexie `4.4` (IndexedDB), `pdf-lib`, `qrcode`, `jszip`.
  - **UI System**: Strict **TerminalUI** design language (`JetBrains Mono`, `0px` corners, zero shadows/gradients, `#0D0D0D` Dark & `#F4F4EE` Light themes, toggleable CRT scanlines).
  - **Localization**: Instant Vietnamese (`vi`) / English (`en`) switching with compile-time key parity (`_viSymmetryCheck`).
- **Windows Local Companion Agent (`MyDeusTools.Agent`)**:
  - **Target**: `.NET 8.0` (`net8.0-windows`, `win-x64`), zero NuGet dependencies, self-contained single-file publish support.
  - **Transport**: Loopback-only `System.Net.HttpListener` on `http://127.0.0.1:41950/` with CORS & Private Network Access headers.
  - **OS Capabilities**: Unrestricted Win32 auto-clicking + global `F6`/`F7` hotkeys, Windows Shutdown/Restart/Hibernate scheduling, and desktop window Always-on-Top + `WS_EX_LAYERED` opacity controls.
- **Native Desktop App (`MyDeusTools.App`)**:
  - **Target**: `.NET 8.0` (`net8.0-windows`, `win-x64`), WPF + `WPF-UI 3.0.5` (Mica backdrop, FluentWindow), `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.DependencyInjection 10.0.7`.
  - **Packaging**: Self-contained single-file executable (`MyDeusTools.App.exe`).
- **Automated Tests (`MyDeusTools.Tests`)**:
  - **282 automated tests** (`xUnit 2.9.2`, `Moq 4.20.72`, `Xunit.StaFact 0.3.18`) across 20 test classes with **100% pass rate**.

---

## The 12 Core Utilities

| # | Utility | Web Command (`$ ./<cmd>`) | Key Capabilities |
|---|---|---|---|
| `01` | **Sticky Notes** | `./sticky-notes` | Colored Kanban notes (`GREEN`, `AMBER`, `CYAN`, `RED`), search, pin-to-top, IndexedDB auto-save, PiP / Floating Desktop Window. |
| `02` | **Clipboard Manager** | `./clip-vault` | Tagged snippet vault & search (`Web`); real-time `WM_CLIPBOARDUPDATE` listener & JSON history (`WPF`). |
| `03` | **Smart Timer & Shutdown** | `./smart-timer` | Separate **Pomodoro Timer** (`01:POMODORO_TIMER`) & Agent-gated **OS Shutdown/Restart/Hibernate** (`02:SCHEDULE_SHUTDOWN`, `70m -> 1h 10m` normalization). |
| `04` | **Text & Dev Tools** | `./text-devtools` | JSON Studio, Base64 (text/files), SHA/MD5 hashing, 7 case modes, URL/HTML encoding. |
| `05` | **QR Code Studio** | `./qr-studio` | High-contrast QR generator + PNG/clipboard copy (`Web` & `WPF`); multi-source decoder & screen snipping (`WPF`). |
| `06` | **Color Picker & Eyedropper** | `./color-picker` | Native `EyeDropper` API (`Web`) / GDI 11x11 magnifier HUD (`WPF`), HEX/RGB/HSL/HSV formats, persistent history. |
| `07` | **PDF Toolkit** | `./pdf-toolkit` | 100% offline Merge & range Split via `pdf-lib` (`Web`); adds single-page/chunk split, Images-to-PDF, encryption & metadata via `PDFsharp` (`WPF`). |
| `08` | **Image Studio** | `./img-studio` | Canvas WebP/JPEG/PNG batch compression, resize, EXIF strip (`Web`); adds multi-layer `.ico` (`16-256px`) & BMP/TIFF (`WPF`). |
| `09` | **Video Converter** | `./video-wasm` | Web client preset workflow (`MP4`, `WEBM`, `MP3`, `GIF`); full local FFmpeg CLI transcoding with GPU acceleration & trimming (`WPF`). |
| `10` | **Bulk File Renamer** | `./bulk-rename` | `File System Access API` (`showDirectoryPicker`) & `.zip` export (`Web`); adds `$1/$2` capture groups, numbering, dates, conflict checks & 1-click Undo (`WPF`). |
| `11` | **Auto Clicker Pro** | `./auto-click` | **Web DOM Sandbox** + portable Bookmarklet (`Tab 1`); **OS Native Agent** Win32 mouse loop + `F6`/`F7` global hotkeys (`Tab 2` / `WPF`). |
| `12` | **Window Pinner & PiP** | `./pip-float` | **Document Picture-in-Picture** Clock/Scratchpad (`Tab 1`); **OS Windows Manager** `HWND_TOPMOST` + 20-100% layered opacity (`Tab 2` / `WPF`). |

---

## Directory Structure

```
d:\.MyDeusTools\MyDeusTools\
├── .agents/skills/               # Antigravity AI Skills (mirrored in .claude/skills/)
├── .claude/                      # Claude Memory Bank, Rules & Mirrored Skills
├── docs/                         # Technical Documentation (This directory)
├── MyDeusTools.Web/              # TerminalUI React 19 + TypeScript Web App
├── MyDeusTools.Agent/            # .NET 8 Local Companion HTTP + Win32 Bridge
├── MyDeusTools.App/              # .NET 8 Native Windows WPF Super-App
├── MyDeusTools.Tests/            # 282 xUnit Automated Tests (App + Agent)
├── MyDeusTools.Website/          # Static Product Website (GitHub Pages)
├── MyDeusTools.sln               # Visual Studio / dotnet Solution
├── package.json                  # Unified npm scripts (web:* & agent:*)
├── CLAUDE.md                     # AI Assistant Developer Guide
└── README.md                     # Repository Front-Page Readme
```
