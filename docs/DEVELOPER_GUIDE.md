# MyDeusTools - Developer & Contribution Guide

This guide provides instructions for developers contributing to or building **MyDeusTools** from source.

---

## 1. Prerequisites

- **Operating System**: Windows 10 (version 1809 or higher) or Windows 11 (x64)
- **SDK**: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.300 or later)
- **IDE / Editor**: Visual Studio 2022 (with *.NET Desktop Development* workload), JetBrains Rider, or VS Code with C# Dev Kit.
- **Git**: For version control.

---

## 2. Getting Started

### Clone the Repository
```bash
git clone https://github.com/KaitoDeus/MyDeusTools.git
cd MyDeusTools
```

### Restore Dependencies
```bash
dotnet restore
```

### Build the Solution
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### Run the Application
```bash
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

---

## 3. Running Automated Tests

MyDeusTools uses **xUnit**, **Moq**, and **Xunit.StaFact** for automated unit and integration tests.

### Run All Tests
```bash
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Suite
```bash
# Test only PDF toolkit
dotnet test --filter "FullyQualifiedName~PdfToolkit"

# Test only Bulk Renamer
dotnet test --filter "FullyQualifiedName~BulkRenamer"

# Test only Language Service
dotnet test --filter "FullyQualifiedName~LanguageService"
```

> [!IMPORTANT]
> All pull requests and commits must maintain a **100% test pass rate** (274/274 tests passing) and compile with **0 warnings and 0 errors** in both Debug and Release configurations.

---

## 4. Single-File Production Packaging

MyDeusTools compiles into a self-contained, single-file Windows x64 executable with embedded native libraries and compression:

```bash
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true
```

The compiled standalone executable will be located at:
`MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe`

---

## 5. Coding Standards & Conventions

### MVVM Architecture
1. **ViewModels**: Inherit from `ObservableObject`. Use `[ObservableProperty]` and `[RelayCommand]` from `CommunityToolkit.Mvvm`.
2. **Views**: Pure XAML with code-behind reserved only for UI-specific events (e.g., Drag & Drop, window position calculations).
3. **Services**: All business logic and external I/O must reside in Services behind an Interface registered in Microsoft DI (`App.xaml.cs`).

### Threading & Dispatcher Rules
1. Background workers and long operations must run asynchronously using `Task.Run` with `CancellationToken` support.
2. UI property updates or window instantiations from background threads must be dispatched via `Application.Current.Dispatcher.Invoke` or `Dispatcher.InvokeAsync`.
3. WPF UI tests must use `[WpfFact]` or `[WpfTheory]` (from `Xunit.StaFact`) to ensure execution on an STA thread.

### Bilingual Localization
1. Every new string or label must be added symmetrically to both:
   - `MyDeusTools.App/Resources/Languages/Strings.vi-VN.xaml`
   - `MyDeusTools.App/Resources/Languages/Strings.en-US.xaml`
2. Never hardcode user-facing strings in XAML; always use `{DynamicResource KeyName}`.
3. In ViewModels, resolve localized strings via `ILanguageService.GetString("Key", "Fallback")`.

### Git Commit Guidelines
- Use the **Conventional Commits** format:
  - `feat(<scope>): description`
  - `fix(<scope>): description`
  - `refactor(<scope>): description`
  - `test(<scope>): description`
  - `docs(<scope>): description`
- Never include AI emojis in commit messages or code comments.
- Include co-author attribution where applicable:
  `Co-authored-by: Claude <81847+claude@users.noreply.github.com>`
