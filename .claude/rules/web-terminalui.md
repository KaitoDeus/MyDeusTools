# Web TerminalUI Standards (`MyDeusTools.Web`)

Rules governing UI components, themes, layout, and Agent gating in `MyDeusTools.Web/`.

---

## 1. Non-Negotiable TerminalUI Aesthetic (`src/index.css`)
- **Typography**: `JetBrains Mono` monospace exclusively across every element.
- **Sharp Geometry**: `border-radius: 0px !important` and `box-shadow: none !important` everywhere. Zero rounded pills, cards, or gradients.
- **Motion**: Stepped timing `steps(1, end)` (`tui-blink`, hover states). No smooth easing springs.
- **No Decorative Icons or Emojis**: Do not use Unicode emojis or SVG icon packs inside UI buttons/headers; use crisp ASCII prefixes (`>`, `$`, `[!]`, `[x]`, `( )`, `(*)`, `[OK]`, `[ERR]`, `[WARN]`, `[INFO]`, `// label`).
- **Dual Theme Semantic Tokens**: Always style using `--tui-*` variables (`--tui-bg`, `--tui-surface`, `--tui-surface-elevated`, `--tui-primary`, `--tui-secondary`, `--tui-tertiary`, `--tui-error`, `--tui-text`, `--tui-text-muted`, `--tui-text-dim`, `--tui-border-*`). Never hardcode `#0D0D0D` or `#00FF41` directly in feature views so TTY Paper Light Mode (`:root`) remains legible.

---

## 2. Shared Component Primitives (`src/components/terminal/TerminalUI.tsx`)
- Always build feature views from: `TuiPageHeader`, `TuiCard`, `TuiButton`, `TuiInput`, `TuiTextarea`, `TuiSelect`, `TuiChip`, `TuiAlert`, and `TuiCheckbox`.
- **`TuiCard` Header/Body Alignment Rule**:
  - `TuiCard` renders a dashed header (`title` + `rightMeta`) above a `<div className="w-full flex-1 min-w-0 ${bodyClassName}">`.
  - Pass grid/spacing classes (`lg:col-span-6 p-6`) in `className`, and pass inner layout classes (`space-y-5`, `flex`, `flex-col`) in `bodyClassName`. Never put `space-y-*` on `className` directly, or margin-top shifts the header down relative to sibling cards.

---

## 3. Agent Feature Separation & Safety Gating
- Browser-safe tools must always work standalone offline without requiring `MyDeusTools.Agent`.
- Destructive or OS-level capabilities (`02:SCHEDULE_SHUTDOWN` in `SmartTimerView`) must be isolated in their own tab and enforce `disabled={!isOnline}` on every control, displaying `[LOCKED]` status and a `[WARN]` notice with `npm run agent:dev` when offline.
- Do not mix browser Pomodoro state with OS Shutdown state; keep independent timers and configs per tab.
