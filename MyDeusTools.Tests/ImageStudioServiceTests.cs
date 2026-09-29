using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using Xunit;

namespace MyDeusTools.Tests
{
    public class ImageStudioServiceTests : IDisposable
    {
        private readonly string _testDir;
        private readonly ImageStudioService _service;

        public ImageStudioServiceTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"imagestudio_tests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
            _service = new ImageStudioService();
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch { }
        }

        private string CreateSamplePng(string fileName, int width = 100, int height = 80)
        {
            string path = Path.Combine(_testDir, fileName);
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;   // Blue
                pixels[i + 1] = 0; // Green
                pixels[i + 2] = 0; // Red
                pixels[i + 3] = 255; // Alpha
            }

            var bitmap = BitmapSource.Create(
                width, height, 96, 96,
                PixelFormats.Bgra32, null, pixels, stride);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var fs = File.Create(path);
            encoder.Save(fs);
            return path;
        }

        [Fact]
        public void CalculateDimensions_Original_ReturnsOriginal()
        {
            var (w, h) = _service.CalculateDimensions(1920, 1080, ImageResizeMode.Original, 100, 800, 600, true);
            Assert.Equal(1920, w);
            Assert.Equal(1080, h);
        }

        [Fact]
        public void CalculateDimensions_Percentage_ScalesCorrectly()
        {
            var (w, h) = _service.CalculateDimensions(1000, 500, ImageResizeMode.Percentage, 50, 0, 0, true);
            Assert.Equal(500, w);
            Assert.Equal(250, h);
        }

        [Fact]
        public void CalculateDimensions_ByWidth_KeepAspect_CalculatesHeight()
        {
            var (w, h) = _service.CalculateDimensions(1000, 500, ImageResizeMode.ByWidth, 100, 500, 0, true);
            Assert.Equal(500, w);
            Assert.Equal(250, h);
        }

        [Fact]
        public void CalculateDimensions_ByHeight_KeepAspect_CalculatesWidth()
        {
            var (w, h) = _service.CalculateDimensions(1000, 500, ImageResizeMode.ByHeight, 100, 0, 250, true);
            Assert.Equal(500, w);
            Assert.Equal(250, h);
        }

        [Fact]
        public void CalculateDimensions_CustomDimensions_FitWithinTarget_MaintainsAspect()
        {
            // 1000x500 (2:1 ratio) fit into 400x400 -> should be 400x200
            var (w, h) = _service.CalculateDimensions(1000, 500, ImageResizeMode.CustomDimensions, 100, 400, 400, true);
            Assert.Equal(400, w);
            Assert.Equal(200, h);
        }

        [Fact]
        public void CalculateDimensions_CustomDimensions_Stretch_IgnoresAspect()
        {
            var (w, h) = _service.CalculateDimensions(1000, 500, ImageResizeMode.CustomDimensions, 100, 400, 400, false);
            Assert.Equal(400, w);
            Assert.Equal(400, h);
        }

        [Fact]
        public void BuildOutputFilePath_DefaultOptions_AppendsSuffixAndExtension()
        {
            string src = Path.Combine(_testDir, "photo.png");
            var options = new ImageConvertOptions
            {
                TargetFormat = ImageTargetFormat.Jpeg,
                FileNameSuffix = "_optimized"
            };

            string outPath = _service.BuildOutputFilePath(src, options);
            Assert.Equal(Path.Combine(_testDir, "photo_optimized.jpg"), outPath);
        }

        [Fact]
        public void BuildOutputFilePath_CustomDirectory_UsesSpecifiedDir()
        {
            string src = Path.Combine(_testDir, "sample.bmp");
            string customDir = Path.Combine(_testDir, "export");
            var options = new ImageConvertOptions
            {
                TargetFormat = ImageTargetFormat.Ico,
                OutputDirectory = customDir,
                FileNameSuffix = ""
            };

            string outPath = _service.BuildOutputFilePath(src, options);
            Assert.Equal(Path.Combine(customDir, "sample.ico"), outPath);
        }

        [Fact]
        public void GetImageMetadata_ValidPng_ReturnsDimensionsAndFormat()
        {
            string sample = CreateSamplePng("test_meta.png", 120, 90);
            var meta = _service.GetImageMetadata(sample);

            Assert.NotNull(meta);
            Assert.Equal(120, meta.Width);
            Assert.Equal(90, meta.Height);
            Assert.Equal("PNG", meta.Format);
            Assert.True(meta.SizeBytes > 0);
        }

        [Fact]
        public async Task ConvertSingleImageAsync_NonExistentFile_ReturnsFailed()
        {
            var options = new ImageConvertOptions { TargetFormat = ImageTargetFormat.Png };
            var result = await _service.ConvertSingleImageAsync(Path.Combine(_testDir, "missing.jpg"), options);

            Assert.Equal(ImageItemStatus.Failed, result.Status);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task ConvertSingleImageAsync_PngToJpeg_Success()
        {
            string sample = CreateSamplePng("input.png", 200, 150);
            var options = new ImageConvertOptions
            {
                TargetFormat = ImageTargetFormat.Jpeg,
                Quality = 85,
                FileNameSuffix = "_out"
            };

            var result = await _service.ConvertSingleImageAsync(sample, options);

            Assert.Equal(ImageItemStatus.Completed, result.Status);
            Assert.NotNull(result.OutputPath);
            Assert.True(File.Exists(result.OutputPath));
            Assert.EndsWith(".jpg", result.OutputPath, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(200, result.OutputWidth);
            Assert.Equal(150, result.OutputHeight);
        }

        [Fact]
        public async Task ConvertSingleImageAsync_PngToIco_GeneratesMultiLayerIco()
        {
            string sample = CreateSamplePng("app_icon.png", 256, 256);
            var options = new ImageConvertOptions
            {
                TargetFormat = ImageTargetFormat.Ico,
                IcoSizes = new[] { 16, 32, 64 },
                FileNameSuffix = "_icon"
            };

            var result = await _service.ConvertSingleImageAsync(sample, options);

            Assert.Equal(ImageItemStatus.Completed, result.Status);
            Assert.NotNull(result.OutputPath);
            Assert.True(File.Exists(result.OutputPath));

            byte[] bytes = File.ReadAllBytes(result.OutputPath);
            Assert.True(bytes.Length > 22); // Header (6) + 3 entries (3*16=48) + data
            // Verify ICO header: Reserved = 0, Type = 1, Count = 3
            Assert.Equal(0, BitConverter.ToUInt16(bytes, 0));
            Assert.Equal(1, BitConverter.ToUInt16(bytes, 2));
            Assert.Equal(3, BitConverter.ToUInt16(bytes, 4));
        }

        [Fact]
        public async Task ConvertBatchAsync_MultipleImages_ConvertsAll()
        {
            string f1 = CreateSamplePng("batch1.png", 80, 80);
            string f2 = CreateSamplePng("batch2.png", 100, 100);

            var options = new ImageConvertOptions
            {
                TargetFormat = ImageTargetFormat.Png,
                ResizeMode = ImageResizeMode.Percentage,
                ResizePercent = 50,
                FileNameSuffix = "_half"
            };

            int progressReports = 0;
            var progress = new Progress<ImageBatchProgress>(_ => progressReports++);

            var results = await _service.ConvertBatchAsync(
                new[] { f1, f2 },
                options,
                progress);

            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.Equal(ImageItemStatus.Completed, r.Status));
            Assert.Equal(40, results[0].OutputWidth);
            Assert.Equal(50, results[1].OutputWidth);
        }
    }
}
