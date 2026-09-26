using System;
using System.IO;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using Xunit;

namespace MyDeusTools.Tests
{
    public class VideoConverterServiceTests : IDisposable
    {
        private readonly string _testSettingsDir;

        public VideoConverterServiceTests()
        {
            _testSettingsDir = Path.Combine(Path.GetTempPath(), $"mydeustools_test_video_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testSettingsDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testSettingsDir))
            {
                try { Directory.Delete(_testSettingsDir, true); } catch { }
            }
        }

        [Fact]
        public void BuildFfmpegArguments_Mp4WithH264_ShouldIncludeCorrectFlags()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Videos\\sample.mov",
                OutputFilePath = "C:\\Videos\\sample_converted.mp4",
                TargetFormat = VideoFormat.Mp4,
                Codec = VideoCodec.H264,
                Quality = VideoQuality.High,
                Resolution = VideoResolution.Fhd1080p,
                FrameRate = VideoFrameRate.Fps60
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-y", args);
            Assert.Contains("-i \"C:\\Videos\\sample.mov\"", args);
            Assert.Contains("-c:v libx264", args);
            Assert.Contains("-crf 21", args);
            Assert.Contains("-preset faster", args);
            Assert.Contains("-vf \"scale=1920:-2\"", args);
            Assert.Contains("-r 60", args);
            Assert.Contains("\"C:\\Videos\\sample_converted.mp4\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_MkvWithH265AndUltraQuality_ShouldIncludeCrf18()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Videos\\test.mp4",
                OutputFilePath = "C:\\Videos\\test.mkv",
                TargetFormat = VideoFormat.Mkv,
                Codec = VideoCodec.H265,
                Quality = VideoQuality.UltraHigh,
                Resolution = VideoResolution.Uhd4K
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-c:v libx265", args);
            Assert.Contains("-crf 18", args);
            Assert.Contains("-vf \"scale=3840:-2\"", args);
            Assert.Contains("\"C:\\Videos\\test.mkv\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_WebmWithVp9_ShouldIncludeVp9AndOpusAudio()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Videos\\clip.mp4",
                OutputFilePath = "C:\\Videos\\clip.webm",
                TargetFormat = VideoFormat.Webm,
                Codec = VideoCodec.Auto,
                Resolution = VideoResolution.Hd720p
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-c:v libvpx-vp9", args);
            Assert.Contains("-c:a libopus", args);
            Assert.Contains("-vf \"scale=1280:-2\"", args);
            Assert.Contains("\"C:\\Videos\\clip.webm\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_StreamCopy_ShouldIncludeLosslessCopyFlags()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Videos\\input.mkv",
                OutputFilePath = "C:\\Videos\\output.mp4",
                TargetFormat = VideoFormat.Mp4,
                Codec = VideoCodec.Copy
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-c:v copy", args);
            Assert.Contains("-c:a copy", args);
        }

        [Fact]
        public void BuildFfmpegArguments_GifFormat_ShouldIncludePaletteFilter()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Videos\\animation.mp4",
                OutputFilePath = "C:\\Videos\\animation.gif",
                TargetFormat = VideoFormat.Gif,
                FrameRate = VideoFrameRate.Fps30,
                Resolution = VideoResolution.Sd480p
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("palettegen", args);
            Assert.Contains("paletteuse", args);
            Assert.Contains("fps=24", args);
            Assert.Contains("\"C:\\Videos\\animation.gif\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_AudioExtractionMp3_ShouldIncludeVnAndLameBitrate()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Music\\concert.mp4",
                OutputFilePath = "C:\\Music\\concert.mp3",
                TargetFormat = VideoFormat.Mp3,
                AudioBitrate = AudioBitrate.Kbps320
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-vn", args);
            Assert.Contains("-c:a libmp3lame", args);
            Assert.Contains("-b:a 320k", args);
            Assert.Contains("\"C:\\Music\\concert.mp3\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_AudioExtractionWav_ShouldIncludePcm()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Audio\\track.m4a",
                OutputFilePath = "C:\\Audio\\track.wav",
                TargetFormat = VideoFormat.Wav
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-vn", args);
            Assert.Contains("-c:a pcm_s16le", args);
            Assert.Contains("\"C:\\Audio\\track.wav\"", args);
        }

        [Fact]
        public void BuildFfmpegArguments_TrimmingAndMute_ShouldIncludeSsToAndAnFlags()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            var options = new VideoConversionOptions
            {
                InputFilePath = "C:\\Videos\\movie.mp4",
                OutputFilePath = "C:\\Videos\\clip.mp4",
                TargetFormat = VideoFormat.Mp4,
                EnableTrimming = true,
                StartTime = TimeSpan.FromSeconds(30),
                EndTime = TimeSpan.FromMinutes(2),
                Audio = AudioOption.Mute,
                UseGpuAcceleration = true
            };

            // Act
            string args = service.BuildFfmpegArguments(options);

            // Assert
            Assert.Contains("-hwaccel auto", args);
            Assert.Contains("-ss 00:00:30", args);
            Assert.Contains("-to 00:02:00", args);
            Assert.Contains("-an", args);
        }

        [Fact]
        public void ParseProgressLine_ShouldCalculatePercentageAndSpeedCorrectly()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            string ffmpegLine = "frame=  900 fps= 45.0 q=21.0 size=    4500kB time=00:00:30.00 bitrate=1228.8kbits/s speed=1.50x";
            TimeSpan totalDuration = TimeSpan.FromMinutes(1); // 60s total, current 30s -> 50%

            // Act
            var progress = service.ParseProgressLine(ffmpegLine, totalDuration);

            // Assert
            Assert.Equal(50.0, progress.Percentage);
            Assert.Equal(TimeSpan.FromSeconds(30), progress.CurrentTime);
            Assert.Equal("00:00:30", progress.CurrentTimeFormatted);
            Assert.Equal("00:01:00", progress.TotalTimeFormatted);
            Assert.Equal("1.50x", progress.Speed);
            Assert.Equal(45.0, progress.Fps);
            Assert.NotNull(progress.EstimatedRemaining);
            Assert.Equal(20, (int)progress.EstimatedRemaining.Value.TotalSeconds); // (60 - 30) / 1.5 = 20s
        }

        [Fact]
        public void ParseProbeOutput_ShouldExtractResolutionDurationAndCodecs()
        {
            // Arrange
            var service = new VideoConverterService(_testSettingsDir);
            string sampleOutput = @"
Input #0, mov,mp4,m4a,3gp,3g2,mj2, from 'sample.mp4':
  Metadata:
    major_brand     : isom
  Duration: 00:02:15.50, start: 0.000000, bitrate: 2450 kb/s
  Stream #0:0[0x1](und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 1920x1080 [SAR 1:1 DAR 16:9], 2250 kb/s, 30 fps, 30 tbr
  Stream #0:1[0x2](und): Audio: aac (LC) (mp4a / 0x6134706D), 48000 Hz, stereo, fltp, 192 kb/s
";

            // Act
            var info = service.ParseProbeOutput(sampleOutput, "sample.mp4");

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(135.5), info.Duration);
            Assert.Equal("00:02:15", info.DurationFormatted);
            Assert.Equal(1920, info.Width);
            Assert.Equal(1080, info.Height);
            Assert.Equal("1920x1080", info.ResolutionFormatted);
            Assert.Equal("H264", info.VideoCodec);
            Assert.Equal("AAC", info.AudioCodec);
            Assert.Equal(30.0, info.FrameRate);
            Assert.Equal(2450, info.BitrateKbps);
        }
    }
}
