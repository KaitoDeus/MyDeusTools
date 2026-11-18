using MyDeusTools.Agent.Services;

Console.Title = "MyDeusTools OS Companion Agent";
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("=================================================================");
Console.WriteLine("  MYDEUSTOOLS LOCAL COMPANION AGENT v1.0.0 (WIN32 BRIDGE)");
Console.WriteLine("  LISTENING : http://127.0.0.1:41950/ (LOOPBACK ONLY)");
Console.WriteLine("  HOTKEY F6 : Toggle OS Auto-Clicker Start/Stop globally");
Console.WriteLine("  HOTKEY F7 : Capture external screen cursor (X, Y) point");
Console.WriteLine("  EXIT      : Press Ctrl+C to terminate");
Console.WriteLine("=================================================================");
Console.ResetColor();

using var shutdownCts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    shutdownCts.Cancel();
};

var mouseInterop = new Win32MouseInterop();
using var autoClickEngine = new OsAutoClickEngine(mouseInterop);
using var hotkeys = new GlobalHotkeyService(mouseInterop, autoClickEngine);
var powerService = new OsSystemPowerService();
var windowPinner = new OsWindowPinnerService();
using var server = new LocalHttpBridgeServer(autoClickEngine, powerService, windowPinner);

hotkeys.Start();

try
{
    await server.RunAsync(shutdownCts.Token);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[FATAL] Could not start companion agent listener: {ex.Message}");
    Console.ResetColor();
    Environment.ExitCode = 1;
}
