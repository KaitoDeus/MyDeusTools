# Windows API & System Interop Rules (`MyDeusTools.Agent` & `MyDeusTools.App`)

## 1. Win32 P/Invoke & Mockability
- Declare explicit `user32.dll` / `gdi32.dll` P/Invoke signatures inside dedicated services or `NativeMethods` helpers.
- Encapsulate native calls behind clean interfaces (`IMouseInterop` in `MyDeusTools.Agent`; `IAutoClickService`, `ISystemService`, `IWindowPinnerService` in `MyDeusTools.App`) so automated tests can mock hardware/OS dependencies cleanly.
- Safety clamp click intervals to `>= 10ms` (`OsAutoClickEngine.MinIntervalMs = 10`).

## 2. Global Hotkeys (`MyDeusTools.Agent` & `MyDeusTools.App`)
- **Agent (`GlobalHotkeyService.cs`)**: Uses edge-triggered `GetAsyncKeyState` polling every `40ms` (`VK_F6 = 0x75` Start/Stop toggle, `VK_F7 = 0x76` cursor capture) so hotkeys work reliably inside desktop apps and games without message-loop window dependencies.
- **WPF App**: Uses `NHotkey.Wpf.HotkeyManager`, ignoring bare modifier keys and `Escape`.

## 3. Loopback HTTP Bridge Security (`LocalHttpBridgeServer.cs`)
- Bind `HttpListener` **strictly** to `http://127.0.0.1:41950/` (loopback only, zero LAN exposure).
- Always attach CORS headers (`Access-Control-Allow-Origin: *`, `Access-Control-Allow-Methods: GET, POST, OPTIONS`) and echo `Access-Control-Allow-Private-Network: true` when `Access-Control-Request-Private-Network` is present.

## 4. Window Management & Layered Opacity (`OsWindowPinnerService` & `WindowPinnerService`)
- Filter out shell windows (`GetShellWindow`), invisible windows (`!IsWindowVisible`), empty titles, `WS_EX_TOOLWINDOW` windows, and self PID.
- Use `IntPtr.Size == 8 ? GetWindowLongPtr64 : GetWindowLongPtr32` (and `SetWindowLongPtr`) for 32/64-bit safety.
- Pin Always-on-Top via `SetWindowPos(HWND_TOPMOST / HWND_NOTOPMOST, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW)`.
- Adjust opacity (clamped `20-100%`) by adding `WS_EX_LAYERED` (`0x00080000`) and calling `SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA)`.

## 5. System Power Commands (`OsSystemPowerService` & `SystemService`)
- Mapping:
  - Shutdown: `shutdown /s /f /t <seconds>`
  - Restart: `shutdown /r /f /t <seconds>`
  - Hibernate: `cmd.exe /c timeout /t <seconds> /nobreak && shutdown /h` (tracking background `Process` to kill the process tree on `Cancel()`)
  - Cancel Scheduled Action: `shutdown /a`
- Always set `CreateNoWindow = true` and `UseShellExecute = false` to suppress console windows.

## 6. Multi-Monitor Coordinates & Registry (WPF App)
- Always use `SystemParameters.VirtualScreen*` (`Left`, `Top`, `Width`, `Height`) for fullscreen overlays (`RecordingOverlayWindow`, `QrSnippingOverlayWindow`, `ColorPickerOverlayWindow`).
- Access startup registry keys strictly under `Registry.CurrentUser\Software\Microsoft\Windows\CurrentVersion\Run` with quoted executable paths.
