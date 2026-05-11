using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;

namespace MyDeusTools.App.Services;

public class PageService : IPageService
{
    private readonly IServiceProvider _serviceProvider;
    public PageService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    // Trả về Page tương ứng với kiểu T
    public T? GetPage<T>() where T : class => _serviceProvider.GetService<T>();

    // Trả về Page dựa trên Type (Dùng cho NavigationControl của Wpf.Ui)
    public FrameworkElement? GetPage(Type pageType) => _serviceProvider.GetService(pageType) as FrameworkElement;
}
