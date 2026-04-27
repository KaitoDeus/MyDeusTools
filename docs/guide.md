# Hướng dẫn triển khai dự án: MyDeusTools

Chào mừng bạn đến với lộ trình xây dựng siêu ứng dụng MyDeusTools. Tài liệu này đóng vai trò là "kim chỉ nam" giúp bạn đi từ khâu khởi tạo đến khi ra mắt sản phẩm hoàn chỉnh.

---

## Bước 1: Chuẩn bị môi trường & Khởi tạo dự án
Đầu tiên, chúng ta cần tạo một cấu trúc Solution vững chắc.

1.  **Công cụ cần thiết:**
    *   Visual Studio 2022 (phiên bản mới nhất) hoặc VS Code.
    *   .NET 8 SDK.
2.  **Tạo dự án qua Command Line (Khuyên dùng):**
    ```powershell
    # Tạo Solution
    dotnet new sln -n MyDeusTools
    
    # Tạo Project WPF
    dotnet new wpf -n MyDeusTools.App
    
    # Thêm Project vào Solution
    dotnet sln add MyDeusTools.App/MyDeusTools.App.csproj
    ```

---

## Bước 2: Thiết lập cấu trúc thư mục (Folder Structure)
Để dự án dễ mở rộng, hãy tổ chức thư mục trong Project `MyDeusTools.App` như sau:
*   `Models/`: Chứa các lớp thực thể dữ liệu.
*   `ViewModels/`: Chứa logic xử lý giao diện (Dùng CommunityToolkit.Mvvm).
*   `Views/`: Chứa file XAML (Pages, Windows, UserControls).
*   `Services/`: Chứa logic nghiệp vụ (Ví dụ: `AutoClickService.cs`, `SystemTimeService.cs`).
*   `Resources/`: Chứa Styles, Icons, Images.

---

## Bước 3: Cài đặt các thư viện quan trọng (NuGet)
Mở Package Manager Console và chạy các lệnh sau:
```powershell
# UI Hiện đại (Fluent Design)
dotnet add package Wpf.Ui

# MVVM Toolkit (Source Generators)
dotnet add package CommunityToolkit.Mvvm

# Dependency Injection
dotnet add package Microsoft.Extensions.DependencyInjection

# Quản lý phím tắt
dotnet add package NHotkey.Wpf
```

---

## Bước 4: Triển khai kiến trúc Dependency Injection (DI)
Đây là bước quan trọng nhất để app chạy chuyên nghiệp. Chỉnh sửa file `App.xaml.cs`:

1.  Khởi tạo `IHost` để quản lý các Service và ViewModel.
2.  Đăng ký các Service (Singleton) và ViewModels (Transient).

---

## Bước 5: Kỹ thuật thiết kế Giao diện (Designer & Toolbox)
Trong WPF, giao diện được viết bằng **XAML**. Bạn có hai cách để làm việc:

1.  **Sử dụng Toolbox (Cửa sổ công cụ):**
    *   Mở file `.xaml`, nhấn `Ctrl + Alt + X` để mở Toolbox.
    *   Bạn có thể kéo các control cơ bản như `Button`, `TextBox`, `Grid` vào màn hình Design.
    *   **Lưu ý:** Khi dùng thư viện như `Wpf.Ui`, bạn nên ưu tiên viết XAML trực tiếp vì Toolbox đôi khi không hiển thị hết các thuộc tính tùy chỉnh của thư viện bên thứ ba.
2.  **Sử dụng XAML Designer (Giao diện trực quan):**
    *   Sử dụng cửa sổ **Properties** (nhấn `F4`) để chỉnh màu sắc, lề (Margin), và font chữ mà không cần nhớ tên thuộc tính.
    *   **Mẹo:** Hãy chia màn hình theo chiều dọc (Split view) để vừa nhìn thấy code XAML vừa nhìn thấy kết quả hiển thị ngay lập tức.
3.  **Tư duy Layout:**
    *   Đừng dùng tọa độ tuyệt đối. Hãy dùng `Grid` (chia hàng/cột) và `StackPanel` (xếp chồng) để giao diện tự co giãn theo cửa sổ.

---

## Bước 6: Quy trình viết code chuẩn (The MVVM Workflow)
Để app không bị rối, hãy tuân thủ quy trình 3 bước sau khi tạo một tính năng mới:

1.  **Bước 1: Viết Logic (Service)**
    *   Tạo file trong thư mục `Services/`. Ví dụ: `AutoClickService.cs`.
    *   Đây là nơi chứa code "thuần kỹ thuật" (ví dụ: gọi Windows API để click chuột).
2.  **Bước 2: Viết "Bộ não" giao diện (ViewModel)**
    *   Tạo file trong `ViewModels/`. Ví dụ: `AutoClickViewModel.cs`.
    *   Khai báo các biến mà giao diện sẽ hiển thị (dùng `[ObservableProperty]`).
    *   Viết các hàm xử lý khi nhấn nút (dùng `[RelayCommand]`).
3.  **Bước 3: Viết Giao diện (View)**
    *   Tạo file `AutoClickPage.xaml`.
    *   Dùng `{Binding NameInViewModel}` để kết nối các thành phần giao diện với dữ liệu trong ViewModel.
    *   **Quan trọng:** Hạn chế tối đa việc viết code vào file `.xaml.cs` (Code-behind). Mọi thứ nên nằm ở ViewModel.

3.  Ghi đè phương thức `OnStartup` để khởi chạy `MainWindow` thông qua DI.

---

## Bước 7: Xây dựng Giao diện chính (The Shell)
Đừng bắt đầu bằng code tính năng ngay, hãy xây dựng "Cái khung" trước:
*   **MainWindow.xaml:** Sử dụng `ui:NavigationView` từ thư viện Wpf.Ui.
*   **ContentFrame:** Nơi hiển thị các công cụ khi người dùng nhấn vào Menu.
*   **NavigationService:** Viết một class đơn giản để điều hướng giữa các Page bên trong Frame.

---

## Bước 8: Code tính năng đầu tiên (Module hóa)
Hãy bắt đầu với **Schedule Shutdown** vì nó ít rủi ro nhất:
1.  **Service:** Viết class `SystemService` có phương thức `Shutdown(int minutes)`.
2.  **ViewModel:** Tạo `ShutdownViewModel` chứa biến `TimerValue` và lệnh `StartCommand`.
3.  **View:** Tạo `ShutdownPage.xaml` với giao diện nhập số phút và nút Start.

---

## Bước 9: Tối ưu & Đóng gói
Khi app đã chạy ổn định:
1.  **Single File Publish:** Cấu hình trong file `.csproj` để khi Build chỉ ra một file `.exe` duy nhất cho người dùng dễ sử dụng.
    ```xml
    <PublishSingleFile>true</PublishSingleFile>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PublishReadyToRun>true</PublishReadyToRun>
    ```
2.  **Icon & Branding:** Thiết kế một icon "Deus" thật ngầu để khẳng định thương hiệu cá nhân.

---

### Lời khuyên từ cố vấn:
> "Đừng cố làm tất cả các công cụ cùng một lúc. Hãy hoàn thiện **Bộ khung (Core)** và **Một công cụ (Module)** thật chỉn chu, sau đó các công cụ tiếp theo sẽ chỉ là việc copy-paste cấu trúc."

**Chúc bạn thành công với dự án MyDeusTools!**
