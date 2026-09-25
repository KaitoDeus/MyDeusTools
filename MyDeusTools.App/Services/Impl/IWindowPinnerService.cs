using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDeusTools.App.Services.Impl
{
    public class WindowInfoModel : ObservableObject
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public int ProcessId { get; set; }

        private bool _isTopMost;
        public bool IsTopMost
        {
            get => _isTopMost;
            set => SetProperty(ref _isTopMost, value);
        }

        private byte _opacity = 255;
        public byte Opacity
        {
            get => _opacity;
            set
            {
                if (SetProperty(ref _opacity, value))
                {
                    OnPropertyChanged(nameof(OpacityPercent));
                    OnOpacityChanged?.Invoke(this);
                }
            }
        }

        public int OpacityPercent => (int)Math.Round(Opacity / 255.0 * 100);
        public string DisplayText => string.IsNullOrWhiteSpace(Title) ? ProcessName : Title;

        [System.Text.Json.Serialization.JsonIgnore]
        public Action<WindowInfoModel>? OnOpacityChanged { get; set; }
    }

    public interface IWindowPinnerService
    {
        List<WindowInfoModel> GetOpenWindows();
        bool SetTopMost(IntPtr hwnd, bool topMost);
        bool ToggleTopMost(IntPtr hwnd);
        bool SetOpacity(IntPtr hwnd, byte opacity);
        bool BringToFront(IntPtr hwnd);
        IntPtr GetForegroundWindowHandle();
        bool IsWindowTopMost(IntPtr hwnd);
        byte GetWindowOpacity(IntPtr hwnd);
    }
}
