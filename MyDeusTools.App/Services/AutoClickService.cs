using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace MyDeusTools.App.Services.Impl
{
    public class AutoClickService : IAutoClickService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x40;

        private readonly DispatcherTimer _timer;
        private readonly object _lock = new();
        private int _clickCount;
        private int _maxRepeat;
        private int _currentPointIndex = 0;
        private MouseButton _button;
        private ClickType _type;

        public event Action? Stopped;
        public bool IsRunning { get; private set; }
        public int Interval { get; private set; }
        public List<MousePoint> RecordedPoints { get; } = new();

        public AutoClickService()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }

        public void Start(int intervalMs, MouseButton button = MouseButton.Left, ClickType clickType = ClickType.Single, int repeatCount = 0)
        {
            if (IsRunning) return;

            _button = button;
            _type = clickType;
            _maxRepeat = repeatCount;
            _clickCount = 0;
            _currentPointIndex = 0;

            Interval = Math.Max(10, intervalMs);
            _timer.Interval = TimeSpan.FromMilliseconds(Interval);
            _timer.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _timer.Stop();
            IsRunning = false;
            Stopped?.Invoke();
        }

        public void ClearRecordedPoints()
        {
            lock (_lock)
            {
                RecordedPoints.Clear();
                _currentPointIndex = 0;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            lock (_lock)
            {
                if (RecordedPoints.Count > 0)
                {
                    if (_currentPointIndex >= RecordedPoints.Count)
                        _currentPointIndex = 0;

                    var point = RecordedPoints[_currentPointIndex];
                    SetCursorPos(point.X, point.Y);
                    
                    _currentPointIndex = (_currentPointIndex + 1) % RecordedPoints.Count;
                }
            }

            PerformClick();

            if (_type == ClickType.Double)
            {
                Task.Delay(10).ContinueWith(_ =>
                {
                    if (IsRunning) PerformClick();
                });
            }

            _clickCount++;
            if (_maxRepeat > 0 && _clickCount >= _maxRepeat)
            {
                Stop();
            }
        }

        private void PerformClick()
        {
            uint downFlag = _button switch
            {
                MouseButton.Right => MOUSEEVENTF_RIGHTDOWN,
                MouseButton.Middle => MOUSEEVENTF_MIDDLEDOWN,
                _ => MOUSEEVENTF_LEFTDOWN
            };

            uint upFlag = _button switch
            {
                MouseButton.Right => MOUSEEVENTF_RIGHTUP,
                MouseButton.Middle => MOUSEEVENTF_MIDDLEUP,
                _ => MOUSEEVENTF_LEFTUP
            };

            mouse_event(downFlag | upFlag, 0, 0, 0, 0);
        }
    }
}
