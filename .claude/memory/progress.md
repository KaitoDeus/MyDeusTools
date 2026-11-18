# Progress & Feature Status - MyDeusTools

## 1. Unified Feature Matrix (Web + Agent + Desktop WPF)

| Feature / Module | Web (`MyDeusTools.Web`) | Agent (`MyDeusTools.Agent`) | Desktop (`MyDeusTools.App`) | Status |
|---|---|---|---|---|
| **Web TerminalUI Shell** | Vite 8 + React 19, Lazy loading, Light/Dark & CRT toggles, `Ctrl+K` | Poll `/api/health` every 5s | FluentWindow, Mica, Tray, Startup Registry | Complete |
| **Bilingual Localization** | `vi`/`en` instant toggle (`_viSymmetryCheck`) | Console / JSON neutral | `vi-VN`/`en-US` `{DynamicResource}` | Complete |
| **01 Sticky Notes** | Kanban cards, accents, search, pin, Dexie IndexedDB, PiP | N/A | Floating draggable `StickyNoteWindow` + JSON | Complete |
| **02 Clipboard Manager** | Tagged snippet vault, search, pin, uses count, OS paste | N/A | Real-time Win32 `WM_CLIPBOARDUPDATE` hook | Complete |
| **03 Smart Timer & Shutdown** | 2 tabs: `01:POMODORO_TIMER` (WakeLock, Webhook, `70m->1h10m`) + `02:SCHEDULE_SHUTDOWN` (`disabled={!isOnline}`) | `POST /api/system/schedule` (`/s`, `/r`, `/h`) & `/cancel` (`/a`) | `ShutdownViewModel` + `SystemService` | Complete |
| **04 Text & Dev Tools** | JSON Studio, Base64, SHA-1/256/512, 7 Case modes, URL | N/A | Adds MD5, HTML Entities, binary/file Base64 & Hash | Complete |
| **05 QR Code Studio** | `qrcode` generator + presets, PNG/clipboard, image dimension read | N/A | `QRCoder` + `ZXing.Net` + Screen Snip overlay | Complete |
| **06 Color Eyedropper** | Native `EyeDropper API`, HEX/RGB/HSL, IndexedDB history | N/A | GDI 11x11 magnifier HUD + HSV + JSON | Complete |
| **07 PDF Toolkit** | `pdf-lib` offline Merge & custom range Split | N/A | `PDFsharp` Merge, Split (all modes), Img2PDF, Encrypt | Complete |
| **08 Image Studio** | Canvas batch WebP/JPEG/PNG compress, scale, EXIF strip | N/A | Batch compressor, `.ico` multi-layer, aspect resize | Complete |
| **09 Video Converter** | Container/Resolution/CRF preset workflow UI | N/A | Native FFmpeg CLI wrapper, GPU, trim, progress | Complete |
| **10 Bulk Renamer** | `File System Access API` (`showDirectoryPicker`), ZIP export | N/A | NTFS safe Regex groups, numbering, dates, Undo | Complete |
| **11 Auto Clicker Pro** | 2 tabs: `01:WEB_DOM_SANDBOX` (+ Bookmarklet) + `02:OS_NATIVE_AGENT` | `OsAutoClickEngine` + `GlobalHotkeyService` (`F6`/`F7`) | `AutoClickService` + `RecordingOverlayWindow` | Complete |
| **12 Window Pinner & PiP** | 2 tabs: `01:WEB_DOCUMENT_PiP` (Clock, Pad) + `02:OS_WINDOWS_MANAGER` | `OsWindowPinnerService` (`EnumWindows`, `TOPMOST`, Opacity) | `WindowPinnerService` + 20-100% slider | Complete |

---

## 2. Automated .NET Test Inventory (`dotnet test`)

- **Total Test Count**: **282 tests** (across 20 test classes)
- **Passed**: **282** | **Failed**: **0** | **Skipped**: **0** (100% Pass Rate)

| Test Class | Count | Coverage Scope |
|---|---|---|
| `LanguageServiceTests` | 57 | Dictionary switching, toggling, persistence & critical key symmetry |
| `VideoConverterViewModelTests` | 31 | Container presets, scaling, bitrate, validation & ViewModel state |
| `AppIntegrationTests` | 29 | Full DI container resolution across all WPF Services, ViewModels & Pages |
| `BulkRenamerServiceTests` | 22 | Regex rules, case modes, numbering, conflict checks & NTFS case-only undo |
| `WindowPinnerServiceTests` | 17 | Opacity math, model notifications, UI wheel scroll & ViewModel commands |
| `TextUtilityServiceTests` | 14 | JSON format/minify/validation, Base64, URL/HTML, hashes & cases |
| `PdfToolkitServiceTests` | 14 | Merge, range/single/chunk split, Images-to-PDF, encryption & metadata |
| `ImageStudioServiceTests` | 13 | Batch resizing math, aspect ratios, format output & multi-layer ICO |
| `ShutdownViewModelTests` | 11 | Countdown calculation, scheduling, cancel & `70m -> 1h 10m` normalization |
| `VideoConverterServiceTests` | 10 | FFmpeg CLI flag construction, ffprobe parsing & live progress parsing |
| `ImageStudioViewModelTests` | 10 | Queue management, configuration state & processing commands |
| `ClipboardServiceTests` | 9 | Deduplication, pinned preservation, search & JSON serialization |
| `PdfToolkitViewModelTests` | 8 | Merge/Split/Protect ViewModel state & reindexing commands |
| `QrCodeServiceTests` | 8 | PNG generation, roundtrip encode/decode, URL recognition & ViewModel |
| `OsAutoClickEngineTests` | 8 | Agent 10ms clamp, capture/clear points, double-click, waypoint cycle & repeat limit |
| `ColorPickerServiceTests` | 7 | Screen pixel sampling, color format conversions & palette JSON |
| `BulkRenamerViewModelTests` | 7 | Live preview pipeline, selection toggles & undo flow |
| `AutoClickServiceTests` | 3 | Interval clamping, state tracking & stop logic |
| `StickyNoteServiceTests` | 2 | Note persistence & missing file resilience |
| `AutoStartServiceTests` | 2 | HKCU Registry read/write safety |

---

## 3. Build & Quality Verification Status
- `npm run web:build` (`tsc && vite build`): Passing cleanly (verifies `_viSymmetryCheck` & production bundle `242 modules`).
- `dotnet test`: **282/282 passed**.
- `dotnet build -c Release`: Clean compile (`MyDeusTools.App`, `MyDeusTools.Agent`, `MyDeusTools.Tests`).
