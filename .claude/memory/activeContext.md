# Active Context - MyDeusTools

## Current Focus & Status
- **Current State**: Full project documentation, Memory Bank, rules, and AI skills are being synchronized after completing the **Pure Web Transformation (`MyDeusTools.Web`)**, **TerminalUI Redesign**, **Windows Local Companion Agent (`MyDeusTools.Agent`)**, and **2-Tab Agent-Gated SmartTimer (`SmartTimerView`)**.
- **Latest Verified Baseline**:
  - `.NET` Solution (`MyDeusTools.sln`): **282/282 tests passing** (`274` WPF app tests + `8` `OsAutoClickEngineTests`).
  - Web Application (`MyDeusTools.Web`): `npm run web:build` (`tsc && vite build`) passes cleanly (242 modules) with 100% symmetric `vi`/`en` keys enforced by `_viSymmetryCheck`.
- **Recent Commits & Milestones**:
  1. `c5f34ee` & `9d74be3`: Transformed MyDeusTools into pure Web/PWA app (`React 19`, `Vite 8`, `TypeScript`, `Tailwind v4`, `Dexie`) and wired Light/Dark theme switching + `VideoConverterView`.
  2. `51146ae` & `461774c`: Rebuilt entire Web UI using strict **TerminalUI** design tokens (`#0D0D0D`/`#F4F4EE`, `JetBrains Mono`, `0px` corners, CRT scanlines), fixed `TuiCard` header alignment via `bodyClassName`, and completed 100% VI/EN translations.
  3. `e4f15ea` & `789581e`: Created `.NET 8` `MyDeusTools.Agent` loopback bridge (`http://127.0.0.1:41950/`) for OS-wide Auto-Clicking (`F6`/`F7` hotkeys), System Power (`/s`, `/r`, `/h`, `/a`), and Window Pinner/Opacity; added `agent:dev` & `agent:publish` npm scripts to both root and `MyDeusTools.Web` `package.json`.
  4. `bc96ba9` & `56dad09`: Split Tool `03` (`SmartTimerView`) into two independent tabs (`01:POMODORO_TIMER` vs `02:SCHEDULE_SHUTDOWN`) where OS Shutdown controls remain strictly `disabled={!isOnline}` when the Companion Agent is offline.

## Active Technical Notes
- **Agent Port & Protocol**: Loopback only `http://127.0.0.1:41950/` using `System.Net.HttpListener` (`net8.0-windows`, no ASP.NET Core dependency) with `Access-Control-Allow-Private-Network: true`.
- **Web Persistence**: `MyDeusToolsWebDB` in IndexedDB (`notes`, `snippets`, `colors`) + `localStorage` keys `mydeustools_lang`, `mydeustools_theme`, `mydeustools_crt`.
- **WPF Persistence**: `AppDomain.CurrentDomain.BaseDirectory\Data\` (`notes.json`, `clipboard.json`, `colors.json`, `settings.json`).
