using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class VideoConverterService : IVideoConverterService
    {
        private readonly string _customPathSettingsFile;
        private string? _ffmpegPath;
        private string _engineVersion = "Chưa phát hiện";
        private readonly object _lock = new();

        public bool IsEngineAvailable => !string.IsNullOrEmpty(_ffmpegPath) && File.Exists(_ffmpegPath);
        public string? FfmpegPath => _ffmpegPath;
        public string EngineVersion => _engineVersion;

        public event Action? EngineStatusChanged;

        public VideoConverterService(string? customSettingsDir = null)
        {
            string baseDir = customSettingsDir ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(baseDir))
            {
                try { Directory.CreateDirectory(baseDir); } catch { }
            }
            _customPathSettingsFile = Path.Combine(baseDir, "ffmpeg_path.txt");

            CheckEngine();
        }

        public bool CheckEngine()
        {
            lock (_lock)
            {
                // 1. Kiểm tra đường dẫn tùy chỉnh đã lưu
                if (File.Exists(_customPathSettingsFile))
                {
                    try
                    {
                        string savedPath = File.ReadAllText(_customPathSettingsFile).Trim();
                        if (File.Exists(savedPath))
                        {
                            _ffmpegPath = savedPath;
                            DetectVersion();
                            EngineStatusChanged?.Invoke();
                            return true;
                        }
                    }
                    catch { }
                }

                // 2. Thư mục cài đặt ứng dụng
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] candidatePaths =
                {
                    Path.Combine(appDir, "ffmpeg.exe"),
                    Path.Combine(appDir, "Tools", "ffmpeg.exe"),
                    Path.Combine(appDir, "Data", "ffmpeg", "ffmpeg.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyDeusTools", "ffmpeg", "ffmpeg.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ffmpeg", "bin", "ffmpeg.exe")
                };

                foreach (var path in candidatePaths)
                {
                    if (File.Exists(path))
                    {
                        _ffmpegPath = path;
                        DetectVersion();
                        EngineStatusChanged?.Invoke();
                        return true;
                    }
                }

                // 3. Kiểm tra biến môi trường PATH
                string? pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathEnv))
                {
                    string[] folders = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var folder in folders)
                    {
                        try
                        {
                            string combined = Path.Combine(folder.Trim(), "ffmpeg.exe");
                            if (File.Exists(combined))
                            {
                                _ffmpegPath = combined;
                                DetectVersion();
                                EngineStatusChanged?.Invoke();
                                return true;
                            }
                        }
                        catch { }
                    }
                }

                _ffmpegPath = null;
                _engineVersion = "Chưa phát hiện";
                EngineStatusChanged?.Invoke();
                return false;
            }
        }

        public void SetCustomFfmpegPath(string path)
        {
            if (File.Exists(path))
            {
                _ffmpegPath = path;
                try
                {
                    string? dir = Path.GetDirectoryName(_customPathSettingsFile);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(_customPathSettingsFile, path);
                }
                catch { }

                DetectVersion();
                EngineStatusChanged?.Invoke();
            }
        }

        private void DetectVersion()
        {
            if (string.IsNullOrEmpty(_ffmpegPath) || !File.Exists(_ffmpegPath)) return;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadLine() ?? "";
                    process.WaitForExit(1500);

                    // Output mẫu: "ffmpeg version 7.1-full_build-www.gyan.dev Copyright (c)..."
                    var match = Regex.Match(output, @"ffmpeg version (\S+)");
                    if (match.Success)
                    {
                        _engineVersion = "v" + match.Groups[1].Value;
                    }
                    else if (!string.IsNullOrWhiteSpace(output))
                    {
                        _engineVersion = output.Length > 25 ? output.Substring(0, 25) + "..." : output;
                    }
                }
            }
            catch
            {
                _engineVersion = "Sẵn sàng";
            }
        }

        public async Task<MediaInfo?> ProbeMediaInfoAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(filePath)) return null;

            var info = new MediaInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                var fi = new FileInfo(filePath);
                info.FileSizeBytes = fi.Length;
                info.FileSizeFormatted = FormatFileSize(fi.Length);
            }
            catch { }

            if (!IsEngineAvailable)
            {
                return info;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffmpegPath!,
                    Arguments = $"-i \"{filePath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    string stderr = await process.StandardError.ReadToEndAsync(ct);
                    await process.WaitForExitAsync(ct);

                    return ParseProbeOutput(stderr, filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi probe video: {ex.Message}");
            }

            return info;
        }

        public MediaInfo ParseProbeOutput(string ffmpegOutput, string filePath)
        {
            var info = new MediaInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                if (File.Exists(filePath))
                {
                    var fi = new FileInfo(filePath);
                    info.FileSizeBytes = fi.Length;
                    info.FileSizeFormatted = FormatFileSize(fi.Length);
                }
            }
            catch { }

            // 1. Duration: 00:01:23.45, start: 0.000000, bitrate: 2450 kb/s
            var durationMatch = Regex.Match(ffmpegOutput, @"Duration:\s*(\d{2}):(\d{2}):(\d{2}(?:\.\d+)?)");
            if (durationMatch.Success)
            {
                int hours = int.Parse(durationMatch.Groups[1].Value);
                int minutes = int.Parse(durationMatch.Groups[2].Value);
                double seconds = double.Parse(durationMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                info.Duration = TimeSpan.FromSeconds(hours * 3600 + minutes * 60 + seconds);
                info.DurationFormatted = $"{(int)info.Duration.TotalHours:D2}:{info.Duration.Minutes:D2}:{info.Duration.Seconds:D2}";
            }

            // Bitrate
            var bitrateMatch = Regex.Match(ffmpegOutput, @"bitrate:\s*(\d+)\s*kb/s");
            if (bitrateMatch.Success)
            {
                info.BitrateKbps = long.Parse(bitrateMatch.Groups[1].Value);
            }

            // 2. Video Stream: Stream #0:0: Video: h264 (...), ..., 1920x1080 ..., 30 fps
            var videoMatch = Regex.Match(ffmpegOutput, @"Stream.*Video:\s*([a-zA-Z0-9_\-]+)");
            if (videoMatch.Success)
            {
                info.VideoCodec = videoMatch.Groups[1].Value.ToUpperInvariant();
            }

            // Resolution: 1920x1080
            var resMatch = Regex.Match(ffmpegOutput, @"(\d{3,4})x(\d{3,4})");
            if (resMatch.Success)
            {
                info.Width = int.Parse(resMatch.Groups[1].Value);
                info.Height = int.Parse(resMatch.Groups[2].Value);
            }

            // FPS: 30 fps or 29.97 fps
            var fpsMatch = Regex.Match(ffmpegOutput, @"(\d+(?:\.\d+)?)\s*fps");
            if (fpsMatch.Success)
            {
                info.FrameRate = double.Parse(fpsMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            // 3. Audio Stream: Stream #0:1: Audio: aac ...
            var audioMatch = Regex.Match(ffmpegOutput, @"Stream.*Audio:\s*([a-zA-Z0-9_\-]+)");
            if (audioMatch.Success)
            {
                info.AudioCodec = audioMatch.Groups[1].Value.ToUpperInvariant();
            }

            return info;
        }

        public string BuildFfmpegArguments(VideoConversionOptions options, TimeSpan? totalDuration = null)
        {
            var args = new System.Collections.Generic.List<string>();

            // Luôn ghi đè tệp đầu ra không hỏi
            args.Add("-y");

            // Tăng tốc phần cứng nếu được kích hoạt
            if (options.UseGpuAcceleration)
            {
                args.Add("-hwaccel auto");
            }

            // Cắt thời gian bắt đầu trước input (-ss) để seek nhanh
            if (options.EnableTrimming && options.StartTime.HasValue && options.StartTime.Value > TimeSpan.Zero)
            {
                args.Add($"-ss {options.StartTime.Value:hh\\:mm\\:ss}");
            }

            // Tệp đầu vào
            args.Add($"-i \"{options.InputFilePath}\"");

            // Cắt thời gian kết thúc (-to)
            if (options.EnableTrimming && options.EndTime.HasValue && options.EndTime.Value > TimeSpan.Zero)
            {
                args.Add($"-to {options.EndTime.Value:hh\\:mm\\:ss}");
            }

            bool isAudioOnly = options.TargetFormat is VideoFormat.Mp3 or VideoFormat.Wav or VideoFormat.Aac or VideoFormat.M4a or VideoFormat.Ogg or VideoFormat.Flac;

            if (isAudioOnly)
            {
                // Bỏ luồng video (-vn)
                args.Add("-vn");

                string audioBitrateStr = GetAudioBitrateString(options.AudioBitrate);

                switch (options.TargetFormat)
                {
                    case VideoFormat.Mp3:
                        args.Add($"-c:a libmp3lame -b:a {audioBitrateStr}");
                        break;
                    case VideoFormat.Wav:
                        args.Add("-c:a pcm_s16le");
                        break;
                    case VideoFormat.Aac:
                    case VideoFormat.M4a:
                        args.Add($"-c:a aac -b:a {audioBitrateStr}");
                        break;
                    case VideoFormat.Ogg:
                        args.Add($"-c:a libvorbis -b:a {audioBitrateStr}");
                        break;
                    case VideoFormat.Flac:
                        args.Add("-c:a flac");
                        break;
                }
            }
            else if (options.TargetFormat == VideoFormat.Gif)
            {
                // Xử lý chuyển đổi sang ảnh động GIF
                int gifFps = options.FrameRate switch
                {
                    VideoFrameRate.Fps60 => 30,
                    VideoFrameRate.Fps30 => 24,
                    VideoFrameRate.Fps24 => 18,
                    _ => 15
                };

                string scaleFilter = options.Resolution switch
                {
                    VideoResolution.Uhd4K => "scale=1920:-1:flags=lanczos",
                    VideoResolution.Qhd2K => "scale=1280:-1:flags=lanczos",
                    VideoResolution.Fhd1080p => "scale=960:-1:flags=lanczos",
                    VideoResolution.Hd720p => "scale=640:-1:flags=lanczos",
                    VideoResolution.Sd480p => "scale=480:-1:flags=lanczos",
                    VideoResolution.Low360p => "scale=360:-1:flags=lanczos",
                    _ => "scale=480:-1:flags=lanczos"
                };

                // Palettegen + Paletteuse để GIF mượt mà, không vỡ hạt
                args.Add($"-vf \"fps={gifFps},{scaleFilter},split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\"");
            }
            else
            {
                // Xử lý Video (MP4, MKV, WebM, AVI, MOV, WMV, FLV)
                if (options.Codec == VideoCodec.Copy)
                {
                    args.Add("-c:v copy");
                }
                else
                {
                    // Video Codec
                    string vCodec = options.Codec switch
                    {
                        VideoCodec.H264 => "libx264",
                        VideoCodec.H265 => "libx265",
                        VideoCodec.Vp9 => "libvpx-vp9",
                        _ => options.TargetFormat switch
                        {
                            VideoFormat.Webm => "libvpx-vp9",
                            VideoFormat.Wmv => "wmv2",
                            VideoFormat.Avi => "mpeg4",
                            _ => "libx264"
                        }
                    };
                    args.Add($"-c:v {vCodec}");

                    // CRF Quality
                    if (vCodec is "libx264" or "libx265")
                    {
                        int crf = options.Quality switch
                        {
                            VideoQuality.UltraHigh => 18,
                            VideoQuality.High => 21,
                            VideoQuality.Medium => 25,
                            VideoQuality.Compact => 28,
                            _ => 21
                        };
                        args.Add($"-crf {crf}");
                        args.Add("-preset faster");
                    }
                    else if (vCodec is "libvpx-vp9")
                    {
                        int crf = options.Quality switch
                        {
                            VideoQuality.UltraHigh => 20,
                            VideoQuality.High => 28,
                            VideoQuality.Medium => 34,
                            VideoQuality.Compact => 40,
                            _ => 28
                        };
                        args.Add($"-crf {crf} -b:v 0");
                    }

                    // Resolution scaling
                    string? scaleStr = options.Resolution switch
                    {
                        VideoResolution.Uhd4K => "scale=3840:-2",
                        VideoResolution.Qhd2K => "scale=2560:-2",
                        VideoResolution.Fhd1080p => "scale=1920:-2",
                        VideoResolution.Hd720p => "scale=1280:-2",
                        VideoResolution.Sd480p => "scale=854:-2",
                        VideoResolution.Low360p => "scale=640:-2",
                        _ => null
                    };

                    if (!string.IsNullOrEmpty(scaleStr))
                    {
                        args.Add($"-vf \"{scaleStr}\"");
                    }

                    // Frame rate
                    int? targetFps = options.FrameRate switch
                    {
                        VideoFrameRate.Fps60 => 60,
                        VideoFrameRate.Fps30 => 30,
                        VideoFrameRate.Fps24 => 24,
                        _ => null
                    };

                    if (targetFps.HasValue)
                    {
                        args.Add($"-r {targetFps.Value}");
                    }
                }

                // Audio configuration
                if (options.Audio == AudioOption.Mute)
                {
                    args.Add("-an");
                }
                else if (options.Audio == AudioOption.ConvertAac)
                {
                    args.Add($"-c:a aac -b:a {GetAudioBitrateString(options.AudioBitrate)}");
                }
                else if (options.Audio == AudioOption.ConvertMp3)
                {
                    args.Add($"-c:a libmp3lame -b:a {GetAudioBitrateString(options.AudioBitrate)}");
                }
                else
                {
                    // Keep / Auto
                    if (options.TargetFormat == VideoFormat.Webm)
                    {
                        args.Add("-c:a libopus -b:a 128k");
                    }
                    else if (options.TargetFormat == VideoFormat.Wmv)
                    {
                        args.Add("-c:a wmav2");
                    }
                    else
                    {
                        args.Add("-c:a copy");
                    }
                }
            }

            // Tệp đích đầu ra
            args.Add($"\"{options.OutputFilePath}\"");

            return string.Join(" ", args);
        }

        public ConversionProgress ParseProgressLine(string line, TimeSpan totalDuration)
        {
            var progress = new ConversionProgress();

            if (totalDuration > TimeSpan.Zero)
            {
                progress.TotalTimeFormatted = $"{(int)totalDuration.TotalHours:D2}:{totalDuration.Minutes:D2}:{totalDuration.Seconds:D2}";
            }

            // time=00:00:15.30
            var timeMatch = Regex.Match(line, @"time=(\d{2}):(\d{2}):(\d{2}(?:\.\d+)?)");
            if (timeMatch.Success)
            {
                int hours = int.Parse(timeMatch.Groups[1].Value);
                int minutes = int.Parse(timeMatch.Groups[2].Value);
                double seconds = double.Parse(timeMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                var currentTime = TimeSpan.FromSeconds(hours * 3600 + minutes * 60 + seconds);
                progress.CurrentTime = currentTime;
                progress.CurrentTimeFormatted = $"{(int)currentTime.TotalHours:D2}:{currentTime.Minutes:D2}:{currentTime.Seconds:D2}";

                if (totalDuration > TimeSpan.Zero)
                {
                    double pct = (currentTime.TotalSeconds / totalDuration.TotalSeconds) * 100.0;
                    progress.Percentage = Math.Clamp(Math.Round(pct, 1), 0.0, 100.0);
                }
            }

            // speed=1.85x
            var speedMatch = Regex.Match(line, @"speed=\s*([0-9\.]+)x");
            if (speedMatch.Success)
            {
                progress.Speed = speedMatch.Groups[1].Value + "x";
                if (double.TryParse(speedMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double speedVal) && speedVal > 0 && totalDuration > progress.CurrentTime)
                {
                    double remainingSeconds = (totalDuration.TotalSeconds - progress.CurrentTime.TotalSeconds) / speedVal;
                    progress.EstimatedRemaining = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
                }
            }

            // fps=45.2
            var fpsMatch = Regex.Match(line, @"fps=\s*([0-9\.]+)");
            if (fpsMatch.Success && double.TryParse(fpsMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double fpsVal))
            {
                progress.Fps = fpsVal;
            }

            progress.StatusText = $"{progress.Percentage:0.0}% • Tốc độ: {progress.Speed}";
            if (progress.EstimatedRemaining.HasValue)
            {
                var r = progress.EstimatedRemaining.Value;
                progress.StatusText += $" • Còn lại: {(int)r.TotalMinutes:D2}:{r.Seconds:D2}";
            }

            return progress;
        }

        public async Task<bool> ConvertAsync(
            VideoConversionOptions options,
            IProgress<ConversionProgress>? progress = null,
            Action<string>? logCallback = null,
            CancellationToken ct = default)
        {
            if (!IsEngineAvailable)
            {
                throw new InvalidOperationException("Bộ máy chuyển đổi FFmpeg chưa sẵn sàng!");
            }

            if (!File.Exists(options.InputFilePath))
            {
                throw new FileNotFoundException("Không tìm thấy tệp video nguồn!", options.InputFilePath);
            }

            // Đảm bảo thư mục lưu tồn tại
            string? outDir = Path.GetDirectoryName(options.OutputFilePath);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            // Lấy thời lượng video nguồn để tính toán % tiến trình
            var mediaInfo = await ProbeMediaInfoAsync(options.InputFilePath, ct);
            TimeSpan duration = mediaInfo?.Duration ?? TimeSpan.Zero;
            if (options.EnableTrimming && options.StartTime.HasValue && options.EndTime.HasValue && options.EndTime > options.StartTime)
            {
                duration = options.EndTime.Value - options.StartTime.Value;
            }

            string arguments = BuildFfmpegArguments(options, duration);
            logCallback?.Invoke($"[LỆNH] ffmpeg {arguments}");

            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath!,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            try
            {
                process.Start();

                // Đọc stderr để bắt các dòng tiến trình và log
                var readErrorTask = Task.Run(async () =>
                {
                    using var reader = process.StandardError;
                    string? line;
                    while ((line = await reader.ReadLineAsync(ct)) != null)
                    {
                        logCallback?.Invoke(line);

                        if (line.Contains("time=") || line.Contains("frame="))
                        {
                            var prog = ParseProgressLine(line, duration);
                            progress?.Report(prog);
                        }
                    }
                }, ct);

                // Đăng ký hủy bỏ tiến trình
                using (ct.Register(() =>
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                        }
                    }
                    catch { }
                }))
                {
                    await process.WaitForExitAsync(ct);
                    await readErrorTask;
                }

                if (process.ExitCode == 0 && File.Exists(options.OutputFilePath))
                {
                    // Báo cáo hoàn thành 100%
                    progress?.Report(new ConversionProgress
                    {
                        Percentage = 100,
                        CurrentTime = duration,
                        CurrentTimeFormatted = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}",
                        TotalTimeFormatted = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}",
                        Speed = "1.0x",
                        StatusText = "Chuyển đổi hoàn tất thành công 100%!"
                    });
                    return true;
                }

                return false;
            }
            catch (OperationCanceledException)
            {
                // Dọn dẹp tệp dở dang nếu người dùng nhấn Hủy
                try
                {
                    if (File.Exists(options.OutputFilePath))
                    {
                        File.Delete(options.OutputFilePath);
                    }
                }
                catch { }
                throw;
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"[LỖI] {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DownloadEngineAsync(IProgress<double>? progress = null, CancellationToken ct = default)
        {
            // Tải bản ffmpeg essential Windows static
            string downloadUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
            string targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyDeusTools", "ffmpeg");
            string zipPath = Path.Combine(targetFolder, "ffmpeg_download.zip");

            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;
                using var contentStream = await response.Content.ReadAsStreamAsync(ct);
                using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read, ct);
                    totalRead += read;
                    if (totalBytes.HasValue && totalBytes.Value > 0)
                    {
                        double pct = (double)totalRead / totalBytes.Value * 100.0;
                        progress?.Report(Math.Round(pct, 1));
                    }
                }

                fileStream.Close();

                // Giải nén ffmpeg.exe từ zip
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            string destFile = Path.Combine(targetFolder, "ffmpeg.exe");
                            entry.ExtractToFile(destFile, overwrite: true);
                            break;
                        }
                    }
                }

                // Xóa tệp zip tạm
                try { File.Delete(zipPath); } catch { }

                string finalFfmpeg = Path.Combine(targetFolder, "ffmpeg.exe");
                if (File.Exists(finalFfmpeg))
                {
                    SetCustomFfmpegPath(finalFfmpeg);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tải bộ máy FFmpeg: {ex.Message}");
                try { if (File.Exists(zipPath)) File.Delete(zipPath); } catch { }
                return false;
            }
        }

        private static string GetAudioBitrateString(AudioBitrate bitrate) => bitrate switch
        {
            AudioBitrate.Kbps128 => "128k",
            AudioBitrate.Kbps192 => "192k",
            AudioBitrate.Kbps256 => "256k",
            AudioBitrate.Kbps320 => "320k",
            _ => "192k"
        };

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024 * 1024)
                return $"{(double)bytes / (1024 * 1024 * 1024):0.00} GB";
            if (bytes >= 1024 * 1024)
                return $"{(double)bytes / (1024 * 1024):0.00} MB";
            if (bytes >= 1024)
                return $"{(double)bytes / 1024:0.0} KB";
            return $"{bytes} B";
        }
    }
}
