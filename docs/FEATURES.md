# MyDeusTools - Features & Utilities Manual

This document provides an exhaustive, in-depth guide to all **12 native utilities** included in MyDeusTools.

---

## 1. Auto Clicker Pro

High-performance automated mouse clicker designed for productivity workflows, data entry, repetitive software testing, and gaming automation.

- **Mouse Buttons**: Left, Right, Middle.
- **Click Types**: Single Click, Double Click.
- **Interval Control**: Granular hours, minutes, seconds, and milliseconds with a safety clamp of minimum 10ms to prevent system lockup.
- **Repeat Limits**: Infinite click loop (until stopped) or fixed click count.
- **Global Hotkey Integration**: Background hotkey listener via `NHotkey.Wpf` (default: `F6`), allowing instant start/stop even when MyDeusTools is minimized or out of focus.
- **Coordinate Replay & Multi-Monitor Overlay (`RecordingOverlayWindow`)**:
  - Fullscreen transparent overlay spanning the entire virtual desktop (`SystemParameters.VirtualScreenLeft/Top/Width/Height`).
  - Interactive click recording: each click places a numbered visual badge at the exact screen coordinates.
  - Waypoint replay: automatically replays recorded coordinates in sequential order with the configured delay between each point.
  - Reset & clear waypoints on demand.

---

## 2. Schedule Shutdown

Automated power management utility for scheduled system shutdowns, restarts, and hibernations.

- **Action Modes**:
  - **Shutdown** (`shutdown.exe /s /t ...`): Full system power-off.
  - **Restart** (`shutdown.exe /r /t ...`): Complete OS reboot.
  - **Hibernate** (`shutdown.exe /h`): Dumps RAM to disk and powers down.
- **Automatic Time Normalization**:
  - Resolves any human entry gracefully: for example, entering **70 minutes** automatically normalizes to **1h 10m 0s** (4,200 seconds).
  - Triggers on input box focus loss (`LostFocus`), pressing Enter (`KeyDown`), or clicking the **Start** button.
  - Extended input bounds (Hours up to 999, Minutes up to 9999, Seconds up to 9999).
- **Real-Time Visual Countdown**:
  - Displays remaining time in `hh:mm:ss` format.
  - Updates every second via `DispatcherTimer`.
- **Instant Cancellation**:
  - Cancels active pending OS schedules via `shutdown.exe /a` and stops internal timer countdowns.

---

## 3. Sticky Notes

Lightweight desktop note manager with persistent storage and floating windows.

- **Note Organization**:
  - Dynamic note cards with creation timestamps and content preview.
  - Add, edit, delete individual notes, or clear all notes.
- **Floating Desktop Windows (`StickyNoteWindow`)**:
  - Pop out any note into a borderless, movable desktop window.
  - Drag anywhere on the header to reposition.
  - Pin-to-top toggle: keeps the note floating above all other application windows.
  - Real-time auto-saving as the user types.
- **Data Persistence**:
  - Serialized asynchronously to `Data/notes.json` with thread-safe file handling.

---

## 4. Clipboard Manager

Real-time Windows clipboard listener with history management and instant search.

- **Native Win32 Hook**:
  - Registers `AddClipboardFormatListener` to receive OS-level `WM_CLIPBOARDUPDATE` messages without polling.
  - Re-entrancy protection (`_isInternalCopy`) ensures copying from within MyDeusTools does not trigger redundant entries.
- **History Management**:
  - Instant search filter with real-time text matching.
  - Pin important items to top to prevent accidental deletion.
  - One-click copy back to clipboard.
  - Clear all unpinned items in one action.
- **Data Persistence**:
  - Stored in `Data/clipboard.json` with automated loading on startup.

---

## 5. QR Code Studio

Complete QR code generator and multi-source decoder.

- **Generation Engine (`QRCoder`)**:
  - Pure C# managed QR generation with customizable error correction levels.
  - Export generated QR codes to PNG image files or copy image directly to Windows clipboard.
- **Multi-Source Decoding Engine (`ZXing.Net`)**:
  - **Screen Snipping Scanner (`QrSnippingOverlayWindow`)**: Fullscreen crosshair snipping tool with multi-monitor virtual screen support. Drag to capture a rectangle; the overlay hides momentarily to capture pure screen pixels and decode instantly.
  - **File Scanner**: Select any local image file (.png, .jpg, .bmp, .webp) to decode.
  - **Clipboard Image Scanner**: Read and decode images directly from the clipboard.
  - **Smart URL Detection**: Automatically detects web URLs and displays a clickable link to open directly in the default browser.

---

## 6. Color Picker & Eyedropper

Pixel-precise screen color sampler with magnifying glass HUD and color palette history.

- **Fullscreen Screen Eyedropper (`ColorPickerOverlayWindow`)**:
  - Transparent fullscreen window capturing exact screen pixels via GDI `GetPixel` / `CopyFromScreen`.
  - Floating 11x11 pixel Magnifier HUD following the cursor, displaying individual pixels with a crosshair on the center pixel.
  - Shows live HEX code and RGB values under the mouse.
- **Format Support**:
  - Instant copying for **HEX**, **RGB**, **HSL**, and **HSV** color formats.
  - Auto-copy on pick: selected color is immediately copied to clipboard.
- **Palette History**:
  - Save sampled colors to a persistent color list (`Data/colors.json`).
  - Pin favorite colors to top; copy or remove individual colors.

---

## 7. Text & Dev Tools

All-in-one developer utility for text transformations, encoding, hashing, and formatting.

- **JSON Studio**:
  - Format JSON (indented with 2 spaces).
  - Minify JSON (strips whitespace and newlines).
  - Validate JSON syntax with detailed line/column error reporting via `System.Text.Json`.
- **Base64 Converter**:
  - Text: Encode text to Base64 (UTF-8) and decode Base64 back to text.
  - Files: Encode any binary file to Base64 string; save Base64 string back into binary file.
- **URL & HTML Entities**:
  - URL Encode (`Uri.EscapeDataString`) and URL Decode (`Uri.UnescapeDataString`).
  - HTML Entity Encode (`WebUtility.HtmlEncode`) and Decode (`WebUtility.HtmlDecode`).
- **Instant Hash Generator**:
  - Computes **MD5**, **SHA-1**, **SHA-256**, and **SHA-512** simultaneously from text input or selected files.
- **Case Converter & Text Inspector**:
  - Case Modes: `camelCase`, `PascalCase`, `snake_case`, `kebab-case`, `UPPERCASE`, `lowercase`, `Title Case`.
  - Text Metrics: Character count, word count, line count, and UTF-8 byte size.

---

## 8. Window Pinner & Transparency

Desktop window controller for multitasking, side-by-side coding, and video referencing.

- **Always-on-Top Controller**:
  - Enumerate visible desktop windows (`user32.dll EnumWindows` + `GetWindowText`).
  - Pin any selected window to remain permanently on top (`SetWindowPos` with `HWND_TOPMOST`).
  - Quick action to pin the currently active foreground window with a single click.
- **Smooth Opacity & Transparency**:
  - Adds `WS_EX_LAYERED` window style and modulates alpha channel via `SetLayeredWindowAttributes`.
  - Continuous slider from 20% to 100% opacity.
  - One-click presets: 100% (Solid), 80%, 60%, 40% (Ghost).
- **Window Activation**:
  - Bring selected window to front (`SetForegroundWindow`).

---

## 9. Video Converter & Transcoder

High-speed multimedia conversion engine powered by FFmpeg integration.

- **Container Formats**:
  - Video: MP4, MKV, WebM, AVI, MOV, WMV, FLV, Animated GIF.
  - Audio Extraction: MP3, WAV, AAC, M4A, FLAC.
- **Transcoding Controls**:
  - Resolution Scaling: Original, 4K (3840x2160), 1080p, 720p, 480p, 360p.
  - Quality Modes: Ultra Quality (CRF 18), High (CRF 23), Medium (CRF 28), Compact (CRF 32).
  - Stream Copy: Lossless fast transmuxing when video/audio codecs match.
  - Hardware Acceleration: NVIDIA NVENC / Intel QuickSync / AMD AMF support when available.
  - Trimming: Configurable start time and duration.
  - Real-time progress bar with percentage, speed, and ETA.

---

## 10. Image Studio & Optimizer

Batch image compression, format conversion, and icon generation utility.

- **Supported Formats**: WebP, PNG, JPEG, ICO, BMP, TIFF.
- **Batch Compression**:
  - Multi-threaded processing of multiple images.
  - Quality tuning slider (1% to 100%).
  - Disk space savings reporting: computes exact bytes saved and percentage reduction.
- **Resolution Resizing Modes**:
  - Percentage scale (e.g. 50%, 75%).
  - Fixed dimensions (Width x Height).
  - Fit-within bounding box (preserves original aspect ratio).
- **Multi-Layer Windows `.ico` Generator**:
  - Generates valid multi-resolution Windows icon files packaging 16x16, 32x32, 48x48, 64x64, 128x128, and 256x256 layers.
- **Privacy Stripping**:
  - Strips EXIF metadata, camera serials, and GPS coordinates for privacy-safe sharing.

---

## 11. PDF Toolkit

Comprehensive 100% offline PDF manipulation toolkit powered by `PDFsharp` 6.1.1.

- **Tab 1: Merge PDFs**:
  - Drag and drop or browse multiple PDF files.
  - Inspect file size, total page count, and status.
  - Reorder items via Move Up / Move Down buttons.
  - Output to a merged document with progress reporting.
- **Tab 2: Split PDF**:
  - **Custom Page Range**: Flexible syntax such as `1-3, 5, 7-10`.
  - **Single Page Extraction**: Splits every page into an individual PDF file.
  - **N-Page Chunks**: Splits document into uniform chunks of $N$ pages per file.
- **Tab 3: Images to PDF**:
  - Convert image collections (.png, .jpg, .bmp, .webp, .tiff) into a single PDF album.
  - Page Sizes: **Fit to Image**, **A4**, **Letter**.
  - Page Orientations: **Portrait**, **Landscape**.
  - Margins: None (0 pt), Narrow (10 pt), Standard (20 pt), Wide (36 pt).
- **Tab 4: Protect & Metadata**:
  - Password Encryption: Set User Password (open document) and Owner Password (permissions).
  - Metadata Editor: Update Document Title, Author, Subject, and Keywords.

---

## 12. Bulk File & Folder Renamer

Industrial-grade batch file and directory renaming suite with live preview and safety conflict detection.

- **Rule Tabs**:
  - **Find & Replace**: Plain text or Regular Expressions (Regex) with capture group replacement (`$1`, `$2`), case sensitivity toggle.
  - **Insert & Remove**: Add prefix at start, suffix at end, insert text at specific character index $N$, remove first $N$ characters, remove last $N$ characters, trim whitespace.
  - **Case Conversion**: 8 transformation modes (`UPPERCASE`, `lowercase`, `Title Case`, `Sentence case`, `camelCase`, `PascalCase`, `snake_case`, `kebab-case`).
  - **Numbering & Date**: Sequential numbering (Start, Step, Zero Padding `01, 001`), placement (Prefix, Suffix, Replace), date insertion (Current date, Date Modified, Date Created) with customizable format strings (`yyyy-MM-dd`).
- **Scope & Options**:
  - Target: Files and Folders, Files Only, Folders Only.
  - Extension Toggle: Include or exclude file extension from rule modifications.
  - Recursive: Scan subdirectories recursively.
- **Live Preview & Conflict Detection**:
  - Instant preview of every file's new name before committing changes.
  - Visual status badges:
    - **Ready** (Green): Valid new name.
    - **Unchanged** (Gray): Name remains identical.
    - **Duplicate in Batch** (Red): Multiple files would receive the same name.
    - **Target Exists on Disk** (Red): A file with the new name already exists.
    - **Invalid Characters** (Red): Contains forbidden Windows characters (`\ / : * ? " < > |`).
- **One-Click Undo**:
  - Maintains transaction history of the last batch rename.
  - Restores all files and folders to their exact original names with one click.
