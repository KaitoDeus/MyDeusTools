using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDeusTools.App.Services.Impl
{
    public partial class ClipboardItemModel : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set
            {
                if (SetProperty(ref _content, value))
                {
                    OnPropertyChanged(nameof(CharCount));
                    OnPropertyChanged(nameof(Preview));
                }
            }
        }

        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set => SetProperty(ref _isPinned, value);
        }

        private DateTime _copiedAt = DateTime.Now;
        public DateTime CopiedAt
        {
            get => _copiedAt;
            set => SetProperty(ref _copiedAt, value);
        }

        public int CharCount => Content?.Length ?? 0;

        public string Preview
        {
            get
            {
                if (string.IsNullOrEmpty(Content)) return string.Empty;
                var singleLine = Content.Replace("\r\n", " ").Replace("\n", " ").Trim();
                return singleLine.Length > 100 ? singleLine.Substring(0, 100) + "..." : singleLine;
            }
        }
    }

    public interface IClipboardService
    {
        ObservableCollection<ClipboardItemModel> Items { get; }
        event Action? ClipboardChanged;

        void StartMonitoring(IntPtr hwnd);
        void StopMonitoring(IntPtr hwnd);
        void ProcessClipboardUpdate();

        void AddItem(string text);
        void CopyItem(ClipboardItemModel item);
        void RemoveItem(ClipboardItemModel item);
        void TogglePin(ClipboardItemModel item);
        void ClearHistory(bool keepPinned = true);

        Task SaveHistoryAsync();
        void LoadHistory();
    }
}
