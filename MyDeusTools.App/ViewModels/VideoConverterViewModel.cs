using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class VideoConverterViewModel : ObservableObject
    {
        private readonly IVideoConverterService _videoConverterService;
        private readonly ILanguageService? _languageService;
        private CancellationTokenSource? _conversionCts;
        private CancellationTokenSource? _downloadCts;

        #region Input File Properties

        private string _inputFilePath = string.Empty;
        public string InputFilePath
        {
            get => _inputFilePath;
            set
            {
                if (SetProperty(ref _inputFilePath, value))
                {
                    OnPropertyChanged(nameof(HasInputFile));
                    UpdateOutputPaths();
                }
            }
        }

        private string _inputFileName = string.Empty;
        public string InputFileName
        {
            get => _inputFileName;
            set => SetProperty(ref _inputFileName, value);
        }

        private string _inputFileSizeFormatted = string.Empty;
        public string InputFileSizeFormatted
        {
            get => _inputFileSizeFormatted;
            set => SetProperty(ref _inputFileSizeFormatted, value);
        }

        private string _durationFormatted = "--:--:--";
        public string DurationFormatted
        {
            get => _durationFormatted;
            set => SetProperty(ref _durationFormatted, value);
        }

        private string _resolutionFormatted = "--";
        public string ResolutionFormatted
        {
            get => _resolutionFormatted;
            set => SetProperty(ref _resolutionFormatted, value);
        }

        private string _videoCodecFormatted = "--";
        public string VideoCodecFormatted
        {
            get => _videoCodecFormatted;
            set => SetProperty(ref _videoCodecFormatted, value);
        }

        private string _audioCodecFormatted = "--";
        public string AudioCodecFormatted
        {
            get => _audioCodecFormatted;
            set => SetProperty(ref _audioCodecFormatted, value);
        }

        public bool HasInputFile => !string.IsNullOrEmpty(InputFilePath) && File.Exists(InputFilePath);

        #endregion

        #region Target Options

        private VideoFormat _selectedFormat = VideoFormat.Mp4;
        public VideoFormat SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                if (SetProperty(ref _selectedFormat, value))
                {
                    OnPropertyChanged(nameof(IsAudioOnlyFormat));
                    OnPropertyChanged(nameof(IsGifFormat));
                    UpdateOutputPaths();
                }
            }
        }

        public bool IsAudioOnlyFormat => SelectedFormat is VideoFormat.Mp3 or VideoFormat.Wav or VideoFormat.Aac or VideoFormat.M4a or VideoFormat.Ogg or VideoFormat.Flac;
        public bool IsGifFormat => SelectedFormat == VideoFormat.Gif;

        private VideoResolution _selectedResolution = VideoResolution.Original;
        public VideoResolution SelectedResolution
        {
            get => _selectedResolution;
            set => SetProperty(ref _selectedResolution, value);
        }

        private VideoCodec _selectedCodec = VideoCodec.Auto;
        public VideoCodec SelectedCodec
        {
            get => _selectedCodec;
            set => SetProperty(ref _selectedCodec, value);
        }

        private VideoQuality _selectedQuality = VideoQuality.High;
        public VideoQuality SelectedQuality
        {
            get => _selectedQuality;
            set => SetProperty(ref _selectedQuality, value);
        }

        private VideoFrameRate _selectedFrameRate = VideoFrameRate.Original;
        public VideoFrameRate SelectedFrameRate
        {
            get => _selectedFrameRate;
            set => SetProperty(ref _selectedFrameRate, value);
        }

        private AudioOption _selectedAudioOption = AudioOption.Keep;
        public AudioOption SelectedAudioOption
        {
            get => _selectedAudioOption;
            set => SetProperty(ref _selectedAudioOption, value);
        }

        private AudioBitrate _selectedAudioBitrate = AudioBitrate.Kbps192;
        public AudioBitrate SelectedAudioBitrate
        {
            get => _selectedAudioBitrate;
            set => SetProperty(ref _selectedAudioBitrate, value);
        }

        private bool _enableTrimming = false;
        public bool EnableTrimming
        {
            get => _enableTrimming;
            set => SetProperty(ref _enableTrimming, value);
        }

        private string _startTimeText = "00:00:00";
        public string StartTimeText
        {
            get => _startTimeText;
            set => SetProperty(ref _startTimeText, value);
        }

        private string _endTimeText = "00:00:00";
        public string EndTimeText
        {
            get => _endTimeText;
            set => SetProperty(ref _endTimeText, value);
        }

        private bool _useGpuAcceleration = false;
        public bool UseGpuAcceleration
        {
            get => _useGpuAcceleration;
            set => SetProperty(ref _useGpuAcceleration, value);
        }

        private string _outputDirectory = string.Empty;
        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                if (SetProperty(ref _outputDirectory, value))
                {
                    UpdateOutputPaths();
                }
            }
        }

        private string _outputFileName = string.Empty;
        public string OutputFileName
        {
            get => _outputFileName;
            set
            {
                if (SetProperty(ref _outputFileName, value))
                {
                    OnPropertyChanged(nameof(OutputFilePath));
                }
            }
        }

        public string OutputFilePath => (!string.IsNullOrEmpty(OutputDirectory) && !string.IsNullOrEmpty(OutputFileName))
            ? Path.Combine(OutputDirectory, OutputFileName)
            : string.Empty;

        #endregion

        #region Engine & Execution State

        public bool IsEngineAvailable => _videoConverterService.IsEngineAvailable;

        public string EngineStatusText => _videoConverterService.IsEngineAvailable
            ? $"Sẵn sàng ({_videoConverterService.EngineVersion})"
            : "Chưa cài đặt bộ máy FFmpeg";

        private bool _isConverting = false;
        public bool IsConverting
        {
            get => _isConverting;
            set
            {
                if (SetProperty(ref _isConverting, value))
                {
                    OnPropertyChanged(nameof(CanStartConversion));
                }
            }
        }

        private bool _isCompleted = false;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        private bool _isDownloadingEngine = false;
        public bool IsDownloadingEngine
        {
            get => _isDownloadingEngine;
            set => SetProperty(ref _isDownloadingEngine, value);
        }

        private double _downloadProgress = 0;
        public double DownloadProgress
        {
            get => _downloadProgress;
            set => SetProperty(ref _downloadProgress, value);
        }

        private double _progressPercentage = 0;
        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        private string _progressCurrentTime = "00:00:00";
        public string ProgressCurrentTime
        {
            get => _progressCurrentTime;
            set => SetProperty(ref _progressCurrentTime, value);
        }

        private string _progressTotalTime = "00:00:00";
        public string ProgressTotalTime
        {
            get => _progressTotalTime;
            set => SetProperty(ref _progressTotalTime, value);
        }

        private string _progressSpeed = "1.0x";
        public string ProgressSpeed
        {
            get => _progressSpeed;
            set => SetProperty(ref _progressSpeed, value);
        }

        private string _progressFps = "0";
        public string ProgressFps
        {
            get => _progressFps;
            set => SetProperty(ref _progressFps, value);
        }

        private string _progressEstimatedRemaining = "--:--";
        public string ProgressEstimatedRemaining
        {
            get => _progressEstimatedRemaining;
            set => SetProperty(ref _progressEstimatedRemaining, value);
        }

        private string _statusMessage = "Sẵn sàng chuyển đổi video";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _logText = string.Empty;
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        private bool _isLogExpanded = false;
        public bool IsLogExpanded
        {
            get => _isLogExpanded;
            set => SetProperty(ref _isLogExpanded, value);
        }

        public bool CanStartConversion => HasInputFile && !IsConverting && IsEngineAvailable;

        #endregion

        public VideoConverterViewModel(IVideoConverterService videoConverterService, ILanguageService? languageService = null)
        {
            _videoConverterService = videoConverterService;
            _languageService = languageService;

            _videoConverterService.EngineStatusChanged += OnEngineStatusChanged;

            // Đặt thư mục đầu ra mặc định là thư mục Videos hoặc desktop
            string myVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            _outputDirectory = Directory.Exists(myVideos) ? myVideos : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void OnEngineStatusChanged()
        {
            OnPropertyChanged(nameof(IsEngineAvailable));
            OnPropertyChanged(nameof(EngineStatusText));
            OnPropertyChanged(nameof(CanStartConversion));
        }

        public void HandleFileDrop(string filePath)
        {
            if (File.Exists(filePath))
            {
                LoadInputFile(filePath);
            }
        }

        public async void LoadInputFile(string filePath)
        {
            InputFilePath = filePath;
            InputFileName = Path.GetFileName(filePath);
            IsCompleted = false;
            ProgressPercentage = 0;
            StatusMessage = "Đang phân tích thông tin video...";

            var info = await _videoConverterService.ProbeMediaInfoAsync(filePath);
            if (info != null)
            {
                InputFileSizeFormatted = info.FileSizeFormatted;
                DurationFormatted = info.DurationFormatted;
                ResolutionFormatted = info.ResolutionFormatted;
                VideoCodecFormatted = info.VideoCodec;
                AudioCodecFormatted = info.AudioCodec;
                EndTimeText = info.DurationFormatted;
                StatusMessage = $"Đã tải tệp: {info.FileName} ({info.FileSizeFormatted})";
            }
            else
            {
                StatusMessage = "Đã chọn tệp. Nhấn Bắt đầu để chuyển đổi.";
            }

            OnPropertyChanged(nameof(CanStartConversion));
        }

        private void UpdateOutputPaths()
        {
            if (string.IsNullOrEmpty(InputFilePath)) return;

            string baseName = Path.GetFileNameWithoutExtension(InputFilePath);
            string ext = GetExtensionForFormat(SelectedFormat);

            OutputFileName = $"{baseName}_converted{ext}";

            if (string.IsNullOrEmpty(OutputDirectory))
            {
                string? inDir = Path.GetDirectoryName(InputFilePath);
                OutputDirectory = !string.IsNullOrEmpty(inDir) ? inDir : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
        }

        public static string GetExtensionForFormat(VideoFormat format) => format switch
        {
            VideoFormat.Mp4 => ".mp4",
            VideoFormat.Mkv => ".mkv",
            VideoFormat.Webm => ".webm",
            VideoFormat.Avi => ".avi",
            VideoFormat.Mov => ".mov",
            VideoFormat.Wmv => ".wmv",
            VideoFormat.Gif => ".gif",
            VideoFormat.Flv => ".flv",
            VideoFormat.Mp3 => ".mp3",
            VideoFormat.Wav => ".wav",
            VideoFormat.Aac => ".aac",
            VideoFormat.M4a => ".m4a",
            VideoFormat.Ogg => ".ogg",
            VideoFormat.Flac => ".flac",
            _ => ".mp4"
        };

        #region Commands

        [RelayCommand]
        public void SelectInputFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn tệp video nguồn",
                Filter = "Tất cả tệp video/âm thanh|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm;*.flv;*.ts;*.m4v;*.3gp;*.mp3;*.wav;*.aac|Video Files (*.mp4;*.mkv;*.avi;*.mov)|*.mp4;*.mkv;*.avi;*.mov|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadInputFile(dialog.FileName);
            }
        }

        [RelayCommand]
        public void ClearInput()
        {
            InputFilePath = string.Empty;
            InputFileName = string.Empty;
            InputFileSizeFormatted = string.Empty;
            DurationFormatted = "--:--:--";
            ResolutionFormatted = "--";
            VideoCodecFormatted = "--";
            AudioCodecFormatted = "--";
            OutputFileName = string.Empty;
            IsCompleted = false;
            ProgressPercentage = 0;
            StatusMessage = "Sẵn sàng chuyển đổi video";
            OnPropertyChanged(nameof(CanStartConversion));
        }

        [RelayCommand]
        public void SelectOutputDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Chọn thư mục lưu tệp đầu ra",
                InitialDirectory = Directory.Exists(OutputDirectory) ? OutputDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            };

            if (dialog.ShowDialog() == true)
            {
                OutputDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        public void QuickSelectFormat(string formatName)
        {
            if (Enum.TryParse<VideoFormat>(formatName, true, out var format))
            {
                SelectedFormat = format;
            }
        }

        [RelayCommand]
        public async Task StartConversionAsync()
        {
            if (!HasInputFile)
            {
                StatusMessage = "Vui lòng chọn tệp video nguồn!";
                return;
            }

            if (!IsEngineAvailable)
            {
                StatusMessage = "Chưa phát hiện bộ máy FFmpeg! Vui lòng cài đặt trước.";
                return;
            }

            if (string.IsNullOrEmpty(OutputFilePath))
            {
                UpdateOutputPaths();
            }

            var options = new VideoConversionOptions
            {
                InputFilePath = InputFilePath,
                OutputFilePath = OutputFilePath,
                TargetFormat = SelectedFormat,
                Resolution = SelectedResolution,
                Codec = SelectedCodec,
                Quality = SelectedQuality,
                FrameRate = SelectedFrameRate,
                Audio = SelectedAudioOption,
                AudioBitrate = SelectedAudioBitrate,
                EnableTrimming = EnableTrimming,
                UseGpuAcceleration = UseGpuAcceleration
            };

            if (EnableTrimming)
            {
                if (TimeSpan.TryParse(StartTimeText, out var start))
                    options.StartTime = start;
                if (TimeSpan.TryParse(EndTimeText, out var end))
                    options.EndTime = end;
            }

            IsConverting = true;
            IsCompleted = false;
            ProgressPercentage = 0;
            LogText = string.Empty;
            StatusMessage = "Đang chuyển đổi video...";

            _conversionCts = new CancellationTokenSource();

            var progress = new Progress<ConversionProgress>(p =>
            {
                ProgressPercentage = p.Percentage;
                ProgressCurrentTime = p.CurrentTimeFormatted;
                ProgressTotalTime = p.TotalTimeFormatted;
                ProgressSpeed = p.Speed;
                ProgressFps = $"{p.Fps:0}";
                ProgressEstimatedRemaining = p.EstimatedRemaining.HasValue
                    ? $"{(int)p.EstimatedRemaining.Value.TotalMinutes:D2}:{p.EstimatedRemaining.Value.Seconds:D2}"
                    : "--:--";

                StatusMessage = p.StatusText;
            });

            try
            {
                bool success = await _videoConverterService.ConvertAsync(
                    options,
                    progress,
                    line =>
                    {
                        LogText = (LogText.Length > 20000 ? LogText.Substring(LogText.Length - 10000) : LogText) + line + Environment.NewLine;
                    },
                    _conversionCts.Token);

                if (success)
                {
                    IsCompleted = true;
                    ProgressPercentage = 100;
                    StatusMessage = $"Chuyển đổi thành công: {Path.GetFileName(OutputFilePath)}";
                }
                else
                {
                    StatusMessage = "Quá trình chuyển đổi gặp lỗi! Xem nhật ký (Logs) để biết chi tiết.";
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Đã hủy bỏ chuyển đổi.";
                ProgressPercentage = 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsConverting = false;
                _conversionCts?.Dispose();
                _conversionCts = null;
            }
        }

        [RelayCommand]
        public void CancelConversion()
        {
            if (IsConverting && _conversionCts != null)
            {
                _conversionCts.Cancel();
                StatusMessage = "Đang dừng tiến trình chuyển đổi...";
            }
        }

        [RelayCommand]
        public async Task DownloadEngineAsync()
        {
            if (IsDownloadingEngine) return;

            IsDownloadingEngine = true;
            DownloadProgress = 0;
            StatusMessage = "Đang tải bộ máy FFmpeg Portable...";

            _downloadCts = new CancellationTokenSource();
            var progress = new Progress<double>(p =>
            {
                DownloadProgress = p;
                StatusMessage = $"Đang tải FFmpeg: {p:0.0}%";
            });

            try
            {
                bool success = await _videoConverterService.DownloadEngineAsync(progress, _downloadCts.Token);
                if (success)
                {
                    StatusMessage = "Cài đặt bộ máy FFmpeg thành công! Đã sẵn sàng chuyển đổi.";
                    OnEngineStatusChanged();
                }
                else
                {
                    StatusMessage = "Tải thất bại. Vui lòng kiểm tra kết nối mạng hoặc chọn tệp ffmpeg.exe sẵn có.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tải: {ex.Message}";
            }
            finally
            {
                IsDownloadingEngine = false;
                _downloadCts?.Dispose();
                _downloadCts = null;
            }
        }

        [RelayCommand]
        public void BrowseFfmpeg()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn tệp thực thi ffmpeg.exe",
                Filter = "FFmpeg Executable (ffmpeg.exe)|ffmpeg.exe|All Executables (*.exe)|*.exe"
            };

            if (dialog.ShowDialog() == true)
            {
                _videoConverterService.SetCustomFfmpegPath(dialog.FileName);
                StatusMessage = $"Đã liên kết bộ máy: {dialog.FileName}";
            }
        }

        [RelayCommand]
        public void OpenOutputFolder()
        {
            if (File.Exists(OutputFilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{OutputFilePath}\"");
            }
            else if (Directory.Exists(OutputDirectory))
            {
                Process.Start("explorer.exe", $"\"{OutputDirectory}\"");
            }
        }

        [RelayCommand]
        public void OpenOutputFile()
        {
            if (File.Exists(OutputFilePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = OutputFilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Không thể mở tệp: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void ToggleLog()
        {
            IsLogExpanded = !IsLogExpanded;
        }

        #endregion
    }
}
