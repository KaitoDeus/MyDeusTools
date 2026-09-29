using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class ImageStudioService : IImageStudioService
    {
        private readonly IVideoConverterService? _videoConverterService;
        private string? _ffmpegPath;

        public bool IsWebPAvailable => !string.IsNullOrEmpty(FfmpegPath) && File.Exists(FfmpegPath);

        public string? FfmpegPath
        {
            get
            {
                if (_videoConverterService != null && _videoConverterService.IsEngineAvailable)
                {
                    return _videoConverterService.FfmpegPath;
                }
                return _ffmpegPath ?? FindFfmpegFallback();
            }
        }

        public ImageStudioService(IVideoConverterService? videoConverterService = null)
        {
            _videoConverterService = videoConverterService;
            _ffmpegPath = FindFfmpegFallback();
        }

        private static string? FindFfmpegFallback()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidatePaths =
            {
                Path.Combine(appDir, "ffmpeg.exe"),
                Path.Combine(appDir, "Tools", "ffmpeg.exe"),
                Path.Combine(appDir, "Data", "ffmpeg", "ffmpeg.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyDeusTools", "ffmpeg", "ffmpeg.exe")
            };

            foreach (var path in candidatePaths)
            {
                if (File.Exists(path)) return path;
            }

            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                string[] folders = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var folder in folders)
                {
                    try
                    {
                        string combined = Path.Combine(folder.Trim(), "ffmpeg.exe");
                        if (File.Exists(combined)) return combined;
                    }
                    catch { }
                }
            }

            return null;
        }

        public ImageItemMetadata? GetImageMetadata(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);

                if (decoder.Frames.Count > 0)
                {
                    var frame = decoder.Frames[0];
                    return new ImageItemMetadata
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        Width = frame.PixelWidth,
                        Height = frame.PixelHeight,
                        SizeBytes = fileInfo.Length,
                        Format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant()
                    };
                }
            }
            catch
            {
                // Nếu WIC decoder không đọc được (ví dụ một số file WebP trên máy chưa cài codec), thử lấy size tệp
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    return new ImageItemMetadata
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        Width = 0,
                        Height = 0,
                        SizeBytes = fileInfo.Length,
                        Format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant()
                    };
                }
                catch { }
            }

            return null;
        }

        public (int width, int height) CalculateDimensions(
            int origWidth,
            int origHeight,
            ImageResizeMode mode,
            int percent,
            int targetWidth,
            int targetHeight,
            bool keepAspectRatio)
        {
            if (origWidth <= 0) origWidth = 1;
            if (origHeight <= 0) origHeight = 1;

            switch (mode)
            {
                case ImageResizeMode.Percentage:
                    int p = Math.Clamp(percent, 1, 1000);
                    int pw = Math.Max(1, (int)Math.Round(origWidth * (p / 100.0)));
                    int ph = Math.Max(1, (int)Math.Round(origHeight * (p / 100.0)));
                    return (pw, ph);

                case ImageResizeMode.ByWidth:
                    int bw = Math.Max(1, targetWidth);
                    int bh = keepAspectRatio
                        ? Math.Max(1, (int)Math.Round((double)origHeight * bw / origWidth))
                        : origHeight;
                    return (bw, bh);

                case ImageResizeMode.ByHeight:
                    int ah = Math.Max(1, targetHeight);
                    int aw = keepAspectRatio
                        ? Math.Max(1, (int)Math.Round((double)origWidth * ah / origHeight))
                        : origWidth;
                    return (aw, ah);

                case ImageResizeMode.CustomDimensions:
                    int cw = Math.Max(1, targetWidth);
                    int ch = Math.Max(1, targetHeight);
                    if (!keepAspectRatio)
                    {
                        return (cw, ch);
                    }
                    double scale = Math.Min((double)cw / origWidth, (double)ch / origHeight);
                    int fitW = Math.Max(1, (int)Math.Round(origWidth * scale));
                    int fitH = Math.Max(1, (int)Math.Round(origHeight * scale));
                    return (fitW, fitH);

                case ImageResizeMode.Original:
                default:
                    return (origWidth, origHeight);
            }
        }

        public string BuildOutputFilePath(string sourcePath, ImageConvertOptions options)
        {
            string dir = !string.IsNullOrWhiteSpace(options.OutputDirectory)
                ? options.OutputDirectory
                : Path.GetDirectoryName(sourcePath) ?? AppDomain.CurrentDomain.BaseDirectory;

            string ext = options.TargetFormat switch
            {
                ImageTargetFormat.WebP => ".webp",
                ImageTargetFormat.Png => ".png",
                ImageTargetFormat.Jpeg => ".jpg",
                ImageTargetFormat.Ico => ".ico",
                ImageTargetFormat.Bmp => ".bmp",
                ImageTargetFormat.Tiff => ".tif",
                _ => ".png"
            };

            string nameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
            string suffix = options.FileNameSuffix ?? string.Empty;
            string targetName = $"{nameWithoutExt}{suffix}{ext}";
            string fullPath = Path.Combine(dir, targetName);

            if (!options.OverwriteExisting && File.Exists(fullPath))
            {
                int counter = 1;
                while (File.Exists(fullPath))
                {
                    targetName = $"{nameWithoutExt}{suffix}_{counter}{ext}";
                    fullPath = Path.Combine(dir, targetName);
                    counter++;
                }
            }

            return fullPath;
        }

        public async Task<ImageItemResult> ConvertSingleImageAsync(
            string sourcePath,
            ImageConvertOptions options,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                var result = new ImageItemResult
                {
                    SourcePath = sourcePath,
                    Status = ImageItemStatus.Processing
                };

                if (!File.Exists(sourcePath))
                {
                    result.Status = ImageItemStatus.Failed;
                    result.ErrorMessage = "Tệp nguồn không tồn tại.";
                    return result;
                }

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    string outputPath = BuildOutputFilePath(sourcePath, options);
                    result.OutputPath = outputPath;

                    string? outDir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                    {
                        Directory.CreateDirectory(outDir);
                    }

                    // Nếu đích đến là WebP và có FFmpeg
                    if (options.TargetFormat == ImageTargetFormat.WebP)
                    {
                        if (!IsWebPAvailable)
                        {
                            throw new InvalidOperationException("Định dạng WebP yêu cầu công cụ FFmpeg. Vui lòng tải FFmpeg ở mục Video Converter hoặc chọn PNG/JPEG/ICO.");
                        }

                        await ConvertViaFfmpegWebPAsync(sourcePath, outputPath, options, cancellationToken);
                    }
                    else
                    {
                        // Xử lý hoàn toàn bằng WPF Native Pipeline (PNG, JPEG, ICO, BMP, TIFF)
                        ConvertViaWpfNative(sourcePath, outputPath, options, cancellationToken);
                    }

                    var outInfo = new FileInfo(outputPath);
                    var srcInfo = new FileInfo(sourcePath);

                    result.OutputSizeBytes = outInfo.Length;
                    result.Status = ImageItemStatus.Completed;

                    if (srcInfo.Length > 0)
                    {
                        double savedRatio = (1.0 - ((double)outInfo.Length / srcInfo.Length)) * 100.0;
                        result.ReductionPercent = Math.Round(savedRatio, 1);
                    }

                    // Đọc lại kích thước đầu ra
                    var outMeta = GetImageMetadata(outputPath);
                    if (outMeta != null)
                    {
                        result.OutputWidth = outMeta.Width;
                        result.OutputHeight = outMeta.Height;
                    }

                    return result;
                }
                catch (OperationCanceledException)
                {
                    result.Status = ImageItemStatus.Canceled;
                    result.ErrorMessage = "Đã hủy bởi người dùng.";
                    return result;
                }
                catch (Exception ex)
                {
                    result.Status = ImageItemStatus.Failed;
                    result.ErrorMessage = ex.Message;
                    return result;
                }
            }, cancellationToken);
        }

        public async Task<IReadOnlyList<ImageItemResult>> ConvertBatchAsync(
            IReadOnlyList<string> sourcePaths,
            ImageConvertOptions options,
            IProgress<ImageBatchProgress>? progress = null,
            Action<int, ImageItemResult>? itemCompleted = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<ImageItemResult>();
            long totalOriginalBytes = 0;
            long totalOutputBytes = 0;

            for (int i = 0; i < sourcePaths.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string path = sourcePaths[i];
                var srcInfo = File.Exists(path) ? new FileInfo(path) : null;
                if (srcInfo != null) totalOriginalBytes += srcInfo.Length;

                progress?.Report(new ImageBatchProgress
                {
                    CurrentIndex = i + 1,
                    TotalCount = sourcePaths.Count,
                    CurrentFileName = Path.GetFileName(path),
                    Percent = Math.Round(((double)i / sourcePaths.Count) * 100, 1),
                    TotalOriginalBytes = totalOriginalBytes,
                    TotalOutputBytes = totalOutputBytes
                });

                var res = await ConvertSingleImageAsync(path, options, cancellationToken);
                results.Add(res);

                if (res.Status == ImageItemStatus.Completed && res.OutputSizeBytes > 0)
                {
                    totalOutputBytes += res.OutputSizeBytes;
                }

                itemCompleted?.Invoke(i, res);
            }

            progress?.Report(new ImageBatchProgress
            {
                CurrentIndex = sourcePaths.Count,
                TotalCount = sourcePaths.Count,
                CurrentFileName = "Hoàn tất",
                Percent = 100.0,
                TotalOriginalBytes = totalOriginalBytes,
                TotalOutputBytes = totalOutputBytes
            });

            return results;
        }

        private void ConvertViaWpfNative(
            string sourcePath,
            string outputPath,
            ImageConvertOptions options,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 1. Tải ảnh gốc vào BitmapImage an toàn (OnLoad để giải phóng khóa tệp)
            var srcBitmap = new BitmapImage();
            using (var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                srcBitmap.BeginInit();
                srcBitmap.CacheOption = BitmapCacheOption.OnLoad;
                srcBitmap.StreamSource = stream;
                srcBitmap.EndInit();
            }
            srcBitmap.Freeze();

            cancellationToken.ThrowIfCancellationRequested();

            int origW = srcBitmap.PixelWidth;
            int origH = srcBitmap.PixelHeight;

            var (targetW, targetH) = CalculateDimensions(
                origW, origH,
                options.ResizeMode,
                options.ResizePercent,
                options.TargetWidth,
                options.TargetHeight,
                options.KeepAspectRatio);

            // 2. Nếu là tạo ICO
            if (options.TargetFormat == ImageTargetFormat.Ico)
            {
                byte[] icoBytes = GenerateMultiLayerIco(srcBitmap, options.IcoSizes, options.RotateFlip, cancellationToken);
                File.WriteAllBytes(outputPath, icoBytes);
                return;
            }

            // 3. Áp dụng Biến đổi (Scaling & Rotation)
            BitmapSource transformed = ApplyTransformations(srcBitmap, targetW, targetH, options.RotateFlip);

            // 4. Chọn Bộ mã hóa (Encoder)
            BitmapEncoder encoder = options.TargetFormat switch
            {
                ImageTargetFormat.Jpeg => new JpegBitmapEncoder
                {
                    QualityLevel = Math.Clamp(options.Quality, 1, 100)
                },
                ImageTargetFormat.Png => new PngBitmapEncoder(),
                ImageTargetFormat.Bmp => new BmpBitmapEncoder(),
                ImageTargetFormat.Tiff => new TiffBitmapEncoder(),
                _ => new PngBitmapEncoder()
            };

            // Nếu StripExifMetadata = true, tạo BitmapFrame thuần không truyền BitmapMetadata
            BitmapFrame frame = BitmapFrame.Create(transformed);
            encoder.Frames.Add(frame);

            using var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(outStream);
        }

        private static BitmapSource ApplyTransformations(
            BitmapSource source,
            int targetWidth,
            int targetHeight,
            ImageRotateFlip rotateFlip)
        {
            double scaleX = (double)targetWidth / source.PixelWidth;
            double scaleY = (double)targetHeight / source.PixelHeight;

            var transformGroup = new TransformGroup();

            if (Math.Abs(scaleX - 1.0) > 0.001 || Math.Abs(scaleY - 1.0) > 0.001)
            {
                transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
            }

            switch (rotateFlip)
            {
                case ImageRotateFlip.Rotate90:
                    transformGroup.Children.Add(new RotateTransform(90));
                    break;
                case ImageRotateFlip.Rotate180:
                    transformGroup.Children.Add(new RotateTransform(180));
                    break;
                case ImageRotateFlip.Rotate270:
                    transformGroup.Children.Add(new RotateTransform(270));
                    break;
                case ImageRotateFlip.FlipHorizontal:
                    transformGroup.Children.Add(new ScaleTransform(-1, 1));
                    break;
                case ImageRotateFlip.FlipVertical:
                    transformGroup.Children.Add(new ScaleTransform(1, -1));
                    break;
            }

            if (transformGroup.Children.Count == 0)
            {
                return source;
            }

            var transformed = new TransformedBitmap(source, transformGroup);
            transformed.Freeze();
            return transformed;
        }

        public static byte[] GenerateMultiLayerIco(
            BitmapSource source,
            int[]? sizes = null,
            ImageRotateFlip rotateFlip = ImageRotateFlip.None,
            CancellationToken cancellationToken = default)
        {
            sizes ??= new[] { 16, 32, 48, 64, 128, 256 };
            var pngFrames = new List<(int size, byte[] data)>();

            foreach (int size in sizes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (size <= 0 || size > 256) continue;

                var transformed = ApplyTransformations(source, size, size, rotateFlip);
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(transformed));

                using var ms = new MemoryStream();
                pngEncoder.Save(ms);
                pngFrames.Add((size, ms.ToArray()));
            }

            if (pngFrames.Count == 0)
            {
                throw new InvalidOperationException("Không có kích thước ICO hợp lệ nào được chỉ định.");
            }

            using var outStream = new MemoryStream();
            using var writer = new BinaryWriter(outStream);

            // 1. ICONDIR Header (6 bytes)
            writer.Write((ushort)0); // Reserved, must be 0
            writer.Write((ushort)1); // Image type: 1 = ICO
            writer.Write((ushort)pngFrames.Count); // Number of images

            // 2. ICONDIRENTRY (16 bytes per entry)
            int offset = 6 + (16 * pngFrames.Count);
            for (int i = 0; i < pngFrames.Count; i++)
            {
                int sz = pngFrames[i].size;
                byte dim = sz >= 256 ? (byte)0 : (byte)sz;

                writer.Write(dim);                // Width (0 means 256)
                writer.Write(dim);                // Height (0 means 256)
                writer.Write((byte)0);            // Color count (0 for 32bpp)
                writer.Write((byte)0);            // Reserved
                writer.Write((ushort)1);          // Color planes
                writer.Write((ushort)32);         // Bits per pixel
                writer.Write((uint)pngFrames[i].data.Length); // Image size in bytes
                writer.Write((uint)offset);       // Image data offset

                offset += pngFrames[i].data.Length;
            }

            // 3. Image Data (PNG streams)
            for (int i = 0; i < pngFrames.Count; i++)
            {
                writer.Write(pngFrames[i].data);
            }

            return outStream.ToArray();
        }

        private async Task ConvertViaFfmpegWebPAsync(
            string sourcePath,
            string outputPath,
            ImageConvertOptions options,
            CancellationToken cancellationToken)
        {
            string ffmpeg = FfmpegPath ?? throw new InvalidOperationException("FFmpeg không khả dụng.");

            var (tw, th) = CalculateDimensions(
                options.TargetWidth > 0 ? options.TargetWidth : 1920,
                options.TargetHeight > 0 ? options.TargetHeight : 1080,
                options.ResizeMode,
                options.ResizePercent,
                options.TargetWidth,
                options.TargetHeight,
                options.KeepAspectRatio);

            var vfList = new List<string>();

            if (options.ResizeMode != ImageResizeMode.Original)
            {
                vfList.Add($"scale={tw}:{th}:flags=lanczos");
            }

            switch (options.RotateFlip)
            {
                case ImageRotateFlip.Rotate90:
                    vfList.Add("transpose=1");
                    break;
                case ImageRotateFlip.Rotate180:
                    vfList.Add("transpose=2,transpose=2");
                    break;
                case ImageRotateFlip.Rotate270:
                    vfList.Add("transpose=2");
                    break;
                case ImageRotateFlip.FlipHorizontal:
                    vfList.Add("hflip");
                    break;
                case ImageRotateFlip.FlipVertical:
                    vfList.Add("vflip");
                    break;
            }

            string vfArg = vfList.Count > 0 ? $"-vf \"{string.Join(",", vfList)}\"" : "";
            int quality = Math.Clamp(options.Quality, 1, 100);

            string arguments = $"-y -i \"{sourcePath}\" {vfArg} -c:v libwebp -quality {quality} \"{outputPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

            using var reg = cancellationToken.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(true); } catch { }
            });

            await process.WaitForExitAsync(cancellationToken);

            string err = await errorTask;
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"FFmpeg WebP lỗi ({process.ExitCode}): {err}");
            }
        }
    }
}
