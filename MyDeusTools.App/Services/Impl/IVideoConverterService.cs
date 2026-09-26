using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyDeusTools.App.Services.Impl
{
    public enum VideoFormat
    {
        // Video Containers
        Mp4,
        Mkv,
        Webm,
        Avi,
        Mov,
        Wmv,
        Gif,
        Flv,

        // Audio Extraction
        Mp3,
        Wav,
        Aac,
        M4a,
        Ogg,
        Flac
    }

    public enum VideoResolution
    {
        Original,
        Uhd4K,      // 3840x2160
        Qhd2K,      // 2560x1440
        Fhd1080p,   // 1920x1080
        Hd720p,     // 1280x720
        Sd480p,     // 854x480
        Low360p     // 640x360
    }

    public enum VideoCodec
    {
        Auto,
        H264,       // libx264
        H265,       // libx265 (HEVC)
        Vp9,        // libvpx-vp9
        Copy        // Lossless stream copy
    }

    public enum VideoQuality
    {
        UltraHigh,  // CRF 18
        High,       // CRF 21 (Default)
        Medium,     // CRF 25
        Compact     // CRF 28 (Small file size)
    }

    public enum VideoFrameRate
    {
        Original,
        Fps60,
        Fps30,
        Fps24
    }

    public enum AudioOption
    {
        Keep,       // Original / auto
        ConvertAac, // AAC
        ConvertMp3, // MP3
        Mute        // Remove audio (-an)
    }

    public enum AudioBitrate
    {
        Kbps128,
        Kbps192,
        Kbps256,
        Kbps320
    }

    public class VideoConversionOptions
    {
        public string InputFilePath { get; set; } = string.Empty;
        public string OutputFilePath { get; set; } = string.Empty;
        public VideoFormat TargetFormat { get; set; } = VideoFormat.Mp4;
        public VideoResolution Resolution { get; set; } = VideoResolution.Original;
        public VideoCodec Codec { get; set; } = VideoCodec.Auto;
        public VideoQuality Quality { get; set; } = VideoQuality.High;
        public VideoFrameRate FrameRate { get; set; } = VideoFrameRate.Original;
        public AudioOption Audio { get; set; } = AudioOption.Keep;
        public AudioBitrate AudioBitrate { get; set; } = AudioBitrate.Kbps192;
        public bool EnableTrimming { get; set; } = false;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool UseGpuAcceleration { get; set; } = false;
    }

    public class MediaInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string DurationFormatted { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string ResolutionFormatted => (Width > 0 && Height > 0) ? $"{Width}x{Height}" : "N/A";
        public string VideoCodec { get; set; } = "N/A";
        public string AudioCodec { get; set; } = "N/A";
        public double FrameRate { get; set; }
        public long BitrateKbps { get; set; }
    }

    public class ConversionProgress
    {
        public double Percentage { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public string CurrentTimeFormatted { get; set; } = "00:00:00";
        public string TotalTimeFormatted { get; set; } = "00:00:00";
        public string Speed { get; set; } = "1.0x";
        public double Fps { get; set; }
        public TimeSpan? EstimatedRemaining { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }

    public interface IVideoConverterService
    {
        bool IsEngineAvailable { get; }
        string? FfmpegPath { get; }
        string EngineVersion { get; }

        event Action? EngineStatusChanged;

        bool CheckEngine();
        void SetCustomFfmpegPath(string path);
        Task<MediaInfo?> ProbeMediaInfoAsync(string filePath, CancellationToken ct = default);
        Task<bool> ConvertAsync(VideoConversionOptions options, IProgress<ConversionProgress>? progress = null, Action<string>? logCallback = null, CancellationToken ct = default);
        Task<bool> DownloadEngineAsync(IProgress<double>? progress = null, CancellationToken ct = default);
        string BuildFfmpegArguments(VideoConversionOptions options, TimeSpan? totalDuration = null);
        ConversionProgress ParseProgressLine(string line, TimeSpan totalDuration);
        MediaInfo ParseProbeOutput(string ffmpegOutput, string filePath);
    }
}
