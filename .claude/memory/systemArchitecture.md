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

    // 2. ViewModels (Transient)
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<AutoClickViewModel>();
    services.AddTransient<ShutdownViewModel>();
    services.AddTransient<StickyNoteViewModel>();

    // 3. Views / Pages (Transient)
    services.AddTransient<MainWindow>();
    services.AddTransient<AutoClickPage>();
    services.AddTransient<ShutdownPage>();
    services.AddTransient<StickyNotePage>();

    return services.BuildServiceProvider();
}
```

---

## 3. Layer Breakdown & Component Responsibilities

### 3.1. Views & UI Layer (`MyDeusTools.App/Views/`)
- `MainWindow.xaml / .cs`: Main container with `NavigationView`, title bar, Mica backdrop, and System Tray (`TaskbarIcon`). Manages minimize-to-tray intercept (`Closing` event handler).
- `Views/Pages/AutoClickPage.xaml`: AutoClicker UI with tab toggle buttons for Basic Config, Interval/Repeat, and Coordinate Recording.
- `Views/Pages/ShutdownPage.xaml`: System power schedule control, dynamic countdown timer rendering, and action selection.
- `Views/Pages/StickyNotePage.xaml`: Note cards gallery (`WrapPanel`), note editor with Enter/LostFocus triggers, and delete/pin buttons.
- `Views/Windows/RecordingOverlayWindow.xaml / .cs`: Fullscreen borderless transparent overlay spanning virtual desktop (`VirtualScreenWidth` / `VirtualScreenHeight`) for mouse coordinate selection and visual point marker rendering.
- `Views/Windows/StickyNoteWindow.xaml / .cs`: Frameless draggable desktop widget for individual sticky notes with `Topmost="True"`.

### 3.2. ViewModels Layer (`MyDeusTools.App/ViewModels/`)
- `AutoClickViewModel`: Bridges UI and `IAutoClickService`. Manages keybind capture listening mode, interval calculations, and waypoint counter state.
- `ShutdownViewModel`: Bridges UI and `ISystemService`. Contains a 1-second `DispatcherTimer` for ticking UI countdown text (`hh:mm:ss`).
- `StickyNoteViewModel`: Exposes `ObservableCollection<StickyNoteModel>` from `IStickyNoteService`, provides commands for adding, deleting, pinning, and auto-saving notes.
- `MainWindowViewModel`: Manages navigation commands.

### 3.3. Services & Interop Layer (`MyDeusTools.App/Services/`)
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
