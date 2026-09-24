# MyDeusTools - Claude Developer & AI Assistant Guide

## Project Overview
**MyDeusTools** is a modern Windows desktop utility Super-App built with **C# .NET 8 (WPF)**, utilizing **WPF-UI 3.0** (Fluent Design & Mica backdrop), **CommunityToolkit.Mvvm**, and Microsoft Extensions Dependency Injection.

### Core Features
1. **Auto Clicker Pro**:
   - Customizable mouse buttons (Left, Right, Middle) and click types (Single, Double).
   - Flexible interval settings (Hours, Minutes, Seconds, Milliseconds - min 10ms).
   - Global Hotkey listening & assignment (via `NHotkey.Wpf`).
   - Coordinate recording overlay (`RecordingOverlayWindow`) with visual numbered markers for waypoint replay.
2. **Schedule Shutdown (Hẹn giờ hệ thống)**:
   - Supports Actions: `Shutdown` (`/s`), `Restart` (`/r`), `Hibernate` (`/h`).
   - Countdown timer with live display and cancellation (`shutdown /a`).
3. **Sticky Notes (Ghi chú nổi)**:
   - Create, edit, auto-save, delete notes with JSON persistence (`Data/notes.json`).
   - Pop-out sticky note window (`StickyNoteWindow`) with borderless draggable interface.
4. **Clipboard Manager (Lịch sử khay nhớ tạm)**:
   - Real-time Windows clipboard listener via Win32 `AddClipboardFormatListener` / `WM_CLIPBOARDUPDATE`.
   - Search filter, pin items to top, delete individual items, clear unpinned items, auto-save JSON (`Data/clipboard.json`).
5. **QR Code Studio (Tạo & Quét mã QR)**:
   - High-definition QR generation (QRCoder), export PNG or copy image to clipboard.
   - Screen Snipping QR Scanner (`QrSnippingOverlayWindow`), file image scanner, and clipboard image scanner via `ZXing.Net`.
6. **Color Picker & Eyedropper (Chấm & lấy màu màn hình)**:
   - Fullscreen crosshair eyedropper with floating 11x11 pixel Magnifier HUD (`ColorPickerOverlayWindow`).
   - Instant format copying (HEX, RGB, HSL, HSV) and auto-copy to clipboard on pick.
   - Persistent color palette history with pin-to-top support (`Data/colors.json`).
7. **System Tray & Lifecycle**:
   - Minimize-to-tray on close (`Hardcodet.NotifyIcon.Wpf`).
   - Dark / Light mode switching dynamically via `Wpf.Ui.Appearance.ApplicationThemeManager`.
   - Windows Startup toggle via CurrentUser Registry (`Software\Microsoft\Windows\CurrentVersion\Run`).

---

## Tech Stack & Dependencies
- **Target Framework**: `net8.0-windows` (x64)
- **UI Framework**: WPF with `WPF-UI` (v3.0.5)
- **MVVM Toolkit**: `CommunityToolkit.Mvvm` (v8.4.2)
- **DI Container**: `Microsoft.Extensions.DependencyInjection` (v10.0.7)
- **Global Hotkeys**: `NHotkey.Wpf` (v4.0.0)
- **Tray Icon**: `Hardcodet.NotifyIcon.Wpf` (v2.0.1)
- **QR Generation & Decoding**: `QRCoder` (v1.8.0), `ZXing.Net` (v0.16.11)
- **Testing**: `xunit` (v2.9.2), `Moq` (v4.20.72), `Xunit.StaFact` (v0.3.18), `coverlet.collector` (v6.0.2)

---

## Architecture & Project Structure

```
d:\.MyDeusTools\MyDeusTools\
├── .claude/                     # Claude AI configuration, Memory Bank & Rules
│   ├── MEMORY.md                # Memory Bank Master Index
│   ├── memory/                  # Detailed Memory Modules
│   │   ├── activeContext.md     # Current focus & active tasks
│   │   ├── projectBrief.md      # Project overview & goals
│   │   ├── systemArchitecture.md# High-level & low-level architecture
│   │   ├── technicalPatterns.md # Coding guidelines, MVVM, Dispatcher rules
│   │   ├── progress.md          # Completed features & roadmap
│   │   └── history.md           # Operation & change history log
│   └── rules/                   # Specific guidance rules
│       ├── wpf-standards.md     # WPF / XAML / MVVM rules
│       └── windows-api.md       # Win32 / PInvoke / Registry rules
├── CLAUDE.md                    # Claude root instructions (this file)
├── MyDeusTools.App/             # Main Application project
│   ├── App.xaml / App.xaml.cs   # Application entry point & DI configuration
│   ├── MainWindow.xaml (.cs)    # Main FluentWindow, NavigationView & System Tray
│   ├── Services/                # Business logic & hardware/OS interaction
│   │   ├── PageService.cs       # Page resolution for WPF-UI Navigation
│   │   ├── StickyNoteService.cs # Sticky notes JSON repository
│   │   ├── AutoClickService.cs  # Mouse simulation (Win32 mouse_event, SetCursorPos)
│   │   ├── AutoStartService.cs  # Windows Registry Run Key manager
│   │   ├── ClipboardService.cs  # Windows Clipboard listener & JSON storage
│   │   ├── QrCodeService.cs     # QR Code generation (QRCoder) & decoding (ZXing)
│   │   ├── ColorPickerService.cs# Screen color sampling, pixel magnifier & history
│   │   └── Impl/                # Service Interfaces (IAutoClickService, IColorPickerService, etc.)
│   ├── ViewModels/              # MVVM ViewModels (CommunityToolkit.Mvvm)
│   │   ├── MainWindowViewModel.cs
│   │   ├── AutoClickViewModel.cs
│   │   ├── ShutdownViewModel.cs
│   │   ├── StickyNoteViewModel.cs
│   │   ├── ClipboardViewModel.cs
│   │   ├── QrCodeViewModel.cs
│   │   └── ColorPickerViewModel.cs
│   ├── Views/                   # UI Pages & Sub-Windows
│   │   ├── Pages/               # AutoClickPage, ShutdownPage, StickyNotePage, ClipboardPage, QrCodePage, ColorPickerPage
│   │   └── Windows/             # RecordingOverlayWindow, StickyNoteWindow, QrSnippingOverlayWindow, ColorPickerOverlayWindow
│   └── Resources/               # Icons & static assets (avatar.ico)
└── MyDeusTools.Tests/           # Unit & Integration Tests (xUnit, StaFact)
    ├── AppIntegrationTests.cs   # DI Resolution verification
    ├── AutoClickServiceTests.cs # AutoClicker business logic & clamp tests
    ├── AutoStartServiceTests.cs # Registry toggle safety tests
    ├── StickyNoteServiceTests.cs# Sticky note persistence tests
    ├── ClipboardServiceTests.cs # Clipboard history, pin, search & deduplication tests
    ├── QrCodeServiceTests.cs    # QR code generation, roundtrip decode, and URL detection tests
    └── ColorPickerServiceTests.cs # Screen sampling, format conversions, and JSON persistence tests
```

---

## Essential Commands

### Build & Run
```powershell
# Build solution
dotnet build

# Run application
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj

# Run Tests
dotnet test

# Run Specific Test Project with detailed logging
dotnet test MyDeusTools.Tests/MyDeusTools.Tests.csproj --logger "console;verbosity=detailed"
```

### Publish Single File Executable
The project is configured for self-contained, single-file win-x64 deployment:
```powershell
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```
Output directory: `MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/`

---

## Key Development Rules & Conventions
1. **MVVM Pattern**:
   - Use `[ObservableProperty]` and `[RelayCommand]` from `CommunityToolkit.Mvvm`.
   - Never write business logic directly inside code-behind `.xaml.cs` (use code-behind only for pure UI logic like overlays or mouse-drag events).
2. **Dependency Injection**:
   - All services must be registered in `App.xaml.cs` (`ConfigureServices()`).
   - Use `Singleton` for stateful services (`AutoClickService`, `StickyNoteService`, `AutoStartService`, `SystemService`, `PageService`, `NavigationService`).
   - Use `Transient` for ViewModels and Views.
3. **UI Thread Safety**:
   - Any background timer callback or asynchronous task updating UI-bound properties must marshal updates via `Application.Current.Dispatcher.Invoke()`.
4. **Memory Bank Maintenance**:
   - Whenever completing a feature, refactoring code, or updating dependencies, update the corresponding files in `.claude/memory/` and log the actions in `history.md`.
