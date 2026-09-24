using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class ClipboardViewModel : ObservableObject
    {
        private readonly IClipboardService _clipboardService;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private string _statusMessage = "Sẵn sàng theo dõi khay nhớ tạm";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        private bool _hasItems;
        public bool HasItems
        {
            get => _hasItems;
            set => SetProperty(ref _hasItems, value);
        }

        public ObservableCollection<ClipboardItemModel> FilteredItems { get; } = new();

        public ClipboardViewModel(IClipboardService clipboardService)
        {
            _clipboardService = clipboardService;
            _clipboardService.ClipboardChanged += OnClipboardServiceChanged;

            ApplyFilter();
        }

        private void OnClipboardServiceChanged()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                ApplyFilter();
            });
        }

        public void ApplyFilter()
        {
            var rawItems = _clipboardService.Items;
            TotalCount = rawItems.Count;

            FilteredItems.Clear();

            var query = rawItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(x => x.Content != null &&
                    x.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Ưu tiên hiển thị mục đã ghim lên đầu, sau đó sắp xếp theo thời gian mới nhất
            var sorted = query.OrderByDescending(x => x.IsPinned)
                              .ThenByDescending(x => x.CopiedAt);

            foreach (var item in sorted)
            {
                FilteredItems.Add(item);
            }

            HasItems = FilteredItems.Count > 0;
        }

        [RelayCommand]
        private void CopyItem(ClipboardItemModel item)
        {
            if (item == null) return;
            _clipboardService.CopyItem(item);
            StatusMessage = $"Đã sao chép: {item.Preview}";
            ApplyFilter();
        }

        [RelayCommand]
        private void DeleteItem(ClipboardItemModel item)
        {
            if (item == null) return;
            _clipboardService.RemoveItem(item);
            StatusMessage = "Đã xóa 1 mục khỏi lịch sử";
            ApplyFilter();
        }

        [RelayCommand]
        private void TogglePin(ClipboardItemModel item)
        {
            if (item == null) return;
            _clipboardService.TogglePin(item);
            StatusMessage = item.IsPinned ? "Đã ghim mục này" : "Đã bỏ ghim mục này";
            ApplyFilter();
        }

        [RelayCommand]
        private void ClearHistory()
        {
            _clipboardService.ClearHistory(keepPinned: true);
            StatusMessage = "Đã xóa toàn bộ lịch sử (giữ lại các mục đã ghim)";
            ApplyFilter();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }
    }
}
