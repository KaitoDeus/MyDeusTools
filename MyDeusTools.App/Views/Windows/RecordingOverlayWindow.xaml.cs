using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace MyDeusTools.App.Views.Windows
{
    public partial class RecordingOverlayWindow : Window
    {
        public event Action<int, int>? PointRecorded;
        public event Action? RecordingCanceled;

        public RecordingOverlayWindow()
        {
            InitializeComponent();
            
            // Che phủ toàn bộ các màn hình
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            // Lấy tọa độ tương đối với màn hình ảo
            var pos = e.GetPosition(this);
            
            // Tính toán tọa độ thực tế trên màn hình Windows
            int realX = (int)(pos.X + SystemParameters.VirtualScreenLeft);
            int realY = (int)(pos.Y + SystemParameters.VirtualScreenTop);

            CoordsText.Text = $"X: {realX}, Y: {realY}";

            // Di chuyển tooltip theo chuột (cách một đoạn nhỏ)
            Canvas.SetLeft(TooltipBorder, pos.X + 15);
            Canvas.SetTop(TooltipBorder, pos.Y + 15);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            int realX = (int)(pos.X + SystemParameters.VirtualScreenLeft);
            int realY = (int)(pos.Y + SystemParameters.VirtualScreenTop);

            // Bắn event ghi tọa độ
            PointRecorded?.Invoke(realX, realY);
            
            // Đóng cửa sổ sau 1 click
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                RecordingCanceled?.Invoke();
                Close();
            }
        }
    }
}
