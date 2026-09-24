using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDeusTools.App.Services.Impl
{
    public partial class ColorItemModel : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _hex = "#FFFFFF";
        public string Hex
        {
            get => _hex;
            set => SetProperty(ref _hex, value);
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string Rgb => $"rgb({R}, {G}, {B})";

        [System.Text.Json.Serialization.JsonIgnore]
        public string Hsl => ConvertRgbToHsl(R, G, B);

        [System.Text.Json.Serialization.JsonIgnore]
        public string Hsv => ConvertRgbToHsv(R, G, B);

        [System.Text.Json.Serialization.JsonIgnore]
        public SolidColorBrush PreviewBrush => new SolidColorBrush(Color.FromRgb(R, G, B));

        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set => SetProperty(ref _isPinned, value);
        }

        private DateTime _pickedAt = DateTime.Now;
        public DateTime PickedAt
        {
            get => _pickedAt;
            set => SetProperty(ref _pickedAt, value);
        }

        public static string ConvertRgbToHsl(byte r, byte g, byte b)
        {
            float rd = r / 255.0f;
            float gd = g / 255.0f;
            float bd = b / 255.0f;

            float max = Math.Max(rd, Math.Max(gd, bd));
            float min = Math.Min(rd, Math.Min(gd, bd));
            float diff = max - min;

            float h = 0f;
            float s = 0f;
            float l = (max + min) / 2.0f;

            if (diff > 0.0001f)
            {
                s = l > 0.5f ? diff / (2.0f - max - min) : diff / (max + min);

                if (Math.Abs(max - rd) < 0.0001f)
                {
                    h = (gd - bd) / diff + (gd < bd ? 6.0f : 0.0f);
                }
                else if (Math.Abs(max - gd) < 0.0001f)
                {
                    h = (bd - rd) / diff + 2.0f;
                }
                else
                {
                    h = (rd - gd) / diff + 4.0f;
                }
                h /= 6.0f;
            }

            int hDeg = (int)Math.Round(h * 360f);
            int sPct = (int)Math.Round(s * 100f);
            int lPct = (int)Math.Round(l * 100f);

            return $"hsl({hDeg}, {sPct}%, {lPct}%)";
        }

        public static string ConvertRgbToHsv(byte r, byte g, byte b)
        {
            float rd = r / 255.0f;
            float gd = g / 255.0f;
            float bd = b / 255.0f;

            float max = Math.Max(rd, Math.Max(gd, bd));
            float min = Math.Min(rd, Math.Min(gd, bd));
            float diff = max - min;

            float h = 0f;
            float s = max == 0 ? 0 : diff / max;
            float v = max;

            if (diff > 0.0001f)
            {
                if (Math.Abs(max - rd) < 0.0001f)
                {
                    h = (gd - bd) / diff + (gd < bd ? 6.0f : 0.0f);
                }
                else if (Math.Abs(max - gd) < 0.0001f)
                {
                    h = (bd - rd) / diff + 2.0f;
                }
                else
                {
                    h = (rd - gd) / diff + 4.0f;
                }
                h /= 6.0f;
            }

            int hDeg = (int)Math.Round(h * 360f);
            int sPct = (int)Math.Round(s * 100f);
            int vPct = (int)Math.Round(v * 100f);

            return $"hsv({hDeg}, {sPct}%, {vPct}%)";
        }
    }

    public interface IColorPickerService
    {
        ObservableCollection<ColorItemModel> History { get; }
        event Action? HistoryChanged;

        ColorItemModel AddColor(byte r, byte g, byte b);
        void RemoveColor(ColorItemModel item);
        void TogglePin(ColorItemModel item);
        void ClearHistory(bool keepPinned = true);

        (byte r, byte g, byte b) GetPixelColorAt(int x, int y);
        BitmapSource? CaptureMagnifierArea(int centerX, int centerY, int radius = 5);

        Task SaveHistoryAsync();
        void LoadHistory();
    }
}
