using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace MyDeusTools.App;

public partial class MainWindow : FluentWindow
{
    public ViewModels.MainWindowViewModel ViewModel { get; }

    public MainWindow(ViewModels.MainWindowViewModel viewModel, Wpf.Ui.INavigationService navigationService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // Cấu hình điều hướng: Gán NavigationView cho Service
        navigationService.SetNavigationControl(RootNavigation);
    }

    private void OnThemeToggleClick(object sender, RoutedEventArgs e)
    {
        var currentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
        var isDark = currentTheme == Wpf.Ui.Appearance.ApplicationTheme.Dark;

        var newTheme = isDark
            ? Wpf.Ui.Appearance.ApplicationTheme.Light
            : Wpf.Ui.Appearance.ApplicationTheme.Dark;

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(newTheme);
    }
}