using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MyDeusTools.App.ViewModels;
using MyDeusTools.App.Services;
using Wpf.Ui;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace MyDeusTools.App;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public App()
    {
        Services = ConfigureServices();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = Services?.GetRequiredService<MainWindow>();
        mainWindow?.Show();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Dịch vụ điều hướng của Wpf.Ui (v2.1.0)
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}