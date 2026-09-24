using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.Views.Windows;

namespace MyDeusTools.App.ViewModels
{
    public partial class QrCodeViewModel : ObservableObject
    {
        private readonly IQrCodeService _qrCodeService;

        // --- Generator State ---
        private string _inputText = "https://github.com";
        public string InputText
        {
            get => _inputText;
            set
            {
                if (SetProperty(ref _inputText, value))
                {
                    GenerateQr();
                }
            }
        }

        private BitmapSource? _generatedImage;
        public BitmapSource? GeneratedImage
        {
            get => _generatedImage;
            set => SetProperty(ref _generatedImage, value);
        }

        private bool _hasGeneratedImage;
        public bool HasGeneratedImage
        {
            get => _hasGeneratedImage;
            set => SetProperty(ref _hasGeneratedImage, value);
        }

        // --- Scanner State ---
        private string _scannedResult = string.Empty;
        public string ScannedResult
        {
            get => _scannedResult;
            set
            {
                if (SetProperty(ref _scannedResult, value))
                {
                    HasScannedResult = !string.IsNullOrWhiteSpace(value);
                    IsUrl = Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                }
            }
        }

        private bool _hasScannedResult;
        public bool HasScannedResult
        {
            get => _hasScannedResult;
            set => SetProperty(ref _hasScannedResult, value);
        }

        private bool _isUrl;
        public bool IsUrl
        {
            get => _isUrl;
            set => SetProperty(ref _isUrl, value);
        }

        private string _statusMessage = "Sẵn sàng tạo hoặc quét mã QR";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public QrCodeViewModel(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
            GenerateQr();
        }

        [RelayCommand]
        public void GenerateQr()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                GeneratedImage = null;
                HasGeneratedImage = false;
                StatusMessage = "Vui lòng nhập nội dung để tạo mã QR.";
                return;
            }

            try
            {
                GeneratedImage = _qrCodeService.GenerateQrCode(InputText);
                HasGeneratedImage = true;
                StatusMessage = "Đã tạo mã QR thành công!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi tạo mã QR: {ex.Message}";
                HasGeneratedImage = false;
            }
        }

        [RelayCommand]
        public void SaveImage()
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;

            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PNG Image (*.png)|*.png",
                    FileName = "qrcode.png",
                    DefaultExt = ".png"
                };

                if (dialog.ShowDialog() == true)
                {
                    byte[] pngBytes = _qrCodeService.GenerateQrCodePng(InputText, 20);
                    File.WriteAllBytes(dialog.FileName, pngBytes);
                    StatusMessage = $"Đã lưu mã QR vào: {Path.GetFileName(dialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi khi lưu ảnh: {ex.Message}";
            }
        }

        [RelayCommand]
        public void CopyImage()
        {
            if (GeneratedImage == null) return;

            try
            {
                Clipboard.SetImage(GeneratedImage);
                StatusMessage = "Đã sao chép ảnh mã QR vào khay nhớ tạm!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi sao chép ảnh: {ex.Message}";
            }
        }

        [RelayCommand]
        public void ScanScreen()
        {
            StatusMessage = "Đang chọn vùng màn hình để quét...";

            var overlay = new QrSnippingOverlayWindow();
            overlay.RegionSelected += (x, y, w, h) =>
            {
                // Delay nhỏ để overlay biến mất hoàn toàn khỏi màn hình
                Application.Current?.Dispatcher?.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(100);
                    string? result = _qrCodeService.DecodeFromScreenRegion(x, y, w, h);
                    if (!string.IsNullOrEmpty(result))
                    {
                        ScannedResult = result;
                        StatusMessage = "Quét mã QR từ màn hình thành công!";
                    }
                    else
                    {
                        StatusMessage = "Không tìm thấy mã QR trong vùng vừa chọn.";
                    }
                });
            };

            overlay.SnippingCanceled += () =>
            {
                StatusMessage = "Đã hủy thao tác quét màn hình.";
            };

            overlay.ShowDialog();
        }

        [RelayCommand]
        public void ScanFile()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.bmp;*.webp|Tất cả tệp (*.*)|*.*",
                    Title = "Chọn hình ảnh chứa mã QR"
                };

                if (dialog.ShowDialog() == true)
                {
                    string? result = _qrCodeService.DecodeQrCodeFromFile(dialog.FileName);
                    if (!string.IsNullOrEmpty(result))
                    {
                        ScannedResult = result;
                        StatusMessage = $"Quét thành công từ file: {Path.GetFileName(dialog.FileName)}";
                    }
                    else
                    {
                        StatusMessage = "Không tìm thấy hoặc không thể đọc mã QR từ file hình ảnh.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi đọc file: {ex.Message}";
            }
        }

        [RelayCommand]
        public void ScanClipboard()
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    var img = Clipboard.GetImage();
                    if (img != null)
                    {
                        string? result = _qrCodeService.DecodeQrCode(img);
                        if (!string.IsNullOrEmpty(result))
                        {
                            ScannedResult = result;
                            StatusMessage = "Quét thành công mã QR từ hình ảnh trong Clipboard!";
                            return;
                        }
                    }
                }

                StatusMessage = "Khay nhớ tạm không chứa ảnh hoặc không đọc được mã QR.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi đọc từ clipboard: {ex.Message}";
            }
        }

        [RelayCommand]
        public void CopyResult()
        {
            if (string.IsNullOrEmpty(ScannedResult)) return;

            try
            {
                Clipboard.SetText(ScannedResult);
                StatusMessage = "Đã sao chép kết quả quét vào khay nhớ tạm!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi sao chép: {ex.Message}";
            }
        }

        [RelayCommand]
        public void OpenUrl()
        {
            if (!IsUrl || string.IsNullOrEmpty(ScannedResult)) return;

            try
            {
                Process.Start(new ProcessStartInfo(ScannedResult) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Không thể mở liên kết: {ex.Message}";
            }
        }

        [RelayCommand]
        public void ClearScan()
        {
            ScannedResult = string.Empty;
            StatusMessage = "Đã xóa kết quả quét.";
        }
    }
}
