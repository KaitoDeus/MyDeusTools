using System;

namespace MyDeusTools.App.Services.Impl
{
    public enum AppLanguage
    {
        Vietnamese,
        English
    }

    public interface ILanguageService
    {
        AppLanguage CurrentLanguage { get; }
        string CurrentLanguageCode { get; }
        event Action<AppLanguage>? LanguageChanged;

        void SetLanguage(AppLanguage language);
        void ToggleLanguage();
        string GetString(string key, string fallback = "");
    }
}
