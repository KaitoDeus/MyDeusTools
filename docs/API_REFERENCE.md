# MyDeusTools - API Reference

This document provides a technical reference for all service interfaces, data models, and enums within `MyDeusTools.App.Services.Impl`.

---

## 1. IAutoClickService

Handles mouse simulation, click timing, waypoint replay, and background loops.

```csharp
public interface IAutoClickService
{
    bool IsRunning { get; }
    void Start(AutoClickConfig config);
    void Stop();
    void ReplayWaypoints(IReadOnlyList<ClickWaypoint> waypoints, TimeSpan interval);
}
```

### Models & Enums
- `AutoClickConfig`: `MouseButton Button`, `ClickType ClickType`, `TimeSpan Interval`, `int RepeatCount` (0 = infinite).
- `ClickWaypoint`: `int Index`, `int X`, `int Y`, `TimeSpan Delay`.
- `MouseButton`: `Left`, `Right`, `Middle`.
- `ClickType`: `Single`, `Double`.

---

## 2. ISystemService

Handles Windows power state actions and schedules.

```csharp
public interface ISystemService
{
    void ScheduleAction(ShutdownMode mode, int seconds);
    void CancelShutdown();
}
```

### Enums
- `ShutdownMode`: `Shutdown`, `Restart`, `Hibernate`.

---

## 3. IStickyNoteService

Manages creation, retrieval, updates, and deletion of desktop sticky notes.

```csharp
public interface IStickyNoteService
{
    Task<List<StickyNoteModel>> LoadNotesAsync();
    Task SaveNotesAsync(IEnumerable<StickyNoteModel> notes);
    Task<StickyNoteModel> CreateNoteAsync(string content);
    Task DeleteNoteAsync(Guid noteId);
}
```

### Models
- `StickyNoteModel`: `Guid Id`, `string Content`, `DateTime CreatedAt`, `DateTime UpdatedAt`, `bool IsPinned`, `double WindowLeft`, `double WindowTop`, `double WindowWidth`, `double WindowHeight`.

---

## 4. IClipboardService

Listens to OS clipboard updates and maintains persistent history.

```csharp
public interface IClipboardService
{
    event EventHandler<ClipboardItemModel> ItemAdded;
    Task<List<ClipboardItemModel>> GetHistoryAsync();
    Task AddItemAsync(string text);
    Task TogglePinAsync(Guid id);
    Task DeleteItemAsync(Guid id);
    Task ClearUnpinnedAsync();
}
```

### Models
- `ClipboardItemModel`: `Guid Id`, `string Text`, `DateTime Timestamp`, `bool IsPinned`, `int CharacterCount`.

---

## 5. IQrCodeService

Provides QR code image generation and decoding from multiple sources.

```csharp
public interface IQrCodeService
{
    byte[] GenerateQrCodePng(string content, int pixelsPerModule = 10);
    string? DecodeFromImageBytes(byte[] imageBytes);
    string? DecodeFromFile(string filePath);
    string? DecodeFromScreenRegion(int x, int y, int width, int height);
}
```

---

## 6. IColorPickerService

Samples screen colors, formats values, and maintains palette history.

```csharp
public interface IColorPickerService
{
    ColorSample SampleScreenPoint(int x, int y);
    ColorSample[,] SampleMagnifierGrid(int centerX, int centerY, int gridSize = 11);
    Task<List<ColorSample>> LoadPaletteAsync();
    Task SavePaletteAsync(IEnumerable<ColorSample> samples);
}
```

### Models
- `ColorSample`: `byte R`, `byte G`, `byte B`, `string Hex`, `string Rgb`, `string Hsl`, `string Hsv`, `DateTime SampledAt`, `bool IsPinned`.

---

## 7. ITextUtilityService

Pure managed .NET 8 text and developer utilities.

```csharp
public interface ITextUtilityService
{
    string FormatJson(string json, bool indent = true);
    bool ValidateJson(string json, out string? errorMessage);
    string EncodeBase64(string text);
    string DecodeBase64(string base64);
    string EncodeBase64File(string filePath);
    void DecodeBase64ToFile(string base64, string outputPath);
    string UrlEncode(string input);
    string UrlDecode(string input);
    string HtmlEncode(string input);
    string HtmlDecode(string input);
    HashResults ComputeHashes(string input);
    HashResults ComputeFileHashes(string filePath);
    string ConvertCase(string input, CaseMode mode);
    TextMetrics AnalyzeText(string input);
}
```

---

## 8. IWindowPinnerService

Interacts with Win32 window handles for Always-on-Top and opacity.

```csharp
public interface IWindowPinnerService
{
    IReadOnlyList<WindowInfo> GetDesktopWindows();
    WindowInfo? GetForegroundWindowInfo();
    void SetAlwaysOnTop(IntPtr hWnd, bool alwaysOnTop);
    void SetWindowOpacity(IntPtr hWnd, byte opacityPercent);
    void BringToFront(IntPtr hWnd);
}
```

---

## 9. IVideoConverterService

Handles media conversion, container transmuxing, audio extraction, and progress reporting.

```csharp
public interface IVideoConverterService
{
    Task ConvertMediaAsync(
        VideoConversionConfig config, 
        IProgress<VideoProgressInfo>? progress = null, 
        CancellationToken cancellationToken = default);
}
```

---

## 10. IImageStudioService

Provides batch image compression, format conversion, and multi-layer ICO generation.

```csharp
public interface IImageStudioService
{
    Task<ImageBatchResult> ProcessBatchAsync(
        IEnumerable<ImageItemViewModel> items, 
        ImageProcessingConfig config, 
        IProgress<double>? progress = null, 
        CancellationToken cancellationToken = default);
}
```

---

## 11. IPdfToolkitService

Provides offline PDF merging, splitting, image album conversion, password protection, and metadata editing.

```csharp
public interface IPdfToolkitService
{
    PdfDocumentInfo GetPdfInfo(string filePath);
    
    Task<bool> MergePdfsAsync(
        IEnumerable<string> sourceFilePaths, 
        string outputFilePath, 
        IProgress<double>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<int> SplitPdfAsync(
        string sourceFilePath, 
        string outputDirectory, 
        PdfSplitMode splitMode, 
        string? pageRange = null, 
        int chunkSize = 1, 
        IProgress<double>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<bool> ImagesToPdfAsync(
        IEnumerable<string> imageFilePaths, 
        string outputFilePath, 
        ImagesToPdfConfig config, 
        IProgress<double>? progress = null, 
        CancellationToken cancellationToken = default);

    Task<bool> ProtectPdfAsync(
        string sourceFilePath, 
        string outputFilePath, 
        PdfSecurityConfig securityConfig, 
        CancellationToken cancellationToken = default);
}
```

---

## 12. IBulkRenamerService

Executes batch file and directory renaming with rule combination, conflict validation, and undo history.

```csharp
public interface IBulkRenamerService
{
    string ApplyRenameRule(
        string originalName,
        bool isDirectory,
        RenameConfig config,
        int itemIndex,
        DateTime? dateCreated = null,
        DateTime? dateModified = null);

    void UpdatePreviews(IList<RenameItemViewModel> items, RenameConfig config);

    Task<RenameResult> ExecuteRenameAsync(
        IList<RenameItemViewModel> items,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    Task<RenameResult> UndoRenameAsync(
        IList<RenameHistoryEntry> history,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    IReadOnlyList<string> ScanPaths(IEnumerable<string> paths, bool recursive, RenameTargetType targetType);
}
```

---

## 13. ILanguageService

Runtime bilingual language management with event notifications.

```csharp
public interface ILanguageService
{
    AppLanguage CurrentLanguage { get; }
    string CurrentLanguageCode { get; }
    event Action<AppLanguage>? LanguageChanged;
    void SetLanguage(AppLanguage language);
    void ToggleLanguage();
    string GetString(string key, string fallback = "");
}
```
