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
dotnet add package WPF-UI --version 3.0.5

# MVVM Toolkit (Source Generators)
dotnet add package CommunityToolkit.Mvvm

# Dependency Injection
dotnet add package Microsoft.Extensions.DependencyInjection

# Quản lý phím tắt
dotnet add package NHotkey.Wpf
```

---

## Bước 4: Triển khai kiến trúc Dependency Injection (DI)
Kiến trúc DI giúp ứng dụng của bạn "lỏng lẻo" (loosely coupled), dễ dàng thay thế các thành phần và cực kỳ thuận tiện cho việc viết Unit Test sau này.

1.  **Loại bỏ `StartupUri`:** Mở file `App.xaml` và xóa thuộc tính `StartupUri="MainWindow.xaml"`. Chúng ta sẽ tự tay khởi tạo cửa sổ chính thông qua DI.
2.  **Cấu hình trong `App.xaml.cs`:**
    *   Tạo một thuộc tính `IServiceProvider` để lưu trữ "thùng chứa" các dịch vụ.
    *   Sử dụng `ServiceCollection` để đăng ký:
        *   **Singleton:** Các dịch vụ dùng chung xuyên suốt app (Navigation, PageService).
        *   **Transient:** Các ViewModel và View (mỗi lần gọi sẽ tạo mới một instance để đảm bảo dữ liệu sạch).

```csharp
public partial class App : Application
{
    // Thùng chứa dịch vụ (Container)
    public static IServiceProvider? Services { get; private set; }

    public App()
    {
        Services = ConfigureServices();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Lấy MainWindow từ DI Container thay vì tạo bằng từ khóa 'new'
        var mainWindow = Services?.GetRequiredService<MainWindow>();
        mainWindow?.Show();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. Dịch vụ hệ thống & Điều hướng của Wpf.Ui
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // 2. Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        // Ví dụ: services.AddTransient<DashboardViewModel>();

        // 3. Register Views (Windows/Pages)
        services.AddTransient<MainWindow>();
        // Ví dụ: services.AddTransient<DashboardPage>();

        return services.BuildServiceProvider();
    }
}
```
> [!TIP]
> Luôn đăng ký ViewModel trước khi đăng ký View tương ứng để DI có thể tự động "tiêm" (inject) ViewModel vào constructor của View.

---

## Bước 5: Thiết lập Điều hướng (Navigation Service)
Trong một ứng dụng đa năng (Super-App), việc chuyển đổi giữa các module (như từ AutoClicker sang Sticky Note) cần được quản lý tập trung.

1.  **IPageService:** Đây là "bản đồ" giúp `Wpf.Ui` biết tìm thấy View của một ViewModel ở đâu.
2.  **PageService.cs:** Lớp này thực hiện việc lấy Page từ DI Container.

```csharp
// Services/PageService.cs
public class PageService : IPageService
{
    private readonly IServiceProvider _serviceProvider;
    public PageService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    // Trả về Page tương ứng với kiểu T
    public T? GetPage<T>() where T : class => _serviceProvider.GetService<T>();

    // Trả về Page dựa trên Type (Dùng cho NavigationControl của Wpf.Ui)
    public FrameworkElement? GetPage(Type pageType) => _serviceProvider.GetService(pageType) as FrameworkElement;
}
```

3.  **MainWindowViewModel:** Phải nhận `INavigationService` vào constructor để có thể điều khiển chuyển trang từ logic (Code-behind hoặc Command).

```csharp
public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}
```

---

## Bước 6: Xây dựng Giao diện chính (The Shell)
Đây là nơi chúng ta tạo ra "bộ mặt" của ứng dụng. Cửa sổ chính sẽ chứa thanh menu điều hướng và vùng hiển thị nội dung các công cụ.

### 1. Tích hợp Styles vào `App.xaml`
Để các control của `Wpf.Ui` hiển thị đúng giao diện Fluent Design, bạn **BẮT BUỘC** phải thêm các Resource sau vào `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ui:ThemesDictionary Theme="Light" />
            <ui:ControlsDictionary />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 2. Thiết lập `MainWindow.xaml`
Thay đổi `Window` thành `ui:FluentWindow` và sử dụng `ui:NavigationView` để tạo thanh menu bên trái.

```xml
<ui:FluentWindow ...
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml">
    
    <Grid>
        <ui:NavigationView x:Name="RootNavigation"
                           PaneDisplayMode="Left"
                           IsBackButtonVisible="Collapsed">
            <!-- Vùng hiển thị nội dung các Page -->
            <ui:NavigationControl x:Name="RootFrame" />
        </ui:NavigationView>
    </Grid>
</ui:FluentWindow>
```

### 3. Kết nối NavigationService trong `MainWindow.xaml.cs`
Bạn cần kết nối `NavigationView` với `INavigationService` để việc chuyển trang hoạt động.

```csharp
public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService)
{
    ViewModel = viewModel;
    DataContext = this;

    InitializeComponent();

    // Thiết lập dịch vụ điều hướng cho NavigationView
    navigationService.SetNavigationControl(RootFrame);
}
```

---

## Bước 7: Thiết kế Theme và Resources
Tùy chỉnh màu sắc chủ đạo và hỗ trợ Light/Dark mode đồng bộ với hệ thống.

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
