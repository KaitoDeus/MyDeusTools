# Claude & AI Agent Memory Bank Index

Welcome to the **MyDeusTools Memory Bank**. This system preserves architecture, patterns, decisions, operations, milestones, and status across all AI interactions.

---

## Memory Bank Architecture & Reading Order

For any AI assistant or developer working on MyDeusTools, consult the memory modules in the following strict order:

```
.claude/
├── MEMORY.md                   # [Step 1] Master Index & Reading Protocol (This file)
├── memory/
│   ├── projectBrief.md         # [Step 2] Foundation: Vision, Goals, 12 Core Features & Metrics
│   ├── systemArchitecture.md   # [Step 3] Structure: High/Low Architecture, DI Matrix, Win32 & Storage
│   ├── technicalPatterns.md    # [Step 4] Standards: MVVM, Dispatcher, Threading, XAML & Testing
│   ├── progress.md             # [Step 5] State: Feature Completion Matrix & Test Inventory (274 Tests)
│   ├── activeContext.md        # [Step 6] Current Focus: Active tasks, considerations & recent milestones
│   └── history.md              # [Step 7] Chronological Log: Milestone evolution from inception to v1.0
├── rules/
│   ├── wpf-standards.md        # WPF-UI, XAML, FluentWindow & MVVM Guidelines
│   ├── windows-api.md          # Win32 P/Invoke, User32, GDI & Registry Guidelines
│   └── localization-standards.md# Bilingual Dictionary Symmetry (vi-VN & en-US)
└── skills/                     # Reusable AI Developer Skills (Core, MVVM, Win32, Feature Workflow)
```

---

## How AI Agents Must Use This Memory Bank

1. **Read-First Protocol**:
   - At the beginning of any session or task: Read `CLAUDE.md`, `activeContext.md`, and `progress.md`.
   - Consult `systemArchitecture.md` before adding, modifying, or refactoring services.
   - Consult `technicalPatterns.md` and `rules/` before writing code.
   - Use `.agents/skills/` or `.claude/skills/` for execution runbooks.

2. **Update Protocol**:
   - When a feature is implemented or modified: Update `progress.md`, `activeContext.md`, and `history.md`.
   - When architectural changes or new services are added: Update `systemArchitecture.md`, `projectBrief.md`, and `docs/`.
   - When new conventions or patterns are established: Update `technicalPatterns.md`.
   - Always ensure 100% bilingual dictionary symmetry (`Strings.vi-VN.xaml` and `Strings.en-US.xaml`).

---

## Core System Overview
- **Project**: MyDeusTools (Windows WPF Utility Super-App)
- **Target**: .NET 8 (`net8.0-windows`, `win-x64`)
- **UI Architecture**: WPF-UI 3.0, Mica Backdrop, FluentWindow, MVVM (CommunityToolkit.Mvvm)
- **Automated Tests**: 274 Tests (100% Pass Rate)
- **The 12 Core Native Tools**:
  1. Auto Clicker Pro (`AutoClickService`)
  2. Schedule Shutdown (`SystemService` with Time Normalization)
  3. Sticky Notes (`StickyNoteService`)
  4. Clipboard Manager (`ClipboardService`)
  5. QR Code Studio (`QrCodeService`)
  6. Color Picker & Eyedropper (`ColorPickerService`)
  7. Text & Dev Tools (`TextUtilityService`)
  8. Window Pinner & Transparency (`WindowPinnerService`)
  9. Video Converter & Transcoder (`VideoConverterService`)
  10. Image Studio & Optimizer (`ImageStudioService`)
  11. PDF Toolkit (`PdfToolkitService`)
  12. Bulk File & Folder Renamer (`BulkRenamerService`)
