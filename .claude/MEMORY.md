# Claude & AI Agent Memory Bank Index

Welcome to the **MyDeusTools Memory Bank**. This system preserves architecture, patterns, decisions, operations, milestones, and status across **MyDeusTools.Web**, **MyDeusTools.Agent**, **MyDeusTools.App**, and **MyDeusTools.Tests**.

---

## Memory Bank Architecture & Reading Order

Consult the memory modules in strict order:

```
.claude/
├── MEMORY.md                    # [Step 1] Master Index & Reading Protocol (This file)
├── memory/
│   ├── projectBrief.md          # [Step 2] Foundation: Hybrid Vision, 12 Tools & Quality Baselines
│   ├── systemArchitecture.md    # [Step 3] Structure: Web TerminalUI, Agent HTTP Bridge, WPF MVVM & Storage
│   ├── technicalPatterns.md     # [Step 4] Standards: React/TerminalUI, Agent Loopback, WPF & Testing
│   ├── progress.md              # [Step 5] State: Web/Agent/WPF Feature Matrix & 282-Test Inventory
│   ├── activeContext.md         # [Step 6] Current Focus: Latest commits, tasks & verification status
│   └── history.md               # [Step 7] Chronological Log: Milestones v0.1.0 -> TerminalUI Web + Agent
├── rules/
│   ├── web-terminalui.md        # TerminalUI Design System, TuiCard & Agent Gate Guidelines
│   ├── wpf-standards.md         # WPF-UI, XAML, FluentWindow & MVVM Guidelines
│   ├── windows-api.md           # Win32 P/Invoke (Agent & App), Hotkeys, Power & Registry Guidelines
│   └── localization-standards.md# Bilingual Parity (translations.ts _viSymmetryCheck & Strings.*.xaml)
└── skills/                      # Reusable AI Developer Skills (mirrored with .agents/skills/)
```

---

## How AI Agents Must Use This Memory Bank

1. **Read-First Protocol**:
   - At the beginning of a session: Read `CLAUDE.md`, `activeContext.md`, and `progress.md`.
   - Consult `systemArchitecture.md` before modifying Agent endpoints, Web providers, or WPF DI boundaries.
   - Consult `technicalPatterns.md` and `.claude/rules/` before writing Web or C# code.
   - Use `.agents/skills/` (or mirrored `.claude/skills/`) for execution workflows.

2. **Update Protocol**:
   - When implementing or changing features: Update `progress.md`, `activeContext.md`, and `history.md`.
   - When changing REST endpoints or DI contracts: Update `systemArchitecture.md`, `docs/API_REFERENCE.md`, and `docs/ARCHITECTURE.md`.
   - Always preserve 100% bilingual symmetry (`translations.ts` AND `Strings.*.xaml`).

---

## Ecosystem Overview
- **Web App (`MyDeusTools.Web`)**: React 19 + Vite 8 + TypeScript 7 + Tailwind CSS v4 (TerminalUI design, Dark/Light/CRT themes, Dexie IndexedDB)
- **Local Companion Agent (`MyDeusTools.Agent`)**: `.NET 8` (`net8.0-windows`, `win-x64`) loopback HTTP bridge on `http://127.0.0.1:41950/` (zero external NuGet deps)
- **Desktop App (`MyDeusTools.App`)**: `.NET 8` (`net8.0-windows`, `win-x64`), WPF-UI 3.0, CommunityToolkit.Mvvm
- **Automated Tests (`MyDeusTools.Tests`)**: **282 Tests (100% Pass Rate)** covering `MyDeusTools.App` (274) and `MyDeusTools.Agent` (8)
- **The 12 Core Tools**:
  1. Sticky Notes (`sticky-notes`)
  2. Clipboard Manager (`clip-vault`)
  3. Smart Timer & Shutdown (`smart-timer` &mdash; 2-tab Pomodoro + Agent-gated OS Shutdown)
  4. Text & Dev Tools (`text-devtools`)
  5. QR Code Studio (`qr-studio`)
  6. Color Picker & Eyedropper (`color-picker`)
  7. PDF Toolkit (`pdf-toolkit`)
  8. Image Studio & Optimizer (`img-studio`)
  9. Video Converter (`video-wasm`)
  10. Bulk File & Folder Renamer (`bulk-rename`)
  11. Auto Clicker Pro (`auto-click` &mdash; Web DOM Sandbox + OS Native Agent & `F6`/`F7`)
  12. Window Pinner & PiP (`pip-float` &mdash; Document PiP + OS Windows Manager)
