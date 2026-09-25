# History & Operations Log - MyDeusTools

This log records major architecture changes, development operations, refactoring, and milestones.

---

### [2026-08-31] Project Exploration & Claude AI Setup (Memory Bank System)
- **Action**: Comprehensive repository scan and audit of all solution files (`MyDeusTools.App`, `MyDeusTools.Tests`, `MyDeusTools.sln`).
- **Verification**: Executed test suite (`dotnet test`). All 20/20 unit and integration tests passed.
- **Created**:
  - `CLAUDE.md`: Master project guide, build/test/publish commands, architecture overview, and guidelines.
  - `.claude/MEMORY.md`: Central index and instructions for Claude AI interactions.
  - `.claude/memory/projectBrief.md`: Mission, requirements, feature scope.
  - `.claude/memory/systemArchitecture.md`: Layer-by-layer breakdown, DI configuration, data flow pipelines.
  - `.claude/memory/technicalPatterns.md`: MVVM standards, P/Invoke, WPF-UI theming, Dispatcher rules.
  - `.claude/memory/activeContext.md`: Current runtime state and session focus.
  - `.claude/memory/progress.md`: Feature matrix, test results, future roadmap.
  - `.claude/memory/history.md`: Chronological changelog.
  - `.claude/rules/wpf-standards.md`: Specific coding guidelines for WPF and XAML.
  - `.claude/rules/windows-api.md`: Specific rules for Win32 interop and registry management.
- **Result**: Complete AI memory configuration established.

---

### [2026-09-24] Feature Implementation: Clipboard Manager
- **Action**: Designed and developed native Windows Clipboard Manager feature.
- **Components Created**:
  - `MyDeusTools.App/Services/Impl/IClipboardService.cs`: Service interface and `ClipboardItemModel` with preview, timestamp, character count, and pin state.
  - `MyDeusTools.App/Services/ClipboardService.cs`: Win32 `AddClipboardFormatListener` / `RemoveClipboardFormatListener` integration, STA thread-safe reading, deduplication, item capacity enforcement, and JSON persistence (`Data/clipboard.json`).
  - `MyDeusTools.App/ViewModels/ClipboardViewModel.cs`: Real-time search filtering, pin management, copy-back command, and clear actions.
  - `MyDeusTools.App/Views/Pages/ClipboardPage.xaml` & `.xaml.cs`: Fluent UI page with search bar, card-based history entries, pin badges, empty state illustration, and copy/delete actions.
  - `MyDeusTools.Tests/ClipboardServiceTests.cs`: 9 unit tests covering deduplication, pin preservation, max item limits, search filtering, and serialization.
- **Integration**:
  - Connected `MainWindow.xaml.cs` to message pump hook (`WM_CLIPBOARDUPDATE = 0x031D`).
  - Registered `IClipboardService`, `ClipboardViewModel`, and `ClipboardPage` in `App.xaml.cs`.
  - Added new `NavigationViewItem` to `MainWindow.xaml`.
  - Extended `AppIntegrationTests.cs` to verify DI resolution.
- **Verification**: Executed `dotnet test`. All 32/32 tests passed (100%).

---

### [2026-09-24] Feature Implementation: QR Code Studio
- **Action**: Designed and developed QR Code Studio (Generator & Multi-Source Scanner).
- **Packages Integrated**:
  - `QRCoder` (v1.8.0): Pure managed C# QR encoder.
  - `ZXing.Net` (v0.16.11): Barcode/QR code reader and decoder.
- **Components Created**:
  - `MyDeusTools.App/Services/Impl/IQrCodeService.cs`: Interface for generating and decoding QR codes.
  - `MyDeusTools.App/Services/QrCodeService.cs`: PNG byte generation, WPF BitmapSource conversion, RGBLuminanceSource hybrid binarizer decoding, file decoding, and GDI screen region capture decoding.
  - `MyDeusTools.App/Views/Windows/QrSnippingOverlayWindow.xaml` & `.xaml.cs`: Fullscreen crosshair snipping overlay with rubber-band rectangle selection across multi-monitors.
  - `MyDeusTools.App/ViewModels/QrCodeViewModel.cs`: Two-tab state (Generator / Scanner), image preview, PNG file export, clipboard copy, screen snipping trigger, and browser URL opener.
  - `MyDeusTools.App/Views/Pages/QrCodePage.xaml` & `.xaml.cs`: Modern Fluent interface with dark-mode friendly white QR card, multi-source scanner buttons, and formatted result viewer.
  - `MyDeusTools.Tests/QrCodeServiceTests.cs`: 8 unit and integration tests covering generation bytes, roundtrip encode/decode, file decoding, and URL recognition.
- **Integration**:
  - Registered `IQrCodeService`, `QrCodeViewModel`, and `QrCodePage` in `App.xaml.cs`.
  - Added new `NavigationViewItem` to `MainWindow.xaml` (`SymbolRegular.QrCode24`).
  - Extended `AppIntegrationTests.cs` to verify DI resolution.
- **Verification**: Executed `dotnet test`. All 43/43 tests passed (100%).

---

### [2026-09-24] Feature Implementation: Color Picker & Screen Eyedropper
- **Action**: Designed and developed Color Picker & Screen Eyedropper with floating pixel magnifier HUD.
- **Components Created**:
  - `MyDeusTools.App/Services/Impl/IColorPickerService.cs`: Interface and `ColorItemModel` supporting HEX, RGB, HSL, HSV representations and JSON serialization exclusions (`[JsonIgnore]`).
  - `MyDeusTools.App/Services/ColorPickerService.cs`: Win32 `GetDC` / `GetPixel` / GDI `CopyFromScreen` screen sampling, deduplication, capacity management, and async JSON persistence (`Data/colors.json`).
  - `MyDeusTools.App/Views/Windows/ColorPickerOverlayWindow.xaml` & `.xaml.cs`: Fullscreen crosshair canvas with real-time floating 11x11 pixel Magnifier HUD, center reticle marker, live swatch, and HEX/RGB labels.
  - `MyDeusTools.App/ViewModels/ColorPickerViewModel.cs`: State management, overlay launcher, auto-copy to clipboard on pick, format copier, and pin/delete handlers.
  - `MyDeusTools.App/Views/Pages/ColorPickerPage.xaml` & `.xaml.cs`: Hero color inspector card, quick copy chips, and color history wrap grid.
  - `MyDeusTools.Tests/ColorPickerServiceTests.cs`: 7 unit tests covering screen sampling, format conversions, max capacity clamping, and JSON persistence.
- **Integration**:
  - Registered `IColorPickerService`, `ColorPickerViewModel`, and `ColorPickerPage` in `App.xaml.cs`.
  - Added new `NavigationViewItem` to `MainWindow.xaml` (`SymbolRegular.Color24`).
  - Extended `AppIntegrationTests.cs` to verify DI resolution (total 22 test cases).
- **Verification**: Executed `dotnet test`. All 53/53 tests passed (100%).

---

### [2026-09-25] Feature Implementation: Text & Dev Tools
- **Action**: Designed and developed Swiss Army knife utility suite for text processing, code formatting, hashing, and conversions.
- **Components Created**:
  - `MyDeusTools.App/Services/Impl/ITextUtilityService.cs`: Interface with JSON operations, Base64 operations, URL/HTML escaping, Hash calculations, Case conversions, and TextStatistics analysis.
  - `MyDeusTools.App/Services/TextUtilityService.cs`: High-performance managed implementation using System.Text.Json, System.Security.Cryptography, System.Net.WebUtility, and regex tokenizers.
  - `MyDeusTools.App/ViewModels/TextUtilityViewModel.cs`: Reactive state management with 5 operational modules (JSON Studio, Base64, URL/HTML, Hash Generator, Case & Inspector).
  - `MyDeusTools.App/Views/Pages/TextUtilityPage.xaml` & `.xaml.cs`: Tabbed Fluent interface with dual editor columns, monospace Consolas textboxes, instant copy actions, and real-time statistic metric cards.
  - `MyDeusTools.Tests/TextUtilityServiceTests.cs`: 14 comprehensive unit tests verifying formatting, minification, syntax validation, Base64/file roundtrips, standard hashes, and case conversions.
- **Integration**:
  - Registered `ITextUtilityService`, `TextUtilityViewModel`, and `TextUtilityPage` in `App.xaml.cs`.
  - Added new `NavigationViewItem` to `MainWindow.xaml` (`SymbolRegular.Code24`).
  - Extended `AppIntegrationTests.cs` to verify DI resolution (total 25 test cases).
  - Fixed `ColorPickerService.cs` asynchronous file saving by introducing `SemaphoreSlim` to eliminate file write collisions during rapid state mutations.
- **Verification**: Executed `dotnet test`. All 70/70 tests passed (100%).

---

### [2026-09-25] Feature Implementation: Bilingual Localization (EN & VIE)
- **Action**: Implemented dynamic runtime language switching between Vietnamese (`vi-VN`) and English (`en-US`).
- **Components Created**:
  - `MyDeusTools.App/Resources/Languages/Strings.vi-VN.xaml` & `Strings.en-US.xaml`: Comprehensive localization string dictionaries covering shell, navigation, buttons, and page titles.
  - `MyDeusTools.App/Services/Impl/ILanguageService.cs`: Interface defining `CurrentLanguage`, `CurrentLanguageCode`, `LanguageChanged`, `SetLanguage`, `ToggleLanguage`, and `GetString`.
  - `MyDeusTools.App/Services/LanguageService.cs`: Manages dynamic replacement of language ResourceDictionary in `Application.Current.Resources.MergedDictionaries` and auto-saves preference to `Data/settings.json`.
  - `MyDeusTools.Tests/LanguageServiceTests.cs`: 6 unit tests verifying default language, switching, toggling, event notification, settings persistence, and fallback lookup.
- **UI Integration**:
  - Added `LanguageMenuItem` to `MainWindow.xaml` in `FooterMenuItems` with globe icon (`SymbolRegular.Globe24`) and dynamic binding `{DynamicResource App_Language_Display}`.
  - Bound navigation menu items and page headers across all pages to `{DynamicResource ...}` to enable zero-restart live translation.
  - Registered `ILanguageService` in `App.xaml.cs` and verified via `AppIntegrationTests.cs` (total 26 test cases).
- **Verification**: Executed `dotnet test`. All 77/77 tests passed (100%).



