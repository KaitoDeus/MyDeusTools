using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;
using NHotkey.Wpf;
using System.Windows.Input;

namespace MyDeusTools.App.ViewModels
{
    public partial class AutoClickViewModel : ObservableObject
    {
        private readonly IAutoClickService _autoClickService;

        // Viết thủ công các thuộc tính để không phụ thuộc vào Source Generator
        private int _interval = 100;
        public int Interval
        {
            get => _interval;
            set => SetProperty(ref _interval, value);
        }

        private string _statusText = "Đã dừng";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _buttonText = "Bắt đầu (F6)";
        public string ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        public AutoClickViewModel(IAutoClickService autoClickService)
        {
            _autoClickService = autoClickService;
            
            try 
            {
                HotkeyManager.Current.AddOrReplace("ToggleAutoClick", Key.F6, ModifierKeys.None, OnHotkeyPressed);
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
                ButtonText = "Bắt đầu (F6)";
            }
            else
            {
                _autoClickService.Start(Interval);
                StatusText = "Đang chạy...";
                ButtonText = "Dừng lại (F6)";
            }
        }

        private void OnHotkeyPressed(object? sender, NHotkey.HotkeyEventArgs e)
        {
            ToggleClick();
        }
    }
}
