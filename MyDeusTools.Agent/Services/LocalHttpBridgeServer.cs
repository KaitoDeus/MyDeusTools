using System.Net;
using System.Text;
using System.Text.Json;

namespace MyDeusTools.Agent.Services;

public record StartAutoClickRequest(
    int? IntervalMs,
    string? Button,
    string? ClickType,
    int? RepeatCount,
    bool? UseRecordedPoints,
    List<CursorPoint>? Points
);

public record CapturePointRequest(int DelaySeconds = 0);
public record SchedulePowerRequest(string Mode, int Seconds);
public record PinWindowRequest(long Handle, bool TopMost);
public record OpacityWindowRequest(long Handle, int Percent);
public record FocusWindowRequest(long Handle);

/// <summary>
/// Lightweight zero-dependency HTTP bridge on http://127.0.0.1:41950/ enabling the browser Web App
/// to control Win32 OS Auto-Clicker, System Power, and Desktop Window Pinner safely on loopback.
/// </summary>
public sealed class LocalHttpBridgeServer : IDisposable
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpListener _listener = new();
    private readonly OsAutoClickEngine _autoClick;
    private readonly OsSystemPowerService _power;
    private readonly OsWindowPinnerService _pinner;
    private readonly string _prefix;

    public LocalHttpBridgeServer(
        OsAutoClickEngine autoClick,
        OsSystemPowerService power,
        OsWindowPinnerService pinner,
        string prefix = "http://127.0.0.1:41950/")
    {
        _autoClick = autoClick;
        _power = power;
        _pinner = pinner;
        _prefix = prefix;
        _listener.Prefixes.Add(_prefix);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        _listener.Start();
        using var reg = ct.Register(() =>
        {
            try { _listener.Stop(); } catch { }
        });

        while (!ct.IsCancellationRequested && _listener.IsListening)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync();
            }
            catch (HttpListenerException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[HTTP_ERR] {ex.Message}");
                continue;
            }

            _ = Task.Run(() => HandleContextAsync(ctx), ct);
        }
    }

    private async Task HandleContextAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var res = ctx.Response;

        try
        {
            ApplyCorsHeaders(req, res);

            if (req.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                res.StatusCode = 204;
                res.Close();
                return;
            }

            string path = (req.Url?.AbsolutePath ?? "/").TrimEnd('/');
            if (string.IsNullOrEmpty(path)) path = "/";

            switch (req.HttpMethod.ToUpperInvariant(), path)
            {
                case ("GET", "/api/health"):
                    await WriteJsonAsync(res, 200, new
                    {
                        status = "online",
                        agent = "MyDeusTools.Agent",
                        version = "1.0.0",
                        platform = Environment.OSVersion.Platform.ToString().ToLowerInvariant(),
                        capabilities = new[] { "autoclick", "hotkey-f6", "hotkey-f7", "system-power", "window-pinner" }
                    });
                    return;

                case ("GET", "/api/autoclick/status"):
                    await WriteJsonAsync(res, 200, _autoClick.GetStatus());
                    return;

                case ("POST", "/api/autoclick/start"):
                {
                    var body = await ReadJsonAsync<StartAutoClickRequest>(req);
                    if (body is not null)
                    {
                        var button = Enum.TryParse<OsMouseButton>(body.Button, true, out var b)
                            ? b
                            : OsMouseButton.Left;
                        var clickType = Enum.TryParse<OsClickType>(body.ClickType, true, out var ct2)
                            ? ct2
                            : OsClickType.Single;

                        _autoClick.Configure(
                            intervalMs: body.IntervalMs ?? 100,
                            button: button,
                            clickType: clickType,
                            repeatCount: body.RepeatCount ?? 0,
                            useRecordedPoints: body.UseRecordedPoints ?? true,
                            replacePoints: body.Points
                        );
                    }
                    _autoClick.Start();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [API] OS Auto-Clicker STARTED ({_autoClick.IntervalMs}ms, {_autoClick.Button})");
                    await WriteJsonAsync(res, 200, _autoClick.GetStatus());
                    return;
                }

                case ("POST", "/api/autoclick/stop"):
                    _autoClick.Stop();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [API] OS Auto-Clicker STOPPED (clicks={_autoClick.ClicksDone})");
                    await WriteJsonAsync(res, 200, _autoClick.GetStatus());
                    return;

                case ("POST", "/api/autoclick/capture"):
                {
                    var body = await ReadJsonAsync<CapturePointRequest>(req);
                    int delay = Math.Clamp(body?.DelaySeconds ?? 0, 0, 10);
                    if (delay > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(delay));
                    }
                    var captured = _autoClick.CaptureCursorPoint();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [API] Captured point ({captured.X}, {captured.Y})");
                    await WriteJsonAsync(res, 200, new { captured, status = _autoClick.GetStatus() });
                    return;
                }

                case ("POST", "/api/autoclick/points/clear"):
                    _autoClick.ClearRecordedPoints();
                    await WriteJsonAsync(res, 200, _autoClick.GetStatus());
                    return;

                case ("POST", "/api/system/schedule"):
                {
                    var body = await ReadJsonAsync<SchedulePowerRequest>(req);
                    if (body is null)
                    {
                        await WriteJsonAsync(res, 400, new { error = "Invalid payload" });
                        return;
                    }
                    var mode = Enum.TryParse<OsPowerMode>(body.Mode, true, out var parsedMode)
                        ? parsedMode
                        : OsPowerMode.Shutdown;
                    _power.Schedule(mode, body.Seconds);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [API] Scheduled OS {mode} in {body.Seconds}s");
                    await WriteJsonAsync(res, 200, new { scheduled = true, mode = mode.ToString().ToLowerInvariant(), seconds = body.Seconds });
                    return;
                }

                case ("POST", "/api/system/cancel"):
                    _power.Cancel();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [API] Cancelled scheduled OS power action");
                    await WriteJsonAsync(res, 200, new { cancelled = true });
                    return;

                case ("GET", "/api/windows"):
                    await WriteJsonAsync(res, 200, _pinner.GetOpenWindows());
                    return;

                case ("POST", "/api/windows/pin"):
                {
                    var body = await ReadJsonAsync<PinWindowRequest>(req);
                    if (body is null)
                    {
                        await WriteJsonAsync(res, 400, new { error = "Invalid payload" });
                        return;
                    }
                    await WriteJsonAsync(res, 200, new { success = _pinner.SetTopMost(body.Handle, body.TopMost) });
                    return;
                }

                case ("POST", "/api/windows/opacity"):
                {
                    var body = await ReadJsonAsync<OpacityWindowRequest>(req);
                    if (body is null)
                    {
                        await WriteJsonAsync(res, 400, new { error = "Invalid payload" });
                        return;
                    }
                    await WriteJsonAsync(res, 200, new { success = _pinner.SetOpacityPercent(body.Handle, body.Percent) });
                    return;
                }

                case ("POST", "/api/windows/focus"):
                {
                    var body = await ReadJsonAsync<FocusWindowRequest>(req);
                    if (body is null)
                    {
                        await WriteJsonAsync(res, 400, new { error = "Invalid payload" });
                        return;
                    }
                    await WriteJsonAsync(res, 200, new { success = _pinner.BringToFront(body.Handle) });
                    return;
                }

                default:
                    await WriteJsonAsync(res, 404, new { error = "Endpoint not found", path });
                    return;
            }
        }
        catch (Exception ex)
        {
            try
            {
                await WriteJsonAsync(res, 500, new { error = ex.Message });
            }
            catch
            {
                // Response stream may already be closed
            }
        }
    }

    private static void ApplyCorsHeaders(HttpListenerRequest req, HttpListenerResponse res)
    {
        res.Headers["Access-Control-Allow-Origin"] = "*";
        res.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
        res.Headers["Access-Control-Allow-Headers"] = "Content-Type, Access-Control-Request-Private-Network";
        if (req.Headers["Access-Control-Request-Private-Network"] != null)
        {
            res.Headers["Access-Control-Allow-Private-Network"] = "true";
        }
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpListenerRequest req)
    {
        if (!req.HasEntityBody) return default;
        using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
        string raw = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(raw)) return default;
        return JsonSerializer.Deserialize<T>(raw, JsonOpts);
    }

    private static async Task WriteJsonAsync(HttpListenerResponse res, int statusCode, object payload)
    {
        string json = JsonSerializer.Serialize(payload, JsonOpts);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        res.StatusCode = statusCode;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentLength64 = bytes.Length;
        await res.OutputStream.WriteAsync(bytes);
        res.Close();
    }

    public void Dispose()
    {
        _listener.Close();
    }
}
