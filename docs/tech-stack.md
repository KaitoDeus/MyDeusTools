# Technology Stack: MyDeusTools (WPF Edition)

Dự án sẽ được xây dựng trên nền tảng .NET hiện đại nhất, tập trung vào hiệu năng cao, giao diện đẹp (Fluent Design) và kiến trúc dễ bảo trì.

## 1. Nền tảng cốt lõi (Core Platform)
*   **Framework:** **.NET 8** (hoặc **.NET 9** nếu muốn đi đầu công nghệ). Đây là các phiên bản Long Term Support (LTS) mang lại hiệu năng vượt trội so với .NET Framework cũ.
*   **Language:** **C# 12** với các tính năng mới như Primary Constructors, Collection Expressions để code ngắn gọn, sạch sẽ.
*   **Project Type:** **WPF (Windows Presentation Foundation)**.

## 2. Giao diện & Trải nghiệm (UI/UX Stack)
Để thoát khỏi cái bóng "nhàm chán" của WPF mặc định, chúng ta sẽ sử dụng:
*   **Library:** **WPF UI** (Của lepo.co) hoặc **ModernWPF**. 
    *   *Lý do:* Mang lại giao diện Windows 11 Fluent Design (Bo góc, hiệu ứng Mica/Acrylic) một cách tự nhiên.
*   **Iconography:** **Font Awesome** hoặc **Material Design Icons**.
*   **Animations:** **WPF-UI built-in animations** kết hợp với Storyboards để tạo hiệu ứng mượt mà khi chuyển đổi giữa các công cụ.

## 3. Kiến trúc phần mềm (Architecture)
*   **Pattern:** **MVVM (Model-View-ViewModel)**.
*   **Toolkit:** **CommunityToolkit.Mvvm** (từ Microsoft).
    *   *Lý do:* Sử dụng **Source Generators** để tự động tạo mã (boilerplate code) cho ObservableProperty và RelayCommand, giúp giảm 50% lượng code thủ công.
*   **Dependency Injection (DI):** **Microsoft.Extensions.DependencyInjection**.
    *   *Lý do:* Quản lý vòng đời của các Service (AutoClickerService, SystemService) và ViewModel một cách chuyên nghiệp.

## 4. Lưu trữ & Dữ liệu (Storage & Data)
*   **Database:** **LiteDB** hoặc **SQLite with EF Core**.
    *   *Lý do:* LiteDB là một NoSQL Database nhỏ gọn, single-file, rất phù hợp cho ứng dụng desktop không cần cài đặt SQL Server.
*   **Configuration:** **C# AppSettings (JSON)** sử dụng `System.Text.Json` để lưu cấu hình người dùng.

## 5. Các thư viện bổ trợ (Essential Libraries)
*   **Logging:** **Serilog** với Sink là File và Console để dễ dàng debug và hỗ trợ người dùng khi có lỗi.
*   **Taskbar/Tray:** **Hardcodet.Wpf.TaskbarNotification**.
    *   *Lý do:* Thư viện tốt nhất hiện nay để quản lý Icon và Context Menu dưới thanh Taskbar.
*   **Hotkeys:** **NHotkey.Wpf**.
    *   *Lý do:* Giúp đăng ký phím tắt toàn hệ thống (Global Hotkeys) một cách đơn giản.

## 6. Lộ trình triển khai kỹ thuật (Technical Roadmap)
1.  **Giai đoạn 1:** Setup Project với Dependency Injection và Navigation Service (Chuyển trang).
2.  **Giai đoạn 2:** Xây dựng Shell Window (Giao diện chính) với hiệu ứng Mica.
3.  **Giai đoạn 3:** Hiện thực hóa từng Module công cụ dưới dạng các UserControl riêng biệt.
4.  **Giai đoạn 4:** Đóng gói ứng dụng sử dụng **MSIX** hoặc **Single Executable (Publish as single file)** để phân phối dạng Portable.
