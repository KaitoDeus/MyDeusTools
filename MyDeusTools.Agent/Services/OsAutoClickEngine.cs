namespace MyDeusTools.Agent.Services;

public record AutoClickStatusDto(
    bool IsRunning,
    int IntervalMs,
    string Button,
    string ClickType,
    int RepeatCount,
    int ClicksDone,
    bool UseRecordedPoints,
    IReadOnlyList<CursorPoint> Points,
    int CursorX,
    int CursorY,
    string HotkeyToggle,
    string HotkeyCapture
);

public sealed class OsAutoClickEngine : IDisposable
{
    public const int MinIntervalMs = 10;

    private readonly IMouseInterop _mouse;
    private readonly object _sync = new();
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;

    private int _intervalMs = 100;
    private OsMouseButton _button = OsMouseButton.Left;
    private OsClickType _clickType = OsClickType.Single;
    private int _repeatCount = 0; // 0 = infinite
    private bool _useRecordedPoints = true;
    private int _clicksDone = 0;
    private int _pointIndex = 0;
    private readonly List<CursorPoint> _recordedPoints = new();

    public bool IsRunning { get; private set; }
    public int IntervalMs => _intervalMs;
    public OsMouseButton Button => _button;
    public OsClickType ClickType => _clickType;
    public int RepeatCount => _repeatCount;
    public int ClicksDone => _clicksDone;
    public bool UseRecordedPoints => _useRecordedPoints;

    public OsAutoClickEngine(IMouseInterop mouseInterop)
    {
        _mouse = mouseInterop;
    }

    public void Configure(
        int intervalMs,
        OsMouseButton button = OsMouseButton.Left,
        OsClickType clickType = OsClickType.Single,
        int repeatCount = 0,
        bool useRecordedPoints = true,
        IEnumerable<CursorPoint>? replacePoints = null)
    {
        lock (_sync)
        {
            _intervalMs = Math.Max(MinIntervalMs, intervalMs);
            _button = button;
            _clickType = clickType;
            _repeatCount = Math.Max(0, repeatCount);
            _useRecordedPoints = useRecordedPoints;

            if (replacePoints != null)
            {
                _recordedPoints.Clear();
                _recordedPoints.AddRange(replacePoints);
                _pointIndex = 0;
            }
        }
    }

    public void Start()
    {
        lock (_sync)
        {
            if (IsRunning) return;

            _clicksDone = 0;
            _pointIndex = 0;
            IsRunning = true;

            _loopCts = new CancellationTokenSource();
            var ct = _loopCts.Token;
            var interval = Math.Max(MinIntervalMs, _intervalMs);

            _loopTask = Task.Run(() => RunLoopAsync(interval, ct), ct);
        }
    }

    public void Stop()
    {
        CancellationTokenSource? ctsToCancel = null;
        lock (_sync)
        {
            if (!IsRunning && _loopCts == null) return;
            IsRunning = false;
            ctsToCancel = _loopCts;
            _loopCts = null;
        }

        try
        {
            ctsToCancel?.Cancel();
        }
        catch
        {
            // Ignore cancellation race
        }
        finally
        {
            ctsToCancel?.Dispose();
        }
    }

    public void Toggle()
    {
        lock (_sync)
        {
            if (IsRunning)
                Stop();
            else
                Start();
        }
    }

    public CursorPoint CaptureCursorPoint()
    {
        var pos = _mouse.GetCursorPosition();
        lock (_sync)
        {
            _recordedPoints.Add(pos);
        }
        return pos;
    }

    public void ClearRecordedPoints()
    {
        lock (_sync)
        {
            _recordedPoints.Clear();
            _pointIndex = 0;
        }
    }

    public AutoClickStatusDto GetStatus()
    {
        var cursor = _mouse.GetCursorPosition();
        lock (_sync)
        {
            return new AutoClickStatusDto(
                IsRunning: IsRunning,
                IntervalMs: _intervalMs,
                Button: _button.ToString().ToLowerInvariant(),
                ClickType: _clickType.ToString().ToLowerInvariant(),
                RepeatCount: _repeatCount,
                ClicksDone: _clicksDone,
                UseRecordedPoints: _useRecordedPoints,
                Points: _recordedPoints.ToArray(),
                CursorX: cursor.X,
                CursorY: cursor.Y,
                HotkeyToggle: "F6",
                HotkeyCapture: "F7"
            );
        }
    }

    private async Task RunLoopAsync(int intervalMs, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                if (!ExecuteSingleTick())
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on stop
        }
        finally
        {
            lock (_sync)
            {
                IsRunning = false;
            }
        }
    }

    internal void SetRunningForTest(bool running)
    {
        lock (_sync)
        {
            IsRunning = running;
        }
    }

    /// <summary>
    /// Executes one logical click iteration. Exposed internally for deterministic testing.
    /// </summary>
    internal bool ExecuteSingleTick()
    {
        OsMouseButton button;
        OsClickType clickType;
        bool moveFirst = false;
        CursorPoint targetPoint = default!;

        lock (_sync)
        {
            if (!IsRunning) return false;

            button = _button;
            clickType = _clickType;

            if (_useRecordedPoints && _recordedPoints.Count > 0)
            {
                if (_pointIndex >= _recordedPoints.Count)
                    _pointIndex = 0;

                targetPoint = _recordedPoints[_pointIndex];
                _pointIndex = (_pointIndex + 1) % _recordedPoints.Count;
                moveFirst = true;
            }
        }

        if (moveFirst)
        {
            _mouse.SetCursorPosition(targetPoint.X, targetPoint.Y);
        }

        _mouse.Click(button);

        if (clickType == OsClickType.Double)
        {
            Thread.Sleep(12);
            if (IsRunning)
            {
                _mouse.Click(button);
            }
        }

        lock (_sync)
        {
            _clicksDone++;
            if (_repeatCount > 0 && _clicksDone >= _repeatCount)
            {
                IsRunning = false;
                try { _loopCts?.Cancel(); } catch { }
                return false;
            }
        }

        return true;
    }

    public void Dispose()
    {
        Stop();
    }
}
