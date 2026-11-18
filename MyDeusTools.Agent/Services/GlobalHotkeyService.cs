namespace MyDeusTools.Agent.Services;

/// <summary>
/// Listens globally for F6 (Toggle OS Auto-Clicker) and F7 (Capture Cursor Coordinates)
/// using edge-triggered virtual key checks so hotkeys work inside games and external desktop apps.
/// </summary>
public sealed class GlobalHotkeyService : IDisposable
{
    private const int VK_F6 = 0x75;
    private const int VK_F7 = 0x76;

    private readonly IMouseInterop _mouse;
    private readonly OsAutoClickEngine _autoClick;
    private CancellationTokenSource? _cts;
    private Task? _task;

    private bool _f6WasDown;
    private bool _f7WasDown;

    public GlobalHotkeyService(IMouseInterop mouse, OsAutoClickEngine autoClick)
    {
        _mouse = mouse;
        _autoClick = autoClick;
    }

    public void Start()
    {
        if (_cts != null) return;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _task = Task.Run(() => PollLoopAsync(token), token);
    }

    private async Task PollLoopAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(40));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                bool f6Down = _mouse.IsVirtualKeyPressed(VK_F6);
                if (f6Down && !_f6WasDown)
                {
                    _autoClick.Toggle();
                    Console.WriteLine(
                        $"[{DateTime.Now:HH:mm:ss}] [HOTKEY:F6] OS Auto-Clicker -> {(_autoClick.IsRunning ? "RUNNING" : "STOPPED")}"
                    );
                }
                _f6WasDown = f6Down;

                bool f7Down = _mouse.IsVirtualKeyPressed(VK_F7);
                if (f7Down && !_f7WasDown)
                {
                    var pt = _autoClick.CaptureCursorPoint();
                    Console.WriteLine(
                        $"[{DateTime.Now:HH:mm:ss}] [HOTKEY:F7] Captured screen point ({pt.X}, {pt.Y})"
                    );
                }
                _f7WasDown = f7Down;
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        _cts?.Dispose();
        _cts = null;
    }
}
