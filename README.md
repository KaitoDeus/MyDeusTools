# MyDeusTools

<p align="center">
  <img src="MyDeusTools.App/Resources/avatar.ico" width="80" height="80" alt="MyDeusTools Icon" />
</p>

<p align="center">
  <strong>Bộ tiện ích máy tính tất cả trong một dành cho Windows 10 và 11</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0--windows-512BD4?logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/UI-WPF--UI%203.0-0078D4?logo=windows" alt="WPF-UI" />
  <img src="https://img.shields.io/badge/Architecture-MVVM%20Toolkit-blue" alt="MVVM" />
  <img src="https://img.shields.io/badge/Tests-43%20Passed%20(100%25)-brightgreen" alt="Tests" />
  <img src="https://img.shields.io/badge/Platform-Windows%20x64-00A4EF" alt="Platform" />
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License" />
</p>

---

## Giới thiệu

MyDeusTools là ứng dụng tiện ích máy tính hiện đại, gọn nhẹ và đa năng, được thiết kế theo ngôn ngữ giao diện Fluent Design (Mica Backdrop) của Windows 11. Thay vì phải cài đặt nhiều phần mềm riêng lẻ, MyDeusTools tích hợp các công cụ thiết yếu hàng ngày vào một ứng dụng duy nhất, có thể đóng gói thành một file thực thi độc lập (.exe) chạy ngay mà không cần cài đặt.

---

## Tính năng chính

| Tính năng | Mô tả chi tiết |
|---|---|
| **Auto Clicker Pro** | Click chuột tự động tùy chỉnh (Trái, Phải, Giữa, Đơn, Đúp), điều chỉnh khoảng thời gian (Ms/Giây/Phút/Giờ, tối thiểu 10ms), gán phím tắt toàn cục tùy ý (NHotkey), chế độ ghi và phát lại chuỗi tọa độ đa màn hình (RecordingOverlayWindow). |
| **Schedule Shutdown** | Lên lịch hẹn giờ Tắt máy (Shutdown), Khởi động lại (Restart), Ngủ đông (Hibernate) với đồng hồ đếm ngược thời gian thực định dạng hh:mm:ss và hỗ trợ hủy lệnh bất kỳ lúc nào. |
| **Sticky Notes** | Ghi chú nổi trên màn hình dạng thẻ, tự động lưu trữ (Data/notes.json), hỗ trợ ghim từng ghi chú ra cửa sổ riêng biệt trên Desktop (StickyNoteWindow - Topmost, không viền, kéo thả tự do). |
| **Clipboard Manager** | Tự động theo dõi khay nhớ tạm thời gian thực thông qua Win32 API (AddClipboardFormatListener), tìm kiếm nhanh nội dung đã sao chép, ghim các đoạn text quan trọng, copy lại với một thao tác. |
| **QR Code Studio** | Tạo mã QR độ nét cao tức thì (QRCoder), xuất file PNG hoặc sao chép ảnh vào clipboard; Quét mã QR đa nguồn (ZXing.Net): chụp cắt vùng màn hình trực tiếp (QrSnippingOverlayWindow), quét từ tệp ảnh hoặc từ khay nhớ tạm, tự nhận diện liên kết web. |
| **System Tray & Khởi động cùng Windows** | Thu nhỏ ứng dụng vào khay hệ thống (Taskbar Tray Icon) khi đóng cửa sổ, chuyển đổi giao diện Sáng / Tối (Dark/Light mode) linh hoạt, tùy chọn khởi động cùng Windows qua Registry. |

---

## Công nghệ sử dụng

- **Ngôn ngữ và Nền tảng**: C# 12, .NET 8.0 Windows Desktop (net8.0-windows, win-x64)
- **Giao diện**: WPF kết hợp thư viện giao diện hiện đại [WPF-UI 3.0.5](https://wpfui.lepo.co/) (Mica Effect, FluentWindow, NavigationControl)
- **Kiến trúc**: Model-View-ViewModel (MVVM) với [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Tương tác phần cứng và Hệ điều hành**:
  - Win32 P/Invoke (user32.dll cho mô phỏng chuột, lắng nghe Clipboard)
  - NHotkey.Wpf cho phím tắt toàn cục
  - Hardcodet.NotifyIcon.Wpf cho Taskbar Notification Tray
  - QRCoder và ZXing.Net cho xử lý mã QR
- **Kiểm thử tự động**: xUnit 2.9, Moq 4.20, Xunit.StaFact (43/43 tests passed)

---

## Cài đặt và Khởi chạy

### Yêu cầu hệ thống
- Hệ điều hành: Windows 10 hoặc Windows 11 (x64)
- .NET 8.0 SDK (nếu muốn tự biên dịch từ mã nguồn)

### 1. Khởi chạy trong môi trường phát triển
```bash
# Clone repository
git clone https://github.com/KaitoDeus/MyDeusTools.git
cd MyDeusTools

# Chạy ứng dụng
dotnet run --project MyDeusTools.App/MyDeusTools.App.csproj
```

### 2. Chạy kiểm thử tự động
```bash
dotnet test
```

### 3. Đóng gói file thực thi độc lập (Single-File Executable)
```bash
dotnet publish MyDeusTools.App/MyDeusTools.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```
File thực thi nằm tại:
`MyDeusTools.App/bin/Release/net8.0-windows/win-x64/publish/MyDeusTools.App.exe`

---

## Đóng góp và Phát triển

Dự án được xây dựng và phát triển thông qua mô hình Human-AI Pair Programming:

- **[KaitoDeus](https://github.com/KaitoDeus)** — Tác giả, Kiến trúc sư và Nhà phát triển chính
- **[Claude](https://anthropic.com)** (Anthropic) — Trợ lý phát triển AI (Cộng sự kiến trúc, tối ưu mã nguồn và lập trình module)

---

## Bản quyền

Dự án được phân phối dưới giấy phép [MIT License](LICENSE).
