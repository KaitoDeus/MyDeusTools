using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Mvvm.Contracts;

namespace MyDeusTools.App.Services;

public class PageService : IPageService
{
    private readonly IServiceProvider _serviceProvider;

    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T? GetPage<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }

    public FrameworkElement? GetPage(Type pageType)
    {
        return _serviceProvider.GetService(pageType) as FrameworkElement;
    }
}
