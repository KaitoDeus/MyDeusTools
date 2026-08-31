# Windows API & System Interop Rules

## 1. Win32 P/Invoke Declarations
- Use explicit signature declarations with `[DllImport("user32.dll")]`.
- For mouse actions, use `mouse_event` and `SetCursorPos` with bounds validation.
- Always encapsulate low-level Win32 calls inside a C# Service behind an Interface (`IAutoClickService`, `ISystemService`) so unit tests can mock them cleanly.

## 2. Multi-Monitor Coordinate Handling
- When overlaying the screen or recording click coordinates, always use virtual screen bounds:
  - `SystemParameters.VirtualScreenLeft`
  - `SystemParameters.VirtualScreenTop`
  - `SystemParameters.VirtualScreenWidth`
  - `SystemParameters.VirtualScreenHeight`
- Never assume single-monitor coordinates `(0, 0)` to `(PrimaryScreenWidth, PrimaryScreenHeight)`.

## 3. Windows Registry Management
- Access registry keys strictly under `Registry.CurrentUser` (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
- Never perform unconditional write/delete operations without validating key existence and handling `SecurityException` / `UnauthorizedAccessException`.
- Quote executable paths when registering for startup: `\"path\to\app.exe\"`.

## 4. System Power Commands
- Standard Windows command mapping:
  - Shutdown: `shutdown /s /f /t <seconds>`
  - Restart: `shutdown /r /f /t <seconds>`
  - Hibernate: `cmd.exe /c timeout /t <seconds> /nobreak && shutdown /h`
  - Cancel Scheduled Action: `shutdown /a`
- Always suppress console window popups with `ProcessStartInfo.CreateNoWindow = true` and `ProcessStartInfo.UseShellExecute = false`.
