# Project Brief - MyDeusTools

## Vision & Purpose
**MyDeusTools** is a modern, lightweight, high-performance desktop Super-App for Windows 10 & 11 designed to replace dozens of disjointed, ad-heavy, or legacy utilities with a unified, elegant, and native application.

Built with **C# .NET 8 (WPF)** and **WPF-UI 3.0**, it combines Windows 11 Fluent Design, Mica material, clean MVVM architecture, and zero-dependency standalone single-file packaging.

---

## The 12 Core Native Tools

1. **Auto Clicker Pro**: High-frequency mouse automation (Left, Right, Middle, Single/Double), high-precision intervals (down to 10ms safety clamp), global hotkey toggle (`NHotkey`), and multi-monitor coordinate recording/replay overlay.
2. **Schedule Shutdown**: Timed system power management supporting Shutdown, Restart, and Hibernate actions with real-time countdown, instant cancellation, and automatic input time normalization (e.g., 70 minutes automatically normalizes to 1h 10m).
3. **Sticky Notes**: Card-based desktop note manager with automatic JSON persistence (`Data/notes.json`) and borderless floating draggable windows (`StickyNoteWindow`).
4. **Clipboard Manager**: Real-time clipboard listener utilizing native Win32 APIs (`AddClipboardFormatListener`), instant search filtering, pin-to-top, delete, and persistent JSON history.
5. **QR Code Studio**: High-definition QR generation (`QRCoder`) with PNG export and clipboard copy; multi-source decoding (`ZXing.Net`) supporting screen region snipping (`QrSnippingOverlayWindow`), image files, and clipboard.
6. **Color Picker & Eyedropper**: Fullscreen precision screen eyedropper with floating 11x11 pixel magnifier HUD, one-click HEX/RGB/HSL/HSV format copying, pin-to-palette management, and persistent color history (`Data/colors.json`).
7. **Text & Dev Tools**: Swiss Army knife for text and code: JSON Formatter and Minifier with syntax validator, Base64 encoder/decoder (Text and Files), URL and HTML entity escaping, instant multi-hash generator (MD5, SHA-1, SHA-256, SHA-512), and case conversions.
8. **Window Pinner & Transparency**: Pin any third-party desktop window Always-on-Top (`HWND_TOPMOST`), smoothly adjust real-time opacity and transparency (Win32 `WS_EX_LAYERED`), with quick opacity presets (100%, 80%, 60%, 40%).
9. **Video Converter & Transcoder**: Media transcoder supporting diverse video/audio containers (MP4, MKV, WebM, AVI, MOV, WMV, GIF, FLV, MP3, WAV, AAC, FLAC), resolution scaling, CRF quality controls, and video trimming.
10. **Image Studio & Optimizer**: Batch image compressor and converter supporting WebP, PNG, JPEG, ICO, BMP, and TIFF. Multi-layer Windows `.ico` generation (16px to 256px), quality tuning, scaling modes, and EXIF/metadata stripping.
11. **PDF Toolkit**: 100% offline PDF processing powered by `PDFsharp` 6.1.1: Merge multiple PDFs, Split PDF (page range, single pages, N-page chunks), Convert images to PDF, Password encryption & document metadata editor.
12. **Bulk File & Folder Renamer**: Industrial-grade batch file and directory renaming suite with Regex and capture groups, text insertion/removal/trimming, 8 case conversion modes, sequential numbering with zero padding, date/timestamp insertion, scope filtering, live conflict validation, and 1-click Undo.

---

## Foundation & Cross-Cutting Capabilities

- **Bilingual Localization**: Instant runtime switching between English (`en-US`) and Vietnamese (`vi-VN`) via `{DynamicResource}` with 100% key symmetry and persistence in `Data/settings.json`.
- **System Tray & Window Lifecycle**: Minimize-to-tray on close (`Hardcodet.NotifyIcon.Wpf`), instant Dark/Light theme switching, and Windows Startup registry toggle.
- **Packaging**: Zero-dependency self-contained single-file executable (`MyDeusTools.App.exe`).
- **Testing Standard**: 100% automated test pass rate across all suites (currently 274/274 tests passing).
