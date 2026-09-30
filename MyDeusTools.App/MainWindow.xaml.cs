using System;
using System.Windows;
using Wpf.Ui.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App;

public partial class MainWindow : FluentWindow
{
    public ViewModels.MainWindowViewModel ViewModel { get; }
    private readonly IAutoStartService _autoStartService;
    private readonly IClipboardService _clipboardService;
    private readonly ILanguageService _languageService;
    private TaskbarIcon? _notifyIcon;
    private bool _isExitAllowed = false;
    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private System.Windows.Interop.HwndSource? _hwndSource;

    public MainWindow(ViewModels.MainWindowViewModel viewModel, Wpf.Ui.INavigationService navigationService, IAutoStartService autoStartService, IClipboardService clipboardService, ILanguageService languageService)
    {
        ViewModel = viewModel;
        _autoStartService = autoStartService;
        _clipboardService = clipboardService;
        _languageService = languageService;
        DataContext = this;

        InitializeComponent();

        _languageService.LanguageChanged += OnLanguageChanged;

        // Cấu hình điều hướng: Gán NavigationView cho Service
        navigationService.SetNavigationControl(RootNavigation);

        // Cấu hình Tray
        InitializeTray();
        Closing += MainWindow_Closing;

        // Khởi tạo trạng thái khởi động cùng Windows
        UpdateAutoStartUI();
        UpdateThemeUI();
        UpdateTrayLanguage();

        // Lắng nghe sự kiện Clipboard hệ thống
        SourceInitialized += MainWindow_SourceInitialized;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        _hwndSource = System.Windows.Interop.HwndSource.FromHwnd(handle);
        _hwndSource?.AddHook(HwndHook);
        _clipboardService.StartMonitoring(handle);
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        _clipboardService.StopMonitoring(handle);
        _hwndSource?.RemoveHook(HwndHook);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            _clipboardService.ProcessClipboardUpdate();
        }
        return IntPtr.Zero;
    }

    private System.Windows.Controls.MenuItem? _trayOpenItem;
    private System.Windows.Controls.MenuItem? _trayExitItem;

    private void InitializeTray()
    {
        _trayOpenItem = new System.Windows.Controls.MenuItem { Header = _languageService.GetString("App_Tray_Open", "Open Application") };
        _trayOpenItem.Click += OnShowMainWindow;

        _trayExitItem = new System.Windows.Controls.MenuItem { Header = _languageService.GetString("App_Tray_Exit", "Exit") };
        _trayExitItem.Click += OnExitApp;

        _notifyIcon = new TaskbarIcon
        {
            ToolTipText = _languageService.GetString("App_Tray_Tooltip", "MyDeusTools"),
            IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/avatar.ico")),
            ContextMenu = new System.Windows.Controls.ContextMenu()
        };

        _notifyIcon.ContextMenu.Items.Add(_trayOpenItem);
        _notifyIcon.ContextMenu.Items.Add(new System.Windows.Controls.Separator());
        _notifyIcon.ContextMenu.Items.Add(_trayExitItem);

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
        UpdateThemeUI();
    }

    private void OnLanguageClick(object sender, RoutedEventArgs e)
    {
        _languageService.ToggleLanguage();
    }

    private void OnLanguageChanged(AppLanguage lang)
    {
        UpdateAutoStartUI();
        UpdateThemeUI();
        UpdateTrayLanguage();
    }

    private void UpdateTrayLanguage()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ToolTipText = _languageService.GetString("App_Tray_Tooltip", "MyDeusTools");
        }
        if (_trayOpenItem != null)
        {
            _trayOpenItem.Header = _languageService.GetString("App_Tray_Open", "Open Application");
        }
        if (_trayExitItem != null)
        {
            _trayExitItem.Header = _languageService.GetString("App_Tray_Exit", "Exit");
        }
    }

    private void UpdateThemeUI()
    {
        var currentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
        var isDark = currentTheme == Wpf.Ui.Appearance.ApplicationTheme.Dark;
        ThemeToggleButton.Content = isDark
            ? _languageService.GetString("App_Theme_Dark", "Theme: Dark")
            : _languageService.GetString("App_Theme_Light", "Theme: Light");
        ThemeToggleIcon.Symbol = isDark ? SymbolRegular.WeatherMoon24 : SymbolRegular.WeatherSunny24;
    }

    private void OnAutoStartClick(object sender, RoutedEventArgs e)
    {
        bool currentStatus = _autoStartService.IsEnabled();
        _autoStartService.SetEnabled(!currentStatus);
        UpdateAutoStartUI();
    }

    private void UpdateAutoStartUI()
    {
        bool isEnabled = _autoStartService.IsEnabled();
        string key = isEnabled ? "App_AutoStart_On" : "App_AutoStart_Off";
        string fallback = isEnabled ? "Khởi động cùng Win: BẬT" : "Khởi động cùng Win: TẮT";
        AutoStartMenuItem.Content = _languageService.GetString(key, fallback);
        AutoStartIcon.Symbol = isEnabled ? SymbolRegular.CheckboxChecked24 : SymbolRegular.CheckboxUnchecked24;
    }
}