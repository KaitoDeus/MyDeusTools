using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyDeusTools.App.Views.Windows
{
    public partial class QrSnippingOverlayWindow : Window
    {
        public event Action<int, int, int, int>? RegionSelected;
        public event Action? SnippingCanceled;

        private Point _startPoint;
        private bool _isSelecting = false;

        public QrSnippingOverlayWindow()
        {
            InitializeComponent();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _startPoint = e.GetPosition(this);
            _isSelecting = true;

            Canvas.SetLeft(SelectionRect, _startPoint.X);
            Canvas.SetTop(SelectionRect, _startPoint.Y);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
            SelectionRect.Visibility = Visibility.Visible;
            CaptureMouse();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            var currentPoint = e.GetPosition(this);

            double left = Math.Min(_startPoint.X, currentPoint.X);
            double top = Math.Min(_startPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _startPoint.X);
            double height = Math.Abs(currentPoint.Y - _startPoint.Y);

            Canvas.SetLeft(SelectionRect, left);
            Canvas.SetTop(SelectionRect, top);
            SelectionRect.Width = width;
            SelectionRect.Height = height;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting) return;

            _isSelecting = false;
            ReleaseMouseCapture();

            var currentPoint = e.GetPosition(this);

            double left = Math.Min(_startPoint.X, currentPoint.X);
            double top = Math.Min(_startPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _startPoint.X);
            double height = Math.Abs(currentPoint.Y - _startPoint.Y);

            // Kiểm tra kích thước tối thiểu để tránh vô tình click
            if (width > 20 && height > 20)
            {
                int screenX = (int)(left + SystemParameters.VirtualScreenLeft);
                int screenY = (int)(top + SystemParameters.VirtualScreenTop);
                int screenWidth = (int)width;
                int screenHeight = (int)height;

                // Ẩn overlay trước khi trigger để không chụp phải chính overlay
                Hide();
                RegionSelected?.Invoke(screenX, screenY, screenWidth, screenHeight);
            }
            else
            {
                SnippingCanceled?.Invoke();
            }

            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SnippingCanceled?.Invoke();
                Close();
            }
        }
    }
}
