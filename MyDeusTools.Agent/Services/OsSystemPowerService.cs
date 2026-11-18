using System.Diagnostics;

namespace MyDeusTools.Agent.Services;

public enum OsPowerMode
{
    Shutdown,
    Restart,
    Hibernate
}

public sealed class OsSystemPowerService
{
    private Process? _hibernateProcess;

    public void Schedule(OsPowerMode mode, int seconds)
    {
        int clampedSeconds = Math.Max(0, seconds);
        Cancel();

        if (mode == OsPowerMode.Hibernate)
        {
            _hibernateProcess = RunCommand(
                "cmd.exe",
                $"/c timeout /t {clampedSeconds} /nobreak && shutdown /h",
                waitForExit: false
            );
        }
        else
        {
            string flag = mode == OsPowerMode.Restart ? "/r" : "/s";
            RunCommand("shutdown", $"{flag} /f /t {clampedSeconds}", waitForExit: true);
        }
    }

    public void Cancel()
    {
        RunCommand("shutdown", "/a", waitForExit: true);

        if (_hibernateProcess != null)
        {
            try
            {
                if (!_hibernateProcess.HasExited)
                {
                    _hibernateProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignore if already exited
            }
            finally
            {
                _hibernateProcess.Dispose();
                _hibernateProcess = null;
            }
        }
    }

    private static Process? RunCommand(string fileName, string arguments, bool waitForExit)
    {
        try
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var process = Process.Start(psi);
            if (waitForExit && process != null)
            {
                process.WaitForExit(2000);
            }
            return process;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error running system command: {ex.Message}");
            return null;
        }
    }
}
