using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyDeusTools.App.Views.Windows
{
    public partial class RecordingOverlayWindow : Window
    {
        public event Action<int, int>? PointRecorded;
        public event Action? RecordingCanceled;

        private int _recordedCount = 0;

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
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var pos = e.GetPosition(this);
            int realX = (int)(pos.X + SystemParameters.VirtualScreenLeft);
            int realY = (int)(pos.Y + SystemParameters.VirtualScreenTop);

            _recordedCount++;
            PointRecorded?.Invoke(realX, realY);

            // Vẽ điểm Marker tròn đánh số thứ tự trên Canvas
            DrawPointMarker(pos.X, pos.Y, _recordedCount);

            InstructionText.Text = $"Đã ghi {_recordedCount} điểm. Click tiếp để ghi thêm. Nhấn ENTER/ESC để hoàn tất.";
        }

        private void DrawPointMarker(double x, double y, int number)
        {
            var grid = new Grid
            {
                Width = 24,
                Height = 24,
                IsHitTestVisible = false
            };

            var circle = new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromRgb(255, 60, 60)),
                Stroke = Brushes.White,
                StrokeThickness = 2
            };

            var txt = new TextBlock
            {
                Text = number.ToString(),
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            grid.Children.Add(circle);
            grid.Children.Add(txt);

            Canvas.SetLeft(grid, x - 12);
            Canvas.SetTop(grid, y - 12);

            MainCanvas.Children.Add(grid);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                if (_recordedCount == 0 && e.Key == Key.Escape)
                {
                    RecordingCanceled?.Invoke();
                }
                Close();
            }
        }
    }
}
