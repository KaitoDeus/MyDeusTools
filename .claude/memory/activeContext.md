# Active Context - MyDeusTools

## Current Focus & Status
- **Current State**: Project builds cleanly with 0 warnings/errors, and passes 100% of unit & integration tests (53/53 passed).
- **Recent Milestone**: Successfully implemented **Color Picker & Eyedropper** (Fullscreen precision screen eyedropper with floating 11x11 pixel Magnifier HUD, instant HEX/RGB/HSL/HSV format copy, color history palette with pin support, and comprehensive test suite).

## Active Session Goals
1. Maintain and extend Super-App features according to user priorities.
2. Ensure 100% test coverage for newly introduced business services.
3. Keep Memory Bank synchronized.

## Active Considerations & Technical Notes
- **Color Picker & Eyedropper**: Uses Win32 GDI `GetDC`, `GetPixel`, `ReleaseDC` and `CopyFromScreen` for real-time 11x11 pixel grid sampling. The overlay window is hidden during selection to capture actual desktop/application pixels.
- **Serialization Safety**: Computed model properties (`PreviewBrush`, `Rgb`, `Hsl`, `Hsv`) are annotated with `[JsonIgnore]` to prevent `JsonException` during `SolidColorBrush` serialization.
- **QR Code Studio**: Generation is powered by `QRCoder` (zero external native dependencies); decoding is powered by `ZXing.Net`.
- **Screen Snipping Scanner**: Uses `QrSnippingOverlayWindow` with crosshair cursor, drag selection rectangle, and virtual screen multi-monitor coordinates. Screen pixels are captured via GDI `CopyFromScreen` after hiding the overlay to prevent capturing the overlay itself.
- **Clipboard Monitoring**: Uses Win32 `AddClipboardFormatListener` / `RemoveClipboardFormatListener` via `HwndSource` on `MainWindow`.
- **Clipboard Concurrency & Re-entrancy**: Copying from inside the app flags `_isInternalCopy = true` to avoid redundant loops. Clipboard read operations dispatch to STA thread with try-catch.
- **WPF Single File Publishing**: Configured in `MyDeusTools.App.csproj` (`PublishSingleFile=true`, `SelfContained=true`, `RuntimeIdentifier=win-x64`).
- **Resource Bundling**: `avatar.ico` is compiled as `<Resource Include="Resources\avatar.ico" />` and loaded with `pack://application:,,,/Resources/avatar.ico`.
- **Note & Clipboard Persistence**: Files are saved in `AppDomain.CurrentDomain.BaseDirectory\Data\` (`notes.json`, `clipboard.json`).
- **Recording Overlay**: Supports multi-monitor configurations using `SystemParameters.VirtualScreen*`.
