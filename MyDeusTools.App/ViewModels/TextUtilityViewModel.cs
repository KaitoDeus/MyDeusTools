using System;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.ViewModels
{
    public partial class TextUtilityViewModel : ObservableObject
    {
        private readonly ITextUtilityService _textService;

        // ==================== 1. JSON STUDIO ====================
        private string _jsonInput = "{\n  \"name\": \"MyDeusTools\",\n  \"version\": \"1.0.0\",\n  \"features\": [\"AutoClicker\", \"StickyNotes\", \"Clipboard\", \"QRCode\", \"ColorPicker\", \"TextDevTools\"]\n}";
        public string JsonInput
        {
            get => _jsonInput;
            set => SetProperty(ref _jsonInput, value);
        }

        private string _jsonOutput = string.Empty;
        public string JsonOutput
        {
            get => _jsonOutput;
            set => SetProperty(ref _jsonOutput, value);
        }

        private string _jsonStatus = "Sẵn sàng định dạng hoặc kiểm tra JSON";
        public string JsonStatus
        {
            get => _jsonStatus;
            set => SetProperty(ref _jsonStatus, value);
        }

        private bool _isJsonValid = true;
        public bool IsJsonValid
        {
            get => _isJsonValid;
            set => SetProperty(ref _isJsonValid, value);
        }

        // ==================== 2. BASE64 ====================
        private string _base64Input = string.Empty;
        public string Base64Input
        {
            get => _base64Input;
            set => SetProperty(ref _base64Input, value);
        }

        private string _base64Output = string.Empty;
        public string Base64Output
        {
            get => _base64Output;
            set => SetProperty(ref _base64Output, value);
        }

        private string _base64Status = "Sẵn sàng mã hóa hoặc giải mã Base64";
        public string Base64Status
        {
            get => _base64Status;
            set => SetProperty(ref _base64Status, value);
        }

        // ==================== 3. URL & HTML ====================
        private string _urlHtmlInput = string.Empty;
        public string UrlHtmlInput
        {
            get => _urlHtmlInput;
            set => SetProperty(ref _urlHtmlInput, value);
        }

        private string _urlHtmlOutput = string.Empty;
        public string UrlHtmlOutput
        {
            get => _urlHtmlOutput;
            set => SetProperty(ref _urlHtmlOutput, value);
        }

        private string _urlHtmlStatus = "Sẵn sàng xử lý URL / HTML";
        public string UrlHtmlStatus
        {
            get => _urlHtmlStatus;
            set => SetProperty(ref _urlHtmlStatus, value);
        }

        // ==================== 4. HASH GENERATOR ====================
        private string _hashInput = string.Empty;
        public string HashInput
        {
            get => _hashInput;
            set
            {
                if (SetProperty(ref _hashInput, value))
                {
                    ComputeAllHashes(value);
                }
            }
        }

        private string _md5Hash = string.Empty;
        public string Md5Hash
        {
            get => _md5Hash;
            set => SetProperty(ref _md5Hash, value);
        }

        private string _sha1Hash = string.Empty;
        public string Sha1Hash
        {
            get => _sha1Hash;
            set => SetProperty(ref _sha1Hash, value);
        }

        private string _sha256Hash = string.Empty;
        public string Sha256Hash
        {
            get => _sha256Hash;
            set => SetProperty(ref _sha256Hash, value);
        }

        private string _sha512Hash = string.Empty;
        public string Sha512Hash
        {
            get => _sha512Hash;
            set => SetProperty(ref _sha512Hash, value);
        }

        private string _selectedHashFile = string.Empty;
        public string SelectedHashFile
        {
            get => _selectedHashFile;
            set => SetProperty(ref _selectedHashFile, value);
        }

        private string _fileMd5Hash = string.Empty;
        public string FileMd5Hash
        {
            get => _fileMd5Hash;
            set => SetProperty(ref _fileMd5Hash, value);
        }

        private string _fileSha256Hash = string.Empty;
        public string FileSha256Hash
        {
            get => _fileSha256Hash;
            set => SetProperty(ref _fileSha256Hash, value);
        }

        // ==================== 5. CASE & INSPECTOR ====================
        private string _inspectorInput = "The quick brown fox jumps over the lazy dog";
        public string InspectorInput
        {
            get => _inspectorInput;
            set
            {
                if (SetProperty(ref _inspectorInput, value))
                {
                    UpdateInspector(value);
                }
            }
        }

        private int _characterCount;
        public int CharacterCount
        {
            get => _characterCount;
            set => SetProperty(ref _characterCount, value);
        }

        private int _charactersNoSpacesCount;
        public int CharactersNoSpacesCount
        {
            get => _charactersNoSpacesCount;
            set => SetProperty(ref _charactersNoSpacesCount, value);
        }

        private int _wordCount;
        public int WordCount
        {
            get => _wordCount;
            set => SetProperty(ref _wordCount, value);
        }

        private int _lineCount;
        public int LineCount
        {
            get => _lineCount;
            set => SetProperty(ref _lineCount, value);
        }

        private int _byteCount;
        public int ByteCount
        {
            get => _byteCount;
            set => SetProperty(ref _byteCount, value);
        }

        private string _camelCase = string.Empty;
        public string CamelCase
        {
            get => _camelCase;
            set => SetProperty(ref _camelCase, value);
        }

        private string _pascalCase = string.Empty;
        public string PascalCase
        {
            get => _pascalCase;
            set => SetProperty(ref _pascalCase, value);
        }

        private string _snakeCase = string.Empty;
        public string SnakeCase
        {
            get => _snakeCase;
            set => SetProperty(ref _snakeCase, value);
        }

        private string _kebabCase = string.Empty;
        public string KebabCase
        {
            get => _kebabCase;
            set => SetProperty(ref _kebabCase, value);
        }

        private string _upperCase = string.Empty;
        public string UpperCase
        {
            get => _upperCase;
            set => SetProperty(ref _upperCase, value);
        }

        private string _lowerCase = string.Empty;
        public string LowerCase
        {
            get => _lowerCase;
            set => SetProperty(ref _lowerCase, value);
        }

        private string _titleCase = string.Empty;
        public string TitleCase
        {
            get => _titleCase;
            set => SetProperty(ref _titleCase, value);
        }

        // ==================== CONSTRUCTOR ====================
        public TextUtilityViewModel(ITextUtilityService textService)
        {
            _textService = textService;
            UpdateInspector(_inspectorInput);
            FormatJson();
        }

        // ==================== JSON COMMANDS ====================
        [RelayCommand]
        public void FormatJson()
        {
            try
            {
                var (isValid, error) = _textService.ValidateJson(JsonInput);
                if (isValid)
                {
                    JsonOutput = _textService.FormatJson(JsonInput);
                    JsonStatus = "JSON hợp lệ (Đã định dạng)";
                    IsJsonValid = true;
                }
                else
                {
                    JsonStatus = error ?? "Cú pháp JSON không hợp lệ";
                    IsJsonValid = false;
                }
            }
            catch (Exception ex)
            {
                JsonStatus = $"Lỗi: {ex.Message}";
                IsJsonValid = false;
            }
        }

        [RelayCommand]
        public void MinifyJson()
        {
            try
            {
                var (isValid, error) = _textService.ValidateJson(JsonInput);
                if (isValid)
                {
                    JsonOutput = _textService.MinifyJson(JsonInput);
                    JsonStatus = "JSON hợp lệ (Đã nén gọn)";
                    IsJsonValid = true;
                }
                else
                {
                    JsonStatus = error ?? "Cú pháp JSON không hợp lệ";
                    IsJsonValid = false;
                }
            }
            catch (Exception ex)
            {
                JsonStatus = $"Lỗi: {ex.Message}";
                IsJsonValid = false;
            }
        }

        [RelayCommand]
        public void ValidateJson()
        {
            var (isValid, error) = _textService.ValidateJson(JsonInput);
            IsJsonValid = isValid;
            JsonStatus = isValid ? "Cú pháp JSON hoàn toàn hợp lệ!" : (error ?? "JSON không hợp lệ");
        }

        [RelayCommand]
        public void ClearJson()
        {
            JsonInput = string.Empty;
            JsonOutput = string.Empty;
            JsonStatus = "Đã xóa nội dung JSON";
            IsJsonValid = true;
        }

        [RelayCommand]
        public void CopyJsonOutput()
        {
            SafeSetClipboard(JsonOutput);
            JsonStatus = "Đã sao chép kết quả vào khay nhớ tạm";
        }

        [RelayCommand]
        public void PasteJsonInput()
        {
            string text = SafeGetClipboard();
            if (!string.IsNullOrEmpty(text))
            {
                JsonInput = text;
                FormatJson();
            }
        }

        [RelayCommand]
        public void SwapJson()
        {
            if (!string.IsNullOrEmpty(JsonOutput))
            {
                (JsonInput, JsonOutput) = (JsonOutput, JsonInput);
            }
        }

        // ==================== BASE64 COMMANDS ====================
        [RelayCommand]
        public void EncodeBase64()
        {
            try
            {
                Base64Output = _textService.TextToBase64(Base64Input);
                Base64Status = "Đã mã hóa sang Base64 thành công";
            }
            catch (Exception ex)
            {
                Base64Status = $"Lỗi: {ex.Message}";
            }
        }

        [RelayCommand]
        public void DecodeBase64()
        {
            try
            {
                Base64Output = _textService.Base64ToText(Base64Input);
                Base64Status = "Đã giải mã Base64 thành công";
            }
            catch (Exception ex)
            {
                Base64Status = $"Lỗi giải mã: {ex.Message}";
            }
        }

        [RelayCommand]
        public void ClearBase64()
        {
            Base64Input = string.Empty;
            Base64Output = string.Empty;
            Base64Status = "Đã xóa nội dung";
        }

        [RelayCommand]
        public void CopyBase64Output()
        {
            SafeSetClipboard(Base64Output);
            Base64Status = "Đã sao chép kết quả Base64";
        }

        [RelayCommand]
        public void PasteBase64Input()
        {
            string text = SafeGetClipboard();
            if (!string.IsNullOrEmpty(text))
            {
                Base64Input = text;
            }
        }

        [RelayCommand]
        public void SwapBase64()
        {
            if (!string.IsNullOrEmpty(Base64Output))
            {
                (Base64Input, Base64Output) = (Base64Output, Base64Input);
            }
        }

        [RelayCommand]
        public void EncodeFileBase64()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn tập tin để chuyển sang Base64",
                Filter = "Mọi tập tin (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Base64Output = _textService.FileToBase64(dialog.FileName);
                    Base64Status = $"Đã mã hóa tập tin '{Path.GetFileName(dialog.FileName)}' sang Base64";
                }
                catch (Exception ex)
                {
                    Base64Status = $"Lỗi đọc file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void SaveBase64ToFile()
        {
            string content = !string.IsNullOrWhiteSpace(Base64Output) ? Base64Output : Base64Input;
            if (string.IsNullOrWhiteSpace(content))
            {
                Base64Status = "Không có dữ liệu Base64 để lưu tập tin.";
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Lưu dữ liệu Base64 thành tập tin",
                FileName = "decoded_file.bin",
                Filter = "Mọi tập tin (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _textService.Base64ToFile(content, dialog.FileName);
                    Base64Status = $"Đã lưu tập tin thành công: {Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    Base64Status = $"Lỗi lưu tập tin: {ex.Message}";
                }
            }
        }

        // ==================== URL & HTML COMMANDS ====================
        [RelayCommand]
        public void UrlEncode()
        {
            UrlHtmlOutput = _textService.UrlEncode(UrlHtmlInput);
            UrlHtmlStatus = "Đã mã hóa URL (Escape)";
        }

        [RelayCommand]
        public void UrlDecode()
        {
            UrlHtmlOutput = _textService.UrlDecode(UrlHtmlInput);
            UrlHtmlStatus = "Đã giải mã URL (Unescape)";
        }

        [RelayCommand]
        public void HtmlEncode()
        {
            UrlHtmlOutput = _textService.HtmlEncode(UrlHtmlInput);
            UrlHtmlStatus = "Đã mã hóa thực thể HTML";
        }

        [RelayCommand]
        public void HtmlDecode()
        {
            UrlHtmlOutput = _textService.HtmlDecode(UrlHtmlInput);
            UrlHtmlStatus = "Đã giải mã thực thể HTML";
        }

        [RelayCommand]
        public void ClearUrlHtml()
        {
            UrlHtmlInput = string.Empty;
            UrlHtmlOutput = string.Empty;
            UrlHtmlStatus = "Đã xóa nội dung URL/HTML";
        }

        [RelayCommand]
        public void CopyUrlHtmlOutput()
        {
            SafeSetClipboard(UrlHtmlOutput);
            UrlHtmlStatus = "Đã sao chép kết quả";
        }

        [RelayCommand]
        public void PasteUrlHtmlInput()
        {
            string text = SafeGetClipboard();
            if (!string.IsNullOrEmpty(text))
            {
                UrlHtmlInput = text;
            }
        }

        [RelayCommand]
        public void SwapUrlHtml()
        {
            if (!string.IsNullOrEmpty(UrlHtmlOutput))
            {
                (UrlHtmlInput, UrlHtmlOutput) = (UrlHtmlOutput, UrlHtmlInput);
            }
        }

        // ==================== HASH COMMANDS ====================
        private void ComputeAllHashes(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Md5Hash = string.Empty;
                Sha1Hash = string.Empty;
                Sha256Hash = string.Empty;
                Sha512Hash = string.Empty;
                return;
            }

            Md5Hash = _textService.ComputeMd5(text);
            Sha1Hash = _textService.ComputeSha1(text);
            Sha256Hash = _textService.ComputeSha256(text);
            Sha512Hash = _textService.ComputeSha512(text);
        }

        [RelayCommand]
        public void CopyHash(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                SafeSetClipboard(hash);
            }
        }

        [RelayCommand]
        public void ClearHash()
        {
            HashInput = string.Empty;
            SelectedHashFile = string.Empty;
            FileMd5Hash = string.Empty;
            FileSha256Hash = string.Empty;
        }

        [RelayCommand]
        public void SelectFileForHash()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn tập tin để tính mã băm (Hash)",
                Filter = "Mọi tập tin (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    SelectedHashFile = dialog.FileName;
                    FileMd5Hash = _textService.ComputeFileHash(dialog.FileName, "MD5");
                    FileSha256Hash = _textService.ComputeFileHash(dialog.FileName, "SHA256");
                }
                catch (Exception ex)
                {
                    SelectedHashFile = $"Lỗi: {ex.Message}";
                    FileMd5Hash = string.Empty;
                    FileSha256Hash = string.Empty;
                }
            }
        }

        // ==================== CASE & INSPECTOR ====================
        private void UpdateInspector(string text)
        {
            var stats = _textService.AnalyzeText(text);
            CharacterCount = stats.Characters;
            CharactersNoSpacesCount = stats.CharactersNoSpaces;
            WordCount = stats.Words;
            LineCount = stats.Lines;
            ByteCount = stats.ByteCount;

            CamelCase = _textService.ToCamelCase(text);
            PascalCase = _textService.ToPascalCase(text);
            SnakeCase = _textService.ToSnakeCase(text);
            KebabCase = _textService.ToKebabCase(text);
            UpperCase = _textService.ToUpperCase(text);
            LowerCase = _textService.ToLowerCase(text);
            TitleCase = _textService.ToTitleCase(text);
        }

        [RelayCommand]
        public void CopyCase(string text)
        {
            SafeSetClipboard(text);
        }

        [RelayCommand]
        public void ClearInspector()
        {
            InspectorInput = string.Empty;
        }

        // ==================== CLIPBOARD HELPER ====================
        private static void SafeSetClipboard(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                Clipboard.SetDataObject(text, true);
            }
            catch { }
        }

        private static string SafeGetClipboard()
        {
            try
            {
                return Clipboard.GetText() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
