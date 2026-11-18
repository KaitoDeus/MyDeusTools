# MyDeusTools - Features & Utilities Manual

This document describes all **12 utilities** in MyDeusTools across **Web (`MyDeusTools.Web`)**, **Local Companion Agent (`MyDeusTools.Agent`)**, and **Native Desktop (`MyDeusTools.App`)**.

---

## Cross-Cutting Shell Capabilities

- **Dashboard & Command Filtering (`Ctrl+K`)**: Press `Ctrl+K` (or `Cmd+K`) anywhere in the Web app to jump to `00` Dashboard (`./dashboard`), filter tools via `grep_utilities`, or switch categories (`All (12)`, `Productivity`, `Developer`, `Media`, `System`).
- **TerminalUI Light/Dark & CRT Scanlines**: Switch between High-Contrast Dark (`#0D0D0D` / `#00FF41`) and TTY Paper Light (`#F4F4EE` / `#007A1F`) modes (`mydeustools_theme` in `localStorage`) and toggle retro CRT scanlines (`CRT:ON` / `CRT:OFF`, persisted in `mydeustools_crt`). On WPF Desktop, WPF-UI handles Fluent Mica Dark/Light theming with minimize-to-tray (`Hardcodet.NotifyIcon.Wpf`) and HKCU Windows startup toggle.
- **Agent Connection Status Monitor**: The Web top bar continuously polls `http://127.0.0.1:41950/api/health` every 5 seconds, displaying `[AGENT:ONLINE]` or `[AGENT:OFFLINE]`. Clicking it triggers an immediate recheck and navigates to tool `11`.
- **Zero-Restart Bilingual UI (`vi` / `en`)**: Instant switching between Vietnamese and English in both Web (`LanguageContext` + `translations.ts`) and Desktop WPF (`LanguageService` + `Strings.*.xaml`).

---

## 01. Sticky Notes (`./sticky-notes`)

- **Web (`StickyNotesView`)**:
  - Create notes with 4 TerminalUI semantic accents (`GREEN`, `AMBER`, `CYAN`, `RED`).
  - Live `grep_notes` filtering across note titles and payloads.
  - `[PIN]` / `[*PIN]` sorting prioritizes pinned notes at the top.
  - Persistent local IndexedDB storage (`MyDeusToolsWebDB.notes` via Dexie).
  - `[PiP]` pop-out button spawns any note inside an always-on-top **Document Picture-in-Picture** window styled to match current Light/Dark TerminalUI tokens.
- **Desktop WPF (`StickyNotePage` & `StickyNoteWindow`)**:
  - Borderless, draggable, resizable floating desktop windows with `Topmost` toggle, auto-save on keystroke, and JSON persistence (`Data/notes.json`).

---

## 02. Clipboard Manager (`./clip-vault`)

- **Web (`ClipboardHubView`)**:
  - Save reusable code/SQL/auth snippets with uppercase category tags, pin state, and automatic `USES:` click counters in IndexedDB (`MyDeusToolsWebDB.snippets`).
  - Instant search across title, content, and tag.
  - `PASTE_OS_CLIPBOARD`: Reads text directly from the OS clipboard via `navigator.clipboard.readText()` (with browser permission) and stores it in the vault.
- **Desktop WPF (`ClipboardPage`)**:
  - Subscribes at OS level using Win32 `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE` (no polling), guarded by `_isInternalCopy` re-entrancy prevention, saving to `Data/clipboard.json`.

---

## 03. Smart Timer & Shutdown (`./smart-timer`)

### Web Mode (`SmartTimerView`) &mdash; Two Strict Independent Tabs

1. **Tab 1: `01:POMODORO_TIMER` (Pure Browser &mdash; Always Enabled)**
   - Independent countdown timer (default `00:25:00`, presets `[15M_BREAK]`, `[25M_FOCUS]`, `[45M_DEEP]`, `[70M->1H10M]`).
   - **Automatic Time Normalization**: Entering raw overflow minutes/seconds (e.g., `70` minutes) automatically resolves to `1h 10m 0s` on blur, pressing `Enter`, or clicking `Start`.
   - **Screen Wake Lock API**: Acquires `navigator.wakeLock.request('screen')` while the timer is running (`[ACTIVE]` / `[STANDBY]`) to prevent monitor sleep.
   - **Alarm & Webhook**: Generates an 880Hz/1100Hz Web Audio oscillator chime and optionally sends a `POST` payload `{ event: 'timer_finished' }` to a user-provided webhook URL.

2. **Tab 2: `02:SCHEDULE_SHUTDOWN` (Windows OS &mdash; Agent-Online Gate)**
   - Completely independent state and clock (default `00:30:00`, presets `[30_MIN]`, `[1_HOUR]`, `[70M->1H10M]`, `[2_HOURS]`).
   - **Strict Offline Lock (`disabled={!isOnline}`)**: When `MyDeusTools.Agent` is offline, the tab badge shows `[LOCKED]`, status shows `STATUS:LOCKED`, a `[WARN]` alert displays `npm run agent:dev` + `[RECHECK_AGENT]`, and all action controls (`SHUTDOWN PC`, `RESTART PC`, `HIBERNATE PC`, `hh`/`mm`/`ss` inputs, presets, `SCHEDULE_SHUTDOWN`, `Reset`, `CANCEL_OS_SHUTDOWN`) are strictly disabled.
   - **When Agent Online**: Dispatches `POST /api/system/schedule` (`shutdown`, `restart`, `hibernate`) or `POST /api/system/cancel` directly to Windows `shutdown.exe`.

### Desktop WPF (`ShutdownPage`)
- Native implementation executing `shutdown /s /f /t`, `shutdown /r /f /t`, `timeout ... && shutdown /h`, and `shutdown /a` with `DispatcherTimer` live countdown and `70m -> 1h 10m` normalization.

---

## 04. Text & Dev Tools (`./text-devtools`)

- **Web (`TextDevToolsView`, 5 Tabs)**:
  - `01:JSON_STUDIO`: 2-space pretty formatting, minification, and `JSON.parse` syntax error inspection.
  - `02:BASE64`: UTF-8 safe `encode` / `decode` plus `$ FILE_TO_BASE64` binary file DataURL encoding.
  - `03:CRYPTO_HASH`: Real asynchronous digest generation (`SHA-1`, `SHA-256`, `SHA-512`) using `window.crypto.subtle.digest`.
  - `04:CASE_CONVERT`: Converts between `UPPERCASE`, `lowercase`, `camelCase`, `PascalCase`, `snake_case`, `kebab-case`, and `Title Case`, updating character/word/line metrics.
  - `05:URL_ENTITIES`: Safe `encodeURIComponent` / `decodeURIComponent` transformations.
- **Desktop WPF (`TextUtilityPage`)**:
  - Adds `MD5`, HTML entity encode/decode (`WebUtility`), binary Base64-to-file decoding, and streaming hashes over local files.

---

## 05. QR Code Studio (`./qr-studio`)

- **Web (`QrStudioView`)**:
  - Live canvas/image QR code rendering powered by `qrcode` (`L` 7%, `M` 15%, `Q` 25%, `H` 30% error correction).
  - Foreground/background custom hex pickers plus 4 TerminalUI presets (`HACKER`, `CLASSIC`, `CYAN`, `AMBER`).
  - Export as `.png` download or copy PNG directly to clipboard (`ClipboardItem`).
  - Accepts uploaded image files or pasted clipboard images (`navigator.clipboard.read()`), reading image dimensions cleanly inside the browser sandbox.
- **Desktop WPF (`QrCodePage` & `QrSnippingOverlayWindow`)**:
  - High-definition generation via `QRCoder` and full multi-source decoding via `ZXing.Net` from image files, clipboard, and fullscreen multi-monitor region snipping with URL launch detection.

---

## 06. Color Picker & Eyedropper (`./color-picker`)

- **Web (`ColorPickerView`)**:
  - Native **EyeDropper API** (`window.EyeDropper`) in Chromium browsers to sample any pixel across monitors cleanly, plus `<input type="color">` manual tuner.
  - Automatic conversions to **HEX**, **RGB**, and **HSL** with 1-click clipboard copy.
  - Saves picked colors automatically to IndexedDB (`MyDeusToolsWebDB.colors`) with history reload and clear actions.
- **Desktop WPF (`ColorPickerPage` & `ColorPickerOverlayWindow`)**:
  - Fullscreen GDI screen capture overlay with floating 11x11 pixel magnifier HUD, HSL + HSV formatting, auto-copy on pick, pin-to-palette, and `Data/colors.json`.

---

## 07. PDF Toolkit (`./pdf-toolkit`)

- **Web (`PdfToolkitView`, 100% Client-Side via `pdf-lib`)**:
  - `01:MERGE_PDFS`: Upload multiple PDFs, inspect file sizes & page counts, reorder items (`[UP]` / `[DN]`), merge pages in memory, and download the combined `.pdf`.
  - `02:SPLIT_PAGES`: Inspect source page count and extract arbitrary comma/hyphen page ranges (e.g. `1-3, 5`) into a new `.pdf` without uploading bytes to any server.
- **Desktop WPF (`PdfToolkitPage`, `PDFsharp 6.1.1`)**:
  - Adds 4 full tabs: Merge PDFs, Split PDF (custom range, single pages, N-page chunks), Images-to-PDF (`A4`, `Letter`, `Fit-to-Image` with margins & orientation), and Password Encryption + Document Metadata Editor.

---

## 08. Image Studio (`./img-studio`)

- **Web (`ImageStudioView`)**:
  - Multi-file image queue showing dimensions (`WxH`) and original KB size.
  - Re-encodes images cleanly via HTML5 `<canvas>` (`canvas.toBlob`) into `WebP`, `JPEG`, or `PNG` with configurable quality (`10-100%`) and percentage resolution scale (`25-100%`).
  - Re-encoding through Canvas naturally strips EXIF camera telemetry and GPS metadata for privacy.
- **Desktop WPF (`ImageStudioPage`)**:
  - Multi-threaded batch compression across WebP, PNG, JPEG, ICO, BMP, TIFF; multi-layer Windows `.ico` bundling (`16px`&ndash;`256px`); aspect-ratio fit-within scaling; exact byte savings analytics.

---

## 09. Video Converter (`./video-wasm`)

- **Web (`VideoConverterView`)**:
  - Client workspace for selecting target containers (`MP4 H.264/AAC`, `WEBM VP9/OPUS`, `MP3 Audio-Only`, `GIF Animated`), output resolution (`ORIGINAL`, `1080p`, `720p`, `480p`), and CRF quality preset (`18`, `23`, `28`, `32`), along with local stream preview and export workflow. *(Full native FFmpeg transcoding runs in the Desktop WPF app below.)*
- **Desktop WPF (`VideoConverterPage`)**:
  - Full FFmpeg CLI orchestration supporting MP4, MKV, WebM, AVI, MOV, WMV, GIF, FLV; audio extraction (MP3, WAV, AAC, M4A, FLAC); hardware acceleration flags; stream copy; start/duration trimming; and real-time ETA/progress parsing.

---

## 10. Bulk File & Folder Renamer (`./bulk-rename`)

- **Web (`BulkRenamerView`)**:
  - `MOUNT_DISK_DIRECTORY`: Uses the Chromium **File System Access API** (`window.showDirectoryPicker({ mode: 'readwrite' })`) to inspect disk files and write renamed targets in place.
  - `$ SELECT_BATCH_FILES` + `EXPORT_ZIP`: Cross-browser fallback uploading batches in memory and packaging renamed files into a downloadable `.zip` archive via `JSZip`.
  - Rules pipeline: Regex find & replace, filename prefix insertion, and case transforms (`UPPERCASE`, `lowercase`, `Title Case`) with live `ORIGINAL_NAME -> TARGET_PREVIEW` dry-run status (`MODIFIED` / `NOOP`).
- **Desktop WPF (`BulkRenamerPage`)**:
  - Full NTFS-safe batch engine (temp-file hop for case-only renames, depth-sorted directory renaming), Regex capture groups (`$1`, `$2`), 8 case modes, zero-padded numbering, date tags, file conflict detection, and 1-click transaction Undo.

---

## 11. Auto Clicker Pro (`./auto-click`)

- **Web (`AutoClickerView`) &mdash; Two Tabs**:
  - **Tab 1: `01:WEB_DOM_SANDBOX`**: Safe in-browser click loop (minimum `10ms` clamp, infinite or fixed repeat count, live CPS rate), built-in interactive `TARGET_SANDBOX` hit counter, and a one-click copyable JavaScript **Bookmarklet** (`Escape` aborts) for automating DOM elements on any web tab.
  - **Tab 2: `02:OS_NATIVE_AGENT`** *(requires `MyDeusTools.Agent`)*: Bypasses browser sandbox restrictions to click anywhere on Windows & inside games (`mouse_event` + `SetCursorPos`). Supports Left/Right/Middle buttons, Single/Double clicks, 3-second delayed cursor capture (`CAPTURE_IN_3S`), global **`F6`** (Start/Stop anywhere) and **`F7`** (capture cursor `(X, Y)` anywhere) hotkeys, multi-point sequential waypoint cycling, and 300ms live cursor/click telemetry. Automatically becomes active tab when Agent is online.
- **Desktop WPF (`AutoClickPage` & `RecordingOverlayWindow`)**:
  - `AutoClickService` + `NHotkey.Wpf` global hotkeys + multi-monitor transparent recording overlay placing numbered visual badges at clicked coordinates.

---

## 12. Window Pinner & PiP (`./pip-float`)

- **Web (`WindowPipView`) &mdash; Two Tabs**:
  - **Tab 1: `01:WEB_DOCUMENT_PiP`**: Uses Chromium's `window.documentPictureInPicture.requestWindow` to launch lightweight Always-on-Top **Digital Clock** (`280x180`) and **Scratchpad** (`320x320`) floating windows above all desktop apps.
  - **Tab 2: `02:OS_WINDOWS_MANAGER`** *(requires `MyDeusTools.Agent`)*: Enumerates open Windows application windows (`HWND`, title, `.exe` name, PID, current TopMost & opacity state), pins/unpins `HWND_TOPMOST`, focuses/restores minimized windows, and applies instant opacity presets (`100%`, `80%`, `60%`, `40%` via `WS_EX_LAYERED`). Automatically becomes active tab when Agent is online.
- **Desktop WPF (`WindowPinnerPage`)**:
  - Continuous 20%&ndash;100% slider, foreground-window 1-click pin, search filter, and Win32 `WindowPinnerService`.
