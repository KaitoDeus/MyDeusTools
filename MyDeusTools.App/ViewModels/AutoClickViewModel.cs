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

        public ObservableCollection<Key> AvailableKeys { get; } = new() 
        { 
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, 
            Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12 
        };

        private Key _selectedKey = Key.F6;
        public Key SelectedKey 
        { 
            get => _selectedKey; 
            set 
            {
                if (SetProperty(ref _selectedKey, value))
                {
                    UpdateHotkey();
                    ButtonText = $"Bắt đầu ({_selectedKey})";
                }
            }
        }

        public AutoClickViewModel(IAutoClickService autoClickService)
        {
            _autoClickService = autoClickService;
            UpdateHotkey();
        }

        private void UpdateHotkey()
        {
            try 
            {
                HotkeyManager.Current.AddOrReplace("ToggleAutoClick", SelectedKey, ModifierKeys.None, OnHotkeyPressed);
            }
            catch { }
        }

        [RelayCommand]
        public void ToggleClick()
        {
            if (_autoClickService.IsRunning)
            {
                _autoClickService.Stop();
                StatusText = "Đã dừng";
                ButtonText = $"Bắt đầu ({SelectedKey})";
            }
            else
            {
                if (_autoClickService.IsRecording) return;

                int totalInterval = (Hours * 3600000) + (Minutes * 60000) + (Seconds * 1000) + Milliseconds;
                _autoClickService.Start(totalInterval, SelectedMouseButton, SelectedClickType, RepeatCount);
                StatusText = _autoClickService.RecordedPoints.Count > 0 ? "Đang chạy (Replay)..." : "Đang chạy...";
                ButtonText = $"Dừng lại ({SelectedKey})";
            }
        }

        [RelayCommand]
        public void ToggleRecord()
        {
            if (_autoClickService.IsRunning) return;

            if (_autoClickService.IsRecording)
            {
                _autoClickService.StopRecording();
                RecordButtonText = "Ghi tọa độ";
                StatusText = "Đã ghi " + _autoClickService.RecordedPoints.Count + " điểm";
                RecordedPointsCount = _autoClickService.RecordedPoints.Count;
            }
            else
            {
                _autoClickService.StartRecording();
                RecordButtonText = "Dừng ghi";
                StatusText = "Đang ghi tọa độ chuột...";
            }
        }

        [RelayCommand]
        public void ClearRecording()
        {
            _autoClickService.ClearRecordedPoints();
            RecordedPointsCount = 0;
            StatusText = "Đã xóa các điểm ghi";
        }

        private void OnHotkeyPressed(object? sender, NHotkey.HotkeyEventArgs e)
        {
            ToggleClick();
        }
    }
}
