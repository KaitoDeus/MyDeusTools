using System;

namespace MyDeusTools.App.Services.Impl
{
    public interface IAutoClickService
    {
        bool IsRunning { get; }
        void Start(int intervalMs);
        void Stop();
    }
}
