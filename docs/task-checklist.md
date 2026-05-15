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
- [x] Bước 9.1: Nâng cấp **AutoClicker Pro** (Nâng cao)
  - [x] Nâng cấp `IAutoClickService` (Hỗ trợ Left/Right, Single/Double)
  - [x] Triển khai `Recording Engine` thế hệ mới (Interactive Overlay click)
  - [x] Hệ thống gán phím tắt động (Dynamic Key Capturing & Modifiers)
  - [x] Khắc phục lỗi Layout & Hit-testing (Scrolling fix cho toàn bộ App)
  - [x] Tối ưu Sidebar width và giao diện Fluent chuyên nghiệp
  - [x] Redesign AutoClicker: Button-based navigation, horizontal interval layout
  - [x] Window Security: Hard-locked resolution (1280x720) and disabled resizing/maximization
- [x] Bước 10: Module **Sticky Note**
  - [x] `StickyNoteService.cs`
  - [x] `StickyNoteViewModel.cs`
  - [x] `StickyNotePage.xaml`
- [ ] Bước 13: Hoàn thiện **AutoClicker Pro**
  - [ ] Repeat Settings: Loop Mode (Chạy vô hạn hoặc lặp lại theo số lần)
  - [ ] Schedule Execution (Tự động dừng sau một khoảng thời gian)
- [ ] Bước 14: Module **Alarm Clock** (Đồng hồ báo thức)
  - [ ] Alarms: Quản lý nhiều mốc báo thức & âm báo
  - [ ] Timer: Đồng hồ đếm ngược
  - [ ] Stopwatch: Bấm giờ và ghi nhận Lap (Lap recording)
  - [ ] World Clock: Giờ thế giới
- [ ] Bước 15: Module **Screen Recorder** (Quay màn hình)
  - [ ] Capture Engine: Quay toàn màn hình, cửa sổ, khu vực
  - [ ] Audio Engine: Thu âm hệ thống & Micro
  - [ ] Overlay & Drawing: Tích hợp hiệu ứng chuột, vẽ trực tiếp
  - [ ] Quality Settings: Tùy chỉnh FPS, độ phân giải, định dạng

## 5. Hoàn thiện & Đóng gói

- [x] Bước 11: Tích hợp System Tray (Khay hệ thống)
- [x] Bước 12: Đóng gói ứng dụng (Single File Publish)
- [ ] Bước 16: Tích hợp hệ thống: Khởi động cùng Windows (Auto-start)
