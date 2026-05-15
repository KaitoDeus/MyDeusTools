using System;
using System.Windows;
using Wpf.Ui.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace MyDeusTools.App;

public partial class MainWindow : FluentWindow
{
    public ViewModels.MainWindowViewModel ViewModel { get; }
    private TaskbarIcon? _notifyIcon;
    private bool _isExitAllowed = false;

    public MainWindow(ViewModels.MainWindowViewModel viewModel, Wpf.Ui.INavigationService navigationService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // Cấu hình điều hướng: Gán NavigationView cho Service
        navigationService.SetNavigationControl(RootNavigation);

        // Cấu hình Tray
        InitializeTray();
        Closing += MainWindow_Closing;
    }

    private void InitializeTray()
    {
        _notifyIcon = new TaskbarIcon
        {
            ToolTipText = "MyDeusTools is running",
            IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/avatar.ico")),
            ContextMenu = new System.Windows.Controls.ContextMenu()
        };

        var openItem = new System.Windows.Controls.MenuItem { Header = "Mở ứng dụng" };
        openItem.Click += OnShowMainWindow;

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Thoát" };
        exitItem.Click += OnExitApp;

        _notifyIcon.ContextMenu.Items.Add(openItem);
        _notifyIcon.ContextMenu.Items.Add(new System.Windows.Controls.Separator());
        _notifyIcon.ContextMenu.Items.Add(exitItem);

        _notifyIcon.TrayLeftMouseUp += (s, e) => ShowWindow();
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExitAllowed)
        {
            e.Cancel = true;
            this.Hide();
        }
    }

    private void OnShowMainWindow(object? sender, RoutedEventArgs e) => ShowWindow();

    private void OnExitApp(object? sender, RoutedEventArgs e)
    {
        _isExitAllowed = true;
        Application.Current.Shutdown();
    }

    private void ShowWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
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