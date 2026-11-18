# Active Context - MyDeusTools

## Current Focus & Status
- **Current State**: Project builds cleanly with 0 warnings/errors in both Debug and Release configurations, and passes 100% of unit & integration tests (**274/274 passed**).
- **Recent Milestones**:
  1. Implemented **Bulk File & Folder Renamer** (Regex, case conversion, sequential numbering, date insertion, live preview, conflict detection, 1-click undo).
  2. Implemented **PDF Toolkit** (PDFsharp 6.1.1 offline merge, split by page range/chunk, images to PDF, encryption & metadata).
  3. Implemented **Automatic Time Normalization** in Schedule Shutdown (e.g., 70 minutes automatically normalizes to 1h 10m).
  4. Standardized comprehensive documentation (`docs/`), AI developer skills (`.agents/skills/`), and the Memory Bank in strict sequence.
  5. Implemented **Pure Web App Transformation (`MyDeusTools.Web/`)**: Complete client-side rewrite in Vite + React 19 + TypeScript + Tailwind CSS with code-splitting, WebAssembly (FFmpeg.wasm, pdf-lib), native Web APIs (EyeDropper, File System Access, Screen Wake Lock, Document PiP), and bilingual i18n support.

## Active Session Goals
1. Maintain and extend Super-App features according to user priorities.
2. Ensure 100% test coverage for newly introduced business services.
3. Keep Memory Bank, Skills, and Documentation synchronized with all code changes.

## Active Considerations & Technical Notes
- **Testing Baseline**: 274 tests with 100% pass rate. Every PR/change must maintain this baseline.
- **Bilingual Symmetry**: When adding keys to `Strings.vi-VN.xaml`, always mirror them with identical keys in `Strings.en-US.xaml`.
- **WPF Single File Publishing**: Configured in `MyDeusTools.App.csproj` (`PublishSingleFile=true`, `SelfContained=true`, `RuntimeIdentifier=win-x64`).
- **Resource Bundling**: `avatar.ico` is compiled as `<Resource Include="Resources\avatar.ico" />` and loaded with `pack://application:,,,/Resources/avatar.ico`.
- **Persistence Storage**: Managed files are saved in `AppDomain.CurrentDomain.BaseDirectory\Data\` (`notes.json`, `clipboard.json`, `colors.json`, `settings.json`).
- **Multi-Monitor Coordinate Safety**: Overlays use `SystemParameters.VirtualScreen*` to guarantee correct scaling across multi-monitor setups.
