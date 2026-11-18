# MyDeusTools - API Reference

Covers:
1. **Local Companion Agent REST API** (`http://127.0.0.1:41950/`)
2. **Web Client Contracts & Storage** (`agentClient.ts`, `useOsAgent()`, Dexie `db.ts`)
3. **Agent & Native WPF C# Interfaces** (`MyDeusTools.Agent.Services` & `MyDeusTools.App.Services.Impl`)

---

## 1. Local Companion Agent REST API (`http://127.0.0.1:41950/`)

All endpoints use `application/json; charset=utf-8` (camelCase JSON) and include CORS + `Access-Control-Allow-Private-Network: true` headers for `GET`, `POST`, and `OPTIONS`.

### Health & Status
- `GET /api/health`
  - **Response (`200 OK`)**:
    ```json
    {
      "status": "online",
      "agent": "MyDeusTools.Agent",
      "version": "1.0.0",
      "platform": "win32nt",
      "capabilities": ["autoclick", "hotkey-f6", "hotkey-f7", "system-power", "window-pinner"]
    }
    ```

### OS Auto-Clicker Endpoints
- `GET /api/autoclick/status` &rarr; Returns `AutoClickStatusDto`:
  - `{ isRunning, intervalMs, button, clickType, repeatCount, clicksDone, useRecordedPoints, points: [{ x, y }], cursorX, cursorY, hotkeyToggle: "F6", hotkeyCapture: "F7" }`
- `POST /api/autoclick/start`
  - **Body (`StartAutoClickRequest`, fields optional)**: `{ intervalMs?: number, button?: "left" | "right" | "middle", clickType?: "single" | "double", repeatCount?: number, useRecordedPoints?: boolean, points?: [{ x, y }] }`
  - Configures (`IntervalMs` clamped `>= 10ms`) and starts the `OsAutoClickEngine`; returns `AutoClickStatusDto`.
- `POST /api/autoclick/stop` &rarr; Stops engine and returns `AutoClickStatusDto`.
- `POST /api/autoclick/capture`
  - **Body (`CapturePointRequest`)**: `{ delaySeconds?: number }` (clamped `0..10`).
  - Waits `delaySeconds`, records current Win32 cursor point, returns `{ captured: { x, y }, status: AutoClickStatusDto }`.
- `POST /api/autoclick/points/clear` &rarr; Clears recorded waypoints and returns `AutoClickStatusDto`.

### OS System Power Endpoints
- `POST /api/system/schedule`
  - **Body (`SchedulePowerRequest`)**: `{ mode: "shutdown" | "restart" | "hibernate", seconds: number }`
  - Cancels any prior schedule and schedules Windows power action. Returns `{ scheduled: true, mode, seconds }`.
- `POST /api/system/cancel` &rarr; Runs `shutdown /a` and terminates any pending hibernate timeout process. Returns `{ cancelled: true }`.

### OS Window Pinner Endpoints
- `GET /api/windows` &rarr; Returns `OsWindowInfoDto[]`:
  - `[{ handle, title, processName, processId, isTopMost, opacityPercent }]`
- `POST /api/windows/pin` &rarr; Body: `{ handle: number, topMost: boolean }` &rarr; `{ success: boolean }`
- `POST /api/windows/opacity` &rarr; Body: `{ handle: number, percent: number }` (clamped `20..100`) &rarr; `{ success: boolean }`
- `POST /api/windows/focus` &rarr; Body: `{ handle: number }` &rarr; `{ success: boolean }`

---

## 2. Web Client APIs (`MyDeusTools.Web/src/`)

### `agentClient` (`src/services/agentClient.ts`)
Typed `fetch` wrapper against `AGENT_BASE_URL = 'http://127.0.0.1:41950'` with `AbortController` timeouts:
- `checkHealth()` (1200ms timeout)
- `getAutoClickStatus()`, `startOsAutoClick(payload)`, `stopOsAutoClick()`, `captureCursorPoint(delaySeconds = 3)`, `clearCapturedPoints()`
- `scheduleOsPower(mode, seconds)`, `cancelOsPower()`
- `getOsWindows()`, `pinOsWindow(handle, topMost)`, `setOsWindowOpacity(handle, percent)`, `focusOsWindow(handle)`

### Context Hooks
- `useOsAgent()` (`src/contexts/AgentContext.tsx`): `{ isOnline: boolean, health: AgentHealth | null, checking: boolean, refreshAgentStatus: () => Promise<boolean> }` (5-second background polling).
- `useTheme()` (`src/contexts/ThemeContext.tsx`): `{ theme: 'dark' | 'light', toggleTheme, setTheme }`.
- `useI18n()` (`src/i18n/LanguageContext.tsx`): `{ language: 'vi' | 'en', setLanguage, t: Translations, toggleLanguage }`.

### IndexedDB Schema (`src/services/db.ts`)
`MyDeusToolsWebDB` (Dexie):
- `notes`: `StickyNote { id?, title, content, color, isPinned, createdAt, updatedAt }`
- `snippets`: `ClipboardSnippet { id?, title, content, tag, isPinned, usageCount, createdAt }`
- `colors`: `SavedColor { id?, hex, rgb, hsl, hsv, label, createdAt }`

---

## 3. Agent & WPF C# Service Contracts

### Agent Mouse Abstraction (`MyDeusTools.Agent.Services`)
```csharp
public interface IMouseInterop
{
    CursorPoint GetCursorPosition();
    void SetCursorPosition(int x, int y);
    void Click(OsMouseButton button);
    bool IsVirtualKeyPressed(int vkCode);
}
```
- `OsMouseButton`: `Left`, `Right`, `Middle`
- `OsClickType`: `Single`, `Double`
- `OsPowerMode`: `Shutdown`, `Restart`, `Hibernate`

### WPF Desktop Services (`MyDeusTools.App.Services.Impl`)

1. **`IAutoClickService`**: `bool IsRunning`, `void Start(AutoClickConfig)`, `void Stop()`, `void ReplayWaypoints(IReadOnlyList<ClickWaypoint>, TimeSpan)`
2. **`ISystemService`**: `void ScheduleAction(ShutdownMode mode, int seconds)`, `void CancelShutdown()` (`ShutdownMode`: `Shutdown`, `Restart`, `Hibernate`)
3. **`IStickyNoteService`**: `LoadNotesAsync()`, `SaveNotesAsync(...)`, `CreateNoteAsync(string)`, `DeleteNoteAsync(Guid)` (`StickyNoteModel`)
4. **`IClipboardService`**: `event ItemAdded`, `GetHistoryAsync()`, `AddItemAsync(string)`, `TogglePinAsync(Guid)`, `DeleteItemAsync(Guid)`, `ClearUnpinnedAsync()` (`ClipboardItemModel`)
5. **`IQrCodeService`**: `GenerateQrCodePng(string, int)`, `DecodeFromImageBytes(byte[])`, `DecodeFromFile(string)`, `DecodeFromScreenRegion(int, int, int, int)`
6. **`IColorPickerService`**: `SampleScreenPoint(int, int)`, `SampleMagnifierGrid(int, int, int)`, `LoadPaletteAsync()`, `SavePaletteAsync(...)` (`ColorSample`)
7. **`ITextUtilityService`**: `FormatJson`, `ValidateJson`, `EncodeBase64`, `DecodeBase64`, `EncodeBase64File`, `DecodeBase64ToFile`, `UrlEncode`, `UrlDecode`, `HtmlEncode`, `HtmlDecode`, `ComputeHashes`, `ComputeFileHashes`, `ConvertCase`, `AnalyzeText`
8. **`IWindowPinnerService`**: `GetDesktopWindows()`, `GetForegroundWindowInfo()`, `SetAlwaysOnTop(IntPtr, bool)`, `SetWindowOpacity(IntPtr, byte)`, `BringToFront(IntPtr)`
9. **`IVideoConverterService`**: `ConvertMediaAsync(VideoConversionConfig, IProgress<VideoProgressInfo>?, CancellationToken)`
10. **`IImageStudioService`**: `ProcessBatchAsync(IEnumerable<ImageItemViewModel>, ImageProcessingConfig, IProgress<double>?, CancellationToken)`
11. **`IPdfToolkitService`**: `GetPdfInfo`, `MergePdfsAsync`, `SplitPdfAsync`, `ImagesToPdfAsync`, `ProtectPdfAsync`
12. **`IBulkRenamerService`**: `ApplyRenameRule`, `UpdatePreviews`, `ExecuteRenameAsync`, `UndoRenameAsync`, `ScanPaths`
13. **`ILanguageService`**: `CurrentLanguage`, `CurrentLanguageCode`, `event LanguageChanged`, `SetLanguage(AppLanguage)`, `ToggleLanguage()`, `GetString(string, string)`
14. **`IAutoStartService`**: Windows CurrentUser Run registry startup state management.
