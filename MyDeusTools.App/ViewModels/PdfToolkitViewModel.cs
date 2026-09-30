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
    public partial class PdfMergeItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _orderNumber;

        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private long _fileSizeBytes;

        [ObservableProperty]
        private string _fileSizeFormatted = string.Empty;

        [ObservableProperty]
        private int _pageCount;
    }

    public partial class ImageToPdfItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _orderNumber;

        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private long _fileSizeBytes;

        [ObservableProperty]
        private string _fileSizeFormatted = string.Empty;
    }

    public partial class PdfToolkitViewModel : ObservableObject
    {
        private readonly IPdfToolkitService _pdfService;
        private readonly ILanguageService? _languageService;
        private CancellationTokenSource? _cts;

        private string GetLoc(string key, string fallback) =>
            _languageService?.GetString(key, fallback) ?? fallback;

        #region Common Properties

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private double _progressPercentage;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        #endregion

        #region Tab 1: Merge Properties

        public ObservableCollection<PdfMergeItemViewModel> MergeFiles { get; } = new();

        [ObservableProperty]
        private string _mergeOutputFileName = "merged_document.pdf";

        [ObservableProperty]
        private string? _mergeOutputDirectory;

        [ObservableProperty]
        private string? _lastMergedFilePath;

        [ObservableProperty]
        private bool _hasMergedResult;

        #endregion

        #region Tab 2: Split Properties

        [ObservableProperty]
        private string _splitSourceFilePath = string.Empty;

        [ObservableProperty]
        private PdfFileInfo? _splitSourceInfo;

        [ObservableProperty]
        private PdfSplitMode _selectedSplitMode = PdfSplitMode.PageRange;

        [ObservableProperty]
        private string _splitPageRange = "1";

        [ObservableProperty]
        private int _splitEveryN = 1;

        [ObservableProperty]
        private string _splitBaseFileName = "extracted";

        [ObservableProperty]
        private string? _splitOutputDirectory;

        [ObservableProperty]
        private string? _lastSplitOutputDirectory;

        [ObservableProperty]
        private bool _hasSplitResult;

        #endregion

        #region Tab 3: Images to PDF Properties

        public ObservableCollection<ImageToPdfItemViewModel> ImageFiles { get; } = new();

        [ObservableProperty]
        private PageSizePreference _selectedPageSize = PageSizePreference.A4;

        [ObservableProperty]
        private PageOrientationPreference _selectedOrientation = PageOrientationPreference.Auto;

        [ObservableProperty]
        private PageMarginPreference _selectedMargin = PageMarginPreference.None;

        [ObservableProperty]
        private string _imagesPdfOutputFileName = "images_album.pdf";

        [ObservableProperty]
        private string? _imagesPdfOutputDirectory;

        [ObservableProperty]
        private string? _lastImagesPdfPath;

        [ObservableProperty]
        private bool _hasImagesPdfResult;

        #endregion

        #region Tab 4: Protect & Metadata Properties

        [ObservableProperty]
        private string _protectSourceFilePath = string.Empty;

        [ObservableProperty]
        private PdfFileInfo? _protectSourceInfo;

        [ObservableProperty]
        private string _protectUserPassword = string.Empty;

        [ObservableProperty]
        private string _protectOwnerPassword = string.Empty;

        [ObservableProperty]
        private string _protectTitle = string.Empty;

        [ObservableProperty]
        private string _protectAuthor = string.Empty;

        [ObservableProperty]
        private string _protectSubject = string.Empty;

        [ObservableProperty]
        private string _protectKeywords = string.Empty;

        [ObservableProperty]
        private string _protectOutputFileName = "protected_document.pdf";

        [ObservableProperty]
        private string? _protectOutputDirectory;

        [ObservableProperty]
        private string? _lastProtectedPdfPath;

        [ObservableProperty]
        private bool _hasProtectedPdfResult;

        #endregion

        public PdfToolkitViewModel(IPdfToolkitService pdfService, ILanguageService? languageService = null)
        {
            _pdfService = pdfService;
            _languageService = languageService;

            string defaultDocDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _mergeOutputDirectory = defaultDocDir;
            _splitOutputDirectory = defaultDocDir;
            _imagesPdfOutputDirectory = defaultDocDir;
            _protectOutputDirectory = defaultDocDir;

            if (_languageService != null)
            {
                _languageService.LanguageChanged += _ =>
                {
                    if (StatusMessage == "Sẵn sàng xử lý tài liệu PDF" || StatusMessage == "Ready to process PDF documents")
                    {
                        StatusMessage = GetLoc("Pdf_StatusReady", "Sẵn sàng xử lý tài liệu PDF");
                    }
                };
            }

            _statusMessage = GetLoc("Pdf_StatusReady", "Sẵn sàng xử lý tài liệu PDF");
        }

        #region Tab 1: Merge Commands

        [RelayCommand]
        public void AddMergeFiles()
        {
            var dialog = new OpenFileDialog
            {
                Title = GetLoc("Pdf_DialogSelectPdfs", "Chọn các tệp PDF"),
                Multiselect = true,
                Filter = GetLoc("Pdf_FilterPdf", "Tệp PDF (*.pdf)|*.pdf|Tất cả tệp (*.*)|*.*")
            };

            if (dialog.ShowDialog() == true)
            {
                AddMergeFilePaths(dialog.FileNames);
            }
        }

        public void AddMergeFilePaths(IEnumerable<string> filePaths)
        {
            foreach (var path in filePaths)
            {
                if (!File.Exists(path) || !path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (MergeFiles.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var info = _pdfService.GetPdfInfo(path);
                var fi = new FileInfo(path);

                MergeFiles.Add(new PdfMergeItemViewModel
                {
                    FilePath = path,
                    FileName = fi.Name,
                    FileSizeBytes = fi.Length,
                    FileSizeFormatted = PdfToolkitService.FormatFileSize(fi.Length),
                    PageCount = info?.PageCount ?? 0
                });
            }

            ReindexMergeItems();
            StatusMessage = string.Format(GetLoc("Pdf_StatusMergeAdded", "Đã thêm {0} tệp PDF vào danh sách ghép."), MergeFiles.Count);
        }

        [RelayCommand]
        public void MoveMergeItemUp(PdfMergeItemViewModel? item)
        {
            if (item == null) return;
            int idx = MergeFiles.IndexOf(item);
            if (idx > 0)
            {
                MergeFiles.Move(idx, idx - 1);
                ReindexMergeItems();
            }
        }

        [RelayCommand]
        public void MoveMergeItemDown(PdfMergeItemViewModel? item)
        {
            if (item == null) return;
            int idx = MergeFiles.IndexOf(item);
            if (idx >= 0 && idx < MergeFiles.Count - 1)
            {
                MergeFiles.Move(idx, idx + 1);
                ReindexMergeItems();
            }
        }

        [RelayCommand]
        public void RemoveMergeItem(PdfMergeItemViewModel? item)
        {
            if (item != null && MergeFiles.Contains(item))
            {
                MergeFiles.Remove(item);
                ReindexMergeItems();
            }
        }

        [RelayCommand]
        public void ClearMergeFiles()
        {
            MergeFiles.Clear();
            StatusMessage = GetLoc("Pdf_StatusMergeCleared", "Đã dọn sạch danh sách ghép.");
        }

        [RelayCommand]
        public void SelectMergeOutputDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Pdf_DialogSelectFolder", "Chọn thư mục lưu tệp PDF"),
                InitialDirectory = MergeOutputDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                MergeOutputDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        public async Task StartMergeAsync()
        {
            if (MergeFiles.Count < 2)
            {
                StatusMessage = GetLoc("Pdf_ErrorMergeMinFiles", "Vui lòng chọn ít nhất 2 tệp PDF để ghép!");
                return;
            }

            string targetDir = !string.IsNullOrWhiteSpace(MergeOutputDirectory) ? MergeOutputDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outPath = Path.Combine(targetDir, !string.IsNullOrWhiteSpace(MergeOutputFileName) ? MergeOutputFileName : "merged_document.pdf");

            IsProcessing = true;
            ProgressPercentage = 0;
            HasMergedResult = false;
            StatusMessage = GetLoc("Pdf_StatusMerging", "Đang ghép các tệp PDF...");

            _cts = new CancellationTokenSource();
            var progress = new Progress<double>(p => ProgressPercentage = p);

            try
            {
                var paths = MergeFiles.Select(f => f.FilePath).ToList();
                bool success = await _pdfService.MergePdfsAsync(paths, outPath, progress, _cts.Token);
                if (success)
                {
                    LastMergedFilePath = outPath;
                    HasMergedResult = true;
                    ProgressPercentage = 100;
                    StatusMessage = string.Format(GetLoc("Pdf_StatusMergeSuccess", "Ghép PDF thành công: {0}"), Path.GetFileName(outPath));
                }
                else
                {
                    StatusMessage = GetLoc("Pdf_StatusMergeFailed", "Không thể ghép tệp PDF.");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Pdf_StatusCanceled", "Đã hủy thao tác.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Pdf_StatusError", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public void OpenMergeOutputFolder()
        {
            if (!string.IsNullOrEmpty(LastMergedFilePath) && File.Exists(LastMergedFilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{LastMergedFilePath}\"");
            }
            else if (!string.IsNullOrEmpty(MergeOutputDirectory) && Directory.Exists(MergeOutputDirectory))
            {
                Process.Start("explorer.exe", $"\"{MergeOutputDirectory}\"");
            }
        }

        [RelayCommand]
        public void OpenMergeOutputFile()
        {
            OpenFile(LastMergedFilePath);
        }

        private void ReindexMergeItems()
        {
            for (int i = 0; i < MergeFiles.Count; i++)
            {
                MergeFiles[i].OrderNumber = i + 1;
            }
        }

        #endregion

        #region Tab 2: Split Commands

        [RelayCommand]
        public void SelectSplitSourceFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = GetLoc("Pdf_DialogSelectPdf", "Chọn tệp PDF"),
                Filter = GetLoc("Pdf_FilterPdf", "Tệp PDF (*.pdf)|*.pdf|Tất cả tệp (*.*)|*.*")
            };

            if (dialog.ShowDialog() == true)
            {
                LoadSplitSource(dialog.FileName);
            }
        }

        public void LoadSplitSource(string filePath)
        {
            if (!File.Exists(filePath)) return;

            SplitSourceFilePath = filePath;
            SplitSourceInfo = _pdfService.GetPdfInfo(filePath);
            SplitBaseFileName = Path.GetFileNameWithoutExtension(filePath);
            HasSplitResult = false;

            if (SplitSourceInfo != null)
            {
                SplitPageRange = $"1-{Math.Min(SplitSourceInfo.PageCount, 3)}";
                StatusMessage = string.Format(GetLoc("Pdf_StatusLoadedSource", "Đã tải: {0} ({1} trang)"), SplitSourceInfo.FileName, SplitSourceInfo.PageCount);
            }
        }

        [RelayCommand]
        public void SelectSplitOutputDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Pdf_DialogSelectFolder", "Chọn thư mục lưu tệp PDF"),
                InitialDirectory = SplitOutputDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                SplitOutputDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        public async Task StartSplitAsync()
        {
            if (string.IsNullOrWhiteSpace(SplitSourceFilePath) || !File.Exists(SplitSourceFilePath))
            {
                StatusMessage = GetLoc("Pdf_ErrorSelectSourcePdf", "Vui lòng chọn tệp PDF nguồn!");
                return;
            }

            string targetDir = !string.IsNullOrWhiteSpace(SplitOutputDirectory) ? SplitOutputDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            IsProcessing = true;
            ProgressPercentage = 0;
            HasSplitResult = false;
            StatusMessage = GetLoc("Pdf_StatusSplitting", "Đang tách tệp PDF...");

            _cts = new CancellationTokenSource();
            var progress = new Progress<double>(p => ProgressPercentage = p);

            try
            {
                int count = await _pdfService.SplitPdfAsync(
                    SplitSourceFilePath,
                    targetDir,
                    SelectedSplitMode,
                    SplitPageRange,
                    SplitEveryN,
                    SplitBaseFileName,
                    progress,
                    _cts.Token);

                if (count > 0)
                {
                    LastSplitOutputDirectory = targetDir;
                    HasSplitResult = true;
                    ProgressPercentage = 100;
                    StatusMessage = string.Format(GetLoc("Pdf_StatusSplitSuccess", "Tách PDF hoàn tất! Đã tạo {0} tệp PDF mới."), count);
                }
                else
                {
                    StatusMessage = GetLoc("Pdf_StatusSplitFailed", "Không có trang nào được tách. Vui lòng kiểm tra lại phạm vi trang.");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Pdf_StatusCanceled", "Đã hủy thao tác.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Pdf_StatusError", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public void OpenSplitOutputFolder()
        {
            if (!string.IsNullOrEmpty(LastSplitOutputDirectory) && Directory.Exists(LastSplitOutputDirectory))
            {
                Process.Start("explorer.exe", $"\"{LastSplitOutputDirectory}\"");
            }
            else if (!string.IsNullOrEmpty(SplitOutputDirectory) && Directory.Exists(SplitOutputDirectory))
            {
                Process.Start("explorer.exe", $"\"{SplitOutputDirectory}\"");
            }
        }

        #endregion

        #region Tab 3: Images to PDF Commands

        [RelayCommand]
        public void AddImages()
        {
            var dialog = new OpenFileDialog
            {
                Title = GetLoc("Pdf_DialogSelectImages", "Chọn hình ảnh"),
                Multiselect = true,
                Filter = GetLoc("Pdf_FilterImages", "Hình ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.bmp;*.webp|Tất cả tệp (*.*)|*.*")
            };

            if (dialog.ShowDialog() == true)
            {
                AddImageFilePaths(dialog.FileNames);
            }
        }

        public void AddImageFilePaths(IEnumerable<string> filePaths)
        {
            string[] validExts = { ".png", ".jpg", ".jpeg", ".bmp", ".webp" };
            foreach (var path in filePaths)
            {
                if (!File.Exists(path) || !validExts.Contains(Path.GetExtension(path).ToLowerInvariant()))
                    continue;

                if (ImageFiles.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var fi = new FileInfo(path);
                ImageFiles.Add(new ImageToPdfItemViewModel
                {
                    FilePath = path,
                    FileName = fi.Name,
                    FileSizeBytes = fi.Length,
                    FileSizeFormatted = PdfToolkitService.FormatFileSize(fi.Length)
                });
            }

            ReindexImageItems();
            StatusMessage = string.Format(GetLoc("Pdf_StatusImagesAdded", "Đã thêm {0} ảnh vào danh sách."), ImageFiles.Count);
        }

        [RelayCommand]
        public void MoveImageItemUp(ImageToPdfItemViewModel? item)
        {
            if (item == null) return;
            int idx = ImageFiles.IndexOf(item);
            if (idx > 0)
            {
                ImageFiles.Move(idx, idx - 1);
                ReindexImageItems();
            }
        }

        [RelayCommand]
        public void MoveImageItemDown(ImageToPdfItemViewModel? item)
        {
            if (item == null) return;
            int idx = ImageFiles.IndexOf(item);
            if (idx >= 0 && idx < ImageFiles.Count - 1)
            {
                ImageFiles.Move(idx, idx + 1);
                ReindexImageItems();
            }
        }

        [RelayCommand]
        public void RemoveImageItem(ImageToPdfItemViewModel? item)
        {
            if (item != null && ImageFiles.Contains(item))
            {
                ImageFiles.Remove(item);
                ReindexImageItems();
            }
        }

        [RelayCommand]
        public void ClearImages()
        {
            ImageFiles.Clear();
            StatusMessage = GetLoc("Pdf_StatusImagesCleared", "Đã xóa toàn bộ danh sách ảnh.");
        }

        [RelayCommand]
        public void SelectImagesPdfOutputDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Pdf_DialogSelectFolder", "Chọn thư mục lưu tệp PDF"),
                InitialDirectory = ImagesPdfOutputDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                ImagesPdfOutputDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        public async Task StartImagesToPdfAsync()
        {
            if (ImageFiles.Count == 0)
            {
                StatusMessage = GetLoc("Pdf_ErrorSelectImagesPrompt", "Vui lòng chọn ít nhất một hình ảnh!");
                return;
            }

            string targetDir = !string.IsNullOrWhiteSpace(ImagesPdfOutputDirectory) ? ImagesPdfOutputDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outPath = Path.Combine(targetDir, !string.IsNullOrWhiteSpace(ImagesPdfOutputFileName) ? ImagesPdfOutputFileName : "images_album.pdf");

            IsProcessing = true;
            ProgressPercentage = 0;
            HasImagesPdfResult = false;
            StatusMessage = GetLoc("Pdf_StatusConvertingImages", "Đang chuyển đổi hình ảnh sang PDF...");

            _cts = new CancellationTokenSource();
            var progress = new Progress<double>(p => ProgressPercentage = p);

            try
            {
                var paths = ImageFiles.Select(f => f.FilePath).ToList();
                bool success = await _pdfService.ImagesToPdfAsync(
                    paths,
                    outPath,
                    SelectedPageSize,
                    SelectedOrientation,
                    SelectedMargin,
                    progress,
                    _cts.Token);

                if (success)
                {
                    LastImagesPdfPath = outPath;
                    HasImagesPdfResult = true;
                    ProgressPercentage = 100;
                    StatusMessage = string.Format(GetLoc("Pdf_StatusImagesPdfSuccess", "Tạo file PDF từ ảnh thành công: {0}"), Path.GetFileName(outPath));
                }
                else
                {
                    StatusMessage = GetLoc("Pdf_StatusImagesPdfFailed", "Không thể tạo file PDF từ ảnh.");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Pdf_StatusCanceled", "Đã hủy thao tác.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Pdf_StatusError", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public void OpenImagesPdfOutputFolder()
        {
            if (!string.IsNullOrEmpty(LastImagesPdfPath) && File.Exists(LastImagesPdfPath))
            {
                Process.Start("explorer.exe", $"/select,\"{LastImagesPdfPath}\"");
            }
            else if (!string.IsNullOrEmpty(ImagesPdfOutputDirectory) && Directory.Exists(ImagesPdfOutputDirectory))
            {
                Process.Start("explorer.exe", $"\"{ImagesPdfOutputDirectory}\"");
            }
        }

        [RelayCommand]
        public void OpenImagesPdfOutputFile()
        {
            OpenFile(LastImagesPdfPath);
        }

        private void ReindexImageItems()
        {
            for (int i = 0; i < ImageFiles.Count; i++)
            {
                ImageFiles[i].OrderNumber = i + 1;
            }
        }

        #endregion

        #region Tab 4: Protect & Metadata Commands

        [RelayCommand]
        public void SelectProtectSourceFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = GetLoc("Pdf_DialogSelectPdf", "Chọn tệp PDF"),
                Filter = GetLoc("Pdf_FilterPdf", "Tệp PDF (*.pdf)|*.pdf|Tất cả tệp (*.*)|*.*")
            };

            if (dialog.ShowDialog() == true)
            {
                LoadProtectSource(dialog.FileName);
            }
        }

        public void LoadProtectSource(string filePath)
        {
            if (!File.Exists(filePath)) return;

            ProtectSourceFilePath = filePath;
            ProtectSourceInfo = _pdfService.GetPdfInfo(filePath);
            HasProtectedPdfResult = false;

            if (ProtectSourceInfo != null)
            {
                ProtectTitle = ProtectSourceInfo.Title;
                ProtectAuthor = ProtectSourceInfo.Author;
                ProtectSubject = ProtectSourceInfo.Subject;
                ProtectKeywords = ProtectSourceInfo.Keywords;

                string baseName = Path.GetFileNameWithoutExtension(filePath);
                ProtectOutputFileName = $"{baseName}_protected.pdf";

                StatusMessage = string.Format(GetLoc("Pdf_StatusLoadedSource", "Đã tải: {0} ({1} trang)"), ProtectSourceInfo.FileName, ProtectSourceInfo.PageCount);
            }
        }

        [RelayCommand]
        public void SelectProtectOutputDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = GetLoc("Pdf_DialogSelectFolder", "Chọn thư mục lưu tệp PDF"),
                InitialDirectory = ProtectOutputDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                ProtectOutputDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        public async Task StartProtectAsync()
        {
            if (string.IsNullOrWhiteSpace(ProtectSourceFilePath) || !File.Exists(ProtectSourceFilePath))
            {
                StatusMessage = GetLoc("Pdf_ErrorSelectSourcePdf", "Vui lòng chọn tệp PDF nguồn!");
                return;
            }

            string targetDir = !string.IsNullOrWhiteSpace(ProtectOutputDirectory) ? ProtectOutputDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outPath = Path.Combine(targetDir, !string.IsNullOrWhiteSpace(ProtectOutputFileName) ? ProtectOutputFileName : "protected_document.pdf");

            IsProcessing = true;
            ProgressPercentage = 0;
            HasProtectedPdfResult = false;
            StatusMessage = GetLoc("Pdf_StatusProtecting", "Đang lưu và bảo vệ tệp PDF...");

            _cts = new CancellationTokenSource();

            try
            {
                bool success = await _pdfService.ProtectPdfAsync(
                    ProtectSourceFilePath,
                    outPath,
                    ProtectUserPassword,
                    ProtectOwnerPassword,
                    ProtectTitle,
                    ProtectAuthor,
                    ProtectSubject,
                    ProtectKeywords,
                    _cts.Token);

                if (success)
                {
                    LastProtectedPdfPath = outPath;
                    HasProtectedPdfResult = true;
                    ProgressPercentage = 100;
                    StatusMessage = string.Format(GetLoc("Pdf_StatusProtectSuccess", "Lưu bảo vệ PDF thành công: {0}"), Path.GetFileName(outPath));
                }
                else
                {
                    StatusMessage = GetLoc("Pdf_StatusProtectFailed", "Không thể lưu tệp PDF.");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = GetLoc("Pdf_StatusCanceled", "Đã hủy thao tác.");
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(GetLoc("Pdf_StatusError", "Lỗi: {0}"), ex.Message);
            }
            finally
            {
                IsProcessing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        public void OpenProtectOutputFolder()
        {
            if (!string.IsNullOrEmpty(LastProtectedPdfPath) && File.Exists(LastProtectedPdfPath))
            {
                Process.Start("explorer.exe", $"/select,\"{LastProtectedPdfPath}\"");
            }
            else if (!string.IsNullOrEmpty(ProtectOutputDirectory) && Directory.Exists(ProtectOutputDirectory))
            {
                Process.Start("explorer.exe", $"\"{ProtectOutputDirectory}\"");
            }
        }

        [RelayCommand]
        public void OpenProtectOutputFile()
        {
            OpenFile(LastProtectedPdfPath);
        }

        #endregion

        private static void OpenFile(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
    }
}
