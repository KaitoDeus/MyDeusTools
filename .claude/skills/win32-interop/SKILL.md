---
name: win32-interop
description: >-
  Win32 P/Invoke and native desktop interop guidelines for MyDeusTools.
  Covers User32 mouse simulation, clipboard format listeners, window Z-order, layered opacity, GDI pixel capture, and Registry autostart.
---

# Win32 Interop & P/Invoke Guidelines

This skill documents native Windows API conventions, safe P/Invoke patterns, and OS-level hooks used throughout MyDeusTools.

---

## 1. Safe P/Invoke Signatures

All native signatures must be encapsulated within dedicated internal static classes (e.g., `NativeMethods` or inside the respective Service):

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

    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_SHOWWINDOW = 0x0040;
}
```

---

## 2. Multi-Monitor Coordinate Translation

Always account for multi-monitor setups where secondary monitors may have negative coordinates:
- Use `SystemParameters.VirtualScreenLeft` and `SystemParameters.VirtualScreenTop`.
- Span overlays across `SystemParameters.VirtualScreenWidth` and `SystemParameters.VirtualScreenHeight`.
- Convert DPI/WPF DIPs (Device Independent Pixels) to physical pixels when invoking GDI `GetPixel` or `CopyFromScreen`.

---

## 3. Clipboard Listener Re-Entrancy

When implementing `AddClipboardFormatListener`:
- Wrap the listener window in `HwndSource`.
- In the `WndProc` hook, handle `WM_CLIPBOARDUPDATE` (0x031D).
- Maintain an `_isInternalCopy` boolean flag before copying from inside MyDeusTools to prevent circular triggering.
- Always unhook with `RemoveClipboardFormatListener` during window disposal or service shutdown.

---

## 4. Layered Windows & Window Opacity

To adjust third-party window transparency:
1. Fetch current extended style:
   ```csharp
   int exStyle = (int)GetWindowLongPtr(hWnd, GWL_EXSTYLE);
   ```
2. Enable `WS_EX_LAYERED` (0x00080000):
   ```csharp
   SetWindowLongPtr(hWnd, GWL_EXSTYLE, (IntPtr)(exStyle | WS_EX_LAYERED));
   ```
3. Set alpha channel:
   ```csharp
   byte alpha = (byte)(opacityPercent * 255 / 100);
   SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
   ```

---

## 5. Registry Autostart Safety

1. Never target `HKEY_LOCAL_MACHINE` which requires elevated Administrator privileges.
2. Target `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`.
3. Wrap executable paths in quotes to handle directory names containing spaces:
   `"\"" + Process.GetCurrentProcess().MainModule?.FileName + "\""`.
