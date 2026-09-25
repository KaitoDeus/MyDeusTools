using System;
using System.IO;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using Xunit;

namespace MyDeusTools.Tests
{
    public class LanguageServiceTests : IDisposable
    {
        private readonly string _testSettingsPath;

        public LanguageServiceTests()
        {
            _testSettingsPath = Path.Combine(Path.GetTempPath(), $"mydeustools_test_settings_{Guid.NewGuid():N}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testSettingsPath))
            {
                try { File.Delete(_testSettingsPath); } catch { }
            }
        }

        [Fact]
        public void DefaultLanguage_ShouldBeVietnamese()
        {
            // Act
            var service = new LanguageService(_testSettingsPath);

            // Assert
            Assert.Equal(AppLanguage.Vietnamese, service.CurrentLanguage);
            Assert.Equal("vi-VN", service.CurrentLanguageCode);
        }

        [Fact]
        public void SetLanguage_ShouldUpdateCurrentLanguageAndCode()
        {
            // Arrange
            var service = new LanguageService(_testSettingsPath);

            // Act
            service.SetLanguage(AppLanguage.English);

            // Assert
            Assert.Equal(AppLanguage.English, service.CurrentLanguage);
            Assert.Equal("en-US", service.CurrentLanguageCode);
        }

        [Fact]
        public void ToggleLanguage_ShouldSwitchBetweenVietnameseAndEnglish()
        {
            // Arrange
            var service = new LanguageService(_testSettingsPath);

            // Act 1: Toggle from Vietnamese -> English
            service.ToggleLanguage();
            Assert.Equal(AppLanguage.English, service.CurrentLanguage);
            Assert.Equal("en-US", service.CurrentLanguageCode);

            // Act 2: Toggle from English -> Vietnamese
            service.ToggleLanguage();
            Assert.Equal(AppLanguage.Vietnamese, service.CurrentLanguage);
            Assert.Equal("vi-VN", service.CurrentLanguageCode);
        }

        [Fact]
        public void LanguageChangedEvent_ShouldFire_WhenLanguageChanges()
        {
            // Arrange
            var service = new LanguageService(_testSettingsPath);
            AppLanguage? firedLanguage = null;
            service.LanguageChanged += lang => firedLanguage = lang;

            // Act
            service.SetLanguage(AppLanguage.English);

            // Assert
            Assert.Equal(AppLanguage.English, firedLanguage);
        }

        [Fact]
        public void SaveAndLoad_ShouldPersistLanguageSetting()
        {
            // Arrange
            var service1 = new LanguageService(_testSettingsPath);
            service1.SetLanguage(AppLanguage.English);

            // Act
            var service2 = new LanguageService(_testSettingsPath);

            // Assert
            Assert.Equal(AppLanguage.English, service2.CurrentLanguage);
            Assert.Equal("en-US", service2.CurrentLanguageCode);
        }

        [Fact]
        public void GetString_ShouldReturnFallback_WhenKeyNotFound()
        {
            // Arrange
            var service = new LanguageService(_testSettingsPath);

            // Act
            string val = service.GetString("NonExistentKey_12345", "FallbackText");

            // Assert
            Assert.Equal("FallbackText", val);
        }
    }
}
