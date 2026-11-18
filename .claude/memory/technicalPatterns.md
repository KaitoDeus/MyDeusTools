# Technical Patterns & Conventions - MyDeusTools

This document captures core architectural patterns across **Web (TerminalUI + TypeScript)**, **Local Companion Agent (.NET 8 + Win32)**, and **WPF Desktop (MVVM + WPF-UI)**.

---

## 1. Web & TerminalUI Patterns (`MyDeusTools.Web`)

1. **TuiCard Slot Separation**:
   - `TuiCard` wraps its children inside `<div className="w-full flex-1 min-w-0 ${bodyClassName}">`.
   - Pass card outer geometry (e.g. `lg:col-span-6 p-6`) in `className` and inner child layout (e.g. `space-y-5`, `flex`) in `bodyClassName` so the dashed header never misaligns or overlaps body controls.
2. **Strict TerminalUI CSS Guarantees (`src/index.css`)**:
   - Monospace font only (`JetBrains Mono`), `border-radius: 0px !important`, `box-shadow: none !important`, and `transition-timing-function: steps(1, end) !important`.
   - Theme switching uses CSS custom properties on `:root` (Light TTY Paper `#F4F4EE`) vs `.dark` (Dark `#0D0D0D`). Never hardcode dark-only hex colors inside views.
3. **Compile-Time Translation Symmetry (`translations.ts`)**:
   - `export type Translations = WidenStrings<typeof translations.en>;`
   - `const _viSymmetryCheck: Translations = translations.vi;`
   - Any missing or mismatched key between `en` and `vi` immediately fails `tsc` during `npm run web:build`.
4. **Agent-Online Feature Gating**:
   - Features affecting host Windows OS power state (such as `02:SCHEDULE_SHUTDOWN` inside `SmartTimerView`) must remain **isolated from browser-only tabs** and enforce `disabled={!isOnline}` on every interactive control (`Shutdown`/`Restart`/`Hibernate` radios, inputs, presets, Start/Reset/Cancel).
   - Automatic Human Time Normalization (e.g., `70m -> 1h 10m`) executes cleanly on input blur, `Enter` key, and Start submission.

---

## 2. Local Companion Agent Patterns (`MyDeusTools.Agent`)

1. **Loopback Security & Browser Compatibility**:
   - Always bind strictly to `http://127.0.0.1:41950/` (never `0.0.0.0` or `*`).
   - Always echo `Access-Control-Allow-Private-Network: true` when `Access-Control-Request-Private-Network` is present so Chrome/Edge allow loopback calls from non-localhost/https pages.
2. **Mockable Native Interop (`IMouseInterop`)**:
   - Abstract Win32 P/Invoke (`mouse_event`, `GetCursorPos`, `SetCursorPos`, `GetAsyncKeyState`) behind `IMouseInterop`.
   - Keep `OsAutoClickEngine` thread-safe via `lock (_sync)`, clamping intervals `>= MinIntervalMs` (10ms) and exposing `internal bool ExecuteSingleTick()` for deterministic xUnit verification without real mouse movement.
3. **Edge-Triggered Global Hotkeys**:
   - `GlobalHotkeyService` tracks previous key state (`_f6WasDown`, `_f7WasDown`) across 40ms `PeriodicTimer` ticks so holding a key never triggers repeated toggles/captures.

---

## 3. WPF MVVM Patterns (`MyDeusTools.App`)

1. **CommunityToolkit.Mvvm**: Inherit `ObservableObject`; use `[ObservableProperty]` and `[RelayCommand]` with zero UI element references in ViewModels.
2. **Async & Dispatcher**: Run heavy work via `Task.Run` with `CancellationToken`; marshal UI updates with `Application.Current.Dispatcher.Invoke` / `IProgress<T>`; use `[WpfFact]` / `[WpfTheory]` in STA UI tests.
3. **Dynamic Bilingual Resources**: Bind all XAML text via `{DynamicResource KeyName}` with 100% key parity across `Strings.vi-VN.xaml` and `Strings.en-US.xaml` (verified by `LanguageServiceTests`).
4. **NTFS Rename Safety**: Handle case-only NTFS renames through a temporary GUID path, and sort directories descending by path length so children rename before parents.

---

## 4. Verification & Git Standards

- **Testing Baseline**: Maintain **282/282 passing .NET tests** (`dotnet test`) and clean TypeScript + Vite build (`npm run web:build`).
- **Commit Format**: Conventional Commits (`feat(<scope>): ...`, `fix(<scope>): ...`, `docs(<scope>): ...`) with `Co-authored-by: Claude <81847+claude@users.noreply.github.com>` and zero emojis.
