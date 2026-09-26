using System.Diagnostics;

namespace MyDeusTools.App.Services.Impl
{
    public enum ShutdownMode
    {
        Shutdown,
        Restart,
        Hibernate
    }

    public interface ISystemService
    {
        void ScheduleShutdown(int seconds);
        void ScheduleAction(ShutdownMode mode, int seconds);
        void CancelShutdown();
    }

    public class SystemService : ISystemService
    {
        private Process? _hibernateProcess;

        public void ScheduleShutdown(int seconds)
        {
            ScheduleAction(ShutdownMode.Shutdown, seconds);
        }

        public void ScheduleAction(ShutdownMode mode, int seconds)
        {
            // Luôn hủy bất kỳ lệnh hẹn giờ nào đã được lên lịch trước đó (Windows báo lỗi 1190 nếu không hủy trước)
            CancelShutdown();

            string flag = mode switch
            {
                ShutdownMode.Restart => "/r",
                ShutdownMode.Hibernate => "/h",
                _ => "/s"
            };

            if (mode == ShutdownMode.Hibernate)
            {
                // Lên lịch ngủ đông: timeout rồi gọi shutdown /h
                _hibernateProcess = RunCommand("cmd.exe", $"/c timeout /t {seconds} /nobreak && shutdown /h");
            }
            else
            {
                RunCommand("shutdown", $"{flag} /f /t {seconds}", waitForExit: true);
            }
        }

        public void CancelShutdown()
        {
            // Hủy lệnh hẹn giờ Windows: shutdown /a
            RunCommand("shutdown", "/a", waitForExit: true);

            // Hủy tiến trình ngủ đông nếu đang chạy
            if (_hibernateProcess != null)
            {
                try
                {
                    if (!_hibernateProcess.HasExited)
                    {
                        _hibernateProcess.Kill(entireProcessTree: true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi hủy tiến trình ngủ đông: {ex.Message}");
                }
                finally
                {
                    _hibernateProcess.Dispose();
                    _hibernateProcess = null;
                }
            }
        }

        private Process? RunCommand(string fileName, string arguments, bool waitForExit = false)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments)
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
                Debug.WriteLine($"Lỗi thực thi lệnh hệ thống: {ex.Message}");
                return null;
            }
        }
    }
}
