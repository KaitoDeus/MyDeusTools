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
        private readonly ILanguageService? _languageService;
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

        private string _statusMessage = string.Empty;
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

        public WindowPinnerViewModel(IWindowPinnerService windowPinnerService, ILanguageService? languageService = null)
        {
            _windowPinnerService = windowPinnerService;
            _languageService = languageService;

            if (_languageService != null)
            {
                _languageService.LanguageChanged += _ => UpdateLoadedStatus();
            }

            _statusMessage = GetLoc("WindowPinner_StatusReady", "Ready to manage window pinning");
            Refresh();
        }

        private string GetLoc(string key, string fallback) =>
            _languageService?.GetString(key, fallback) ?? fallback;

        private void UpdateLoadedStatus()
        {
            string fmt = GetLoc("WindowPinner_StatusLoaded", "Loaded {0} windows ({1} pinned)");
            StatusMessage = string.Format(fmt, TotalCount, PinnedCount);
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
                        string fmt = GetLoc("WindowPinner_StatusOpacity", "Opacity of {0}: {1}%");
                        StatusMessage = string.Format(fmt, w.DisplayText, w.OpacityPercent);
                    };
                }

                TotalCount = _rawWindows.Count;
                PinnedCount = _rawWindows.Count(w => w.IsTopMost);
                ApplyFilter();
                UpdateLoadedStatus();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
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
                string fmt = window.IsTopMost
                    ? GetLoc("WindowPinner_StatusPinned", "Pinned to top: {0}")
                    : GetLoc("WindowPinner_StatusUnpinned", "Unpinned: {0}");
                StatusMessage = string.Format(fmt, window.DisplayText);
                ApplyFilter();
            }
            else
            {
                StatusMessage = $"Cannot toggle pin: {window.DisplayText}";
            }
        }

        [RelayCommand]
        private void ResetOpacity(WindowInfoModel? window)
        {
            if (window == null) return;

            window.Opacity = 255;
            _windowPinnerService.SetOpacity(window.Handle, 255);
            string fmt = GetLoc("WindowPinner_StatusResetOpacity", "Restored 100% opacity: {0}");
            StatusMessage = string.Format(fmt, window.DisplayText);
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
            string fmt = GetLoc("WindowPinner_StatusBroughtToFront", "Brought to front: {0}");
            StatusMessage = string.Format(fmt, window.DisplayText);
        }

        [RelayCommand]
        private void PinForegroundWindow()
        {
            IntPtr foreground = _windowPinnerService.GetForegroundWindowHandle();
            if (foreground == IntPtr.Zero)
            {
                StatusMessage = GetLoc("WindowPinner_StatusNoForeground", "No active window found");
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
                StatusMessage = success
                    ? GetLoc("WindowPinner_StatusPinned", "Changed pin state of active window")
                    : "Cannot pin window";
                Refresh();
            }
        }
    }
}
