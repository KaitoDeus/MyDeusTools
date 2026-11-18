using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MyDeusTools.Agent.Services;

public record OsWindowInfoDto(
    long Handle,
    string Title,
    string ProcessName,
    int ProcessId,
    bool IsTopMost,
    int OpacityPercent
);

public sealed class OsWindowPinnerService
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int LWA_ALPHA = 0x00000002;

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int SW_RESTORE = 9;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetLayeredWindowAttributes(
        IntPtr hwnd,
        uint crKey,
        byte bAlpha,
        uint dwFlags
    );

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLayeredWindowAttributes(
        IntPtr hwnd,
        out uint pcrKey,
        out byte pbAlpha,
        out uint pdwFlags
    );

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
        IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLongPtr32(hWnd, nIndex, dwNewLong);

    public List<OsWindowInfoDto> GetOpenWindows()
    {
        var result = new List<OsWindowInfoDto>();
        IntPtr shellWindow = GetShellWindow();
        int currentPid = Process.GetCurrentProcess().Id;

        EnumWindows(
            (hWnd, _) =>
            {
                if (hWnd == shellWindow || !IsWindowVisible(hWnd))
                    return true;

                long exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
                if ((exStyle & WS_EX_TOOLWINDOW) != 0)
                    return true;

                int len = GetWindowTextLength(hWnd);
                if (len <= 0)
                    return true;

                var sb = new StringBuilder(len + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString().Trim();
                if (string.IsNullOrWhiteSpace(title))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid == currentPid)
                    return true;

                string procName = "unknown";
                try
                {
                    procName = Process.GetProcessById((int)pid).ProcessName;
                }
                catch
                {
                    // Access denied for elevated processes
                }

                bool isTopMost = (exStyle & WS_EX_TOPMOST) != 0;
                int opacityPct = GetWindowOpacityPercent(hWnd);

                result.Add(
                    new OsWindowInfoDto(
                        Handle: hWnd.ToInt64(),
                        Title: title,
                        ProcessName: procName,
                        ProcessId: (int)pid,
                        IsTopMost: isTopMost,
                        OpacityPercent: opacityPct
                    )
                );

                return true;
            },
            IntPtr.Zero
        );

        return result;
    }

    public bool SetTopMost(long handleValue, bool topMost)
    {
        IntPtr hwnd = new(handleValue);
        return SetWindowPos(
            hwnd,
            topMost ? HWND_TOPMOST : HWND_NOTOPMOST,
            0,
            0,
            0,
            0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW
        );
    }

    public bool SetOpacityPercent(long handleValue, int percent)
    {
        IntPtr hwnd = new(handleValue);
        int clamped = Math.Clamp(percent, 20, 100);
        byte alpha = (byte)Math.Round(clamped / 100.0 * 255);

        long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        if ((exStyle & WS_EX_LAYERED) == 0)
        {
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle | WS_EX_LAYERED));
        }

        return SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);
    }

    public bool BringToFront(long handleValue)
    {
        IntPtr hwnd = new(handleValue);
        ShowWindow(hwnd, SW_RESTORE);
        return SetForegroundWindow(hwnd);
    }

    private static int GetWindowOpacityPercent(IntPtr hwnd)
    {
        long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        if ((exStyle & WS_EX_LAYERED) != 0 &&
            GetLayeredWindowAttributes(hwnd, out _, out byte alpha, out uint flags) &&
            (flags & LWA_ALPHA) != 0)
        {
            return (int)Math.Round(alpha / 255.0 * 100);
        }
        return 100;
    }
}
