using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class ShutdownViewModel : ObservableObject
    {
        private readonly ISystemService _systemService;
        private readonly DispatcherTimer _countdownTimer;
        private int _remainingSeconds;

        // Mode selection
        public ObservableCollection<ShutdownMode> ShutdownModes { get; } = new()
        {
            ShutdownMode.Shutdown,
            ShutdownMode.Restart,
            ShutdownMode.Hibernate
        };

        private ShutdownMode _selectedMode = ShutdownMode.Shutdown;
        public ShutdownMode SelectedMode { get => _selectedMode; set => SetProperty(ref _selectedMode, value); }

        // Input properties
        private int _hours = 0;
        public int Hours { get => _hours; set => SetProperty(ref _hours, value); }

        private int _minutes = 30;
        public int Minutes { get => _minutes; set => SetProperty(ref _minutes, value); }

        private int _seconds = 0;
        public int Seconds { get => _seconds; set => SetProperty(ref _seconds, value); }

        private string _statusMessage = "Sẵn sàng";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        private string _countdownText = "";
        public string CountdownText { get => _countdownText; set => SetProperty(ref _countdownText, value); }

        private bool _isScheduled = false;
        public bool IsScheduled { get => _isScheduled; set => SetProperty(ref _isScheduled, value); }

        public ShutdownViewModel(ISystemService systemService)
        {
            _systemService = systemService;
            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        [RelayCommand]
        private void Schedule()
        {
            int totalSeconds = (Hours * 3600) + (Minutes * 60) + Seconds;
            
            if (totalSeconds <= 0)
            {
                StatusMessage = "Vui lòng nhập thời gian lớn hơn 0!";
                return;
            }

            // Dừng timer hiện tại để làm mới đồng hồ đếm ngược
            _countdownTimer.Stop();

            // Thực thi lên lịch mới (hệ thống sẽ tự động hủy lệnh cũ trước)
            _systemService.ScheduleAction(SelectedMode, totalSeconds);
            
            _remainingSeconds = totalSeconds;
            _countdownTimer.Start();
            UpdateCountdownDisplay();

            string modeName = SelectedMode switch
            {
                ShutdownMode.Restart => "khởi động lại",
                ShutdownMode.Hibernate => "ngủ đông",
                _ => "tắt máy"
            };

            bool wasScheduled = _isScheduled;
            IsScheduled = true;

            StatusMessage = wasScheduled 
                ? $"Đã cập nhật hẹn giờ {modeName} sau {Hours}h {Minutes}m {Seconds}s" 
                : $"Đã hẹn giờ {modeName} sau {Hours}h {Minutes}m {Seconds}s";
        }

        [RelayCommand]
        private void Cancel()
        {
            _countdownTimer.Stop();
            _remainingSeconds = 0;
            CountdownText = "";
            IsScheduled = false;
            _systemService.CancelShutdown();
            StatusMessage = "Đã hủy lệnh hẹn giờ";
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            if (_remainingSeconds <= 0)
            {
                _countdownTimer.Stop();
                _remainingSeconds = 0;
                CountdownText = "00:00:00";
                IsScheduled = false;
                StatusMessage = "Đã hết thời gian chờ!";
                return;
            }

            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            TimeSpan ts = TimeSpan.FromSeconds(_remainingSeconds);
            CountdownText = ts.ToString(@"hh\:mm\:ss");
        }
    }
}
