using System;
using System.IO;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class TextUtilityServiceTests
    {
        private readonly ITextUtilityService _service;

        public TextUtilityServiceTests()
        {
            _service = new TextUtilityService();
        }

        // ==================== JSON TESTS ====================
        [Fact]
        public void FormatJson_ShouldReturnIndentedJson_WhenValidJson()
        {
            // Arrange
            string raw = "{\"name\":\"MyDeus\",\"version\":\"1.0\"}";

            // Act
            string formatted = _service.FormatJson(raw);

            // Assert
            Assert.Contains("\n", formatted);
            Assert.Contains("\"name\": \"MyDeus\"", formatted);
        }

        [Fact]
        public void MinifyJson_ShouldReturnCompactJson_WhenValidJson()
        {
            // Arrange
            string indented = "{\n  \"name\": \"MyDeus\",\n  \"count\": 42\n}";

            // Act
            string minified = _service.MinifyJson(indented);

            // Assert
            Assert.DoesNotContain("\n", minified);
            Assert.Equal("{\"name\":\"MyDeus\",\"count\":42}", minified);
        }

        [Fact]
        public void ValidateJson_ShouldReturnTrueForValid_AndFalseForInvalid()
        {
            // Arrange
            string valid = "{\"valid\": true}";
            string invalid = "{ invalid json: 123 }";

            // Act
            var validResult = _service.ValidateJson(valid);
            var invalidResult = _service.ValidateJson(invalid);

            // Assert
            Assert.True(validResult.IsValid);
            Assert.Null(validResult.ErrorMessage);

            Assert.False(invalidResult.IsValid);
            Assert.NotNull(invalidResult.ErrorMessage);
        }

        // ==================== BASE64 TESTS ====================
        [Fact]
        public void TextToBase64_And_Base64ToText_ShouldRoundtrip()
        {
            // Arrange
            string original = "Xin chào Việt Nam! 12345 &*#";

            // Act
            string base64 = _service.TextToBase64(original);
            string decoded = _service.Base64ToText(base64);

            // Assert
            Assert.NotEqual(original, base64);
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void FileToBase64_And_Base64ToFile_ShouldRoundtrip()
        {
            // Arrange
            string tempInputFile = Path.Combine(Path.GetTempPath(), $"test_in_{Guid.NewGuid():N}.dat");
            string tempOutputFile = Path.Combine(Path.GetTempPath(), $"test_out_{Guid.NewGuid():N}.dat");
            byte[] originalBytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x02, 0x03 };
            File.WriteAllBytes(tempInputFile, originalBytes);

            try
            {
                // Act
                string base64 = _service.FileToBase64(tempInputFile);
                _service.Base64ToFile(base64, tempOutputFile);

                // Assert
                Assert.True(File.Exists(tempOutputFile));
                byte[] decodedBytes = File.ReadAllBytes(tempOutputFile);
                Assert.Equal(originalBytes, decodedBytes);
            }
            finally
            {
                if (File.Exists(tempInputFile)) File.Delete(tempInputFile);
                if (File.Exists(tempOutputFile)) File.Delete(tempOutputFile);
            }
        }

        // ==================== URL & HTML TESTS ====================
        [Fact]
        public void UrlEncode_And_UrlDecode_ShouldRoundtrip()
        {
            // Arrange
            string original = "https://example.com/search?q=c# 12 & .net 8";

            // Act
            string encoded = _service.UrlEncode(original);
            string decoded = _service.UrlDecode(encoded);

            // Assert
            Assert.Contains("%20", encoded);
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void HtmlEncode_And_HtmlDecode_ShouldRoundtrip()
        {
            // Arrange
            string original = "<div>Hello & Welcome to <MyDeusTools> \"Suite\"</div>";

            // Act
            string encoded = _service.HtmlEncode(original);
            string decoded = _service.HtmlDecode(encoded);

            // Assert
            Assert.Contains("&lt;div&gt;", encoded);
            Assert.Contains("&amp;", encoded);
            Assert.Equal(original, decoded);
        }

        // ==================== HASH TESTS ====================
        [Fact]
        public void ComputeHashes_ShouldMatchKnownStandards()
        {
            // Arrange
            string text = "hello world";

            // Act
            string md5 = _service.ComputeMd5(text);
            string sha1 = _service.ComputeSha1(text);
            string sha256 = _service.ComputeSha256(text);
            string sha512 = _service.ComputeSha512(text);

            // Assert
            Assert.Equal("5eb63bbbe01eeed093cb22bb8f5acdc3", md5);
            Assert.Equal("2aae6c35c94fcfb415dbe95f408b9ce91ee846ed", sha1);
            Assert.Equal("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", sha256);
            Assert.StartsWith("309ecc489c12d6eb4cc40f50c902f2b4d0ed77ee511a7c7a9bcd3ca86d4cd86f", sha512);
        }

        [Fact]
        public void ComputeFileHash_ShouldComputeAccurateHash()
        {
            // Arrange
            string tempFile = Path.Combine(Path.GetTempPath(), $"hash_test_{Guid.NewGuid():N}.txt");
            File.WriteAllText(tempFile, "hello world");

            try
            {
                // Act
                string fileSha256 = _service.ComputeFileHash(tempFile, "SHA256");

                // Assert
                // Note: File.WriteAllText by default emits UTF8 without BOM in modern .NET
                Assert.Equal("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", fileSha256);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        // ==================== CASE & STATS TESTS ====================
        [Fact]
        public void CaseConversions_ShouldConvertAccurately()
        {
            // Arrange
            string input = "hello world test";

            // Act & Assert
            Assert.Equal("helloWorldTest", _service.ToCamelCase(input));
            Assert.Equal("HelloWorldTest", _service.ToPascalCase(input));
            Assert.Equal("hello_world_test", _service.ToSnakeCase(input));
            Assert.Equal("hello-world-test", _service.ToKebabCase(input));
            Assert.Equal("HELLO WORLD TEST", _service.ToUpperCase(input));
            Assert.Equal("hello world test", _service.ToLowerCase(input));
            Assert.Equal("Hello World Test", _service.ToTitleCase(input));
        }

        [Fact]
        public void AnalyzeText_ShouldReturnAccurateStatistics()
        {
            // Arrange
            string text = "Hello World\nLine 2";

            // Act
            var stats = _service.AnalyzeText(text);

            // Assert
            Assert.Equal(18, stats.Characters);
            Assert.Equal(15, stats.CharactersNoSpaces); // 18 minus 2 spaces minus 1 newline = 15
            Assert.Equal(4, stats.Words); // "Hello", "World", "Line", "2"
            Assert.Equal(2, stats.Lines);
            Assert.Equal(18, stats.ByteCount);
        }

        [Fact]
        public void AnalyzeText_EmptyInput_ShouldReturnZeroes()
        {
            // Act
            var stats = _service.AnalyzeText("");

            // Assert
            Assert.Equal(0, stats.Characters);
            Assert.Equal(0, stats.Words);
            Assert.Equal(0, stats.Lines);
        }

        // ==================== VIEWMODEL INTEGRATION ====================
        [Fact]
        public void ViewModel_FormatJson_ShouldPopulateOutput()
        {
            // Arrange
            var vm = new TextUtilityViewModel(_service);
            vm.JsonInput = "{\"test\": 123}";

            // Act
            vm.FormatJson();

            // Assert
            Assert.Contains("\n", vm.JsonOutput);
            Assert.True(vm.IsJsonValid);
        }

        [Fact]
        public void ViewModel_LiveHashGeneration_ShouldUpdateWhenInputChanges()
        {
            // Arrange
            var vm = new TextUtilityViewModel(_service);

            // Act
            vm.HashInput = "hello world";

            // Assert
            Assert.Equal("5eb63bbbe01eeed093cb22bb8f5acdc3", vm.Md5Hash);
            Assert.Equal("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", vm.Sha256Hash);
        }
    }
}
