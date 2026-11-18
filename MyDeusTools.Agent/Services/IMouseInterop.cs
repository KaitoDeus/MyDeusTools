using System.Runtime.InteropServices;

namespace MyDeusTools.Agent.Services;

public enum OsMouseButton
{
    Left,
    Right,
    Middle
}

public enum OsClickType
{
    Single,
    Double
}

public record CursorPoint(int X, int Y);

public interface IMouseInterop
{
    CursorPoint GetCursorPosition();
    void SetCursorPosition(int x, int y);
    void Click(OsMouseButton button);
    bool IsVirtualKeyPressed(int vkCode);
}

public sealed class Win32MouseInterop : IMouseInterop
{
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public CursorPoint GetCursorPosition()
    {
        if (GetCursorPos(out var pt))
        {
            return new CursorPoint(pt.X, pt.Y);
        }
        return new CursorPoint(0, 0);
    }

    public void SetCursorPosition(int x, int y)
    {
        SetCursorPos(x, y);
    }

    public void Click(OsMouseButton button)
    {
        uint downFlag = button switch
        {
            OsMouseButton.Right => MOUSEEVENTF_RIGHTDOWN,
            OsMouseButton.Middle => MOUSEEVENTF_MIDDLEDOWN,
            _ => MOUSEEVENTF_LEFTDOWN
        };

        uint upFlag = button switch
        {
            OsMouseButton.Right => MOUSEEVENTF_RIGHTUP,
            OsMouseButton.Middle => MOUSEEVENTF_MIDDLEUP,
            _ => MOUSEEVENTF_LEFTUP
        };

        mouse_event(downFlag | upFlag, 0, 0, 0, 0);
    }

    public bool IsVirtualKeyPressed(int vkCode)
    {
        return (GetAsyncKeyState(vkCode) & 0x8000) != 0;
    }
}
