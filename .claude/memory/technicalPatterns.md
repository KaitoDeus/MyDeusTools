# Technical Patterns & Conventions - MyDeusTools

## 1. MVVM & Code Patterns (CommunityToolkit.Mvvm)

### 1.1. Observable Objects & Properties
- ViewModels inherit from `ObservableObject` or `ObservableRecipient`.
- Use `SetProperty(ref _field, value)` or the source generator `[ObservableProperty] private string _name;`.
- Avoid putting UI controls directly in ViewModels; use data binding, commands, and interfaces.

### 1.2. Relay Commands
- Implement commands using `[RelayCommand]` or `new RelayCommand(...)`.
- For asynchronous commands, return `Task` and use `[RelayCommand]` which provides `IAsyncRelayCommand` with `IsRunning` state support.

### 1.3. UI Dispatching & Thread Safety
- Any event originating from background threads, OS hooks, or timers updating UI-bound properties must be dispatched:
```csharp
System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
{
    StatusText = "Updated state";
});
```

---

## 2. WPF-UI & XAML Styling Patterns

### 2.1. Styling & Theming
- Use dynamic resources for colors and brushes so the app adapts seamlessly to Dark and Light modes:
  - `Foreground="{DynamicResource TextFillColorPrimaryBrush}"`
  - `Foreground="{DynamicResource TextFillColorSecondaryBrush}"`
  - `Background="{DynamicResource CardBackgroundFillColorSecondaryBrush}"`
  - `Foreground="{DynamicResource SystemFillColorCriticalBrush}"`
- Use WPF-UI controls: `<ui:Button>`, `<ui:Card>`, `<ui:NumberBox>`, `<ui:SymbolIcon>`, `<ui:InfoBar>`, `<ui:FluentWindow>`.

### 2.2. Navigation Control
- Main navigation is powered by `<ui:NavigationView x:Name="RootNavigation">` with items bound to page types:
```xml
<ui:NavigationViewItem Content="Auto Clicker" TargetPageType="{x:Type pages:AutoClickPage}">
    <ui:NavigationViewItem.Icon>
        <ui:SymbolIcon Symbol="CursorClick24" />
    </ui:NavigationViewItem.Icon>
</ui:NavigationViewItem>
```

---

## 3. Windows Interop & Low-Level API Patterns

### 3.1. P/Invoke Conventions
- Declare P/Invoke methods with standard attributes:
```csharp
[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
```
- Wrap unmanaged calls inside clean C# service interfaces to allow proper mocking in unit tests.

### 3.2. Registry Access
- Always use `try-catch` when accessing `Microsoft.Win32.Registry` to prevent crashes when running under non-elevated or restricted environments.
- Use explicit open/close with `using var key = ...`.

### 3.3. Command Execution
- Set `CreateNoWindow = true` and `UseShellExecute = false` on `ProcessStartInfo` when invoking system utilities like `shutdown.exe`.

---

## 4. Testing Standards (xUnit & Moq & Xunit.StaFact)
- **Framework**: `xunit` 2.9.2 with `Moq` 4.20.72.
- **WPF / UI Thread Tests**: Use `[WpfFact]` or `[WpfTheory]` from `Xunit.StaFact` when testing classes that interact with WPF Dispatcher or Dependency Objects.
- **Service Isolation**: Ensure services have constructor dependency injection or optional parameters to enable deterministic testing without side effects on the host machine.
