# Project Brief - MyDeusTools

## 1. Executive Summary
**MyDeusTools** is an all-in-one lightweight Windows utility desktop application ("Super-App") built in **C# .NET 8** with **WPF** and **WPF-UI 3.0**. It is engineered to combine essential power-user automation and productivity tools into a single, cohesive, modern Fluent Design application with minimal resource footprint and Single-File zero-dependency distribution.

## 2. Core Missions & Problems Solved
- **Productivity Acceleration**: Provide instant access to daily desktop tools (auto-click automation, system power schedules, floating scratchpad notes) without requiring heavy third-party software.
- **Modern User Experience**: Deliver Windows 11 Fluent aesthetic (Mica backdrop, rounded corners, Dark/Light modes) via `WPF-UI`.
- **System Integration**: Run in the system tray, toggle startup on boot via Windows Registry, and register low-latency global keyboard hotkeys.

## 3. Product Features & Modules

### 3.1. Auto Clicker Pro (`AutoClickPage` / `AutoClickViewModel` / `AutoClickService`)
- **Configurable Mouse Actions**: Left / Right / Middle buttons, Single or Double click.
- **High-Precision Timing**: Configurable delay (Hours, Minutes, Seconds, Milliseconds; minimum 10ms clamp).
- **Execution Limits**: Infinite repeat or fixed repeat count.
- **Global Hotkey Manager**: Dynamic keybind remapping via `NHotkey.Wpf` (default `F6`), supports key combos (e.g. `Ctrl+Alt+F6`).
- **Waypoint / Coordinate Recording**:
  - Full-screen transparent overlay (`RecordingOverlayWindow`) capturing coordinates across multi-monitor setups (`SystemParameters.VirtualScreen*`).
  - Interactive numbered circular badges marking clicked positions on the canvas.
  - Sequenced replay of recorded points during execution.

### 3.2. Schedule Shutdown (`ShutdownPage` / `ShutdownViewModel` / `SystemService`)
- **System Actions**: Power Off (`Shutdown`), Reboot (`Restart`), and `Hibernate`.
- **Timer Settings**: Hour / Minute / Second countdowns.
- **System Level Invocation**: Executes native Windows commands (`shutdown /s /f /t <sec>`, `shutdown /r /f /t <sec>`, `cmd /c timeout ... && shutdown /h`, `shutdown /a`).
- **Real-Time Visual Countdown**: Dynamic format `hh:mm:ss` display with cancel ability.

### 3.3. Sticky Notes (`StickyNotePage` / `StickyNoteViewModel` / `StickyNoteService` / `StickyNoteWindow`)
- **Card-based Sticky Notes**: Masonry/wrap layout in main view.
- **Floating Desktop Pin**: Pop-out individual notes into frameless, draggable, topmost desktop windows (`StickyNoteWindow`).
- **JSON Persistence**: Auto-saves note content, timestamps, and pinned states into local storage (`Data/notes.json`).

### 3.4. App Shell & System Tray Integration (`MainWindow` / `AutoStartService`)
- **System Tray Lifecycle**: Closing the window hides to tray (`_notifyIcon.TrayLeftMouseUp` to restore, Context menu for "Mở ứng dụng" and "Thoát").
- **Windows Boot Toggle**: Registry integration at `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- **Theme Switcher**: Instant switching between Dark and Light mode using `ApplicationThemeManager.Apply(...)`.

## 4. Key Constraints & Requirements
- Target OS: Windows 10/11 x64.
- .NET Runtime: .NET 8.0 Windows Desktop.
- Distribution: Single-file win-x64 executable (`PublishSingleFile=true`, `PublishReadyToRun=true`, `IncludeNativeLibrariesForSelfExtract=true`, `EnableCompressionInSingleFile=true`).
- No external bloat: Keep footprint minimal, start time instantaneous, memory footprint low.
