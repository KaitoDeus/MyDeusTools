using System.Diagnostics;

namespace MyDeusTools.App.Services.Impl
{
    public interface ISystemService
    {
        void ScheduleShutdown(int seconds);
        void CancelShutdown();
    }

    public class SystemService : ISystemService
    {
        public void ScheduleShutdown(int seconds)
        {
            // Lệnh: shutdown /s /t [giây]
            // /s: Shutdown, /t: Time (giây)
            RunShutdownCommand($"/s /t {seconds}");
        }

        public void CancelShutdown()
        {
            // Lệnh: shutdown /a
            // /a: Abort (Hủy lệnh tắt máy đang chờ)
            RunShutdownCommand("/a");
        }

        private void RunShutdownCommand(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("shutdown", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi thực thi lệnh Shutdown: {ex.Message}");
            }
        }
    }
}
