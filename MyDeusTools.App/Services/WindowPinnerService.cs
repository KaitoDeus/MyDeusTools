using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class WindowPinnerService : IWindowPinnerService
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int LWA_ALPHA = 0x00000002;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int SW_RESTORE = 9;

        #region Win32 P/Invoke

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
        private static extern IntPtr GetForegroundWindow();

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
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint pcrKey, out byte pbAlpha, out uint pdwFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }

        #endregion

        public List<WindowInfoModel> GetOpenWindows()
        {
            var windows = new List<WindowInfoModel>();
            IntPtr shellWindow = GetShellWindow();
            int currentProcessId = Process.GetCurrentProcess().Id;

            EnumWindows((hWnd, lParam) =>
            {
                if (hWnd == shellWindow || !IsWindowVisible(hWnd))
                    return true;

                int length = GetWindowTextLength(hWnd);
                if (length <= 0)
                    return true;

                var builder = new StringBuilder(length + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                string title = builder.ToString().Trim();

                if (string.IsNullOrWhiteSpace(title))
                    return true;

                long exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
                if ((exStyle & WS_EX_TOOLWINDOW) != 0)
                    return true;

                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid == currentProcessId)
                    return true;

                string processName = "Unknown";
                try
                {
                    using var proc = Process.GetProcessById((int)pid);
                    processName = proc.ProcessName;
                }
                catch
                {
                    // Ignore process access errors
                }

                bool isTopMost = (exStyle & WS_EX_TOPMOST) != 0;
                byte opacity = 255;
                if ((exStyle & WS_EX_LAYERED) != 0 && GetLayeredWindowAttributes(hWnd, out _, out byte alpha, out _))
                {
                    opacity = alpha == 0 ? (byte)255 : alpha;
                }

                windows.Add(new WindowInfoModel
                {
                    Handle = hWnd,
                    Title = title,
                    ProcessName = processName,
                    ProcessId = (int)pid,
                    IsTopMost = isTopMost,
                    Opacity = opacity
                });

                return true;
            }, IntPtr.Zero);

            windows.Sort((a, b) =>
            {
                int topComparison = b.IsTopMost.CompareTo(a.IsTopMost);
                if (topComparison != 0) return topComparison;
                return string.Compare(a.DisplayText, b.DisplayText, StringComparison.OrdinalIgnoreCase);
            });

            return windows;
        }

        public bool SetTopMost(IntPtr hwnd, bool topMost)
        {
            if (hwnd == IntPtr.Zero) return false;

            IntPtr insertAfter = topMost ? HWND_TOPMOST : HWND_NOTOPMOST;
            return SetWindowPos(hwnd, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        public bool ToggleTopMost(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;
            bool current = IsWindowTopMost(hwnd);
            return SetTopMost(hwnd, !current);
        }

        public bool SetOpacity(IntPtr hwnd, byte opacity)
        {
            if (hwnd == IntPtr.Zero) return false;

            try
            {
                long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
                if ((exStyle & WS_EX_LAYERED) == 0)
                {
                    SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle | WS_EX_LAYERED));
                }

                return SetLayeredWindowAttributes(hwnd, 0, opacity, LWA_ALPHA);
            }
            catch
            {
                return false;
            }
        }

        public bool BringToFront(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;

            ShowWindow(hwnd, SW_RESTORE);
            return SetForegroundWindow(hwnd);
        }

        public IntPtr GetForegroundWindowHandle()
        {
            return GetForegroundWindow();
        }

        public bool IsWindowTopMost(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;

            long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
            return (exStyle & WS_EX_TOPMOST) != 0;
        }

        public byte GetWindowOpacity(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return 255;

            long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
            if ((exStyle & WS_EX_LAYERED) != 0 && GetLayeredWindowAttributes(hwnd, out _, out byte alpha, out _))
            {
                return alpha == 0 ? (byte)255 : alpha;
            }

            return 255;
        }
    }
}
