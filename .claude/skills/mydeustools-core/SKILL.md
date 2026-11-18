---
name: mydeustools-core
description: >-
  Core workflow skill for MyDeusTools. Covers building the solution, running the application,
  executing the automated test suite, publishing single-file binaries, and navigating project structure.
---

# MyDeusTools Core Development Skill

This skill defines the core development workflows, build procedures, test executions, and packaging commands for the **MyDeusTools** repository.

---

## 1. Environment & Setup

- **Solution File**: `MyDeusTools.sln`
- **Main App**: `MyDeusTools.App/MyDeusTools.App.csproj` (Target: `net8.0-windows`, `win-x64`)
- **Test Project**: `MyDeusTools.Tests/MyDeusTools.Tests.csproj`
- **SDK**: .NET 8.0 SDK (8.0.300+)
- **Shell**: PowerShell (Windows)

---

## 2. Standard Commands

### Restore Dependencies
```powershell
dotnet restore
```

### Build Solution
```powershell
# Debug configuration
dotnet build

# Release configuration (must have 0 warnings and 0 errors)
dotnet build -c Release
```

### Run Application
```powershell
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

### Run Automated Tests
```powershell
# Run all tests (all 274 tests must pass)
dotnet test

# Run with logger
dotnet test --logger "console;verbosity=normal"

# Filter by test class
dotnet test --filter "FullyQualifiedName~PdfToolkit"
dotnet test --filter "FullyQualifiedName~BulkRenamer"
dotnet test --filter "FullyQualifiedName~Shutdown"
```

### Publish Single-File Executable
```powershell
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:PublishReadyToRun=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true
```
Binary output: `MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe`

---

## 3. Pre-Commit Checklist

Before pushing any commit to Git:
1. `dotnet test` returns **0 failed** (100% pass rate).
2. `dotnet build -c Release` returns **0 Warning(s), 0 Error(s)**.
3. Git commit message follows Conventional Commits: `feat(...)`, `fix(...)`, `docs(...)`, `test(...)`.
4. Co-author attribution is included:
   `Co-authored-by: Claude <81847+claude@users.noreply.github.com>`
