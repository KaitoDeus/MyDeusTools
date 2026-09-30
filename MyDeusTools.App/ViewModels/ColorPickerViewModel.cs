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
        private readonly ILanguageService? _languageService;

        private string GetLoc(string key, string fallback) =>
            _languageService?.GetString(key, fallback) ?? fallback;

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

        public ColorPickerViewModel(IColorPickerService colorPickerService, ILanguageService? languageService = null)
        {
            _colorPickerService = colorPickerService;
            _languageService = languageService;

            if (_languageService != null)
            {
                _languageService.LanguageChanged += _ =>
                {
                    if (StatusMessage == "Sẵn sàng bắt màu màn hình" || StatusMessage == "Ready to pick colors from screen")
                    {
                        StatusMessage = GetLoc("Color_StatusReady", "Sẵn sàng bắt màu màn hình");
                    }
                };
            }

            _statusMessage = GetLoc("Color_StatusReady", "Sẵn sàng bắt màu màn hình");
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
            StatusMessage = GetLoc("Color_StatusPicking", "Rê chuột đến vùng cần lấy màu và click để chọn...");

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
                        string fmt = GetLoc("Color_StatusPickedCopied", "Đã bắt màu {0} và sao chép vào khay nhớ tạm!");
                        StatusMessage = string.Format(fmt, item.Hex);
                    }
                    catch
                    {
                        string fmt = GetLoc("Color_StatusPicked", "Đã bắt màu {0}!");
                        StatusMessage = string.Format(fmt, item.Hex);
                    }
                });
            };

            overlay.PickingCanceled += () =>
            {
                StatusMessage = GetLoc("Color_StatusCanceled", "Đã hủy thao tác bắt màu.");
            };

            overlay.ShowDialog();
        }

        [RelayCommand]
        public void SelectColor(ColorItemModel item)
        {
            if (item == null) return;
            SelectedColor = item;
            string fmt = GetLoc("Color_StatusViewing", "Đang xem chi tiết màu {0}");
            StatusMessage = string.Format(fmt, item.Hex);
        }

        [RelayCommand]
        public void CopyFormat(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                Clipboard.SetText(text);
                string fmt = GetLoc("Color_StatusCopied", "Đã sao chép: {0}");
                StatusMessage = string.Format(fmt, text);
            }
            catch (Exception ex)
            {
                string fmt = GetLoc("Color_StatusCopyError", "Lỗi sao chép: {0}");
                StatusMessage = string.Format(fmt, ex.Message);
            }
        }

        [RelayCommand]
        public void TogglePin(ColorItemModel item)
        {
            if (item == null) return;
            _colorPickerService.TogglePin(item);
            string fmt = item.IsPinned 
                ? GetLoc("Color_StatusPinned", "Đã ghim màu {0}") 
                : GetLoc("Color_StatusUnpinned", "Đã bỏ ghim màu {0}");
            StatusMessage = string.Format(fmt, item.Hex);
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
            string fmt = GetLoc("Color_StatusDeleted", "Đã xóa màu {0} khỏi lịch sử.");
            StatusMessage = string.Format(fmt, item.Hex);
        }

        [RelayCommand]
        public void ClearHistory()
        {
            _colorPickerService.ClearHistory(keepPinned: true);
            SelectedColor = History.FirstOrDefault();
            StatusMessage = GetLoc("Color_StatusCleared", "Đã xóa toàn bộ lịch sử (giữ lại các màu đã ghim).");
        }
    }
}
