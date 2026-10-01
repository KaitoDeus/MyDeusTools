using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class RenameItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _index;

        [ObservableProperty]
        private string _originalPath = string.Empty;

        [ObservableProperty]
        private string _directoryPath = string.Empty;

        [ObservableProperty]
        private string _originalName = string.Empty;

        [ObservableProperty]
        private string _originalBaseName = string.Empty;

        [ObservableProperty]
        private string _originalExtension = string.Empty;

        [ObservableProperty]
        private bool _isDirectory;

        [ObservableProperty]
        private string _newName = string.Empty;

        [ObservableProperty]
        private string _newPath = string.Empty;

        [ObservableProperty]
        private RenameItemStatus _status = RenameItemStatus.Ready;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _fileSizeFormatted = string.Empty;

        [ObservableProperty]
        private DateTime _dateModified;

        [ObservableProperty]
        private DateTime _dateCreated;

        [ObservableProperty]
        private bool _isSelected;

        public bool HasChanged => !string.Equals(OriginalName, NewName, StringComparison.Ordinal);

        public static RenameItemViewModel FromPath(string path, int index = 0)
        {
            bool isDir = Directory.Exists(path);
            var item = new RenameItemViewModel
            {
                Index = index,
                OriginalPath = path,
                IsDirectory = isDir
            };

            if (isDir)
            {
                var dirInfo = new DirectoryInfo(path);
                item.DirectoryPath = dirInfo.Parent?.FullName ?? string.Empty;
                item.OriginalName = dirInfo.Name;
                item.OriginalBaseName = dirInfo.Name;
                item.OriginalExtension = string.Empty;
                item.DateModified = dirInfo.LastWriteTime;
                item.DateCreated = dirInfo.CreationTime;
                item.FileSizeFormatted = "-";
            }
            else
            {
                var fileInfo = new FileInfo(path);
                item.DirectoryPath = fileInfo.DirectoryName ?? string.Empty;
                item.OriginalName = fileInfo.Name;
                item.OriginalBaseName = Path.GetFileNameWithoutExtension(path);
                item.OriginalExtension = fileInfo.Extension;
                item.DateModified = fileInfo.LastWriteTime;
                item.DateCreated = fileInfo.CreationTime;
                item.FileSizeFormatted = FormatBytes(fileInfo.Exists ? fileInfo.Length : 0);
            }

            item.NewName = item.OriginalName;
            item.NewPath = item.OriginalPath;
            return item;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{(bytes / (1024.0 * 1024.0)):F1} MB";
            return $"{(bytes / (1024.0 * 1024.0 * 1024.0)):F2} GB";
        }
    }
}
