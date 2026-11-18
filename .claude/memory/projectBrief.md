# Project Brief - MyDeusTools

## Vision & Purpose
**MyDeusTools** is a unified, privacy-first **12-utility productivity and developer suite** structured as a **Hybrid Web + Windows Companion Architecture** alongside the original **Native WPF Desktop App**:

- **`MyDeusTools.Web`**: Pure client-side Web & PWA suite built with **React 19, Vite 8, TypeScript, and Tailwind CSS v4**, styled in an ultra-crisp **TerminalUI** design system (`JetBrains Mono`, `0px` corners, Dark `#0D0D0D` / Light `#F4F4EE` themes, toggleable CRT scanlines). All data stays inside browser memory and IndexedDB.
- **`MyDeusTools.Agent`**: Optional zero-dependency **.NET 8** loopback HTTP bridge (`http://127.0.0.1:41950/`) extending the Web App with real Windows OS automation (`mouse_event`/`F6`/`F7` global auto-clicking, `shutdown.exe` power scheduling, and `EnumWindows`/`HWND_TOPMOST`/`WS_EX_LAYERED` window pinning).
- **`MyDeusTools.App`**: Original standalone Windows 10/11 desktop app built with **C# .NET 8 (WPF)** and **WPF-UI 3.0** (Fluent Design & Mica backdrop), packable into a zero-dependency single-file `.exe`.

---

## The 12 Core Tools

1. **Sticky Notes (`01`)**: Colored Kanban/Canvas board, search, pin-to-top, IndexedDB auto-save, Document PiP pop-out (`Web`); borderless floating `StickyNoteWindow` (`WPF`).
2. **Clipboard Manager (`02`)**: Tagged snippet vault with usage counts, search, OS clipboard paste/copy (`Web`); Win32 `AddClipboardFormatListener` realtime listener (`WPF`).
3. **Smart Timer & Shutdown (`03`)**: Two strict independent tabs in Web &mdash; `01:POMODORO_TIMER` (always enabled in browser + Wake Lock + Webhook + `70m -> 1h 10m` auto-normalization) & `02:SCHEDULE_SHUTDOWN` (**strictly enabled only when Agent is online**, Shutdown/Restart/Hibernate/Cancel); native `ShutdownViewModel` (`WPF`).
4. **Text & Dev Tools (`04`)**: JSON Studio, Base64 (text/files), SHA-1/256/512 (`Web`) + MD5/HTML/File-Hash (`WPF`), case converter, URL encoder.
5. **QR Code Studio (`05`)**: High-contrast QR generator with theme presets, ECC levels, PNG export/clipboard (`Web`); full `QRCoder` + `ZXing.Net` + screen snipping (`WPF`).
6. **Color Picker & Eyedropper (`06`)**: Native `EyeDropper API` + HEX/RGB/HSL + IndexedDB history (`Web`); fullscreen GDI 11x11 magnifier HUD (`WPF`).
7. **PDF Toolkit (`07`)**: Client-side Merge & page-range Split via `pdf-lib` (`Web`); `PDFsharp 6.1.1` Merge/Split/Images-to-PDF/Encryption/Metadata (`WPF`).
8. **Image Studio & Optimizer (`08`)**: HTML5 Canvas WebP/JPEG/PNG batch compressor, scale, and EXIF strip (`Web`); multi-layer `.ico` (`16-256px`) & BMP/TIFF (`WPF`).
9. **Video Converter (`09`)**: Web client container/resolution/CRF configuration flow (`Web`); native FFmpeg CLI engine with GPU acceleration, trimming, stream copy & progress parsing (`WPF`).
10. **Bulk File & Folder Renamer (`10`)**: Direct disk renaming via Chromium `File System Access API` + uploaded `.zip` export (`Web`); full NTFS-safe Regex groups, numbering, date tags, conflict checks & 1-click Undo (`WPF`).
11. **Auto Clicker Pro (`11`)**: Dual-tab Web DOM Sandbox + Bookmarklet vs. Agent OS Native Clicker (`F6`/`F7` global hotkeys, Left/Right/Middle, Single/Double, >=10ms clamp, waypoint replay); `AutoClickService` + `RecordingOverlayWindow` (`WPF`).
12. **Window Pinner & PiP (`12`)**: Dual-tab Document PiP widgets (Digital Clock, Scratchpad) vs. Agent OS Windows Manager (`HWND_TOPMOST`, opacity `40-100%`, focus window); `WindowPinnerService` (`WPF`).

---

## Foundation & Quality Baselines

- **100% Bilingual Parity**: Symmetrical `vi` and `en` in Web (`translations.ts` `_viSymmetryCheck`) and `vi-VN` / `en-US` in WPF (`Strings.*.xaml`).
- **Verification Standard**: `npm run web:build` (`tsc && vite build`) passes cleanly + `dotnet test` maintains **282/282 (100% pass rate)**.
- **Zero-Warning Release**: `dotnet build -c Release` compiles cleanly with 0 errors.
