using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class ClipboardService : IClipboardService
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private readonly string _filePath;
        private readonly int _maxItems;
        private readonly object _lock = new();
        private readonly System.Threading.SemaphoreSlim _saveSemaphore = new(1, 1);
        private bool _isInternalCopy = false;
        private IntPtr _monitoredHwnd = IntPtr.Zero;

        public ObservableCollection<ClipboardItemModel> Items { get; } = new();
        public event Action? ClipboardChanged;

        public ClipboardService(string? customFilePath = null, int maxItems = 100)
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
                _filePath = Path.Combine(folder, "clipboard.json");
            }

            LoadHistory();
        }

        public void StartMonitoring(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || _monitoredHwnd != IntPtr.Zero) return;

            try
            {
                if (AddClipboardFormatListener(hwnd))
                {
                    _monitoredHwnd = hwnd;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đăng ký theo dõi Clipboard: {ex.Message}");
            }
        }

        public void StopMonitoring(IntPtr hwnd)
        {
            IntPtr targetHwnd = hwnd != IntPtr.Zero ? hwnd : _monitoredHwnd;
            if (targetHwnd == IntPtr.Zero) return;

            try
            {
                RemoveClipboardFormatListener(targetHwnd);
                if (_monitoredHwnd == targetHwnd)
                {
                    _monitoredHwnd = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hủy đăng ký theo dõi Clipboard: {ex.Message}");
            }
        }

        public void ProcessClipboardUpdate()
        {
            if (_isInternalCopy)
            {
                _isInternalCopy = false;
                return;
            }

            var app = Application.Current;
            if (app == null) return;

            app.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (!Clipboard.ContainsText()) return;
                    string text = Clipboard.GetText();
                    if (string.IsNullOrWhiteSpace(text)) return;

                    AddItem(text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc Clipboard: {ex.Message}");
                }
            });
        }

        public void AddItem(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            lock (_lock)
            {
                // Kiểm tra xem mục mới nhất có trùng nội dung không
                var existingIndex = -1;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (string.Equals(Items[i].Content, text, StringComparison.Ordinal))
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex == 0)
                {
                    // Đã ở vị trí đầu tiên, cập nhật thời gian
                    Items[0].CopiedAt = DateTime.Now;
                    _ = SaveHistoryAsync();
                    ClipboardChanged?.Invoke();
                    return;
                }

                bool isPinned = false;
                if (existingIndex > 0)
                {
                    isPinned = Items[existingIndex].IsPinned;
                    Items.RemoveAt(existingIndex);
                }

                var newItem = new ClipboardItemModel
                {
                    Content = text,
                    CopiedAt = DateTime.Now,
                    IsPinned = isPinned
                };

                // Đưa lên đầu danh sách
                Items.Insert(0, newItem);

                // Giới hạn số lượng mục tối đa (giữ lại các mục ghim)
                while (Items.Count > _maxItems)
                {
                    // Tìm mục không ghim cuối cùng để xóa
                    int removeIndex = -1;
                    for (int i = Items.Count - 1; i >= 0; i--)
                    {
                        if (!Items[i].IsPinned)
                        {
                            removeIndex = i;
                            break;
                        }
                    }

                    if (removeIndex >= 0)
                    {
                        Items.RemoveAt(removeIndex);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            _ = SaveHistoryAsync();
            ClipboardChanged?.Invoke();
        }

        public void CopyItem(ClipboardItemModel item)
        {
            if (item == null || string.IsNullOrEmpty(item.Content)) return;

            try
            {
                _isInternalCopy = true;
                Clipboard.SetText(item.Content);

                // Cập nhật lại thời gian mục này và đưa lên đầu
                lock (_lock)
                {
                    item.CopiedAt = DateTime.Now;
                    int currentIndex = Items.IndexOf(item);
                    if (currentIndex > 0)
                    {
                        Items.Move(currentIndex, 0);
                    }
                }

                _ = SaveHistoryAsync();
                ClipboardChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _isInternalCopy = false;
                System.Diagnostics.Debug.WriteLine($"Lỗi khi sao chép Clipboard: {ex.Message}");
            }
        }

        public void RemoveItem(ClipboardItemModel item)
        {
            if (item == null) return;

            lock (_lock)
            {
                Items.Remove(item);
            }

            _ = SaveHistoryAsync();
            ClipboardChanged?.Invoke();
        }

        public void TogglePin(ClipboardItemModel item)
        {
            if (item == null) return;

            item.IsPinned = !item.IsPinned;
            _ = SaveHistoryAsync();
            ClipboardChanged?.Invoke();
        }

        public void ClearHistory(bool keepPinned = true)
        {
            lock (_lock)
            {
                if (keepPinned)
                {
                    var itemsToRemove = Items.Where(x => !x.IsPinned).ToList();
                    foreach (var item in itemsToRemove)
                    {
                        Items.Remove(item);
                    }
                }
                else
                {
                    Items.Clear();
                }
            }

            _ = SaveHistoryAsync();
            ClipboardChanged?.Invoke();
        }

        public async Task SaveHistoryAsync()
        {
            await _saveSemaphore.WaitAsync();
            try
            {
                List<ClipboardItemModel> itemsToSave;
                lock (_lock)
                {
                    itemsToSave = Items.ToList();
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
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu lịch sử Clipboard: {ex.Message}");
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
                    var loadedItems = JsonSerializer.Deserialize<List<ClipboardItemModel>>(json);
                    if (loadedItems != null)
                    {
                        lock (_lock)
                        {
                            Items.Clear();
                            foreach (var item in loadedItems)
                            {
                                Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc lịch sử Clipboard: {ex.Message}");
            }
        }
    }
}
