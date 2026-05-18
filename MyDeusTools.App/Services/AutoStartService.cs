using System;
using System.IO;
using Microsoft.Win32;

namespace MyDeusTools.App.Services.Impl
{
    public interface IAutoStartService
    {
        bool IsEnabled();
        void SetEnabled(bool enable);
    }

    public class AutoStartService : IAutoStartService
    {
        private readonly string _registryKeyName;

        public AutoStartService(string registryKeyName = "MyDeusTools")
        {
            _registryKeyName = registryKeyName;
        }

        public bool IsEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                if (key != null)
                {
                    object? value = key.GetValue(_registryKeyName);
                    if (value != null)
                    {
                        string path = value.ToString() ?? "";
                        string currentPath = Environment.ProcessPath ?? "";
                        return string.Equals(path.Trim('"'), currentPath.Trim('"'), StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra khởi động cùng Windows: {ex.Message}");
            }
            return false;
        }

        public void SetEnabled(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    if (enable)
                    {
                        string currentPath = Environment.ProcessPath ?? "";
                        if (!string.IsNullOrEmpty(currentPath))
                        {
                            key.SetValue(_registryKeyName, $"\"{currentPath}\"");
                        }
                    }
                    else
                    {
                        key.DeleteValue(_registryKeyName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi thiết lập khởi động cùng Windows: {ex.Message}");
            }
        }
    }
}
