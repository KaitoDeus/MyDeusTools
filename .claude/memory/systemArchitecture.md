# System Architecture - MyDeusTools

This document outlines the architectural layers, Dependency Injection registrations, navigation hierarchy, native interop subsystems, and storage models of **MyDeusTools**.

---

## 1. System Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        PRESENTATION                         │
│  - FluentWindow (Mica Backdrop, Rounded Corners, TitleBar)  │
│  - NavigationView (12 Tool Items + Language + Settings)    │
│  - 12 Views/Pages & 4 Overlays (Record, Snip, Color, Sticky)│
│  - DynamicResource Bilingual Strings (vi-VN / en-US)        │
└──────────────────────────────▲──────────────────────────────┘
                               │ DataBindings & RelayCommands
┌──────────────────────────────▼──────────────────────────────┐
│                         VIEWMODELS                          │
│  - CommunityToolkit.Mvvm (ObservableObject, [ObservableProp])│
│  - 13 ViewModels managing asynchronous tasks & validations  │
│  - ILanguageService integration & Progress<double> reporting│
└──────────────────────────────▲──────────────────────────────┘
                               │ Service Method Calls
┌──────────────────────────────▼──────────────────────────────┐
│                           SERVICES                          │
│  - 15 Managed Domain Services (Singletons in DI)            │
│  - Win32 P/Invoke Subsystems (user32, gdi32, registry)      │
│  - External Engines (PDFsharp, QRCoder, ZXing.Net, FFmpeg)  │
│  - Asynchronous File & JSON Repository Operations           │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Dependency Injection Registration Matrix (`App.xaml.cs`)

### 15 Domain Services (Singletons)
1. `IPageService` -> `PageService` (WPF-UI Navigation page resolution)
2. `INavigationService` -> `NavigationService` (WPF-UI Navigation framework)
3. `IAutoClickService` -> `AutoClickService` (Mouse simulation & waypoint replay)
4. `ISystemService` -> `SystemService` (Windows shutdown/restart/hibernate scheduler)
5. `IStickyNoteService` -> `StickyNoteService` (Notes JSON repository)
6. `IAutoStartService` -> `AutoStartService` (Windows HKCU Run registry manager)
7. `IClipboardService` -> `ClipboardService` (OS clipboard listener & JSON history)
8. `IQrCodeService` -> `QrCodeService` (QR encode & decode)
9. `IColorPickerService` -> `ColorPickerService` (Screen color sampling & palette)
10. `ITextUtilityService` -> `TextUtilityService` (Text transformations, JSON, Base64, Hashes)
11. `ILanguageService` -> `LanguageService` (Dynamic runtime bilingual switching)
12. `IWindowPinnerService` -> `WindowPinnerService` (Always-on-Top & layered opacity)
13. `IVideoConverterService` -> `VideoConverterService` (Media transcode engine)
14. `IImageStudioService` -> `ImageStudioService` (Image compressor & ICO generator)
15. `IPdfToolkitService` -> `PdfToolkitService` (Offline PDF merge, split, image conversion, encryption)
16. `IBulkRenamerService` -> `BulkRenamerService` (Batch file/folder renaming, preview, conflict detection, undo)

### 13 ViewModels (Transient)
- `MainWindowViewModel`, `AutoClickViewModel`, `ShutdownViewModel`, `StickyNoteViewModel`, `ClipboardViewModel`, `QrCodeViewModel`, `ColorPickerViewModel`, `TextUtilityViewModel`, `WindowPinnerViewModel`, `VideoConverterViewModel`, `ImageStudioViewModel`, `PdfToolkitViewModel`, `BulkRenamerViewModel`.

### 12 Pages & Windows (Transient)
- `MainWindow`
- `AutoClickPage`, `ShutdownPage`, `StickyNotePage`, `ClipboardPage`, `QrCodePage`, `ColorPickerPage`, `TextUtilityPage`, `WindowPinnerPage`, `VideoConverterPage`, `ImageStudioPage`, `PdfToolkitPage`, `BulkRenamerPage`.

---

## 3. Subsystem Architecture

### A. Navigation & Shell
- `MainWindow.xaml`: Uses `<ui:NavigationView>` with `PaneDisplayMode="Left"` and `OpenPaneLength="216"`.
- Each menu item specifies `TargetPageType="{x:Type pages:...Page}"`.
- Resolved automatically by `IPageService` via DI.

### B. Overlays & Specialized Windows
1. `RecordingOverlayWindow`: Fullscreen transparent overlay across virtual multi-monitor desktop. Captures clicks and places numbered waypoint markers.
2. `StickyNoteWindow`: Borderless, draggable, resizable floating note window with Topmost toggle.
3. `QrSnippingOverlayWindow`: Interactive screen rectangle selector for QR decoding.
4. `ColorPickerOverlayWindow`: Fullscreen crosshair with floating 11x11 pixel magnifier HUD.

### C. Win32 Native Interop Layer
- **Mouse**: `mouse_event` (down/up events for left, right, middle) and `SetCursorPos`.
- **Clipboard**: `AddClipboardFormatListener` / `RemoveClipboardFormatListener` with `WM_CLIPBOARDUPDATE`.
- **Window Pinning**: `SetWindowPos` with `HWND_TOPMOST` and `HWND_NOTOPMOST`.
- **Transparency**: `SetWindowLongPtr` with `WS_EX_LAYERED` and `SetLayeredWindowAttributes` with `LWA_ALPHA`.
- **GDI Capture**: `GetDC`, `ReleaseDC`, `GetPixel`, `BitBlt` / `CopyFromScreen`.

### D. File Storage Architecture
Stored in `AppDomain.CurrentDomain.BaseDirectory\Data\`:
- `Data/notes.json`
- `Data/clipboard.json`
- `Data/colors.json`
- `Data/settings.json`
