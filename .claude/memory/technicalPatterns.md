# Technical Patterns & Conventions - MyDeusTools

This document records the design patterns, coding rules, threading conventions, XAML standards, and testing guidelines enforced across MyDeusTools.

---

## 1. MVVM Implementation Patterns

1. **Toolkit Usage**: All ViewModels inherit from `ObservableObject` in `CommunityToolkit.Mvvm`.
2. **Field-Backed Properties**:
   ```csharp
   [ObservableProperty]
   private string _statusMessage = string.Empty;
   ```
3. **Change Notification Callbacks**:
   ```csharp
   partial void OnFindTextChanged(string value) => RefreshPreview();
   ```
4. **Command Authoring**:
   ```csharp
   [RelayCommand]
   private async Task ExecuteAsync()
   {
       // Background execution with CancellationToken
   }
   ```
5. **No UI Coupling**: ViewModels never reference WPF elements (`FrameworkElement`, `Button`, `Page`, etc.). Use bindings, commands, and `IProgress<T>`.

---

## 2. Threading & Dispatcher Rules

1. **Heavy Work Offloading**:
   - Heavy tasks (PDF manipulation, batch file renaming, image encoding, video conversion, file hashing) must execute on background threads via `Task.Run`.
2. **CancellationToken Support**:
   - Long-running async methods must accept `CancellationToken cancellationToken = default`.
   - ViewModel manages `CancellationTokenSource`, exposing a `CancelCommand`.
3. **UI Marshaling**:
   - Direct changes to `ObservableCollection` bound to the UI must occur on the UI thread or use `IProgress<T>`.
   - When background threads need UI invocation: `Application.Current.Dispatcher.Invoke(...)` or `Dispatcher.InvokeAsync(...)`.
4. **STA Threading for Tests**:
   - Any unit test touching WPF UI components, dialogs, or dependency objects must use `[WpfFact]` or `[WpfTheory]` from `Xunit.StaFact`.

---

## 3. Bilingual ResourceDictionary Localization Pattern

1. **Dynamic Binding**:
   - Always use `{DynamicResource KeyName}` in XAML.
   - Do not use `{StaticResource}` for localizable text.
2. **Symmetrical Dictionaries**:
   - `Strings.vi-VN.xaml` and `Strings.en-US.xaml` must maintain identical key names.
   - Verified by `LanguageServiceTests.CriticalKeys_ShouldExistInBothDictionaries`.
3. **Runtime Language Switch**:
   - `LanguageService.SetLanguage(...)` swaps the language dictionary in `Application.Current.Resources.MergedDictionaries`.
   - Dispatches `LanguageChanged` event to notify ViewModels if dynamic format strings need recalculation.

---

## 4. File I/O & Concurrency Patterns

1. **Thread-Safe JSON Writing**:
   - Use `SemaphoreSlim` or asynchronous file locks when persisting lists (`Data/notes.json`, `Data/clipboard.json`, `Data/colors.json`).
   - Create the directory (`Directory.CreateDirectory`) before attempting file write.
2. **Case-Only Renaming on Windows**:
   - Windows NTFS is case-preserving but case-insensitive.
   - When renaming a file from `sample.txt` to `SAMPLE.TXT`, rename through a temporary file (`sample.txt.tmp_guid`) before the final rename to prevent collision errors.
3. **Directory Ordering for Renaming**:
   - When renaming folders in batch, sort by path depth descending (`OrderByDescending(p => p.Length)`) so child directories are renamed before parent directories.

---

## 5. Testing & Code Quality Standards

1. **Isolation**: Tests must never write into real user directories. Use `Path.GetTempPath()` and clean up in `Dispose()`.
2. **Moq Verification**: Verify mock service invocations explicitly with expected arguments (`Times.Once`, `Times.Never`).
3. **Coverage Baseline**: 100% test pass rate (all 274 tests must pass).
4. **Release Quality**: Zero compiler warnings, zero errors under `dotnet build -c Release`.
