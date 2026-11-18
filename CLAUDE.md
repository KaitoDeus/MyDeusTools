# MyDeusTools - Claude Developer & AI Assistant Guide

## Project Overview
**MyDeusTools** is a modern Windows desktop utility Super-App built with **C# .NET 8 (WPF)**, utilizing **WPF-UI 3.0** (Fluent Design & Mica backdrop), **CommunityToolkit.Mvvm**, and Microsoft Extensions Dependency Injection.

### Core Features (12 Native Utilities)
1. **Auto Clicker Pro**:
   - Customizable mouse buttons (Left, Right, Middle) and click types (Single, Double).
   - Flexible interval settings (Hours, Minutes, Seconds, Milliseconds - min 10ms safety clamp).
   - Global Hotkey listening & assignment (via `NHotkey.Wpf`).
   - Coordinate recording overlay (`RecordingOverlayWindow`) with visual numbered markers for waypoint replay.
2. **Schedule Shutdown**:
   - Supports Actions: `Shutdown` (`/s`), `Restart` (`/r`), `Hibernate` (`/h`).
   - Automated time normalization (e.g. 70m automatically normalizes to 1h 10m).
   - Real-time countdown timer with live display and cancellation (`shutdown /a`).
3. **Sticky Notes**:
   - Create, edit, auto-save, delete notes with JSON persistence (`Data/notes.json`).
   - Pop-out sticky note window (`StickyNoteWindow`) with borderless draggable interface and always-on-top toggle.
4. **Clipboard Manager**:
   - Real-time Windows clipboard listener via Win32 `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE`.
   - Search filter, pin items to top, delete individual items, clear unpinned items, auto-save JSON (`Data/clipboard.json`).
5. **QR Code Studio**:
   - High-definition QR generation (`QRCoder`), export PNG or copy image to clipboard.
   - Screen Snipping QR Scanner (`QrSnippingOverlayWindow`), file image scanner, and clipboard image scanner via `ZXing.Net`.
6. **Color Picker & Eyedropper**:
   - Fullscreen crosshair eyedropper with floating 11x11 pixel Magnifier HUD (`ColorPickerOverlayWindow`).
   - Instant format copying (HEX, RGB, HSL, HSV) and auto-copy to clipboard on pick.
   - Persistent color palette history with pin-to-top support (`Data/colors.json`).
7. **Text & Dev Tools**:
   - JSON Studio: Format (Indented), Minify, Syntax Validation with line/col errors.
   - Base64: Encode/decode text and binary files, save Base64 to file.
   - URL & HTML: Encode and decode URL components and HTML entities.
   - Hash Generator: Simultaneous MD5, SHA-1, SHA-256, SHA-512 for text and files.
   - Case & Inspector: Text metrics and case conversions (camel, Pascal, snake, kebab, UPPER, lower, Title).
8. **Window Pinner & Transparency**:
   - Pin any desktop window Always-on-Top (`HWND_TOPMOST`).
   - Adjust opacity from 20% to 100% via Win32 `WS_EX_LAYERED` and `SetLayeredWindowAttributes`.
   - Quick presets (100%, 80%, 60%, 40%) and 1-click active window pinning.
9. **Video Converter & Transcoder**:
   - Multi-format media transcoding (MP4, MKV, WebM, AVI, MOV, WMV, GIF, FLV).
   - Audio extraction (MP3, WAV, AAC, M4A, FLAC), resolution scaling, CRF quality presets, and video trimming.
10. **Image Studio & Optimizer**:
    - High-performance batch image compressor (WebP, PNG, JPEG, ICO, BMP, TIFF).
    - Multi-layer Windows `.ico` generation (16px to 256px), quality tuning, resizing modes, and EXIF stripping.
11. **PDF Toolkit**:
    - 100% offline PDF processing via `PDFsharp` 6.1.1: Merge PDFs, Split PDF (range, single pages, N-page chunks), Convert images to PDF, Password encryption & metadata editing.
12. **Bulk File & Folder Renamer**:
    - Industrial-grade batch renaming with Regex and capture groups, text insertion/removal/trimming, 8 case conversion modes, sequential numbering with zero padding, date/timestamp insertion, scope filtering, live conflict validation, and 1-click Undo.
13. **Bilingual Localization (EN & VIE)**:
    - Real-time language switching between Vietnamese (`vi-VN`) and English (`en-US`) with zero app restarts via `{DynamicResource}` and `Data/settings.json`.
14. **System Tray & Lifecycle**:
    - Minimize-to-tray on close (`Hardcodet.NotifyIcon.Wpf`), dynamic Dark/Light theme switching, and Windows Startup registry toggle.

---

## Tech Stack & Dependencies
- **Target Framework**: `net8.0-windows` (x64)
- **UI Framework**: WPF with `WPF-UI` (v3.0.5)
- **MVVM Toolkit**: `CommunityToolkit.Mvvm` (v8.4.2)
- **DI Container**: `Microsoft.Extensions.DependencyInjection` (v10.0.7)
- **Global Hotkeys**: `NHotkey.Wpf` (v4.0.0)
- **Tray Icon**: `Hardcodet.NotifyIcon.Wpf` (v2.0.1)
- **QR Generation & Decoding**: `QRCoder` (v1.8.0), `ZXing.Net` (v0.16.11)
- **PDF Engine**: `PDFsharp` (v6.1.1)
- **Testing**: `xunit` (v2.9.2), `Moq` (v4.20.72), `Xunit.StaFact` (v0.3.18), `coverlet.collector` (v6.0.2) - **274/274 tests passing (100%)**

---

## Architecture & Project Structure

```
d:\.MyDeusTools\MyDeusTools\
├── .agents/                      # Antigravity AI Agent Skills
│   └── skills/                   # mydeustools-core, wpf-mvvm, win32-interop, feature-expansion
├── .claude/                      # Claude AI configuration, Memory Bank & Rules
│   ├── MEMORY.md                 # Memory Bank Master Index
│   ├── memory/                   # Detailed Memory Modules (activeContext, projectBrief, systemArchitecture, etc.)
│   └── rules/                    # Specific guidance rules (wpf-standards, windows-api, localization)
├── docs/                         # Official Technical Documentation
│   ├── README.md                 # Master Documentation Index
│   ├── FEATURES.md               # Complete Feature & Utility Manual
│   ├── ARCHITECTURE.md           # Architecture, Layers, DI & Win32 Interop
│   ├── DEVELOPER_GUIDE.md        # Setup, Build, Test & Publishing Guide
│   └── API_REFERENCE.md          # Service & Model API Reference
├── MyDeusTools.App/              # Main Application project
│   ├── App.xaml / App.xaml.cs    # Application entry point & DI configuration
│   ├── MainWindow.xaml (.cs)     # Main FluentWindow, NavigationView & System Tray
│   ├── Services/                 # Business logic & hardware/OS interaction
│   │   └── Impl/                 # 15 Service Interfaces & Implementations
│   ├── ViewModels/               # 13 MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── Views/                    # UI Pages & Sub-Windows
│   │   ├── Pages/                # 12 Navigation Pages
│   │   └── Windows/              # RecordingOverlayWindow, StickyNoteWindow, QrSnippingOverlayWindow, ColorPickerOverlayWindow
│   └── Resources/                # Static assets & bilingual dictionaries (Strings.vi-VN.xaml, Strings.en-US.xaml)
├── MyDeusTools.Tests/            # Automated Unit & Integration Tests (274 Tests)
├── MyDeusTools.Website/          # Static Official Product Website (GitHub Pages)
├── CLAUDE.md                     # Claude Assistant Guidance File (this file)
└── README.md                     # Repository Front-Page Readme
```

---

## Memory Bank Protocols
1. Always consult `.claude/memory/systemArchitecture.md` before adding or modifying service boundaries.
2. Maintain bilingual dictionary symmetry in `MyDeusTools.App/Resources/Languages/Strings.*.xaml`.
3. All PRs and commits must maintain a 100% test pass rate (`dotnet test`) and compile with 0 warnings/errors.
4. Conventional Commits format with co-author attribution.
