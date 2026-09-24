using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Views.Windows
{
    public partial class ColorPickerOverlayWindow : Window
    {
        public event Action<byte, byte, byte>? ColorSelected;
        public event Action? PickingCanceled;

        private readonly IColorPickerService _colorPickerService;
        private byte _currentR = 255;
        private byte _currentG = 255;
        private byte _currentB = 255;

        public ColorPickerOverlayWindow(IColorPickerService colorPickerService)
        {
            _colorPickerService = colorPickerService;
            InitializeComponent();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            int screenX = (int)(pos.X + SystemParameters.VirtualScreenLeft);
            int screenY = (int)(pos.Y + SystemParameters.VirtualScreenTop);

            var (r, g, b) = _colorPickerService.GetPixelColorAt(screenX, screenY);
            _currentR = r;
            _currentG = g;
            _currentB = b;

            // Cập nhật nhãn và mẫu màu
            ColorSwatch.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            HexText.Text = $"#{r:X2}{g:X2}{b:X2}";
            RgbText.Text = $"{r}, {g}, {b}";

            // Chụp vùng zoom quanh trỏ chuột
            var zoomBitmap = _colorPickerService.CaptureMagnifierArea(screenX, screenY, 5);
            if (zoomBitmap != null)
            {
                MagnifierImage.Source = zoomBitmap;
            }

            // Định vị khung Magnifier tránh trôi ra khỏi màn hình
            double hudLeft = pos.X + 22;
            double hudTop = pos.Y + 22;

            if (hudLeft + 140 > ActualWidth)
            {
                hudLeft = pos.X - 135;
            }
            if (hudTop + 170 > ActualHeight)
            {
                hudTop = pos.Y - 165;
            }

            Canvas.SetLeft(MagnifierBorder, Math.Max(0, hudLeft));
            Canvas.SetTop(MagnifierBorder, Math.Max(0, hudTop));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            ColorSelected?.Invoke(_currentR, _currentG, _currentB);
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                PickingCanceled?.Invoke();
                Close();
            }
        }
    }
}
