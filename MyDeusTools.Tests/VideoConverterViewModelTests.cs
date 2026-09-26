using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.ViewModels;
using Xunit;

namespace MyDeusTools.Tests
{
    public class VideoConverterViewModelTests
    {
        [Fact]
        public void SelectedFormat_ChangeToMkv_ShouldUpdateOutputFileExtension()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object);
            vm.InputFilePath = "C:\\Videos\\my_vacation.mp4";

            // Act
            vm.SelectedFormat = VideoFormat.Mkv;

            // Assert
            Assert.EndsWith("_converted.mkv", vm.OutputFileName);
            Assert.False(vm.IsAudioOnlyFormat);
            Assert.False(vm.IsGifFormat);
        }

        [Fact]
        public void SelectedFormat_ChangeToMp3_ShouldSetIsAudioOnlyFormatTrue()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object);
            vm.InputFilePath = "C:\\Videos\\podcast.mp4";

            // Act
            vm.SelectedFormat = VideoFormat.Mp3;

            // Assert
            Assert.EndsWith("_converted.mp3", vm.OutputFileName);
            Assert.True(vm.IsAudioOnlyFormat);
            Assert.False(vm.IsGifFormat);
        }

        [Fact]
        public void SelectedFormat_ChangeToGif_ShouldSetIsGifFormatTrue()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object);
            vm.InputFilePath = "C:\\Videos\\reaction.mp4";

            // Act
            vm.SelectedFormat = VideoFormat.Gif;

            // Assert
            Assert.EndsWith("_converted.gif", vm.OutputFileName);
            Assert.False(vm.IsAudioOnlyFormat);
            Assert.True(vm.IsGifFormat);
        }

        [Theory]
        [InlineData("Mp4", VideoFormat.Mp4)]
        [InlineData("Mkv", VideoFormat.Mkv)]
        [InlineData("Webm", VideoFormat.Webm)]
        [InlineData("Avi", VideoFormat.Avi)]
        [InlineData("Mov", VideoFormat.Mov)]
        [InlineData("Wmv", VideoFormat.Wmv)]
        [InlineData("Gif", VideoFormat.Gif)]
        [InlineData("Mp3", VideoFormat.Mp3)]
        [InlineData("Wav", VideoFormat.Wav)]
        [InlineData("Flac", VideoFormat.Flac)]
        public void QuickSelectFormat_ShouldSetCorrespondingEnum(string formatName, VideoFormat expectedFormat)
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object);

            // Act
            vm.QuickSelectFormatCommand.Execute(formatName);

            // Assert
            Assert.Equal(expectedFormat, vm.SelectedFormat);
        }

        [Fact]
        public void ClearInput_ShouldResetMediaProperties()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object)
            {
                InputFilePath = "C:\\test\\sample.mp4",
                InputFileName = "sample.mp4",
                InputFileSizeFormatted = "25.0 MB",
                DurationFormatted = "00:05:00",
                ResolutionFormatted = "1920x1080",
                VideoCodecFormatted = "H264",
                AudioCodecFormatted = "AAC"
            };

            // Act
            vm.ClearInputCommand.Execute(null);

            // Assert
            Assert.Empty(vm.InputFilePath);
            Assert.Empty(vm.InputFileName);
            Assert.Empty(vm.InputFileSizeFormatted);
            Assert.Equal("--:--:--", vm.DurationFormatted);
            Assert.Equal("--", vm.ResolutionFormatted);
            Assert.Equal("--", vm.VideoCodecFormatted);
            Assert.Equal("--", vm.AudioCodecFormatted);
            Assert.False(vm.HasInputFile);
            Assert.False(vm.CanStartConversion);
        }

        [Fact]
        public async Task StartConversion_WhenNoInputFile_ShouldSetWarningStatus()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object);

            // Act
            await vm.StartConversionCommand.ExecuteAsync(null);

            // Assert
            Assert.Contains("chọn tệp video nguồn", vm.StatusMessage);
            mockService.Verify(s => s.ConvertAsync(It.IsAny<VideoConversionOptions>(), It.IsAny<IProgress<ConversionProgress>>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StartConversion_WhenEngineUnavailable_ShouldSetWarningStatus()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            mockService.SetupGet(s => s.IsEngineAvailable).Returns(false);

            // Create temporary test file
            string tempFile = Path.Combine(Path.GetTempPath(), $"mydeustools_test_{Guid.NewGuid():N}.mp4");
            File.WriteAllText(tempFile, "fake video data");

            try
            {
                var vm = new VideoConverterViewModel(mockService.Object)
                {
                    InputFilePath = tempFile
                };

                // Act
                await vm.StartConversionCommand.ExecuteAsync(null);

                // Assert
                Assert.Contains("bộ máy FFmpeg", vm.StatusMessage);
                mockService.Verify(s => s.ConvertAsync(It.IsAny<VideoConversionOptions>(), It.IsAny<IProgress<ConversionProgress>>(), It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void ToggleLog_ShouldInvertIsLogExpanded()
        {
            // Arrange
            var mockService = new Mock<IVideoConverterService>();
            var vm = new VideoConverterViewModel(mockService.Object);
            Assert.False(vm.IsLogExpanded);

            // Act 1
            vm.ToggleLogCommand.Execute(null);
            Assert.True(vm.IsLogExpanded);

            // Act 2
            vm.ToggleLogCommand.Execute(null);
            Assert.False(vm.IsLogExpanded);
        }

        [Theory]
        [InlineData(VideoFormat.Mp4, ".mp4")]
        [InlineData(VideoFormat.Mkv, ".mkv")]
        [InlineData(VideoFormat.Webm, ".webm")]
        [InlineData(VideoFormat.Avi, ".avi")]
        [InlineData(VideoFormat.Mov, ".mov")]
        [InlineData(VideoFormat.Wmv, ".wmv")]
        [InlineData(VideoFormat.Gif, ".gif")]
        [InlineData(VideoFormat.Flv, ".flv")]
        [InlineData(VideoFormat.Mp3, ".mp3")]
        [InlineData(VideoFormat.Wav, ".wav")]
        [InlineData(VideoFormat.Aac, ".aac")]
        [InlineData(VideoFormat.M4a, ".m4a")]
        [InlineData(VideoFormat.Ogg, ".ogg")]
        [InlineData(VideoFormat.Flac, ".flac")]
        public void GetExtensionForFormat_ShouldReturnCorrectExtension(VideoFormat format, string expectedExt)
        {
            string ext = VideoConverterViewModel.GetExtensionForFormat(format);
            Assert.Equal(expectedExt, ext);
        }
    }
}
