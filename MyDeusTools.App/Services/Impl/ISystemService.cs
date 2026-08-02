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
        public void ScheduleShutdown(int seconds)
        {
            ScheduleAction(ShutdownMode.Shutdown, seconds);
        }

        public void ScheduleAction(ShutdownMode mode, int seconds)
        {
            string flag = mode switch
            {
                ShutdownMode.Restart => "/r",
                ShutdownMode.Hibernate => "/h",
                _ => "/s"
            };

            if (mode == ShutdownMode.Hibernate)
            {
                // Lên lịch ngủ đông: timeout rồi gọi shutdown /h
                RunCommand("cmd.exe", $"/c timeout /t {seconds} /nobreak && shutdown /h");
            }
            else
            {
                RunCommand("shutdown", $"{flag} /f /t {seconds}");
            }
        }

        public void CancelShutdown()
        {
            // Lệnh: shutdown /a
            RunCommand("shutdown", "/a");
        }

        private void RunCommand(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi thực thi lệnh hệ thống: {ex.Message}");
            }
        }
    }
}
