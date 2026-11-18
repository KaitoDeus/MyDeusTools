---
name: feature-expansion
description: >-
  Standardized end-to-end workflow for adding or extending utilities in MyDeusTools
  across the TerminalUI Web App (React/TS + translations + Optional Agent APIs) and
  the Native WPF App (Service, ViewModel, XAML, DI, strings, unit tests).
---

# Feature Expansion Workflow for MyDeusTools

Choose the workflow matching your target layer (**Web + Agent** or **Native WPF Desktop**).

---

## Track A: Extending `MyDeusTools.Web` & `MyDeusTools.Agent`

### Step 1: Extend Agent Endpoints (if Windows OS privileges are needed)
- Add a clean service in `MyDeusTools.Agent/Services/` (e.g., `Os*Service.cs`), abstracting P/Invoke behind an interface (`IMouseInterop`) for unit testing.
- Register new loopback HTTP routes inside `LocalHttpBridgeServer.cs` (`http://127.0.0.1:41950/api/...`) using camelCase JSON records.
- Add matching TypeScript methods and interfaces to `MyDeusTools.Web/src/services/agentClient.ts`.

### Step 2: Author TerminalUI Feature View
- Create `MyDeusTools.Web/src/features/<feature-slug>/<Feature>View.tsx`.
- Compose strictly with TerminalUI primitives (`TuiPageHeader`, `TuiCard`, `TuiButton`, `TuiInput`, `TuiTextarea`, `TuiSelect`, `TuiChip`, `TuiAlert`, `TuiCheckbox`) from `src/components/terminal/TerminalUI.tsx`.
- Put card body layout classes inside `TuiCard`'s `bodyClassName` prop (never on outer `className`) so dashed card headers stay aligned.
- Isolate any host OS actions (like Windows Shutdown) in a separate tab with `disabled={!isOnline}` using `useOsAgent()` and clear `[LOCKED]` / `[WARN]` indicators.
- Keep `border-radius: 0px`, zero emojis/decorative icons, and `--tui-*` theme CSS variables.

### Step 3: Wire Tool Navigation & Lazy Loading
- Register metadata (`id`, `index`, `cmd`, `asciiTag`, `nameKey`, `descKey`, `category`, `accent`) in `MyDeusTools.Web/src/data/toolsData.ts`.
- Add `React.lazy(...)` import and `switch (activeToolId)` route in `MyDeusTools.Web/src/App.tsx`.
- Persist client data (if needed) via IndexedDB in `MyDeusTools.Web/src/services/db.ts` (Dexie).

### Step 4: Symmetrical Bilingual Translations (`vi` & `en`)
- Add identical key structures under both `translations.vi` and `translations.en` in `MyDeusTools.Web/src/i18n/translations.ts`.
- Run `npm run web:build` (`tsc && vite build`) to verify `_viSymmetryCheck` passes with zero missing keys.

---

## Track B: Extending `MyDeusTools.App` (Native WPF Desktop)

### Step 1: Define Service Interface & Implementation
- Create `MyDeusTools.App/Services/Impl/I<FeatureName>Service.cs` and `<FeatureName>Service.cs` with `CancellationToken` and `IProgress<T>`.

### Step 2: Author ViewModel & XAML Page
- Create `MyDeusTools.App/ViewModels/<FeatureName>ViewModel.cs` inheriting `ObservableObject` (`[ObservableProperty]`, `[RelayCommand]`).
- Create `MyDeusTools.App/Views/Pages/<FeatureName>Page.xaml` using `Wpf.Ui` controls and `{DynamicResource ...}` localization keys.

### Step 3: Register DI, Navigation & Symmetrical Dictionaries
- Register Singleton Service + Transient ViewModel/Page in `App.xaml.cs` and `<ui:NavigationViewItem>` in `MainWindow.xaml`.
- Add identical keys to both `Strings.vi-VN.xaml` and `Strings.en-US.xaml`.

### Step 4: Automated Tests & Verification
- Add xUnit/Moq (`Os*Tests.cs` or `<FeatureName>Tests.cs`) in `MyDeusTools.Tests/`.
- Confirm `dotnet test` passes 100% (`282+` tests), `dotnet build -c Release` passes cleanly, and `npm run web:build` exits `0`.
