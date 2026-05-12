using System;
using Microsoft.Extensions.DependencyInjection;
using MyDeusTools.App.ViewModels;
using MyDeusTools.App.Services;
using MyDeusTools.App.Services.Impl;
using MyDeusTools.App.Views.Pages;
using Wpf.Ui;
using System.Windows;

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

    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 1. Dịch vụ hệ thống & Điều hướng của Wpf.Ui
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAutoClickService, AutoClickService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IStickyNoteService, StickyNoteService>();

        // 2. Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<AutoClickViewModel>();
        services.AddTransient<ShutdownViewModel>();
        services.AddTransient<StickyNoteViewModel>();

        // 3. Register Views (Windows/Pages)
        services.AddTransient<MainWindow>();
        services.AddTransient<AutoClickPage>();
        services.AddTransient<ShutdownPage>();
        services.AddTransient<StickyNotePage>();

        return services.BuildServiceProvider();
    }
}