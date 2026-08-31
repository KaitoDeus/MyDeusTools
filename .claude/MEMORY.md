# Claude Memory Bank Index

Welcome to the **MyDeusTools Memory Bank**. This system preserves context, architectural patterns, decisions, operations, and status across AI interactions.

## Memory Bank Structure

```
.claude/
├── MEMORY.md                   # This master index file
├── memory/
│   ├── activeContext.md        # Current task focus, active goals, immediate steps
│   ├── projectBrief.md         # Foundation goals, project scope, core features
│   ├── systemArchitecture.md   # Detailed technical architecture, DI, layers, APIs
│   ├── technicalPatterns.md    # Design patterns, MVVM rules, threading, XAML rules
│   ├── progress.md             # Status of features, test coverage, roadmap
│   └── history.md              # Historical log of operations, changes & milestones
└── rules/
    ├── wpf-standards.md        # Guidelines for WPF, WPF-UI, XAML & MVVM
    └── windows-api.md          # Guidelines for Win32 API, P/Invoke, Registry & OS
```

## How AI Agents Must Use This Memory Bank

1. **Read-First Protocol**:
   - At the beginning of any task or session, read `CLAUDE.md`, `activeContext.md`, and `progress.md`.
   - Consult `systemArchitecture.md` before making design decisions or modifying service boundaries.
   - Consult `technicalPatterns.md` and `rules/` before authoring code.

2. **Update Protocol**:
   - When a feature is implemented or modified: update `progress.md` and `activeContext.md`.
   - When architectural changes or new services are added: update `systemArchitecture.md` and `projectBrief.md`.
   - When new conventions or patterns are established: update `technicalPatterns.md`.
   - Log all meaningful operations, fixes, and refactorings in `history.md`.

## Core System Overview
- **Project**: MyDeusTools (Windows WPF Utility Super-App)
- **Target**: .NET 8 (net8.0-windows, win-x64)
- **UI Architecture**: WPF-UI 3.0, Mica Backdrop, FluentWindow, MVVM (CommunityToolkit.Mvvm)
- **Core Modules**:
  1. `AutoClickService` & `AutoClickViewModel` (Mouse Automation, Waypoint Replay, Hotkey)
  2. `SystemService` & `ShutdownViewModel` (Shutdown / Restart / Hibernate scheduler)
  3. `StickyNoteService` & `StickyNoteViewModel` (Floating persistent desktop notes)
  4. `AutoStartService` (Windows Run registry toggle)
  5. `App` & `MainWindow` (Tray icon, Dark/Light theme toggle, Single instance / background run)
