# Localization Standards - MyDeusTools

This rule enforces standards for bilingual localization across all pages, viewmodels, and overlays in MyDeusTools.

---

## 1. Dictionary Files & Locations
- **Vietnamese (Default)**: `MyDeusTools.App/Resources/Languages/Strings.vi-VN.xaml`
- **English**: `MyDeusTools.App/Resources/Languages/Strings.en-US.xaml`

---

## 2. 100% Dictionary Symmetry Rule
- Every key defined in `Strings.vi-VN.xaml` must exist with the **exact same key name** in `Strings.en-US.xaml`.
- Verified by automated tests in `LanguageServiceTests.CriticalKeys_ShouldExistInBothDictionaries`.

---

## 3. XAML Usage Rules
- Always bind user-facing strings using `{DynamicResource KeyName}`.
- Never use `{StaticResource}` for localizable strings, as `{StaticResource}` does not update dynamically when the user switches languages at runtime.
- Never hardcode raw string literals in XAML user interface elements (`TextBlock`, `Button`, `TabItem`, `ToolTip`, etc.).

---

## 4. ViewModel Usage Rules
- Inject `ILanguageService` into ViewModels that format runtime messages.
- Always provide a fallback string when querying `ILanguageService.GetString(key, fallback)`:
  ```csharp
  private string GetLoc(string key, string fallback) =>
      _languageService?.GetString(key, fallback) ?? fallback;
  ```
- Subscribe to `LanguageService.LanguageChanged` if dynamic status strings need to be reformatted when the language switches while a task is idle.
