using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class LanguageService : ILanguageService
    {
        private class SettingsModel
        {
            public string Language { get; set; } = "vi-VN";
        }

        private readonly string _settingsFilePath;
        private AppLanguage _currentLanguage = AppLanguage.Vietnamese;

        public AppLanguage CurrentLanguage => _currentLanguage;

        public string CurrentLanguageCode => _currentLanguage switch
        {
            AppLanguage.English => "en-US",
            _ => "vi-VN"
        };

        public event Action<AppLanguage>? LanguageChanged;

        public LanguageService(string? customSettingsFilePath = null)
        {
            if (!string.IsNullOrEmpty(customSettingsFilePath))
            {
                _settingsFilePath = customSettingsFilePath;
            }
            else
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                _settingsFilePath = Path.Combine(folder, "settings.json");
            }

            LoadSettings();
        }

        public void SetLanguage(AppLanguage language)
        {
            _currentLanguage = language;
            string code = CurrentLanguageCode;

            ApplyLanguageResource(code);
            SaveSettings();

            LanguageChanged?.Invoke(_currentLanguage);
        }

        public void ToggleLanguage()
        {
            SetLanguage(_currentLanguage == AppLanguage.Vietnamese ? AppLanguage.English : AppLanguage.Vietnamese);
        }

        public string GetString(string key, string fallback = "")
        {
            try
            {
                if (Application.Current != null)
                {
                    var res = Application.Current.TryFindResource(key);
                    if (res is string s) return s;
                }
            }
            catch { }

            return fallback;
        }

        private void ApplyLanguageResource(string code)
        {
            if (Application.Current == null) return;

            void Apply()
            {
                var appResources = Application.Current.Resources.MergedDictionaries;
                var currentDict = appResources.FirstOrDefault(d =>
                    (d.Source != null && d.Source.OriginalString.Contains("Strings.")) ||
                    d.Contains("App_Language_Code"));

                var newDict = new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/Resources/Languages/Strings.{code}.xaml", UriKind.Absolute)
                };

                if (currentDict != null)
                {
                    int index = appResources.IndexOf(currentDict);
                    appResources[index] = newDict;
                }
                else
                {
                    appResources.Add(newDict);
                }
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                Apply();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(Apply);
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<SettingsModel>(json);
                    if (settings != null)
                    {
                        _currentLanguage = settings.Language switch
                        {
                            "en-US" or "en" => AppLanguage.English,
                            _ => AppLanguage.Vietnamese
                        };
                    }
                }
            }
            catch
            {
                _currentLanguage = AppLanguage.Vietnamese;
            }

            ApplyLanguageResource(CurrentLanguageCode);
        }

        private void SaveSettings()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var settings = new SettingsModel
                {
                    Language = CurrentLanguageCode
                };

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu cài đặt: {ex.Message}");
            }
        }
    }
}
