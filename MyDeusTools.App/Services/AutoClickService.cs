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
        private readonly DispatcherTimer _recordTimer;
        private int _clickCount;
        private int _maxRepeat;
        private int _currentPointIndex = 0;
        private MouseButton _button;
        private ClickType _type;

        public bool IsRunning { get; private set; }
        public bool IsRecording { get; private set; }
        public int Interval { get; private set; }
        public List<MousePoint> RecordedPoints { get; } = new();

        public AutoClickService()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;

            _recordTimer = new DispatcherTimer();
            _recordTimer.Interval = TimeSpan.FromMilliseconds(200); // Ghi tọa độ mỗi 200ms
            _recordTimer.Tick += RecordTimer_Tick;
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
            _timer.Stop();
            IsRunning = false;
        }

        public void StartRecording()
        {
            if (IsRunning) return;
            IsRecording = true;
            _recordTimer.Start();
        }

        public void StopRecording()
        {
            IsRecording = false;
            _recordTimer.Stop();
        }

        public void ClearRecordedPoints()
        {
            RecordedPoints.Clear();
        }

        private void RecordTimer_Tick(object? sender, EventArgs e)
        {
            if (GetCursorPos(out POINT lpPoint))
            {
                // Chỉ thêm nếu tọa độ khác điểm cuối cùng để tránh trùng lặp
                if (RecordedPoints.Count == 0 || 
                    RecordedPoints[^1].X != lpPoint.X || 
                    RecordedPoints[^1].Y != lpPoint.Y)
                {
                    RecordedPoints.Add(new MousePoint { X = lpPoint.X, Y = lpPoint.Y });
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Nếu có điểm đã ghi, di chuyển chuột tới đó trước khi click
            if (RecordedPoints.Count > 0)
            {
                var point = RecordedPoints[_currentPointIndex];
                SetCursorPos(point.X, point.Y);
                
                _currentPointIndex++;
                if (_currentPointIndex >= RecordedPoints.Count)
                    _currentPointIndex = 0;
            }

            PerformClick();

            if (_type == ClickType.Double)
            {
                Task.Delay(10).ContinueWith(_ => PerformClick());
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
