---
name: wpf-mvvm-guidelines
description: >-
  WPF, WPF-UI 3.0, and CommunityToolkit.Mvvm development guidelines for MyDeusTools.
  Covers FluentWindow, Mica, NavigationView, DataTemplates, XAML bindings, and dynamic runtime localization.
---

# WPF & MVVM Guidelines for MyDeusTools

This skill enforces architectural standards and best practices for authoring UI components, ViewModels, and XAML in MyDeusTools.

---

## 1. UI Stack Standards

- **Controls**: Use `Wpf.Ui.Controls` (e.g., `ui:FluentWindow`, `ui:TitleBar`, `ui:NavigationView`, `ui:Button`, `ui:Card`, `ui:TextBox`, `ui:SymbolIcon`).
- **Backdrop**: Windows 11 Mica backdrop (`WindowBackdropType="Mica"`).
- **Brushes**: Always use WPF-UI theme brushes for automatic Light/Dark mode compatibility:
  - `{DynamicResource TextFillColorPrimaryBrush}`
  - `{DynamicResource TextFillColorSecondaryBrush}`
  - `{DynamicResource CardBackgroundFillColorDefaultBrush}`
  - `{DynamicResource ControlFillColorDefaultBrush}`
  - `{DynamicResource AccentTextFillColorPrimaryBrush}`

---

## 2. ViewModel Conventions (`CommunityToolkit.Mvvm`)

1. **Inheritance**: All ViewModels inherit from `ObservableObject`.
2. **Properties**: Use `[ObservableProperty]` for automatic change notification:
   ```csharp
   [ObservableProperty]
   private string _statusMessage = string.Empty;
   ```
3. **Property Change Callbacks**: Use `partial void On<Property>Changed(...)` for reactive updates:
   ```csharp
   partial void OnFindTextChanged(string value) => RefreshPreview();
   ```
4. **Commands**: Use `[RelayCommand]` on methods:
   ```csharp
   [RelayCommand]
   private async Task ApplyChangesAsync() { ... }
   ```
5. **Thread Safety**: Never touch UI elements directly from a ViewModel. Use `IProgress<T>` or `Dispatcher` when marshaling background results to UI properties.

---

## 3. Bilingual DynamicResource Binding

Never hardcode text in XAML or ViewModels.
- **XAML**:
  ```xaml
  <!-- Correct -->
  <TextBlock Text="{DynamicResource Renamer_Title}" />
  <ui:Button Content="{DynamicResource App_Save}" />

  <!-- Incorrect -->
  <TextBlock Text="Bulk Renamer" />
  ```
- **ViewModel**:
  ```csharp
  // Inject ILanguageService into ViewModel constructor
  private string GetLoc(string key, string fallback) =>
      _languageService?.GetString(key, fallback) ?? fallback;
  ```
- **Dictionary Symmetry**:
  Every key added to `Strings.vi-VN.xaml` must be added with identical key name to `Strings.en-US.xaml`.
