# Operation & History Log - MyDeusTools

This log documents chronological milestones, key feature integrations, refactorings, and architectural evolutions of **MyDeusTools**.

---

## Chronological Milestones

### Milestone 1: Inception & Core Foundation (v0.1.0)
- Initialized solution with .NET 8 (WPF) and Lepo `WPF-UI 3.0` Fluent Design.
- Established Dependency Injection via `Microsoft.Extensions.DependencyInjection`.
- Implemented **Auto Clicker Pro** with configurable intervals, buttons, click types, and `NHotkey.Wpf` hotkey listening.
- Implemented **Schedule Shutdown** with Shutdown, Restart, and Hibernate modes.
- Implemented **Sticky Notes** with card views, borderless floating desktop windows, and JSON persistence.
- Implemented **System Tray** minimize-to-tray lifecycle via `Hardcodet.NotifyIcon.Wpf`.

### Milestone 2: Clipboard & Visual Utilities (v0.2.0 - v0.4.0)
- Integrated **Clipboard Manager** with OS-level `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE` hook, search, and pin-to-top.
- Implemented **QR Code Studio** with `QRCoder` generation and `ZXing.Net` decoding (including fullscreen region snipping overlay).
- Implemented **Color Picker & Eyedropper** with fullscreen sampling, floating 11x11 pixel magnifier HUD, and persistent palette history.
- Implemented **Text & Dev Tools** with JSON Studio, Base64, URL/HTML escaping, instant multi-hash generator, and case converter.

### Milestone 3: Bilingual Localization & Window Controls (v0.5.0 - v0.7.0)
- Implemented runtime **Bilingual Localization (EN & VIE)** via dynamic WPF ResourceDictionaries and `{DynamicResource}`.
- Added persistent language preferences in `Data/settings.json`.
- Implemented **Window Pinner & Transparency** (`WindowPinnerService`): Enumerate desktop windows, toggle `HWND_TOPMOST`, and modulate opacity via `WS_EX_LAYERED`.
- Resolved XAML parsing and mouse scroll bubbling issues.

### Milestone 4: Product Landing Page & Website (v0.8.0)
- Developed responsive product website in `MyDeusTools.Website/` with interactive simulator, dark/light theme, and bilingual toggle.
- Configured automated GitHub Pages deployment workflow (`.github/workflows/deploy-pages.yml`).
- Added standard MIT License and `.nojekyll` configuration.

### Milestone 5: Multimedia & Graphics Suite (v0.9.0 - v0.9.5)
- Implemented **Video Converter & Transcoder** (`VideoConverterService`): Multi-format transcoding (MP4/MKV/WebM/GIF/FLV), audio extraction, resolution scaling, CRF controls, and real-time progress parsing.
- Implemented **Image Studio & Optimizer** (`ImageStudioService`): Batch compression (WebP/PNG/JPEG/ICO/BMP/TIFF), multi-layer Windows `.ico` generation, aspect ratio preserved scaling, and EXIF metadata stripping.
- Expanded automated unit test suite past 200 tests.

### Milestone 6: Productivity Suite Expansion (v1.0.0)
- Implemented **PDF Toolkit** (`PdfToolkitService`): 100% offline PDF processing via `PDFsharp` 6.1.1 (Merge, Split by range/chunks, Images-to-PDF album conversion, Password encryption, Document metadata).
- Implemented **Bulk File & Folder Renamer** (`BulkRenamerService`): Batch renaming with Regex and capture groups, text insertion/removal/trimming, 8 case modes, sequential numbering with zero padding, date tags, live conflict validation, and 1-click Undo.
- Implemented **Automatic Time Normalization** in Schedule Shutdown: Automatically normalizes inputs like 70m into 1h 10m on focus loss, Enter keydown, or Schedule click.
- Automated test suite expanded to **274 tests (100% pass rate)** with zero compiler warnings or errors in Release mode.

### Milestone 7: Documentation & Skills Standardization (Current)
- Standardized comprehensive technical documentation in `docs/` (`README.md`, `FEATURES.md`, `ARCHITECTURE.md`, `DEVELOPER_GUIDE.md`, `API_REFERENCE.md`).
- Established standard Antigravity & AI Agent skills in `.agents/skills/` (`mydeustools-core`, `wpf-mvvm-guidelines`, `win32-interop`, `feature-expansion`).
- Standardized project Memory Bank (`.claude/memory/`) in strict logical sequence.
- Added `localization-standards.md` to `.claude/rules/`.
- Synchronized repository `README.md` and `CLAUDE.md`.
