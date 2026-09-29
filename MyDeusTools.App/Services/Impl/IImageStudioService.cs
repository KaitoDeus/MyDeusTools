using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyDeusTools.App.Services.Impl
{
    public enum ImageTargetFormat
    {
        WebP,
        Png,
        Jpeg,
        Ico,
        Bmp,
        Tiff
    }

    public enum ImageResizeMode
    {
        Original,
        Percentage,
        ByWidth,
        ByHeight,
        CustomDimensions
    }

    public enum ImageRotateFlip
    {
        None,
        Rotate90,
        Rotate180,
        Rotate270,
        FlipHorizontal,
        FlipVertical
    }

    public enum ImageItemStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Canceled
    }

    public class ImageConvertOptions
    {
        public ImageTargetFormat TargetFormat { get; set; } = ImageTargetFormat.WebP;
        public int Quality { get; set; } = 80; // 1 - 100
        public bool ProgressiveJpeg { get; set; } = true;
        public ImageResizeMode ResizeMode { get; set; } = ImageResizeMode.Original;
        public int ResizePercent { get; set; } = 100;
        public int TargetWidth { get; set; } = 1920;
        public int TargetHeight { get; set; } = 1080;
        public bool KeepAspectRatio { get; set; } = true;
        public ImageRotateFlip RotateFlip { get; set; } = ImageRotateFlip.None;
        public bool StripExifMetadata { get; set; } = true;
        public string? OutputDirectory { get; set; }
        public string? FileNameSuffix { get; set; } = "_min";
        public bool OverwriteExisting { get; set; } = true;
        public int[] IcoSizes { get; set; } = new[] { 16, 32, 48, 64, 128, 256 };
    }

    public class ImageItemMetadata
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long SizeBytes { get; set; }
        public string Format { get; set; } = string.Empty;
    }

    public class ImageItemResult
    {
        public string SourcePath { get; set; } = string.Empty;
        public string? OutputPath { get; set; }
        public ImageItemStatus Status { get; set; } = ImageItemStatus.Pending;
        public int OutputWidth { get; set; }
        public int OutputHeight { get; set; }
        public long OutputSizeBytes { get; set; }
        public double ReductionPercent { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ImageBatchProgress
    {
        public int CurrentIndex { get; set; }
        public int TotalCount { get; set; }
        public string CurrentFileName { get; set; } = string.Empty;
        public double Percent { get; set; }
        public long TotalOriginalBytes { get; set; }
        public long TotalOutputBytes { get; set; }
    }

    public interface IImageStudioService
    {
        bool IsWebPAvailable { get; }
        string? FfmpegPath { get; }

        ImageItemMetadata? GetImageMetadata(string filePath);

        Task<ImageItemResult> ConvertSingleImageAsync(
            string sourcePath,
            ImageConvertOptions options,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ImageItemResult>> ConvertBatchAsync(
            IReadOnlyList<string> sourcePaths,
            ImageConvertOptions options,
            IProgress<ImageBatchProgress>? progress = null,
            Action<int, ImageItemResult>? itemCompleted = null,
            CancellationToken cancellationToken = default);

        string BuildOutputFilePath(string sourcePath, ImageConvertOptions options);

        (int width, int height) CalculateDimensions(
            int origWidth,
            int origHeight,
            ImageResizeMode mode,
            int percent,
            int targetWidth,
            int targetHeight,
            bool keepAspectRatio);
    }
}
