using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class WindowPinnerViewModel : ObservableObject
    {
        private readonly IWindowPinnerService _windowPinnerService;
        private List<WindowInfoModel> _rawWindows = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private string _statusMessage = "Sẵn sàng quản lý ghim cửa sổ";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        private int _pinnedCount;
        public int PinnedCount
        {
            get => _pinnedCount;
            set => SetProperty(ref _pinnedCount, value);
        }

        private bool _hasWindows;
        public bool HasWindows
        {
            get => _hasWindows;
            set => SetProperty(ref _hasWindows, value);
        }

        private WindowInfoModel? _selectedWindow;
        public WindowInfoModel? SelectedWindow
        {
            get => _selectedWindow;
            set => SetProperty(ref _selectedWindow, value);
        }

        public ObservableCollection<WindowInfoModel> FilteredWindows { get; } = new();

        public WindowPinnerViewModel(IWindowPinnerService windowPinnerService)
        {
            _windowPinnerService = windowPinnerService;
            Refresh();
        }

        [RelayCommand]
        public void Refresh()
        {
            try
            {
                _rawWindows = _windowPinnerService.GetOpenWindows();
                foreach (var win in _rawWindows)
                {
                    win.OnOpacityChanged = w =>
                    {
                        _windowPinnerService.SetOpacity(w.Handle, w.Opacity);
                        StatusMessage = $"Độ mờ {w.DisplayText}: {w.OpacityPercent}%";
                    };
                }

                TotalCount = _rawWindows.Count;
                PinnedCount = _rawWindows.Count(w => w.IsTopMost);
                ApplyFilter();
                StatusMessage = $"Đã tải {TotalCount} cửa sổ ({PinnedCount} đang ghim)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi khi tải danh sách cửa sổ: {ex.Message}";
            }
        }

        public void ApplyFilter()
        {
            FilteredWindows.Clear();

            var query = _rawWindows.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(w =>
                    (!string.IsNullOrEmpty(w.Title) && w.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(w.ProcessName) && w.ProcessName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            var sorted = query.OrderByDescending(w => w.IsTopMost)
                              .ThenBy(w => w.DisplayText);

            foreach (var win in sorted)
            {
                FilteredWindows.Add(win);
            }

            HasWindows = FilteredWindows.Count > 0;
            if (SelectedWindow != null && !FilteredWindows.Contains(SelectedWindow))
            {
                SelectedWindow = FilteredWindows.FirstOrDefault();
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private void TogglePin(WindowInfoModel? window)
        {
            if (window == null) return;

            bool success = _windowPinnerService.ToggleTopMost(window.Handle);
            if (success)
            {
                window.IsTopMost = _windowPinnerService.IsWindowTopMost(window.Handle);
                PinnedCount = _rawWindows.Count(w => w.IsTopMost);
                StatusMessage = window.IsTopMost
                    ? $"Đã ghim nổi: {window.DisplayText}"
                    : $"Đã bỏ ghim: {window.DisplayText}";
                ApplyFilter();
            }
            else
            {
                StatusMessage = $"Không thể thay đổi trạng thái ghim: {window.DisplayText}";
            }
        }

        [RelayCommand]
        private void ResetOpacity(WindowInfoModel? window)
        {
            if (window == null) return;

            window.Opacity = 255;
            _windowPinnerService.SetOpacity(window.Handle, 255);
            StatusMessage = $"Đã khôi phục độ mờ 100%: {window.DisplayText}";
        }

        [RelayCommand]
        private void SetOpacity100(WindowInfoModel? window)
        {
            if (window == null) return;
            window.Opacity = 255;
        }

        [RelayCommand]
        private void SetOpacity80(WindowInfoModel? window)
        {
            if (window == null) return;
            window.Opacity = 204;
        }

        [RelayCommand]
        private void SetOpacity60(WindowInfoModel? window)
        {
            if (window == null) return;
            window.Opacity = 153;
        }

        [RelayCommand]
        private void SetOpacity40(WindowInfoModel? window)
        {
            if (window == null) return;
            window.Opacity = 102;
        }

        [RelayCommand]
        private void BringToFront(WindowInfoModel? window)
        {
            if (window == null) return;

            _windowPinnerService.BringToFront(window.Handle);
            StatusMessage = $"Đã kích hoạt lên trước: {window.DisplayText}";
        }

        [RelayCommand]
        private void PinForegroundWindow()
        {
            IntPtr foreground = _windowPinnerService.GetForegroundWindowHandle();
            if (foreground == IntPtr.Zero)
            {
                StatusMessage = "Không tìm thấy cửa sổ đang hoạt động";
                return;
            }

            var match = _rawWindows.FirstOrDefault(w => w.Handle == foreground);
            if (match != null)
            {
                TogglePin(match);
            }
            else
            {
                bool success = _windowPinnerService.ToggleTopMost(foreground);
                StatusMessage = success ? "Đã đổi trạng thái ghim cửa sổ phía trước" : "Không thể ghim cửa sổ này";
                Refresh();
            }
        }
    }
}
