---
name: feature-expansion
description: >-
  Standardized end-to-end workflow for adding a new native utility tool to MyDeusTools.
  Covers Service interface, Service implementation, ViewModel, View XAML, Navigation, DI registration, bilingual strings, unit tests, and release verification.
---

# Feature Expansion Workflow for MyDeusTools

Follow this standardized 8-step sequence whenever designing, developing, and integrating a new native utility into MyDeusTools.

---

## Step 1: Define Service Interface & Models
- **Location**: `MyDeusTools.App/Services/Impl/I<FeatureName>Service.cs`
- Define clear method contracts with asynchronous execution, cancellation tokens (`CancellationToken`), and progress reporting (`IProgress<T>`).
- Define all necessary enums, configurations, and result models.

## Step 2: Implement Service Logic
- **Location**: `MyDeusTools.App/Services/Impl/<FeatureName>Service.cs`
- Implement the interface using pure managed .NET 8 code or lightweight, well-vetted libraries (e.g. `PDFsharp`, `QRCoder`, `ZXing.Net`).
- Ensure all file operations are thread-safe and handle IO exceptions gracefully.

## Step 3: Author ViewModel
- **Location**: `MyDeusTools.App/ViewModels/<FeatureName>ViewModel.cs`
- Inherit from `ObservableObject`.
- Inject `I<FeatureName>Service` and optional `ILanguageService`.
- Declare observable properties with `[ObservableProperty]` and commands with `[RelayCommand]`.
- Provide helper item ViewModels if dealing with collections or queues.

## Step 4: Create View (Page & Sub-Windows)
- **Location**: `MyDeusTools.App/Views/Pages/<FeatureName>Page.xaml` and `.xaml.cs`
- Root element: `<Page x:Class="..." ... Title="{DynamicResource ...}">`.
- Use Lepo `Wpf.Ui` controls (`ui:Button`, `ui:Card`, `ui:TextBox`, `ui:SymbolIcon`).
- Bind all user-facing strings to `{DynamicResource ...}` keys.
- If drag-and-drop is required, implement `AllowDrop="True"` with `Drop` and `DragOver` handlers in code-behind calling ViewModel methods.

## Step 5: Register in Shell Navigation & Dependency Injection
1. **DI Container (`App.xaml.cs`)**:
   - Register Service: `services.AddSingleton<I<FeatureName>Service, <FeatureName>Service>();`
   - Register ViewModel: `services.AddTransient<<FeatureName>ViewModel>();`
   - Register Page: `services.AddTransient<<FeatureName>Page>();`
2. **Navigation Menu (`MainWindow.xaml`)**:
   - Add `<ui:NavigationViewItem>` with `TargetPageType="{x:Type pages:<FeatureName>Page}"` and a relevant `ui:SymbolIcon`.

## Step 6: Add Symmetrical Bilingual Localization
1. Add all keys to `MyDeusTools.App/Resources/Languages/Strings.vi-VN.xaml` (Vietnamese).
2. Add the exact same keys to `MyDeusTools.App/Resources/Languages/Strings.en-US.xaml` (English).
3. Ensure 100% key name symmetry between both files.

## Step 7: Author Unit & Integration Tests
- **Location**: `MyDeusTools.Tests/<FeatureName>ServiceTests.cs` and `<FeatureName>ViewModelTests.cs`
- Cover normal execution, boundary edge cases, null handling, cancellation, and validation errors.
- Add new navigation/title keys to `LanguageServiceTests.cs` theory tests.
- Run `dotnet test` and confirm all tests pass (100% pass rate).

## Step 8: Build Verification & Git Commit
1. Verify Release build: `dotnet build -c Release` (0 warnings, 0 errors).
2. Stage and commit changes using Conventional Commits:
   `feat(<scope>): add <feature name> with ...`
   `Co-authored-by: Claude <81847+claude@users.noreply.github.com>`
3. Push to `origin main`.
