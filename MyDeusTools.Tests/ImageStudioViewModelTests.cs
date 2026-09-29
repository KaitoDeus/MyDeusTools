using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class ImageStudioViewModelTests : IDisposable
    {
        private readonly string _testDir;
        private readonly ImageStudioService _service;
        private readonly ImageStudioViewModel _viewModel;

        public ImageStudioViewModelTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"imagestudiovm_tests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
            _service = new ImageStudioService();
            _viewModel = new ImageStudioViewModel(_service);
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

        private string CreateSamplePng(string fileName, int width = 50, int height = 50)
        {
            string path = Path.Combine(_testDir, fileName);
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var fs = File.Create(path);
            encoder.Save(fs);
            return path;
        }

        [Fact]
        public void InitialState_HasEmptyQueueAndDefaultOptions()
        {
            Assert.Empty(_viewModel.Files);
            Assert.Equal(ImageTargetFormat.WebP, _viewModel.TargetFormat);
            Assert.Equal(80, _viewModel.Quality);
            Assert.Equal(ImageResizeMode.Original, _viewModel.ResizeMode);
            Assert.True(_viewModel.KeepAspectRatio);
            Assert.True(_viewModel.StripExifMetadata);
            Assert.False(_viewModel.IsProcessing);
            Assert.Equal(0, _viewModel.TotalFilesCount);
        }

        [Fact]
        public void AddFilePaths_AddsDistinctFiles_UpdatesStats()
        {
            string f1 = CreateSamplePng("test1.png");
            string f2 = CreateSamplePng("test2.png");

            _viewModel.AddFilePaths(new[] { f1, f2, f1 }); // includes duplicate

            Assert.Equal(2, _viewModel.Files.Count);
            Assert.Equal(2, _viewModel.TotalFilesCount);
            Assert.Equal("test1.png", _viewModel.Files[0].FileName);
            Assert.Equal("test2.png", _viewModel.Files[1].FileName);
        }

        [Fact]
        public void RemoveFileCommand_RemovesItemFromQueue()
        {
            string f1 = CreateSamplePng("a.png");
            string f2 = CreateSamplePng("b.png");
            _viewModel.AddFilePaths(new[] { f1, f2 });

            var itemToRemove = _viewModel.Files[0];
            _viewModel.RemoveFileCommand.Execute(itemToRemove);

            Assert.Single(_viewModel.Files);
            Assert.Equal("b.png", _viewModel.Files[0].FileName);
            Assert.Equal(1, _viewModel.TotalFilesCount);
        }

        [Fact]
        public void ClearFilesCommand_ClearsAllFilesAndResetsProgress()
        {
            string f1 = CreateSamplePng("x.png");
            _viewModel.AddFilePaths(new[] { f1 });
            _viewModel.OverallProgressPercent = 50;

            _viewModel.ClearFilesCommand.Execute(null);

            Assert.Empty(_viewModel.Files);
            Assert.Equal(0, _viewModel.TotalFilesCount);
            Assert.Equal(0, _viewModel.OverallProgressPercent);
        }

        [Fact]
        public void QuickQualityCommand_SetsQualityWithinValidBounds()
        {
            _viewModel.QuickQualityCommand.Execute("60");
            Assert.Equal(60, _viewModel.Quality);

            _viewModel.QuickQualityCommand.Execute("100");
            Assert.Equal(100, _viewModel.Quality);

            _viewModel.QuickQualityCommand.Execute("999");
            Assert.Equal(100, _viewModel.Quality);
        }

        [Fact]
        public void QuickResizePresetCommand_AppliesPresetCorrectly()
        {
            _viewModel.QuickResizePresetCommand.Execute("Avatar_256");
            Assert.Equal(ImageResizeMode.CustomDimensions, _viewModel.ResizeMode);
            Assert.Equal(256, _viewModel.TargetWidth);
            Assert.Equal(256, _viewModel.TargetHeight);

            _viewModel.QuickResizePresetCommand.Execute("HD_720");
            Assert.Equal(1280, _viewModel.TargetWidth);
            Assert.Equal(720, _viewModel.TargetHeight);

            _viewModel.QuickResizePresetCommand.Execute("Percent_50");
            Assert.Equal(ImageResizeMode.Percentage, _viewModel.ResizeMode);
            Assert.Equal(50, _viewModel.ResizePercent);
        }

        [Fact]
        public void FormatFileSize_FormatsUnitsProperly()
        {
            Assert.Equal("500 B", ImageItemViewModel.FormatFileSize(500));
            Assert.Equal("1.5 KB", ImageItemViewModel.FormatFileSize(1536));
            Assert.Equal("2.00 MB", ImageItemViewModel.FormatFileSize(2 * 1024 * 1024));
            Assert.Equal("1.50 GB", ImageItemViewModel.FormatFileSize((long)(1.5 * 1024 * 1024 * 1024)));
        }

        [Fact]
        public void ImageItemViewModel_UpdateResult_UpdatesProperties()
        {
            var item = new ImageItemViewModel { FilePath = "sample.png" };
            var result = new ImageItemResult
            {
                Status = ImageItemStatus.Completed,
                OutputPath = "sample_min.jpg",
                OutputWidth = 800,
                OutputHeight = 600,
                OutputSizeBytes = 102400,
                ReductionPercent = 45.5
            };

            item.UpdateResult(result);

            Assert.Equal(ImageItemStatus.Completed, item.Status);
            Assert.Equal("800 × 600", item.OutputDimensions);
            Assert.Equal("-45.5%", item.ReductionDisplay);
            Assert.Equal("Hoàn tất", item.StatusDisplay);
        }

        [Fact]
        public async Task StartConversionAsync_EmptyQueue_SetsStatusMessage()
        {
            await _viewModel.StartConversionAsync();
            Assert.Contains("ít nhất một ảnh", _viewModel.StatusMessage);
        }

        [Fact]
        public async Task StartConversionAsync_ValidQueue_ProcessesImages()
        {
            string f1 = CreateSamplePng("img1.png", 60, 40);
            _viewModel.AddFilePaths(new[] { f1 });
            _viewModel.TargetFormat = ImageTargetFormat.Jpeg;
            _viewModel.FileNameSuffix = "_test_out";

            await _viewModel.StartConversionAsync();

            Assert.Equal(ImageItemStatus.Completed, _viewModel.Files[0].Status);
            Assert.NotNull(_viewModel.Files[0].OutputPath);
            Assert.True(File.Exists(_viewModel.Files[0].OutputPath));
            Assert.Equal(100.0, _viewModel.OverallProgressPercent);
        }
    }
}
