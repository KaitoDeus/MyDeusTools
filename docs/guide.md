# Hướng dẫn triển khai dự án: MyDeusTools (Executive Edition)

Chào mừng bạn đến với lộ trình xây dựng siêu ứng dụng MyDeusTools. Tài liệu này đóng vai trò là "kim chỉ nam" về quy trình và tư duy phát triển dự án.

---

## Bước 1: Chuẩn bị môi trường & Khởi tạo dự án

Đầu tiên, chúng ta cần tạo một cấu trúc Solution vững chắc bằng cách khởi tạo Solution và Project WPF (.NET 8). Đây là nền móng để quản lý toàn bộ các module công cụ sau này.

---

## Bước 2: Thiết lập cấu trúc thư mục (Folder Structure)

Để dự án dễ mở rộng, hãy tổ chức thư mục một cách khoa học:

- **Models:** Chứa thực thể dữ liệu.
- **ViewModels:** Điều khiển logic giao diện.
- **Views:** Nơi định nghĩa XAML.
- **Services:** Chứa logic nghiệp vụ cốt lõi.
- **Resources:** Quản lý tài nguyên hình ảnh và phong cách.

---

## Bước 3: Cài đặt các thư viện quan trọng (NuGet)

Sử dụng các thư viện hàng đầu hiện nay để tối ưu hóa hiệu suất:

- **WPF-UI:** Mang lại giao diện Fluent Design hiện đại.
- **MVVM Toolkit:** Giảm thiểu code rườm rà (boilerplate).
- **Dependency Injection:** Quản lý vòng đời đối tượng.
- **NHotkey:** Xử lý phím tắt hệ thống.

---

## Bước 4: Triển khai kiến trúc Dependency Injection (DI)

Kiến trúc này giúp ứng dụng linh hoạt và dễ kiểm thử. Thay vì khởi tạo thủ công, chúng ta đăng ký các thành phần vào một "thùng chứa" tập trung và để hệ thống tự động cung cấp khi cần thiết.

---

## Bước 5: Thiết lập Điều hướng (Navigation Service)

Xây dựng một dịch vụ điều hướng trung tâm để quản lý việc chuyển đổi qua lại giữa các công cụ (như từ AutoClicker sang Sticky Note) một cách mượt mà và không lỗi trạng thái.

---

## Bước 6: Xây dựng Giao diện chính (The Shell)

Thiết kế cửa sổ chính (Shell) đóng vai trò là khung đỡ cho toàn bộ ứng dụng. Nó bao gồm thanh menu bên trái để lựa chọn công cụ và vùng hiển thị nội dung chính ở bên phải.

---

## Bước 7: Thiết kế Theme và Resources

Đảm bảo ứng dụng hòa hợp với Windows bằng cách hỗ trợ đầy đủ chế độ Sáng/Tối (Light/Dark mode) và các hiệu ứng hình ảnh cao cấp như Mica hay bo góc.

---

## Bước 8: Quy trình thêm tính năng mới (The MVVM Workflow)

Mỗi khi muốn thêm một công cụ mới, hãy tuân thủ chu trình:

1. Xác định logic xử lý (**Service**).
2. Kết nối logic với giao diện (**ViewModel**).
3. Thiết kế trang hiển thị (**View**).
4. Đăng ký module vào hệ thống (**DI Registration**).

---

## 🚀 Hướng dẫn chi tiết cho các Module mục tiêu

### 1. Module: Schedule Shutdown (Hẹn giờ tắt máy)
Đây là module đơn giản nhất để làm quen với quy trình.
- **Tư duy Logic:** Sử dụng lớp `System.Diagnostics.Process` để gọi lệnh hệ thống `shutdown.exe`. Các tham số quan trọng: `/s` (shutdown), `/t` (time), `/a` (abort).
- **Service:** Thiết kế `ISystemService` có các hàm `Schedule(int seconds)` và `Cancel()`.
- **UI:** Cần các ô nhập (Input) cho Giờ/Phút/Giây và các nút bấm Start/Stop trực quan.

### 2. Module: AutoClicker Pro (Tự động Click chuột)
Đây là module nâng cao, yêu cầu sự kết hợp giữa hệ thống và giao diện.

#### A. Logic cốt lõi (The Engine)
- **Khai báo Windows API (P/Invoke):** Để điều khiển chuột, bạn cần khai báo hàm từ `user32.dll`. Đây là "cầu nối" giữa C# và Windows.
```csharp
[DllImport("user32.dll")]
static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

// Các hằng số (Flags) quan trọng
const int MOUSEEVENTF_LEFTDOWN = 0x02;
const int MOUSEEVENTF_LEFTUP = 0x04;
const int MOUSEEVENTF_RIGHTDOWN = 0x08;
const int MOUSEEVENTF_RIGHTUP = 0x10;
```

- **Vòng lặp (The Loop):** Sử dụng `DispatcherTimer` để an toàn cho UI thread.
```csharp
// Trong Service của bạn
private DispatcherTimer _timer;

_timer = new DispatcherTimer();
_timer.Tick += (s, e) => {
    // Gọi hàm click ở đây
    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
};
```

#### B. Thiết kế Service
Hãy định nghĩa một Interface để đảm bảo tính trừu tượng (Abstraction).
```csharp
public interface IAutoClickService
{
    bool IsRunning { get; }
    void Start(int intervalMs);
    void Stop();
}
```

#### C. Phím tắt (Global Hotkeys)
Sử dụng thư viện `NHotkey` để bắt sự kiện phím tắt toàn hệ thống.
```csharp
// Trong ViewModel
HotkeyManager.Current.AddOrReplace("StartStopClick", Key.F6, ModifierKeys.None, OnHotkeyPressed);

private void OnHotkeyPressed(object sender, HotkeyEventArgs e)
{
    // Logic đảo ngược trạng thái Start/Stop
}
```

#### D. Giao diện (User Interface)
Thiết kế các điều khiển nhập liệu và hiển thị trạng thái một cách trực quan.
- Sử dụng `ui:NumberBox` cho Interval, dàn hàng ngang (Horizontal Layout) bằng Grid để tối ưu không gian.
- Thay thế `TabControl` bằng `RadioButton` được styled thành `ToggleButton` để chuyển đổi giữa các Form, giúp khắc phục lỗi Resource resolution của Designer.
- Sử dụng `ui:Button` với màu sắc thay đổi theo trạng thái `IsRunning`.
- Cấu hình Window với `ResizeMode="NoResize"` và `ShowMaximize="False"` để khóa cứng trải nghiệm người dùng.

### 3. Module: Sticky Note (Ghi chú nhanh)
Đây là module giúp bạn luyện tập về quản lý dữ liệu và cửa sổ.
- **Tư duy Logic:** Cần một cơ chế lưu trữ (Persistence). Đơn giản nhất là lưu danh sách ghi chú thành file `.json` trong thư mục `AppData`.
- **Đặc trưng giao diện:** Cửa sổ ghi chú thường không có thanh tiêu đề chuẩn, cần hỗ trợ tính năng "Always on Top" (luôn hiện trên cùng).
- **MVVM nâng cao:** Sử dụng `ObservableCollection` để quản lý danh sách các ghi chú, giúp giao diện tự động cập nhật khi bạn thêm/xóa.

---

## Bước 9: Tích hợp System Tray (Khay hệ thống)

Tính năng này giúp ứng dụng chạy ngầm và người dùng có thể truy cập nhanh từ góc phải màn hình.

1. **Cài đặt thư viện:**
   Mở terminal và chạy lệnh:
   `dotnet add package Hardcodet.NotifyIcon.Wpf`

2. **Khởi tạo TaskbarIcon trong C#:**
   Viết code khởi tạo trong `MainWindow.xaml.cs`:
   ```csharp
   using Hardcodet.Wpf.TaskbarNotification;
   
   private TaskbarIcon? _notifyIcon;

   private void InitializeTray()
   {
       _notifyIcon = new TaskbarIcon
       {
           ToolTipText = "MyDeusTools is running",
           IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/avatar.ico")),
           ContextMenu = new ContextMenu()
       };

       var openItem = new MenuItem { Header = "Mở ứng dụng" };
       openItem.Click += OnShowMainWindow;
       var exitItem = new MenuItem { Header = "Thoát" };
       exitItem.Click += OnExitApp;

       _notifyIcon.ContextMenu.Items.Add(openItem);
       _notifyIcon.ContextMenu.Items.Add(new Separator());
       _notifyIcon.ContextMenu.Items.Add(exitItem);

       _notifyIcon.TrayLeftMouseUp += (s, e) => ShowWindow();
   }
   ```

3. **Xử lý sự kiện Close:**
   Ghi đè sự kiện `Closing` để ẩn cửa sổ thay vì tắt ứng dụng hoàn toàn. Sử dụng một biến cờ `_isExitAllowed` để thực sự thoát khi chọn từ menu Tray.

---

## Bước 10: Tối ưu & Đóng gói (Single File Publish)

Để người dùng chỉ cần nhận một file `.exe` duy nhất mà không cần cài đặt rườm rà:

1. **Cấu hình .csproj:**
   Thêm các thuộc tính sau vào `PropertyGroup`:
   - `<PublishSingleFile>true</PublishSingleFile>`: Gộp toàn bộ DLL vào một file EXE.
   - `<SelfContained>true</SelfContained>`: Đóng gói kèm theo .NET Runtime (không cần cài .NET máy khách).
   - `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`: Chỉ định kiến trúc hệ điều hành.
   - `<PublishReadyToRun>true</PublishReadyToRun>`: Tối ưu thời gian khởi động.

2. **Lệnh xuất bản:**
   Sử dụng lệnh sau trong terminal:
   `dotnet publish -c Release`

---

### Lời khuyên từ cố vấn:

> "Đừng cố làm tất cả các công cụ cùng một lúc. Hãy hoàn thiện bộ khung thật chỉn chu, sau đó việc thêm các module mới sẽ cực kỳ nhanh chóng và an toàn."

**Chúc bạn thành công với dự án MyDeusTools!**
