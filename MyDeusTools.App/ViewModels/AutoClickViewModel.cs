using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;
using NHotkey.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyDeusTools.App.ViewModels
{
    public partial class AutoClickViewModel : ObservableObject
    {
        private readonly IAutoClickService _autoClickService;
        private readonly ILanguageService? _languageService;

        private string GetLoc(string key, string fallback) =>
            _languageService?.GetString(key, fallback) ?? fallback;

        // Time settings
        private int _hours = 0;
        public int Hours { get => _hours; set => SetProperty(ref _hours, value); }

        private int _minutes = 0;
        public int Minutes { get => _minutes; set => SetProperty(ref _minutes, value); }

        private int _seconds = 0;
        public int Seconds { get => _seconds; set => SetProperty(ref _seconds, value); }

        private int _milliseconds = 100;
        public int Milliseconds { get => _milliseconds; set => SetProperty(ref _milliseconds, value); }

        // Click settings
        public ObservableCollection<MouseButton> MouseButtons { get; } = new() { MouseButton.Left, MouseButton.Right, MouseButton.Middle };
        private MouseButton _selectedMouseButton = MouseButton.Left;
        public MouseButton SelectedMouseButton { get => _selectedMouseButton; set => SetProperty(ref _selectedMouseButton, value); }

        public ObservableCollection<ClickType> ClickTypes { get; } = new() { ClickType.Single, ClickType.Double };
        private ClickType _selectedClickType = ClickType.Single;
        public ClickType SelectedClickType { get => _selectedClickType; set => SetProperty(ref _selectedClickType, value); }

        // Repeat settings
        private int _repeatCount = 0; // 0 = Infinite
        public int RepeatCount { get => _repeatCount; set => SetProperty(ref _repeatCount, value); }

        private int _recordedPointsCount = 0;
        public int RecordedPointsCount { get => _recordedPointsCount; set => SetProperty(ref _recordedPointsCount, value); }

        private string _statusText = "Sẵn sàng";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private string _buttonText = "Bắt đầu (F6)";
        public string ButtonText { get => _buttonText; set => SetProperty(ref _buttonText, value); }

        private string _recordButtonText = "Ghi tọa độ";
        public string RecordButtonText { get => _recordButtonText; set => SetProperty(ref _recordButtonText, value); }

        // Hotkey settings
        private ModifierKeys _selectedModifiers = ModifierKeys.None;
        public ModifierKeys SelectedModifiers { get => _selectedModifiers; set => SetProperty(ref _selectedModifiers, value); }

        private Key _selectedKey = Key.F6;
        public Key SelectedKey { get => _selectedKey; set => SetProperty(ref _selectedKey, value); }

        private string _hotkeyDisplayText = "F6";
        public string HotkeyDisplayText { get => _hotkeyDisplayText; set => SetProperty(ref _hotkeyDisplayText, value); }

        private bool _isListeningForHotkey = false;
        public bool IsListeningForHotkey { get => _isListeningForHotkey; set => SetProperty(ref _isListeningForHotkey, value); }

        public AutoClickViewModel(IAutoClickService autoClickService, ILanguageService? languageService = null)
        {
            _autoClickService = autoClickService;
            _languageService = languageService;

            if (_languageService != null)
            {
                _languageService.LanguageChanged += _ => UpdateButtonAndStatusTexts();
            }

            _autoClickService.Stopped += OnAutoClickServiceStopped;
            _statusText = GetLoc("AutoClick_StatusReady", "Sẵn sàng");
            _recordButtonText = GetLoc("AutoClick_Tab_Record", "Ghi tọa độ");
            UpdateHotkey();
        }

        private void UpdateButtonAndStatusTexts()
        {
            RecordButtonText = GetLoc("AutoClick_Tab_Record", "Ghi tọa độ");
            if (_autoClickService.IsRunning)
            {
                ButtonText = string.Format(GetLoc("AutoClick_BtnStopFmt", "Dừng lại ({0})"), HotkeyDisplayText);
            }
            else
            {
                ButtonText = string.Format(GetLoc("AutoClick_BtnStartFmt", "Bắt đầu ({0})"), HotkeyDisplayText);
                if (StatusText == "Sẵn sàng" || StatusText == "Ready" || StatusText == "Đã dừng" || StatusText == "Stopped")
                {
                    StatusText = GetLoc("AutoClick_StatusReady", "Sẵn sàng");
                }
            }
        }

        private void OnAutoClickServiceStopped()
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                StatusText = GetLoc("AutoClick_StatusStopped", "Đã dừng");
                ButtonText = string.Format(GetLoc("AutoClick_BtnStartFmt", "Bắt đầu ({0})"), HotkeyDisplayText);
            });
        }

        private void UpdateHotkey()
        {
            try
            {
                // Xóa hotkey cũ nếu có
                HotkeyManager.Current.Remove("ToggleAutoClick");

                // Đăng ký hotkey mới với cả Modifier và Key
                HotkeyManager.Current.AddOrReplace("ToggleAutoClick", SelectedKey, SelectedModifiers, OnHotkeyPressed);

                // Cập nhật text hiển thị
                string modifiers = SelectedModifiers == ModifierKeys.None ? "" : SelectedModifiers.ToString().Replace(",", " +") + " + ";
                HotkeyDisplayText = $"{modifiers}{SelectedKey}";
                ButtonText = _autoClickService.IsRunning 
                    ? string.Format(GetLoc("AutoClick_BtnStopFmt", "Dừng lại ({0})"), HotkeyDisplayText)
                    : string.Format(GetLoc("AutoClick_BtnStartFmt", "Bắt đầu ({0})"), HotkeyDisplayText);
            }
            catch { }
        }

        [RelayCommand]
        public void StartListening()
        {
            if (_autoClickService.IsRunning) return;
            IsListeningForHotkey = true;
            StatusText = GetLoc("AutoClick_StatusListening", "Nhấn tổ hợp phím bất kỳ để gán (Trừ ESC)...");
        }

        public void ProcessCapturedKey(Key key, ModifierKeys modifiers)
        {
            if (!IsListeningForHotkey) return;

            // Không cho phép phím ESC
            if (key == Key.Escape)
            {
                IsListeningForHotkey = false;
                StatusText = GetLoc("AutoClick_StatusHotkeyCanceled", "Đã hủy đổi phím tắt.");
                return;
            }

            // Bỏ qua nếu chỉ nhấn các phím bổ trợ đơn thuần (Ctrl, Alt, Shift, Win)
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            SelectedKey = key;
            SelectedModifiers = modifiers;
            IsListeningForHotkey = false;

            UpdateHotkey();
            StatusText = string.Format(GetLoc("AutoClick_StatusHotkeyChanged", "Đã đổi phím tắt thành: {0}"), HotkeyDisplayText);
        }

        [RelayCommand]
        public void ToggleClick()
        {
            if (_autoClickService.IsRunning)
            {
                _autoClickService.Stop();
                StatusText = GetLoc("AutoClick_StatusStopped", "Đã dừng");
                ButtonText = string.Format(GetLoc("AutoClick_BtnStartFmt", "Bắt đầu ({0})"), HotkeyDisplayText);
            }
            else
            {
                int totalInterval = (Hours * 3600000) + (Minutes * 60000) + (Seconds * 1000) + Milliseconds;
                _autoClickService.Start(totalInterval, SelectedMouseButton, SelectedClickType, RepeatCount);
                StatusText = _autoClickService.RecordedPoints.Count > 0 
                    ? GetLoc("AutoClick_StatusRunningReplay", "Đang chạy (Replay)...") 
                    : GetLoc("AutoClick_StatusRunning", "Đang chạy...");
                ButtonText = string.Format(GetLoc("AutoClick_BtnStopFmt", "Dừng lại ({0})"), HotkeyDisplayText);
            }
        }

        [RelayCommand]
        public void ToggleRecord()
        {
            if (_autoClickService.IsRunning) return;

            var overlay = new Views.Windows.RecordingOverlayWindow();
            overlay.PointRecorded += (x, y) =>
            {
                _autoClickService.RecordedPoints.Add(new MousePoint { X = x, Y = y });
                RecordedPointsCount = _autoClickService.RecordedPoints.Count;
                StatusText = string.Format(GetLoc("AutoClick_StatusPointRecorded", "Đã ghi điểm thứ {0} tại: {1}, {2}"), RecordedPointsCount, x, y);
            };
            overlay.RecordingCanceled += () =>
            {
                StatusText = GetLoc("AutoClick_StatusRecordingExit", "Đã thoát chế độ ghi tọa độ.");
            };

            StatusText = GetLoc("AutoClick_StatusWaitingClick", "Đang chờ click để ghi tọa độ...");
            overlay.ShowDialog(); // Hiển thị overlay ghi tọa độ
        }

        [RelayCommand]
        public void ClearRecording()
        {
            _autoClickService.ClearRecordedPoints();
            RecordedPointsCount = 0;
            StatusText = GetLoc("AutoClick_StatusClearedPoints", "Đã xóa các điểm ghi");
        }

        private void OnHotkeyPressed(object? sender, NHotkey.HotkeyEventArgs e)
        {
            ToggleClick();
        }
    }
}
