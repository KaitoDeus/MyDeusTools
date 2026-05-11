using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App;

public partial class MainWindow : FluentWindow
{
    public ViewModels.MainWindowViewModel ViewModel { get; }

    public MainWindow(ViewModels.MainWindowViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // Kết nối NavigationService với NavigationView
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

        // Cập nhật Text và Icon dựa trên Theme MỚI sau khi đổi
        if (newTheme == Wpf.Ui.Appearance.ApplicationTheme.Dark)
        {
            ThemeToggleButton.Content = "Light Mode";
            ThemeToggleIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.WeatherSunny24;
        }
        else
        {
            ThemeToggleButton.Content = "Dark Mode";
            ThemeToggleIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.WeatherMoon24;
        }
    }
}