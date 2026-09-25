using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class ColorPickerService : IColorPickerService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        private readonly string _filePath;
        private readonly int _maxItems;
        private readonly object _lock = new();
        private readonly System.Threading.SemaphoreSlim _saveSemaphore = new(1, 1);

        public ObservableCollection<ColorItemModel> History { get; } = new();
        public event Action? HistoryChanged;

        public ColorPickerService(string? customFilePath = null, int maxItems = 50)
        {
            _maxItems = maxItems;

            if (!string.IsNullOrEmpty(customFilePath))
            {
                _filePath = customFilePath;
            }
            else
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                _filePath = Path.Combine(folder, "colors.json");
            }

            LoadHistory();
        }

        public (byte r, byte g, byte b) GetPixelColorAt(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            try
            {
                uint pixel = GetPixel(hdc, x, y);
                // Win32 COLORREF format: 0x00BBGGRR
                byte r = (byte)(pixel & 0x000000FF);
                byte g = (byte)((pixel & 0x0000FF00) >> 8);
                byte b = (byte)((pixel & 0x00FF0000) >> 16);
                return (r, g, b);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        public BitmapSource? CaptureMagnifierArea(int centerX, int centerY, int radius = 5)
        {
            int diam = radius * 2 + 1;
            int startX = centerX - radius;
            int startY = centerY - radius;

            try
            {
                using var bmp = new System.Drawing.Bitmap(diam, diam);
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(startX, startY, 0, 0, new System.Drawing.Size(diam, diam));
                }

                IntPtr hBitmap = bmp.GetHbitmap();
                try
                {
                    var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    bitmapSource.Freeze();
                    return bitmapSource;
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi chụp vùng kính lúp: {ex.Message}");
                return null;
            }
        }

        public ColorItemModel AddColor(byte r, byte g, byte b)
        {
            string hex = $"#{r:X2}{g:X2}{b:X2}";

            ColorItemModel item;
            lock (_lock)
            {
                int existingIndex = -1;
                for (int i = 0; i < History.Count; i++)
                {
                    if (string.Equals(History[i].Hex, hex, StringComparison.OrdinalIgnoreCase))
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex == 0)
                {
                    History[0].PickedAt = DateTime.Now;
                    _ = SaveHistoryAsync();
                    HistoryChanged?.Invoke();
                    return History[0];
                }

                bool isPinned = false;
                if (existingIndex > 0)
                {
                    isPinned = History[existingIndex].IsPinned;
                    History.RemoveAt(existingIndex);
                }

                item = new ColorItemModel
                {
                    Hex = hex,
                    R = r,
                    G = g,
                    B = b,
                    IsPinned = isPinned,
                    PickedAt = DateTime.Now
                };

                History.Insert(0, item);

                while (History.Count > _maxItems)
                {
                    int removeIdx = -1;
                    for (int i = History.Count - 1; i >= 0; i--)
                    {
                        if (!History[i].IsPinned)
                        {
                            removeIdx = i;
                            break;
                        }
                    }

                    if (removeIdx >= 0)
                    {
                        History.RemoveAt(removeIdx);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            _ = SaveHistoryAsync();
            HistoryChanged?.Invoke();
            return item;
        }

        public void RemoveColor(ColorItemModel item)
        {
            if (item == null) return;

            lock (_lock)
            {
                History.Remove(item);
            }

            _ = SaveHistoryAsync();
            HistoryChanged?.Invoke();
        }

        public void TogglePin(ColorItemModel item)
        {
            if (item == null) return;

            item.IsPinned = !item.IsPinned;
            _ = SaveHistoryAsync();
            HistoryChanged?.Invoke();
        }

        public void ClearHistory(bool keepPinned = true)
        {
            lock (_lock)
            {
                if (keepPinned)
                {
                    var unpinned = History.Where(x => !x.IsPinned).ToList();
                    foreach (var item in unpinned)
                    {
                        History.Remove(item);
                    }
                }
                else
                {
                    History.Clear();
                }
            }

            _ = SaveHistoryAsync();
            HistoryChanged?.Invoke();
        }

        public async Task SaveHistoryAsync()
        {
            await _saveSemaphore.WaitAsync();
            try
            {
                List<ColorItemModel> itemsToSave;
                lock (_lock)
                {
                    itemsToSave = History.ToList();
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(itemsToSave, options);

                string? dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu lịch sử màu: {ex.Message}");
            }
            finally
            {
                _saveSemaphore.Release();
            }
        }

        public void LoadHistory()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    var loaded = JsonSerializer.Deserialize<List<ColorItemModel>>(json);
                    if (loaded != null)
                    {
                        lock (_lock)
                        {
                            History.Clear();
                            foreach (var item in loaded)
                            {
                                History.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải lịch sử màu: {ex.Message}");
            }
        }
    }
}
