# WPF & XAML Development Standards

## 1. MVVM Design Principles
- Always separate View (XAML), ViewModel (`CommunityToolkit.Mvvm`), and Services/Models.
- UI elements must bind to ViewModel properties. Never handle business logic directly in code-behind event handlers.
- Code-behind (`.xaml.cs`) is reserved solely for pure visual tasks (e.g. `DragMove()`, Canvas drawing, window coordinate calculations).

## 2. Dynamic Theming & Styling
- Always use `DynamicResource` with WPF-UI theme tokens rather than hardcoded hex colors or `StaticResource` for theming:
  - Text: `TextFillColorPrimaryBrush`, `TextFillColorSecondaryBrush`, `TextFillColorDisabledBrush`
  - Cards & Backgrounds: `CardBackgroundFillColorDefaultBrush`, `CardBackgroundFillColorSecondaryBrush`
  - Controls: `ControlFillColorDefaultBrush`, `ControlFillColorSecondaryBrush`
  - Accent & Alerts: `AccentTextFillColorPrimaryBrush`, `SystemFillColorCriticalBrush`
- Windows should inherit from `<ui:FluentWindow>` with `WindowBackdropType="Mica"`, `WindowCornerPreference="Round"`, and `ExtendsContentIntoTitleBar="True"`.

## 3. Asynchronous Operations & Dispatcher
- Long-running work or I/O must run on background threads using `Task.Run` or async/await.
- Any property binding updates triggered from background events or timers MUST be dispatched to the UI thread via `Application.Current.Dispatcher.Invoke()`.

## 4. Input & Hotkey Handling
- Use `NHotkey.Wpf.HotkeyManager` for global keyboard shortcuts to ensure consistent behavior when the app is minimized or in the background.
- Handle Key/Modifier capture carefully: exclude pure modifier presses (`LeftCtrl`, `Shift`, etc.) and `Escape` to ensure clean cancellation.
