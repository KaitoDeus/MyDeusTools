using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.Views.Windows;

namespace MyDeusTools.App.ViewModels
{
    public partial class ColorPickerViewModel : ObservableObject
    {
        private readonly IColorPickerService _colorPickerService;

        private ColorItemModel? _selectedColor;
        public ColorItemModel? SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (SetProperty(ref _selectedColor, value))
                {
                    HasSelectedColor = value != null;
                }
            }
        }

        private bool _hasSelectedColor;
        public bool HasSelectedColor
        {
            get => _hasSelectedColor;
            set => SetProperty(ref _hasSelectedColor, value);
        }

        private string _statusMessage = "Sẵn sàng bắt màu màn hình";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _hasHistory;
        public bool HasHistory
        {
            get => _hasHistory;
            set => SetProperty(ref _hasHistory, value);
        }

        public ObservableCollection<ColorItemModel> History => _colorPickerService.History;

        public ColorPickerViewModel(IColorPickerService colorPickerService)
        {
            _colorPickerService = colorPickerService;
            _colorPickerService.HistoryChanged += OnHistoryChanged;

            UpdateState();
            if (History.Count > 0)
            {
                SelectedColor = History[0];
            }
        }

        private void OnHistoryChanged()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                UpdateState();
            });
        }

        private void UpdateState()
        {
            HasHistory = History.Count > 0;
            if (SelectedColor == null && History.Count > 0)
            {
                SelectedColor = History[0];
            }
        }

        [RelayCommand]
        public void PickColor()
        {
            StatusMessage = "Rê chuột đến vùng cần lấy màu và click để chọn...";

            var overlay = new ColorPickerOverlayWindow(_colorPickerService);
            overlay.ColorSelected += (r, g, b) =>
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    var item = _colorPickerService.AddColor(r, g, b);
                    SelectedColor = item;

                    // Tự động sao chép mã HEX vào clipboard
                    try
                    {
                        Clipboard.SetText(item.Hex);
                        StatusMessage = $"Đã bắt màu {item.Hex} và sao chép vào khay nhớ tạm!";
                    }
                    catch
                    {
                        StatusMessage = $"Đã bắt màu {item.Hex}!";
                    }
                });
            };

            overlay.PickingCanceled += () =>
            {
                StatusMessage = "Đã hủy thao tác bắt màu.";
            };

            overlay.ShowDialog();
        }

        [RelayCommand]
        public void SelectColor(ColorItemModel item)
        {
            if (item == null) return;
            SelectedColor = item;
            StatusMessage = $"Đang xem chi tiết màu {item.Hex}";
        }

        [RelayCommand]
        public void CopyFormat(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                Clipboard.SetText(text);
                StatusMessage = $"Đã sao chép: {text}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi sao chép: {ex.Message}";
            }
        }

        [RelayCommand]
        public void TogglePin(ColorItemModel item)
        {
            if (item == null) return;
            _colorPickerService.TogglePin(item);
            StatusMessage = item.IsPinned ? $"Đã ghim màu {item.Hex}" : $"Đã bỏ ghim màu {item.Hex}";
        }

        [RelayCommand]
        public void DeleteColor(ColorItemModel item)
        {
            if (item == null) return;

            _colorPickerService.RemoveColor(item);
            if (SelectedColor == item)
            {
                SelectedColor = History.FirstOrDefault();
            }
            StatusMessage = $"Đã xóa màu {item.Hex} khỏi lịch sử.";
        }

        [RelayCommand]
        public void ClearHistory()
        {
            _colorPickerService.ClearHistory(keepPinned: true);
            SelectedColor = History.FirstOrDefault();
            StatusMessage = "Đã xóa toàn bộ lịch sử (giữ lại các màu đã ghim).";
        }
    }
}
