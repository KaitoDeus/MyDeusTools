# MyDeusTools - Developer & Contribution Guide

This guide covers prerequisites, development workflows, automated testing, packaging, and coding standards for **`MyDeusTools.Web`**, **`MyDeusTools.Agent`**, and **`MyDeusTools.App`**.

---

## 1. Prerequisites

- **Web App (`MyDeusTools.Web`)**:
  - [Node.js](https://nodejs.org/) `v20+` and `npm`
- **Local Companion Agent (`MyDeusTools.Agent`) & WPF Desktop (`MyDeusTools.App`)**:
  - Windows 10 (1809+) or Windows 11 (x64)
  - [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`8.0.300+`)
- **Tooling**: VS Code / Visual Studio 2022 / JetBrains Rider + Git

---

## 2. Getting Started & Development Commands

```bash
git clone https://github.com/KaitoDeus/MyDeusTools.git
cd MyDeusTools
```

### Root npm Scripts (`package.json`)
```bash
# Install web dependencies (first time)
npm --prefix MyDeusTools.Web install

# Run Web App dev server (http://localhost:3000)
npm run web:dev

# Run TypeScript check + production Vite build (MyDeusTools.Web/dist)
npm run web:build

# Preview production Web bundle locally (default http://localhost:4173)
npm run web:preview

# Run Windows Local Companion Agent (http://127.0.0.1:41950/)
npm run agent:dev

# Publish standalone self-contained Agent binary to publish/agent/
npm run agent:publish
```

> Both `agent:dev` and `agent:publish` are also mirrored inside `MyDeusTools.Web/package.json` using relative paths (`../MyDeusTools.Agent`), so running `npm run agent:dev` works from either the root directory or `MyDeusTools.Web/`.

### .NET Solution Commands (`MyDeusTools.sln`)
```powershell
dotnet restore
dotnet build
dotnet build -c Release

# Launch Native WPF Desktop App
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

---

## 3. Running Automated Tests & Verification

MyDeusTools validates both **`.NET` (`MyDeusTools.App` + `MyDeusTools.Agent`)** and **`TypeScript + Vite` (`MyDeusTools.Web`)**.

### .NET Test Suite (282 Tests)
```powershell
# Run all tests (WPF + Agent: 282 tests)
dotnet test

# Run Agent OS Engine tests only
dotnet test --filter "FullyQualifiedName~OsAutoClickEngine"

# Filter WPF suites
dotnet test --filter "FullyQualifiedName~PdfToolkit"
dotnet test --filter "FullyQualifiedName~BulkRenamer"
dotnet test --filter "FullyQualifiedName~LanguageService"
```

### Web Type-Check & Build Check
```bash
npm run web:build
```
This runs `tsc` (including `_viSymmetryCheck` proving `vi` mirrors every `en` key in `translations.ts`) followed by `vite build`.

> [!IMPORTANT]
> Pull requests and commits must keep **all 282 .NET tests passing (100%)**, pass `dotnet build -c Release` with zero errors, and pass `npm run web:build` cleanly.

---

## 4. Packaging & Standalone Publishing

### Local Companion Agent (`MyDeusTools.Agent`)
```bash
npm run agent:publish
```
Outputs standalone single-file `publish/agent/MyDeusTools.Agent.exe`.

### Native WPF App (`MyDeusTools.App`)
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
Outputs `MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe`.

---

## 5. Coding Standards & Conventions

### Web / TerminalUI (`MyDeusTools.Web`)
1. **Strict TerminalUI Geometry**: Zero rounded corners (`0px`), zero drop shadows, zero decorative icons or emojis, monospace `JetBrains Mono` everywhere, stepped transitions.
2. **Semantic Tokens & Primitives**: Always use CSS variables (`--tui-bg`, `--tui-surface`, `--tui-primary`, etc.) and shared primitives from `src/components/terminal/TerminalUI.tsx` (`TuiCard`, `TuiButton`, `TuiInput`, `TuiAlert`, `TuiChip`). When adding wrapper classes on `TuiCard`, put inner layout classes (`space-y-*`, `flex`) in `bodyClassName` instead of `className`.
3. **Agent Safety Gates**: OS-only actions such as Windows Scheduled Shutdown must remain strictly `disabled={!isOnline}` when `useOsAgent().isOnline` is `false`.
4. **Symmetric Translations**: Add every new Web UI key identically under both `vi` and `en` in `src/i18n/translations.ts`.

### C# / WPF (`MyDeusTools.App` & `MyDeusTools.Agent`)
1. **MVVM & DI**: Inherit ViewModels from `ObservableObject`; encapsulate OS/Win32 logic behind mockable interfaces (`I*Service`, `IMouseInterop`).
2. **Threading**: Offload heavy I/O with `Task.Run` / `CancellationToken`; marshal UI updates via `Dispatcher`; annotate WPF unit tests with `[WpfFact]` / `[WpfTheory]`.
3. **Symmetric WPF Dictionaries**: Keep `Strings.vi-VN.xaml` and `Strings.en-US.xaml` in 100% key parity using `{DynamicResource ...}`.

### Git Commit Guidelines
- Follow **Conventional Commits**: `feat(<scope>): ...`, `fix(<scope>): ...`, `refactor(<scope>): ...`, `test(<scope>): ...`, `docs(<scope>): ...`.
- Avoid emojis in commits or code comments.
- Include co-author attribution where appropriate:
  `Co-authored-by: Claude <81847+claude@users.noreply.github.com>`
