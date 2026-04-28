# Hướng dẫn triển khai dự án: MyDeusTools (Full Mentor Edition)

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
Mở Package Manager Console và chạy các lệnh sau (Đã tối ưu cho .NET 8):
```powershell
# UI Hiện đại (Sử dụng ID chính chủ)
dotnet add package WPF-UI --version 2.1.0

# MVVM Toolkit (Source Generators)
dotnet add package CommunityToolkit.Mvvm

# Dependency Injection
dotnet add package Microsoft.Extensions.DependencyInjection

# Quản lý phím tắt
dotnet add package NHotkey.Wpf
```

---

## Bước 4: Triển khai kiến trúc Dependency Injection (DI)
Đây là "xương sống" của ứng dụng. Chúng ta cấu hình trong `App.xaml.cs`:

1.  Loại bỏ `StartupUri` trong `App.xaml`.
2.  Đăng ký các Service và ViewModel trong `ConfigureServices()`:

```csharp
private static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Dịch vụ UI & Navigation
    services.AddSingleton<IPageService, PageService>();
    services.AddSingleton<INavigationService, NavigationService>();

    // ViewModels
    services.AddTransient<MainWindowViewModel>();

    // Views (Windows/Pages)
    services.AddTransient<MainWindow>();

    return services.BuildServiceProvider();
}
```

---

## Bước 5: Thiết lập Điều hướng (Navigation)
Sử dụng `IPageService` (trong `Wpf.Ui.Mvvm.Contracts`) để quản lý chuyển trang.

### PageService.cs chuẩn:
```csharp
public class PageService : IPageService
{
    private readonly IServiceProvider _serviceProvider;
    public PageService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public T? GetPage<T>() where T : class => _serviceProvider.GetService<T>();
    public FrameworkElement? GetPage(Type pageType) => _serviceProvider.GetService(pageType) as FrameworkElement;
}
```

---

## Bước 6: Kỹ thuật thiết kế Giao diện (Designer & Toolbox)
Trong WPF, giao diện được viết bằng **XAML**.

1.  **Sử dụng XAML Designer:** Dùng `F4` để chỉnh thuộc tính nhanh chóng.
2.  **Tư duy Layout:** Ưu tiên `Grid` và `StackPanel` để giao diện tự co giãn (Responsive). Hạn chế dùng tọa độ tuyệt đối.
3.  **Wpf.Ui Styles:** Nhớ tích hợp các Resources của Wpf.Ui vào `App.xaml` để các nút bấm, ô nhập liệu có giao diện Fluent Design đẹp mắt.

---

## Bước 7: Xây dựng Giao diện chính (The Shell)
Thiết lập "Cái khung" trong `MainWindow.xaml`:
*   Sử dụng `ui:NavigationView` để tạo thanh Menu bên trái.
*   Sử dụng `ui:NavigationControl` (hoặc Frame) để hiển thị nội dung các Module.
*   Kết nối `INavigationService` vào cửa sổ chính để điều khiển việc chuyển trang.

---

## Bước 8: Quy trình thêm tính năng mới (The MVVM Workflow)
Mỗi khi thêm một công cụ mới (Ví dụ: AutoClick):
1.  **Service:** Viết logic xử lý (Click chuột) trong `Services/`.
2.  **ViewModel:** Tạo lớp kế thừa `ObservableObject` trong `ViewModels/`.
3.  **View:** Tạo `Page` XAML trong `Views/Pages/`.
4.  **Đăng ký:** Thêm View và ViewModel vào DI Container trong `App.xaml.cs`.

---

## Bước 9: Tối ưu & Đóng gói
Cấu hình trong file `.csproj` để xuất bản ra một file duy nhất:
```xml
<PublishSingleFile>true</PublishSingleFile>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
<PublishReadyToRun>true</PublishReadyToRun>
```

---

### Lời khuyên từ cố vấn:
> "Đừng cố làm tất cả các công cụ cùng một lúc. Hãy hoàn thiện **Bộ khung (Core)** thật chỉn chu, sau đó việc thêm các module mới sẽ cực kỳ nhanh chóng và không gây lỗi hệ thống."

**Chúc bạn thành công với dự án MyDeusTools!**
