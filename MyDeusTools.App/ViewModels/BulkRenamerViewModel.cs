using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class BulkRenamerViewModel : ObservableObject
    {
        private readonly IBulkRenamerService _renamerService;
        private readonly ILanguageService? _languageService;
        private CancellationTokenSource? _cts;

        public BulkRenamerViewModel(IBulkRenamerService renamerService, ILanguageService? languageService = null)
        {
            _renamerService = renamerService ?? throw new ArgumentNullException(nameof(renamerService));
            _languageService = languageService;
            _statusMessage = GetLoc("Renamer_StatusReadyPrompt", "Kéo thả tệp hoặc thư mục vào danh sách để bắt đầu.");
        }

        private string GetLoc(string key, string fallback) =>
            _languageService?.GetString(key, fallback) ?? fallback;

        #region Collections & Stats

        public ObservableCollection<RenameItemViewModel> Items { get; } = new();

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _readyCount;

        [ObservableProperty]
        private int _conflictCount;

        [ObservableProperty]
        private int _unchangedCount;

        [ObservableProperty]
        private bool _hasItems;

        [ObservableProperty]
        private bool _hasReadyItems;

        [ObservableProperty]
        private bool _canUndo;

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private double _progressPercentage;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _scanRecursive = true;

        public List<RenameHistoryEntry> LastHistory { get; } = new();

        #endregion

        #region Configuration Properties

        // Target scope
        [ObservableProperty]
        private RenameTargetType _targetType = RenameTargetType.FilesAndFolders;

        partial void OnTargetTypeChanged(RenameTargetType value) => RefreshPreview();

        [ObservableProperty]
        private bool _includeExtension;

        partial void OnIncludeExtensionChanged(bool value) => RefreshPreview();

        // 1. Find & Replace
        [ObservableProperty]
        private string _findText = string.Empty;

        partial void OnFindTextChanged(string value) => RefreshPreview();

        [ObservableProperty]
        private string _replaceText = string.Empty;

        partial void OnReplaceTextChanged(string value) => RefreshPreview();

        [ObservableProperty]
        private bool _matchCase;

        partial void OnMatchCaseChanged(bool value) => RefreshPreview();

        [ObservableProperty]
        private bool _useRegex;

        partial void OnUseRegexChanged(bool value) => RefreshPreview();

        // 2. Insert & Remove
        [ObservableProperty]
        private string _prefix = string.Empty;

        partial void OnPrefixChanged(string value) => RefreshPreview();

        [ObservableProperty]
        private string _suffix = string.Empty;

        partial void OnSuffixChanged(string value) => RefreshPreview();

        [ObservableProperty]
        private string _insertText = string.Empty;

        partial void OnInsertTextChanged(string value) => RefreshPreview();

        [ObservableProperty]
        private int _insertPosition;

        partial void OnInsertPositionChanged(int value) => RefreshPreview();

        [ObservableProperty]
        private int _removeFirstN;

        partial void OnRemoveFirstNChanged(int value) => RefreshPreview();

        [ObservableProperty]
        private int _removeLastN;

        partial void OnRemoveLastNChanged(int value) => RefreshPreview();

        [ObservableProperty]
        private bool _trimWhitespace;

        partial void OnTrimWhitespaceChanged(bool value) => RefreshPreview();

        // 3. Case Conversion
        [ObservableProperty]
        private CaseConversionMode _selectedCaseMode = CaseConversionMode.None;

        partial void OnSelectedCaseModeChanged(CaseConversionMode value) => RefreshPreview();

        // 4. Numbering
        [ObservableProperty]
        private bool _enableNumbering;

        partial void OnEnableNumberingChanged(bool value) => RefreshPreview();

        [ObservableProperty]
        private NumberingPlacement _numberPlacement = NumberingPlacement.Suffix;

        partial void OnNumberPlacementChanged(NumberingPlacement value) => RefreshPreview();

        [ObservableProperty]
        private int _startNumber = 1;

        partial void OnStartNumberChanged(int value) => RefreshPreview();

        [ObservableProperty]
        private int _numberStep = 1;

        partial void OnNumberStepChanged(int value) => RefreshPreview();

        [ObservableProperty]
        private int _zeroPadding = 2;

        partial void OnZeroPaddingChanged(int value) => RefreshPreview();

        [ObservableProperty]
        private string _numberSeparator = "_";

        partial void OnNumberSeparatorChanged(string value) => RefreshPreview();

        // 5. Date & Time
        [ObservableProperty]
        private DatePlacement _datePlacement = DatePlacement.None;

        partial void OnDatePlacementChanged(DatePlacement value) => RefreshPreview();

        [ObservableProperty]
        private DateSource _dateSource = DateSource.CurrentDate;

        partial void OnDateSourceChanged(DateSource value) => RefreshPreview();

        [ObservableProperty]
        private string _dateFormat = "yyyy-MM-dd";

        partial void OnDateFormatChanged(string value) => RefreshPreview();

        [ObservableProperty]
        private string _dateSeparator = "_";

        partial void OnDateSeparatorChanged(string value) => RefreshPreview();

        #endregion

        #region ComboBox Sources

        public IReadOnlyList<RenameTargetType> TargetTypes { get; } =
            (RenameTargetType[])Enum.GetValues(typeof(RenameTargetType));

        public IReadOnlyList<CaseConversionMode> CaseModes { get; } =
            (CaseConversionMode[])Enum.GetValues(typeof(CaseConversionMode));

        public IReadOnlyList<NumberingPlacement> NumberPlacements { get; } =
            (NumberingPlacement[])Enum.GetValues(typeof(NumberingPlacement));

        public IReadOnlyList<DatePlacement> DatePlacements { get; } =
            (DatePlacement[])Enum.GetValues(typeof(DatePlacement));

        public IReadOnlyList<DateSource> DateSources { get; } =
            (DateSource[])Enum.GetValues(typeof(DateSource));

        #endregion

        #region Public Methods & Commands

        public RenameConfig GetCurrentConfig()
        {
            return new RenameConfig
            {
                TargetType = TargetType,
                IncludeExtension = IncludeExtension,
                FindText = FindText,
                ReplaceText = ReplaceText,
                MatchCase = MatchCase,
                UseRegex = UseRegex,
                Prefix = Prefix,
                Suffix = Suffix,
                InsertText = InsertText,
                InsertPosition = InsertPosition,
                RemoveFirstN = RemoveFirstN,
                RemoveLastN = RemoveLastN,
                TrimWhitespace = TrimWhitespace,
                CaseMode = SelectedCaseMode,
                EnableNumbering = EnableNumbering,
                NumberPlacement = NumberPlacement,
                StartNumber = StartNumber,
                NumberStep = NumberStep,
                ZeroPadding = ZeroPadding,
                NumberSeparator = NumberSeparator,
                DatePlacement = DatePlacement,
                DateSource = DateSource,
                DateFormat = DateFormat,
                DateSeparator = DateSeparator
            };
        }

        [RelayCommand]
        public void RefreshPreview()
        {
            if (Items.Count == 0)
            {
                UpdateStats();
                return;
            }

            var config = GetCurrentConfig();
            _renamerService.UpdatePreviews(Items, config);
            UpdateStats();
        }

        public void AddPaths(IEnumerable<string> paths)
        {
            if (paths == null) return;

            var scanned = _renamerService.ScanPaths(paths, ScanRecursive, TargetType);
            var existingPaths = new HashSet<string>(Items.Select(i => i.OriginalPath), StringComparer.OrdinalIgnoreCase);

            int addedCount = 0;
            foreach (var path in scanned)
            {
                if (!existingPaths.Contains(path))
                {
                    Items.Add(RenameItemViewModel.FromPath(path, Items.Count + 1));
                    existingPaths.Add(path);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                RefreshPreview();
                StatusMessage = string.Format(GetLoc("Renamer_StatusItemsAdded", "Đã thêm {0} mục vào danh sách."), addedCount);
            }
        }

        [RelayCommand]
        public void AddFiles()
        {
            var dialog = new OpenFileDialog
            {
                Title = GetLoc("Renamer_DialogSelectFiles", "Chọn tệp cần đổi tên"),
                Multiselect = true,
                Filter = GetLoc("Renamer_FilterAllFiles", "Tất cả tệp (*.*)|*.*")
            };

            if (dialog.ShowDialog() == true && dialog.FileNames.Length > 0)
            {
                AddPaths(dialog.FileNames);
            }
        }

        [RelayCommand]
        public void AddFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Renamer_DialogSelectFolder", "Chọn thư mục chứa tệp cần đổi tên")
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
            {
                AddPaths(new[] { dialog.FolderName });
            }
        }

        [RelayCommand]
        public void ClearList()
        {
            Items.Clear();
            UpdateStats();
            StatusMessage = GetLoc("Renamer_StatusCleared", "Đã xóa toàn bộ danh sách.");
        }

        [RelayCommand]
        public void RemoveItem(RenameItemViewModel? item)
        {
            if (item != null)
            {
                Items.Remove(item);
                RefreshPreview();
            }
        }

        [RelayCommand]
        public void RemoveSelected()
        {
            var selected = Items.Where(i => i.IsSelected).ToList();
            if (selected.Count > 0)
            {
                foreach (var item in selected)
                {
                    Items.Remove(item);
                }
                RefreshPreview();
                StatusMessage = string.Format(GetLoc("Renamer_StatusSelectedRemoved", "Đã xóa {0} mục được chọn."), selected.Count);
            }
        }

        [RelayCommand]
        public void SelectAll()
        {
            foreach (var item in Items)
            {
                item.IsSelected = true;
            }
        }

        [RelayCommand]
        public void DeselectAll()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
        }

        [RelayCommand]
        public void OpenContainingFolder(RenameItemViewModel? item)
        {
            if (item == null) return;

            string targetDir = Directory.Exists(item.OriginalPath)
                ? item.OriginalPath
                : item.DirectoryPath;

            if (Directory.Exists(targetDir))
            {
                try
                {
                    if (File.Exists(item.OriginalPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"/select,\"{item.OriginalPath}\"",
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = targetDir,
                            UseShellExecute = true
                        });
                    }
                }
                catch
                {
                    // Ignore explorer launch issues
                }
            }
        }

        [RelayCommand]
        public async Task ApplyRenameAsync()
        {
            if (!HasReadyItems || IsProcessing) return;

            IsProcessing = true;
            ProgressPercentage = 0;
            StatusMessage = GetLoc("Renamer_StatusExecuting", "Đang tiến hành đổi tên...");

            _cts = new CancellationTokenSource();
            var progress = new Progress<double>(p => ProgressPercentage = p);

            try
            {
                var result = await _renamerService.ExecuteRenameAsync(Items, progress, _cts.Token);

                if (result.History.Count > 0)
                {
                    LastHistory.Clear();
                    LastHistory.AddRange(result.History);
                    CanUndo = true;
                }

                if (result.FailedCount > 0)
                {
                    StatusMessage = string.Format(
                        GetLoc("Renamer_StatusRenamePartial", "Đổi tên hoàn tất: {0} thành công, {1} lỗi."),
                        result.SuccessCount,
                        result.FailedCount);
                }
                else
                {
                    StatusMessage = string.Format(
                        GetLoc("Renamer_StatusRenameSuccess", "Đổi tên thành công {0} mục!"),
                        result.SuccessCount);
                }

                RefreshPreview();
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Renamer_StatusCanceled", "Đã hủy thao tác.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Renamer_StatusError", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public async Task UndoAsync()
        {
            if (!CanUndo || LastHistory.Count == 0 || IsProcessing) return;

            IsProcessing = true;
            ProgressPercentage = 0;
            StatusMessage = GetLoc("Renamer_StatusUndoing", "Đang hoàn tác lần đổi tên gần nhất...");

            _cts = new CancellationTokenSource();
            var progress = new Progress<double>(p => ProgressPercentage = p);

            try
            {
                var result = await _renamerService.UndoRenameAsync(LastHistory, progress, _cts.Token);
                LastHistory.Clear();
                CanUndo = false;

                // Re-sync items from current paths
                foreach (var item in Items)
                {
                    if (File.Exists(item.OriginalPath) || Directory.Exists(item.OriginalPath))
                    {
                        var reloaded = RenameItemViewModel.FromPath(item.OriginalPath, item.Index);
                        item.OriginalName = reloaded.OriginalName;
                        item.OriginalBaseName = reloaded.OriginalBaseName;
                        item.OriginalExtension = reloaded.OriginalExtension;
                        item.Status = RenameItemStatus.Reverted;
                        item.StatusMessage = "Đã hoàn tác";
                    }
                }

                StatusMessage = string.Format(
                    GetLoc("Renamer_StatusUndoSuccess", "Hoàn tác thành công {0} mục!"),
                    result.SuccessCount);

                RefreshPreview();
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Renamer_StatusCanceled", "Đã hủy thao tác.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Renamer_StatusError", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            _cts?.Cancel();
        }

        private void UpdateStats()
        {
            TotalCount = Items.Count;
            ReadyCount = Items.Count(i => i.Status == RenameItemStatus.Ready);
            ConflictCount = Items.Count(i =>
                i.Status == RenameItemStatus.ConflictDuplicate ||
                i.Status == RenameItemStatus.ConflictExists ||
                i.Status == RenameItemStatus.InvalidCharacters);
            UnchangedCount = Items.Count(i => i.Status == RenameItemStatus.Unchanged);
            HasItems = TotalCount > 0;
            HasReadyItems = ReadyCount > 0;
        }

        #endregion
    }
}
