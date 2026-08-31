# Active Context - MyDeusTools

## Current Focus & Status
- **Current State**: Project is fully initialized, builds cleanly with 0 warnings/errors, and passes 100% of unit & integration tests (20/20 passed).
- **Recent Milestone**: Full codebase audit, architectural review, creation of the Claude AI configuration system, and establishment of persistent project memory.

## Active Session Goals
1. Establish standard Claude AI instructions (`CLAUDE.md`) and Memory Bank (`.claude/`).
2. Document all features, architecture, Win32 P/Invoke APIs, WPF-UI bindings, and test suites.
3. Track all ongoing and future tasks in `progress.md` and historical operations in `history.md`.

## Active Considerations & Technical Notes
- **WPF Single File Publishing**: Configured in `MyDeusTools.App.csproj` (`PublishSingleFile=true`, `SelfContained=true`, `RuntimeIdentifier=win-x64`).
- **Resource Bundling**: `avatar.ico` is compiled as `<Resource Include="Resources\avatar.ico" />` and loaded with `pack://application:,,,/Resources/avatar.ico`.
- **Note Persistence**: Files are currently saved in `AppDomain.CurrentDomain.BaseDirectory\Data\notes.json`. Consider roaming AppData if user migration across updates is required in the future.
- **Recording Overlay**: Supports multi-monitor configurations using `SystemParameters.VirtualScreen*`.
