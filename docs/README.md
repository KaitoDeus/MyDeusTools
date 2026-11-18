# MyDeusTools Documentation

Welcome to the comprehensive documentation for **MyDeusTools**, a modern all-in-one desktop utility Super-App for Windows 10 and 11 built with **.NET 8 (WPF)**, **WPF-UI 3.0** (Fluent Design & Mica backdrop), **CommunityToolkit.Mvvm**, and Microsoft Extensions Dependency Injection.

---

## Documentation Index

| Document | Description |
|---|---|
| [**FEATURES.md**](FEATURES.md) | Comprehensive catalog and detailed usage manual for all 12 native utilities. |
| [**ARCHITECTURE.md**](ARCHITECTURE.md) | In-depth technical architecture, system layers, DI container, Win32 interop, and data storage. |
| [**DEVELOPER_GUIDE.md**](DEVELOPER_GUIDE.md) | Setup, development prerequisites, building, running tests, single-file publishing, and coding standards. |
| [**API_REFERENCE.md**](API_REFERENCE.md) | Technical reference for all service interfaces, models, enums, and ViewModels. |

---

## Project Overview

- **Target Framework**: .NET 8.0 Windows Desktop (`net8.0-windows`, `win-x64`)
- **UI Framework**: Windows Presentation Foundation (WPF) with [WPF-UI 3.0.5](https://wpfui.lepo.co/)
- **Design Language**: Windows 11 Fluent Design, Rounded Corners, Mica Backdrop, Dynamic Dark/Light Theme
- **Architecture**: Clean Model-View-ViewModel (MVVM) powered by `CommunityToolkit.Mvvm` (v8.4.2)
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection` (v10.0.7)
- **Localization**: Dynamic runtime bilingual switching (English `en-US` and Vietnamese `vi-VN`) via `{DynamicResource}`
- **Test Coverage**: 274 automated unit & integration tests (`xUnit`, `Moq`, `Xunit.StaFact`) with 100% pass rate
- **Packaging**: Self-contained, single-file zero-dependency executable (`publish/MyDeusTools.App.exe`)

---

## The 12 Core Native Utilities

1. **Auto Clicker Pro**: High-frequency mouse automation (Left, Right, Middle, Single/Double), high-precision intervals (down to 10ms safety clamp), global hotkey toggle (`NHotkey`), and multi-monitor full-screen coordinate recording/playback overlay.
2. **Schedule Shutdown**: Automated power state scheduler (Shutdown, Restart, Hibernate) with live countdown display, instant cancellation, and automatic input time normalization (e.g., 70 minutes automatically normalizes to 1h 10m).
3. **Sticky Notes**: Floating desktop notes with borderless draggable windows, always-on-top toggle, and persistent JSON storage.
4. **Clipboard Manager**: Real-time Windows clipboard listener (`AddClipboardFormatListener`), instant search, pin-to-top, delete, and persistent JSON history.
5. **QR Code Studio**: HD QR code generation (`QRCoder`) with PNG export and clipboard copy; multi-source decoding (`ZXing.Net`) from screen region snipping, image files, or clipboard.
6. **Color Picker & Eyedropper**: Fullscreen screen eyedropper with floating 11x11 pixel magnifier HUD, one-click HEX/RGB/HSL/HSV copying, pin-to-palette management, and persistent color history.
7. **Text & Dev Tools**: JSON Studio (format, minify, syntax validation), Base64 encoder/decoder (text & binary files), URL/HTML entity escaping, instant multi-hash generator (MD5, SHA-1, SHA-256, SHA-512), and case conversions.
8. **Window Pinner & Transparency**: Pin any third-party desktop window Always-on-Top (`HWND_TOPMOST`), smoothly adjust window opacity/transparency (`WS_EX_LAYERED`), with quick opacity presets (100%, 80%, 60%, 40%).
9. **Video Converter & Transcoder**: Media transcoder supporting diverse video/audio containers (MP4, MKV, WebM, AVI, MOV, WMV, GIF, MP3, WAV, AAC, FLAC), resolution scaling, CRF quality presets, lossless stream copy, and trimming.
10. **Image Studio & Optimizer**: Batch image compressor and converter supporting WebP, PNG, JPEG, ICO, BMP, and TIFF. Multi-layer Windows `.ico` generation (16px to 256px), quality tuning, scaling modes, and EXIF/metadata stripping.
11. **PDF Toolkit**: 100% offline PDF processing powered by PDFsharp 6.1.1: Merge multiple PDFs, Split PDF (page range, single pages, N-page chunks), Convert images to PDF (A4, Letter, Fit-to-image), Password encryption & document metadata editor.
12. **Bulk File & Folder Renamer**: Batch renaming with Regex and capture groups, text insertion/removal/trimming, 8 case conversion modes, sequential numbering with zero padding, date/timestamp insertion, scope filtering, live conflict validation, and 1-click Undo.

---

## Directory Structure

```
d:\.MyDeusTools\MyDeusTools\
├── .agents/                      # Antigravity AI Agent Skills & Workspace Configurations
│   └── skills/                   # Standardized Agent Skills (Core, MVVM, Win32, Feature Workflow)
├── .claude/                      # Claude AI Configuration, Memory Bank & Rules
│   ├── MEMORY.md                 # Memory Bank Master Index
│   ├── memory/                   # Standardized Memory Modules (activeContext, history, progress, etc.)
│   └── rules/                    # Architectural Guidelines (wpf-standards, windows-api, localization)
├── docs/                         # Official Comprehensive Technical Documentation (This directory)
│   ├── README.md                 # Master Documentation Index
│   ├── FEATURES.md               # Detailed Guide for all 12 Utilities
│   ├── ARCHITECTURE.md           # Architecture, Layers, DI & Win32 Interop
│   ├── DEVELOPER_GUIDE.md        # Setup, Build, Test & Publishing Guide
│   └── API_REFERENCE.md          # Complete Service & Model Reference
├── MyDeusTools.App/              # Main WPF Application Project (.NET 8)
│   ├── Services/                 # Business Logic, OS Interop & Core Services
│   │   └── Impl/                 # Service Interfaces & Implementations
│   ├── ViewModels/               # MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── Views/                    # WPF Views (Pages & Sub-Windows)
│   │   ├── Pages/                # 12 Navigation Pages
│   │   └── Windows/              # Overlays & Floating Tool Windows
│   └── Resources/                # Static Resources, Icons & Bilingual Dictionaries
├── MyDeusTools.Tests/            # Automated Unit & Integration Tests (274 Tests)
├── MyDeusTools.Website/          # Static Official Product Website (GitHub Pages)
├── MyDeusTools.Web/              # Pure Web Application (Vite + React 19 + TypeScript + Tailwind CSS)
├── CLAUDE.md                     # Assistant Guidance File
└── README.md                     # Repository Front-Page Readme
```
