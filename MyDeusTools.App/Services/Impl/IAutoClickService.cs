using System;

namespace MyDeusTools.App.Services.Impl
{
    public interface IAutoClickService
    {
        bool IsRunning { get; }
        int Interval { get; }
        void Start(int intervalMs);
        void Stop();
    }
}
