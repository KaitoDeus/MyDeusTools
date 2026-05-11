using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MyDeusTools.App.ViewModels;
using MyDeusTools.App.Services;
using Wpf.Ui;

namespace MyDeusTools.App;

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