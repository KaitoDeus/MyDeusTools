# MyDeusTools

<p align="center">
  <img src="MyDeusTools.App/Resources/avatar.ico" width="80" height="80" alt="MyDeusTools Icon" />
</p>

<p align="center">
  <strong>Modern All-in-One Desktop Utility Super-App for Windows 10 & 11</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/UI-WPF--UI%203.0-0078D4?logo=windows" alt="WPF-UI" />
  <img src="https://img.shields.io/badge/Architecture-MVVM%20Toolkit-blue" alt="MVVM" />
  <img src="https://img.shields.io/badge/Tests-274%20Passed%20(100%25)-brightgreen" alt="Tests" />
  <img src="https://img.shields.io/badge/Platform-Windows%20x64-00A4EF" alt="Platform" />
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License" />
</p>

<p align="center">
  <a href="https://kaitodeus.github.io/MyDeusTools/"><strong>Explore Official Website &amp; Interactive Simulator &rarr;</strong></a>
  &nbsp;|&nbsp;
  <a href="docs/README.md"><strong>Read In-Depth Technical Documentation &rarr;</strong></a>
</p>

---

## Overview

**MyDeusTools** is a lightweight, modern, and versatile Windows desktop utility suite built with **C# .NET 8 (WPF)** and **WPF-UI 3.0** (Windows 11 Fluent Design & Mica Backdrop). Consolidates **12 essential daily productivity and developer tools** into a single cohesive application that packages into a zero-dependency, self-contained single-file executable.

---

## Key Features

| Feature | Description |
|---|---|
| **Auto Clicker Pro** | Highly configurable automated mouse clicking (Left, Right, Middle buttons; Single or Double click), high-precision interval control (down to 10ms safety clamp), global hotkey assignment (NHotkey), and multi-monitor coordinate recording/replay overlay (RecordingOverlayWindow). |
| **Schedule Shutdown** | Timed system power management supporting Shutdown, Restart, and Hibernate actions with real-time visual countdown (hh:mm:ss), instant cancellation, and automatic input time normalization (e.g., 70 minutes automatically normalizes to 1h 10m). |
| **Sticky Notes** | Card-based desktop note manager with automatic JSON persistence (Data/notes.json), and pop-out borderless floating desktop windows (StickyNoteWindow - Topmost, draggable, non-intrusive). |
| **Clipboard Manager** | Real-time clipboard listener utilizing native Win32 APIs (AddClipboardFormatListener), instant search filtering, pin-to-top support, and one-click copy back to clipboard. |
| **QR Code Studio** | High-definition QR code generation (QRCoder) with PNG export and clipboard copy; multi-source QR decoding (ZXing.Net) supporting screen region snipping (QrSnippingOverlayWindow), image file decoding, and clipboard image reading with automatic URL detection. |
| **Color Picker & Eyedropper** | Fullscreen precision screen eyedropper with floating 11x11 pixel magnifier HUD, one-click HEX/RGB/HSL/HSV format copying, pin-to-palette management, and persistent color history (Data/colors.json). |
| **Text & Dev Tools** | Swiss Army knife for text and code: JSON Formatter and Minifier with syntax validator, Base64 encoder/decoder (Text and Files), URL and HTML entity escaping, instant multi-hash generator (MD5, SHA-1, SHA-256, SHA-512), case converter (camel, Pascal, snake, kebab, UPPER, lower, Title), and text metrics analyzer. |
| **Window Pinner & Transparency** | Pin any third-party application window to remain Always-on-Top (HWND_TOPMOST), smoothly adjust real-time opacity and transparency (Win32 WS_EX_LAYERED with SetLayeredWindowAttributes), quick opacity presets (100%, 80%, 60%, 40%), 1-click active window pinning, and bring-to-front window activation. |
| **Video Converter & Transcoder** | Fast and versatile multimedia converter supporting diverse containers (MP4, MKV, WebM, AVI, MOV, WMV, GIF, FLV), audio extraction (MP3, WAV, AAC, M4A, FLAC), resolution scaling (4K to 360p), CRF quality controls (Ultra, High, Medium, Compact), lossless stream copy, GPU hardware acceleration, video trimming, and real-time progress parsing. |
| **Image Studio & Optimizer** | High-performance batch image compressor and converter supporting WebP, PNG, JPEG, ICO, BMP, and TIFF. Features quality level tuning (1-100%), multi-mode scaling (percentage, fixed width/height, aspect-ratio preserved bounding box), multi-layer Windows .ico generation (16px to 256px), EXIF/GPS privacy stripping, and real-time disk space savings calculation. |
| **PDF Toolkit** | 100% offline PDF processing powered by PDFsharp 6.1.1: Merge multiple PDFs, Split PDF (page range, single pages, N-page chunks), Convert images to PDF (A4, Letter, Fit-to-image with margin/orientation options), Password encryption & document metadata editor. |
| **Bulk File & Folder Renamer** | Industrial-grade batch file and directory renaming suite with Regex and capture groups, text insertion/removal/trimming, 8 case conversion modes, sequential numbering with zero padding, date/timestamp insertion, scope filtering, live conflict validation, and 1-click Undo. |
| **Bilingual Localization (EN & VIE)** | Instant runtime switching between English and Vietnamese across all navigation menus and pages without requiring application restarts, with persistent settings (Data/settings.json). |
| **System Tray & Windows Startup** | Minimize-to-tray lifecycle management (Hardcodet.NotifyIcon), instant Dark/Light theme switching, and Windows startup toggle via CurrentUser Registry. |

---

## Documentation

Full documentation is available in the [`docs/`](docs/) directory:

- [**docs/FEATURES.md**](docs/FEATURES.md): In-depth guide and manual for all 12 tools.
- [**docs/ARCHITECTURE.md**](docs/ARCHITECTURE.md): System architecture, layers, DI registrations, Win32 P/Invoke, and storage models.
- [**docs/DEVELOPER_GUIDE.md**](docs/DEVELOPER_GUIDE.md): Setup, compilation, testing, single-file publishing, and contribution standards.
- [**docs/API_REFERENCE.md**](docs/API_REFERENCE.md): Technical API reference for all service interfaces and models.

---

## Technology Stack

- **Framework & Runtime**: C# 12, .NET 8.0 Windows Desktop (`net8.0-windows`, `win-x64`)
- **UI Framework**: WPF with [WPF-UI 3.0.5](https://wpfui.lepo.co/) (Fluent Design, Mica Backdrop, NavigationControl)
- **Architecture**: Model-View-ViewModel (MVVM) powered by [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **OS & Native Interop**:
  - Win32 P/Invoke (`user32.dll` for mouse synthesization, clipboard format listener, window enumeration, `HWND_TOPMOST` pinning, and layered opacity attributes)
  - `NHotkey.Wpf` for global system-wide hotkeys
  - `Hardcodet.NotifyIcon.Wpf` for taskbar notification tray
  - `QRCoder` & `ZXing.Net` for QR code generation and decoding
  - `PDFsharp` 6.1.1 for 100% offline PDF processing
- **Testing**: xUnit 2.9, Moq 4.20, Xunit.StaFact (274/274 tests passing, 100% success rate)

---

## Getting Started

### Prerequisites
- Operating System: Windows 10 or Windows 11 (x64)
- .NET 8.0 SDK (required for building from source)

### 1. Run in Development Mode
```bash
# Clone repository
git clone https://github.com/KaitoDeus/MyDeusTools.git
cd MyDeusTools

# Run application
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

### 2. Run Automated Test Suite
```bash
dotnet test
```

### 3. Build Single-File Self-Contained Executable
```bash
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true
```
The standalone executable will be located at:
`MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe`

---

## Contributors

This project is developed and maintained through Human-AI Pair Programming:

- **[KaitoDeus](https://github.com/KaitoDeus)** — Project Creator, Lead Software Architect & Developer
- **[Claude](https://claude.ai)** — AI Architecture Consultant, Pair Programmer & Automated Testing Partner

---

## License

This project is open-sourced under the [MIT License](LICENSE).
