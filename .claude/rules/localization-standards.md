# Localization Standards - MyDeusTools

This rule enforces **100% bilingual symmetry** (`vi` & `en`) across both **`MyDeusTools.Web`** and **`MyDeusTools.App`**.

---

## 1. Web App (`MyDeusTools.Web`)

- **Dictionary Source**: `MyDeusTools.Web/src/i18n/translations.ts` (`vi` and `en` trees), consumed via `useI18n()` from `LanguageContext.tsx`.
- **Persistence**: Saved in `localStorage['mydeustools_lang']`, falling back to `navigator.language.startsWith('vi')`.
- **Compile-Time Symmetry Rule**:
  - `export type Translations = WidenStrings<typeof translations.en>;`
  - `const _viSymmetryCheck: Translations = translations.vi;`
  - Every key under `translations.en` must exist identically under `translations.vi`. Missing keys fail `tsc` in `npm run web:build`.
- **No Unlocalized UI**: Never hardcode user-visible Vietnamese or English text inside views (`SmartTimerView`, `AutoClickerView`, etc.), including dynamic placeholder templates, alerts, badges, or tab labels.

---

## 2. Native WPF App (`MyDeusTools.App`)

- **Dictionary Files**:
  - **Vietnamese (Default)**: `MyDeusTools.App/Resources/Languages/Strings.vi-VN.xaml`
  - **English**: `MyDeusTools.App/Resources/Languages/Strings.en-US.xaml`
- **100% Dictionary Symmetry Rule**:
  - Every key defined in `Strings.vi-VN.xaml` must exist with the identical key name in `Strings.en-US.xaml` (verified by `LanguageServiceTests`).
- **XAML Usage Rules**:
  - Always bind user-facing strings via `{DynamicResource KeyName}` (never `{StaticResource}` or literal strings).
- **ViewModel Usage Rules**:
  - Inject `ILanguageService` and resolve runtime strings via `GetString(key, fallback)`, subscribing to `LanguageChanged` if dynamic idle status strings need recalculation.
