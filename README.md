# MyDeusTools

<p align="center">
  <img src="MyDeusTools.App/Resources/avatar.ico" width="80" height="80" alt="MyDeusTools Icon" />
</p>

<p align="center">
  <strong>Modern All-in-One Desktop Utility Suite for Windows 10 & 11</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/UI-WPF--UI%203.0-0078D4?logo=windows" alt="WPF-UI" />
  <img src="https://img.shields.io/badge/Architecture-MVVM%20Toolkit-blue" alt="MVVM" />
  <img src="https://img.shields.io/badge/Tests-173%20Passed%20(100%25)-brightgreen" alt="Tests" />
  <img src="https://img.shields.io/badge/Platform-Windows%20x64-00A4EF" alt="Platform" />
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License" />
</p>

<p align="center">
  <a href="https://kaitodeus.github.io/MyDeusTools/"><strong>Explore Official Website &amp; Interactive Simulator &rarr;</strong></a>
</p>

---

## Overview

MyDeusTools is a lightweight, modern, and versatile Windows desktop utility suite built with WPF and WPF-UI 3.0 (Windows 11 Fluent Design & Mica Backdrop). Instead of installing multiple standalone applications, MyDeusTools consolidates essential daily productivity tools into a single, cohesive application that packages into a zero-dependency, self-contained single-file executable.

---

## Key Features

| Feature | Description |
|---|---|
| **Auto Clicker Pro** | Highly configurable automated mouse clicking (Left, Right, Middle buttons; Single or Double click), high-precision interval control (Hours, Minutes, Seconds, Milliseconds with 10ms safety clamp), global hotkey assignment (NHotkey), and full-screen coordinate recording/playback across multi-monitor setups (RecordingOverlayWindow). |
| **Schedule Shutdown** | Timed system power management supporting Shutdown, Restart, and Hibernate actions, featuring a real-time visual countdown timer (hh:mm:ss) and instantaneous cancellation support. |
| **Sticky Notes** | Card-based desktop note manager with automatic JSON persistence (Data/notes.json), and pop-out borderless floating desktop windows (StickyNoteWindow - Topmost, draggable, non-intrusive). |
| **Clipboard Manager** | Real-time clipboard listener utilizing native Win32 APIs (AddClipboardFormatListener), instant search filtering, pin-to-top support, and one-click copy back to clipboard. |
| **QR Code Studio** | High-definition QR code generation (QRCoder) with PNG export and clipboard copy; multi-source QR decoding (ZXing.Net) supporting screen region snipping (QrSnippingOverlayWindow), image file decoding, and clipboard image reading with automatic web URL detection. |
| **Color Picker & Eyedropper** | Fullscreen precision screen eyedropper with floating 11x11 pixel magnifier HUD, one-click HEX/RGB/HSL/HSV format copying, pin-to-palette management, and persistent color history (Data/colors.json). |
| **Text & Dev Tools** | Swiss Army knife for text and code: JSON Formatter and Minifier with syntax validator, Base64 encoder/decoder (Text and Files), URL and HTML entity escaping, instant multi-hash generator (MD5, SHA-1, SHA-256, SHA-512), case converter (camel, Pascal, snake, kebab, UPPER, lower, Title), and text statistics analyzer. |
| **Window Pinner & Transparency** | Pin any third-party application window to remain Always-on-Top (HWND_TOPMOST), smoothly adjust real-time opacity and transparency (Win32 WS_EX_LAYERED with SetLayeredWindowAttributes), quick opacity presets (100%, 80%, 60%, 40%), 1-click active window pinning, and bring-to-front window activation. |
| **Video Converter & Transcoder** | Fast and versatile multimedia converter supporting diverse containers (MP4, MKV, WebM, AVI, MOV, WMV, GIF, FLV), audio extraction (MP3, WAV, AAC, M4A, FLAC), resolution scaling (4K to 360p), CRF quality controls (Ultra, High, Medium, Compact), lossless stream copy, GPU hardware acceleration, video trimming, and real-time progress parsing. |
| **Bilingual Localization (EN & VIE)** | Instant runtime switching between English and Vietnamese across all navigation menus and pages without requiring application restarts, with persistent settings (Data/settings.json). |
| **System Tray & Windows Startup** | Minimize-to-tray lifecycle management (Hardcodet.NotifyIcon), instant Dark/Light theme switching, and Windows startup toggle via CurrentUser Registry. |

---

## Technology Stack

- **Framework & Runtime**: C# 12, .NET 8.0 Windows Desktop (net8.0-windows, win-x64)
- **UI Framework**: WPF with [WPF-UI 3.0.5](https://wpfui.lepo.co/) (Fluent Design, Mica Backdrop, NavigationControl)
- **Architecture**: Model-View-ViewModel (MVVM) powered by [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **OS & Native Interop**:
  - Win32 P/Invoke (user32.dll for mouse synthesization, clipboard format listener, window enumeration, HWND_TOPMOST pinning, and layered opacity attributes)
  - NHotkey.Wpf for global system-wide hotkeys
  - Hardcodet.NotifyIcon.Wpf for taskbar notification tray
  - QRCoder & ZXing.Net for QR code generation and decoding
- **Testing**: xUnit 2.9, Moq 4.20, Xunit.StaFact (173/173 tests passing, 100% success rate)

---

## Getting Started

### Prerequisites
- Operating System: Windows 10 or Windows 11 (x64)
- .NET 8.0 SDK (required for building from source)

### 1. Run in Development Mode
```bash
# Clone repository
git clone https://github.com/KaitoDeus/MyDeusTools.git
cd MyDeusTools

# Run application
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

### 2. Run Automated Test Suite
```bash
dotnet test
```

### 3. Build Single-File Self-Contained Executable
```bash
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```
The standalone executable will be located at:
`MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe`

---

## Contributors

This project is developed and maintained through Human-AI Pair Programming:

- **[KaitoDeus](https://github.com/KaitoDeus)** — Project Creator, Lead Software Architect & Developer
- **[Claude](https://github.com/claude)** — AI Pair Programmer & Code Contributor

---

## License

This project is licensed under the [MIT License](LICENSE).
