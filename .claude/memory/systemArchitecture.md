# System Architecture - MyDeusTools

## 1. Architectural Style & Design Principles
- **Pattern**: Model-View-ViewModel (MVVM) powered by `CommunityToolkit.Mvvm`.
- **Dependency Injection**: Inversion of Control using `Microsoft.Extensions.DependencyInjection` (configured in `App.xaml.cs`).
- **UI & Presentation**: Windows Presentation Foundation (WPF) with `WPF-UI 3.0.5` utilizing modern Fluent styling, Mica window backdrop, and vector icons (`SymbolIcon`).
- **OS Interop**: Native Win32 API calls via P/Invoke (`user32.dll` for cursor manipulation and mouse event synthesization), Windows Registry (`Microsoft.Win32.Registry`), and native process spawning (`Process.Start`).

---

## 2. Dependency Injection Container Configuration (`App.xaml.cs`)

```csharp
public static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // 1. Core Services (Singletons)
    services.AddSingleton<IPageService, PageService>();
    services.AddSingleton<INavigationService, NavigationService>();
    services.AddSingleton<IAutoClickService, AutoClickService>();
    services.AddSingleton<ISystemService, SystemService>();
    services.AddSingleton<IStickyNoteService, StickyNoteService>();
    services.AddSingleton<IAutoStartService, AutoStartService>();
    services.AddSingleton<IClipboardService, ClipboardService>();
    services.AddSingleton<IQrCodeService, QrCodeService>();
    services.AddSingleton<IColorPickerService, ColorPickerService>();
    services.AddSingleton<ITextUtilityService, TextUtilityService>();

    // 2. ViewModels (Transient)
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<AutoClickViewModel>();
    services.AddTransient<ShutdownViewModel>();
    services.AddTransient<StickyNoteViewModel>();
    services.AddTransient<ClipboardViewModel>();
    services.AddTransient<QrCodeViewModel>();
    services.AddTransient<ColorPickerViewModel>();
    services.AddTransient<TextUtilityViewModel>();

    // 3. Views / Pages (Transient)
    services.AddTransient<MainWindow>();
    services.AddTransient<AutoClickPage>();
    services.AddTransient<ShutdownPage>();
    services.AddTransient<StickyNotePage>();
    services.AddTransient<ClipboardPage>();
    services.AddTransient<QrCodePage>();
    services.AddTransient<ColorPickerPage>();
    services.AddTransient<TextUtilityPage>();

    return services.BuildServiceProvider();
}
```

---

## 3. Layer Breakdown & Component Responsibilities

### 3.1. Views & UI Layer (`MyDeusTools.App/Views/`)
- `MainWindow.xaml / .cs`: Main container with `NavigationView`, title bar, Mica backdrop, and System Tray (`TaskbarIcon`). Manages minimize-to-tray intercept and Windows message hook (`WM_CLIPBOARDUPDATE`).
- `Views/Pages/AutoClickPage.xaml`: AutoClicker UI with tab toggle buttons for Basic Config, Interval/Repeat, and Coordinate Recording.
- `Views/Pages/ShutdownPage.xaml`: System power schedule control, dynamic countdown timer rendering, and action selection.
- `Views/Pages/StickyNotePage.xaml`: Note cards gallery (`WrapPanel`), note editor with Enter/LostFocus triggers, and delete/pin buttons.
- `Views/Pages/ClipboardPage.xaml`: Clipboard manager gallery with search bar, text preview, pin indicators, character count, and copy/delete actions.
- `Views/Pages/QrCodePage.xaml`: QR Code Studio with tabs for generator and multi-source scanner (screen snipping, file picker, clipboard paste).
- `Views/Pages/ColorPickerPage.xaml`: Color Picker & Eyedropper inspector hero card, quick format copy chips, and palette history wrap grid.
- `Views/Pages/TextUtilityPage.xaml`: Text & Dev Tools tabbed workbench (JSON Studio, Base64, URL/HTML, Hashes, Case Transformations & Statistics).
- `Views/Windows/RecordingOverlayWindow.xaml / .cs`: Fullscreen borderless transparent overlay spanning virtual desktop (`VirtualScreenWidth` / `VirtualScreenHeight`) for mouse coordinate selection and visual point marker rendering.
- `Views/Windows/StickyNoteWindow.xaml / .cs`: Frameless draggable desktop widget for individual sticky notes with `Topmost="True"`.
- `Views/Windows/QrSnippingOverlayWindow.xaml / .cs`: Fullscreen crosshair snipping overlay with rubber-band rectangle selection for scanning screen QR codes.
- `Views/Windows/ColorPickerOverlayWindow.xaml / .cs`: Fullscreen crosshair canvas with real-time floating 11x11 pixel Magnifier HUD, center reticle marker, and live swatch.

### 3.2. ViewModels Layer (`MyDeusTools.App/ViewModels/`)
- `AutoClickViewModel`: Bridges UI and `IAutoClickService`. Manages keybind capture listening mode, interval calculations, and waypoint counter state.
- `ShutdownViewModel`: Bridges UI and `ISystemService`. Contains a 1-second `DispatcherTimer` for ticking UI countdown text (`hh:mm:ss`).
- `StickyNoteViewModel`: Exposes `ObservableCollection<StickyNoteModel>` from `IStickyNoteService`, provides commands for adding, deleting, pinning, and auto-saving notes.
- `ClipboardViewModel`: Bridges UI and `IClipboardService`. Provides real-time text search filtering, pin management, copy-back commands, and history clearance.
- `QrCodeViewModel`: Bridges UI and `IQrCodeService`. Manages QR generation, image export, and multi-source scanning.
- `ColorPickerViewModel`: Bridges UI and `IColorPickerService`. Launches eyedropper overlay, auto-copies to clipboard, copies formats, and handles pin/delete palette actions.
- `TextUtilityViewModel`: Bridges UI and `ITextUtilityService`. Manages JSON formatting/minify/validation, Base64 encoding/decoding, URL/HTML conversion, live multi-hash generation, and text inspection.
- `MainWindowViewModel`: Manages navigation commands.

### 3.3. Services & Interop Layer (`MyDeusTools.App/Services/`)
- `TextUtilityService` (`ITextUtilityService`):
  - Pure .NET 8 high-performance text utilities: JSON formatting & validation (`System.Text.Json`), Base64 text/file encoding, URL & HTML entities escaping, cryptographic hashes (`System.Security.Cryptography`), case transformers, and text statistics.
- `ColorPickerService` (`IColorPickerService`):
  - Screen pixel sampling via Win32 `GetDC` / `GetPixel` / `ReleaseDC` and GDI `CopyFromScreen`.
  - Format conversions (HEX, RGB, HSL, HSV), color history deduplication, and JSON persistence (`Data/colors.json`).
- `QrCodeService` (`IQrCodeService`):
  - QR Encoding using `QRCoder` (zero external native dependencies).
  - QR Decoding using `ZXing.Net` across raw RGBLuminanceSource, BitmapSource, and GDI screen regions.
- `ClipboardService` (`IClipboardService`):
  - Win32 API bindings: `AddClipboardFormatListener(hwnd)` and `RemoveClipboardFormatListener(hwnd)`.
  - STA thread-safe reading of clipboard data via `Dispatcher.InvokeAsync`.
  - Intelligent deduplication, pin preservation, maximum capacity limits, and JSON persistence (`Data/clipboard.json`).
- `AutoClickService` (`IAutoClickService`):
  - Win32 API bindings:
    - `mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo)`
    - `GetCursorPos(out POINT lpPoint)`
    - `SetCursorPos(int x, int y)`
  - Internal loop: `DispatcherTimer` triggered at configured intervals.
  - Thread safety: `lock (_lock)` for recorded points manipulation and thread-safe stop event invocation.
- `SystemService` (`ISystemService`):
  - Encapsulates Windows `shutdown.exe` and `cmd.exe` process invocation (`/s`, `/r`, `/h`, `/a`).
- `StickyNoteService` (`IStickyNoteService`):
  - Handles JSON serialization / deserialization of `ObservableCollection<StickyNoteModel>` into `Data/notes.json`.
- `AutoStartService` (`IAutoStartService`):
  - Reads and writes `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` registry key matching `Environment.ProcessPath`.
- `PageService` (`IPageService`):
  - Resolves WPF-UI Page types via DI container.

---

## 4. Data Flow & Event Pipelines

### 4.1. Auto Clicker Execution Flow
```
User (UI / Hotkey) 
    ──> AutoClickViewModel.ToggleClick()
        ──> IAutoClickService.Start(...)
            ──> DispatcherTimer (Interval ms)
                ──> [If Waypoints] SetCursorPos(point.X, point.Y)
                ──> mouse_event(DOWN | UP)
                ──> [If DoubleClick] Task.Delay(10) -> mouse_event(DOWN | UP)
                ──> [If Reached RepeatCount] Stop() -> Stopped Action
```

### 4.2. Sticky Note Lifecycle Flow
```
User Actions (Add / Edit / Delete)
    ──> StickyNoteViewModel (RelayCommand)
        ──> StickyNoteService (_notes collection mutated)
            ──> SaveNotesAsync() (JSON serialization -> Data/notes.json)
[Pin Action]
    ──> StickyNoteWindow instantiated & shown with Topmost=true
    ──> Note closed -> IsPinned=false -> SaveNotesAsync()
```

### 4.3. Navigation & App Lifecycle
```
App.OnStartup 
    ──> DI Container resolves MainWindow (injecting MainWindowViewModel, NavigationService, AutoStartService)
    ──> NavigationService sets RootNavigation control
    ──> Tray Icon initialized
    ──> Window Close intercepted: hides window unless explicit Exit triggered from Tray Menu.
```
