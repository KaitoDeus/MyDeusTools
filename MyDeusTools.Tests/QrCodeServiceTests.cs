using System;
using System.IO;
using MyDeusTools.App.Services;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class QrCodeServiceTests
    {
        [Fact]
        public void GenerateQrCodePng_ShouldReturnValidPngBytes()
        {
            // Arrange
            var service = new QrCodeService();
            string text = "Hello MyDeusTools";

            // Act
            byte[] pngBytes = service.GenerateQrCodePng(text);

            // Assert
            Assert.NotNull(pngBytes);
            Assert.True(pngBytes.Length > 0);
            // PNG signature header bytes: 0x89, 'P', 'N', 'G'
            Assert.Equal(0x89, pngBytes[0]);
            Assert.Equal(0x50, pngBytes[1]);
            Assert.Equal(0x4E, pngBytes[2]);
            Assert.Equal(0x47, pngBytes[3]);
        }

        [WpfFact]
        public void GenerateQrCode_ShouldReturnValidBitmapSource()
        {
            // Arrange
            var service = new QrCodeService();
            string text = "https://mydeustools.local";

            // Act
            var bitmap = service.GenerateQrCode(text);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.PixelWidth > 0);
            Assert.True(bitmap.PixelHeight > 0);
        }

        [Fact]
        public void GenerateQrCode_EmptyContent_ShouldThrowArgumentException()
        {
            // Arrange
            var service = new QrCodeService();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.GenerateQrCode("   "));
        }

        [WpfFact]
        public void GenerateAndDecode_Roundtrip_ShouldMatchOriginalText()
        {
            // Arrange
            var service = new QrCodeService();
            string expectedText = "Test-QR-Content-123456789";

            // Act: Generate QR image
            var bitmap = service.GenerateQrCode(expectedText, pixelsPerModule: 20);

            // Act: Decode the generated QR image
            string? decodedText = service.DecodeQrCode(bitmap);

            // Assert
            Assert.Equal(expectedText, decodedText);
        }

        [WpfFact]
        public void DecodeQrCodeFromFile_ShouldDecodeSavedImage()
        {
            // Arrange
            var service = new QrCodeService();
            string expectedText = "https://github.com/KaitoDeus/MyDeusTools";
            string tempFile = Path.Combine(Path.GetTempPath(), $"test_qr_{Guid.NewGuid():N}.png");

            try
            {
                byte[] bytes = service.GenerateQrCodePng(expectedText, pixelsPerModule: 20);
                File.WriteAllBytes(tempFile, bytes);

                // Act
                string? decodedText = service.DecodeQrCodeFromFile(tempFile);

                // Assert
                Assert.Equal(expectedText, decodedText);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        [Fact]
        public void DecodeQrCode_NullBitmap_ShouldReturnNull()
        {
            // Arrange
            var service = new QrCodeService();

            // Act
            string? result = service.DecodeQrCode(null!);

            // Assert
            Assert.Null(result);
        }

        [WpfFact]
        public void QrCodeViewModel_GenerateQr_ShouldUpdateProperties()
        {
            // Arrange
            var service = new QrCodeService();
            var vm = new QrCodeViewModel(service);

            // Act
            vm.InputText = "New QR Content";

            // Assert
            Assert.NotNull(vm.GeneratedImage);
            Assert.True(vm.HasGeneratedImage);
        }

        [WpfFact]
        public void QrCodeViewModel_DetectsUrlCorrectly()
        {
            // Arrange
            var service = new QrCodeService();
            var vm = new QrCodeViewModel(service);

            // Act 1: Plain text
            vm.ScannedResult = "Just simple text";
            Assert.False(vm.IsUrl);

            // Act 2: Valid HTTPS URL
            vm.ScannedResult = "https://google.com";
            Assert.True(vm.IsUrl);
        }
    }
}
