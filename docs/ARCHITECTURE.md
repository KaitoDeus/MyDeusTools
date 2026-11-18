# MyDeusTools - Architecture & Technical Design

This document details the software architecture, design patterns, technology stack, dependency injection matrix, threading model, and native interop layers of **MyDeusTools**.

---

## 1. High-Level Architecture

MyDeusTools adheres to the **Model-View-ViewModel (MVVM)** architectural pattern, structured into three distinct layers:

```
┌─────────────────────────────────────────────────────────────┐
│                        VIEW LAYER                           │
│  MainWindow (FluentWindow, Mica, NavigationView, Tray)      │
│  Views/Pages (12 Pages) & Views/Windows (Overlays/Popups)   │
│  XAML DataBindings, DynamicResource (i18n), EventHandlers  │
└──────────────────────────────▲──────────────────────────────┘
                               │ Data Binding & RelayCommands
┌──────────────────────────────▼──────────────────────────────┐
│                     VIEWMODEL LAYER                         │
│  CommunityToolkit.Mvvm (ObservableObject, ObservableProperty)│
│  13 ViewModels managing UI state, validation & async tasks  │
│  ILanguageService integration & Progress<double> reporting │
└──────────────────────────────▲──────────────────────────────┘
                               │ Method Calls & CancellationToken
┌──────────────────────────────▼──────────────────────────────┐
│                      SERVICE LAYER                          │
│  15 Singleton & Transient Domain Services                   │
│  Win32 P/Invoke, GDI Screen Capture, File I/O, Cryptography │
│  PDFsharp, QRCoder, ZXing.Net, FFmpeg Transcoding           │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Dependency Injection Container

Configured in `App.xaml.cs` via `Microsoft.Extensions.DependencyInjection`.

### Service Registrations

| Service Interface | Implementation | Lifetime | Responsibility |
|---|---|---|---|
| `IPageService` | `PageService` | Singleton | Resolves Page Types for WPF-UI NavigationView. |
| `INavigationService` | `NavigationService` | Singleton | Handles view navigation and history within the Shell. |
| `IAutoClickService` | `AutoClickService` | Singleton | High-frequency mouse click automation & waypoint replay. |
| `ISystemService` | `SystemService` | Singleton | Windows shutdown/restart/hibernate scheduler. |
| `IStickyNoteService` | `StickyNoteService` | Singleton | Desktop sticky notes repository and persistence. |
| `IAutoStartService` | `AutoStartService` | Singleton | Windows CurrentUser Run registry key manager. |
| `IClipboardService` | `ClipboardService` | Singleton | OS clipboard listener & history storage. |
| `IQrCodeService` | `QrCodeService` | Singleton | QR generation and multi-source decoding. |
| `IColorPickerService` | `ColorPickerService` | Singleton | Screen color sampling, magnifier & palette storage. |
| `ITextUtilityService` | `TextUtilityService` | Singleton | JSON, Base64, URL/HTML, Hashes, and Case conversion. |
| `ILanguageService` | `LanguageService` | Singleton | Dynamic bilingual switching (vi-VN / en-US) & settings. |
| `IWindowPinnerService` | `WindowPinnerService` | Singleton | Desktop window Always-on-Top & opacity controller. |
| `IVideoConverterService` | `VideoConverterService` | Singleton | Multimedia transcode, scale, and format converter. |
| `IImageStudioService` | `ImageStudioService` | Singleton | Batch image compression, resize, and ICO builder. |
| `IPdfToolkitService` | `PdfToolkitService` | Singleton | Offline PDF merge, split, images-to-PDF, encryption. |
| `IBulkRenamerService` | `BulkRenamerService` | Singleton | Batch file/folder renaming, preview, conflicts, undo. |

### ViewModel Registrations (Transient)

- `MainWindowViewModel`: Manages navigation state, current language display, and tray events.
- `AutoClickViewModel`: Mouse button, click type, interval, coordinates, and hotkey state.
- `ShutdownViewModel`: Shutdown mode, normalized hours/minutes/seconds, countdown display.
- `StickyNoteViewModel`: Note collections, active notes, and popup window launches.
- `ClipboardViewModel`: Clipboard item collections, search filters, and pin toggles.
- `QrCodeViewModel`: QR generation text/image, decode results, and snipping launcher.
- `ColorPickerViewModel`: Current color, magnifier HUD properties, and palette collections.
- `TextUtilityViewModel`: JSON, Base64, Hashes, and text metrics bindings.
- `WindowPinnerViewModel`: Desktop window enumeration, active window pinning, and opacity sliders.
- `VideoConverterViewModel`: Video file list, output configurations, and transcode queue.
- `ImageStudioViewModel`: Image queue, format targets, quality sliders, and savings stats.
- `PdfToolkitViewModel`: Merge files, split ranges, images-to-PDF options, and password encryption.
- `BulkRenamerViewModel`: Renamer rule parameters, item previews, conflict validation, and undo history.

---

## 3. Native Interoperability & Win32 APIs

MyDeusTools leverages native Windows APIs via safe P/Invoke wrappers:

1. **Mouse Simulation (`user32.dll`)**:
   - `mouse_event`: Synthesizes left, right, and middle button mouse down/up events.
   - `SetCursorPos`: Repositions cursor to absolute virtual desktop coordinates during waypoint replay.
2. **Clipboard Listener (`user32.dll`)**:
   - `AddClipboardFormatListener(IntPtr hwnd)`: Subscribes the main window to `WM_CLIPBOARDUPDATE` messages.
   - `RemoveClipboardFormatListener(IntPtr hwnd)`: Unsubscribes on application shutdown.
3. **Window Management & Layered Opacity (`user32.dll`)**:
   - `EnumWindows`: Enumerates top-level desktop windows.
   - `GetWindowText` / `GetWindowTextLength`: Retrieves window titles.
   - `IsWindowVisible`: Filters out hidden/background handles.
   - `SetWindowPos`: Modifies window Z-order (`HWND_TOPMOST` / `HWND_NOTOPMOST`).
   - `GetWindowLongPtr` / `SetWindowLongPtr`: Adds the `WS_EX_LAYERED` (0x80000) extended window style.
   - `SetLayeredWindowAttributes`: Adjusts alpha transparency (`LWA_ALPHA` 0x2) from 0 to 255.
   - `SetForegroundWindow`: Focuses and activates target windows.
4. **Screen Pixel Sampling & GDI (`gdi32.dll` & `user32.dll`)**:
   - `GetDC(IntPtr.Zero)` / `ReleaseDC`: Obtains screen device context.
   - `GetPixel`: Retrieves single-pixel RGB values for magnifier HUD.
   - `BitBlt` / `CopyFromScreen`: Captures screen rectangles for eyedropper and QR snipping.
5. **Windows Registry Autostart (`Microsoft.Win32`)**:
   - Targets `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`.
   - Adds or removes the executable path to launch on Windows login without requiring Administrator privileges.

---

## 4. Bilingual Localization Architecture

- **Dictionary Files**:
  - `MyDeusTools.App/Resources/Languages/Strings.vi-VN.xaml` (Vietnamese default)
  - `MyDeusTools.App/Resources/Languages/Strings.en-US.xaml` (English)
- **Dynamic Resource Binding**:
  - UI labels and strings are bound via `{DynamicResource KeyName}` rather than `{StaticResource}`.
  - When the user toggles the language, `LanguageService` dynamically replaces the language dictionary in `Application.Current.Resources.MergedDictionaries`.
  - All UI elements update instantaneously across all opened pages and overlays with zero restart.
- **Persistence**:
  - User language preference is stored in `Data/settings.json` and restored on application startup.

---

## 5. Data Persistence & Storage Architecture

Persistent files are stored in `AppDomain.CurrentDomain.BaseDirectory\Data\`:

| File | Content | Format | Concurrency Guard |
|---|---|---|---|
| `Data/notes.json` | Sticky notes content, pinned state, timestamps | JSON | Async FileStream with try-catch |
| `Data/clipboard.json` | Clipboard text history and pinned status | JSON | Lock / SemaphoreSlim protection |
| `Data/colors.json` | Saved color palette history | JSON | SemaphoreSlim protection |
| `Data/settings.json` | Language code and application preferences | JSON | Direct write with atomic replace |
