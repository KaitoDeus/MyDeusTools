using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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

        [Fact]
        public void ResourceDictionaries_ShouldContainIdenticalKeySets()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "MyDeusTools.sln")))
            {
                dir = Directory.GetParent(dir)?.FullName!;
            }
            Assert.NotNull(dir);

            string enPath = Path.Combine(dir, "MyDeusTools.App", "Resources", "Languages", "Strings.en-US.xaml");
            string viPath = Path.Combine(dir, "MyDeusTools.App", "Resources", "Languages", "Strings.vi-VN.xaml");

            Assert.True(File.Exists(enPath), $"File not found: {enPath}");
            Assert.True(File.Exists(viPath), $"File not found: {viPath}");

            var enXml = XDocument.Load(enPath);
            var viXml = XDocument.Load(viPath);
            XNamespace xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

            var enKeys = enXml.Descendants()
                .Select(e => e.Attribute(xNs + "Key")?.Value)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToHashSet();

            var viKeys = viXml.Descendants()
                .Select(e => e.Attribute(xNs + "Key")?.Value)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToHashSet();

            Assert.NotEmpty(enKeys);
            Assert.NotEmpty(viKeys);

            var missingInVi = enKeys.Except(viKeys).ToList();
            var missingInEn = viKeys.Except(enKeys).ToList();

            Assert.Empty(missingInVi);
            Assert.Empty(missingInEn);
        }

        [Theory]
        [InlineData("WindowPinner_Title")]
        [InlineData("WindowPinner_Pin")]
        [InlineData("WindowPinner_Unpin")]
        [InlineData("WindowPinner_ClearSearch")]
        [InlineData("WindowPinner_StatusReady")]
        [InlineData("TextDev_Title")]
        [InlineData("TextDev_JsonInput")]
        [InlineData("TextDev_JsonOutput")]
        [InlineData("TextDev_Base64Input")]
        [InlineData("TextDev_Base64Output")]
        [InlineData("TextDev_UrlHtmlInput")]
        [InlineData("TextDev_UrlHtmlOutput")]
        [InlineData("TextDev_HashStringHeader")]
        [InlineData("TextDev_HashTextHeader")]
        [InlineData("TextDev_HashFileHeader")]
        [InlineData("TextDev_InspectorPrompt")]
        [InlineData("TextDev_StatsChars")]
        [InlineData("TextDev_CaseHeader")]
        [InlineData("TextDev_Format")]
        [InlineData("TextDev_Minify")]
        [InlineData("TextDev_Validate")]
        [InlineData("TextDev_Clear")]
        [InlineData("TextDev_Swap")]
        [InlineData("Color_OverlayHelp")]
        [InlineData("Qr_OverlayHelp")]
        [InlineData("AutoClick_OverlayHelp")]
        [InlineData("Nav_VideoConverter")]
        [InlineData("Video_Title")]
        public void CriticalKeys_ShouldExistInBothDictionaries(string key)
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "MyDeusTools.sln")))
            {
                dir = Directory.GetParent(dir)?.FullName!;
            }
            Assert.NotNull(dir);

            string enPath = Path.Combine(dir, "MyDeusTools.App", "Resources", "Languages", "Strings.en-US.xaml");
            string viPath = Path.Combine(dir, "MyDeusTools.App", "Resources", "Languages", "Strings.vi-VN.xaml");

            var enXml = XDocument.Load(enPath);
            var viXml = XDocument.Load(viPath);
            XNamespace xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

            bool existsInEn = enXml.Descendants().Any(e => e.Attribute(xNs + "Key")?.Value == key);
            bool existsInVi = viXml.Descendants().Any(e => e.Attribute(xNs + "Key")?.Value == key);

            Assert.True(existsInEn, $"Key '{key}' missing from Strings.en-US.xaml");
            Assert.True(existsInVi, $"Key '{key}' missing from Strings.vi-VN.xaml");
        }
    }
}
