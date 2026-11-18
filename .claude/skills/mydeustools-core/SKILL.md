---
name: mydeustools-core
description: >-
  Core workflow skill for MyDeusTools. Covers running and building the Web app (MyDeusTools.Web),
  running/publishing the Windows Local Companion Agent (MyDeusTools.Agent), building/testing the
  .NET solution (282 tests), and publishing single-file executables.
---

# MyDeusTools Core Development Skill

Defines the core development, build, testing, and packaging workflows across **`MyDeusTools.Web`**, **`MyDeusTools.Agent`**, and **`MyDeusTools.App`**.

---

## 1. Environment & Project Roots

- **Solution File**: `MyDeusTools.sln` (`MyDeusTools.App`, `MyDeusTools.Agent`, `MyDeusTools.Tests`, `MyDeusTools.Website`)
- **Web App**: `MyDeusTools.Web/` (Vite 8 + React 19 + TypeScript 7 + Tailwind CSS v4)
- **Local Companion Agent**: `MyDeusTools.Agent/MyDeusTools.Agent.csproj` (`net8.0-windows`, `win-x64`)
- **WPF Desktop App**: `MyDeusTools.App/MyDeusTools.App.csproj` (`net8.0-windows`, `win-x64`)
- **Automated Tests**: `MyDeusTools.Tests/MyDeusTools.Tests.csproj` (282 tests)
- **Prerequisites**: Node.js `v20+` / `npm` + `.NET 8.0 SDK` (`8.0.300+`)
- **Shell**: PowerShell (Windows)

---

## 2. Standard Commands

### Web & Local Companion Agent (`package.json`)
```powershell
# Install web dependencies (first time)
npm --prefix MyDeusTools.Web install

# Run Web App dev server (http://localhost:3000)
npm run web:dev

# TypeScript check (including _viSymmetryCheck) + Vite production build -> MyDeusTools.Web/dist
npm run web:build

# Preview production Web bundle locally (default http://localhost:4173)
npm run web:preview
# Or directly in PowerShell when passing flags:
cd MyDeusTools.Web; npx vite preview --port 4173

# Run Windows Local Companion Agent (http://127.0.0.1:41950/)
npm run agent:dev

# Publish standalone self-contained Agent EXE -> publish/agent/MyDeusTools.Agent.exe
npm run agent:publish
```

> Note: `agent:dev` and `agent:publish` also work inside `MyDeusTools.Web/` directly.

### .NET Solution & WPF App
```powershell
dotnet restore
dotnet build
dotnet build -c Release

# Launch Native WPF Desktop App
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

### Automated Tests (282 Tests)
```powershell
# Run full suite (WPF App + Agent: all 282 tests must pass)
dotnet test

# Run Agent Engine tests
dotnet test --filter "FullyQualifiedName~OsAutoClickEngine"

# Filter WPF suites
dotnet test --filter "FullyQualifiedName~PdfToolkit"
dotnet test --filter "FullyQualifiedName~BulkRenamer"
dotnet test --filter "FullyQualifiedName~Shutdown"
dotnet test --filter "FullyQualifiedName~LanguageService"
```

### Publish WPF Standalone Single-File EXE
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

Before committing changes:
1. `npm run web:build` exits `0` cleanly (validating TypeScript + `_viSymmetryCheck` + Vite bundle).
2. `dotnet test` returns **0 failed** (`282/282 passed`, 100% pass rate).
3. `dotnet build -c Release` compiles cleanly with **0 Error(s)**.
4. Commit uses Conventional Commits (`feat(...)`, `fix(...)`, `docs(...)`, `test(...)`) with co-author attribution:
   `Co-authored-by: Claude <81847+claude@users.noreply.github.com>`
