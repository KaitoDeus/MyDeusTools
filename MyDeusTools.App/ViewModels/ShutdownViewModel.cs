using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class ShutdownViewModel : ObservableObject
    {
        private readonly ISystemService _systemService;

        // Các thuộc tính nhập liệu
        private int _hours = 0;
        public int Hours { get => _hours; set => SetProperty(ref _hours, value); }

        private int _minutes = 30;
        public int Minutes { get => _minutes; set => SetProperty(ref _minutes, value); }

        private int _seconds = 0;
        public int Seconds { get => _seconds; set => SetProperty(ref _seconds, value); }

        private string _statusMessage = "Sẵn sàng";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ShutdownViewModel(ISystemService systemService)
        {
            _systemService = systemService;
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

            _systemService.ScheduleShutdown(totalSeconds);
            StatusMessage = $"Đã hẹn giờ tắt máy sau {Hours}h {Minutes}m {Seconds}s";
        }

        [RelayCommand]
        private void Cancel()
        {
            _systemService.CancelShutdown();
            StatusMessage = "Đã hủy lệnh tắt máy";
        }
    }
}
