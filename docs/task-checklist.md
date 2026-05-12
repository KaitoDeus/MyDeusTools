# Task Checklist: MyDeusTools

Dưới đây là danh sách các bước triển khai dự án theo `guide.md`. Các bước đã hoàn thành sẽ được đánh dấu [x].

## 1. Chuẩn bị & Khởi tạo dự án

- [x] Bước 1: Khởi tạo Solution và Project WPF (.NET 8/9)
- [x] Bước 2: Thiết lập cấu trúc thư mục (`Models/`, `ViewModels/`, `Views/`, `Services/`, `Resources/`)
- [x] Bước 3: Cài đặt các thư viện NuGet quan trọng:
  - [x] `Wpf.Ui` (v3.0.5 - Giao diện hiện đại)
  - [x] `CommunityToolkit.Mvvm` (MVVM Toolkit)
  - [x] `Microsoft.Extensions.DependencyInjection` (DI Container)
  - [x] `NHotkey.Wpf` (Quản lý phím tắt)

## 2. Thiết lập kiến trúc nền tảng (Core Architecture)

- [x] Bước 4: Triển khai Dependency Injection (DI) trong `App.xaml.cs`
- [x] Bước 5: Thiết lập Navigation Service (Điều hướng trang)

## 3. Xây dựng Giao diện (UI/UX)

- [x] Bước 6: Xây dựng Shell Window (MainWindow) với `NavigationView`
- [x] Bước 7: Thiết kế Theme (Light/Dark Mode) và Resources

## 4. Phát triển các Module (Features)

- [x] Bước 8: Module **Schedule Shutdown**
    - [x] `SystemService.cs`
    - [x] `ShutdownViewModel.cs`
    - [x] `ShutdownPage.xaml`
- [x] Bước 9: Module **AutoClicker Pro** (Cơ bản)
- [ ] Bước 9.1: Nâng cấp **AutoClicker Pro** (Nâng cao)
  - [ ] Nâng cấp `IAutoClickService` (Hỗ trợ Left/Right, Single/Double)
  - [ ] Triển khai `RecordingEngine` (Ghi và phát lại tọa độ)
  - [ ] Thiết lập Hotkey tùy chỉnh (Custom Hotkeys)
  - [ ] Re-design `AutoClickPage.xaml` (Giao diện chuyên nghiệp hơn)
- [x] Bước 10: Module **Sticky Note**
  - [x] `StickyNoteService.cs`
  - [x] `StickyNoteViewModel.cs`
  - [x] `StickyNotePage.xaml`

## 5. Hoàn thiện & Đóng gói

- [ ] Bước 11: Tích hợp System Tray (Khay hệ thống)
- [ ] Bước 12: Đóng gói ứng dụng (Single File Publish)
