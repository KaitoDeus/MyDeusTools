using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class ImageItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private int _originalWidth;

        [ObservableProperty]
        private int _originalHeight;

        [ObservableProperty]
        private long _originalSizeBytes;

        [ObservableProperty]
        private string _originalDimensions = string.Empty;

        [ObservableProperty]
        private string _originalSizeFormatted = string.Empty;

        [ObservableProperty]
        private string _format = string.Empty;

        [ObservableProperty]
        private ImageItemStatus _status = ImageItemStatus.Pending;

        [ObservableProperty]
        private string _statusDisplay = "Chờ xử lý";

        [ObservableProperty]
        private string? _outputPath;

        [ObservableProperty]
        private int _outputWidth;

        [ObservableProperty]
        private int _outputHeight;

        [ObservableProperty]
        private string _outputDimensions = "-";

        [ObservableProperty]
        private long _outputSizeBytes;

        [ObservableProperty]
        private string _outputSizeFormatted = "-";

        [ObservableProperty]
        private double _reductionPercent;

        [ObservableProperty]
        private string _reductionDisplay = "-";

        [ObservableProperty]
        private string? _errorMessage;

        public void UpdateResult(ImageItemResult result)
        {
            Status = result.Status;
            OutputPath = result.OutputPath;
            OutputWidth = result.OutputWidth;
            OutputHeight = result.OutputHeight;
            OutputSizeBytes = result.OutputSizeBytes;
            ReductionPercent = result.ReductionPercent;
            ErrorMessage = result.ErrorMessage;

            if (result.OutputWidth > 0 && result.OutputHeight > 0)
            {
                OutputDimensions = $"{result.OutputWidth} × {result.OutputHeight}";
            }

            if (result.OutputSizeBytes > 0)
            {
                OutputSizeFormatted = FormatFileSize(result.OutputSizeBytes);
            }

            if (result.ReductionPercent != 0)
            {
                string p = result.ReductionPercent.ToString("F1", CultureInfo.InvariantCulture);
                ReductionDisplay = result.ReductionPercent > 0
                    ? $"-{p}%"
                    : $"+{Math.Abs(result.ReductionPercent).ToString("F1", CultureInfo.InvariantCulture)}%";
            }

            StatusDisplay = result.Status switch
            {
                ImageItemStatus.Pending => "Chờ xử lý",
                ImageItemStatus.Processing => "Đang nén...",
                ImageItemStatus.Completed => "Hoàn tất",
                ImageItemStatus.Failed => "Lỗi",
                ImageItemStatus.Canceled => "Đã hủy",
                _ => "-"
            };
        }

        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{(bytes / 1024.0).ToString("F1", CultureInfo.InvariantCulture)} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{(bytes / (1024.0 * 1024.0)).ToString("F2", CultureInfo.InvariantCulture)} MB";
            return $"{(bytes / (1024.0 * 1024.0 * 1024.0)).ToString("F2", CultureInfo.InvariantCulture)} GB";
        }
    }

    public partial class ImageStudioViewModel : ObservableObject
    {
        private readonly IImageStudioService _imageService;
        private readonly IVideoConverterService? _videoService;
        private readonly ILanguageService? _languageService;
        private CancellationTokenSource? _cts;

        private string GetLoc(string key, string fallback) =>
            _languageService?.GetString(key, fallback) ?? fallback;

        public ObservableCollection<ImageItemViewModel> Files { get; } = new();

        [ObservableProperty]
        private ImageItemViewModel? _selectedFile;

        [ObservableProperty]
        private ImageTargetFormat _targetFormat = ImageTargetFormat.WebP;

        [ObservableProperty]
        private int _quality = 80;

        [ObservableProperty]
        private ImageResizeMode _resizeMode = ImageResizeMode.Original;

        [ObservableProperty]
        private int _resizePercent = 100;

        [ObservableProperty]
        private int _targetWidth = 1920;

        [ObservableProperty]
        private int _targetHeight = 1080;

        [ObservableProperty]
        private bool _keepAspectRatio = true;

        [ObservableProperty]
        private ImageRotateFlip _rotateFlip = ImageRotateFlip.None;

        [ObservableProperty]
        private bool _stripExifMetadata = true;

        [ObservableProperty]
        private string? _outputDirectory;

        [ObservableProperty]
        private string _fileNameSuffix = "_min";

        [ObservableProperty]
        private bool _overwriteExisting = true;

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private double _overallProgressPercent;

        [ObservableProperty]
        private string _statusMessage = "Sẵn sàng thêm ảnh vào hàng đợi.";

        [ObservableProperty]
        private int _totalFilesCount;

        [ObservableProperty]
        private int _completedFilesCount;

        [ObservableProperty]
        private string _totalSavedDisplay = "0 B (0%)";

        public bool IsWebPAvailable => _imageService.IsWebPAvailable;

        public ImageStudioViewModel(
            IImageStudioService imageService,
            IVideoConverterService? videoService = null,
            ILanguageService? languageService = null)
        {
            _imageService = imageService;
            _videoService = videoService;
            _languageService = languageService;

            if (_languageService != null)
            {
                _languageService.LanguageChanged += _ =>
                {
                    if (StatusMessage == "Sẵn sàng thêm ảnh vào hàng đợi." || StatusMessage == "Ready to add images to queue.")
                    {
                        StatusMessage = GetLoc("Image_StatusReady", "Sẵn sàng thêm ảnh vào hàng đợi.");
                    }
                };
            }

            _statusMessage = GetLoc("Image_StatusReady", "Sẵn sàng thêm ảnh vào hàng đợi.");
        }

        [RelayCommand]
        public void AddFiles()
        {
            var dialog = new OpenFileDialog
            {
                Title = GetLoc("Image_DialogTitleFiles", "Chọn hình ảnh để nén hoặc chuyển đổi"),
                Multiselect = true,
                Filter = GetLoc("Image_DialogFilterFiles", "Tất cả hình ảnh|*.jpg;*.jpeg;*.png;*.webp;*.bmp;*.tif;*.tiff;*.gif;*.ico|PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|WebP Files (*.webp)|*.webp|Icon Files (*.ico)|*.ico|Tất cả tệp (*.*)|*.*")
            };

            if (dialog.ShowDialog() == true)
            {
                AddFilePaths(dialog.FileNames);
            }
        }

        [RelayCommand]
        public void AddFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Image_DialogTitleFolder", "Chọn thư mục chứa hình ảnh")
            };

            if (dialog.ShowDialog() == true && Directory.Exists(dialog.FolderName))
            {
                string[] validExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tif", ".tiff", ".gif", ".ico" };
                var files = Directory.EnumerateFiles(dialog.FolderName, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .ToArray();

                AddFilePaths(files);
            }
        }

        public void AddFilePaths(IEnumerable<string> filePaths)
        {
            int added = 0;
            foreach (var path in filePaths)
            {
                if (Files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var meta = _imageService.GetImageMetadata(path);
                var item = new ImageItemViewModel
                {
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    OriginalWidth = meta?.Width ?? 0,
                    OriginalHeight = meta?.Height ?? 0,
                    OriginalSizeBytes = meta?.SizeBytes ?? (File.Exists(path) ? new FileInfo(path).Length : 0),
                    Format = meta?.Format ?? Path.GetExtension(path).TrimStart('.').ToUpperInvariant(),
                    OriginalDimensions = meta != null && meta.Width > 0 ? $"{meta.Width} × {meta.Height}" : "N/A",
                    OriginalSizeFormatted = ImageItemViewModel.FormatFileSize(meta?.SizeBytes ?? 0)
                };

                Files.Add(item);
                added++;
            }

            UpdateStats();
            if (added > 0)
            {
                StatusMessage = string.Format(GetLoc("Image_StatusLoaded", "Đã nạp {0} ảnh mới vào hàng đợi."), added);
            }
        }

        [RelayCommand]
        public void RemoveFile(ImageItemViewModel? item)
        {
            if (item != null && Files.Contains(item))
            {
                Files.Remove(item);
                UpdateStats();
            }
        }

        [RelayCommand]
        public void ClearFiles()
        {
            Files.Clear();
            UpdateStats();
            OverallProgressPercent = 0;
            StatusMessage = GetLoc("Image_StatusCleared", "Đã dọn sạch danh sách ảnh.");
        }

        [RelayCommand]
        public void SelectOutputDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Image_DialogTitleOutput", "Chọn thư mục lưu ảnh xuất ra")
            };

            if (dialog.ShowDialog() == true)
            {
                OutputDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        public void ClearOutputDirectory()
        {
            OutputDirectory = null;
        }

        [RelayCommand]
        public void QuickQuality(string qualityStr)
        {
            if (int.TryParse(qualityStr, out int q))
            {
                Quality = Math.Clamp(q, 1, 100);
            }
        }

        [RelayCommand]
        public void QuickResizePreset(string preset)
        {
            switch (preset)
            {
                case "Avatar_256":
                    ResizeMode = ImageResizeMode.CustomDimensions;
                    TargetWidth = 256;
                    TargetHeight = 256;
                    KeepAspectRatio = true;
                    break;
                case "Thumb_640":
                    ResizeMode = ImageResizeMode.CustomDimensions;
                    TargetWidth = 640;
                    TargetHeight = 360;
                    KeepAspectRatio = true;
                    break;
                case "HD_720":
                    ResizeMode = ImageResizeMode.CustomDimensions;
                    TargetWidth = 1280;
                    TargetHeight = 720;
                    KeepAspectRatio = true;
                    break;
                case "FHD_1080":
                    ResizeMode = ImageResizeMode.CustomDimensions;
                    TargetWidth = 1920;
                    TargetHeight = 1080;
                    KeepAspectRatio = true;
                    break;
                case "QHD_1440":
                    ResizeMode = ImageResizeMode.CustomDimensions;
                    TargetWidth = 2560;
                    TargetHeight = 1440;
                    KeepAspectRatio = true;
                    break;
                case "UHD_4K":
                    ResizeMode = ImageResizeMode.CustomDimensions;
                    TargetWidth = 3840;
                    TargetHeight = 2160;
                    KeepAspectRatio = true;
                    break;
                case "Percent_50":
                    ResizeMode = ImageResizeMode.Percentage;
                    ResizePercent = 50;
                    break;
                case "Percent_75":
                    ResizeMode = ImageResizeMode.Percentage;
                    ResizePercent = 75;
                    break;
                case "Original":
                    ResizeMode = ImageResizeMode.Original;
                    break;
            }
        }

        [RelayCommand]
        public async Task StartConversionAsync()
        {
            if (Files.Count == 0)
            {
                StatusMessage = GetLoc("Image_StatusEmptyQueue", "Vui lòng thêm ít nhất một ảnh vào hàng đợi.");
                return;
            }

            if (TargetFormat == ImageTargetFormat.WebP && !IsWebPAvailable)
            {
                StatusMessage = GetLoc("Image_StatusWebPFFmpeg", "Định dạng WebP yêu cầu công cụ FFmpeg. Vui lòng tải FFmpeg hoặc đổi sang PNG/JPEG/ICO.");
                return;
            }

            IsProcessing = true;
            OverallProgressPercent = 0;
            CompletedFilesCount = 0;
            StatusMessage = GetLoc("Image_StatusBatchStarting", "Đang bắt đầu xử lý hàng loạt...");

            _cts = new CancellationTokenSource();

            var options = new ImageConvertOptions
            {
                TargetFormat = TargetFormat,
                Quality = Quality,
                ResizeMode = ResizeMode,
                ResizePercent = ResizePercent,
                TargetWidth = TargetWidth,
                TargetHeight = TargetHeight,
                KeepAspectRatio = KeepAspectRatio,
                RotateFlip = RotateFlip,
                StripExifMetadata = StripExifMetadata,
                OutputDirectory = OutputDirectory,
                FileNameSuffix = FileNameSuffix,
                OverwriteExisting = OverwriteExisting
            };

            var paths = Files.Select(f => f.FilePath).ToList();

            var progress = new Progress<ImageBatchProgress>(p =>
            {
                OverallProgressPercent = p.Percent;
                CompletedFilesCount = p.CurrentIndex;
                StatusMessage = string.Format(GetLoc("Image_StatusProcessingItem", "Đang xử lý ({0}/{1}): {2}"), p.CurrentIndex, p.TotalCount, p.CurrentFileName);
            });

            try
            {
                await _imageService.ConvertBatchAsync(
                    paths,
                    options,
                    progress,
                    (index, result) =>
                    {
                        if (index >= 0 && index < Files.Count)
                        {
                            Files[index].UpdateResult(result);
                        }
                    },
                    _cts.Token);

                UpdateStats();
                StatusMessage = string.Format(GetLoc("Image_StatusCompleted", "Hoàn tất xử lý {0} ảnh!"), Files.Count);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Image_StatusCanceledMsg", "Đã hủy tiến trình xử lý ảnh.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Image_StatusErrorMsg", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public void CancelConversion()
        {
            _cts?.Cancel();
            StatusMessage = GetLoc("Image_StatusCanceling", "Đang gửi yêu cầu hủy...");
        }

        [RelayCommand]
        public void OpenOutputFolder()
        {
            string? target = OutputDirectory;
            if (string.IsNullOrWhiteSpace(target) || !Directory.Exists(target))
            {
                var first = Files.FirstOrDefault(f => !string.IsNullOrEmpty(f.OutputPath));
                if (first?.OutputPath != null)
                {
                    target = Path.GetDirectoryName(first.OutputPath);
                }
            }

            if (!string.IsNullOrEmpty(target) && Directory.Exists(target))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });
            }
            else
            {
                StatusMessage = GetLoc("Image_StatusInvalidDir", "Chưa có thư mục đầu ra hợp lệ.");
            }
        }

        [RelayCommand]
        public void OpenOutputFile(ImageItemViewModel? item)
        {
            if (item != null && !string.IsNullOrEmpty(item.OutputPath) && File.Exists(item.OutputPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.OutputPath,
                    UseShellExecute = true
                });
            }
        }

        private void UpdateStats()
        {
            TotalFilesCount = Files.Count;
            long origTotal = Files.Sum(f => f.OriginalSizeBytes);
            long outTotal = Files.Where(f => f.OutputSizeBytes > 0).Sum(f => f.OutputSizeBytes);

            if (origTotal > 0 && outTotal > 0)
            {
                long saved = origTotal - outTotal;
                double savedPercent = (double)saved / origTotal * 100.0;
                TotalSavedDisplay = $"{ImageItemViewModel.FormatFileSize(Math.Max(0, saved))} ({savedPercent.ToString("F1", CultureInfo.InvariantCulture)}%)";
            }
            else
            {
                TotalSavedDisplay = "0 B (0%)";
            }
        }
    }
}
