using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace MyDeusTools.App.Services.Impl
{
    public class AutoClickService : IAutoClickService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private readonly DispatcherTimer _timer;
        public bool IsRunning { get; private set; }
        public int Interval { get; private set; }

        public AutoClickService()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += OnTimerTick;
        }

        public void Start(int intervalMs)
        {
            if (IsRunning) return;
            Interval = Math.Max(10, intervalMs); // Đảm bảo tối thiểu 10ms
            _timer.Interval = TimeSpan.FromMilliseconds(Interval);
            _timer.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _timer.Stop();
            IsRunning = false;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}
