---
name: win32-interop
description: >-
  Win32 P/Invoke and native desktop interop guidelines for MyDeusTools.Agent and MyDeusTools.App.
  Covers User32 mouse simulation, global hotkeys, clipboard format listeners, window Z-order,
  layered opacity, GDI pixel capture, Registry autostart, and loopback PNA bridge rules.
---

# Win32 Interop & P/Invoke Guidelines (`Agent` & `App`)

Documents native Windows API conventions, loopback HTTP bridge safety, and testable P/Invoke patterns across **`MyDeusTools.Agent`** and **`MyDeusTools.App`**.

---

## 1. Mockable P/Invoke Signatures

Encapsulate native signatures inside dedicated services or internal `NativeMethods` classes. In `MyDeusTools.Agent`, abstract mouse synthesis behind `IMouseInterop` (`Win32MouseInterop`) so `OsAutoClickEngine` can be unit tested deterministically with Moq:

```csharp
internal static class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy,
        uint uFlags);

    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public static readonly IntPtr HWND_NOTOPMOST = new(-2);
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_SHOWWINDOW = 0x0040;
}
```

---

## 2. Mouse Synthesis & Edge-Triggered Hotkeys (`MyDeusTools.Agent`)

- **Mouse Events (`Win32MouseInterop`)**:
  - `mouse_event` with `MOUSEEVENTF_LEFTDOWN/UP`, `RIGHTDOWN/UP`, `MIDDLEDOWN/UP`.
  - `GetCursorPos` / `SetCursorPos` for cursor capture and waypoint cycling.
  - Enforce `OsAutoClickEngine.MinIntervalMs = 10` minimum clamp.
- **Global Hotkeys (`GlobalHotkeyService`)**:
  - Poll `GetAsyncKeyState(vKey)` every `40ms` (`VK_F6 = 0x75`, `VK_F7 = 0x76`) using edge detection (`isDown && !wasDown`) so hotkeys function inside fullscreen apps and games.
- **Loopback HTTP Bridge (`LocalHttpBridgeServer`)**:
  - Bind strictly to `http://127.0.0.1:41950/` and set `Access-Control-Allow-Private-Network: true` alongside standard CORS headers.

---

## 3. Multi-Monitor Coordinate Translation (WPF Overlays)

Always account for negative virtual screen coordinates on secondary displays:
- Use `SystemParameters.VirtualScreenLeft`, `VirtualScreenTop`, `VirtualScreenWidth`, and `VirtualScreenHeight`.
- Scale WPF DIPs to physical pixels before GDI `GetPixel` / `CopyFromScreen`.

---

## 4. Clipboard Listener Re-Entrancy (WPF App)

- Wrap listener window in `HwndSource` and handle `WM_CLIPBOARDUPDATE` (`0x031D`) via `AddClipboardFormatListener` / `RemoveClipboardFormatListener`.
- Guard self-originated clipboard writes using `_isInternalCopy` before copying.

---

## 5. Layered Windows & Opacity (`OsWindowPinnerService` & `WindowPinnerService`)

1. Use 32/64-bit safe `GetWindowLongPtr` / `SetWindowLongPtr`.
2. Add `WS_EX_LAYERED` (`0x00080000`) to `GWL_EXSTYLE`.
3. Call `SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA)` with clamped percent (`20-100%` -> `alpha = Math.Round(percent / 100.0 * 255)`).
4. Filter out `GetShellWindow()`, invisible windows, `WS_EX_TOOLWINDOW`, empty titles, and current PID when enumerating.

---

## 6. System Power & Registry Autostart Safety

- Power CLI (`OsSystemPowerService` & `SystemService`): Run hidden (`CreateNoWindow = true`, `UseShellExecute = false`) with `shutdown /s /f /t`, `/r /f /t`, `cmd.exe /c timeout /t ... && shutdown /h`, and `shutdown /a`.
- Registry Autostart (WPF App): Strictly target `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run` with quoted executable paths (`\"path\to\app.exe\"`) so Administrator elevation is never required.
